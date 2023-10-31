using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBContext;
using Chirp.Models;
using Microsoft.EntityFrameworkCore;
using Chirp.Interfaces;
using Chirp.Infrastructure;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private ICheepRepository cheepRepository;
    public List<Cheep> Cheeps { get; set; } = null!;
    public int totalCheeps;
    public int cheepsPerPage;

    public PublicModel(DatabaseContext context)
    {
        cheepRepository = new CheepRepository(context);
        cheepsPerPage = cheepRepository.CheepsPerPage();
    }

    public async Task<IActionResult> OnGet([FromQuery] int page)
    {
        IEnumerable<Cheep> cheeps = await cheepRepository.GetCheeps(page);
        Cheeps = cheeps.ToList();

        IEnumerable<Cheep> allCheeps = await cheepRepository.GetAllCheeps();
        totalCheeps = allCheeps.Count();

        return Page();
    }
}
