using BookingSystem.Data;
using BookingSystem.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using WebBookingSystem.Data;
using WebBookingSystem.Data.Intefaces;
using WebBookingSystem.Data.Repositories;
using WebBookingSystem.Services;

namespace WebBookingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {         

            // Create a unique log filename per app run
            var logFileName = $"Logs/log-{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            // Ensure Logs directory exists
            Directory.CreateDirectory("Logs");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.File(
                    path: logFileName,
                    rollingInterval: RollingInterval.Infinite,
                    shared: false,
                    buffered: false,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                )
                .CreateLogger();

            try
            { 

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();
                string connectionString;
                if (builder.Environment.IsDevelopment())
                {
                    connectionString = builder.Configuration.GetConnectionString("AzureConnection");
                }
                else
                {
                    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                }

                // JWT setup
                var jwtSettings = builder.Configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

                builder.Services.AddAuthentication(options =>
                {
                    // Set the default scheme used for authentication
                    options.DefaultAuthenticateScheme = "JwtBearer";
                    options.DefaultChallengeScheme = "JwtBearer";
                })
                .AddJwtBearer("JwtBearer", options =>
                {
                    // Disable HTTPS requirement
                    options.RequireHttpsMetadata = false;
                    // Save the token in the AuthenticationProperties after a successful authorization
                    options.SaveToken = true;

                    // Configure how incoming JWTs should be validated
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

                // Add services to the container.
                //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();


                builder.Services.AddControllersWithViews();
                builder.Services.AddTransient<BookingSystemSeeder>();

                //Register JwtService
                builder.Services.AddScoped<JwtService>();

                // Enable Identity with custom ApplicationUser and integer keys
                builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(
                    options =>
                    {
                        // Enable password complexity requirements
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = true;
                        options.Password.RequiredLength = 6;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.AddRazorPages();

                // Repositories and Unit of Work
                builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<AppointmentValidationService>();
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    // Where to send users who aren’t logged in
                    options.LoginPath = "/Auth/Login";

                    // Where to send users who are logged in but lack the right role
                    options.AccessDeniedPath = "/Auth/AccessDenied";

                    // Optional session settings
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });



                var app = builder.Build();

                await RunSeeding(app);
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                //df
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                app.MapRazorPages();

                Log.Information("Starting Booking system...");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        private static async Task RunSeeding(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                //var seeder = scope.ServiceProvider.GetService<BookingSystemSeeder>();
                var seeder = scope.ServiceProvider.GetRequiredService<BookingSystemSeeder>();
                await seeder.Seed();
            }
        }
    }
}
