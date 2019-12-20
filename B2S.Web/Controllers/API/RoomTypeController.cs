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
    public class RoomTypeController : Controller
    {
        private readonly AppDbContext _context;

        public RoomTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RoomType
        [HttpGet]
        public async Task<IActionResult> GetItems(string propertyId)
        {
            var data = _context.RoomType.Where(p => p.PropertyId == propertyId);
            return Json(new { data = await data.ToListAsync() });
        }

        // POST: api/RoomType
        [HttpPost]
        public async Task<IActionResult> PostItem([FromBody] RoomType roomType)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(roomType.RoomTypeId))
            {
                try
                {
                    roomType.CreatedBy = User.Identity.GetUserId();
                    _context.Add(roomType);
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
                    roomType.ModifiedAt = DateTime.Now;
                    roomType.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            }
        }


        // DELETE: api/RoomType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {

            var roomType = await _context.RoomType.FindAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            try
            {
                _context.Remove(roomType);
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