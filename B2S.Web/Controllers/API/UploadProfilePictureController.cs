using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace B2S.Web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/UploadProfilePicture")]
    [Authorize]
    public class UploadProfilePictureController : Controller
    {
        private readonly IUtilityService _utilityService;
        private readonly IHostingEnvironment _env;
        private readonly AppDbContext _context;

        public UploadProfilePictureController(IUtilityService utilityService,
            IHostingEnvironment env,
            AppDbContext context)
        {
            _utilityService = utilityService;
            _env = env;
            _context = context;
        }

        [HttpPost]
        [RequestSizeLimit(15000000)]
        public async Task<IActionResult> PostUploadProfilePicture(List<IFormFile> files)
        {
            try
            {
                var fileName = await _utilityService.UploadFile(files, _env);
                //try to update the user profile pict
                Core.Entities.User appUser = await _context.User.FindAsync(User.Identity.GetUserId());
                appUser.ProfilePictureUrl = "/uploads/" + fileName;
                _context.Update(appUser);
                await _context.SaveChangesAsync();
                return Ok(fileName);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = ex.Message });
            }


        }
    }
}