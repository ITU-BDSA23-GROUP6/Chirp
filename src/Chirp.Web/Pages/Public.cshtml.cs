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
    private readonly UserManager<Author> _userManager;
    
    public List<Cheep> Cheeps { get; set; } = null!;
    public int totalCheeps;
    public int cheepsPerPage;
    public int PageNumber { get; set; }

    public PublicModel(ICheepRepository cheepRepository, UserManager<Author> userManager, ILogger<PublicModel> logger)
    {
        _cheepRepository = cheepRepository;
        _userManager = userManager;
        _logger = logger;

        cheepsPerPage = cheepRepository.CheepsPerPage();
    }

    [BindProperty, Required(ErrorMessage="Cheep must be between 1-to-160 characters"), StringLength(160, MinimumLength = 1)]
    public string CheepText { get; set; } = "";

    public async Task<IActionResult> OnGet([FromQuery] int page)
    {   
        IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheeps(page);
        Cheeps = cheeps.ToList();

        IEnumerable<Cheep> allCheeps = await _cheepRepository.GetAllCheeps();
        totalCheeps = allCheeps.Count();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromQuery] int? page) 
    {
        try
        {
            if(ModelState.IsValid) {
                var currUser = await _userManager.GetUserAsync(User) ?? throw new Exception("ERROR: User could not be found");

                CheepDTO cheepDTO = new(CheepText, currUser.UserName);

                await _cheepRepository.CreateCheep(cheepDTO);

                int pageNumber = page ?? 1;

                return RedirectToPage("Public", new { page = pageNumber });
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
