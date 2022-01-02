using System;
using CoreIdentity.Models;
using CoreIdentity.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace CoreIdentity.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager) { }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
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
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabiniz bir süreligine kilitlenmistir.Lütfen daha sonra tekrar deneyiniz.");

                        return View(userLogin); //Hatalardan arindirilmis bir sekilde sayfayi tekrar yüklüyoruz
                    }

                    if (_userManager.IsEmailConfirmedAsync(user).Result == false) //Kullanicinin, hesabi email dogrulamasi ile aktif edilip edilmediginin konrtolü yapiliyor
                    {
                        ModelState.AddModelError("","Email adresiniz onaylanmamistir. Lütfen e-postanizi kontrol ediniz.");
                        return View(userLogin);
                    }

                    await _signInManager.SignOutAsync(); //Varsa cookie degeri silinsin

                    SignInResult result = await _signInManager.PasswordSignInAsync(user, userLogin.Password, userLogin.RememberMe, false);
                    //userLogin.RememberMe degeri true gelirse 7 günlük cookie(bizim belirledigimiz süre) ömrü gecerli olacak

                    if (result.Succeeded) //Giris basarili ise
                    {
                        await _userManager.ResetAccessFailedCountAsync(user); // Bu metotla giris sayisini 0'a cekiyoruz

                        if (TempData["ReturnUrl"] != null)
                        {
                            return RedirectToAction(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await _userManager.AccessFailedAsync(user); // Giris basarisizsa, basarisiz giris sayisini 1 arttiriyoruz

                        int fail = await _userManager.GetAccessFailedCountAsync(user); // Basarisit giris sayisini aliyoruz

                        ModelState.AddModelError("", $" {fail} kez basarisiz giris."); // Hatalari giris sayilarini kullaniciya gösterme



                        if (fail == 3) //Basarisiz giris sayisi 3'e esitse,
                        {
                            await _userManager.SetLockoutEndDateAsync(user,
                                new DateTimeOffset(DateTime.Now.AddMinutes(20)));// 20dk boyunca kullaniciyi kilitliyoruz.

                            ModelState.AddModelError("", "Hesabiniz 3 basarisiz giris denemesinden dolayi 20 dakika süreyle kilitlenmistir. Lütfen daha sonra tekrar deneyiniz.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Email adresiniz veya sifreniz yanlistir.");//"": Summary alaninda cikacak mesaji belirtir. Kötü niyetli kullanicilarin messajda hangisinin yanlis oldugunu anlamamasi gereklidir.
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayitli kullanici bulunamamistir.");
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
                    string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    }, protocol: HttpContext.Request.Scheme
                    );

                    Helpers.EmailConfirmation.SendEmail(link, user.Email);

                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    AddModelError(result);
                }
            }
            return View(userViewModel); //Hatalari ekle tekrar kullanicinin girdigi bilgileri gönder
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            TempData["durum"] = null;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel passwordResetViewModel)
        {
            if (TempData["durum"] == null) //Sayfayi her yeniledigimizde kullaniciya e-posta gitmesini önlemek icin, kullaniciya e-posta gidince TempData degerini true yapiyoruz.

            {

                AppUser user = _userManager.FindByEmailAsync(passwordResetViewModel.Email)
                    .Result; //Böyle bir email'e ait kullanicinin var olup olmadigini ariyoruz

                if (user != null) // kullanici mevcutsa
                {
                    string passwordResetToken =
                        _userManager.GeneratePasswordResetTokenAsync(user)
                            .Result; //db deki securitystamp degerini aliyoruz

                    string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                    {
                        userId = user.Id,
                        token = passwordResetToken
                    }, HttpContext.Request.Scheme);
                    //www.denizfatih.com/Home/ResetPasswordConfirm?userId=fdsfhdsj&token=dfsjdkgfd

                    Helpers.PasswordReset.PasswordResetSendEmail(passwordResetLink, user.Email); //Helper sinifindaki metodumuz

                    ViewBag.status = "success";
                    TempData["durum"] = true.ToString();
                }
                else
                {
                    ModelState.AddModelError("", "Sistemde kayitli email adresi bulunamamistir.");
                }

                return View(passwordResetViewModel);
            }
            else
            {
                return RedirectToAction("ResetPassword");
            }
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")] PasswordResetViewModel passwordResetViewModel) //[Bind("PasswordNew")] islemi, vievmodel'deki sadece
                                                                                                                                   //istedigimiz alanin gelmesini saglar
        {
            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();

            AppUser user = await _userManager.FindByIdAsync(userId); // Ilgili userId DB de aratilir

            if (user != null) //Ilgili kullanici sistemde mevcutsa
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, token, passwordResetViewModel.PasswordNew); //ilgili parametrelere göre kullanici sifresi sifirlanir

                if (result.Succeeded) //islem gecerliyse
                {
                    await _userManager.UpdateSecurityStampAsync(user); //Kullanicinin kritik bilgileri her degistiginde mutlaka Db'deki SecurityStamp degeri güncellenmelidir.

                    TempData["passwordResetInfo"] = "sifreniz basariyla yenilenmistir. Yeni sifreniz ile giris yapabilirsiniz.";

                    ViewBag.status = "success";
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Bir hata meydana gelmistir. Lütfen daha sonra tekrar deneyiniz");
            }
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId); //Ilgili user'i kontrol ediyoruz

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token); //Kullanici ve token bilgisini geldikten sonra DB de ilgili alani true yapiyoruz

            if (result.Succeeded)
            {
                ViewBag.status = "Email adresiniz onaylanmistir. Login ekranindan giris yapabilirsiniz.";
            }
            else
            {
                ViewBag.status = "Bir hata meydana geldi. Lütfen daha sonra tekrar deneyiniz.";
            }
            return View();
        }
    }
}
