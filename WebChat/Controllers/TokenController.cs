using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// Example of Token Controllers
namespace WebChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private BLL.UserFactory userFactory;
        private IOptions<APIOptions> apiOptions;

        public TokenController(IOptions<APIOptions> _apiOptions, BLL.UserFactory _userFactory)
        {
            this.apiOptions = _apiOptions;
            this.userFactory = _userFactory;
        }
        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(SignInModel signIn)
        {
            try
            {
                // 驗證 user
                var user = userFactory.Get(signIn.userId, signIn.password);
                if (user == null)
                {
                    throw new Exception("使用者驗證失敗");
                }
                var jwt_token = userFactory.GetToken(user);

                return Ok(new
                {
                    token = jwt_token,
                    resultCode = apiOptions.Value.SuccessCode,
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = "驗證身分時發生錯誤：" + ex.Message + ";",
                    resultCode = apiOptions.Value.FailtureCode,
                });
            }
        }
        public class SignInModel
        {
            public string userId { get; set; }
            public string password { get; set; }
        }
    }
}
