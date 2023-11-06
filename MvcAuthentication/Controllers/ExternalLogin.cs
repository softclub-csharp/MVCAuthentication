using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MvcAuthentication.Controllers;

public class ExternalLogin(SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    // ILogger<ExternalLoginModel> logger,
    IEmailSender emailSende)
{
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task GetInfoFromGoogle()
    {
        var info = await signInManager.GetExternalAuthenticationSchemesAsync();
    }
}