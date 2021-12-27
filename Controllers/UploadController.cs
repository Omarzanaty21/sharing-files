using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileSharing.Models;
using Microsoft.AspNetCore.Authorization;
using FileSharing.Data;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment env;

        public UploadController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        public IActionResult Index()
        {
            var result = db.Uploads.Where(u => u.UserId == UserId)
            .OrderByDescending(u => u.UploadTime)
            .Select(u => new UploadViewModel{
                Id = u.id,
                OriginalFileName = u.OriginalFileName,
                FileName = u.FileName,
                ContentType = u.ContentType,
                Size = u.Size,
                UploadDate = u.UploadTime,
                DownloadCount = u.DownloadCount
            });
            
            return View(result);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Browse()
        {
           var model = await db.Uploads
           .OrderByDescending(u => u.DownloadCount)
            .Select(u => new UploadViewModel{
                FileName = u.FileName,  
                OriginalFileName = u.OriginalFileName,
                 ContentType = u.ContentType,
                  Size = u.Size,
                  UploadDate = u.UploadTime,
                  DownloadCount = u.DownloadCount
            }).ToListAsync();
           return View(model); 
        }
        private string UserId 
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
         public async Task<IActionResult> Create(InputUploadViewModel model)
        {
            if(ModelState.IsValid)
            {
                var newName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(model.File.FileName);
                var fileName = string.Concat(newName, extension);
                var root = env.WebRootPath;
                var path = Path.Combine(root,"Uploads",fileName);
                using(var fs = System.IO.File.Create(path))
                {
                   await model.File.CopyToAsync(fs);
                }
               await db.Uploads.AddAsync(new Uploads
                {
                    FileName = fileName,
                    OriginalFileName = model.File.FileName,
                    ContentType = model.File.ContentType,
                    Size = model.File.Length,
                    UserId = UserId
                });
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
                
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var selectedUpload = await db.Uploads.FindAsync(id);
            if(selectedUpload == null)
            {
                return NotFound();
            }
            if(selectedUpload.UserId != UserId)
            {
                return NotFound();
            }
            return View(selectedUpload);
        }
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmation(string id)
        {
            var selectedUpload = await db.Uploads.FindAsync(id);
            if(selectedUpload == null)
            {
                return NotFound();
            }
            if(selectedUpload.UserId != UserId)
            {
                return NotFound();
            }
            db.Uploads.Remove(selectedUpload);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task <IActionResult> Results(string term)
        {
            var model = await db.Uploads
            .Where(u => u.OriginalFileName.Contains(term))
            .OrderByDescending(u => u.DownloadCount)
            .Select(u => new UploadViewModel{
                FileName = u.FileName,  
                OriginalFileName = u.OriginalFileName,
                 ContentType = u.ContentType,
                  Size = u.Size,
                  UploadDate = u.UploadTime
            }).ToListAsync();;
           return View(model); 
        }
        [HttpGet]
        public async Task<IActionResult> Download(string id)
        {
            var selectedItem = await db.Uploads.FirstOrDefaultAsync(u => u.FileName == id);
            if(selectedItem == null)
            {
                return NotFound();
            }
            selectedItem.DownloadCount++;
            db.Update(selectedItem);
            await db.SaveChangesAsync();
            var path = "~/Uploads/" + selectedItem.FileName;
            Response.Headers.Add("Expires", DateTime.Now.AddDays(-3).ToLongDateString());
            Response.Headers.Add("Cache-Control", "no-cache");
            return File(path, selectedItem.ContentType, selectedItem.OriginalFileName);
        }
    }

}