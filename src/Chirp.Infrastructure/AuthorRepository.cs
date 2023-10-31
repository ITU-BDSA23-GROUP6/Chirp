using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
namespace Chirp.Infrastructure;

public class AuthorRepository : IAuthorRepository
{
    private readonly DatabaseContext db;
    private int cheepsPerPage = 32;

    public AuthorRepository(DatabaseContext curDB)
    {
        db = curDB;
    }

    public void CreateNewAuthor(Author author)
    {
        throw new NotImplementedException();
    }

    public Task<Author> GetAuthorFromName(string name)
    {
        throw new NotImplementedException();
    }
}
