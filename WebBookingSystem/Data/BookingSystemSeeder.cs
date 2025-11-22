using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using BookingSystem.Data.Entities;
using WebBookingSystem.Data;
using System.Threading.Tasks;
namespace BookingSystem.Data
{
    public class BookingSystemSeeder
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hosting;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingSystemSeeder> _logger;

        public BookingSystemSeeder(
            ApplicationDbContext db, 
            IWebHostEnvironment hosting,
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<BookingSystemSeeder> logger
        )
        {
            _db = db;
            _hosting = hosting;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Seed()
        {
            try
            {
                _logger.LogInformation("Starting database seeding at {Time}", DateTime.Now);
                //Verify that the database exists. Hover over the method and read the documentation. 
                //_db.Database.EnsureCreated();
                _logger.LogInformation("Database verified/created successfully.");
                
                //  Create roles
                await SeedRolesAsync();

                // Create service
                await SeedServiceAsync();

                // Create admin user
                await SeedAdminUser();
                
                _logger.LogInformation("Database seeding completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during seeding at {Time}", DateTime.Now);
                throw;
            }
        }
        private async Task SeedServiceAsync()
        {
            _logger.LogInformation("Seeding services...");

            if (!_db.Services.Any())
            {
                try
                { 
                    //ContentRootPath is refering to the folders not related to wwwroot
                    var file = Path.Combine(_hosting.ContentRootPath, "Data/service.json");
                    _logger.LogInformation("Reading services from file: {FilePath}", file);
                    var json = File.ReadAllText(file);

                    //Deserialise the json file into the List of Product class
                    var services = JsonSerializer.Deserialize<IEnumerable<Service>>(json);

                    if (services != null && services.Any())
                    {
                        //Add the new list of products to the database
                        _db.Services.AddRange(services);

                        _db.SaveChanges();  //commit changes to the database (make permanent) 
                        _logger.LogInformation("Successfully added {Count} services to the database.", services.Count());
                    }
                    else
                    {
                        _logger.LogWarning("No services were found in the JSON file.");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while seeding services at {Time}", DateTime.Now);
                    throw;
                }

            }
            else
            {
                _logger.LogInformation("Services already exist — skipping service seeding.");
            }
        }
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Employee", "Customer" };

            _logger.LogInformation("Seeding roles...");

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    if (result.Succeeded)
                        _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                    else
                        _logger.LogWarning("Failed to create role '{RoleName}': {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Role '{RoleName}' already exists.", roleName);
                }
            }
        }

        private async Task SeedAdminUser()
        {
            if (!_userManager.Users.Any())
            {
                var admin = new ApplicationUser
                {
                    UserName = "adminpro",
                    Email = "adminpro@gmail.com",
                    LastName = "Admin",
                    FirstName = "Pro",
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                };

                // Create user with pwd
                var result = await _userManager.CreateAsync(admin, "Admin123@");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully: {Email}", admin.Email);

                    var roleResult = await _userManager.AddToRoleAsync(admin, "Admin");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("Admin user assigned to 'Admin' role successfully.");
                    }
                    else
                    {
                        _logger.LogError("Failed to assign Admin role: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // Log detailed reason why CreateAsync failed
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("User creation error: {Code} - {Description}", error.Code, error.Description);
                    }
                }
            }
        }

  
    }
}
