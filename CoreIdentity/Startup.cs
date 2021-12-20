using CoreIdentity.CustomValidations;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreIdentity
{
    public class Startup
    {
        public IConfiguration configuration { get; } // Ayarlarimizi alabilmek icin IConfiguration dan bir property olusturup, constructordan erisebiliriz

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;  //Bu sayede appsettings.json dosyasindaki ayarlara erisebiliriz
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(opt =>
            {
                opt.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionString"]); //ConncetionString'i configuration araciligi ile appsettings.json'dan aliyoruz
            });

            services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequiredLength = 4; //sifre de rakam olsun mu? Test asamasinda kapatiyoruz, Default 6 karakter
                opt.Password.RequiredUniqueChars = 0; //kac adet özel karakter türü olsun?
                opt.Password.RequireNonAlphanumeric = false; //aktif oldugunda özel karakterlerin kullanilmasini saglar
                opt.Password.RequireLowercase = false; //kücük harf kullanilmasi zorunlulugu olsun mu?
                opt.Password.RequireUppercase = false; //büyük harf kullanilmasi zorunlulugu olsun mu?

                //User Username and Email Options
                opt.User.AllowedUserNameCharacters = "abcçdefgğhıijklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._@+$"; //Kullanici adinda hangi karakterler olsun
                opt.User.RequireUniqueEmail = true; // ayni mail adresi ile kayda izin verilmemeli mi?

            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddEntityFrameworkStores<AppIdentityDbContext>(); //AppIdentityDbContext sayesinde AppUser ve IdentityRole tablolarini Db de olusturacak.

            //Service'lerimizin eklenecegi alan
            //services.AddMvc(option => option.EnableEndpointRouting = false); //Asp.Net Core 3.1
            services.AddControllersWithViews().AddRazorRuntimeCompilation(); //(AddRazorRuntimeCompilation()) Bu sayede backend de yapilan degisiklerde tekrar tekrar uygulamayi derlememize ihtiyac kalmiyor. Yani frontend deki gibi kaydettikten sonra uygulamadaki degisiklikleri görebiliriz.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Middleware'lerimizi bu kisma ekliyoruz


            //Test (Development Ortam Kodlari asagiya yazilir)
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseDeveloperExceptionPage(); //Sayfada bir hata alindiginda aciklayici bilgiler sunar.
            app.UseStatusCodePages(); //Herhangi bir content(icerik) dönmeyen saflarimizda bizi bilgilendirici yazilar dönmesini saglar
            app.UseStaticFiles(); //resim,js,css dosyalarimizin yüklenebilmesi icin eklenir
            
            app.UseRouting(); // Authentication ve Authorization dan önce gelmeli

            app.UseAuthentication(); //Authorization dan önce gelmeli
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=index}/{id?}"
                    );

                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
