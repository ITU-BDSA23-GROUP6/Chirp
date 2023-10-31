using Chirp.Interfaces;
namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly DatabaseContext db;
    private int cheepsPerPage = 32;

    public CheepRepository(DatabaseContext curDB)
    {
        db = curDB;
    }

    
}