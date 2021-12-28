using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileSharing.Models;
using FileSharing.Data;
using System.Security.Claims;
using FileSharing.Helpers.Mail;
using System.Text;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace FileSharing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;
        private readonly IMailHelper helper;
        private readonly IStringLocalizer<HomeController> _localizer;

        private string UserId{
            get{
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db,
        IMailHelper helper, IStringLocalizer<HomeController> localizer) 
        {
            _logger = logger;
            this.db = db;
            this.helper = helper;
            _localizer = localizer;
        }
        [HttpGet("/test")]
        public IActionResult Info()
        {
            return Ok(_localizer["test"]);
            // return View();
        }
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if(ModelState.IsValid)
            {
                await db.Contact.AddAsync(new Data.Contact{
                    Email = model.Email,
                    Message = model.Message,
                    Name = model.Name,
                    Subject = model.Subject,
                    UserId = UserId
                });
                await db.SaveChangesAsync();
                TempData["Message"] = "Your Contact was sent successfully";
                //send mail
                StringBuilder sp = new StringBuilder();
                sp.AppendLine("<h1>File Sharing - Unread Message</h1>");
                sp.AppendFormat("Name : {0}", model.Name);
                sp.AppendFormat("Email : {0}", model.Email);
                sp.AppendLine();
                sp.AppendFormat("Subject : {0}", model.Subject);
                sp.AppendFormat("Message : {0}", model.Message);

                helper.SendMail(new InputEmailMessage{
                    Subject = "You have unread messages",
                    Email = model.Email,
                    Body = sp.ToString()
                });
                return RedirectToAction("Contact");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Index()
        {
            var highestDownloads = db.Uploads
            .OrderByDescending(u => u.DownloadCount)
            .Select(u => new UploadViewModel{
                Id = u.id,
                OriginalFileName = u.OriginalFileName,
                FileName = u.FileName,
                ContentType = u.ContentType,
                Size = u.Size,
                UploadDate = u.UploadTime,
                DownloadCount = u.DownloadCount
            })
            .Take(3);
            ViewBag.poplarDownloads = highestDownloads;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult SetCulture(string lang)
        {
            if(!string.IsNullOrEmpty(lang))
            {
                // Response.Cookies.Append("culture", lang);
                
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                    new CookieOptions{Expires = DateTimeOffset.UtcNow.AddYears(1)}  
                );
            }
            return RedirectToAction("Index");
        }
    }
}
