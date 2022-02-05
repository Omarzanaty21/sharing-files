using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace FileSharing.Data 
{
    public class Uploads 
    {
        public Uploads()
        {
            id = Guid.NewGuid().ToString();
            UploadTime = DateTime.Now;
        }
        public string id {get; set;}
        public string OriginalFileName { get; set; }
        public string FileName {get; set;}
        public string ContentType {get; set;}
        public decimal Size {get; set;}
        public string UserId {get; set;}
        public ApplicationUser User {get; set;}
        public DateTime UploadTime {get; set;}
        public long DownloadCount { get; set; }
    }
}