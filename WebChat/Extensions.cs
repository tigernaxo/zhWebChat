using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebChat
{
    public static class AuthenticationExtensions
    {
        // 注入設定好的 JWT Authencation 服務
        public static IServiceCollection zhAddJWTAuth([NotNull] this IServiceCollection services, IConfiguration conf)
        {
            // Reference: https://docs.microsoft.com/zh-tw/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1
            // Token 可參考  https://blog.miniasp.com/post/2019/12/16/How-to-use-JWT-token-based-auth-in-aspnet-core-31
            // 注入驗證服務
            services.AddAuthentication(options =>
            {
                // 設定使用者預設的 Authenticate、authentication challenge 的 Scheme 都設定為 JwtBearerDefaults.AuthenticationScheme
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                // 設定授權後將 bearer token 儲存在 AuthenticationProperties 中 
                options.SaveToken = true;
                // 設定 token 的驗證參數
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // 設定 userId Claim 作為 User.Identity.Name
                    //NameClaimType = JwtRegisteredClaimNames.Sub,
                    NameClaimType = "id",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(conf.GetValue<string>("Jwt:SignKey"))),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                // 在 WebSocket 發起連線時解析 access_token 網址參數放到 context 當中
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/chathub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // 將獲取 User.Identity.Name 的實體類別注入到 IUserIdProvider 介面
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            return services;
        }
        // 注入自訂義的 Options Pattern
        public static IServiceCollection zhAddOptions([NotNull] this IServiceCollection services, IConfiguration conf)
        {
            // 注入 DALOptions、APIOtions、JwtOptions
            services.AddOptions<DALOptions>()
                .Bind(conf.GetSection(DALOptions.DAL));
            services.AddOptions<APIOptions>()
                .Bind(conf.GetSection(APIOptions.API));
            services.AddOptions<JwtOptions>()
                .Bind(conf.GetSection(JwtOptions.JWT));
            return services;
        }
        public static IServiceCollection zhAddBLL ([NotNull] this IServiceCollection services)
        {
            services.AddTransient<BLL.UserFactory>();
            services.AddTransient<BLL.UserAnonymousFactory>();
            services.AddTransient<BLL.ConnectFactory>();
            services.AddTransient<BLL.ChatMsgFactory>();
            services.AddTransient<BLL.ChatRoomFactory>();
            services.AddTransient<BLL.ChatRoomUserFactory>();
            services.AddSingleton<BLL.ChatHubFactory>(); // ChatHubFactory 是 FSM 所以必須要用 Singleton 方式注入記憶狀態
            return services;
        }
        public static IServiceCollection zhAddUtil ([NotNull] this IServiceCollection services)
        {
            services.AddTransient<zhUtil.JWT>();
            return services;
        }
    }
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var claims = ((ClaimsIdentity)connection.User.Identity).Claims;
            //return claims.FirstOrDefault(claim => claim.Type == "userId")?.Value;
            return connection.User?.Identity?.Name;
        }
    }
    public class DALOptions
    {
        public const string DAL = "DAL";
        public string ConnectionString { set; get; }
    }
    public class APIOptions
    {
        public const string API = "API";
        public string SuccessCode { set; get; }
        public string FailtureCode { set; get; }
    }
    public class JwtOptions
    {
        public const string JWT = "JWT";
        public string Issuer { get; set; }
        public string SignKey { get; set; }
        public int Expires { get; set; }
    }
}
