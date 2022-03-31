using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebChat.Models;

namespace WebChat.BLL
{
    public class ChatRoomUserFactory
    {
        private readonly string conStr;
        public ChatRoomUserFactory(IOptions<DALOptions> DALOptions)
        {
            conStr = DALOptions.Value.ConnectionString;
        }
        public ChatRoomUser? Get(int userId, int userType, int roomId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    SELECT * from A_ChatRoomUser 
                    WHERE userId=@userId AND userType=@userType AND roomId=@roomId;
                    ";
                return cn.QueryFirstOrDefault<ChatRoomUser>(sql, new { userId, userType, roomId });
            }
        }
        public List<ChatRoomUser> Get(int roomId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    SELECT * from A_ChatRoomUser 
                    WHERE roomId=@roomId;
                    ";
                return cn.Query<ChatRoomUser>(sql, new { roomId }).ToList();
            }
        }
        public List<ChatRoomUser> Get(int roomId, List<int> userIdList, int userType)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
					SELECT * FROM A_ChatRoomUser
					WHERE roomId=@roomId AND 
					userType=@userType AND 
					userId IN @userIds; 
					";
                return cn.Query<ChatRoomUser>(sql, new
                {
                    roomId,
                    userIds = userIdList.ToArray(),
                    userType
                }
                ).ToList();
            }
        }
        public List<ChatRoomUser> GetAvaiable(int userId, int userType)
        {
            switch (userType)
            {
                case 1:
                    return GetByUserId(userId);
                case 2:
                    return GetByAnonymousId(userId);
                default:
                    throw new Exception("未知的使用者類型:" + Convert.ToString(userType));
            }
        }
        public ChatRoomUser? GetFirstByAnonymousId(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 匿名使用者只能有一個 ChatRoom，選出來就是了
                var sql = @"
                	SELECT * from A_ChatRoomUser 
                    WHERE roomId IN (
                        SELECT roomId FROM A_ChatRoomUser WHERE userId=@userId AND userType=2 AND status=1 
                    )
                    ";
                return cn.QueryFirstOrDefault<ChatRoomUser>(sql, new { userId });
            }
        }
        public List<ChatRoomUser> GetByAnonymousId(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 匿名使用者只能有一個 ChatRoom，選出來就是了
                var sql = @"
                	SELECT * from A_ChatRoomUser 
                    WHERE roomId IN (
                        SELECT roomId FROM A_ChatRoomUser WHERE userId=@userId AND userType=2 AND status=1 
                    )
                    ";
                return cn.Query<ChatRoomUser>(sql, new { userId }).ToList();
            }
        }
        public ChatRoomUser? GetFirstByUserId(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 使用者自己的 ChatRoomUser
                // UNION
                // 從自己的 ChatRoomUser 反推擁有的 ChatRoom，再取得所有的  ChatRoomUser(但不包含自己的)
                var sql = @"
					SELECT * FROM A_ChatRoomUser WHERE userId=@userId AND status=1 AND userType=1
					UNION
					SELECT * from A_ChatRoomUser 
					WHERE NOT (userId=@userId AND userType=1) AND 
                        roomId IN (
                            SELECT roomId FROM A_ChatRoomUser t 
                            LEFT JOIN A_ChatRoom t1 ON t.roomId = t1.id
                            WHERE t.userId=@userId AND t.userType=1 AND t.status=1
                        )
					";
                var param = new { userId };
                return cn.QueryFirstOrDefault<ChatRoomUser>(sql, param);
            }
        }
        public List<ChatRoomUser> GetByUserId(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 使用者自己的 ChatRoomUser
                // UNION
                // 從自己的 ChatRoomUser 反推擁有的 ChatRoom，再取得所有的  ChatRoomUser(但不包含自己的)
                var sql = @"
					SELECT * FROM A_ChatRoomUser WHERE userId=@userId AND status=1 AND userType=1
					UNION
					SELECT * from A_ChatRoomUser 
					WHERE NOT (userId=@userId AND userType=1) AND 
                        roomId IN (
                            SELECT roomId FROM A_ChatRoomUser t 
                            LEFT JOIN A_ChatRoom t1 ON t.roomId = t1.id
                            WHERE t.userId=@userId AND t.userType=1 AND t.status=1
                        )
					";
                var param = new { userId };
                return cn.Query<ChatRoomUser>(sql, param).ToList();
            }
        }
        public void Recover(int roomId, int userId, int userType) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    UPDATE A_ChatRoomUser SET status=1
                    WHERE roomId=@roomId AND userId=@userId AND status=3 AND userType=@userType
                    UPDATE A_ChatRoomUser SET status=2
                    WHERE roomId=@roomId AND userId=@userId AND status=4 AND userType=@userType
                    ;";
                cn.Execute(sql, new { roomId, userId, userType });
            }
            
        }
        public void ReadMsgUpdate(int userId, int userType, int roomId, int readMsgId) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
                    UPDATE A_ChatRoomUser 
                    SET readMsgId=@readMsgId 
                    WHERE userId=@userId AND roomId=@roomId AND userType=@userType
                ";
                var param = new { userId, roomId, readMsgId, userType };
                cn.Execute(sql, param);
            }
        }
        public void RoomSetAdmin(int roomId, int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
                    UPDATE A_ChatRoomUser 
                    SET isAdmin=1
                    WHERE userId=@userId AND roomId=@roomId AND userType=1
                ";
                var param = new { userId, roomId };
                cn.Execute(sql, param);
            }
        }
        public List<ChatRoomUser> RoomAddUser(int roomId, List<int> userIdList)
        {
            using (var cn = new SqlConnection(conStr))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    var sql = @" 
					INSERT INTO  A_ChatRoomUser 
					(roomId, userId, userType, isAdmin, status, createTime)
					VALUES 
					(@roomId, @userId, 1, 0, 1, getdate())
                    ";
                    List<dynamic> datas = new List<dynamic>();
                    foreach (var userId in userIdList)
                    {
                        datas.Add(new { roomId, userId });
                    }
                    cn.Execute(sql, datas, tran);
                    sql = @"
					SELECT * FROM A_ChatRoomUser
					WHERE roomId=@roomId AND 
					userType=1 AND 
					userId IN @userIds;
					";
                    var result = cn.Query<ChatRoomUser>(sql, new { roomId, userIds = userIdList.ToArray() }, tran).ToList();
                    tran.Commit();
                    return result;
                }
            }

        }
        public ChatRoomUser RoomAddPerson(int roomId, int userId, UserFactory.UserType userType)
        {
            using (var cn = new SqlConnection(conStr))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    var sql = @" 
                    IF EXISTS( SELECT 1 FROM A_ChatRoomUser WHERE roomId=@roomId AND userId=@userId AND userType=@userType )
                        BEGIN
                            UPDATE A_ChatRoomUser SET status = 1 WHERE roomId=@roomId AND userId=@userId AND userType=@userType
                        END
                    ELSE
                        BEGIN
                            INSERT INTO  A_ChatRoomUser 
                            (roomId, userId, userType, isAdmin, status, createTime)
                            VALUES 
                            (@roomId, @userId, @userType, 0, 1, getdate())
                        END
                    ";
                    cn.Execute(sql, new { roomId, userId, userType }, tran);
                    sql = @"
					SELECT * FROM A_ChatRoomUser
					WHERE roomId=@roomId AND 
					userType=@userType AND 
					userId = @userId;
					";
                    var result = cn.QueryFirstOrDefault<ChatRoomUser>(sql, new { roomId, userId, userType }, tran);
                    tran.Commit();
                    return result;
                }
            }

        }
        public bool UserisAdmin(int userId, int roomId)
        {
            var room = Get(userId, 1, roomId); ;
            return room.isAdmin;
        }
    }
}
