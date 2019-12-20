using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using B2S.Core.Utilities;
using Microsoft.AspNet.Identity;
using B2S.Web.Extensions;
using B2S.Web.ViewModels.DashBoard;

namespace B2S.Web.Controllers
{
    public class VendorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public VendorController(AppDbContext context, IOptions<PaginationOptions> paginationOptions)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
        }

        // GET: Vendor
        public async Task<IActionResult> Index(string search, int? page)
        {
            ViewData["CurrentSearch"] = search;
            var vendors = _context.Vendor.Select(c => c);

            if (!string.IsNullOrEmpty(search))
            {
                vendors = vendors.Where(l => string.Format("{0} {1}", l.VendorName, l.ContactName).ToUpper().Contains(search.ToUpper()) || l.Email.ToUpper().Contains(search.ToUpper()) || l.Phone.Contains(search));
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<Vendor>.CreateAsync(vendors, page ?? 1, pageSize));
        }

        public async Task<IActionResult> Dashboard()
        {
            var vendor = await GetVendorFromUserAsync();
            if (vendor == null)
            {
                return NotFound();
            }
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);

            var bookings = _context.Booking
                .Include(b => b.Bed.Room.RoomType)
                .Include(b => b.Bed.Room.Building.Property.Vendor)
                .Where(b => b.Bed.Room.Building.Property.VendorId == vendor.VendorId && b.BookingFrom <= lastDOY && b.BookingTo >= firstDOY)
                .OrderBy(b => b.CreatedAt);

            var expenses = _context.Expense;

            var beds = _context.Bed.Include(b => b.Room.Property).Where(b => b.Room.Property.VendorId == vendor.VendorId);
            int totalBookedDays = await bookings.SumAsync(b => (b.BookingTo.CompareMin(lastDOY) - b.BookingFrom.CompareMax(firstDOY)).Days);
            int totalBeds = await beds.CountAsync();
            double vacancyRate = totalBookedDays * 100.00 / (totalBeds * (lastDOY - firstDOY).Days);

            ViewData["VacancyRate"] = Math.Round(vacancyRate, 1);
            ViewData["Revenue"] = (await bookings.SumAsync(b => decimal.Divide((b.BookingTo.CompareMin(lastDOY) - b.BookingFrom.CompareMax(firstDOY)).Days, 7) * b.Price)).ToString("#,##0.00");
            ViewData["Expense"] = (await expenses.SumAsync(e => e.Amount)).ToString("#,##0.00");
            ViewData["DirectBooked"] = await bookings.Where(b => b.AgentId == null).CountAsync();
            ViewData["AgentBooked"] = await bookings.Where(b => b.AgentId != null).CountAsync();
            ViewData["FirstDOY"] = firstDOY;
            ViewData["LastDOY"] = lastDOY;        
            return View(await bookings.ToListAsync());
        }

        [HttpGet]
        public async Task<JsonResult> PopulationRevenueChart()
        {
            var vendor = await GetVendorFromUserAsync();
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var today = DateTime.Today;
            var lastDOM = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);
            var bookings = _context.Booking
              .Include(b => b.Bed.Room.RoomType)
              .Include(b => b.Bed.Room.Building.Property.Vendor)
              .Where(b => b.Bed.Room.Building.Property.VendorId == vendor.VendorId && b.BookingFrom <= lastDOM && b.BookingTo >= firstDOY);
            
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
            var vendor = await GetVendorFromUserAsync();
            var bedOfVendors = _context.Bed.Include(b => b.Room.Property).Where(b => b.Room.Property.VendorId == vendor.VendorId);
            var firstDOY = new DateTime(DateTime.Today.Year, 1, 1);
            var today = DateTime.Today;
            var lastDOM = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            //var lastDOY = new DateTime(DateTime.Today.Year, 12, 31);
            var bookings = _context.Booking
              .Include(b => b.Bed.Room.RoomType)
              .Include(b => b.Bed.Room.Building.Property.Vendor)
              .Where(b => b.Bed.Room.Building.Property.VendorId == vendor.VendorId && b.BookingFrom <= lastDOM && b.BookingTo >= firstDOY);

            List<OccupancyChartViewModel> occupancyChartViewModels = new List<OccupancyChartViewModel>();
            for (int i = 1; i <= today.Month; i++)
            {
                DateTime lastCurDOM = new DateTime(today.Year, i, DateTime.DaysInMonth(today.Year, i));
                DateTime firstCurDOM = new DateTime(today.Year, i, 1);
                var curMonthBookings = bookings.Where(b => b.BookingFrom <= lastCurDOM && b.BookingTo >= firstCurDOM);
                int totalBookedBedDays = await curMonthBookings.SumAsync(b => (b.BookingTo.CompareMin(lastCurDOM) - b.BookingFrom.CompareMax(firstCurDOM)).Days);
                var totalBedDays = (lastCurDOM - firstCurDOM).Days * (await bedOfVendors.CountAsync());
                var percent = totalBookedBedDays * 100.00 / totalBedDays;
                occupancyChartViewModels.Add(new OccupancyChartViewModel { Month = firstCurDOM.ToString("MMM"), Percent = Math.Round(percent, 2) });
            }
            return Json(occupancyChartViewModels);
        }


        [HttpGet]
        public async Task<IActionResult> PopulationMap()
        {
            var vendor = await GetVendorFromUserAsync();          
            var countryCount = _context.Booking
                  .Include(b => b.Bed.Room.Building.Property)
                  .Where(b => b.Bed.Room.Building.Property.VendorId == vendor.VendorId).GroupBy(
                b => b.Nationality,
                (key, g) => new { country = CommonHelper.GetShortHandName(key), count = g.Count() }
                );

            //var countryCount = CommonHelper.CountryList().Select(c => new { country = CommonHelper.GetShortHandName(c), count = countries.FirstOrDefault(a => a.country == c)?.count ?? 0 });
            //var u = countryCount.ToList();
            //return JsonConvert.SerializeObject
            return Json(new { data = await countryCount.ToListAsync() });
        }

        // GET: Vendor/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendor
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: Vendor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vendor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendorId,VendorName,ContactName,Phone,Fax,Email,Address,City,Province,Postcode,Country,IsActive,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Vendor vendor)
        {
            if (ModelState.IsValid)
            {
                vendor.CreatedAt = DateTime.UtcNow;
                vendor.CreatedBy = User.Identity.GetUserId();
                _context.Add(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vendor);
        }

        // GET: Vendor/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendor.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return View(vendor);
        }

        // POST: Vendor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("VendorId,VendorName,ContactName,Phone,Fax,Email,Address,City,Province,Postcode,Country,IsActive,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Vendor vendor)
        {
            if (id != vendor.VendorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vendor.ModifiedAt = DateTime.UtcNow;
                    vendor.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(vendor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.VendorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vendor);
        }

        // GET: Vendor/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendor
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // POST: Vendor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var vendor = await _context.Vendor.FindAsync(id);
            _context.Vendor.Remove(vendor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }       

        private async Task<Vendor> GetVendorFromUserAsync()
        {
            var userAccount = await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == User.Identity.GetUserId());
            return await _context.Vendor.FindAsync(userAccount?.AccountId);
        }

        private bool VendorExists(string id)
        {
            return _context.Vendor.Any(e => e.VendorId == id);
        }
    }
}
