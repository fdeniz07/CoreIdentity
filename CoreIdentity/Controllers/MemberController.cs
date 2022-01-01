using System;
using System.IO;
using System.Security.Claims;
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

        public IActionResult AccessDenied(string returnUrl)
        {
            if (returnUrl.Contains("ViolencePage"))
            {
                ViewBag.message = "Erişmeye çalıştığınız sayfa şiddet videoları içerdiğinden dolayı 15 yaşından büyük olmanız gerekmektedir";
            }
            else if (returnUrl.Contains("AnkaraPage"))
            {
                ViewBag.message = "Bu sayfaya sadece şehir alanı ankara olan kullanıcılar erişebilir";
            }
            else if (returnUrl.Contains("Exchange"))
            {
                ViewBag.message = "30 günlük ücretsiz deneme hakkiniz sona ermistir.";
            }
            else
            {
                ViewBag.message = "Bu sayfadaki içeriklere erişim yetkiniz yoktur.";
            }

            return View();
        }

        [Authorize(Roles = "Admin,Editor")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Manager()
        {
            return View();
        }

        [Authorize(Policy = "AnkaraPolicy")]
        public IActionResult AnkaraPage()
        {
            return View();
        }

        [Authorize(Policy = "ViolencePolicy")]
        public IActionResult ViolencePage()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRedirect() //Claim'i DB ye ekleyecegimiz alan.
        {
            bool result = User.HasClaim(x => x.Type == "ExpireDateExchange"); // ExpireDateExchange isminde bir Claim'in olup olmadigini kontrol ediyoruz

            if (!result) // Eger böyle bir claim yoksa yeni bir claim ekliyoruz
            {
                Claim ExpireDateExchange = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).ToShortDateString(),
                    ClaimValueTypes.String, "Internal"); //Claim olusturuyoruz

                await _userManager.AddClaimAsync(CurrentUser, ExpireDateExchange); //Ilgili kullaniciya bu claim'i atiyoruz
                await _signInManager.SignOutAsync(); // Kullaniciyi arka planda cikartiyoruz. Bunun nedeni cookie bilgilerinin hemen güncellenmesi.
                await _signInManager.SignInAsync(CurrentUser, true); // Burada kullaniciyi arka planda oturum aciyoruz. True degeri ile cookie süresinin saklanma süresini yeniliyoruz. Ancak bu durumu kullanici farketmeyecektir
            }

            return RedirectToAction("Exchange");
        }

        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange() // Ücretli ya da ilk 30 günlük deneme alani
        {
            return View();
        }
    }
}
