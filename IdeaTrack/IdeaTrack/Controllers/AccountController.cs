using IdeaTrack.Models;
using IdeaTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IdeaTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        // Updated role list per user requirements
        private static readonly string[] AvailableRoles = new[]
        {
            "Admin",
            "SciTech",
            "FacultyLeader",
            "CouncilMember",
            "Author",
            "Lecturer"
        };

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult HiddenRegister(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Roles = new SelectList(AvailableRoles);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HiddenRegister(HiddenRegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        // Ensure the role exists before assigning
                        await EnsureRoleExistsAsync(model.SelectedRole);
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    _logger.LogInformation("User created a new account with password and role {Role}.", model.SelectedRole);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            ViewBag.Roles = new SelectList(AvailableRoles);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Redirects the user to their role-specific profile page.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Redirect to role-specific profile page
            if (roles.Contains("SciTech") || roles.Contains("OST_Admin"))
            {
                return RedirectToAction("Index", "Port", new { area = "SciTech" });
            }
            if (roles.Contains("FacultyLeader") || roles.Contains("Faculty_Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Faculty" });
            }
            if (roles.Contains("CouncilMember") || roles.Contains("Council_Member"))
            {
                return RedirectToAction("Index", "Page", new { area = "Councils" });
            }

            // Default: Author portal profile
            return RedirectToAction("Index", "Dashboard", new { area = "Author" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            _logger.LogInformation("ExternalLogin called with provider: {Provider}, returnUrl: {ReturnUrl}", provider, returnUrl);
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            _logger.LogInformation("Configured redirect URL: {RedirectUrl}", redirectUrl);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            _logger.LogInformation("ExternalLoginCallback triggered. returnUrl: {ReturnUrl}, remoteError: {RemoteError}", returnUrl, remoteError);
            
            if (remoteError != null)
            {
                _logger.LogError("Remote error from external provider: {RemoteError}", remoteError);
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction("HiddenRegister");
            }
            
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("GetExternalLoginInfoAsync returned null.");
                TempData["ErrorMessage"] = "Could not retrieve external login information. Please try again.";
                return RedirectToAction("HiddenRegister");
            }

            _logger.LogInformation("External login info retrieved for provider: {Provider}", info.LoginProvider);

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User is locked out.");
                TempData["ErrorMessage"] = "Your account has been locked out.";
                return RedirectToAction("HiddenRegister");
            }
            else
            {
                // If the user does not have an account, automatically create one with Author role
                _logger.LogInformation("No existing account for external login. Auto-creating user with Author role.");
                
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
                
                // Check if email is available
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email not provided by external provider.");
                    TempData["ErrorMessage"] = "Email not provided by external provider. Please try another login method.";
                    return RedirectToAction("HiddenRegister");
                }
                
                // Check if user already exists with this email
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // Link the external login to existing account
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        _logger.LogInformation("Linked {Provider} provider to existing account for {Email}.", info.LoginProvider, email);
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        _logger.LogError("Failed to link external login to existing account: {Errors}", 
                            string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                        TempData["ErrorMessage"] = "Failed to link external login to your account.";
                        return RedirectToAction("HiddenRegister");
                    }
                }
                
                // Create new user
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true, // Trust email from Google
                    FullName = fullName
                };
                
                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Assign default role: Author
                        const string defaultRole = "Author";
                        await EnsureRoleExistsAsync(defaultRole);
                        await _userManager.AddToRoleAsync(user, defaultRole);
                        
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Provider} provider with default role Author.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                
                _logger.LogError("Failed to create user account: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Failed to create user account. Please try again.";
                return RedirectToAction("HiddenRegister");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalRegister(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Roles = new SelectList(AvailableRoles);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalRegister(ExternalRegisterViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("GetExternalLoginInfoAsync returned null during ExternalRegister POST.");
                    TempData["ErrorMessage"] = "External login information not found. Please try again.";
                    return RedirectToAction("HiddenRegister");
                }
                
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email, 
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? model.Email 
                };
                
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(model.SelectedRole))
                        {
                            await EnsureRoleExistsAsync(model.SelectedRole);
                            await _userManager.AddToRoleAsync(user, model.SelectedRole);
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider with role {Role}.", info.LoginProvider, model.SelectedRole);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.Roles = new SelectList(AvailableRoles);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        /// <summary>
        /// Ensures a role exists in the system, creating it if necessary.
        /// </summary>
        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogInformation("Creating role: {RoleName}", roleName);
                var role = new ApplicationRole { Name = roleName, Description = $"{roleName} role" };
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
