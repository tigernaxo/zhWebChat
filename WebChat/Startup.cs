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
            // 內建的 MVC 服務
            services.AddControllersWithViews();
            // 啟用 runtime compile...才能熱更新 cshtml
            // 需要額外安裝套件 Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
            //services.AddRazorPages()
            //    .AddRazorRuntimeCompilation();

            // 注入 SignalR、Cors 服務
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

            // 搭配 vue router history mode 重寫路徑請求
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

            app.UseDefaultFiles(); // 預設 index.html 被傳送
            app.UseHttpsRedirection();  // ???
            app.UseStaticFiles(); // 提供 wwwRoot 靜態內容

            app.UseRouting();  // ???

            app.UseAuthentication(); // 使用者認證
            app.UseAuthorization(); // 使用者授權

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<Hubs.ChatHub>("/chathub");
                endpoints.MapHub<Hubs.AdminHub>("/adminhub");
                // MapControllers 使 Controller 路由屬性(ex:[Route], [HttpGet])生效
                endpoints.MapControllers();
                // 自動將請求網址映射到 MVC Controller/Action，預設以 HomeController.Index 為首頁
                endpoints.MapDefaultControllerRoute();
                // 要更改首頁的話就改用下面的敘述
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
