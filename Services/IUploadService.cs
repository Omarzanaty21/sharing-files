using System;
using System.Linq;
using System.Threading.Tasks;
using FileSharing.Models;

namespace FileSharing.Services
{
    public interface IUploadService
    {
        IQueryable<UploadViewModel> GetAll();
        IQueryable<UploadViewModel> GetBy(string userId);
        IQueryable<UploadViewModel> Search(string term);
        Task CreateAsync(InputUpload model);
        Task DeleteAsync(string id, string userId);
        Task<UploadViewModel> FindAsync(string id);
        Task<UploadViewModel> FindAsync(string id, string userId);
        Task IncrementDownloadAsync(string id);
        Task<int> GetUploadsCountAsync();
    }
}