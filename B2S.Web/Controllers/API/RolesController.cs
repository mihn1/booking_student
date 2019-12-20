using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2S.Web.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IUtilityService _utilityService;

        public RolesController(AppDbContext context
            , Microsoft.AspNetCore.Identity.UserManager<User> userManager
            , IUtilityService utilityService)
        {
            _context = context;
            _userManager = userManager;
            _utilityService = utilityService;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetRoles(string userProfileId)
        {
            var selectedRoles = _context.ProfileRole.Where(p => p.UserProfileId == userProfileId).Select(p => p.Role);
            var roles = _context.Roles.Select(r => new { role = r, isSelectRole = selectedRoles.Contains(r) ? true : false }).OrderBy(r => r.role.Name);
            return Json(new { data = await roles.ToListAsync() });
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> PostRoles(string userProfileId, string[] roleIds)
        {
            if (string.IsNullOrEmpty(userProfileId))
            {
                return Json(new { success = false, message = "No User Profile found." });
            }

            var profileRoles = _context.ProfileRole.Where(p => p.UserProfileId == userProfileId);

            var selectedRoles = _context.Roles.Where(r => roleIds.Contains(r.Id));

            var unselectedRoles = _context.Roles.Where(r => !roleIds.Contains(r.Id));

            //insert to profile role if profile role is not existing
            foreach (var role in selectedRoles)
            {
                if (!(await profileRoles.Select(r => r.RoleId).ContainsAsync(role.Id)))
                {
                    ProfileRole profileRole = new ProfileRole
                    {
                        UserProfileId = userProfileId,
                        RoleId = role.Id
                    };
                    _context.Add(profileRole);
                }
            }

            //delete to profile role if profile role is existing
            foreach (var role in unselectedRoles)
            {
                var profileRole = await profileRoles.FirstOrDefaultAsync(p => p.RoleId == role.Id);
                if (profileRole != null)
                {
                    _context.Remove(profileRole);
                }
            }
            await _context.SaveChangesAsync();

            //remove roles to application users that have same UserProfileId
            IList<User> appUsers = await _context.User.Where(a => a.UserProfileId == userProfileId).ToListAsync();
            foreach (var appUser in appUsers)
            {
                var oldRoleIds = _context.UserRoles.Where(r => r.UserId == appUser.Id).Select(u => u.RoleId);
                var newRoleIds = _context.ProfileRole.Where(p => p.UserProfileId == userProfileId).Select(p => p.RoleId);

                //remove role
                List<string> removeRoleNames = new List<string>();
                foreach (var roleId in oldRoleIds.Except(newRoleIds))
                {
                    var role = await _context.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        removeRoleNames.Add(role.Name);
                    }
                }
                if (removeRoleNames.Count() > 0)
                {
                    await _userManager.RemoveFromRolesAsync(appUser, removeRoleNames);
                }

                //add new role
                List<string> addRoleNames = new List<string>();
                foreach (var roleId in newRoleIds.Except(oldRoleIds))
                {
                    var role = await _context.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        addRoleNames.Add(role.Name);
                    }
                }
                if (addRoleNames.Count() > 0)
                {
                    await _userManager.AddToRolesAsync(appUser, addRoleNames);
                }

                //update roles if same applicatio user
                //ApplicationUser currentUserLogin = await _userManager.GetUserAsync(User);
                //if (appUser.Id == currentUserLogin.Id)
                //{
                //    await _utilityService.UpdateRoles(appUser, currentUserLogin);
                //}
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Update role success." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? "Something went wrong." });
            }
        }
    }
}