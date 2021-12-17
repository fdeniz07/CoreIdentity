using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreIdentity.Models
{
    public class AppIdentityDbContext:IdentityDbContext<AppUser,AppRole,string> //Kullanici id alanlarinin türü string seciyoruz
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options):base(options)
        {

        }
    }
}
