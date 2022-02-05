using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileSharing.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using FileSharing.Data;
using AutoMapper;
using Microsoft.Extensions.Localization;




namespace FileSharing.Controllers
{
    public class AccountController : Controller
    {
        
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> iLocalizer;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IStringLocalizer<> ILocalizer)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _mapper = mapper;
            iLocalizer = ILocalizer;
        }
        // public IActionResult Index()
        // {
        //     return View();
        // }
    
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);
                if(result.Succeeded)
                {
                    if(!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Create", "Upload");
                }
            }
            return View(model);
        }
         [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if(result.Succeeded)
                {
                    await signInManager.SignInAsync(user, true);
                    return RedirectToAction("Create", "Upload");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
           
            return View(model); 
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); 
        }
        public IActionResult ExternalLogin(string provider)
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, "/Account/ExternalResponse");
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalResponse() 
        {
            var info =await signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                TempData["Message"] = "Login Failed!";
                return RedirectToAction("Login");
            }
            var result =await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if(!result.Succeeded)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                //Create Account
                var userCreated = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = LastName
                };
                var createdResult = await userManager.CreateAsync(userCreated);
                if(createdResult.Succeeded)
                {
                    var externalLoginResult = await userManager.AddLoginAsync(userCreated, info); // AspNetUserLogin Table
                    if(externalLoginResult.Succeeded)
                    {
                        await signInManager.SignInAsync(userCreated, false, info.LoginProvider);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        await userManager.DeleteAsync(userCreated);
                    }
                }
               return RedirectToAction("Login");
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Info()
        {
            //return Ok(_localizer["test"]);
            var currentUser = await userManager.GetUserAsync(User);  
            if(currentUser != null)
            {
                var model = _mapper.Map<UserViewModel>(currentUser);
                return View(model);
            }
            return NotFound(); 
        }
        [HttpPost]
        public async Task<IActionResult> Info(UserViewModel model)
        {
            if(ModelState.IsValid)
            {
                var currentUser = await userManager.GetUserAsync(User);  
                if(currentUser != null)
                {
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;
                    var result = await userManager.UpdateAsync(currentUser);
                    if(result.Succeeded)
                    {
                        TempData["Success"] = "";
                        return RedirectToAction("Info");
                    }
                        foreach(var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    
                }
                else
                {
                    return NotFound();
                } 
            }
            return View(model);
        }
    }
}