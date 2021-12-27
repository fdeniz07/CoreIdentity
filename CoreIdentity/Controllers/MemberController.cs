using System;
using System.IO;
using System.Threading.Tasks;
using CoreIdentity.Enums;
using CoreIdentity.Models;
using CoreIdentity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreIdentity.Controllers
{
    [Authorize] // Controller bazli kisitlama
    public class MemberController : BaseController
    {
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager, signInManager) { }
        public IActionResult Index()
        {
            AppUser user = CurrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>(); //Mapster araciligi ile tablolar eslestirilir


            return View(userViewModel);
        }

        [HttpGet]
        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender))); //Enum tipindeki cinsiyetleri Dropdownlist icinde göstermek icin

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model,IFormFile userPicture)
        {
            ModelState.Remove("Password"); // Modelimizden Sifre alanini cikartiyoruz. Sifre degistirme alanini baska action'da yapiyoruz.

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender))); //ilgili alanin post isleminden sonra tekrar dolu gelmesi icin yaziyoruz

            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if (userPicture!=null && userPicture.Length>0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);//Ayni isimde resim olmamasi icin her resme bir guid degeri ekliyoruz

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName); //DB'ye yazacagimiz Path'i belirtiyoruz ve ilgili dosyaninin olup olmadigini kontrol ediyoruz

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream); //ilgili dosyayi kopyaliyoruz

                        user.Picture = "/UserPicture/" + fileName; //DB'ye yazacagimiz yolu kaydediyoruz
                    }
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.City = model.City;
                user.BirthDay=model.BirthDay;
                user.Gender= (int)model.Gender;

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
                    AddModelError(result);
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
                AppUser user = CurrentUser;

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

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
