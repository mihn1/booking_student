using B2S.Core.Entities;
using B2S.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace B2S.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUtilityService utilityService)
        {
            context.Database.Migrate();

            await utilityService.CreateNewRoles();

            //Check for users
            if (context.User.Any())
            {
                return; //If user is not empty, DB has been seed
            }

            //Init B2S Data Settings
            await utilityService.InitApp();
            
        }
    }
}
