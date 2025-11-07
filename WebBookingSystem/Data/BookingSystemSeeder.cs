using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using BookingSystem.Data.Entities;
using WebBookingSystem.Data;
namespace BookingSystem.Data
{
    public class BookingSystemSeeder
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hosting;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public BookingSystemSeeder(
            ApplicationDbContext db, 
            IWebHostEnvironment hosting,
            RoleManager<IdentityRole<int>> roleManager
        )
        {
            _db = db;
            _hosting = hosting;
            _roleManager = roleManager;
            
        }

        public async Task Seed()
        {
            //Verify that the database exists. Hover over the method and read the documentation. 
            _db.Database.EnsureCreated();
            //  Create roles
            await SeedRolesAsync();

            // Create service
            await SeedServiceAsync();
            //

        }
        private async Task SeedServiceAsync()
        {
            if (!_db.Services.Any())
            {
                //ContentRootPath is refering to the folders not related to wwwroot
                var file = Path.Combine(_hosting.ContentRootPath, "Data/service.json");
                var json = File.ReadAllText(file);

                //Deserialise the json file into the List of Product class
                var services = JsonSerializer.Deserialize<IEnumerable<Service>>(json);

                //Add the new list of products to the database
                _db.Services.AddRange(services);

                _db.SaveChanges();  //commit changes to the database (make permanent) 
            }
        }
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Employee", "Customer" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }
  
    }
}
