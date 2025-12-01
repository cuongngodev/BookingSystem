using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using WebBookingSystem.Data;
using WebBookingSystem.Data.Entities;
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
                _db.Database.EnsureCreated();

                _logger.LogInformation("Database verified/created successfully.");
                
                //  Create roles
                await SeedRolesAsync();

                // Create service
                await SeedServiceAsync();

                // Create admin user
                await SeedAdminUser();

                // Create employee user
                await SeedEmployeeUser();

                //Create customer user
                await SeedCustomerUsersAsync();

                //Create Appoitnment
                await SeedAppointmentsAsync();


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

            try
            { 
                //ContentRootPath is refering to the folders not related to wwwroot
                var file = Path.Combine(_hosting.ContentRootPath, "Data/service.json");
                _logger.LogInformation("Reading services from file: {FilePath}", file);
                    
                // Check file whether exist
                if (!File.Exists(file))
                {
                    _logger.LogError("Service.json file not found at: {FilePath}", file);
                    return;
                }

                var json = File.ReadAllText(file);

                //Deserialise the json file into the List of Product class
                var services = JsonSerializer.Deserialize<IEnumerable<Service>>(json);

                if (services != null && services.Any())
                {
                    int addedCount = 0;

                    foreach (var service in services)
                    {
                        var exist = await _db.Services.AnyAsync(s => s.Name == service.Name);
                        if (!exist){
                            _db.Services.Add(service);
                            _logger.LogInformation("Adding new service: {ServiceName}", service.Name);
                            addedCount++;
                        }
                        else
                        {
                            _logger.LogInformation("Service already exists: {ServiceName}", service.Name);
                        }
                    }
                    if (addedCount  > 0)
                    {
                        await _db.SaveChangesAsync();  //commit changes to the database (make permanent) 
                        _logger.LogInformation("Successfully added {Count} services to the database.", services.Count());

                    }
                    else
                    {
                        _logger.LogInformation("No new services to add.");
                    }
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

        /// <summary>
        /// See User (Admin, Employee)
        /// </summary>
        /// <returns></returns>
        private async Task SeedAdminUser()
        {
            if (!_userManager.Users.Any())
            {

                /**
                 * Create User as Admin
                 */
                var admin = new ApplicationUser
                {
                    UserName = "adminpro",
                    Email = "adminpro@gmail.com",
                    LastName = "Admin",
                    FirstName = "Pro",
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                
                };

                // Create admin with pwd
                var result = await _userManager.CreateAsync(admin, "Admin123@");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully: {Email}", admin.Email);
                    // Add role
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
        /// <summary>
        /// See User (Admin, Employee)
        /// </summary>
        /// <returns></returns>
        private async Task SeedEmployeeUser()
        {
            /**
                * Create User as Employee
                */

            var employee = new ApplicationUser
            {
                UserName = "employeepro",
                Email = "employeepro@gmail.com",
                LastName = "Employee",
                FirstName = "Pro",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
            };
            // Create admin with pwd
            var result2 = await _userManager.CreateAsync(employee, "Employee123@");
            if (result2.Succeeded)
            {
                _logger.LogInformation("Employee user created successfully: {Email}", employee.Email);
                // Add role
                var roleResult = await _userManager.AddToRoleAsync(employee, "Employee");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Employee user assigned to 'Employee' role successfully.");
                }
                else
                {
                    _logger.LogError("Failed to assign Employee role: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Log detailed reason why CreateAsync failed
                foreach (var error in result2.Errors)
                {
                    _logger.LogError("User creation error: {Code} - {Description}", error.Code, error.Description);
                }
            }
        }

        //Seed customer using teh users.json file
        private async Task SeedCustomerUsersAsync()
        {
            _logger.LogInformation("Seeding customer users...");

            try
            {
                var file = Path.Combine(_hosting.ContentRootPath, "Data/users.json");

                if (!File.Exists(file))
                {
                    _logger.LogWarning("users.json not found at {Path}", file);
                    return;
                }

                var json = File.ReadAllText(file);
                var customers = JsonSerializer.Deserialize<IEnumerable<ApplicationUser>>(json);

                if (customers == null || !customers.Any())
                {
                    _logger.LogWarning("No customers found in users.json");
                    return;
                }

                foreach (var user in customers)
                {
                    var exists = await _userManager.FindByEmailAsync(user.Email);
                    if (exists != null)
                    {
                        _logger.LogInformation("Customer already exists: {Email}", user.Email);
                        continue;
                    }

                    // Create customer with password
                    var result = await _userManager.CreateAsync(user, "Customer123@");

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Customer created: {Email}", user.Email);

                        await _userManager.AddToRoleAsync(user, "Customer");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError("Customer user creation error: {Code} - {Description}",
                                error.Code, error.Description);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding customers");
                throw;
            }
        }
        //Seed appointmnets using appointment.json
        private async Task SeedAppointmentsAsync()
        {
            _logger.LogInformation("Seeding appointments...");

            try
            {
                var file = Path.Combine(_hosting.ContentRootPath, "Data/appointments.json");

                if (!File.Exists(file))
                {
                    _logger.LogWarning("appointments.json not found at {Path}", file);
                    return;
                }

                var json = File.ReadAllText(file);
                var appointments = JsonSerializer.Deserialize<IEnumerable<Appointment>>(json);

                if (appointments == null || !appointments.Any())
                {
                    _logger.LogWarning("No appointments found in appointments.json");
                    return;
                }

                int added = 0;

                foreach (var appt in appointments)
                {
                    bool exists = await _db.Appointments.AnyAsync(a =>
                        a.UserId == appt.UserId &&
                        a.ServiceId == appt.ServiceId &&
                        a.AppointmentTime == appt.AppointmentTime
                    );

                    if (exists)
                    {
                        continue; // skip duplicates
                    }

                    // Ensure User exists
                    var user = await _userManager.FindByIdAsync(appt.UserId.ToString());
                    if (user == null)
                    {
                        _logger.LogWarning("Skipping appointment — user not found: {UserId}", appt.UserId);
                        continue;
                    }

                    // Ensure Service exists
                    var serviceExists = await _db.Services.AnyAsync(s => s.Id == appt.ServiceId);
                    if (!serviceExists)
                    {
                        _logger.LogWarning("Skipping appointment — service not found: {ServiceId}", appt.ServiceId);
                        continue;
                    }

                    _db.Appointments.Add(appt);
                    added++;
                }

                if (added > 0)
                {
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} appointments.", added);
                }
                else
                {
                    _logger.LogInformation("No new appointments to add.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding appointments");
                throw;
            }
        }

    }
}
