using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CoreIdentity
{
    public class ExpireDateExchangeRequirement : IAuthorizationRequirement
    {
        //Gövdesini bos birakiyoruz, çünkü Startup.cs içerisinde bu sinifin adini kisitlama adi olarak belirteceğiz.
    }

    public class ExpireDateExchangeHandler : AuthorizationHandler<ExpireDateExchangeRequirement> // Bu Handler bizden IAuthorizationRequirement tipinde sinif ister
    {
        /*Bu Handler'in Amaci
         *
         * Kullanici MemberController icerisinde daha index'e (ilgili action'a) gelmeden Authentication ve Authorization adimlarinda, bizim degerlerimizi de kontrol edecektir.
         * Bu yüzden kendimiz custom bir handler yaziyoruz.
         */

        //Handler’in miras aldigi sinifin bir abstract bir metodu var. Bizim bunu override etmemiz gerekecektir.

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExpireDateExchangeRequirement requirement)
        {
            if (context.User != null & context.User.Identity != null) //Böyle bir kullanici ve Kimlik var mi kontrolü yapiyoruz.
            {
                var claim = context.User.Claims.Where(x => x.Type == "ExpireDateExchange" && x.Value != null).FirstOrDefault(); //Böyle bir claim ve degeri var mi kontrol ediyoruz. Bu degerden biz tarih karsilastirmasi yapacagiz.

                if (claim != null)
                {
                    if (DateTime.Now < Convert.ToDateTime(claim.Value)) //tarih degeri DB de string olarak tutuldugu icin convert islemi yapiyoruz. Eger giris tarihi bugünpn tarihinden kücükse (30Gün sartimiz) bu bloga takilacak
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail(); //Dogrulama Basarisiz olursa, kullaniciyi yönlendirecegiz.
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
