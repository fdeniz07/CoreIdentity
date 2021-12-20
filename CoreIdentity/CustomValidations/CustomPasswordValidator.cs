using System.Collections.Generic;
using System.Threading.Tasks;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Identity;

namespace CoreIdentity.CustomValidations
{
    public class CustomPasswordValidator:IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            //override ettiğimiz asenkron metodun başında neden async keywordü yok ? sadece Task yazarak asenkron yapabiliyor muyuz ?

            //El Cevap: Asekron methodlarda, geriye bir TASK instance dönülecekse; async keyword'un kullanılmasına gerek yoktur. Eğer async methodun içerisinde await keyword'u ile asekron methodlar çağrılrmak istenirse kullanilmak zorundadır.


            List<IdentityError> errors = new List<IdentityError>();

            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                if (!password.ToLower().Contains(user.Email.ToLower()))
                {
                    errors.Add(new IdentityError(){Code = "PasswordContainsUserName",Description = "sifre alani kullanici adi iceremez"});
                }
            }

            if (password.ToLower().Contains("1234"))
            {
                errors.Add(new IdentityError() { Code = "PasswordContains1234", Description = "sifre alani ardisik sayi iceremez" });
            }

            if (password.ToLower().Contains(user.Email.ToLower()))
            {
                errors.Add(new IdentityError() { Code = "PasswordContainsEmail", Description = "sifre alani email adresinizi iceremez" });
            }

            // Daha fazla kosul yazilabilir. Mesala; split ile email adresini '@' adresinden parçalayıp sorgulayabiliriz. Sifre icerinde dogum tarihi vb icermesine izin vermeyebiliriz.Bu tamamen gelistiriciye ve kurum politikasina kalmistir.

            if (errors.Count==0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }
    }
}
