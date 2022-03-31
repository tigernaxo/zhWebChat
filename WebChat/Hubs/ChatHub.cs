using Microsoft.AspNetCore.SignalR;
using System.Buffers.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Concurrent;
using WebChat.BLL;
using WebChat.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using System.Transactions;
using System.Globalization;

namespace WebChat.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        private ChatHubFactory factory;
        private ChatMsgFactory chatMsgFactory;
        private UserFactory userFactory;
        private ChatRoomFactory chatRoomFactory;
        private ChatRoomUserFactory chatRoomUserFactory;
        private UserAnonymousFactory userAnonymousFactory;
        public ChatHub(
            ChatHubFactory _chatHubFactory,
            ChatMsgFactory _chatMsgFactory,
            UserFactory _userFactory,
            UserAnonymousFactory _userAnonymousFactory,
            ChatRoomFactory _chatRoomFactory,
            ChatRoomUserFactory _chatRoomUserFactory,
            IOptions<DALOptions> DALOptions
            )
        {
            factory = _chatHubFactory;
            chatMsgFactory = _chatMsgFactory;
            userFactory = _userFactory;
            chatRoomFactory = _chatRoomFactory;
            chatRoomUserFactory = _chatRoomUserFactory;
        }
        #region Message 相關操作
        // 傳送文字訊息
        [Authorize(Roles = "User")]
        public Task InitDB()
        {
            return Clients.Caller.InitDB();
        }
        [Authorize(Roles = "User,Anonymous")]
        public Task MessageSend(MessageSendModel model)
        {
            try
            {
                // 取得 userId
                var userId = getUserId(Context);
                var userType = getUserType(Context);

                // 檢查該使用者是否能在該房間發言
                if (!factory.RoomIsAllowPerson(model.roomId, userId, userType))
                {
                    var errStr = $"使用者({userId})不具在房間({model.roomId})傳送訊息的權限";
                    throw new Exception(errStr);
                }

                // 存入資料庫並取得 msg
                var msg = chatMsgFactory.AddText(model.roomId, userId, userType, model.message);
                // 設定 mesasge 已讀
                chatRoomUserFactory.ReadMsgUpdate(userId, userType, model.roomId, msg.id);
                // 決定要發送給誰
                var conIds = factory.RoomGetConId(model.roomId).ToList();
                //Clients.Clients(conIds).Test(new { Context.ConnectionId, conIds });
                return Clients.Clients(conIds).MessageReceive(msg);
            }
            catch (Exception ex)
            {
                return Clients.Caller.Error(ex.Message);
            }
        }
        public class MessageSendModel
        {
            public int roomId { get; set; }
            public string message { get; set; }
        }
        [Authorize(Roles = "User,Anonymous")]
        public Task MessageUserRead(int roomId, int readMsgId)
        {
            // 根據 roomId, userId update ChatRoomUser
            var userId = getUserId(Context); // 取得 userId
            var userType = getUserType(Context);
            chatRoomUserFactory.ReadMsgUpdate(userId, userType, roomId, readMsgId);
            return Clients.Caller.MessageUserReadResponse();
        }
        // todo: 刪除訊息
        [Authorize(Roles = "User")]
        public async Task MessageDelete(MessageDeleteModel model) { }
        [Authorize(Roles = "User")]
        public Task ChatMsgUpdateRequest(string timeStr)
        {
            var time = DateTime.ParseExact(timeStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var result = chatMsgFactory.GetUpdate(time);
            return Clients.Caller.ChatMsgUpdate(result);
        }
        public class MessageDeleteModel { }
        #endregion

        #region ChatRoom 相關操作
        // 取得聊天室訊息(聊天室提示、最近 message)
        [Authorize(Roles = "User,Anonymous")]
        public Task ChatRoomGetAll()
        {
            var userId = getUserId(Context); // 取得 userId
            // 取得 roomList
            var roomList = chatRoomFactory.Get(userId, getUserType(Context));
            return Clients.Caller.ChatRoomGetAllResponse(roomList);
        }
        // 取得聊天室訊息(聊天室提示、最近 message)
        [Authorize(Roles = "User")]
        public Task ChatRoomGet(int roomId)
        {
            var userId = getUserId(Context); // 取得 userId
            // 檢查是不是能取得
            var chatRoomUser = chatRoomUserFactory.Get(userId, 1, roomId);
            if (chatRoomUser == null || chatRoomUser.status == 2 || chatRoomUser.status == 4)
            return Clients.Caller.Error("該使用者沒有取得該房間的權限");
            // 取得 roomList
            ChatRoom room = chatRoomFactory.Get(roomId);
            return Clients.Caller.ChatRoomGetAllResponse(new List<ChatRoom>() { room });
        }
        [Authorize(Roles = "User,Anonymous")]
        public Task ChatRoomUserGetAll()
        {
            var userId = getUserId(Context); // 取得 userId
            var list = chatRoomUserFactory.GetAvaiable(userId, getUserType(Context));
            return Clients.Caller.ChatRoomUserGetAllResponse(list);
        }
        [Authorize(Roles = "User")]
        public Task ChatRoomUserGet(int roomId)
        {
            var userId = getUserId(Context); // 取得 userId
            // todo 判斷該 user 是否可以存取該 room
            var list = chatRoomUserFactory.Get(roomId);
            return Clients.Caller.ChatRoomUserGetAllResponse(list);
        }

        // 當使用者操作離開群組或房間(匿名使用者不允許主動離開房間)
        [Authorize(Roles = "User")]
        public Task ChatRoomRemove(int roomId)
        {
            var userId = getUserId(Context); // 取得 userId
            chatRoomFactory.RemoveUser(roomId, userId, 1);
            // todo 通知使用者某人離開房間
            Clients.Clients(factory.roomConIdMap[roomId].ToArray()).ChatRoomRemoveUser(userId, 1, roomId);
            //factory.roomConIdMap[roomId].Remove(Context.ConnectionId);
            return Clients.Caller.ChatRoomRemove(roomId);
        }
        // 建立聊天室
        [Authorize(Roles = "User")]
        public Task ChatRoomCreateGroup(string title, string announce, bool isPrivate, List<int> users)
        {
            ChatRoom chatRoom;
            List<ChatRoomUser> chatRoomUsers;
            var userId = getUserId(Context);
            using (var tranScope = new TransactionScope())
            {
                users.Add(userId); //把使用者自己加入
                                   // 將 ChatRoom 寫入資料庫
                chatRoom = chatRoomFactory.CreateGroup(title, announce, isPrivate);
                // 將 ChatRoomUsers 寫入資料庫
                chatRoomUsers = chatRoomUserFactory.RoomAddUser(chatRoom.id, users);
                // 設定管理員
                chatRoomUserFactory.RoomSetAdmin(chatRoom.id, userId);
                chatRoomUsers.First(x => x.roomId == chatRoom.id && x.userId == userId && x.userType == 1).isAdmin = true;
                tranScope.Complete();
            }

            // 將使用者連線字串新增到聊天室快取內，本身就會 AddOrUpdate roomConIdMap
            // 不需考慮 user 為 Anonymous 的情況
            factory.RoomAddUser(chatRoom.id, chatRoomUsers.Select(x => x.userId).ToList());
            // 把 room 加到 User 的 roomSet 裡面
            var clients = new List<string>();
            foreach (var chatRoomUser in chatRoomUsers)
            {
                if (factory.userMap.ContainsKey(chatRoomUser.userId))
                {
                    factory.PersonAddRoom(chatRoomUser.userId, chatRoomUser.userType, chatRoomUser.roomId);
                    factory.RoomAddPerson(chatRoomUser.roomId, chatRoomUser.userId, chatRoomUser.userType);
                    var set = factory.userMap[chatRoomUser.userId].conIdSet;
                    clients.AddRange(set);
                }
            }
            // 通知其他使用者有新群組
            Clients.Clients(clients.ToArray()).ChatRoomAdd(chatRoom, chatRoomUsers);
            return Clients.Caller.ChatRoomCreateGroupResponse(chatRoom, chatRoomUsers);
        }
        // 登入使用者對登入使用者開啟一對一聊天室
        [Authorize(Roles = "User")]
        public void ChatRoomCreateOneToOne(int userId2)
        {
            var userId1 = getUserId(Context); // 取得 userId
            ChatRoom chatRoom;
            using (var tranScope = new TransactionScope())
            {
                // 檢查 1v1 聊天室是否已經存在，如果存在就... 做相應處理
                chatRoom = chatRoomFactory.GetOneToOne(userId1, userId2, 1);
                if (chatRoom != null)
                {
                    chatRoomUserFactory.Recover(chatRoom.id, userId1, 1); // 先嘗試解除刪除狀態
                    factory.RoomAddUser(chatRoom.id, new List<int>() { userId1, userId2 }); // 將連線Id加入房間通知清單
                    factory.PersonAddRoom(userId1, 1, chatRoom.id); // 將房間加入使用者roooSet清單供判斷是否可傳訊用
                    factory.PersonAddRoom(userId2, 1, chatRoom.id); // 將房間加入使用者roooSet清單供判斷是否可傳訊用
                    Clients.Caller.ChatRoomCreateOneToOne(new
                    {
                        chatRoom,
                        chatRoomUsers = chatRoomUserFactory.Get(chatRoom.id, new List<int> { userId1, userId2 }, 1)
                    });
                    // 加入監聽
                    tranScope.Complete();
                    return;
                }
                // 檢查是否能開啟 1v1 聊天(必須為朋友?先不要) 
                // 建立 1v1 聊天室，相關 table: ChatRoom、ChatRoomUser
                chatRoom = chatRoomFactory.CreateOneToOne(); // 建立一個新房間
                var chatRoomUsers = chatRoomUserFactory.RoomAddUser(chatRoom.id, new List<int> { userId1, userId2 }); // 在線上房間清單註冊線上使用者
                tranScope.Complete(); // 至此資料庫工作已結束先完成資料庫交易
                factory.RoomAdd(chatRoom.id); // 將建立的房間寫入記憶體
                factory.RoomAddUser(chatRoom.id, new List<int> { userId1, userId2 }); // 將使用者連線 Id 關連到該房間
                factory.PersonAddRoom(userId1, 1, chatRoom.id); // 將房間加入使用者roooSet清單供判斷是否可傳訊用
                factory.PersonAddRoom(userId2, 1, chatRoom.id); // 將房間加入使用者roooSet清單供判斷是否可傳訊用
                // 通知雙方 1v1 聊天室已經建立，回傳 ChatRoom、ChatRoomUser (理論上雙方應該已經有 user 資料)
                var data = new { chatRoom = chatRoom, chatRoomUsers }; // 要傳送的 data
                var userArr1 = factory.PersonGetConIdArr(userId2, 1); // 取得使用者 1 的所有連線 Id
                var userArr2 = factory.PersonGetConIdArr(userId1, 1); // 取得使用者 2 的所有連線 Id
                Clients.Clients(userArr1).ChatRoomCreateOneToOne(data);
                Clients.Clients(userArr2).ChatRoomCreateOneToOne(data);
            }
        }
        // 匿名使用者開啟一對一聊天室(當匿名使用者用一般使用者給定的私人聊天 token 登入的時候)
        [Authorize(Roles = "Anonymous")]
        public void ChatRoomCreateOneToOneAnonymous(int id)
        {
        }
        #endregion

        #region SocialRelation/SocialUser 相關操作
        // 取得提交id清單上的聯絡人資訊
        [Authorize(Roles = "Anonymous")]
        public Task AnonymousGetUser()
        {
            var userId = getUserId(Context);
            var list = userFactory.GetByAnonymous(userId);
            return Clients.Caller.SocialUserGetALLResponse(list);
        }
        // 取得提交id清單上的聯絡人資訊
        [Authorize(Roles = "User")]
        public Task SocialRelationGetALL()
        {
            var userId = getUserId(Context);
            var list = userFactory.RelationGet(userId);
            return Clients.Caller.SocialRelationGetALLResponse(list);
        }
        [Authorize(Roles = "User")]
        public Task SocialUserGetALL()
        {
            var userId = getUserId(Context);
            var list = userFactory.GetAllAvaiable(userId);
            return Clients.Caller.SocialUserGetALLResponse(list);
        }
        [Authorize(Roles = "User,Anonymous")]
        public Task SocialUserGet(List<int> connectList)
        {
            var userId = getUserId(Context);
            var list = userFactory.GetAvaiable(userId, connectList);
            return Clients.Caller.SocialUserGetResponse(list);
        }
        [Authorize(Roles = "User")]
        public Task SocialRelationUnlock(int userId2)
        {
            var userId = getUserId(Context);
            var conIds = factory.userMap[userId].conIdSet.ToArray();
            return Clients.Clients(conIds).SocialRelationUnlockResponse(userId2);
        }
        #endregion

        #region onConnect/onDisconnect
        [Authorize]
        public override Task OnConnectedAsync()
        {
            try
            {
                factory.ConAdd(Context);
            }
            catch (Exception ex)
            {
                Clients.Caller.Error(ex.Message);
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            factory.ConRemove(getUserId(Context), Context.ConnectionId, getUserType(Context));
            return base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region Utils
        private int getUserId(HubCallerContext ctx)
        {
            return Convert.ToInt32(ctx.UserIdentifier);
        }
        private int getUserType(HubCallerContext ctx)
        {
            var claims = ((ClaimsIdentity)ctx.User.Identity).Claims;
            return Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "userType")?.Value);
        }
        private int getRoomType(HubCallerContext ctx)
        {
            var claims = ((ClaimsIdentity)ctx.User.Identity).Claims;
            return Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "roomType")?.Value);
        }
        private IEnumerable<Claim> getClaims(HubCallerContext ctx)
        {
            return ((ClaimsIdentity)ctx.User.Identity).Claims;
        }
        #endregion
    }
    public interface IChatHub
    {
        Task MessageReceive(ChatMsg chatMsg);
        Task MessageUserReadResponse();
        Task Error(string msg);
        Task BanUser(dynamic d);
        Task Baned(int userId);
        Task UnBanUser(int id);
        Task UnBaned(int id);
        Task UserAdd(dynamic d);
        Task SocialUserGetResponse(List<dynamic> userList);
        Task SocialUserGetALLResponse(List<dynamic> userList);
        Task SocialRelationGetALLResponse(List<UserRelationShip> userRelationShipList);
        Task SocialRelationUnlockResponse(int userId2);
        Task ChatRoomGetAllResponse(List<ChatRoom> roomList);
        Task ChatRoomGetResponse(ChatRoom room);
        Task ChatRoomUserGetAllResponse(List<ChatRoomUser> chatRoomUser);
        Task ChatRoomCreateGroupResponse(ChatRoom chatRoom, List<ChatRoomUser> chatRoomUseList);
        Task ChatRoomCreateOneToOne(dynamic d);
        Task ChatRoomAdd(ChatRoom chatRoom, List<ChatRoomUser> chatRoomUserList);
        Task ChatRoomAddUser(ChatRoomUser chatRoomUser);
        Task ChatRoomRemove(int id);
        Task ChatRoomRemoveUser(int userId, int userType, int roomId);
        Task ChatMsgUpdate(dynamic d);
        Task Test(dynamic d);
        Task UserUpdate(dynamic user);
        Task SystemSetRoom(int id);
        Task InitDB();
    }
}