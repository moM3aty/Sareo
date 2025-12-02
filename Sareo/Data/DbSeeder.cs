
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sareoo.Models;

namespace Sareoo.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Seed Roles
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Student"));
            await roleManager.CreateAsync(new IdentityRole("Teacher"));

            // Create Admin User
            if (await userManager.FindByEmailAsync("admin@Sareoo.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@Sareoo.com",
                    Email = "admin@Sareoo.com",
                    FullName = "Admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
