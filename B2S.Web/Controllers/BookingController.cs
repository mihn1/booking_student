using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using B2S.Web.ViewModels;
using B2S.Core.Enums;
using B2S.Core.Common.Constants;
using B2S.Core.Interfaces;
using B2S.Web.Extensions;

namespace B2S.Web.Controllers
{
    [Authorize(Roles="Booking")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public BookingController(AppDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string search, PropertyType propertyType, DateTime? bookingFrom, DateTime? bookingTo)
        {
            ViewData["CurrentSearch"] = search ?? "";
            ViewData["CurrentPropertyType"] = (int)propertyType;
            ViewData["CurrentFrom"] = bookingFrom ?? DateTime.Today;
            ViewData["CurrentTo"] = bookingTo ?? (new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))).AddMonths(2);

            var buildings = _context.Building.Include(b => b.Property).OrderBy(b => b.Name).Select(b => b);

            if (!string.IsNullOrEmpty(search))
            {
                buildings = buildings.Where(b => b.Name.ToUpper().Contains(search.ToUpper())
                || b.Property.PropertyName.ToUpper().Contains(search.ToUpper())
                || b.Property.Address.ToUpper().Contains(search.ToUpper()));
            }

            if (propertyType != 0)
            {
                buildings = buildings.Where(b => b.Property.PropertyType == propertyType);
            }

