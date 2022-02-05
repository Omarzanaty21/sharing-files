using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FileSharing.Data
{
    public class Contact
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        [ForeignKey("User")]
        public string UserId {get; set;}
        public virtual ApplicationUser User {get; set;}
    }
}