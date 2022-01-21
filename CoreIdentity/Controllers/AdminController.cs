using System.Collections.Generic;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using CoreIdentity.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace CoreIdentity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager) { }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Claims()
        {

            return View(User.Claims.ToList()); // Cookie den gelen User bilgileri listelenecek
        }

        [HttpGet]
        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel model)
        {
            AppRole role = new AppRole();
            role.Name = model.Name;
            IdentityResult result = _roleManager.CreateAsync(role).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }

            return View(model);
        }

        public IActionResult Roles()
        {
            return View(_roleManager.Roles.ToList());
        }

        public IActionResult RoleDelete(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                IdentityResult result = _roleManager.DeleteAsync(role).Result;

            }
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public IActionResult RoleUpdate(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                return View(role.Adapt<RoleViewModel>()); // AppRole'in RoleViewModel'e dönüstürülmesi icin mapster kütüphanesinden yardim aliyoruz.
            }
            return RedirectToAction("Roles");
        }

        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel model)
        {
            AppRole role = _roleManager.FindByIdAsync(model.Id).Result;

            if (role != null)
            {
                role.Name = model.Name;

                IdentityResult result = _roleManager.UpdateAsync(role).Result;

                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme islemi basarisiz oldu.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RolesAssign(string id)
        {
            TempData["userId"] = id;
            AppUser user = _userManager.FindByIdAsync(id).Result; //id den gelen degere göre kullaniciyi buluyoruz

            ViewBag.userName = user.UserName; //Kullanicinin adini aliyoruz

            IQueryable<AppRole> roles = _roleManager.Roles; //Rollerin tamamini cekiyoruz

            List<string> userRoles = _userManager.GetRolesAsync(user).Result as List<string>; //Kullanicinin sahip oldugu rolleri listeliyoruz

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();

            foreach (var role in roles) //Kullanicinin varolan rolleri arasinda geziyoruz
            {
                RoleAssignViewModel roleAssign = new RoleAssignViewModel();

                roleAssign.RoleId = role.Id;
                roleAssign.RoleName = role.Name;

                if (userRoles.Contains(role.Name))
                {
                    roleAssign.Exist=    true;
                }
                else
                {
                    roleAssign.Exist = false;
                }

                roleAssignViewModels.Add(roleAssign);
            }

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RolesAssign(List<RoleAssignViewModel> models) //CheckBox'lardan isaretli olanlarin listesini aliyoruz
        {
            AppUser user = _userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            foreach (var item in models)
            {
                if (item.Exist) //check edildiyse ilgili rolü kullaniciya atiyoruz
                {
                    await _userManager.AddToRoleAsync(user,item.RoleName);
                }
                else //check kaldirildiysa ilgili rolü kullanicidan aliyoruz
                {
                  await  _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);

            PasswordResetByAdminViewModel model = new PasswordResetByAdminViewModel();
            model.UserId = user.Id;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(PasswordResetByAdminViewModel model)
        {
            AppUser user = await _userManager.FindByIdAsync(model.UserId); //Modelden ilgili UserId'yi aliyoruz

            string token = await _userManager.GeneratePasswordResetTokenAsync(user); // Kendimiz bir token olusturuyoruz

            await _userManager.ResetPasswordAsync(user, token, model.NewPassword); // Sifre degistirme islemini yapiyoruz

            await _userManager.UpdateSecurityStampAsync(user); // Kullaniciyi yeniden giris yapmaya zorluyoruz

            //Securitystamp degerini update yapmazsak, kullanici eski sifresi ile sitemizde dolasmaya devam eder. Ne zaman cikis yaparsa, tekrar o zaman tekrar yeni sifreyle girmek zorunda kalir. Eger yukaridaki gibi biz bu degeri update edersek, kullanici otomatik olarak sitemize geldiginde login ekranina yönlendirilecektir.

            return RedirectToAction("Users"); //Sonrasinda Üyeler sayfamiza kullaniciyi yönlendiriyoruz
        }
    }
}
