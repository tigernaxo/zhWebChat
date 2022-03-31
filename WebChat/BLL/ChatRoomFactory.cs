using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebChat.Models;

namespace WebChat.BLL
{
    public class ChatRoomFactory
    {
        private readonly string conStr;
        private readonly zhUtil.JWT jwt;
        public ChatRoomFactory(
            IOptions<DALOptions> DALOptions,
            zhUtil.JWT _jwt
            )
        {
            conStr = DALOptions.Value.ConnectionString;
            jwt = _jwt;
        }
        public List<ChatRoom> Get(int userId, int userType)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 選出未被刪除的
                var sql = @" 
                    SELECT * from A_ChatRoom 
                    WHERE id IN(
                        SELECT roomId FROM A_ChatRoomUser 
                        WHERE userId = @userId AND userType=@userType AND status IN (1, 2)
                    ) ";
                var param = new { userId, userType };
                return cn.Query<ChatRoom>(sql, param).ToList();
            }
        }
        public ChatRoom? Get(int roomId) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = " SELECT * FROM A_ChatRoom WHERE id=@roomId;";
                return cn.QueryFirstOrDefault<ChatRoom>(sql, new { roomId });
            }
        }
        public ChatRoom? GetOneToOne(int userId, int userId2, int userType2) {
            using (var cn = new SqlConnection(conStr))
            {
// 選出兩位使用私人聊天室
                var sql = @" 
					SELECT * FROM A_ChatRoom 
					WHERE type=3 AND id IN (
					    SELECT t.roomId 
					    FROM (
                            SELECT * FROM A_ChatRoomUser 
                            WHERE userId=@userId AND userType=1
                        ) t
					    INNER JOIN (
                            SELECT * FROM A_ChatRoomUser 
                            WHERE userId=@userId2 AND userType=@userType2
                        ) t1 on t.roomId=t1.roomId
					 )
					";
                var param = new { userId, userId2, userType2 };
                return cn.QueryFirstOrDefault<ChatRoom>(sql, param);
            }
        }
        public ChatRoom CreateGroup(string title, string announce, bool isPrivate)
        {
            var type = isPrivate ? 2 : 1;
            // 新增一個聊天室
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
					INSERT INTO A_ChatRoom (title, announce, type, status)
					OUTPUT INSERTED.ID 
					VALUES 
					(@title, @announce, @type, 1);
					";
                var param = new { title, announce, type };
                var id = cn.QueryFirst<int>(sql, param);
                sql = "SELECT * FROM A_ChatRoom WHERE id=@id";
                return cn.QueryFirst<ChatRoom>(sql, new { id });
            }
        }
        public ChatRoom CreateOneToOne()
        {
            // 新增一個聊天室
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
					INSERT INTO A_ChatRoom (type, status)
					OUTPUT INSERTED.ID 
					VALUES 
					(3, 1);
					";
                var id = cn.QueryFirst<int>(sql);
                sql = "SELECT * FROM A_ChatRoom WHERE id=@id";
                return cn.QueryFirst<ChatRoom>(sql, new { id });
            }
        }
        public void RemoveUser(int roomId, int userId, int userType)
        {
            using (var cn = new SqlConnection(conStr))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    // 檢查 roomId 的種類是 1v1 聊天或群組
                    // 如果是 1v1 聊天就不做任何動作 // 如果是 group，從 ChatRoomUser 把使用者移除
                    //var chatRoom = chatRoomRepo.GetById(roomId);
                    var param = new { id = roomId };
                    var sql = "SELECT * FROM A_ChatRoom WHERE id=@id";
                    var chatRoom = cn.QueryFirst<ChatRoom>(sql, param, tran);

                    sql = @"
                        UPDATE A_ChatRoomUser SET status=3
                        WHERE roomId=@roomId AND userId=@userId AND status=1 AND userType=@userType;
                        UPDATE A_ChatRoomUser SET status=4
                        WHERE roomId=@roomId AND userId=@userId AND status=2 AND userType=@userType;
                    ";
                    cn.Execute(sql, new { roomId, userId, userType }, tran);

                    if (isGroup(chatRoom))
                    {
                        sql = @" SELECT count(*) from A_ChatRoomUser WHERE roomId=@roomId AND status=1 ";
                        var cnt = cn.QueryFirst<int>(sql, new { roomId }, tran);
                        // 如果移除後 ChatRoom 已經沒有人(皆被封鎖或已刪除)，就就關閉 ChatRoom(set status = 2)
                        if (cnt == 0)
                        {
                            sql = "UPDATE A_ChatRoom SET status=2 WHERE id=@roomId AND type <> 3";
                            cn.Execute(sql, new { roomId }, tran);
                        }
                    }
                    tran.Commit();
                }
            }
        }
        public bool isGroup(ChatRoom chatRoom)
        {
            return chatRoom.type != 3;
        }
        public bool isGroup(int roomId)
        {
            return isGroup(Get(roomId));
        }
        public string QrcodeRoom(int roomId, int roomType, int userId)
        {
            //List<Claim> claims = new List<Claim>();
            //claims.Add(new Claim("roles", "Anonymous"));
            //// id 會被便認為 User.Identity.Name，因此改用 anonymousId
            //claims.Add(new Claim("id", Convert.ToString(userId)));
            //claims.Add(new Claim("userType", "2"));
            //claims.Add(new Claim("roomType", Convert.ToString(roomType)));
            //return jwt.GET(claims, Convert.ToString(userId));
            return "";
        }
        public string QrcodePerson(int userId)
        {
            return "";
        }
    }
}
