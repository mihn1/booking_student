using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Common;
using B2S.Core.Entities;
using B2S.Core.Utilities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace B2S.Web.Controllers
{
    public class UnconfirmedBookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public UnconfirmedBookingController(AppDbContext context, IOptions<PaginationOptions> options)
        {
            _context = context;
            _paginationOptions = options.Value;
        }

        public async Task<IActionResult> Index(string search, DateTime? currentFrom, DateTime? currentTo, int? page)
        {
            if (currentFrom == null && currentTo == null)
            {
                currentFrom = DateTime.Today;
                currentTo = (new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).AddMonths(2);
            }
            ViewData["CurrentFrom"] = currentFrom;
            ViewData["CurrentTo"] = currentTo;
            ViewData["CurrentSearch"] = search;
            var bookings = _context.Booking.Where(b => b.BookingFrom <= currentTo && b.BookingTo >= currentFrom && b.Status == Core.Enums.BookingStatus.New).Include(b => b.Bed.Room.RoomType).Include(b => b.Bed.Room.Building.Property).Select(b => b);
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
            if (!user.IsSuperAdmin && user.UserType != Core.Enums.UserType.Admin)
            {
                if (user.UserType == Core.Enums.UserType.Agent)
                {
                    bookings = bookings.Where(b => b.AgentId == accountId);
                }
                if (user.UserType == Core.Enums.UserType.Vendor)
                {
                    bookings = bookings.Where(b => b.Bed.Room.Building.Property.VendorId == accountId);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                bookings = bookings.Where(b => (b.FirstName + " " + b.LastName).ToUpper().Contains(search.ToUpper()) || b.IDNumber.ToUpper().Contains(search.ToUpper()) || b.Email.ToUpper().Contains(search.ToUpper()) || b.Phone.ToUpper().Contains(search.ToUpper()));
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<Booking>.CreateAsync(bookings, page ?? 1, pageSize));
        }

        public async Task<IActionResult> Details(string id)
        {
            var booking = await _context.Booking.Include(b => b.Customer).Include(b => b.Bed.Room.Building.Property).Include(b => b.Bed.Room.RoomType).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return new EmptyResult();
            }
            return View(booking);
        }
    }
}