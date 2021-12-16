using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CoreIdentity
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //Service'lerimizin eklenecegi alan
            services.AddMvc(option => option.EnableEndpointRouting = false);
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
            app.UseStaticFiles(); //js,css dosyalarimizin yüklenebilmesi icin eklenir
            app.UseMvcWithDefaultRoute(); // Arka planda Controller/Action/{id} isimli bir default route olusturur. Ayrica yukariya ConfigureServices altinda parametreli Mvc servisi yüklenmelidir.
            
            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
