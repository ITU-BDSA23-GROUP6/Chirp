using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Chirp.Models;
using Chirp.Interfaces;
using Chirp.CDTO;
using Chirp.FDTO;
using Enums.ACO;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Chirp.ODTO;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    // 01. Repositories and Managers:
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ILikeDisRepository _likeDisRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;
    private readonly ILogger<PublicModel> _logger;

    // 02. Variables:
    public List<Cheep> Cheeps { get; set; } = null!;
    public Author SignedInAuthor { get; set; } = null!;
    public Dictionary<int, CheepOpinionDTO> CheepOpinionsInfo { get; set; }
    public int TotalCheeps, CheepsPerPage;
    public string UserName { get; set; }

    // 03. Bind properties:
    [BindProperty, Required(ErrorMessage="Cheep must be between 1-to-160 characters"), StringLength(160, MinimumLength = 1)]
    public string CheepText { get; set; } = "";
    [BindProperty(SupportsGet = true)]
    public bool IsFollow { get; set; } = false;
    [BindProperty(SupportsGet = true)]
    public string TargetAuthorUserName { get; set; } = null!;
    [BindProperty]
    public int TargetCheepId { get; set; }

    public PublicModel(
        ILogger<PublicModel> logger, 
        ILikeDisRepository likeDisRepository,
        IAuthorRepository authorRepository, 
        ICheepRepository cheepRepository, 
        UserManager<Author> userManager, 
        SignInManager<Author> signInManager)
    {
        _logger = logger;
        _likeDisRepository = likeDisRepository;
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _userManager = userManager;
        _signInManager = signInManager;

        CheepsPerPage = cheepRepository.CheepsPerPage();
    }

    public async Task<IActionResult> OnGet([FromQuery] int? page = 0)
    {   
        int pgNum = page ?? 0;
        
        try
        {
            await GetCheepInformation(pgNum, "timestamp");
        } 
        catch(Exception ex)
        {
            string exceptionInfo = $"File: Public.cshtml.cs \n\n Method: 'OnGet()' \n\n Message: {ex.Message} \n\n Stack Trace: {ex.StackTrace}";
            TempData["ErrorMessage"] = exceptionInfo;
            return RedirectToPage("/Error");
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostCheep([FromQuery] int? page = 0) 
    {
        ModelState.Clear();
        
        int pgNum = page ?? 0;

        try
        {
            if(ModelState.IsValid) {
                var currUser = await _userManager.GetUserAsync(User) ?? throw new Exception("ERROR: User could not be found");

                CheepDTO cheepDTO = new(CheepText, currUser.UserName);  // [TODO] Change to User.Identity.?.Name;

                await _cheepRepository.CreateCheep(cheepDTO);

                return RedirectToPage("Public", new { pgNum });
            }
            else if(!ModelState.IsValid)
            {
                //... iterate over model states
            }
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Error");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostFollow([FromQuery] int? page = 0, [FromQuery] string? orderBy = null)
    {
        ModelState.Clear();

        int pgNum = page ?? 0;
        string orderByVal = orderBy ?? "timestamp";

        try
        {
            if(ModelState.IsValid) {
                if(_signInManager.IsSignedIn(User))
                {
                    FollowersDTO followersDTO = new(User.Identity.Name, TargetAuthorUserName);

                    if(IsFollow) 
                    {
                        await _authorRepository.Follow(followersDTO);
                    }
                    else
                    {                        
                        await _authorRepository.Unfollow(followersDTO);
                    }
                } 

                await GetCheepInformation(pgNum, orderByVal);

                return new PartialViewResult {
                    ViewName = "./Shared/Partials/_PublicCheepPartial",
                    ViewData = new ViewDataDictionary<PublicModel>(ViewData)
                    {
                        Model = this
                    }
                };
            } 
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/Error");
        }

        return RedirectToPage("Public", new { page });  // [TODO]: Add Alter - NOT Exception
    }

    public async Task<IActionResult> OnPostDislikeOrLike([FromQuery] int? page = 0, [FromQuery] string? opinion = null, [FromQuery] string? orderBy = null)
    {
        ModelState.Clear();

        int pgNum = page ?? 0;
        string orderByVal = orderBy ?? "timestamp";

        try
        {
            if(ModelState.IsValid) {
                if(_signInManager.IsSignedIn(User))
                {
                    var likeDislikeValue = opinion;
                    if(string.IsNullOrEmpty(likeDislikeValue)) { throw new Exception("File: 'Public.cshtml.cs' - Method: 'OnPostDislikeOrLike()' - Message: Value retrieved from Request Form was NULL"); }

                    if(likeDislikeValue == "like")
                    {
                        await _cheepRepository.GiveOpinionOfCheep(true, TargetCheepId, TargetAuthorUserName);
                    }
                    else if(likeDislikeValue == "dislike")
                    {
                        await _cheepRepository.GiveOpinionOfCheep(false, TargetCheepId, TargetAuthorUserName);
                    } 
                }

                await GetCheepInformation(pgNum, orderByVal);

                return new PartialViewResult {
                    ViewName = "./Shared/Partials/_PublicCheepPartial",
                    ViewData = new ViewDataDictionary<PublicModel>(ViewData)
                    {
                        Model = this
                    }
                };
            } 
            else if(!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "File: 'Public.cshtml.cs' - Method: 'OnPostDislikeOrLike()' - Message: ModelState was Invalid";
                return RedirectToPage("/Error");
            }

            return RedirectToPage("Public", new { page });  // [TODO] // [TODO]: Add Alter - NOT Exception
        }
        catch(Exception ex)
        {
            string exceptionInfo = $"File: Public.cshtml.cs - Method: 'OnPostDislikeOrLike()' - Message: {ex.Message} - Stack Trace: {ex.StackTrace}";
            TempData["ErrorMessage"] = exceptionInfo;
            return RedirectToPage("/Error");
        }

        return RedirectToPage("Public", new { page });
    }

    public async Task<IActionResult> OnGetOrderCheepsBy([FromQuery] int? page = 0, [FromQuery] string? orderBy = "timestamp")
    {
        _logger.LogInformation("OnGetOrderCheepsBy() method has been called");

        int pgNum = page ?? 0;
        string orderByVal = orderBy ?? "timestamp";

        try
        {
            await GetCheepInformation(pgNum, orderByVal);

            return new PartialViewResult {
                ViewName = "./Shared/Partials/_PublicCheepPartial",
                ViewData = new ViewDataDictionary<PublicModel>(ViewData)
                {
                    Model = this
                }
            };
        } 
        catch(Exception ex)
        {
            string exceptionInfo = $"File: Public.cshtml.cs \n\n Method: 'OnGet()' \n\n Message: {ex.Message} \n\n Stack Trace: {ex.StackTrace}";
            TempData["ErrorMessage"] = exceptionInfo;
            return RedirectToPage("/Error");
        }
    }


    public async Task GetCheepInformation(int pgNum = 0, string orderByVal = "timestamp")
    {
        try
        {
            // 01. Retrieve the Cheeps:
            IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheeps(pgNum, orderByVal);
            Cheeps = cheeps.ToList();

            // 02. Retrieve the Authors Opinions AND-OR Likes and Dislikes:
            CheepOpinionsInfo = new Dictionary<int, CheepOpinionDTO>();
            
            // 03. Get the total number of Cheeps [used to calculate pagination]:
            TotalCheeps = await _cheepRepository.GetTotalNumberOfCheeps();

            // 04. Determine whether or not a User is signed in:
            bool IsUserSignedIn = _signInManager.IsSignedIn(User);

            if(IsUserSignedIn)
            {
                UserName = User.Identity?.Name;
                SignedInAuthor = await _authorRepository.GetAuthorByName(UserName);

                if(Cheeps.Any())
                {
                    foreach (Cheep cheep in Cheeps)
                    {
                        CheepOpinionDTO co_Info = await _likeDisRepository.GetAuthorCheepOpinion(cheep.CheepId, SignedInAuthor.UserName);
                        CheepOpinionsInfo.Add(cheep.CheepId, co_Info);
                    }
                }
            }
            else if(!IsUserSignedIn)
            {
                foreach(Cheep cheep in Cheeps)
                {
                    CheepOpinionDTO LikesAndDislikes = await _likeDisRepository.GetCheepLikesAndDislikes(cheep.CheepId);
                    CheepOpinionsInfo.Add(cheep.CheepId, LikesAndDislikes);
                }
            }
        } 
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
