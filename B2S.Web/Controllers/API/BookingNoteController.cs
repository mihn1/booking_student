using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class BookingNoteController : Controller
    {
        private readonly AppDbContext _context;

        public BookingNoteController(AppDbContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> GetBookingNote(string bookingId)
        {
            var notes = _context.BookingNote.Where(n => n.BookingId == bookingId).OrderByDescending(n => n.CreatedAt);
            return Json(new { data = await notes.ToListAsync() });
        }

        [HttpPost]
        public async Task<IActionResult> PostBookingNote([FromBody] BookingNote bookingNote)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(bookingNote.NoteId))
            {
                try
                {
                    bookingNote.CreatedBy = User.Identity.GetUserId();
                    _context.Add(bookingNote);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Add new note success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? "Somethings went wrong" });
                }
            }
            else
            {
                try
                {
                    bookingNote.ModifiedAt = DateTime.Now;
                    bookingNote.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(bookingNote);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit note success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? "Somethings went wrong" });
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DelNote([FromRoute] string id)
        {
            var bookingNote = await _context.BookingNote.FindAsync(id);
            if (bookingNote == null)
            {
                return NotFound();
            }

            _context.Remove(bookingNote);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Delete success" });
        }
    }
}