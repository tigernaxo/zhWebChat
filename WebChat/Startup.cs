using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebChat
{
    public class Startup
    {
        public Startup(IConfiguration conf, IWebHostEnvironment env)
        {
            this.conf = conf;
            this.env = env;
        }

        public IConfiguration conf { get; }
        public IWebHostEnvironment env { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ���ت� MVC �A��
            services.AddControllersWithViews();
            // �ҥ� runtime compile...�~�����s cshtml
            // �ݭn�B�~�w�ˮM�� Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
            //services.AddRazorPages()
            //    .AddRazorRuntimeCompilation();

            // �`�J SignalR�BCors �A��
            services.AddSignalR();
            if (env.IsDevelopment())
            {
                services.AddCors();
            }

            services.zhAddJWTAuth(conf); // inject Bearer Token Auth service
            services.zhAddOptions(conf); // inject Options pattern
            services.zhAddBLL(); // inject BLL Factories
            services.zhAddUtil(); // inject zhUtil
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(builder =>
                {
                    builder.WithOrigins("http://localhost:8080", "http://127.0.0.1:8080", "http://localhost:3000")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST")
                        .AllowCredentials();
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // �f�t vue router history mode ���g���|�ШD
            app.Use(async (context, next) =>
            {
                await next();
                string reqPath = context.Request.Path.Value;
                bool is404 = context.Response.StatusCode == 404;
                if (is404 && !Path.HasExtension(reqPath))
                {
                    string prefix = reqPath.Split("/")[1].ToLower();
                    if (prefix == "admin" || prefix == "chat")
                        context.Request.Path = $"/{prefix}/index.html";
                    await next();
                }
            });

            app.UseDefaultFiles(); // �w�] index.html �Q�ǰe
            app.UseHttpsRedirection();  // ???
            app.UseStaticFiles(); // ���� wwwRoot �R�A���e

            app.UseRouting();  // ???

            app.UseAuthentication(); // �ϥΪ̻{��
            app.UseAuthorization(); // �ϥΪ̱��v

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<Hubs.ChatHub>("/chathub");
                endpoints.MapHub<Hubs.AdminHub>("/adminhub");
                // MapControllers �� Controller �����ݩ�(ex:[Route], [HttpGet])�ͮ�
                endpoints.MapControllers();
                // �۰ʱN�ШD���}�M�g�� MVC Controller/Action�A�w�]�H HomeController.Index ������
                endpoints.MapDefaultControllerRoute();
                // �n��ﭺ�����ܴN��ΤU�����ԭz
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
