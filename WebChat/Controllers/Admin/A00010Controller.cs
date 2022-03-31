using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebChat.Models;

namespace WebChat.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class A00010Controller : ControllerBase
    {
        private readonly string conStr;
        private readonly IOptions<APIOptions> apiOptions;
        public A00010Controller(IOptions<APIOptions> _apiOptions, IOptions<DALOptions> DALOptions)
        {
            apiOptions = _apiOptions;
            conStr = DALOptions.Value.ConnectionString;
        }
        // 取得使用者資訊
        [HttpPost("Search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                // 解析資料
                string loginId = Request.Form["loginId"].ToString();
                string userName = Request.Form["userName"].ToString();
                using (var cn = new SqlConnection(conStr))
                {
                    var sql = @"SELECT id, loginId, userName, phone, photo, status, createTime, actTime
FROM S10_users WHERE loginId like '%' + @loginId + '%' AND userName like '%' + @userName + '%'";
                    var users = cn.Query<dynamic>(sql, new { loginId, userName }).ToArray();
                    return Ok(new // 完成
                    {
                        resultCode = apiOptions.Value.SuccessCode,
                        error = "",
                        users = users,
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    resultCode = apiOptions.Value.FailtureCode,
                    error = ex.Message
                }); ;
            }
        }

        public class SetStatusArg
        {
            public int status { get; set; }
            public int[] ids { get; set; }
        }
        [HttpPost("SetStatus")]
        public async Task<IActionResult> SetStatus([FromBody] SetStatusArg obj)
        {
            try
            {
                // 解析資料
                using (var cn = new SqlConnection(conStr))
                {
                    var sql = @" UPDATE S10_users SET status=@status WHERE id in @ids";
                    cn.Execute(sql, obj);
                    return Ok(new // 完成
                    {
                        resultCode = apiOptions.Value.SuccessCode,
                        error = "",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    resultCode = apiOptions.Value.FailtureCode,
                    error = ex.Message
                }); ;
            }
        }
    }
}
