using BlogProject.Data;
using BlogProject.Enums;
using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Models.Settings;

namespace BlogProject.Services
{
    public class DataService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IConfiguration _config;
        private readonly CredentialSettings _credSettings;

        public DataService(ApplicationDbContext dbContext,
                           RoleManager<IdentityRole> roleManager,
                           UserManager<BlogUser> userManager,
                           IConfiguration config,
                           IOptions<CredentialSettings> credSettings)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _config = config;
            _credSettings = credSettings.Value;
        }

        public async Task ManageDataAsync()
        {
            await _dbContext.Database.MigrateAsync();

            //Seed Roles into the system.
            await SeedRolesAsync();

            //Seed Users into the system.
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            //If there are already Roles do nothing.
            if (_dbContext.Roles.Any())
            {
                return;
            }
            else
            {
                foreach (var role in Enum.GetNames(typeof(UserRole)))
                {
                    //Create roles with Role Manager
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            //If there are already Users do nothing.
            if (_dbContext.Users.Any())
            {
                return;
            }

            else
            {
                //Create default admin with new instance of BlogUser.
                var administrativeUser = new BlogUser()
                {
                    Email = _config["Mail"],
                    UserName = _config["Mail"],
                    FirstName = _credSettings.FirstName,
                    LastName = _credSettings.LastName,
                    DisplayName = _credSettings.FirstName,
                    PhoneNumber = _credSettings.PhoneNumber,
                    EmailConfirmed = true
                };

                //Use UserManager to create new user defined by administrativeUser.
                await _userManager.CreateAsync(administrativeUser, _config["Password"]);

                //Add to Administrator Role.
                await _userManager.AddToRoleAsync(administrativeUser, UserRole.Administrator.ToString());

                //Create default moderator with new instance of BlogUser.
                var moderatorUser = new BlogUser()
                {
                    Email = _config["ModMail"],
                    UserName = _config["ModMail"],
                    FirstName = _credSettings.FirstName,
                    LastName = _credSettings.LastName,
                    DisplayName = _credSettings.FirstName,
                    PhoneNumber = _credSettings.PhoneNumber,
                    EmailConfirmed = true
                };

                //Use UserManager to create new user defined by moderatorUser.
                await _userManager.CreateAsync(moderatorUser, _config["Password"]);

                //Add to Moderator Role.
                await _userManager.AddToRoleAsync(moderatorUser, UserRole.Moderator.ToString());

                //Create default demoAdminMod with new instance of BlogUser.
                var demoAdminModUser = new BlogUser()
                {
                    Email = _credSettings.DemoMail,
                    UserName = _credSettings.DemoMail,
                    FirstName = "Demo",
                    LastName = "User",
                    DisplayName = _credSettings.DemoMail,
                    PhoneNumber = _credSettings.PhoneNumber,
                    EmailConfirmed = true
                };

                //Use UserManager to create new user defined by demoAdminModUser.
                await _userManager.CreateAsync(demoAdminModUser, _credSettings.DemoPassword);

                //Add to AdminModDemo Role.
                await _userManager.AddToRoleAsync(demoAdminModUser, UserRole.AdminModDemo.ToString());
            }

        }

    }
}
