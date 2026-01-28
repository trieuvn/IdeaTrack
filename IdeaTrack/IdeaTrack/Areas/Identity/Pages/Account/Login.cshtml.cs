// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdeaTrack.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Username or Email for login
            /// </summary>
            [Required(ErrorMessage = "Please enter username or email")]
            [Display(Name = "Username or Email")]
            public string UsernameOrEmail { get; set; }

            /// <summary>
            /// Password
            /// </summary>
            [Required(ErrorMessage = "Please enter password")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// Remember me
            /// </summary>
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
                // Determine if input is email or username
                string username = Input.UsernameOrEmail;
                ApplicationUser user = null;

                // If input looks like an email, find user by email first
                if (Input.UsernameOrEmail.Contains("@"))
                {
                    user = await _signInManager.UserManager.FindByEmailAsync(Input.UsernameOrEmail);
                    if (user != null)
                    {
                        username = user.UserName; // Use the actual username for sign-in
                    }
                }
                else
                {
                    user = await _signInManager.UserManager.FindByNameAsync(Input.UsernameOrEmail);
                }

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Account does not exist.");
                    return Page();
                }

                // Sign in with username (Identity requires username, not email)
                var result = await _signInManager.PasswordSignInAsync(username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} logged in.", username);
                    
                    // Role-based redirect after successful login
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);
                    
                    if (roles.Contains("Admin"))
                        return LocalRedirect("/SciTech/Port");
                    if (roles.Contains("SciTech") || roles.Contains("OST_Admin"))
                        return LocalRedirect("/SciTech/Port");
                    if (roles.Contains("FacultyLeader") || roles.Contains("Faculty_Admin"))
                        return LocalRedirect("/Faculty/Dashboard");
                    if (roles.Contains("CouncilMember") || roles.Contains("Council_Member"))
                        return LocalRedirect("/Councils/Page");
                    
                    // Default: Author/Lecturer
                    return LocalRedirect("/Author/Dashboard");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
