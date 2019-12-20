using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers.API
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class InvoiceItemController : Controller
    {
        private readonly AppDbContext _context;

        public InvoiceItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceItem
        [HttpGet]
        public async Task<IActionResult> GetInvoiceItem(string invoiceId)
        {
            var data = _context.InvoiceItem.Where(p => p.InvoiceId == invoiceId);   
            return Json(new { data = await data.ToListAsync() });
        }

        // POST: api/InvoiceItem
        [HttpPost]
        public async Task<IActionResult> PostInvoiceItem([FromBody] InvoiceItem invoiceItem)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(invoiceItem.InvoiceItemId))
            {
                try
                {
                    invoiceItem.CreatedBy = User.Identity.GetUserId();
                    _context.Add(invoiceItem);
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
                    invoiceItem.ModifiedAt = DateTime.Now;
                    invoiceItem.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(invoiceItem);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            }
        }


        // DELETE: api/InvoiceItem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceItem(string id)
        {

            var invoiceItem = await _context.InvoiceItem.FindAsync(id);
            if (invoiceItem == null)
            {
                return NotFound();
            }

            try
            {
                _context.Remove(invoiceItem);
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