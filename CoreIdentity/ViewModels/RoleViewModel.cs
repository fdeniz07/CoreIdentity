using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.ViewModels
{
    public class RoleViewModel
    {
        [Display(Name = "Role adi")]
        [Required(ErrorMessage="Rol ismi gereklidir.")]
        public string Name { get; set; }

        public string Id { get; set; }
    }
}
