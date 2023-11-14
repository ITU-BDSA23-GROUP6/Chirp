using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Chirp.Models;
using Microsoft.Extensions.Configuration;

/*
    @DESCRIPTION:
        - In following is a subclass of the class 'DbContext'.
        - It follows the behavioural pattern 'Unit of Work'. Simply put, this is everything one does in a single business transaction 
          which may alter a databas - [https://en.wikipedia.org/wiki/Unit_of_work].

    @KEY INFO:
        - Our 'Models' folder contains the "Entities" of our SQLite database.
            -> In DBMS, an entity is a piece of data tracked and stored by the system. 
        - The ModelBuilder class acts as a Fluent API, a design pattern based on method chaining - [https://www.entityframeworktutorial.net/efcore/fluent-api-in-entity-framework-core.aspx].
        
*/

namespace DBContext;
public class DatabaseContext : IdentityDbContext<Author>
{
    public virtual DbSet<Cheep> Cheeps { get; set; } = null!;
    public virtual DbSet<Author> Authors { get; set; } = null!;

    public DatabaseContext()
    {
        // Must have empty constructor for Compile-Time Migrtion to work.
    }


    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // [Try to move all of this to Entities and use Data Annotations]
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Authors");
            entity.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Cheep>(entity =>
        {
            entity.ToTable("Cheeps");
            entity.Property(cheep => cheep.Text).HasMaxLength(160);
        });
        base.OnModelCreating(modelBuilder);

    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=tcp:chirpdb.database.windows.net,1433;Initial Catalog=chirpdb;Persist Security Info=False;User ID=chirpadmin;Password=E1k2m3m4v5;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        base.OnConfiguring(optionsBuilder);

    }
}