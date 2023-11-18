using Chirp.Models;
using Chirp.ADTO;

namespace Chirp.Interfaces;

public interface IAuthorRepository
{
    public Task<Author> GetAuthorByName(string name); 
    public Task<IEnumerable<Author>> GetAllAuthorsWithFollowers();
    public void Follow(Author targetAuthor, Author authorToFollow);
    public void Unfollow(Author targetAuthor, Author authorToUnfollow);
}