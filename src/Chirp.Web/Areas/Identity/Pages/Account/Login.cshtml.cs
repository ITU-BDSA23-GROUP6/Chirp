// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Chirp.Models;


namespace Chirp.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Author> _signInManager;
        private readonly UserManager<Author> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<Author> signInManager, UserManager<Author> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByNameAsync(Input.UserName);

                // [TODO] Change to switch case:
                if (signInResult.Succeeded)
                {
                    _logger.LogInformation($"[LOG-IN] User {Input.UserName} logged in.");
                    return LocalRedirect(returnUrl);
                }
                else if (signInResult.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                else if (signInResult.IsLockedOut)
                {
                    _logger.LogWarning("[LOG-IN] User account locked out of page.");
                    return RedirectToPage("./Lockout");
                }
                else if(signInResult.IsNotAllowed)
                {
                    string userNotAllowed_msg = "Invalid Login Attempt - User is not permitted";
                    _logger.LogInformation($"[LOG-IN] User '{Input.UserName}' is not allowed to sign in.");

                    if(!await _userManager.IsEmailConfirmedAsync(user)) 
                    {
                        userNotAllowed_msg += " - Email is NOT confirmed.";
                    }

                    ModelState.AddModelError(string.Empty, userNotAllowed_msg);
                    return Page();
                }
                else
                {
                    _logger.LogInformation($"User '{Input.UserName}' login failed: {signInResult}");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt - Information incorrect");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
