using CoreIdentity.Models;
using CoreIdentity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreIdentity.Controllers
{
    public class HomeController : Controller
    {
        public UserManager<AppUser>  _userManager { get;}

        public HomeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                IdentityResult result = await _userManager.CreateAsync(user, userViewModel.Password);

                if (result.Succeeded) // Kayit olduktan sonra kullaniciyi login ekranina gönderip, bilgilerinin tekrar girilmesi en güvenli yoldur
                {
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    foreach (IdentityError item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description); //"" yazmamizin nedeni hatalari frontend tarafindaki asp-validation-summary kismina yani sayfanin en üstünde hatalari getirecektir. Biz "" yerine icerisine ilgili item'i yazabiliriz.
                    }
                }
            }
            return View(userViewModel); //Hatalari ekle tekrar kullanicinin girdigi bilgileri gönder
        }
    }
}
