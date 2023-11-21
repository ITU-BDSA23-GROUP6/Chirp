using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Chirp.Infrastructure;
using Chirp.Interfaces;
using Chirp.Models;
using DBContext;

using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;

//WHEN IMPLEMENTING GITHUB, WRITE THIS IN TERMINAL IF GIVES YOU A NO CLIENT ID ERR
//dotnet user-secrets set "authentication.github.clientId" "<YOUR_CLIENTID>"
//dotnet user-secrets set "authentication.github.clientSecret" "<YOUR_CLIENTSECRET>"

namespace Chirp.StartUp
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private IConfiguration _configuration { get; set; }
        private IWebHostEnvironment _env { get; set; }

        public Startup(ILogger<Startup> logger, IWebHostEnvironment env, IConfiguration configuration)
        {
            _logger = logger;
            _env    = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultIdentity<Author>(options =>
            {

                // Sign-in Procedure [This has been disabled during Development-Phase]:
                options.SignIn.RequireConfirmedAccount = false;                     // Default is: true
                options.SignIn.RequireConfirmedEmail = false;                       // Default is: true

                // Lock Mechanism: [NOT ACTIVE DURING DEVELOPMENT]
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Sets the maximum amount of "Lock-Out" time.
                //options.Lockout.MaxFailedAccessAttempts = 5;                      // Sets the maximum number of failed attempts.

                // Password:
                options.Password.RequireDigit = false;                              // Default is: true
                options.Password.RequireLowercase = false;                          // Default is: true
                options.Password.RequireNonAlphanumeric = false;                    // Default is: true
                options.Password.RequireUppercase = false;                          // Default is: true
                options.Password.RequiredLength = 6;                                // Default is: 6
                options.Password.RequiredUniqueChars = 1;                           // Default is: 1

                // Users:
                options.User.RequireUniqueEmail = true;                             // Default is: true [..this was a massive pain in the ass...]

            })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();


            try
            {
                _ = services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "GitHub";
                })
                .AddCookie()
                .AddGitHub(o =>
                {//updated ID and SecretID!
                    o.ClientId = _configuration["authentication.github.clientIdAzure"];
                    o.ClientSecret = _configuration["authentication.github.clientSecretAzure"];
                    o.CallbackPath = "/signin-github";
                });
            } catch(Exception exception)
            {
                _logger.LogInformation($"exception {exception.Message}");
            }
            
            SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));

            services.AddDbContext<DatabaseContext>(options =>
            {
                if(_env.IsDevelopment())
                {
                    Console.WriteLine("ENVIRONMENT == DEVELOPMENT");
                    string databasePath = Path.Combine(Path.GetTempPath(), "chirp.db");
                    options.UseSqlite($"Data Source={databasePath}");
                }
                else if(_env.IsProduction())
                {
                    Console.WriteLine("ENVIRONMENT == PRODUCTION");
                    string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("'ConnectionString' could not be located");
                    options.UseSqlServer(connectionString);
                }
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddRazorPages();


            services.AddScoped<ICheepRepository, CheepRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}