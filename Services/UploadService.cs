using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileSharing.Data;
using FileSharing.Models;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Services
{
    public class UploadService : IUploadService
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper _mapper;

        public UploadService(ApplicationDbContext db, IMapper mapper)
        {
            this.db = db;
            this._mapper = mapper;
        }
        public async Task CreateAsync(InputUpload model)
        {
            var mappedObj = _mapper.Map<Uploads>(model);
            var result = await db.Uploads.AddAsync(mappedObj);
            await db.SaveChangesAsync();
        }


        public async Task DeleteAsync(string id, string userId)
        {
             var selectedUpload = await db.Uploads.FirstOrDefaultAsync(u => u.id == id && u.UserId == userId);
             if(selectedUpload != null)
             {
                db.Uploads.Remove(selectedUpload);
                await db.SaveChangesAsync();
             }
        }

        public async Task<UploadViewModel> FindAsync(string id, string userId)
        {
            var result = await db.Uploads.FirstOrDefaultAsync(u => u.id == id && u.UserId == userId);
            if(result != null)
            {
               return _mapper.Map<UploadViewModel>(result);
            }
            return null;
        }

        public async Task<UploadViewModel> FindAsync(string id)
        {
            var result = await db.Uploads.FindAsync(id);
            if(result != null)
            {
               return _mapper.Map<UploadViewModel>(result);
            }
            return null;
        }

        public IQueryable<UploadViewModel> GetAll()
        {
            var result = db.Uploads
            .OrderByDescending(u => u.DownloadCount)
            .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
            return result;
        }

        public IQueryable<UploadViewModel> GetBy(string userId)
        {
            var result =  db.Uploads.Where(u => u.UserId == userId)
            .OrderByDescending(u => u.UploadTime)
            .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
            return result;
        }

        public async Task<int> GetUploadsCountAsync()
        {
            return await db.Uploads.CountAsync();
        }

        public async Task IncrementDownloadAsync(string id)
        {
            var selectedItem = await db.Uploads.FindAsync(id);
            if(selectedItem != null)
            {
                selectedItem.DownloadCount++;
                db.Update(selectedItem);
                await db.SaveChangesAsync();
            }
        }

        public IQueryable<UploadViewModel> Search(string term)
        {
            var result =  db.Uploads
            .Where(u => u.OriginalFileName.Contains(term))
            .OrderByDescending(u => u.DownloadCount)
            .ProjectTo<UploadViewModel>(_mapper.ConfigurationProvider);
            return result;
        }
    }
}