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

                // Define roles, users, and passwords
                var roles = new Dictionary<string, (string Email, string Password)>
    {
        { "WebMaster", ("admin@gmail.com", "Admin@123") },
        { "DataProvider", ("dataprovider@gmail.com", "DataProvider@123") },
        { "MonitoringAdmin", ("monitoringadmin@gmail.com", "MonitoringAdmin@123") }
    };

                // Add roles
                logger.LogInformation("Seeding roles.");
                foreach (var role in roles.Keys)
                {
                    await AddRoleAsync(roleManager, role);
                }

                // Add users and assign roles
                foreach (var (role, (email, password)) in roles)
                {
                    logger.LogInformation($"Seeding user for role {role}.");

                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new Users
                        {
                            FullName = $"MetroAir-{role}",
                            UserName = email,
                            NormalizedUserName = email.ToUpper(),
                            Email = email,
                            NormalizedEmail = email.ToUpper(),
                            EmailConfirmed = true,
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            logger.LogInformation($"Assigning {role} role to {email}.");
                            await userManager.AddToRoleAsync(user, role);
                        }
                        else
                        {
                            logger.LogError("Failed to create user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
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
                var result = await roleManager.CreateAsync(new IdentityRole(roleName)   );
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
