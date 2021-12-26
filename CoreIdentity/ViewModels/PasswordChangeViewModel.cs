using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Display(Name = "Eski sifreniz")]
        [Required(ErrorMessage = "Eski sifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Sifreniz en az 4 karakterden olusmalidir.")]
        public string PasswordOld { get; set; }

        [Display(Name = "Yeni sifreniz")]
        [Required(ErrorMessage = "Yani sifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Sifreniz en az 4 karakterden olusmalidir.")]
        public string PasswordNew { get; set; }


        [Display(Name = "Onay sifreniz")]
        [Required(ErrorMessage = "Onay sifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Sifreniz en az 4 karakterden olusmalidir.")]
        [Compare("PasswordNew", ErrorMessage = "Yeni Sifreniz ve Onay Sifreniz birbirinden farklidir")]
        public string PasswordConfirm { get; set; }
    }
}
