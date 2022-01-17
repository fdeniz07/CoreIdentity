using CoreIdentity.CustomValidations;
using CoreIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

            //Claim Based Authorization Ayarlarimiz
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("AnkaraPolicy", policy =>
                {
                    policy.RequireClaim("city", "ankara");
                });
                opt.AddPolicy("ViolencePolicy", policy =>
                {
                    policy.RequireClaim("violence"); //istersek key alanini yazar, value alanini belirtmeyebiliriz
                });
                opt.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirement());
                });
            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>(); //Claim islemlerimiz icin bu dönüsümü yapiyor

            //Sosyal Medya Login islemleri
            services.AddAuthentication().AddFacebook(opt =>
            {
                opt.AppId = configuration["Authentication:Facebook:AppId"];
                opt.AppSecret = configuration["Authentication:Facebook:AppSecret"];
            }).AddGoogle(opt =>
            {
                opt.ClientId = configuration["Authentication:Google:ClientID"];
                opt.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            }).AddMicrosoftAccount(opt =>
            {
                opt.ClientId = configuration["Authentication:Microsoft:ClientID"];
                opt.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
            });


            //Identity Ayarlarimiz
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

            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders(); //AppIdentityDbContext sayesinde AppUser ve IdentityRole tablolarini Db de olusturacak.

            //Service'lerimizin eklenecegi alan
            //services.AddMvc(option => option.EnableEndpointRouting = false); //Asp.Net Core 3.1


            //Cookie Ayarlarimiz
            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = new PathString("/Home/Login"); //Kullanici üye olmadan, üye olmayanlarin erisebildigi sayfaya tiklarsa, Login sayfamiza yönlendirir
                opt.LogoutPath = new PathString("/Member/LogOut"); // MemberController'deki void türündeki LogOut metoduna gidiyor.

                opt.Cookie = new CookieBuilder
                {
                    Name = "MyBlog", //cookie adimiz
                    HttpOnly = false,//Client tarafinda cookie bilgisi okunmasin
                    SameSite = SameSiteMode.Lax, // Lax: default olarak gelir. Anasitemiz üzerinden subdomaine ait bir sitemize tek bir cookie ile gecilmesine olanak verir.
                                                 //Strict : Bu mode mali,finansal uygulamalar icin secilir ve Siteler arasi istek sahtekarligina (Cross Site Request Forgery - CSRF/XSRF - Session Riding) karsi önlem olarak kullanilir. Bu sayede Client - Sunucu arasinda cookie'ye müdahale edilmesine izin vermez. Subdomanin yapisinda kullanilmaz.
                    SecurePolicy = CookieSecurePolicy.SameAsRequest //Site canliya tasindiginda bu alan .Always olarak degistirilmelidir.!!!
                    //Always : Browser, kullanicinin cookie'sini sadece  Https üzerinden bir istek geldiginde gönderir
                    //SameAsRequest : Browser, kullanicinin cookie istegini hangi protokolden geldiyse ona o sekilde gönderir (Http den geldiyse http den, https den geldiyse https den)
                    //None: Browser, protokole bakmadan tüm gelen isteklere http üzerinden gönderir
                };
                opt.SlidingExpiration = true; // Expiration süresi bitmedigi sürece kullanici hangi gün tekrar siteyi ziyaret ederse, Expiration süresi kadar üzerine süre eklenir
                opt.ExpireTimeSpan = System.TimeSpan.FromDays(7); // cookie yasam süresi (7 gün sonra tekrar login olunmasi gerekli)
                opt.AccessDeniedPath = new PathString("/Member/AccessDenied"); //Üye bir kullanici yetkisiz sayfaya erismeye kalkarsa yönlendirilecek sayfa
            });

           

            services.AddControllersWithViews().AddRazorRuntimeCompilation(); //(AddRazorRuntimeCompilation()) Bu sayede backend de yapilan degisiklerde tekrar tekrar uygulamayi derlememize ihtiyac kalmiyor. Yani frontend deki gibi kaydettikten sonra uygulamadaki degisiklikleri görebiliriz.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Middleware'lerimizi bu kisma ekliyoruz


            //Test (Development Ortam Kodlari asagiya yazilir)
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();//Sayfada bir hata alindiginda aciklayici bilgiler sunar.
            //    app.UseStatusCodePages(); //Herhangi bir content(icerik) dönmeyen saflarimizda bizi bilgilendirici yazilar dönmesini saglar
            //}

            app.UseDeveloperExceptionPage(); //Sayfada bir hata alindiginda aciklayici bilgiler sunar.
            app.UseStatusCodePages(); //Herhangi bir content(icerik) dönmeyen saflarimizda bizi bilgilendirici yazilar dönmesini saglar
            app.UseStaticFiles(); //resim,js,css dosyalarimizin yüklenebilmesi icin eklenir

            app.UseRouting(); // Authentication ve Authorization dan önce gelmeli

            app.UseAuthentication(); //Authorization dan önce gelmeli
            app.UseAuthorization();

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
