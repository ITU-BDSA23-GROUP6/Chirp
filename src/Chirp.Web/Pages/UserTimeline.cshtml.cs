using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using DBContext;
using Chirp.Models;
using Chirp.Interfaces;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;
    private readonly ILogger<UserTimelineModel> _logger;
    public List<Cheep> Cheeps { get; set; } = null!;
    public List<Author> Followers { get; set; } = null!;
    public List<Author> Following { get; set; } = null!;
    public int cheepsPerPage;
    public int totalCheeps;
    public UserTimelineModel(UserManager<Author> userManager, SignInManager<Author> signInManager, IAuthorRepository authorRepository, ICheepRepository cheepRepository, ILogger<UserTimelineModel> logger)
    {
        _logger = logger;

        _userManager = userManager;
        _signInManager = signInManager;   
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;

        cheepsPerPage = _cheepRepository.CheepsPerPage();
    }

    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        try
        {
            // 01. Cheeps:
            IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheepsFromAuthor(author, page);
            Cheeps = cheeps.ToList();

            // 02. All the Authors Cheeps - [TODO] Change this to be more efficient, no need to get all Cheeps everytime:
            IEnumerable<Cheep> allCheeps = await _cheepRepository.GetAllCheepsFromAuthor(author);
            totalCheeps = allCheeps.Count();

            // 03. Followers
            IEnumerable<Author> followers = await _authorRepository.GetAuthorFollowers(author);
            Followers = followers.ToList();

            // 04. Following:
            IEnumerable<Author> following = await _authorRepository.GetAuthorFollowing(author);
            Following = following.ToList();
        }
        catch(Exception ex)
        {
            throw new Exception($"Exception: {ex.Message}");    // Propogate the exception
        }

        return Page();
    }

    // [TODO] Change this and clean up. Simplist solution - but ugly af.
    public async Task<IEnumerable<Cheep>> GetTop4CheepsFromFollower(string author)
    {
        return await _cheepRepository.GetTop4FromAuthor(author);
    }
}
