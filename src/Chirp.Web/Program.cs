using Microsoft.EntityFrameworkCore;

using DBContext;
using Chirp.Data;
using Chirp.StartUp;

/*
    @DESCRIPTION:
        - This our program entry point. 
        - The entire Entity Framework Core relies heavily on the 'Fluent Interface' API design pattern used in OOP languages.
        - 

*/


namespace Chirp.MainApp;

public class Program
{
    public static void Main(string[] args)
    {
        IHost server = CreateNewServerBuilder(args).Build();

        CreateDb(server);

        server.Run();
    }

    private static void CreateDb(IHost server)
    {
        using (var scope = server.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DatabaseContext>();

                // Apply migrations and create the database if it doesn't exist
                context.Database.Migrate();

                DbInitializer.SeedDatabase(context);

                int numAuthors  = context.Authors.Count();
                int numCheeps   = context.Cheeps.Count();
                int numOpinions = context.CheepLikeDis.Count();

                Console.WriteLine($"Num. Authors: {numAuthors} \n Num. Cheeps: {numCheeps} \n Num. Opinions: {numOpinions}");

            }
            catch (Exception e)
            {
                Console.WriteLine("Migration and or Database Seeding Error: " + e.Message);
            }
        }
    }

    public static IHostBuilder CreateNewServerBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
}
