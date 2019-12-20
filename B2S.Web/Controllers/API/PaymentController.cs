using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers.API
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems(string bookingId)
        {
            var data = _context.Payment.Where(p => p.BookingId == bookingId)
                .Select(p => new
                {
                    p.PaymentId,
                    PaymentDate = p.PaymentDate.ToShortDateString(),
                    p.Amount,
                    p.Description
                });
            return Json(new { data = await data.ToListAsync() });
        }

        [HttpPost]
        public async Task<IActionResult> PostItem([FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(payment.PaymentId))
            {
                try
                {
                    payment.CreatedBy = User.Identity.GetUserId();
                    _context.Add(payment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Add new data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }

            }
            else
            {
                try
                {
                    payment.ModifiedAt = DateTime.Now;
                    payment.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            }
        }


        // DELETE: api/Payment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {

            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            try
            {
                _context.Remove(payment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Delete success." });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }


    }
}