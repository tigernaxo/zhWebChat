using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebChat.BLL;

namespace WebChat.Controllers.Admin
{
    public class SignInModel
    {
        public string loginId { get; set; }
        public string password { get; set; }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private UserFactory userFactory;
        private ChatHubFactory chatHubFactory;
        private ConnectFactory connectFactory;
        private IOptions<APIOptions> apiOptions;

        public AdminController(
            IOptions<APIOptions> _apiOptions, 
            UserFactory _userFactory, 
            ChatHubFactory _chatHubFactory,
            ConnectFactory _connectFactory
            )
        {
            apiOptions = _apiOptions;
            userFactory = _userFactory;
            chatHubFactory = _chatHubFactory;
            connectFactory = _connectFactory;
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(SignInModel signIn)
        {
            try
            {
                // 驗證 user
                var user = userFactory.Get(signIn.loginId, signIn.password);
                if (user == null)
                    throw new Exception("使用者驗證失敗");
                if (!userFactory.isUserAdmin(user.id))
                    throw new Exception("使用者非管理員");

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
        [AllowAnonymous]
        [HttpGet("getConTime")]
        public IActionResult GetConTime()
        {
            try
            {
                // 取得 userId
                var userId = Convert.ToInt32(User.Identity.Name);
                var now = DateTime.Now;
                double dTotalSec = 0;
                // 取得資料庫 A_connectInfoDay 的秒數
                dTotalSec += connectFactory.GetDaySettleSec(now.ToString("yyyyMMdd"));
                // 計算記憶體中連線資訊的秒數
                foreach(var info in chatHubFactory.connectInfos.Values)
                {
                    var isTodayStart = DateTime.Compare(info.startTime.Date, DateTime.Now.Date) == 0;
                    dTotalSec += isTodayStart 
                        ? (now - info.startTime).TotalSeconds  // 如果是今天開始的連線，就加入連線開始到目前的秒數
                        : (now - now.Date).TotalSeconds;       // 如果不是今天開始的連線，就加入今天凌晨到現在的秒數
                }

                return Ok(new
                {
                    totalSec = Convert.ToInt64(dTotalSec),
                    updateTime = now,
                    resultCode = apiOptions.Value.SuccessCode,
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = "取得連線時間時時發生錯誤：" + ex.Message + ";",
                    resultCode = apiOptions.Value.FailtureCode,
                });
            }
        }
    }
}
