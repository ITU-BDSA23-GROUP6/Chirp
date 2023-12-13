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
    [BindProperty]
    public bool IsFollow { get; set; } = false;
    [BindProperty]
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
            IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheeps(pgNum, "");
            Cheeps = cheeps.ToList();

            TotalCheeps = await _cheepRepository.GetTotalNumberOfCheeps();
            
            await GetCheepInformation();
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

    public async Task<IActionResult> OnPostFollow([FromQuery] int? page = 0)
    {
        ModelState.Clear();

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

    public async Task<IActionResult> OnPostDislikeOrLike([FromQuery] int? page = 0, [FromQuery] string? opinion = null)
    {
        ModelState.Clear();

        try
        {
            if(ModelState.IsValid) {
                _logger.LogInformation($"[OnPostDislikeOrLike] Value for CheepId {TargetCheepId} --- Value for Author: {TargetAuthorUserName}");

                if(_signInManager.IsSignedIn(User))
                {
                    var likeDislikeValue = opinion;
                    if(string.IsNullOrEmpty(likeDislikeValue)) { throw new Exception("File: 'Public.cshtml.cs' - Method: 'OnPostDislikeOrLike()' - Message: Value retrieved from Request Form was NULL"); }

                    if(likeDislikeValue == "like")
                    {
                        _logger.LogInformation("[OnPostDislikeOrLike] Entered Like");
                        await _cheepRepository.GiveOpinionOfCheep(true, TargetCheepId, TargetAuthorUserName);
                    }
                    else if(likeDislikeValue == "dislike")
                    {
                        _logger.LogInformation("[OnPostDislikeOrLike] Entered Dislike");
                        await _cheepRepository.GiveOpinionOfCheep(false, TargetCheepId, TargetAuthorUserName);
                    } else
                    {
                        return RedirectToPage("Public", new { page });  // [TODO] Add alert trigger: 'failed'
                    }
                }

                _logger.LogInformation("[OnPostDislikeOrLike] Success! Variables status");
                _logger.LogInformation($"[OnPostDislikeOrLike] Cheeps: {Cheeps.Count} --- SignedInAuthor: {SignedInAuthor.UserName}");

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

            // 01. Get Cheeps in a specified order:
            var cheeps = await _cheepRepository.GetCheeps(pgNum, orderByVal);
            Cheeps = cheeps.ToList();
            
            // 02. Get the total number of Cheeps [used for Pagination]:
            TotalCheeps = await _cheepRepository.GetTotalNumberOfCheeps();

            // 03. Get Author Opinion of Cheeps and or Likes-&-Dislikes:
            await GetCheepInformation();

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


    public async Task GetCheepInformation()
    {
        CheepOpinionsInfo = new Dictionary<int, CheepOpinionDTO>();
        bool IsUserSignedIn = _signInManager.IsSignedIn(User);

        try
        {
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
