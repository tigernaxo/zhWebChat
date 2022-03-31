using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebChat.BLL;
using WebChat.Hubs;
using WebChat.Models;

namespace WebChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly IHubContext<ChatHub, IChatHub> hubContext;
        private readonly IOptions<APIOptions> apiOptions;
        private readonly ChatHubFactory chatHubFactory;
        private readonly UserFactory userFactory;
        private readonly ChatRoomFactory chatRoomFactory;
        private readonly ChatRoomUserFactory chatRoomUserFactory;
        public UserController(
            IHubContext<ChatHub, IChatHub> _hubContext, 
            IOptions<APIOptions> _apiOptions, 
            ChatHubFactory _chatHubFactory,
            UserFactory _userFactory,
            ChatRoomFactory _chatRoomFactory,
            ChatRoomUserFactory _chatRoomUserFactory
            )
        {
            apiOptions = _apiOptions;
            hubContext = _hubContext;
            chatHubFactory = _chatHubFactory;
            userFactory = _userFactory;
            chatRoomFactory = _chatRoomFactory;
            chatRoomUserFactory = _chatRoomUserFactory;
        }
        // 取得使用者資訊
        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    user = "",
                    userHead = ""
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
        [HttpPost("Update")]
        public async Task<IActionResult> Update()
        {
            try
            {
                // 檔案大小限制(byte)
                var fileSizeLimitByte = 200 * 1024;
                var userId = Convert.ToInt32(User.Identity.Name);
                var oldName = (User.Identity as ClaimsIdentity).FindFirst(c => c.Type == "userName").Value;

                // 嘗試更新使用者名稱
                string? userName = Request.Form["userName"].ToString();
                bool userNameChanged = userName.Length != 0;
                userName = !userNameChanged ? oldName : userName;
                userFactory.Update(userId, userName);

                // 嘗試更新大頭照
                IFormFile userHead = Request.Form.Files["headFile"];
                string? userPhoto = null;
                if (userHead != null)
                {
                    // 檢查檔案大小
                    if (userHead.Length > fileSizeLimitByte)
                        throw new Exception("File Size Excess 200 KByte");
                    // 儲存檔案並回傳所實際儲存的hash檔案名稱
                    userPhoto = userFactory.HeadImageSave(userId, userHead);
                }

                // 如果有更動使用者名稱或頭像，就通知該通知的人
                if (userNameChanged || userHead != null)
                {
                    var user = new
                    {
                        id = userId,
                        userName,
                        photo = userPhoto,
                    };
                    // 從資料庫找出要通知的使用者 id
                    var userIdList = userFactory.GetAllAvaiable(userId).Select(x => x.id);
                    foreach (var id in userIdList)
                    {
                        // 找出這些 id 的 connectionId // 用 hubContext.Clients 逐一通知
                        if (chatHubFactory.userMap.ContainsKey(id))
                        {
                            var list = chatHubFactory.PersonGetConIdArr(id, 1);
                            await hubContext.Clients.Clients(list).UserUpdate(user);
                        }
                    }
                }

                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = ""
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

        [HttpPost("SearchByLoginId")]
        public async Task<IActionResult> SearchByLoginId([FromBody] JsonElement body)
        {
            try
            {
                var loginId = body.GetProperty("loginId").GetString();
                var userId = Convert.ToInt32(User.Identity.Name);
                var user = userFactory.Search(userId, loginId);
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    user
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
        [HttpPost("SearchByLoginIdFuzzy")]
        public async Task<IActionResult> SearchByLoginIdFuzzy([FromBody] JsonElement body)
        {
            try
            {
                var loginId = body.GetProperty("loginId").GetString();
                var userId = Convert.ToInt32(User.Identity.Name);
                var userList = userFactory.SearchFuzzy(userId, loginId);
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    userList
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

        [HttpPost("AddContact")]
        public async Task<IActionResult> AddContact([FromBody] JsonElement body)
        {
            try
            {
                var userId = Convert.ToInt32(User.Identity.Name);
                var userId2 = body.GetProperty("id").GetInt32();
                var obj = userFactory.ContactAdd(userId, userId2);
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    obj.user,
                    obj.userRelationShip
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
        [HttpPost("DelContact")]
        public async Task<IActionResult> DelContact([FromBody] JsonElement body)
        {
            try
            {
                var userId = Convert.ToInt32(User.Identity.Name);
                var userId2 = body.GetProperty("id").GetInt32();
                userFactory.ContactDel(userId, userId2);
                // todo 通知該通知的使用者
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

        [HttpPost("BanUser")]
        public async Task<IActionResult> BanUser([FromBody] JsonElement body)
        {
            try
            {
                var userId = Convert.ToInt32(User.Identity.Name);
                var userId2 = body.GetProperty("id").GetInt32();
                var result = userFactory.Ban(userId, userId2);
                // 找出兩個使用者的 1v1 聊天室
                var oneToOneRoom = chatRoomFactory.GetOneToOne(userId, userId2, 1);
                if (oneToOneRoom != null)
                {
                    // 從 roomConIdMap 刪除使用者監聽(對方就傳不進去)
                    var userConIds = chatHubFactory.userMap[userId].conIdSet;
                    if (chatHubFactory.roomConIdMap.ContainsKey(oneToOneRoom.id))
                    {
                        chatHubFactory.roomConIdMap[oneToOneRoom.id]
                            .RemoveWhere(conId =>
                            {
                                return userConIds.Contains(conId);
                            });
                    }
                }
                // 通知所有上線的 session 封鎖對方
                await hubContext.Clients.Clients(
                    chatHubFactory.PersonGetConIdArr(userId, 1)
                    ).BanUser(result);
                // 通知封鎖方被誰封鎖
                await hubContext.Clients.Clients(
                chatHubFactory.PersonGetConIdArr(userId2, 1)
                    ).Baned(userId);
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    result
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
        [HttpPost("UnBanUser")]
        public async Task<IActionResult> UnBanUser([FromBody] JsonElement body)
        {
            try
            {
                var userId = Convert.ToInt32(User.Identity.Name);
                var userId2 = body.GetProperty("id").GetInt32();
                userFactory.UnBan(userId, userId2);
                // 找出兩個使用者的 1v1 聊天室
                var oneToOneRoom = chatRoomFactory.GetOneToOne(userId, userId2, 1);
                if (oneToOneRoom != null)
                {
                    // 從 roomConIdMap 添加使用者監聽(對方傳得進去)
                    var userConIds = chatHubFactory.userMap[userId].conIdSet;
                    if ( chatHubFactory.roomConIdMap.ContainsKey(oneToOneRoom.id) )
                    {
                        chatHubFactory.roomConIdMap[oneToOneRoom.id]
                            .UnionWith(userConIds);
                    }
                }
                // 通知所有上線的 session 解除封鎖對方
                await hubContext.Clients.Clients(
                    chatHubFactory.PersonGetConIdArr(userId, 1)
                    ).UnBanUser(userId2);
                // 通知對方被解除封鎖?
                await hubContext.Clients.Clients(
                chatHubFactory.PersonGetConIdArr(userId2, 1)
                    ).UnBaned(userId);
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
        [HttpPost("InviteUser")]
        public async Task<IActionResult> InviteUser([FromBody] JsonElement body)
        {
            try
            {
                var userId = Convert.ToInt32(User.Identity.Name);
                // 目標使用者Id
                var userId2 = body.GetProperty("userId").GetInt32();
                // 找出聊天室
                var roomId = body.GetProperty("roomId").GetInt32();
                var room = chatRoomFactory.Get(roomId);

                // 加入
                var chatRoomUser = chatRoomUserFactory.RoomAddPerson(roomId, userId2, UserFactory.UserType.User);
                var user2ConIds = chatHubFactory.userMap[userId2].conIdSet;
                if (room != null)
                {
                    // 將對象使用者的連線 Id 添加到房間監聽
                    if (chatHubFactory.roomConIdMap.ContainsKey(roomId))
                    {
                        chatHubFactory.roomConIdMap[roomId].UnionWith(user2ConIds);
                    }
                }
                var user2 = chatHubFactory.userMap[userId2];
                if(user2 != null)
                {
                    user2.roomSet.Add(roomId);
                }
                // 通知所有上線的 session 某人加入群組
                await hubContext.Clients.Clients(
                    chatHubFactory.PersonGetConIdArr(userId, 1)
                    ).ChatRoomAddUser(chatRoomUser);
                // 通知對方被加入群組
                await hubContext.Clients.Clients(
                chatHubFactory.PersonGetConIdArr(userId2, 1)
                    ).ChatRoomAdd(room, chatRoomUserFactory.Get(roomId));
                return Ok(new // 完成
                {
                    resultCode = apiOptions.Value.SuccessCode,
                    error = "",
                    chatRoomUser
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
    }
}
