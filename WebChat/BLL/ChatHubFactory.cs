using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WebChat.Hubs;
using WebChat.Models;

namespace WebChat.BLL
{
    /*
     * ChatHubFactory
     * 紀錄 user 資訊，包含 connectionId、可接收訊息的 rooms
     * 紀錄 room 要傳遞訊息的對象的 connectionId
     * 
     */
    public class ChatHubFactory
    {
        public class UserInfo
        {
            public int id;
            public HashSet<Int64> connectInfoSet; // 使用者 connectInfo id
            public HashSet<string> conIdSet; // 使用者 connectionId
            public HashSet<int> roomSet;  // 使用者可存取的 rooms，HashSet 性能 >> List
            public dynamic toDynamic()
            {
                return new
                {
                    id,
                    connectInfoSet = connectInfoSet.ToArray(),
                    conIdSet = conIdSet.ToArray(),
                    roomSet = roomSet.ToArray()
                };
            }
        }

        // 紀錄 connectInfo 從哪一天開始
        public ConcurrentDictionary<int, HashSet<long>> connectInfoStartDay= new ConcurrentDictionary<int, HashSet<long>>();
        public ConcurrentDictionary<long, ConnectInfo> connectInfos = new ConcurrentDictionary<long, ConnectInfo>();
        // 房間內監聽的 connectionId
        public ConcurrentDictionary<int, HashSet<string>> roomConIdMap = new ConcurrentDictionary<int, HashSet<string>>();
        // 使用者清單
        public ConcurrentDictionary<int, UserInfo> userMap = new ConcurrentDictionary<int, UserInfo>(); 
        public ConcurrentDictionary<int, UserInfo> anonymousMap = new ConcurrentDictionary<int, UserInfo>(); 
        private readonly string conStr;

        private readonly IHubContext<ChatHub, IChatHub> hubContext;
        public readonly ConnectFactory connectFactory;
        public readonly ChatRoomFactory chatRoomFactory;
        public readonly ChatRoomUserFactory chatRoomUserFactory;
        public readonly UserAnonymousFactory userAnonymousFactory;
        public ChatHubFactory(
            IHubContext<ChatHub, IChatHub> _hubContext,
            IOptions<DALOptions> DALOptions,
            ConnectFactory _connectFactory,
            ChatRoomFactory _chatRoomFactory,
            ChatRoomUserFactory _chatRoomUserFactory,
            UserAnonymousFactory _userAnonymousFactory
            )
        {
            hubContext = _hubContext;
            conStr = DALOptions.Value.ConnectionString;
            connectFactory = _connectFactory;
            chatRoomFactory = _chatRoomFactory;
            chatRoomUserFactory = _chatRoomUserFactory;
            userAnonymousFactory = _userAnonymousFactory;
        }

        // 添加 connectionId 到 room 的聽眾
        public void RoomAdd(int roomId)
        {
            if (!roomConIdMap.ContainsKey(roomId))
            {
                roomConIdMap.TryAdd(roomId, new HashSet<string>()); ;
            }
        }
        public HashSet<string> RoomGetConId(int roomId)
        {
            return roomConIdMap[roomId];
        }
        public void RoomAddConId(int roomId, string conId)
        {
            roomConIdMap.AddOrUpdate(roomId, new HashSet<string>() { conId },
                (room, oldSet) =>
                {
                    if (!oldSet.Contains(conId))
                    {
                        oldSet.Add(conId);
                    }
                    return oldSet;
                });
        }
        public void RoomAddConId(int roomId, HashSet<string> conIdSet)
        {
            roomConIdMap.AddOrUpdate(roomId, conIdSet,
                (room, oldSet) =>
                {
                    oldSet.UnionWith(conIdSet);
                    return oldSet;
                });
        }

