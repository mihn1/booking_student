using System;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using B2S.Web.ViewModels.UserSecurity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers
{
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
        public async Task<IActionResult> UpdateAgent(Agent account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (account.AgentId == null)
            {
                return NotFound();
            }
            account.ModifiedAt = DateTime.Now;
            account.ModifiedBy = User.Identity.GetUserId();
            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyAccount", "Manage");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomer(Customer account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (account.CustomerId == null)
            {
                return NotFound();
            }
            account.ModifiedAt = DateTime.Now;
            account.ModifiedBy = User.Identity.GetUserId();
            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyAccount", "Manage");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVendor(Vendor account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (account.VendorId == null)
            {
                return NotFound();
            }
            account.ModifiedAt = DateTime.Now;
            account.ModifiedBy = User.Identity.GetUserId();
            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyAccount", "Manage");

        }

    }
}