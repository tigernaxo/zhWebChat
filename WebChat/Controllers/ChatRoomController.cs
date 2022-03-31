using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebChat.BLL;
using WebChat.Hubs;
using WebChat.Models;

namespace WebChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    //[Authorize(Roles = "User,Anonymous")]
    public class ChatRoomController : ControllerBase
    {

        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
        private readonly IHubContext<ChatHub, IChatHub> hubContext;
        private readonly IOptions<APIOptions> apiOptions;
        private readonly ChatHubFactory chatHubFactory;
        private readonly ChatRoomFactory chatRoomFactory;
        private readonly ChatMsgFactory chatMsgFactory;
        private readonly ChatRoomUserFactory chatRoomUserFactory;
        private readonly UserAnonymousFactory userAnonymousFactory;
        private readonly zhUtil.JWT jwt;
        public ChatRoomController(
            IHubContext<ChatHub, IChatHub> _hubContext,
            IOptions<APIOptions> _apiOptions,
            ChatHubFactory _chatHubFactory,
            ChatMsgFactory _chatMsgFactory,
            ChatRoomFactory _chatRoomFactory,
            ChatRoomUserFactory _chatRoomUserFactory,
            UserAnonymousFactory _userAnonymousFactory,
            zhUtil.JWT _jwt
        )
        {
            hubContext = _hubContext;
            apiOptions = _apiOptions;
            chatHubFactory = _chatHubFactory;
            chatMsgFactory = _chatMsgFactory;
            chatRoomFactory = _chatRoomFactory;
            chatRoomUserFactory = _chatRoomUserFactory;
            userAnonymousFactory = _userAnonymousFactory;
            jwt = _jwt;
        }
        [HttpPost("Template")]
        public async Task<IActionResult> Template()
        {
            try
            {
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                });
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
        [HttpPost("GroupQRCode")]
        public async Task<IActionResult> GroupQRCode()
        {
            try
            {
                // 取得使用者和聊天室 id
                var userId = Convert.ToInt32(User.Identity.Name);
                var roomId = Convert.ToInt32(Request.Form["roomId"]);
                var room = chatRoomFactory.Get(roomId);
                // 判斷該聊天室是否為群組聊天室
                if (!chatRoomFactory.isGroup(room))
                    throw new Exception("此聊天室非群組聊天室");
                // 判斷使用者是否為該群組管理員
                if (!chatRoomUserFactory.UserisAdmin(userId, roomId))
                    throw new Exception("使用者非聊天室管理員");
                // 取得新的 anonymous userId
                var userAnonymous = userAnonymousFactory.Add(UserAnonymousFactory.InviteType.Group, roomId);
                chatRoomUserFactory.RoomAddPerson(roomId, userAnonymous.id, UserFactory.UserType.Anonymous);
                var anonymousId = userAnonymous.id;

                #region 產生一組隨拋的 token，儲存在 A_UserAnonymous
                // 設定要加入到 JWT Token 中的聲明資訊(Claims)
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("roles", "Anonymous"));
                claims.Add(new Claim("id", Convert.ToString(anonymousId)));
                claims.Add(new Claim("issuerId", Convert.ToString(userId)));
                claims.Add(new Claim("userType", "2"));
                claims.Add(new Claim("roomType", Convert.ToString(room.type)));
                claims.Add(new Claim("roomId", Convert.ToString(room.id)));
                var token = jwt.GET(claims, Convert.ToString(anonymousId));
                #endregion
                // P.S. token 到前端才產生 QRCode
                #region
                // todo: 通知該通知的人員
                var list = chatHubFactory.roomConIdMap[roomId].ToArray();
                var user = new
                {
                    id = userAnonymous,
                    loginId = "",
                    photo = "",
                    userName = "未知",
                    userType = 2
                };
                hubContext.Clients.Clients(list).UserAdd(user);
                #endregion
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    token
                });
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
        [HttpPost("OneToOneQRCode")]
        public async Task<IActionResult> OneToOneQRCode()
        {
            try
            {
                // 取得使用者和聊天室 id
                var userId = Convert.ToInt32(User.Identity.Name);
                // 取得新的 anonymous userId
                var userAnonymous = userAnonymousFactory.Add(UserAnonymousFactory.InviteType.Personal);
                var anonymousId = userAnonymous.id;

                #region 產生一組隨拋的 token，儲存在 A_UserAnonymous
                // 設定要加入到 JWT Token 中的聲明資訊(Claims)
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("roles", "Anonymous"));
                claims.Add(new Claim("id", Convert.ToString(anonymousId)));
                claims.Add(new Claim("issuerId", Convert.ToString(userId)));
                claims.Add(new Claim("userType", "2"));
                claims.Add(new Claim("roomType", "3"));
                var token = jwt.GET(claims, Convert.ToString(anonymousId));
                #endregion
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    token
                });
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
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                // 檔案大小限制(byte)
                var fileSizeLimitByte = 10 * 1024 * 1024; // 10mb
                                                          // 取得使用者和聊天室 id
                var userId = Convert.ToInt32(User.Identity.Name);
                int roomId = Convert.ToInt32(Request.Form["roomId"]);
                int userType = 1;
                IFormFile file = Request.Form.Files[0];

                // 檔案檢查 
                if (file == null)
                    throw new Exception("沒有檔案");
                if (file.Length > fileSizeLimitByte)
                    throw new Exception("File Size Excess 10 MByte");

                // 存入 File System、ChatMsg
                // 判斷是 image ?
                //var isImage = ImageExtensions.Contains(Path.GetExtension(file.FileName).ToUpperInvariant());
                //ChatMsg chatMsg = isImage 
                //    ?  chatMsgFactory.AddImage(roomId, userId, userType, file) 
                //    : chatMsgFactory.AddFile(roomId, userId, userType, file);
                ChatMsg chatMsg =  chatMsgFactory.AddFile(roomId, userId, userType, file);

                // 以 ChatMsg 通知群組內的人
                var clients = chatHubFactory.RoomGetConId(roomId).ToArray();
                hubContext.Clients.Clients(clients).MessageReceive(chatMsg);

                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                });
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
        //[HttpGet("File/{roomId}/{hashName}")]
        //public async Task<IActionResult> File(int roomId, string hashName)
        //{
        //    //var chatMsg = chatMsgFactory.Get(id);
        //    return File(
        //        //$"room/{chatMsg.roomId}/{chatMsg.hashName}", 
        //        $"room/{roomId}/{hashName}",
        //        "image/jpeg"
        //        //chatMsg.fileName
        //        ); 
        //}
    }
}
