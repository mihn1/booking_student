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

namespace B2S.Web.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserAccountController : Controller
    {
        private readonly AppDbContext _context;

        public UserAccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(Agent account)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            try
            {
                account.ModifiedAt = DateTime.Now;
                account.ModifiedBy = User.Identity.GetUserId();
                _context.Update(account);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Edit account success." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(Customer account)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            try
            {
                account.ModifiedAt = DateTime.Now;
                account.ModifiedBy = User.Identity.GetUserId();
                _context.Update(account);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Edit account success." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(Vendor account)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            try
            {
                account.ModifiedAt = DateTime.Now;
                account.ModifiedBy = User.Identity.GetUserId();
                _context.Update(account);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Edit account success." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

    }
}