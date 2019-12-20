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
    public class BedController : Controller
    {
        private readonly AppDbContext _context;

        public BedController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Bed
        [HttpGet]
        public async Task<IActionResult> GetItems(string roomId)
        {
            var data = _context.Bed.Where(p => p.RoomId == roomId);
            return Json(new { data = await data.ToListAsync() });
        }

        // POST: api/Bed
        [HttpPost]
        public async Task<IActionResult> PostItem([FromBody] Bed bed)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(bed.BedId))
            {
                try
                {
                    bed.CreatedBy = User.Identity.GetUserId();
                    _context.Add(bed);
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
                    bed.ModifiedAt = DateTime.Now;
                    bed.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(bed);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            }
        }


        // DELETE: api/Bed/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {

            var bed = await _context.Bed.FindAsync(id);
            if (bed == null)
            {
                return NotFound();
            }

            try
            {
                _context.Remove(bed);
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