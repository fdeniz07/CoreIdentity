using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage ="Kullanici ismi gereklidir.")]
        [Display(Name ="Kullanici Adi")]
        public string UserName { get; set; }

        [Display(Name = "Telefon No")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [Display(Name = "Email Adresimiz")]
        [EmailAddress(ErrorMessage ="Email adresiniz dogru formatta degil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sifreniz gereklidir.")]
        [Display(Name = "Sifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
