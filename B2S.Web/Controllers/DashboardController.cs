using B2S.Core.Entities;
using B2S.Core.Enums;
using B2S.Core.Utilities;
using B2S.Infrastructure.Data;
using B2S.Web.Extensions;
using B2S.Web.ViewModels;
using B2S.Web.ViewModels.DashBoard;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.Controllers
{
    [Authorize(Roles = "Dashboard")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        public DashboardController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (!user.IsSuperAdmin || user.UserType != UserType.Admin)
            {
                return NotFound();
            }
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);

            var bookings = _context.Booking
                .Include(b => b.Bed.Room.RoomType)
                .Include(b => b.Bed.Room.Building.Property.Vendor)
                .Where(b => b.BookingFrom <= lastDOY && b.BookingTo >= firstDOY)
                .OrderBy(b => b.CreatedAt);

            ViewData["Properties"] = await _context.Property.CountAsync();
            ViewData["Vendors"] = await _context.Vendor.CountAsync();
            ViewData["Students"] = await bookings.CountAsync();
            ViewData["Agents"] = await _context.Agent.CountAsync();
            ViewData["DirectBooked"] = await bookings.Where(b => b.AgentId == null).CountAsync();
            ViewData["AgentBooked"] = await bookings.Where(b => b.AgentId != null).CountAsync();
            return View(bookings);
        }

        [HttpGet]
        public async Task<JsonResult> GetAgents()
        {
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);

            var agents = _context.Booking.Include(b => b.Bed.Room.RoomType).Include(b => b.Agent).Where(b => !string.IsNullOrEmpty(b.AgentId) && b.BookingFrom <= lastDOY && b.BookingTo >= firstDOY)
                .GroupBy(
                b => b.Agent,
                //b => b.BookingId,
                (key, g) => new { agent = key, count = g.Count(), revenue = g.Sum(b => decimal.Divide((b.BookingTo.CompareMin(lastDOY) - b.BookingFrom.CompareMax(firstDOY)).Days, 7) * b.Price) })                
                .OrderByDescending(b => b.count).ThenByDescending(b => b.revenue)
                .Take(10);

            return Json(new { data = await agents.ToListAsync()});
        }

        [HttpGet]
        public async Task<JsonResult> GetVendors()
       {
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);
            var bookings = _context.Booking.Include(b => b.Bed.Room.Building.Property.Vendor).Where(b => b.BookingFrom <= lastDOY && b.BookingTo >= firstDOY);         
           
            var vendors = bookings
                .GroupBy(
                b => b.Bed.Room.Building.Property.Vendor,
                (key, g) => new { vendor = key, count = g.Count(), occuRate = g.Sum(b => (b.BookingTo.CompareMin(lastDOY) - b.BookingFrom.CompareMax(firstDOY)).Days) * 100.00 / (_context.Bed.Include(b => b.Room.Property.VendorId).Where(b => b.Room.Property.VendorId == key.VendorId).Count() * (lastDOY - firstDOY).Days) })             
                .OrderByDescending(b => b.count).ThenByDescending(b => b.occuRate)
                .Take(10);
            return Json(new { data = await vendors.ToListAsync() });
        }

        [HttpGet]
        public async Task<JsonResult> PopulationRevenueChart()
        {
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var today = DateTime.Today;
            var lastDOM = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);
            var bookings = _context.Booking
              .Include(b => b.Bed.Room.RoomType)
              .Include(b => b.Bed.Room.Building.Property.Vendor)
              .Where(b => b.BookingFrom <= lastDOM && b.BookingTo >= firstDOY);

            List<RevenueChartViewModel> revenueChartViewModels = new List<RevenueChartViewModel>();
            for (int i = 1; i <= today.Month; i++)
            {
                DateTime lastCurDOM = new DateTime(today.Year, i, DateTime.DaysInMonth(today.Year, i));
                DateTime firstCurDOM = new DateTime(today.Year, i, 1);
                var curMonthBookings = bookings.Where(b => b.BookingFrom <= lastCurDOM && b.BookingTo >= firstCurDOM);
                decimal revenue = await curMonthBookings.SumAsync(b => decimal.Divide((b.BookingTo.CompareMin(lastCurDOM) - b.BookingFrom.CompareMax(firstCurDOM)).Days, 7) * b.Price);
                revenueChartViewModels.Add(new RevenueChartViewModel { Month = firstCurDOM.ToString("MMM"), Revenue = Math.Round(revenue, 2) });
            }
            return Json(revenueChartViewModels);
        }

        [HttpGet]
        public async Task<JsonResult> PopulationOccupancyChart()
        {
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var today = DateTime.Today;
            var lastDOM = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);
            var bookings = _context.Booking
              .Include(b => b.Bed.Room.RoomType)
              .Include(b => b.Bed.Room.Building.Property.Vendor)
              .Where(b => b.BookingFrom <= lastDOM && b.BookingTo >= firstDOY);

            List<OccupancyChartViewModel> occupancyChartViewModels = new List<OccupancyChartViewModel>();
            for (int i = 1; i <= today.Month; i++)
            {
                DateTime lastCurDOM = new DateTime(today.Year, i, DateTime.DaysInMonth(today.Year, i));
                DateTime firstCurDOM = new DateTime(today.Year, i, 1);
                var curMonthBookings = bookings.Where(b => b.BookingFrom <= lastCurDOM && b.BookingTo >= firstCurDOM);
                int totalBookedBedDays = await curMonthBookings.SumAsync(b => (b.BookingTo.CompareMin(lastCurDOM) - b.BookingFrom.CompareMax(firstCurDOM)).Days);
                var totalBedDays = (lastCurDOM - firstCurDOM).Days * (await _context.Bed.CountAsync());
                var percent = totalBookedBedDays * 100.00 / totalBedDays;
                occupancyChartViewModels.Add(new OccupancyChartViewModel { Month = firstCurDOM.ToString("MMM"), Percent = Math.Round(percent, 2) });
            }
            return Json(occupancyChartViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> PopulationMap()
        {
            var countryCount = _context.Booking.GroupBy(
                b => b.Nationality,
                (key, g) => new { country = CommonHelper.GetShortHandName(key), count = g.Count() }               
                );

            //var countryCount = CommonHelper.CountryList().Select(c => new { country = CommonHelper.GetShortHandName(c), count = countries.FirstOrDefault(a => a.country == c)?.count ?? 0 });
            //var u = countryCount.ToList();
            //return JsonConvert.SerializeObject
            return Json(new { data = await countryCount.ToListAsync() });
        }      

    }
}