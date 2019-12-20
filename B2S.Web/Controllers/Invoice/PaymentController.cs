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
using Microsoft.AspNet.Identity;
using B2S.Core.Utilities;
using B2S.Core.Interfaces;

namespace B2S.Web.Controllers.Invoice
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;
        private readonly IInvoicePaymentService _invoicePaymentService;

        public PaymentController(AppDbContext context, IOptions<PaginationOptions> paginationOptions, IInvoicePaymentService invoicePaymentService)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
            _invoicePaymentService = invoicePaymentService;
        }

        // GET: Payment
        public async Task<IActionResult> Index(string search, DateTime? currentFrom, DateTime? currentTo, int? page)
        {
            currentFrom = currentFrom ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            currentTo = currentTo ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            ViewData["currentFrom"] = currentFrom;
            ViewData["currentTo"] = currentTo;
            ViewData["CurrentSearch"] = search;

            var bookings = _context.Booking.Where(b => b.BookingFrom <= currentTo && b.BookingTo >= currentFrom && (b.Status == Core.Enums.BookingStatus.CheckIn || b.Status == Core.Enums.BookingStatus.Paid)).Include(b => b.Bed.Room.RoomType).Include(b => b.Bed.Room.Building.Property).Select(b => b);
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
                bookings = bookings.Where(b => (b.FirstName + " " + b.LastName).ToUpper().Contains(search.ToUpper()) || b.IDNumber.ToUpper().Contains(search.ToUpper()) || b.Email.ToUpper().Contains(search.ToUpper()));
            }

            int pageSize = 20;

            return View(await PaginatedList<Booking>.CreateAsync(bookings, page ?? 1, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Generate(string search, DateTime? currentFrom, DateTime? currentTo, List<string> bookingIds)
        {                    
            try
            {
                await _invoicePaymentService.GeneratePaymentsByBookingIds(bookingIds);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Payment", new { search, currentFrom, currentTo }) });
        }

        // GET: Payment/Details/5
        public async Task<IActionResult> Details(string id)
        {
            //var payment = await _context.Payment.FindAsync(id);
            //if (payment == null)
            //{
            //    return NotFound();
            //}

            var booking = await _context.Booking.Include(b => b.Customer).Include(b => b.Bed.Room.Building.Property).Include(b => b.Bed.Room.RoomType).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return new EmptyResult();
            }

            ViewData["PaymentId"] = id;
            return View(booking);
        }

        public async Task<IActionResult> Create(string id, string bookingId)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment != null)
            {
                return View(payment);
            }

            Payment newPayment = new Payment
            {
                BookingId = bookingId
            };
            return View(newPayment);
        }


        //public async Task<IActionResult> Generate(List<string> bookingIds)
        //{
        //    try
        //    {
        //        await _invoicePaymentService.GeneratePaymentsByBookingIds(bookingIds);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["StatusMessage"] = "Error: " + ex.Message;
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        private bool PaymentExists(string id)
        {
            return _context.Payment.Any(e => e.PaymentId == id);
        }
    }
}