        // 將 connectionId 從 room 的聽眾移除
        public void RoomRemoveConId(int roomId, string conId) {
            if (roomConIdMap.ContainsKey(roomId))
            {
                if (roomConIdMap[roomId].Contains(conId))
                    roomConIdMap[roomId].Remove(conId);
                if (roomConIdMap[roomId].Count == 0) //  如果空了就整個移除
                    ((IDictionary)roomConIdMap).Remove(roomId);
            }
        }
        public void RoomAddPerson(int roomId, int userId, int userType)
        { 
            var map = userType == 1 ? userMap : anonymousMap;
            // 如果 使用者在線上
            if (map.ContainsKey(userId))
            {
                foreach (var conId in map[userId].conIdSet)
                {
                    RoomAddConId(roomId, conId);
                }
            }
        }
        public void RoomAddUser(int roomId, List<int> userList)
        {
            foreach (var id in userList)
            {
                RoomAddPerson(roomId, id, 1);
            }
        }
        public bool RoomIsAllowPerson(int roomId, int userId, int userType)
        {
            var map = userType == 1 ? userMap : anonymousMap;
            return map[userId].roomSet.Contains(roomId);
        }
        public HashSet<int> PersonGetRooms(int userId, int userType)
        {
            var map = userType == 1 ? userMap : anonymousMap;
            return map[userId].roomSet;
        }
        public string[] PersonGetConIdArr(int userId, int userType) {
            var map = userType == 1 ? userMap : anonymousMap;
            if (map.ContainsKey(userId))
            {
                return map[userId].conIdSet.ToArray();
            }
            return new string[] { };
        }
        // 添加 connectionId 監聽權限
        public void PersonAddRoom(int userId, int userType, int roomId)
        {
            var map = userType == 1 ? userMap : anonymousMap;
            if (!map.ContainsKey(userId)) return;
            // 添加監聽中的房間到使用者資訊
            map[userId].roomSet.Add(roomId);
        }
        public void PersonRemoveRoom(int userId, int roomId, int userType)
        {
            var map = userType == 1 ? userMap : anonymousMap;
            var user = map[userId];
            // 將 connectionId 從 room 的聽眾移除
            foreach (var conId in user.conIdSet)
            {
                RoomRemoveConId(roomId, conId);
            }
            // 將使用者監聽中的 room 移除
            user.roomSet.Remove(roomId);
        }
        /*
        在 userMap/anonymousMap 中加入 user
        在 room 監聽清單中加入 connectionId
         */
        public void ConAdd(HubCallerContext ctx)
        {
            var claims = ((ClaimsIdentity)ctx.User.Identity).Claims;
            var userType = Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "userType")?.Value);
            var userId = Convert.ToInt32(ctx.UserIdentifier);
            var conId = ctx.ConnectionId;
            switch (userType)
            {
                case 1:
                    ConAddUser(ctx);
                    break;
                case 2:
                    ConAddAnonymous(ctx);
                    break;
                default:
                    throw new Exception("未知的使用者種類:" + Convert.ToString(userType));
            }
        }
        private void ConAddAnonymous(HubCallerContext ctx)
        {
            var claims = ((ClaimsIdentity)ctx.User.Identity).Claims;
            var userType = Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "userType")?.Value);
            var userId = Convert.ToInt32(ctx.UserIdentifier);
            var conId = ctx.ConnectionId;
            int issuerId = Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "issuerId")?.Value);
            int? roomType = Convert.ToInt32(claims.FirstOrDefault(claim => claim.Type == "roomType")?.Value);

            if(userAnonymousFactory.Get(userId) == null)
                throw new Exception("匿名使用者不存在資料庫中");
            using (var cn = new SqlConnection(conStr))
            {
                // todo: 處理匿名使用者請求進入1v1聊天室(不帶 roomId, roomType=3)
                switch (roomType)
                {
                    case 1:
                    case 2:
                        ConAddAnonymousGroup(userId, conId);
                        break;
                    case 3:
                        ConAddAnonymousOneToOne(userId, conId, issuerId);
                        break;
                    default:
                        throw new Exception("不合法的 roomType");
                }
            }
        }
        private void ConAddAnonymousOneToOne(int userId, string conId, int issuerId)
        {
            // 建立連線資訊 
            var time = DateTime.Now;
            var connectInfo = connectFactory.Add(userId, 2, conId, time);
            connectInfos.AddOrUpdate(connectInfo.id, connectInfo, (id, obj) => connectInfo);
            var connectInfoId = connectInfo.id;
            connectInfoStartDay.AddOrUpdate(
                Convert.ToInt32(time.ToString("yyyyMMdd")),
                new HashSet<long>() { connectInfoId },
                (id, oldSet) => {
                    oldSet.Add(connectInfoId);
                    return oldSet;
                });
            ChatRoom room;
            List<ChatRoomUser> chatRoomUserList;

            // 取出要建立聯絡的對象使用者
            using (TransactionScope scope = new TransactionScope())
            {
                room = chatRoomFactory.GetOneToOne(issuerId, userId, 2);
                // 如果沒有房間，根據匿名使用者、對象使用者建立 ChatRoom、ChatRoomUser
                if (room == null)
                {
                    room = chatRoomFactory.CreateOneToOne();
                    var chatRoomUserAnonymous = chatRoomUserFactory.RoomAddPerson(room.id, userId, UserFactory.UserType.Anonymous);
                    var chatRoomUserIssuer = chatRoomUserFactory.RoomAddPerson(room.id, issuerId, UserFactory.UserType.User);
                    chatRoomUserList = new List<ChatRoomUser>() { chatRoomUserAnonymous, chatRoomUserIssuer };
                }
                // 把 room 加到  roomConIdMap
                chatRoomUserList = chatRoomUserFactory.GetByAnonymousId(userId);
                scope.Complete();
            }

            // 處理記憶體資料
            // 如果使用者在線上，就監聽的 room 添加到他的 userInfo
            if (userMap.ContainsKey(issuerId))
                userMap[issuerId].roomSet.Add(room.id);
            // 在記憶體中註冊 Anonmous 的資料 
            anonymousMap.AddOrUpdate(
                userId, 
                new UserInfo()
                {
                    id = userId,
                    roomSet = new HashSet<int>() { room.id },
                    conIdSet = new HashSet<string>() { conId },
                    connectInfoSet = new HashSet<long>() { connectInfoId }
                },
                (userId, oldUserInfo) =>
                {
                    oldUserInfo.conIdSet.Add(conId);
                    oldUserInfo.connectInfoSet.Add(connectInfoId);
                    return oldUserInfo;
                });

            var a = userMap.ContainsKey(issuerId);
            // 把 user、Anonymous 的連線資料存到 roomConIdMap 當中
            roomConIdMap.AddOrUpdate(room.id, anonymousMap[userId].conIdSet,
                (room, oldSet) =>
                {
                    // 把 anonymous 的 conId 加到 room 放送名單
                    oldSet.UnionWith(anonymousMap[userId].conIdSet);
                    // 如果對象使用者有在線上，就加到 room 的放送名單
                    if (userMap.ContainsKey(issuerId))
                        oldSet.UnionWith(userMap[issuerId].conIdSet);
                    return oldSet;
                });
            // 再加一次
            if (userMap.ContainsKey(issuerId))
                roomConIdMap[room.id].UnionWith(userMap[issuerId].conIdSet);
            // 通知匿名使用者
            var conIdArr = PersonGetConIdArr(userId, 2);
            hubContext.Clients.Clients(conIdArr).ChatRoomAdd(room, chatRoomUserList);
            hubContext.Clients.Clients(conIdArr).SystemSetRoom(room.id);
            // P.S.由匿名使用者啟動通話的時候，才會通知對象使用者匿名使用者房間已新增
        }
        private void ConAddAnonymousGroup(int userId, string conId)
        {
            var time = DateTime.Now;
            var connectInfo = connectFactory.Add(userId, 2, conId, time);
            var connectInfoId = connectInfo.id;
            connectInfos.AddOrUpdate(connectInfo.id, connectInfo, (id, obj) => connectInfo);
            connectInfoStartDay.AddOrUpdate(
                Convert.ToInt32(time.ToString("yyyyMMdd")),
                new HashSet<long>() { connectInfoId },
                (id, oldSet) => {
                    oldSet.Add(connectInfoId);
                    return oldSet;
                });
            using (var cn = new SqlConnection(conStr))
            {
                string sql = "SELECT DISTINCT roomId from A_ChatRoomUser WHERE userId=@userId AND status=1 AND userType=2;";
                int roomId = cn.QueryFirstOrDefault<int>(sql, new { userId });
                // 添加 anonymous 到 anonymousMap 當中
                anonymousMap.AddOrUpdate(
                    userId,
                    new UserInfo()
                    {
                        id = userId,
                        conIdSet = new HashSet<string> { conId },
                        roomSet = new HashSet<int>() { roomId },
                        connectInfoSet = new HashSet<long>() { connectInfoId}
                    },
                    (userId, oldInfo) =>
                    {
                        oldInfo.conIdSet.Add(conId);
                        oldInfo.connectInfoSet.Add(connectInfoId);
                        return oldInfo;
                    });
                // 添加 conId 到 roomConIdMap 當中
                roomConIdMap.AddOrUpdate(
                    roomId,
                    new HashSet<string>() { conId },
                    (id, oldSet) =>
                    {
                        oldSet.Add(conId);
                        return oldSet;
                    });
            }
        }
        private void ConAddUser(HubCallerContext ctx)
        {
            var userId = Convert.ToInt32(ctx.UserIdentifier);
            var conId = ctx.ConnectionId;
            var time = DateTime.Now;
            var connectInfo = connectFactory.Add(userId, 1, conId, time);
            var connectInfoId = connectInfo.id;
            connectInfos.AddOrUpdate(connectInfo.id, connectInfo, (id, obj) => connectInfo);
            connectInfoStartDay.AddOrUpdate(
                Convert.ToInt32(time.ToString("yyyyMMdd")),
                new HashSet<long>() { connectInfoId },
                (id, oldSet) => {
                    oldSet.Add(connectInfoId);
                    return oldSet;
                });
            string sql;
            using (var cn = new SqlConnection(conStr))
            {
                if (userMap.ContainsKey(userId))
                {
                    userMap[userId].conIdSet.Add(conId);
                    userMap[userId].connectInfoSet.Add(connectInfoId);
                }
                else
                {
                    #region 決定 user 可以發言的 room 清單
                    List<int> roomList;
                    sql = "SELECT DISTINCT roomId from A_ChatRoomUser WHERE userId=@userId AND status IN (1, 3);";
                    var param = new { userId };
                    roomList = cn.Query<int>(sql, param).ToList();

                    // 在使用者清單設置新使用者
                    userMap[userId] = new UserInfo
                    {
                        id = userId,
                        conIdSet = new HashSet<string> { conId },
                        roomSet = new HashSet<int>(roomList),
                        connectInfoSet = new HashSet<long>{ connectInfoId }
                    };
                    #endregion

                }
                #region 決定 user 要監聽的 room，如果是 1v1 聊天室且使用者封鎖對方，就不加入(光刪除的話要)
                // 1:[未被封鎖且未刪除]
                // 2:被封鎖且未刪除    
                // 3:[未被封鎖且刪除]
                // 4:被封鎖且刪除
                /*

                                    -- 選出使用者封鎖的人與使用者共同的單人聊天室 roomId
                                    SELECT roomId FROM A_ChatRoomUser WHERE status IN (1, 3)
                                    AND userId IN (
                                        -- 選出使用者封鎖的人 
                                        SELECT userId2 FROM A_UserRelationShip WHERE userId=@userId AND type=2
                                    )
                                    AND roomId IN (
                                        --  選出使用者有權限存取(未被封鎖)的 roomId 且 為單人聊天室
                                        SELECT id FROM A_ChatRoom WHERE type=3 AND id IN (
                                            --  選出使用者有權限存取(未被封鎖)的 roomId
                                            SELECT DISTINCT roomId from A_ChatRoomUser WHERE userId=@userId AND status in (1 ,3)
                                        )
                                    );
                 */
                sql = @"
					SELECT roomId FROM A_ChatRoomUser WHERE status IN (1, 3)
					AND userId IN (
						SELECT userId2 FROM A_UserRelationShip WHERE userId=@userId AND type=2
					)
					AND roomId IN (
						SELECT id FROM A_ChatRoom WHERE type=3 AND id IN (
							SELECT DISTINCT roomId from A_ChatRoomUser WHERE userId=@userId AND status in (1 ,3)
						)
					);
                ";
                var roomIdSet2 = cn.Query<int>(sql, new { userId }).ToHashSet();
                foreach (var roomId in userMap[userId].roomSet)
                {
                    if (!roomIdSet2.Contains(roomId))
                    {
                        RoomAddConId(roomId, conId);
                    }
                }
                #endregion
            }
        }
        // 從 userMap 以 userId 找 user 取得 connectionId
        // 解除 room 裡面 connectionId 的監聽
        // 從 userMap 移除 user
        public void ConRemove(int userId, string conId, int userType)
        {
            var map = userType == 1 ? userMap : anonymousMap;
            if (map.Keys.Contains(userId))
            {
                var user = map[userId];

                // 處理 connectInfo
                var connectInfoId = connectFactory.GetId(userId, userType, conId, user.connectInfoSet);
                ((IDictionary)connectInfos).Remove(connectInfoId); // 從連線資訊中移除
                user.connectInfoSet.Remove(connectInfoId);  // 從使用者資訊中移除
                connectFactory.Update(connectInfoId); // 執行資料庫結算邏輯
                foreach(var key in connectInfoStartDay.Keys)
                {
                    connectInfoStartDay[key].Remove(connectInfoId); // 從起始時間字典內移除
                    if (connectInfoStartDay[key].Count == 0)
                        ((IDictionary)connectInfoStartDay).Remove(key);
                }

                // 處理 connectionId
                user.conIdSet.Remove(conId); // 從使用者資訊中移除
                foreach (var roomId in user.roomSet)
                {
                    RoomRemoveConId(roomId, conId); // 從 room 的聽眾移除
                }

                // 當使用者全斷線時，移除已註冊的 user
                if (user.conIdSet.Count() == 0)
                {
                    ((IDictionary)map).Remove(userId);
                }
            }
        }
        // debug 用，檢查是不是有多的 connection Id
        public void checkDislinkedConId()
        {
            List<string> disconnectedIds = new List<string>();
            // 蒐集所有 user 註冊的 connectionId
            HashSet<string> conIds = new HashSet<string>();
            foreach (var um in userMap)
            {
                conIds.UnionWith(um.Value.conIdSet);
            }
            foreach (var am in anonymousMap)
            {
                conIds.UnionWith(am.Value.conIdSet);
            }
            // 檢查 room 裡面註冊的 connectionId 是否有多的
            foreach (var rm in roomConIdMap)
            {
                foreach (string conId in rm.Value)
                {
                    if (!conIds.Contains(conId))
                    {
                        disconnectedIds.Add(conId);
                    }
                }
            }
            if (disconnectedIds.Count() > 0)
            {
                throw new Exception("多的 connectionId:" + String.Join(",", disconnectedIds));
            }
        }
    }
}
