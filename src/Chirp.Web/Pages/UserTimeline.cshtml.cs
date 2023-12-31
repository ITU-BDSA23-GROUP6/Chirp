﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using DBContext;
using Chirp.FDTO;
using Chirp.Models;
using Chirp.Interfaces;
using Chirp.ODTO;
using Chirp.Infrastructure;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    // 01. Services & Repositories:
    private readonly ICheepRepository _cheepRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ILikeDisRepository _likeDisRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;
    private readonly ILogger<UserTimelineModel> _logger;

    // 02. Variables:
    public Dictionary<int, CheepOpinionDTO> CheepOpinionsInfo { get; set; }
    public Dictionary<Author, List<Cheep>> FollowersAndTheirCheeps { get; set; } = null!;
    public List<Cheep> TLU_Cheeps { get; set; } = null!;
    public List<Author> Followers { get; set; } = null!;
    public List<Author> Following { get; set; } = null!;
    public Author SignedInUser { get; set; } = null!;
    public Author TimelineUser { get; set; } = null!;
    public int CheepsPerPage;
    public int TotalCheeps;

    // 03. Bind Properties:
    [BindProperty]
    public bool IsFollow { get; set; }
    [BindProperty]
    public string TargetAuthorUserName { get; set; } = null!;
    [BindProperty]
    public int TargetCheepId { get; set; }

    public UserTimelineModel(
        UserManager<Author> userManager, 
        SignInManager<Author> signInManager, 
        IAuthorRepository authorRepository, 
        ICheepRepository cheepRepository, 
        ILikeDisRepository likeDisRepository, 
        ILogger<UserTimelineModel> logger
        )
    {
        _logger = logger;

        _userManager = userManager;
        _signInManager = signInManager;   
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _likeDisRepository = likeDisRepository;

        CheepsPerPage = _cheepRepository.CheepsPerPage();
    }

    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        ModelState.Clear();

        try
        {
            // 00. Variables:
            string? signedInUser = User.Identity?.Name;

            // 01. Cheeps:
            IEnumerable<Cheep> cheeps = await _cheepRepository.GetCheepsFromAuthor(author, page);
            TLU_Cheeps = cheeps.ToList();

            // 02. All the Authors Cheeps:
            TotalCheeps = await _cheepRepository.GetTotalNumberOfAuthorCheeps(author);

            // 03. Get the TLU's Followers:
            IEnumerable<Author> followers = await _authorRepository.GetAuthorFollowers(author);
            Followers = followers.ToList();

            // 04. Get the Authors the TLU is Following:
            IEnumerable<Author> following = await _authorRepository.GetAuthorFollowing(author);
            Following = following.ToList(); 

            // 05. Instantiate containers for our 
            FollowersAndTheirCheeps = new Dictionary<Author, List<Cheep>>();
            CheepOpinionsInfo       = new Dictionary<int, CheepOpinionDTO>();

            // 06. Retrieve info on Signed-In User (if any) and the Timeline User.
            bool IsUserSignedIn = _signInManager.IsSignedIn(User);
            TimelineUser        = await _authorRepository.GetAuthorByName(author);
            if(IsUserSignedIn) { SignedInUser = await _authorRepository.GetAuthorByName(signedInUser); }

            // 07. Stats on User Timeline User Cheeps: 
            if(TLU_Cheeps.Any())
            {
                if(IsUserSignedIn && author != signedInUser)
                {
                    foreach (Cheep cheep in TLU_Cheeps)
                    {
                        CheepOpinionDTO co_Info = await _likeDisRepository.GetAuthorCheepOpinion(cheep.CheepId, SignedInUser.UserName);
                        CheepOpinionsInfo.Add(cheep.CheepId, co_Info);
                    }
                } 
                else 
                {
                    foreach (Cheep cheep in TLU_Cheeps)
                    {
                        CheepOpinionDTO co_CheepLikesAndDislikes = await _likeDisRepository.GetCheepLikesAndDislikes(cheep.CheepId);
                        CheepOpinionsInfo.Add(cheep.CheepId, co_CheepLikesAndDislikes);
                    }
                }
            }

            // 06. Get information on the Timeline User's Followers:
            if(Following.Any())
            {
                foreach (Author follower in Following)
                {
                    var followerCheeps = await _cheepRepository.GetTop4FromAuthor(follower.UserName);
                    if(!FollowersAndTheirCheeps.ContainsKey(follower)) FollowersAndTheirCheeps.Add(follower, followerCheeps.ToList());

                    foreach (Cheep followerCheep in followerCheeps)
                    {
                        if(IsUserSignedIn)
                        {
                            CheepOpinionDTO co_Info = await _likeDisRepository.GetAuthorCheepOpinion(followerCheep.CheepId, follower.UserName);
                            if(!CheepOpinionsInfo.ContainsKey(followerCheep.CheepId)) CheepOpinionsInfo.Add(followerCheep.CheepId, co_Info);
                        }
                        else
                        {
                            CheepOpinionDTO co_CheepLikesAndDislikes = await _likeDisRepository.GetCheepLikesAndDislikes(followerCheep.CheepId);
                            if(!CheepOpinionsInfo.ContainsKey(followerCheep.CheepId)) CheepOpinionsInfo.Add(followerCheep.CheepId, co_CheepLikesAndDislikes);
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            string errorMessage = $"File: 'UserTimeline.cshtml.cs' - Method: 'OnGet' - Message: {ex.Message} - Stack Trace: {ex.StackTrace}";
            TempData["ErrorMessage"] = errorMessage;
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

    public async Task<IActionResult> OnPostDislikeOrLike([FromQuery] int? page = 0)
    {
        ModelState.Clear();

        try
        {
            if(ModelState.IsValid) {
                if(_signInManager.IsSignedIn(User))
                {   
                    var likeDislikeValue = Request.Form["likeDis"];
                    if(string.IsNullOrEmpty(likeDislikeValue)) throw new Exception("File: 'Public.cshtml.cs' - Method: 'OnPostDislikeOrLike()' - Message: Value retrieved from Request Form was NULL");

                    if(likeDislikeValue == "like")
                    {
                        await _cheepRepository.GiveOpinionOfCheep(true, TargetCheepId, TargetAuthorUserName);
                    }
                    else if(likeDislikeValue == "dislike")
                    {
                        await _cheepRepository.GiveOpinionOfCheep(false, TargetCheepId, TargetAuthorUserName);
                    }
                } 

                return RedirectToPage("Public", new { page });
            } 
            else if(!ModelState.IsValid)
            {
                throw new Exception("ModelState was invalid");
            }
        }
        catch(Exception ex)
        {
            string exceptionInfo = "File: Public.cshtml.cs \n Method: 'OnPostDislikeOrLike()' \n Stack Trace: \n";
            TempData["ErrorMessage"] = exceptionInfo += ex.StackTrace;
            return RedirectToPage("/Error");
        }

        return RedirectToPage("Public", new { page });
    }

    public async Task<IActionResult> OnPostDeleteCheep([FromQuery] int page = 0)
    {
        try
        {
            bool IsSuccess = await _cheepRepository.DeleteCheep(TargetAuthorUserName, TargetCheepId);

            string status = (IsSuccess) ? "Delete successful" : "Delete failed";
            _logger.LogInformation(status);
        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            RedirectToPage("/Error");
        }


        return RedirectToPage("UserTimeline", new { page });
    }

    public async Task<IActionResult> OnPostForgetMe([FromQuery] int page = 0)
    {
        _logger.LogInformation("[OnPostForgetMe()] Method called!");
        try
        {
            bool IsSuccess = false;

            IsSuccess = await _cheepRepository.DeleteAllCheepsFromAuthor(TargetAuthorUserName);
            
            _logger.LogInformation($"[OnPostForgetMe()] Forget Me Status: {IsSuccess}");

            if(IsSuccess)
            {
                IsSuccess = await _authorRepository.ForgetAuthor(TargetAuthorUserName);
                _logger.LogInformation($"[OnPostForgetMe()] Forget Me Status: {IsSuccess}");
            }

            if(!IsSuccess)
            {
                return RedirectToPage("/Error");
            } 
            else
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Public");
            }

        } 
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            RedirectToPage("/Error");
        }

        // 01: Get user and update.
        // 02: Delete all user cheeps.
        // 03: Sign out.
        // 04: Update the user so they CANT log back in.

        return RedirectToPage("UserTimeline", new { page });
    }
}
