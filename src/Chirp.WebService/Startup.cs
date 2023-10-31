using DBContext;
using Chirp.Infrastructure;
using Chirp.Interfaces;
using Microsoft.EntityFrameworkCore;

/*
    @DESCRIPTION:
        - The 'Startup.cs' file is an industry standard throughout many C# projects.
        - The aim of the file is to encapsulate the applicatins services and middleware being injected on start up.
        - The file is divide into 2 sections:
                01. Configure Services:
                    -> Used to configure Depedency Injection
                02. Configure Middle:
                    -> Used to inject Middleware into the Request Pipeline - [https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0]
    
        - Both of these sections perform Dependecy Injection (DI). 
            -> The core aim of DI is to "Inversion of Control" a design principle wherein classes are designed to recieve objects rather
               than instantiate it within the class, allowing for loose coupling between objects.
            -> The resulting applications are more testable, modular, and maintainable as a result.
            -> Example: [https://stackoverflow.com/questions/3058/what-is-inversion-of-control].

        - For future reference - OAuth, Cookies and JWT Security [https://fusionauth.io/blog/securing-asp-netcore-razor-pages-app-with-oauth]
*/

namespace Chirp.Startup
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddDbContext<DatabaseContext>(options => {
                _ = options.UseSqlite(_configuration.GetConnectionString("DefaultConnection")) ?? throw new InvalidOperationException("Connection string was invalid.");
            });

            // When you request an ICheepRepository in your application, ASP.NET Core's dependency injection system will 
            // create an instance of CheepRepository and provide it to you.
            
            services.AddScoped<ICheepRepository, CheepRepository>();
            //services.AddScoped<IAuthorRepository, AuthorRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}