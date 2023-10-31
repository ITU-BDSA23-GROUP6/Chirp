using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly DatabaseContext context;

    public CheepRepository(DatabaseContext _context)
    {
        context = _context;
    }

    public int CheepsPerPage() 
    {
        return 32;
    }

    public void CreateCheep(Cheep cheep)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Cheep>> GetCheeps(int page)
    {
        return await context.Cheeps
            .Include(cheep => cheep.Author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .Skip(page * CheepsPerPage())
            .Take(CheepsPerPage())
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Cheep>> GetAllCheeps()
    {
        return await context.Cheeps
            .Include(cheep => cheep.Author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cheep>> GetCheepsFromAuthor(string author, int page)
    {
        return await context.Cheeps
            .Include(cheep => cheep.Author)
            .Where(cheep => cheep.Author.Name == author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .Skip(page * CheepsPerPage())
            .Take(CheepsPerPage())
            .ToListAsync();
    }

    public async Task<IEnumerable<Cheep>> GetAllCheepsFromAuthor(string author)
    {
        return await context.Cheeps
            .Include(cheep => cheep.Author)
            .Where(cheep => cheep.Author.Name == author)
            .OrderByDescending(cheep => cheep.TimeStamp)
            .ToListAsync();
    }
}