using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using DBContext;
using Chirp.FDTO;
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
    public Author SignedInUser { get; set; } = null!;
    public Author TimelineUser { get; set; } = null!;
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
            // 00. Variables:
            string? signedInUser = User.Identity?.Name;

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

            // 05. Signed In User:
            if(_signInManager.IsSignedIn(User) && signedInUser != null && author != signedInUser)
            {
                TimelineUser = await _authorRepository.GetAuthorByName(author);
                SignedInUser = await _authorRepository.GetAuthorByName(signedInUser);
                _logger.LogInformation("[USERTIMELINE] User was signed in");
            }
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.StackTrace;
            return RedirectToPage("/Error");
        }

        return Page();
    }

    [BindProperty]
    public bool IsFollow { get; set; }
    [BindProperty]
    public string TargetAuthorUserName { get; set; }
    public async Task<IActionResult> OnPostFollow([FromQuery] int? page = 0)
    {
        ModelState.Clear();

        try
        {
            if(ModelState.IsValid) {
                if(_signInManager.IsSignedIn(User))
                {
                    FollowersDTO followersDTO = new(User.Identity.Name, TargetAuthorUserName);  // [TODO] Remove warning but we still want it to be caught by exception.

                    if(IsFollow) 
                    {
                        await _authorRepository.Follow(followersDTO);
                    }
                    else
                    {                        
                        await _authorRepository.Unfollow(followersDTO);
                    }
                } 
                else if(SignedInUser == null)
                {
                    throw new Exception("[USER-TIME-LINE.CSHTML.CS] The 'SignedInUser' variable was NULL");
                }

                return RedirectToPage("UserTimeline", new { page });
            } 
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Error");
        }

        return RedirectToPage("UserTimeline", new { page });
    }

    // [TODO] Change this and clean up. Simplist solution - but ugly af.
    public async Task<IEnumerable<Cheep>> GetTop4CheepsFromFollower(string author)
    {
        return await _cheepRepository.GetTop4FromAuthor(author);
    }
}
