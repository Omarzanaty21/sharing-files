using System;
using Microsoft.AspNetCore.Http;

namespace FileSharing.Models
{
    public class InputUploadViewModel 
    {
        public IFormFile File { get; set; }
    }
    public class UploadViewModel 
    {
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public decimal Size { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public string Id { get; set; }
        public long DownloadCount { get; set; }
    }
}