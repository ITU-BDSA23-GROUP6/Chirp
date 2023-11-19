using Chirp.Models;
using Chirp.ADTO;
using Chirp.FDTO;

namespace Chirp.Interfaces;

public interface IAuthorRepository
{
    public Task<Author> GetAuthorByName(string name); 
    public Task<IEnumerable<Author>> GetAllAuthors();
    public Task<IEnumerable<Author>> GetAuthorFollowing(string authorName);
    public Task<IEnumerable<Author>> GetAuthorFollowers(string authorName);
    public Task Follow(FollowersDTO followersDTO);
    public Task Unfollow(FollowersDTO followersDTO);
}