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
using FileSharing.Helpers.Mail;
using System.Text;

namespace FileSharing.Controllers
{
    public class AccountController : Controller
    {
        
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper _mapper;
        private readonly IMailHelper mailHelper;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IMailHelper mailHelper)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _mapper = mapper;
            this.mailHelper = mailHelper;
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
                else if(result.IsNotAllowed)
                {
                    TempData["Error"] = "You must Confirm your Email!";
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
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail","Account", new {token = token, userId = user.Id}, Request.Scheme);
                    StringBuilder body = new StringBuilder();
                    body.AppendLine("FIle Sharing Application : Email Confirmation");
                    body.AppendFormat("To confirm your email you should <a href={0}>Cilck here</a>", url);
                    mailHelper.SendMail(new InputEmailMessage{
                        Email = model.Email,
                        Subject = "Email Confirmation",
                        Body = body.ToString()
                    });
                    //await signInManager.SignInAsync(user, true);
                    //return RedirectToAction("Create", "Upload");
                    return RedirectToAction("RequireEmailConfirm");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
           
            return View(model); 
        }
        public IActionResult RequireEmailConfirm()
        {
            return View();
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
                    LastName = LastName,
                    EmailConfirmed = true
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
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User); 
            if(currentUser != null)
            {   
                if(ModelState.IsValid)
                {
                    var result = await userManager.ChangePasswordAsync(currentUser, model.CurrentPassword,
                     model.NewPassword);
                    if(result.Succeeded)
                    {
                        await signInManager.SignOutAsync();
                        return RedirectToAction("Login");
                    }
                    foreach(var error in result.Errors)
                    {
                            ModelState.AddModelError("", error.Description);
                    }
                    
                }
               
                
            }
            else
            {
                return NotFound();
            } 
            
            return View("Info",_mapper.Map<UserViewModel>(currentUser));
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User); 
            if(currentUser != null)
            {   
                if(ModelState.IsValid)
                {
                    var result = await userManager.AddPasswordAsync(currentUser, model.Password);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("Info");
                    }
                    foreach(var error in result.Errors)
                    {
                            ModelState.AddModelError("", error.Description);
                    }
                    
                }  
            }
            else
            {
                return NotFound();
            } 
           
            return View("Info",_mapper.Map<UserViewModel>(currentUser));
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if(user != null)
                {
                    if(!user.EmailConfirmed)
                    {
                        var result = await userManager.ConfirmEmailAsync(user, model.Token);
                        if(result.Succeeded)
                        {
                            TempData["Success"] = "Your Email Confirmed Successfully!";
                            return RedirectToAction("Login", "Account");
                        }
                        foreach(var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                    else
                    {
                        TempData["Success"] = "Your Email Already Confirmed!";
                    }

                }
            }
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existedUser);
                    var url = Url.Action("ResetPassword", "Account", new{token, model.Email}, Request.Scheme);
                    StringBuilder body = new StringBuilder();
                    body.AppendLine("File Sharing Application: Reset Password");
                    body.AppendLine("We are sending this mail, because we have received a reset password request to your account");
                    body.AppendFormat($"to reset new password, <a href={url}>Click here</a>");
                    mailHelper.SendMail(new InputEmailMessage{
                        Email = model.Email,
                        Subject = "Reset Password",
                        Body = body.ToString()
                    });
                }
                TempData["Success"] = "If you email matched an existed one in our system, an email shuold have been sent!";
            }
            return View(model);
        }
        public async Task<IActionResult> ResetPassword(VerifyResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var isValid = await userManager.VerifyUserTokenAsync(existedUser, TokenOptions.DefaultProvider, "ResetPassword", model.Token);
                    if(isValid)
                    {
                        return View(new ResetPasswordViewModel{Email = model.Email, Token = model.Token});
                    }
                    else
                    {
                        TempData["Error"] = "The token is invalid";
                    }
                }
                
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var result = await userManager.ResetPasswordAsync(existedUser, model.Token, model.NewPassword);
                    if(result.Succeeded)
                    {
                        TempData["Success"] = "Reset Password completed successfully!";
                        return RedirectToAction("Login");
                    }
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }
            return View(model);
        }
        
    }
}