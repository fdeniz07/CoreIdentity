using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.ViewModels
{
    public class PasswordResetByAdminViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }
    }
}
