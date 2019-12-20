using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Enums;
using B2S.Infrastructure.Data;
using B2S.Web.ViewModels;
using B2S.Core.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using Microsoft.AspNet.Identity;
using B2S.Core.Interfaces;
using B2S.Web.Extensions;

namespace B2S.Web.Controllers
{
    [Authorize(Roles="Event")]
    public class EventController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public EventController(AppDbContext context, Microsoft.AspNetCore.Identity.UserManager<User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetEvent()
        {
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            string accountId = (await _context.UserAccount.FirstOrDefaultAsync(a => a.UserId == user.Id))?.AccountId;

            List<EventViewModel> events = new List<EventViewModel>();
            var bookings = _context.Booking.Select(b => b);
            if (!user.IsSuperAdmin || user.UserType != UserType.Admin)
            {
                if (user.UserType == UserType.Agent)
                {
                    bookings = bookings.Where(b => b.AgentId == accountId);
                }
                if (user.UserType == UserType.Vendor)
                {
                    bookings = bookings.Where(b => b.Bed.Room.Building.Property.VendorId == accountId);
                }
            }
            foreach (var booking in await bookings.Where(b => b.Status == BookingStatus.Confirmed).ToListAsync())
            {
                events.Add(new EventViewModel { BookingId = booking.BookingId, Title = string.Format("{0} {1} check in", booking.FirstName, booking.LastName), StartDate = booking.BookingFrom, EndDate = booking.BookingFrom, BackgroundColor = EventColorHex.CheckInColorHex, BorderColor = EventColorHex.CheckInColorHex, EventType = EventType.CheckIn });
            }
            foreach (var booking in await bookings.Where(b => b.Status == BookingStatus.CheckIn || b.Status == BookingStatus.Paid).ToListAsync())
            {
                events.Add(new EventViewModel { BookingId = booking.BookingId, Title = string.Format("{0} {1} check out", booking.FirstName, booking.LastName), StartDate = booking.BookingTo, EndDate = booking.BookingTo, BackgroundColor = EventColorHex.CheckOutColorHex, BorderColor = EventColorHex.CheckOutColorHex, EventType = EventType.CheckOut });
            }
            return Json(events);
        }

        public async Task<IActionResult> Details(string id, EventType eventType)
        {
            ViewData["IsCheckIn"] = false;
            ViewData["IsCheckOut"] = false;

            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "CheckIn") && eventType == EventType.CheckIn)
            {
                ViewData["IsCheckIn"] = true;
            }
            if (await _userManager.IsInRoleAsync(user, "CheckOut") && eventType == EventType.CheckOut)
            {
                ViewData["IsCheckOut"] = true;
            }

            var booking = await _context.Booking.Include(b => b.Customer).Include(b => b.Bed.Room.RoomType).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return new EmptyResult();
            }
            ViewData["EventType"] = eventType.GetDisplayName();
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(string bookingId, bool isSendLetter, bool isConfirmOA)
        {
            if (!isConfirmOA)
            {
                return Json(new { success = false, message = "You need to confirm the Occupancy Agreement to check in this booking." });
            }

            var booking = await _context.Booking.Include(b => b.Bed.Room).FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }
            
            try
            {
                booking.Status = BookingStatus.CheckIn;
                booking.IsConfirmOA = true;
                booking.ModifiedAt = DateTime.UtcNow;
                booking.ModifiedBy = User.Identity.GetUserId();
                _context.Update(booking);
                await _context.SaveChangesAsync();

                if (isSendLetter)
                {
                    var emailTemplate = await _context.EmailTemplate.FirstOrDefaultAsync(e => e.TemplatName == EmailTemplateName.Welcome);
                    if (emailTemplate == null)
                    {
                        return Json(new { success = false, message = "Email template not found." });
                    }

                    var documents = await _context.PropertyDocument.Where(d => d.PropertyId == booking.Bed.Room.PropertyId && d.Category == DocumentCategory.CheckIn).ToListAsync();
                    string subject = emailTemplate.Subject;
                    string body = StringExtension.Format(emailTemplate.BodyHTML.Format(
                           student_name => booking.FirstName + " " + booking.LastName
                           ));
                    await _emailSender.SendEmailWithPropertyDocsAsync(booking.Email, subject, body, documents);
                }
                return Json(new { success = true, message = "Check-in success." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(string bookingId, bool isReportDamage)
        {
            var booking = await _context.Booking.Include(b => b.Bed.Room.Property.Vendor).FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }

            booking.Status = BookingStatus.CheckOut;
            booking.ModifiedAt = DateTime.UtcNow;
            booking.ModifiedBy = User.Identity.GetUserId();
            _context.Update(booking);
            await _context.SaveChangesAsync();

            string redirectUrl = "";
            if (isReportDamage) redirectUrl = Url.Action("Create", "DamageReport", new { bookingId = booking.BookingId });

            //Send departure email
            //var emailTemplate = await _context.EmailTemplate.FirstOrDefaultAsync(e => e.TemplatName == EmailTemplateName.DepartureLetter);
            //if (emailTemplate == null)
            //{
            //    return Json(new { success = true, message = "Check out success without sending departure email. \nError: Email template not found.", redirectUrl });
            //}
            //try
            //{
            //    await _emailSender.SendEmailAsync(booking.Email, emailTemplate.Subject, emailTemplate.BodyHTML);
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { success = true, message = "Check out success without sending departure email. \nError: " + ex.Message, redirectUrl });
            //}

            return Json(new { success = true, message = "Check out success", redirectUrl });
        }
    }
}