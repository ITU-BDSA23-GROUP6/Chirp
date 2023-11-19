using Chirp.Interfaces;
using Chirp.Models;
using DBContext;
using Chirp.ADTO;
using Microsoft.EntityFrameworkCore;
using Chirp.FDTO;
using Microsoft.Extensions.Logging;

namespace Chirp.Infrastructure;

public class AuthorRepository : IAuthorRepository
{
    private readonly ILogger<AuthorRepository> _logger;
    private readonly DatabaseContext _context;

    public AuthorRepository(ILogger<AuthorRepository> logger, DatabaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<Author>> GetAllAuthors()
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

    public async Task Follow(FollowersDTO followersDTO)
    {
        try
        {
            var targetAuthor = await GetAuthorByName(followersDTO.TargetAuthor);
            var authorToFollow = await GetAuthorByName(followersDTO.FollowAuthor);

            _logger.LogInformation($"[FOLLOW - BEFORE] '{targetAuthor.UserName}' is Following {targetAuthor.Following.Count} accounts");
            if(!targetAuthor.Following.Contains(authorToFollow)) // additional check that !(authorToFollow.Followers.Contains(targetAuthor))
            {
                _logger.LogInformation($"[FOLLOW] Author {targetAuthor.UserName} does not contain {authorToFollow.UserName}");
                
                targetAuthor.Following.Add(authorToFollow);
                authorToFollow.Followers.Add(targetAuthor);
            } 
            else
            {
                _logger.LogInformation($"[FOLLOW] Author {targetAuthor.UserName} already follows {authorToFollow.UserName}");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"[FOLLOW - AFTER] '{targetAuthor.UserName}' is Following {targetAuthor.Following.Count} accounts");
        } 
        catch(Exception ex)
        {
            throw new Exception($"Exception: {ex.Message}");
        }
    }

    public async Task Unfollow(FollowersDTO followersDTO)
    {
        try
        {  
            _logger.LogInformation($"[UNFOLLOW] Initiated 'Unfollow' --- DTO: ({followersDTO.TargetAuthor}, {followersDTO.FollowAuthor})");
            var targetAuthor = await GetAuthorByName(followersDTO.TargetAuthor);
            var authorToUnfollow = await GetAuthorByName(followersDTO.FollowAuthor);

            _logger.LogInformation($"[UNFOLLOW - BEFORE] '{targetAuthor.UserName}' is Following {targetAuthor.Following.Count} accounts");
            if(targetAuthor.Following.Contains(authorToUnfollow))
            {
                _logger.LogInformation($"[UNFOLLOW] Author {targetAuthor.UserName} does contain {authorToUnfollow.UserName}");
                targetAuthor.Following.Remove(authorToUnfollow);
                authorToUnfollow.Followers.Remove(targetAuthor);
            } 
            else
            {
                _logger.LogInformation($"[UNFOLLOW] Author {targetAuthor.UserName} does not contain the intended target to remove");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"[UNFOLLOW - AFTER] '{targetAuthor.UserName}' is Following {targetAuthor.Following.Count} accounts");
        } 
        catch(Exception ex)
        {
            throw new Exception($"Exception: {ex.Message}");
        }
    }


    public async Task<IEnumerable<Author>> GetAuthorFollowers(string authorName)
    {
        var author = await GetAuthorByName(authorName);
        return author.Followers.ToList();
    }

    public async Task<IEnumerable<Author>> GetAuthorFollowing(string authorName)
    {
        var author = await GetAuthorByName(authorName);
        return author.Following.ToList();
    }
}
