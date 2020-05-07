using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop.Data;
using Shop.Models;
using Shop.Utility;

namespace Shop.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _db = db;
        }

        [BindProperty]  
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }

            public string PhoneNumber { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Role { get; set; }

        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var isexist = await _db.User.FirstOrDefaultAsync();
            string role;
            if (isexist == null)
            {
                role = "Manager";
            }
            else
            {
                role = Request.Form["UserRole"].ToString();
                if (role == "")
                {
                    role = "Customer";
                }
            }
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new User {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name,
                    City = Input.City,
                    PhoneNumber = Input.PhoneNumber,
                    StreetAddress = Input.StreetAddress,
                    State = Input.State,
                    PostalCode = Input.PostalCode,
                    Role = role
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if(!await _roleManager.RoleExistsAsync(StaticDefault.ManagerUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticDefault.ManagerUser));
                    }
                    if (!await _roleManager.RoleExistsAsync(StaticDefault.ShopUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticDefault.ShopUser));
                    }
                    if (!await _roleManager.RoleExistsAsync(StaticDefault.FrontDeskUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticDefault.FrontDeskUser));
                    }
                    if (!await _roleManager.RoleExistsAsync(StaticDefault.CustomerUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticDefault.CustomerUser));
                    }

                    if (role == StaticDefault.ManagerUser)
                    {
                        await _userManager.AddToRoleAsync(user, StaticDefault.ManagerUser);
                    }
                    else
                    {
                        if (role == StaticDefault.FrontDeskUser)
                        {
                            await _userManager.AddToRoleAsync(user, StaticDefault.FrontDeskUser);
                        }
                        else
                        {
                            if (role == StaticDefault.ShopUser)
                            {
                                await _userManager.AddToRoleAsync(user, StaticDefault.ShopUser);
                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(user, StaticDefault.CustomerUser);
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect(returnUrl);
                            }
                        }
                    }
                    return RedirectToAction("Index", "User", new { area = "Admin" });


                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
