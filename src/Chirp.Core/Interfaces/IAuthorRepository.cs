using Chirp.Models;
using Chirp.ADTO;
using Chirp.FDTO;

namespace Chirp.Interfaces;

public interface IAuthorRepository
{
    public Task<Author> GetAuthorByName(string name); 
    public Task<IEnumerable<Author>> GetAllAuthorsWithFollowers();
    public Task Follow(FollowersDTO followersDTO);
    public Task Unfollow(FollowersDTO followersDTO);
}