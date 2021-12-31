using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CoreIdentity.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _UserManager = userManager;
        }

        public UserManager<AppUser> _UserManager { get; set; }


        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)// principal!=null : kullanicinin authenticate mi degil mi oldugunu, principal.Identity.IsAuthenticated burada ise kullanicinin bir kimlik kartinin olup olmadigini kontrol ediyoruz
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity; // User dan (principal) gelen kimligi ClaimsIdentity as key'i ile ceviriyoruz(casting).

                AppUser user = await _UserManager.FindByNameAsync(identity.Name);

                if (user != null)
                {
                    if (user.BirthDay != null)
                    {
                        var today = DateTime.Today;
                        var age = today.Year - user.BirthDay?.Year;
                        bool status = false;

                        if (age > 15)
                        {
                            Claim ViolenceClaim = new Claim("violence", true.ToString(), ClaimValueTypes.String, "Internal"); // Cookie icerisindeki parametreleri dolduruyoruz.

                            identity.AddClaim(ViolenceClaim);
                        }
                    }

                    if (user.City != null)
                    {
                       if (!principal.HasClaim(c => c.Type == "city"))
                       {
                           Claim CityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");// Cookie icerisindeki parametreleri dolduruyoruz.

                           identity.AddClaim(CityClaim);
                       }
                    }
                }
            }
            return principal;
        }

    }
}
