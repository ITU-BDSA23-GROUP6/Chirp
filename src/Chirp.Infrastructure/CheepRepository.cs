using Microsoft.EntityFrameworkCore;

using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
using Chirp.CDTO;
using Chirp.ADTO;
using Microsoft.Extensions.Logging;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly ILogger<CheepRepository> _logger;
    private readonly DatabaseContext _context;
    private readonly IAuthorRepository _authorRepository;

    public CheepRepository(DatabaseContext context, IAuthorRepository authorRepository, ILogger<CheepRepository> logger)
    {
        _logger = logger;
        _context = context;
        _authorRepository = authorRepository;
    }

    public int CheepsPerPage()
    {
        return 32;
    }

    public async Task<IEnumerable<Cheep>> GetCheeps(int page)
    {
        return await _context.Cheeps
            .Include(cheep => cheep.Author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .Skip(page * CheepsPerPage())
            .Take(CheepsPerPage())
            .ToListAsync();
    }

    public async Task<IEnumerable<Cheep>> GetAllCheeps()
    {
        return await _context.Cheeps
            .Include(cheep => cheep.Author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cheep>> GetCheepsFromAuthor(string author, int page)
    {
        return await _context.Cheeps
            .Include(cheep => cheep.Author)
            .Where(cheep => cheep.Author.UserName == author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .Skip(page * CheepsPerPage())
            .Take(CheepsPerPage())
            .ToListAsync();
    }

    public async Task<IEnumerable<Cheep>> GetAllCheepsFromAuthor(string author)
    {
        return await _context.Cheeps
            .Include(cheep => cheep.Author)
            .Where(cheep => cheep.Author.UserName == author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .ToListAsync();
    }

    public async Task CreateCheep(CheepDTO cheepDTO)
    {
        try
        {
            var author = await _authorRepository.GetAuthorByName(cheepDTO.Author) ?? throw new Exception("Author was NULL");

            _logger.LogInformation($"[POST] Located author is {author.UserName}");

            Cheep newCheep = new()
            {
                Author = author,
                Text = cheepDTO.Text,
                TimeStamp = DateTime.UtcNow,
            };

            if (author.Cheeps == null)
            {
                author.Cheeps = new List<Cheep>();
                _logger.LogInformation("Authors Cheeps was null");
            }

            author.Cheeps.Add(newCheep);
            _context.Cheeps.Add(newCheep);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"[POST] Error Occurred: {ex.Message}");
        }
    }
}


/*    try
            {
                chirpContext.Database.OpenConnection(); // Open the connection to the database

                Console.WriteLine("Opening conncetion to Database");


                //chirpContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Cheeps] ON");

                int numEntriesWritten = chirpContext.SaveChanges();

                Console.WriteLine("numEntriesWritten: " + numEntriesWritten + ". And saved changes");

                //chirpContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Cheeps] OFF");

                if (numEntriesWritten > 0)
                {
                    Console.WriteLine("Changes successfully written to Azure SQL Database");
                    return;
                }
                else
                {
                    Console.WriteLine("Data base already seeded with data");
                    return;
                }
            }
            finally
            {
                chirpContext.Database.CloseConnection(); // Close the database connection
            }
            */