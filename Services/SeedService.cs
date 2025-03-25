using Microsoft.AspNetCore.Identity;
using UserRoles.Data;
using UserRoles.Models;

namespace UserRoles.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                logger.LogInformation("Ensuring the database is created.");
                await context.Database.EnsureCreatedAsync();

                // Define roles
                var roles = new List<string>
                {
                    "WebMaster",  
                    "DataProvider",       
                    "MonitoringAdmin",
                };

                // Add roles
                logger.LogInformation("Seeding roles.");
                foreach (var role in roles)
                {
                    await AddRoleAsync(roleManager, role);
                }

                
                var webMasterEmail = "admin@gmail.com";
                var password = "Admin@123";
                if (await userManager.FindByEmailAsync(webMasterEmail) == null)
                {
                    var user = new Users
                    {
                        FullName = "MetroAir-WebMaster",
                        UserName = webMasterEmail,
                        NormalizedUserName = webMasterEmail.ToUpper(),
                        Email = webMasterEmail,
                        NormalizedEmail = webMasterEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Assigning WebMaster role to {webMasterEmail}.");
                        await userManager.AddToRoleAsync(user, "WebMaster");
                    }
                    else
                    {
                        logger.LogError("Failed to create WebMaster user {Email}: {Errors}", webMasterEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation($"User {webMasterEmail} already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

    }
}
