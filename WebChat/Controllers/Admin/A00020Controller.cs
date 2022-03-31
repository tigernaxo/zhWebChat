using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class A00020Controller : ControllerBase
    {
        private readonly string conStr;
        private readonly IOptions<APIOptions> apiOptions;
        public A00020Controller(IOptions<APIOptions> _apiOptions, IOptions<DALOptions> DALOptions)
        {
            apiOptions = _apiOptions;
            conStr = DALOptions.Value.ConnectionString;
        }
        // 取得對話資訊
        public class SearchArg
        {
            public string? title { get; set; }
            public string? loginId { get; set; }
            public string? userName { get; set; }
            //public DateTime? timeStart { get; set; }
            //public DateTime? timeEnd { get; set; }
            public string? text { get; set; }
            public int type { get; set; }
            public string? fileName { get; set; }
        }
        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] SearchArg obj)
        {
            try
            {
                // 解析資料
                using (var cn = new SqlConnection(conStr))
                {
                    var sql = @"
                        select t.*, t1.title, t2.loginId, t2.userName from A_ChatMsg t 
                        left join A_ChatRoom t1 on t.roomId = t1.id
                        left join S10_users t2 on t.userId=t2.id and t.userType=1
                        WHERE 
                            (@text IS NULL OR t.text like '%' + @text + '%') AND
                            (@type IS NULL OR t.type = @type) AND
                            (@fileName IS NULL OR t.fileName like '%' + @fileName + '%') AND
                            (@title IS NULL OR t1.title like '%' + @title + '%') AND
                            (@loginId IS NULL OR t2.loginId like '%' + @loginId + '%') AND
                            (@userName IS NULL OR t2.userName like '%' + @userName + '%') 
                    ;";
                    var chatMsgs = cn.Query<dynamic>(sql, obj).ToArray();
                    return Ok(new // 完成
                    {
                        resultCode = apiOptions.Value.SuccessCode,
                        error = "",
                        chatMsgs = chatMsgs,
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
                    var sql = @" UPDATE A_ChatMsg SET status=@status WHERE id in @ids";
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
