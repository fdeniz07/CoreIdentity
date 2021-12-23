using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.ViewModels
{
    public class PasswordResetViewModel
    {
        [Display(Name = "Email adresiniz")]
        [Required(ErrorMessage = "Email alani gereklidir")]
        [EmailAddress]
        public string Email { get; set; }


        [Display(Name = "Yeni Sifreniz")]
        [Required(ErrorMessage = "Sifre alani gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Sifreniz en az 4 karakterli olmalidir.")]
        public string PasswordNew { get; set; }
    }
}
