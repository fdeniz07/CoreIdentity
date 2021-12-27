using CoreIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreIdentity.Controllers
{
    public class BaseController : Controller
    {
        protected UserManager<AppUser> _userManager { get; }
        protected SignInManager<AppUser> _signInManager { get; }

        public BaseController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected AppUser CurrentUser => _userManager.FindByNameAsync(User.Identity.Name).Result;//Her kullanici icin bir kimlik karti olustusturulur (User.Identity...) //.Name alani cookie den geliyor (hangi hesaptan login olmus)

        public void AddModelError(IdentityResult result)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("",item.Description);//"" yazmamizin nedeni hatalari frontend tarafindaki asp-validation-summary kismina yani sayfanin en üstünde hatalari getirecektir. Biz "" yerine icerisine ilgili item'i yazabiliriz.
            }
        }
    }
}
