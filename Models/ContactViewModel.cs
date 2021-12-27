using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FileSharing.Models
{
    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Subject { get; set; }
    }
}