using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Chirp.Models;
using Chirp.Interfaces;
using Chirp.CDTO;
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
    public int PageNumber { get; set; }

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
        int pageNumber = page ?? 0;
        if (pageNumber > 0) pageNumber--;
        
        IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheeps(pageNumber);
        Cheeps = cheeps.ToList();

        IEnumerable<Cheep> allCheeps = await _cheepRepository.GetAllCheeps();
        totalCheeps = allCheeps.Count();

        /*
            @ The following process uses Eager Loading.
            @ This is a technique used when dealing with having to include values which are located in
              separate Tables.
        
        */
        if(_signInManager.IsSignedIn(User))
        {
            _logger.LogInformation($"[PUBLIC] User: {User.Identity?.Name} is logged in");

            try
            {
                SignedInAuthor = await _authorRepository.GetAuthorByName(User.Identity?.Name);

                _logger.LogInformation("[PUBLIC] SignedInAuthor assigned a value");
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"[EXCEPTION] {ex.Message}");
            }
        }


        return Page();
    }

    public async Task<IActionResult> OnPostCheep([FromQuery] int? page = 1) 
    {
        try
        {
            if(ModelState.IsValid) {
                var currUser = await _userManager.GetUserAsync(User) ?? throw new Exception("ERROR: User could not be found");

                CheepDTO cheepDTO = new(CheepText, currUser.UserName);

                await _cheepRepository.CreateCheep(cheepDTO);

                int pageNumber = page ?? 1; // [TODO] Change to zero

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

    public async Task<IActionResult> OnPostFollow([FromQuery] int? page = 0)
    {
        try
        {
            if(ModelState.IsValid) {
                if(SignedInAuthor != null)
                {
                    _logger.LogInformation("[FOLLOW/UNFOLLOW] Post made!");
                }


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
}
