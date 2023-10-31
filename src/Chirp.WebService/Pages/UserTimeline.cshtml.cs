using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBContext;
using Chirp.Models;
using Chirp.Interfaces;
using Chirp.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly DatabaseContext _context;
    public List<Cheep> Cheeps { get; set; } = null!;
    private ICheepRepository cheepRepository;
    public int cheepsPerPage;
    public int totalCheeps;
    public UserTimelineModel(DatabaseContext context)
    {
        _context = context;
        cheepRepository = new CheepRepository(context);
        cheepsPerPage = cheepRepository.CheepsPerPage();
    }

    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        IEnumerable<Cheep> cheeps = await cheepRepository.GetCheepsFromAuthor(author, page);
        Cheeps = cheeps.ToList();

        IEnumerable<Cheep> allCheeps = await cheepRepository.GetAllCheepsFromAuthor(author);
        totalCheeps = allCheeps.Count();

        return Page();
    }
}
