using System;
using System.Collections.Generic;
using System.IO;
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
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class PropertyDocumentController : Controller
    {
        private readonly AppDbContext _context;

        public PropertyDocumentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PropertyDocument
        [HttpGet]
        public async Task<IActionResult> GetDocument(string propertyId)
        {
            return Json(new { data = await _context.PropertyDocument.Where(d => d.PropertyId == propertyId).ToListAsync() });
        }


        // POST: api/PropertyDocument
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostDocument(PropertyDocument document)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            if (string.IsNullOrEmpty(document.DocumentId))
            {
                var formFile = Request.Form.Files["FileName"];
                if (formFile != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(stream);
                        document.FileBody = stream.ToArray();
                    }
                    document.FileName = formFile.FileName;
                    document.FileType = formFile.ContentType;
                }
                //document.DocumentId = Guid.NewGuid().ToString();
                document.CreatedBy = User.Identity.GetUserId();
                _context.PropertyDocument.Add(document);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Add new data success." });
            }
            return Json(new { success = false, message = "Document is already existed." });
        }

        // DELETE: api/PropertyDocument/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask([FromRoute] string id)
        {
            var document = await _context.PropertyDocument.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.PropertyDocument.Remove(document);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Delete success." });
        }

        private bool DocumentExists(string id)
        {
            return _context.PropertyDocument.Any(e => e.DocumentId == id);
        }
    }
}