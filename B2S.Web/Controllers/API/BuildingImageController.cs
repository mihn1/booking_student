using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using B2S.Web.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers.API
{
    [Route("api/[controller]")]
    [Controller]
    public class BuildingImageController : Controller
    {
        private readonly AppDbContext _context;
        public BuildingImageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages(string parentId)
        {
            return Json(new { data = await _context.BuildingImage.Where(i => i.BuildingId == parentId).Select(i => new { i.ImageId, i.IsDefault }).ToListAsync() });
        }

        [HttpPost]
        [RequestSizeLimit(15000000)]
        public async Task<IActionResult> PostUploadImage(string parentId, IFormFile formFile)
        {
            try
            {
                if (formFile == null)
                {
                    return Json(new { success = false, message = "No file upload" });
                }
                if (!formFile.IsImage())
                {
                    return Json(new { success = false, message = "Invalid file upload" });
                }                

                byte[] newImageFile;
                using (var stream = new MemoryStream())
                {
                    await formFile.CopyToAsync(stream);
                    newImageFile = stream.ToArray();
                    BuildingImage buildingImage = new BuildingImage
                    {
                        BuildingId = parentId,
                        ImageFile = newImageFile,
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = User.Identity.GetUserId()
                    };
                    _context.Add(buildingImage);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Upload image success." });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/BuildingImage/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage([FromRoute] string id)
        {
            var image = await _context.BuildingImage.FindAsync(id);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            try
            {
                _context.Remove(image);
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