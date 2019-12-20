using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace B2S.Web.Controllers
{
    [Authorize]
    public class BuildingImageController : Controller
    {
        private readonly AppDbContext _context;
        public BuildingImageController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetImage(string id)
        {
            var image = await _context.BuildingImage.FindAsync(id);
            if (image == null || image.ImageFile.Length == 0)
            {
                return null;
            }

            return File(image.ImageFile, "image/png");
        }

        public async Task<IActionResult> ImageBox(string curImgId, string move)
        {
            var curImg = await _context.BuildingImage.FindAsync(curImgId);
            if (curImg == null)
            {
                return new EmptyResult();
            }
            ViewBag.IsInit = true;
            var imgs = _context.BuildingImage.OrderBy(i => i.CreatedAt).Where(i => i.BuildingId == curImg.BuildingId);

            if (move == "Nxt")
            {
                var newImg = await imgs.FirstOrDefaultAsync(i => i.CreatedAt > curImg.CreatedAt) ?? await imgs.FirstOrDefaultAsync();
                ViewData["TotalImgs"] = await imgs.CountAsync();
                ViewData["CurIndex"] = imgs.IndexOf(newImg) + 1;
                ViewBag.IsInit = false;
                return PartialView(newImg);
            }

            if (move == "Prev")
            {
                var newImg = await imgs.LastOrDefaultAsync(i => i.CreatedAt < curImg.CreatedAt) ?? await imgs.LastOrDefaultAsync();
                ViewData["TotalImgs"] = await imgs.CountAsync();
                ViewData["CurIndex"] = imgs.IndexOf(newImg) + 1;
                ViewBag.IsInit = false;
                return PartialView(newImg);
            }
            ViewData["TotalImgs"] = await imgs.CountAsync();
            ViewData["CurIndex"] = imgs.IndexOf(curImg) + 1;
            return PartialView(curImg);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBuildingImage(string id, bool isDefault)
        {

            var image = await _context.BuildingImage.FindAsync(id);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            image.IsDefault = isDefault;
            image.ModifiedAt = DateTime.UtcNow;
            image.ModifiedBy = User.Identity.GetUserId();
            _context.BuildingImage.Update(image);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Update image success." });
        }

    }
}