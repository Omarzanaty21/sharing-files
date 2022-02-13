using System.ComponentModel.DataAnnotations;


namespace FileSharing.Models
{
    public class LoginViewModel 
        {
            [EmailAddress]
            [Required]
            public string Email {get; set;}
            [Required]
            public string Password {get; set;}
        }
        public class RegisterViewModel 
        {
            [EmailAddress]
            [Required]
            public string Email {get; set;}
            [Required]
            public string Password {get; set;}
            [Compare("Password")]
            public string ConfirmPassword {get; set;}
            public string FirstName { get; set; }
            public string LastName { get; set; }

        }
        public class ChangePasswordViewModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            [Compare("NewPassword")]
            public string ConfirmNewPassword { get; set; }
        }
        public class AddPasswordViewModel
        {
            [Required]
            [DataType(DataType.Password)]
           public string Password { get; set; }
        }
        public class ConfirmEmailViewModel
        {
            [Required]
            public string Token { get; set; }
            [Required]
            public string UserId { get; set; }
        }
        public class ForgotPasswordViewModel
        {
            [EmailAddress]
            [Required]
            public string Email { get; set; }
        }
        public class VerifyResetPasswordViewModel
        {
            [Required]
            public string Email { get; set; }
            [Required]
            public string Token { get; set; }
        }
        public class ResetPasswordViewModel
        {
            [Required]
            public string NewPassword { get; set; }
            [Required]
            [Compare("NewPassword")]
            public string ConfirmNewPassword { get; set; }
            [Required]
            public string Token { get; set; }
            [Required]
            public string Email { get; set; }
        }
}