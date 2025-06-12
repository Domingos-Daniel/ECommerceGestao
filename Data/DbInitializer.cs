using ECommerceGestao.Data;
using ECommerceGestao.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceGestao.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    // Ensure database is created and migrations are applied
                    context.Database.EnsureCreated();
                    context.Database.Migrate();
                    
                    // Check if data already exists
                    if (context.Products.Any())
                    {
                        logger.LogInformation("Database already seeded");
                        return;
                    }

                    // Seed data is defined in ApplicationDbContext.OnModelCreating
                    logger.LogInformation("Database seeded successfully");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }
    }

    public static class IdentityDataInitializer
    {
        public static async Task SeedUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles
            string[] roleNames = { "Admin", "Customer" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                Name = "System Admin",
                EmailConfirmed = true
            };

            var existingAdmin = await userManager.FindByEmailAsync(adminUser.Email);

            if (existingAdmin == null)
            {
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create customer user
            var customerUser = new ApplicationUser
            {
                UserName = "customer@example.com",
                Email = "customer@example.com",
                Name = "Sample Customer",
                EmailConfirmed = true
            };

            var existingCustomer = await userManager.FindByEmailAsync(customerUser.Email);

            if (existingCustomer == null)
            {
                var result = await userManager.CreateAsync(customerUser, "Customer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
        }
    }
}
