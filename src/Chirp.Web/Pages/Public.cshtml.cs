using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Chirp.Models;
using Chirp.Interfaces;
using Chirp.CDTO;
using Chirp.FDTO;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ILogger<PublicModel> _logger;
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public List<Cheep> Cheeps { get; set; } = null!;
    public Author SignedInAuthor { get; set; } = null!;
    public int totalCheeps;
    public int cheepsPerPage;

    public PublicModel(IAuthorRepository authorRepository, ICheepRepository cheepRepository, UserManager<Author> userManager, SignInManager<Author> signInManager, ILogger<PublicModel> logger)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;

        cheepsPerPage = cheepRepository.CheepsPerPage();
    }

    [BindProperty, Required(ErrorMessage="Cheep must be between 1-to-160 characters"), StringLength(160, MinimumLength = 1)]
    public string CheepText { get; set; } = "";

    public async Task<IActionResult> OnGet([FromQuery] int? page)
    {   
        int pgNum = page ?? 0;
        
        IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheeps(pgNum);
        Cheeps = cheeps.ToList();

        IEnumerable<Cheep> allCheeps = await _cheepRepository.GetAllCheeps();
        totalCheeps = allCheeps.Count();

        if(_signInManager.IsSignedIn(User))
        {
            try
            {
                SignedInAuthor = await _authorRepository.GetAuthorByName(User.Identity?.Name);
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCheep([FromQuery] int? page = 0) 
    {
        try
        {
            if(ModelState.IsValid) {
                var currUser = await _userManager.GetUserAsync(User) ?? throw new Exception("ERROR: User could not be found");

                CheepDTO cheepDTO = new(CheepText, currUser.UserName);  // [TODO] Change to User.Identity.?.Name;

                await _cheepRepository.CreateCheep(cheepDTO);

                return RedirectToPage("Public", new { page });
            }
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Error");
        }

        return Page();
    }

    [BindProperty]
    public bool IsFollow { get; set; } = false;
    [BindProperty]
    public string TargetAuthorUserName { get; set; } = null!;

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
                else if(SignedInAuthor == null)
                {
                    throw new Exception("[PUBLIC.CSHTML.CS] The 'SignedInAuthor' variable was NULL");
                }

                return RedirectToPage("Public", new { page });
            } 
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Error");
        }

        return RedirectToPage("Public", new { page });
    }
}
