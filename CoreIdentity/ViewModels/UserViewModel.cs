using System;
using System.ComponentModel.DataAnnotations;
using CoreIdentity.Enums;

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
        [Display(Name = "Email Adresiniz")]
        [EmailAddress(ErrorMessage ="Email adresiniz dogru formatta degil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sifreniz gereklidir.")]
        [Display(Name = "Sifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Sehir")]
        public string City { get; set; }

        public string Picture { get; set; }

        [Display(Name = "Dogum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }
    }
}
