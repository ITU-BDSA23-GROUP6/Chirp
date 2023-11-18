using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
using Chirp.ADTO;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class AuthorRepository : IAuthorRepository
{
    private readonly DatabaseContext _context;

    public AuthorRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Author>> GetAllAuthorsWithFollowers()
    {
        return await _context.Authors
            .Include(a => a.Following)
            .Include(a => a.Followers)
            .ToListAsync();
    }

    public async Task<Author> GetAuthorByName(string authorName)
    {
        return await _context.Authors
            .Include(a => a.Following)
            .Include(a => a.Followers)
            .Where(a => a.UserName == authorName)
            .FirstOrDefaultAsync() ?? throw new Exception("Author could not be located");   // [TODO] Handle exception
    }

    public void Follow(Author targetAuthor, Author authorToFollow)
    {
        if(!targetAuthor.Followers.Contains(authorToFollow))
        {
            targetAuthor.Followers.Add(authorToFollow);
        }
        else
        {
            //... Add this ---> [ModelState.AddModelError(string.Empty, "...");]
        }

    }

    public void Unfollow(Author targetAuthor, Author authorToUnfollow)
    {
        if(targetAuthor.Followers.Contains(authorToUnfollow))
        {
            targetAuthor.Followers.Remove(authorToUnfollow);
        }
        else
        {
            //... Add this ---> [ModelState.AddModelError(string.Empty, "...");]
        }
    }

}
