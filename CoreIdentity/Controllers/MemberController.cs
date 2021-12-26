using System.Threading.Tasks;
using CoreIdentity.Models;
using CoreIdentity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreIdentity.Controllers
{
    [Authorize] // Controller bazli kisitlama
    public class MemberController : Controller
    {
        public UserManager<AppUser> _userManager { get; }
        public SignInManager<AppUser> _signInManager { get; }

        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result; //Her kullanici icin bir kimlik karti olustusturulur (User.Identity...)
            UserViewModel userViewModel = user.Adapt<UserViewModel>(); //Mapster araciligi ile tablolar eslestirilir


            return View(userViewModel);
        }

        [HttpGet]
        public IActionResult UserEdit()
        {
            AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            ModelState.Remove("Password"); // Modelimizden Sifre alanini cikartiyoruz. Sifre degistirme alanini baska action'da yapiyoruz.

            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user); //Securitystamp degerini de degistiriyoruz.
                    await _signInManager.SignOutAsync(); //sifre degisimden sonra kullaniciyi sistemden cikartiyoruz
                    await _signInManager.SignInAsync(user, true); // Cikis isleminden sonra kullaniciya tekrar giris yaptiriyoruz.

                    ViewBag.success = "true";
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("",item.Description);
                    }
                }
            }
            return View(model);
        }


        public void LogOut()
        {
            _signInManager.SignOutAsync();

            //return RedirectToAction("Index", "Home");// Biz bu metot yerine startup.cs icerisinde route islemi tanimlayacagiz ve buraya ait _MemberLayout icerisinde ilgili route adres tanimlamasini yapacagiz. Eger biz buradaki gibi bir kodlama yapacaksak, metodun dönüs tipini void'den IActionResult'a cevirmemiz gerekir.
        }

        [HttpGet]
        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = _userManager.FindByNameAsync(User.Identity.Name).Result; //.Name alani cookie den geliyor (hangi hesaptan login olmus)

                //eski sifreyi kontrol ediyoruz
                bool exist = _userManager.CheckPasswordAsync(user, model.PasswordOld).Result;

                if (exist) //eski sifre gecerliyse
                {
                    IdentityResult result = _userManager.ChangePasswordAsync(user, model.PasswordOld, model.PasswordNew).Result;

                    if (result.Succeeded && model.PasswordOld != model.PasswordNew) // sifre degistime basarili ise ve eski sifre yeni sifreyle ayni degilse
                    {
                        await _userManager.UpdateSecurityStampAsync(user); //Securitystamp degerini de degistiriyoruz.
                        await _signInManager.SignOutAsync(); //sifre degisimden sonra kullaniciyi sistemden cikartiyoruz
                        await _signInManager.PasswordSignInAsync(user, model.PasswordNew, true, false); // Cikis isleminden sonra kullaniciya tekrar giris yaptiriyoruz. Eger cikis ve giris islemleri yapilmasaydi kullanici isPersistent degerinden dolayi default olarak 30 dk sonra sistemden cikarilacaktir. 

                        //ÖNEMLI NOT
                        //await _signInManager.SignInAsync() //AuthenticationMethod parametresi olarak birden fazla login ekranimiz varsa onu secmemiz lazim. Mesala User ve Bayi ekrani gibi. Bu projede tek bir login ekrani oldudugu icin biz, await _signInManager.PasswordSignInAsync(user, model.PasswordNew, true, false); bunu kullaniyoruz.

                        ViewBag.success = "true";
                    }
                    else
                    {
                        if (model.PasswordOld == model.PasswordNew)
                        {
                            ModelState.AddModelError("", "Yeni sifreniz eski sifrenizden farkli olmalidir");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Eski sifrenizi hatali girdiniz.");
                }
            }
            return View(model);
        }
    }
}
