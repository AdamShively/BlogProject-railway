
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
using BlogProject.Models;
using MovieProject.Models.Settings;
using Microsoft.Extensions.Options;

namespace BlogProject.Areas.Identity.Pages.Account
{
    public class AdminModDemoModel : PageModel
    {
        private readonly SignInManager<BlogUser> _signInManager;
        private readonly CredentialSettings _credSettings;

        public AdminModDemoModel(SignInManager<BlogUser> signInManager, IOptions<CredentialSettings> credSettings)
        {
            _signInManager = signInManager;
            _credSettings = credSettings.Value;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var email = _credSettings.DemoMail;
            var password = _credSettings.DemoPassword;

            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index");
        }
    }
}