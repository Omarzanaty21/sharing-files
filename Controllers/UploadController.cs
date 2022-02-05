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
using FileSharing.Services;
using AutoMapper;

namespace FileSharing.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IUploadService uploadService;
        private readonly IWebHostEnvironment env;
        

        public UploadController(IUploadService uploadService, IWebHostEnvironment env)
        {
            this.uploadService = uploadService;
            this.env = env;
        }
        public IActionResult Index()
        {
            var result = uploadService.GetBy(UserId);
            return View(result);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Browse(int requiredPage = 1)
        {
            var result = uploadService.GetAll();

            var model = await GetPagedData(result, requiredPage);

           return View(model); 
        }
        private async Task<List<UploadViewModel>> GetPagedData(IQueryable<UploadViewModel> result, int requiredPage = 1)
        {
            const int pageSize = 3;
            decimal rowsCount = await uploadService.GetUploadsCountAsync();
            var pageCount = Math.Ceiling(rowsCount / pageSize);

            requiredPage = requiredPage <= 0 ? 1: requiredPage;

             if(requiredPage > pageCount)
            {
                requiredPage = 1;
            }
            int skipCount = (requiredPage - 1) * 3;
            
           
            var pagedData = await result
            .Skip(skipCount)
            .Take(pageSize)
            .ToListAsync();
            ViewBag.currentPage = requiredPage;
            ViewBag.pageCount = pageCount;
            return pagedData;
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
                await uploadService.CreateAsync(new InputUpload{
                    FileName = fileName,
                    OriginalFileName = model.File.FileName,
                    Size = model.File.Length,
                    ContentType = model.File.ContentType,
                    UserId = UserId, 
                });
                return RedirectToAction("Index");
                
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var selectedUpload = await uploadService.FindAsync(id, UserId);
            if(selectedUpload == null)
            {
                return NotFound();
            }
            return View(selectedUpload);
        }
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmation(string id)
        {
           var selectedUpload = await uploadService.FindAsync(id, UserId);
            if(selectedUpload == null)
            {
                return NotFound();
            }
            await uploadService.DeleteAsync(id, UserId);
            return RedirectToAction("Index");
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task <IActionResult> Results(string term, int requiredPage = 1)
        {
            var result = uploadService.Search(term);
           
            var model = await GetPagedData(result, requiredPage);
            
            return View(model); 
        }
        [HttpGet]
        public async Task<IActionResult> Download(string id)
        {
            var selectedItem = await uploadService.FindAsync(id);
            if(selectedItem == null)
            {
                return NotFound();
            }
            
            await uploadService.IncrementDownloadAsync(id);
            var path = "~/Uploads/" + selectedItem.FileName;
            Response.Headers.Add("Expires", DateTime.Now.AddDays(-3).ToLongDateString());
            Response.Headers.Add("Cache-Control", "no-cache");
            return File(path, selectedItem.ContentType, selectedItem.OriginalFileName);
        }
    }

}