            return View(await buildings.ToListAsync());
        }

        public async Task<IActionResult> GetBedBookingRow(BedBookingViewModel bedBookingVM)
        {
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
            bool isAdmin = user.IsSuperAdmin || user.UserType == UserType.Admin;
            string tooltipInner = "";
            string htmlClass = "";
         
            List<CalendarCellViewModel> calCellVMs = new List<CalendarCellViewModel>();
            var totalDays = (bedBookingVM.BookingTo - bedBookingVM.BookingFrom).Days;
            // calendar headers
            if (bedBookingVM.IsHeader)
            {
                if (bedBookingVM.HeaderType == CalendarHeaderType.MonthYear)
                {
                    for (int i = 0; i <= totalDays; i++)
                    {
                        var curDay = bedBookingVM.BookingFrom.AddDays(i);
                        if (i == 0 || curDay.Day == 1)
                        {
                            string curMthYear = "";
                            if (curDay.Day <= 28)
                            {
                                curMthYear = string.Format("{0} {1}", curDay.ToString("MMM"), curDay.ToString("yyyy"));
                            }
                            calCellVMs.Add(new CalendarCellViewModel { IsHeader = true, HTMLClass = "monthtag month-text cal-month", HTMLInner = curMthYear });
                        }
                        else
                        {
                            calCellVMs.Add(new CalendarCellViewModel { IsHeader = true, HTMLClass = "cal-month" });
                        }
                    }
                }
                else if (bedBookingVM.HeaderType == CalendarHeaderType.DayOfWeek)
                {
                    for (int i = 0; i <= totalDays; i++)
                    {
                        htmlClass = "";
                        var curDay = bedBookingVM.BookingFrom.AddDays(i);
                        if (i == 0 || curDay.Day == 1)
                        {
                            htmlClass = "monthtag";
                        }
                        string curDOW = curDay.DayOfWeek.ToString().Substring(0, 2);
                        calCellVMs.Add(new CalendarCellViewModel { IsHeader = true, HTMLInner = curDOW, HTMLClass = htmlClass });
                    }
                }
                else if (bedBookingVM.HeaderType == CalendarHeaderType.DayOfMonth)
                {
                    for (int i = 0; i <= totalDays; i++)
                    {
                        htmlClass = "";
                        var curDay = bedBookingVM.BookingFrom.AddDays(i);
                        if (i == 0 || curDay.Day == 1)
                        {
                            htmlClass = "monthtag";
                        }
                        string curDOM = curDay.Day.ToString();
                        calCellVMs.Add(new CalendarCellViewModel { IsHeader = true, HTMLInner = curDOM, HTMLClass = htmlClass });
                    }
                }

            }
            //not header
            else
            {
                if (string.IsNullOrEmpty(bedBookingVM.BedId))
                {
                    return new EmptyResult();
                }
                var bed = await _context.Bed.Include(b => b.Room.RoomType).FirstOrDefaultAsync(b => b.BedId == bedBookingVM.BedId);

                var bookings = await _context.Booking.Include(b => b.Bed.Room.Property).Where(b => b.BedId == bedBookingVM.BedId && b.BookingFrom <= bedBookingVM.BookingTo && b.BookingTo >= bedBookingVM.BookingFrom).ToListAsync();
                for (int i = 0; i <= totalDays; i++)
                {
                    htmlClass = "";
                    tooltipInner = "";
                    var curDay = bedBookingVM.BookingFrom.AddDays(i);
                    if (i == 0 || curDay.Day == 1)
                    {
                        htmlClass = " monthtag";
                    }
                    //find booking of the bed of current day
                    var foundBookings = bookings.Where(b => curDay >= b.BookingFrom && curDay <= b.BookingTo && !b.IsDeleted);
                    if (foundBookings.Count() == 1)
                    {
                        var booking = foundBookings.First();
                        if (isAdmin || booking.Bed.Room.Property.VendorId == accountId || accountId == booking.AgentId || accountId == booking.CustomerId)
                            tooltipInner = string.Format(BookingNotification.BookingDetails, bed.Name, bed.Room.RoomName, bed.Room.RoomType?.RoomTypeName, booking.FirstName + " " + booking.LastName, booking.BookingFrom.ToShortDateString(), booking.BookingTo.ToShortDateString());
                        else
                            tooltipInner = BookingNotification.NotYourBooking;

                        calCellVMs.Add(new CalendarCellViewModel
                        {
                            IsHeader = false,
                            BookingId = booking.BookingId,
                            BedId = bedBookingVM.BedId,
                            BookingFrom = curDay,
                            Gender = booking.Gender.ToString(),
                            HTMLClass = htmlClass,
                            Status = booking.Status,
                            IsTwoBooking = booking.BookingTo == curDay,
                            HTMLInner = tooltipInner,
                            IsGreenTick = booking.BookingFrom == curDay && booking.Status != BookingStatus.New
                        });
                        if (booking.BookingTo == curDay)
                        {
                            tooltipInner = string.Format(BookingNotification.AvailableBooking, bed.Name, bed.Room.RoomName, bed.Room.RoomType?.RoomTypeName, curDay.ToShortDateString());
                            calCellVMs.Add(new CalendarCellViewModel { BedId = bedBookingVM.BedId, BookingFrom = curDay, Gender = "not-booking", HTMLClass = htmlClass, HTMLInner = tooltipInner });
                        }
                    }
                    else if (foundBookings.Count() == 2)
                    {
                        var firstBooking = foundBookings.OrderBy(b => b.BookingFrom).First();
                        if (isAdmin || firstBooking.Bed.Room.Property.VendorId == accountId || accountId == firstBooking.AgentId || accountId == firstBooking.CustomerId)
                            tooltipInner = string.Format(BookingNotification.BookingDetails, bed.Name, bed.Room.RoomName, bed.Room.RoomType?.RoomTypeName, firstBooking.FirstName + " " + firstBooking.LastName, firstBooking.BookingFrom.ToShortDateString(), firstBooking.BookingTo.ToShortDateString());
                        else
                            tooltipInner = BookingNotification.NotYourBooking;

                        calCellVMs.Add(new CalendarCellViewModel
                        {
                            IsHeader = false,
                            BookingId = firstBooking.BookingId,
                            BedId = bedBookingVM.BedId,
                            BookingFrom = curDay,
                            Gender = firstBooking.Gender.ToString(),
                            HTMLClass = htmlClass,
                            Status = firstBooking.Status,
                            HTMLInner = tooltipInner,
                            IsTwoBooking = true,
                            IsGreenTick = firstBooking.BookingFrom == curDay && firstBooking.Status != BookingStatus.New
                        });

                        var secondBooking = foundBookings.First(b => b != firstBooking);
                        if (isAdmin || secondBooking.Bed.Room.Property.VendorId == accountId || accountId == secondBooking.AgentId || accountId == secondBooking.CustomerId)
                            tooltipInner = string.Format(BookingNotification.BookingDetails, bed.Name, bed.Room.RoomName, bed.Room.RoomType?.RoomTypeName, secondBooking.FirstName + " " + secondBooking.LastName, secondBooking.BookingFrom.ToShortDateString(), secondBooking.BookingTo.ToShortDateString());
                        else
                            tooltipInner = BookingNotification.NotYourBooking;

                        calCellVMs.Add(new CalendarCellViewModel
                        {
                            IsHeader = false,
                            BookingId = secondBooking.BookingId,
                            BedId = bedBookingVM.BedId,
                            BookingFrom = curDay,
                            Gender = secondBooking.Gender.ToString(),
                            HTMLClass = htmlClass,
                            Status = secondBooking.Status,
                            HTMLInner = tooltipInner,
                            IsTwoBooking = true,
                            IsGreenTick = secondBooking.BookingFrom == curDay && secondBooking.Status != BookingStatus.New
                        });
                    }
                    else
                    {
                        tooltipInner = string.Format(BookingNotification.AvailableBooking, bed.Name, bed.Room.RoomName, bed.Room.RoomType?.RoomTypeName, curDay.ToShortDateString());
                        calCellVMs.Add(new CalendarCellViewModel { BedId = bedBookingVM.BedId, BookingFrom = curDay, Gender = "not-booking", HTMLClass = htmlClass, HTMLInner = tooltipInner });
                    }
                }

            }
            return PartialView("_BedBookingRow", calCellVMs.OrderBy(c => c.BookingFrom));

        }
        
        // GET: Booking/Create
        public async Task<IActionResult> Create(string id, DateTime bookingFrom, string bedId)
        {
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;

            if (user.UserType == UserType.Customer)
            {
                return RedirectToAction(nameof(StudentCreate), new { id, bookingFrom, bedId});
            }

            if (string.IsNullOrEmpty(id))
            {
                ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName");
                ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text");
                ViewData["CountryList"] = new SelectList(Core.Utilities.CommonHelper.CountryNativeList().Select(c => c));
                var bed = await _context.Bed.Include(b => b.Room.RoomType).Include(b => b.Room.Building.Property).FirstOrDefaultAsync();
                Booking newBooking = new Booking
                {
                    BookingFrom = bookingFrom,
                    BedId = bedId,
                    Bed = bed,
                    Status = BookingStatus.New
                };
                return View(newBooking);
            }
            
                var booking = await _context.Booking.Include(b => b.Bed.Room.Building.Property).Include(b => b.Bed.Room.RoomType).FirstOrDefaultAsync(b => b.BookingId == id);
                if (user.UserType != UserType.Admin && !user.IsSuperAdmin && booking.Bed.Room.Property.VendorId != accountId && accountId != booking.AgentId && accountId != booking.CustomerId)
                {
                    return new EmptyResult();
                }               

                ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", booking?.AgentId);
                ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text", booking?.CustomerId);
                ViewData["CountryList"] = new SelectList(Core.Utilities.CommonHelper.CountryList().Select(c => new { value = c, text = c}), "value", "text", booking?.Nationality);
                return View(booking);
            
        }

        // POST: Booking/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] [Bind("BookingId,FirstName,LastName,Gender,IDType,IDNumber,Phone,Email,Status,Nationality,AgentId,CustomerId,BookingFrom,BookingTo,BedId,IsConfirmTC,CreatedAt,CreatedBy,Price,DepositAmount,DiscountAmount")] Booking booking)
        {
            if (ModelState.IsValid)
            {                           
                //validate time range
                if (booking.BookingFrom >= booking.BookingTo)
                {
                    return Json(new { success = false, message = "Start Date must less than End Date." });
                }         
                //validate gender in room
                var roomId = (await _context.Bed.FindAsync(booking.BedId)).RoomId;
                var existBookings = _context.Booking.Where(b => b.Bed.RoomId == roomId && b.BookingId != booking.BookingId && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingFrom <= booking.BookingTo.AddDays(-1) && !b.IsDeleted);
                if (existBookings.Count() > 0)
                {
                    bool unValidGender = await existBookings.AnyAsync(b => b.Gender != booking.Gender);
                    if (unValidGender)
                    {
                        return Json(new { success = false, message = string.Format("{0} can not book this bed in this time range", booking.Gender.ToString()) });
                    }
                }
                try
                {
                    if (string.IsNullOrEmpty(booking.BookingId))
                    {
                        //confirm OCA
                        if (!booking.IsConfirmTC)
                        {
                            return Json(new { success = false, message = "You need to confirm Occupancy Agreement to book this bed." });
                        }
                        //validate booking by time range
                        bool isExistBooking = await _context.Booking.AnyAsync(b => b.BedId == booking.BedId && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingFrom <= booking.BookingTo.AddDays(-1) && !b.IsDeleted);
                        if (isExistBooking)
                        {
                            return Json(new { success = false, message = "This bed can not be booked in this time range." });
                        }

                        //Update AgentId or CustomerId base on User Type
                        var user = await _context.User.FindAsync(User.Identity.GetUserId());
                        string accountId = (await _context.UserAccount.FirstOrDefaultAsync(a => a.UserId == user.Id))?.AccountId;
                        if (user.UserType == UserType.Customer)
                        {
                            booking.CustomerId = accountId;
                        }
                        else if (user.UserType == UserType.Agent)
                        {
                            booking.AgentId = accountId;
                        }
                        var roomType = (await _context.Bed.Include(b => b.Room.RoomType).FirstOrDefaultAsync(b => b.BedId == booking.BedId)).Room.RoomType;
                        booking.Price = roomType.Price;
                        booking.DepositAmount = roomType.DepositAmount;
                        booking.DiscountAmount = roomType.DiscountAmount;
                        booking.Status = BookingStatus.New;
                        booking.CreatedBy = user.Id;
                        _context.Add(booking);
                        //if (!string.IsNullOrEmpty(note))
                        //    _context.Add(new BookingNote { Description = note });
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Booking success." });
                    }
                    else
                    {
                        //validate booking by time range
                        bool isExistBooking = await _context.Booking.AnyAsync(b => b.BookingId != booking.BookingId && b.BedId == booking.BedId && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingFrom <= booking.BookingTo.AddDays(-1) && !b.IsDeleted);
                        if (isExistBooking)
                        {
                            return Json(new { success = false, message = "This bed can not be booked in this time range." });
                        }
                        booking.ModifiedAt = DateTime.UtcNow;
                        booking.ModifiedBy = User.Identity.GetUserId();
                        _context.Update(booking);
                        //if (!string.IsNullOrEmpty(note))
                        //    _context.Add(new BookingNote { Description = note, BookingId = booking.BookingId });
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Edit booking success." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
            return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Booking not found." });
            }
            var booking = await _context.Booking.FindAsync(id);
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType != UserType.Admin && booking.CreatedBy != user.Id && !user.IsSuperAdmin)
            {
                return Json(new { success = false, message = "Sorry, you cannot delete data as this is not your booking." });
            }
            if (booking.Status == BookingStatus.Paid)
            {
                return Json(new { success = false, message = "Can not delete paid booking." });
            }        
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete success." });
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(string id)
        {
            var booking = await _context.Booking.Include(b => b.Bed.Room.Building.Property).Include(b => b.Agent).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }            

            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType != UserType.Admin && booking.CreatedBy != user.Id && !user.IsSuperAdmin)
            {
                return Json(new { success = false, message = "Sorry, you cannot change data as this is not your booking." });
            }

            if (booking.Status != BookingStatus.New)
            {
                return Json(new { success = false, message = "Sorry, this booking is confirmed already." });
            }

            booking.Status = BookingStatus.Confirmed;
            booking.ModifiedAt = DateTime.UtcNow;
            booking.ModifiedBy = user.Id;
            _context.Booking.Update(booking);
            await _context.SaveChangesAsync();

            try
            {
                EmailTemplate emailTemplate;
                string subject = "", body = "";
                //Call function to send email to vendor and student
                var vendorId = booking.Bed.Room.Building.Property.VendorId;
                var userIds = await _context.UserAccount.Where(u => u.AccountId == vendorId).Select(u => u.UserId).ToListAsync();
                emailTemplate = await _context.EmailTemplate.FirstOrDefaultAsync(e => e.TemplatName == EmailTemplateName.BookingNotification);
                if (emailTemplate != null)
                {
                    subject = StringExtension.Format(emailTemplate.Subject.Format(
                       student_name => booking.FirstName + " " + booking.LastName
                       ));

                    string agentName = booking.Agent?.ContactName ?? "";
                    body = StringExtension.Format(emailTemplate.BodyHTML.Format(
                      agent_name => agentName,
                      booking_details => PopulateBookingDetails(booking)
                      ));

                    foreach (var userId in userIds)
                    {
                        var userVendor = await _context.User.FindAsync(userId);
                        SendEmail(userVendor.Email, subject, body);
                    }
                }
                
                //Send arrival instruction to studnet
                var documents = await _context.PropertyDocument.Where(d => d.PropertyId == booking.Bed.Room.Building.PropertyId && d.Category == DocumentCategory.Booking).ToListAsync();
                emailTemplate = await _context.EmailTemplate.FirstOrDefaultAsync(e => e.TemplatName == EmailTemplateName.ArrivalInstruction);
                if (emailTemplate != null)
                {
                    subject = emailTemplate.Subject;

                    body = StringExtension.Format(emailTemplate.BodyHTML.Format(
                        student_name => booking.FirstName + " " + booking.LastName
                        ));

                    SendEmailWithPropertyDocs(booking.Email, subject, body, documents);
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = "Confirmed success without sending emails to vendors or student.\nError: " + ex.Message });
            }

            return Json(new { success = true, message = "Confirm success." });

        }

        public async Task<IActionResult> Move(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Booking not found."});
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }

            BookingMovingViewModel bookingMovingVM = new BookingMovingViewModel
            {
                BookingId = id,
                BookingFrom = booking.BookingFrom,
                BookingTo = booking.BookingTo,
                BedId = booking.BedId
            };

            //populate dropdown for bed
            var invalidDateBeds = _context.Booking.Where(b => b.BookingFrom <= booking.BookingTo.AddDays(-1) && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingId != booking.BookingId).Select(b => b.Bed).Distinct();
            var invaidGenderRooms = _context.Booking.Where(b => b.Gender != booking.Gender && b.BookingFrom <= booking.BookingTo.AddDays(-1) && b.BookingTo.AddDays(-1) >= booking.BookingFrom).Select(b => b.Bed).Select(r => r.RoomId).Distinct();
            var invalidGenderBeds = _context.Bed.Where(b => invaidGenderRooms.Contains(b.RoomId));
            var beds = await _context.Bed.Include(b => b.Room.Building.Property).Select(b => b).Except(invalidDateBeds).Except(invalidGenderBeds).OrderBy(b => b.Room.Building.Name).ThenBy(b => b.Room.RoomName).ThenBy(b => b.Name).ToListAsync();
            List<SelectListGroup> buildingGroups = new List<SelectListGroup>();
            foreach (var item in _context.Building.Include(b => b.Property).Select(b => b))
            {
                buildingGroups.Add(new SelectListGroup { Name = string.Format("{0} | {1}", item.Property.PropertyName, item.Name) });
            }

            ViewData["BedId"] = new SelectList(beds.Select(b => new SelectListItem
            {
                Value = b.BedId,
                Text = string.Format("Room {0} | Bed {1}", b.Room.RoomName, b.Name),
                Group = buildingGroups.FirstOrDefault(g => g.Name == string.Format("{0} | {1}", b.Room.Building.Property.PropertyName, b.Room.Building.Name)) ?? new SelectListGroup { Name = "Other" },
            }), "Value", "Text", booking.BedId, "Group.Name");

            return PartialView("_Move", bookingMovingVM);
        }

        [HttpPost]
        public async Task<IActionResult> Move([FromBody] BookingMovingViewModel bookingMovingVM)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (bookingMovingVM.BookingTo < bookingMovingVM.BookingFrom)
            {
                return Json(new { success = false, data = "Start date must be less than end date." });
            }

            var booking = await _context.Booking.FindAsync(bookingMovingVM.BookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Booking not found." });
            }

            if (booking.Status == Core.Enums.BookingStatus.Paid)
            {
                return Json(new { success = false, message = "Can not move paid booking." });
            }

            bool isInValidBed = await _context.Booking.AnyAsync(b => b.BookingFrom <= bookingMovingVM.BookingTo.AddDays(-1) && b.BookingTo >= bookingMovingVM.BookingTo.AddDays(-1) && b.BedId == bookingMovingVM.BedId && b.BookingId != bookingMovingVM.BookingId);
            if (isInValidBed)
            {
                return Json(new { success = false, message = "This bed can not be booked in this time range." });
            }

            booking.BedId = bookingMovingVM.BedId;
            booking.BookingFrom = bookingMovingVM.BookingFrom;
            booking.BookingTo = bookingMovingVM.BookingTo;
            booking.ModifiedAt = DateTime.UtcNow;
            booking.ModifiedBy = User.Identity.GetUserId();
            _context.Booking.Update(booking);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Update success." });
        }

        public async Task<IActionResult> GetBed(string id, DateTime bookingFrom, DateTime bookingTo)
        {
            if (bookingTo < bookingFrom)
            {
                return Json(new { success = false, data = "Start date must be less than end date." });
            }

            var booking = await _context.Booking.FindAsync(id);

            var invalidDateBeds = _context.Booking.Where(b => b.BookingFrom <= bookingTo && b.BookingTo >= bookingFrom && b.BookingId != booking.BookingId).Select(b => b.Bed).Distinct();
            var invaidGenderRooms = _context.Booking.Where(b => b.Gender != booking.Gender && b.BookingFrom <= bookingTo && b.BookingTo >= bookingFrom).Select(b => b.Bed).Select(r => r.RoomId).Distinct();
            var invalidGenderBeds = _context.Bed.Where(b => invaidGenderRooms.Contains(b.RoomId));
            var beds = await _context.Bed.Include(b => b.Room.Building.Property).Select(b => b).Except(invalidDateBeds).Except(invalidGenderBeds).OrderBy(b => b.Room.Building.Name).ThenBy(b => b.Room.RoomName).ThenBy(b => b.Name).ToListAsync();
            List<SelectListGroup> buildingGroups = new List<SelectListGroup>();
            foreach (var item in _context.Building.Include(b => b.Property).Select(b => b))
            {
                buildingGroups.Add(new SelectListGroup { Name = string.Format("{0} | {1}", item.Property.PropertyName, item.Name) });
            }

            SelectList bedsSelectlist = new SelectList(beds.Select(b => new SelectListItem
            {
                Value = b.BedId,
                Text = string.Format("Room {0} | Bed {1}", b.Room.RoomName, b.Name),
                Group = buildingGroups.FirstOrDefault(g => g.Name == string.Format("{0} | {1}", b.Room.Building.Property.PropertyName, b.Room.Building.Name)) ?? new SelectListGroup { Name = "Other" },
            }), "Value", "Text", "", "Group.Name");

            return Json(new { success = true, data = bedsSelectlist });

        }


        public async Task<IActionResult> StudentCreate(string id, DateTime bookingFrom, string bedId)
        {
            string accountId = (await _context.UserAccount.FirstOrDefaultAsync(a => a.UserId == User.Identity.GetUserId()))?.AccountId;
            var student = await _context.Customer.FindAsync(accountId);
            if (student == null)
            {
                return new EmptyResult();
            }

            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Student = student;
                ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName");
                ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text");
                ViewData["CountryList"] = new SelectList(Core.Utilities.CommonHelper.CountryNativeList().Select(c => c));
                var bed = await _context.Bed.Include(b => b.Room.RoomType).Include(b => b.Room.Building.Property).FirstOrDefaultAsync();
                Booking booking = new Booking
                {
                    BookingFrom = bookingFrom,
                    BedId = bedId,
                    Bed = bed
                };
                return View(booking);
            }
            else
            {
                var booking = await _context.Booking.Include(b => b.Bed.Room.Building.Property).Include(b => b.Bed.Room.RoomType).FirstOrDefaultAsync(b => b.BookingId == id);
                var user = await _context.User.FindAsync(User.Identity.GetUserId());
                if (user.UserType != UserType.Admin && !user.IsSuperAdmin && booking.Bed.Room.Property.VendorId != accountId && accountId != booking.AgentId && accountId != booking.CustomerId)
                {
                    return new EmptyResult();
                }

                ViewBag.Student = student;
                ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", booking?.AgentId);
                ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text", booking?.CustomerId);
                ViewData["CountryList"] = new SelectList(Core.Utilities.CommonHelper.CountryList().Select(c => new { value = c, text = c }), "value", "text", booking?.Nationality);
                return View(booking);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PaymentWithPayPal([FromBody] [Bind("BookingId,FirstName,LastName,Gender,IDType,IDNumber,Phone,Email,Status,Nationality,AgentId,CustomerId,BookingFrom,BookingTo,BedId,IsConfirmTC,CreatedAt,CreatedBy,Price,DepositAmount,DiscountAmount")] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            string accountId = (await _context.UserAccount.FirstOrDefaultAsync(a => a.UserId == user.Id))?.AccountId;
            if (user.UserType != UserType.Customer)
            {
                return Json(new { success = false, message = "Only customer can access payment." });
            }

            //validate time range
            if (booking.BookingFrom >= booking.BookingTo)
            {
                return Json(new { success = false, message = "Start Date must less than End Date." });
            }
            //validate gender in room
            var roomId = (await _context.Bed.FindAsync(booking.BedId)).RoomId;
            var existBookings = _context.Booking.Where(b => b.Bed.RoomId == roomId && b.BookingId != booking.BookingId && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingFrom <= booking.BookingTo.AddDays(-1) && !b.IsDeleted);
            if (existBookings.Count() > 0)
            {
                bool unValidGender = await existBookings.AnyAsync(b => b.Gender != booking.Gender);
                if (unValidGender)
                {
                    return Json(new { success = false, message = string.Format("{0} can not book this bed in this time range", booking.Gender.ToString()) });
                }
            }

            if (string.IsNullOrEmpty(booking.BookingId))
            {
                //confirm OCA
                if (!booking.IsConfirmTC)
                {
                    return Json(new { success = false, message = "You need to confirm our terms and conditions to book this bed." });
                }
                //validate booking by time range
                bool isExistBooking = await _context.Booking.AnyAsync(b => b.BedId == booking.BedId && b.BookingTo.AddDays(-1) >= booking.BookingFrom && b.BookingFrom <= booking.BookingTo.AddDays(-1) && !b.IsDeleted);
                if (isExistBooking)
                {
                    return Json(new { success = false, message = "This bed can not be booked in this time range." });
                }

                booking.CustomerId = accountId;
                var roomType = (await _context.Bed.Include(b => b.Room.RoomType).FirstOrDefaultAsync(b => b.BedId == booking.BedId)).Room.RoomType;
                booking.Price = roomType.Price;
                booking.DepositAmount = roomType.DepositAmount;
                booking.DiscountAmount = roomType.DiscountAmount;
                booking.Status = BookingStatus.New;
                booking.CreatedBy = user.Id;
                _context.Add(booking);
                //if (!string.IsNullOrEmpty(note))
                //    _context.Add(new BookingNote { Description = note });

                //Add payment of current booking
                Payment payment = new Payment
                {
                    BookingId = booking.BookingId,
                    CustomerId = booking.CustomerId,
                    AgentId = booking.AgentId,
                    Price = booking.Price,
                    Quantity = 1,
                    Amount = booking.Price,
                    PaymentDate = DateTime.Today,
                    PaymentMethod = PaymentMethod.PayPal,
                    PaymentStatus = PaymentStatus.Pending,
                    IsCreateInvoice = false
                };
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirectUrl = Url.Action("PaymentWithPaypal", "PayPalMessage", new { localPaymentId = payment.PaymentId }).ToString() });
            }

            return Json(new { success = false, message = "" });
        }
   
        private async Task SendEmail(string email, string subject, string body)
        {
            await _emailSender.SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailWithPropertyDocs(string email, string subject, string body, List<PropertyDocument> documents)
        {
            await _emailSender.SendEmailWithPropertyDocsAsync(email, subject, body, documents);
        }

        private string PopulateBookingDetails(Booking booking)
        {
            if (booking == null)
            {
                return string.Empty;
            }
            string textBody = " <table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + ">";
            textBody += "<tr><td bgcolor='#4da6ff'>Check-in </td><td> " + booking.BookingFrom.ToShortDateString() + "</td> </tr>";
            textBody += "<tr><td bgcolor='#4da6ff'>Check-out </td><td> " + booking.BookingTo.ToShortDateString() + "</td> </tr>";
            textBody += "<tr><td bgcolor='#4da6ff'>Student Name </td><td> " + booking.FirstName + " " + booking.LastName + "</td> </tr>";
            textBody += "<tr><td bgcolor='#4da6ff'>Property </td><td> " + booking.Bed.Room.Building.Property.PropertyName + "</td> </tr>";
            textBody += "<tr><td bgcolor='#4da6ff'>Room </td><td> " + booking.Bed.Room.RoomName + "</td> </tr>";
            textBody += "<tr><td bgcolor='#4da6ff'>Bed </td><td> " + booking.Bed.Name + "</td> </tr>";
            textBody += "</table>";
            return textBody;
        }

        private bool BookingExists(string id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}
