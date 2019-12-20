using B2S.Core.Common;
using B2S.Core.Entities;
//using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using B2S.Web.ViewModels.UserSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.Controllers
{
    [Authorize(Roles = Core.Common.Modules.User.Role)]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUtilityService _utilityService;
        private readonly UserManager<Core.Entities.User> _userManager;
        private readonly ILogger _logger;

        public UserController(AppDbContext context,
            IUtilityService utilityService,
            UserManager<Core.Entities.User> userManager,
            ILogger<UserController> logger)
        {
            _context = context;
            _utilityService = utilityService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = _context.User.Include(u => u.UserProfile);
            return View(await users.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.Include(u => u.UserProfile)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewData["UserProfileId"] = new SelectList(_context.UserProfile, "UserProfileId", "UserProfileName");
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel userVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    userVM.User.ProfilePictureUrl = "/images/userprofile/empty_profile.png";
                    userVM.User.UserName = userVM.User.Email;
                    await _userManager.CreateAsync(userVM.User, userVM.Password);
                    //Update roles for user
                    await _utilityService.UpdateUserRoles(userVM.User, userVM.User.UserProfileId);
                   
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewData["UserProfileId"] = new SelectList(_context.UserProfile, "UserProfileId", "UserProfileName", userVM.User.UserProfileId);
                throw;
            }

            return View(userVM);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var User = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
            if (User == null)
            {

                return NotFound();
            }

            ViewData["UserProfileId"] = new SelectList(_context.UserProfile, "UserProfileId", "UserProfileName", User.UserProfileId);
            return View(User);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //super admin should always have access to Roles
                    //User.UserRole = User.IsSuperAdmin ? true : User.UserRole;
                    //_context.Update(User);                  

                    user.UserName = user.Email;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    await _userManager.UpdateAsync(user);


                    //Update roles for user
                    await _utilityService.UpdateUserRoles(user, user.UserProfileId);

                    //implement to update role immediately with out relogin
                    User currentUserLogin = await _userManager.GetUserAsync(User);
                    if (user.Id == currentUserLogin.Id)
                    {
                        await _utilityService.UpdateRoles(user, currentUserLogin);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(User);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var User = await _context.User
                .SingleOrDefaultAsync(m => m.Id == id);
            if (User == null)
            {
                return NotFound();
            }

            return View(User);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var User = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
                _context.User.Remove(User);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                throw;
            }

        }



        private bool UserExists(string id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
