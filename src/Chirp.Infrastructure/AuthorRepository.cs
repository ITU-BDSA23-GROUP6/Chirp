using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
namespace Chirp.Infrastructure;

public class AuthorRepository : IAuthorRepository
{
    private readonly DatabaseContext context;

    public AuthorRepository(DatabaseContext _context)
    {
        context = _context;
    }

    public void CreateNewAuthor(Author author)
    {
        throw new NotImplementedException();
    }

    public Task<Author> GetAuthor(string name)
    {
        throw new NotImplementedException();
    }
}
