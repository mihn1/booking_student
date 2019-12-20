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
    public class PropertyController : Controller
    {
        private readonly AppDbContext _context;

        public PropertyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Property
        [HttpGet]
        public async Task<IActionResult> GetProperty(string masterid)
        {
            var data = await _context.Property.Where(p => p.VendorId == masterid).Select(p => new
            {
                p.PropertyId,
                p.PropertyName,
                propertyType = p.PropertyType.ToString(),
                address = string.Format("{0} {1} {2} {3}", p.Address, p.City, p.Province, p.Postcode)
            }).ToListAsync();

            return Json(new { data });
        }

        // POST: api/Property
        [HttpPost]
        public async Task<IActionResult> PostPorperty([FromBody] Property property)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(property.PropertyId))
            {
                try
                {
                    property.CreatedBy = User.Identity.GetUserId();
                    _context.Add(property);
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
                    property.ModifiedAt = DateTime.Now;
                    property.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Edit data success." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                }
            }

        }

        // DELETE: api/Property/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty([FromRoute] string id)
        {
            
            var property = await _context.Property.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            try
            {
                _context.Remove(property);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Delete success." });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }


        private bool ContactExists(string id)
        {
            return _context.Property.Any(e => e.PropertyId == id);
        }

    }
}