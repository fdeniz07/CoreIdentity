using CoreIdentity.Models;
using CoreIdentity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace CoreIdentity.Controllers
{
    public class HomeController : Controller
    {
        public UserManager<AppUser> _userManager { get; }
        public SignInManager<AppUser> _signInManager { get; }

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LogIn(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel userLogin)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(userLogin.Email);

                if (user != null)
                {
                    await _signInManager.SignOutAsync(); //Varsa cookie degeri silinsin

                    SignInResult result = await _signInManager.PasswordSignInAsync(user, userLogin.Password, userLogin.RememberMe, false);
                    //userLogin.RememberMe degeri true gelirse 7 günlük cookie(bizim belirledigimiz süre) ömrü gecerli olacak

                    if (result.Succeeded)
                    {
                        if (TempData["ReturnUrl"]!=null)
                        {
                            return RedirectToAction(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                }
                else
                {
                    ModelState.AddModelError("","Gecersiz email adresi veya sifresi"); //"": Summary alaninda cikacak mesaji belirtir. Kötü niyetli kullanicilarin messajda hangisinin yanlis oldugunu anlamamasi gereklidir.
                }
            }
            return View(userLogin);
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
