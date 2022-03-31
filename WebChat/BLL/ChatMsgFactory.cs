using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebChat.Models;

namespace WebChat.BLL
{
    public class ChatMsgFactory
    {
        private readonly string conStr;
        private IWebHostEnvironment webhostEnvironment;
        public ChatMsgFactory(
            IOptions<DALOptions> DALOptions, 
            IWebHostEnvironment _webhostEnvironment
            )
        {
            conStr = DALOptions.Value.ConnectionString;
            webhostEnvironment = _webhostEnvironment;
        }
        public ChatMsg AddImage(int roomId, int userId, int userType, IFormFile file)
        {
            return new ChatMsg();
        }
        public ChatMsg AddFile(int roomId, int userId, int userType, IFormFile file)
        {
            // 重新命名檔案
            var fileName = file.FileName;
            var hashName = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                + "-" + Guid.NewGuid().ToString()
                + Path.GetExtension(fileName);

            // 把檔案儲存到檔案系統
            //var dir = Path.Combine(webhostEnvironment.WebRootPath, "room", Convert.ToString(roomId)); // 組裝資料夾路徑
            var dir = Path.Combine(webhostEnvironment.WebRootPath, "room", Convert.ToString(roomId)); // 組裝資料夾路徑
            //var dir = Path.Combine(webhostEnvironment.ContentRootPath, "room", Convert.ToString(roomId)); // 組裝資料夾路徑
            Directory.CreateDirectory(dir); // 確保資料夾被建立
            var path = Path.Combine(dir, hashName); // 將檔名加入資料夾路徑
            ZH.Util.zhFile.SaveFormFile(file, path); // 寫入檔案 

            // 把資訊存到資料表 ChatMsg
            var chatMsg = Add(new ChatMsg()
            {
                roomId = roomId,
                userId = userId,
                userType = userType,
                type = 13,
                status = 1,
                hashName = hashName,
                fileName = fileName
            });

            // 回傳 ChatMsg
            return chatMsg;
        }
        public ChatMsg Add(ChatMsg chatMsg)
        {
            using (var cn = new SqlConnection(conStr))
            {
                ChatMsg result;
                string strSql = @"
                    INSERT INTO A_ChatMsg
                    (roomId, userId, userType, type, text, createTime, actTime, fileName, hashName, status)
                    OUTPUT INSERTED.ID 
                    VALUES
                    (@roomId, @userId, @userType, @type, @text, getdate(), null, @fileName, @hashName, 1)
                ";
                var id = cn.QueryFirst<int>(strSql, chatMsg);
                strSql = "SELECT * FROM A_ChatMsg WHERE id=@id";
                result = cn.QueryFirst<ChatMsg>(strSql, new { id });
                return result;
            }
        }
        public ChatMsg AddText(int roomId, int userId, int userType, string message)
        {
            var chagMsg = new ChatMsg();
            chagMsg.roomId = roomId;
            chagMsg.userId = userId;
            chagMsg.userType = userType;
            chagMsg.text = message;
            chagMsg.type = 11;
            chagMsg.status = 1;
            using (var cn = new SqlConnection(conStr))
            {
                ChatMsg result;
                // 新增一筆聊天紀錄
                // 如果是被對方刪除的私人聊天室(room.type = 3, status =3)，就恢復不刪除
                string strSql = @"
                    INSERT INTO A_ChatMsg
                    (roomId, userId, userType, type, text, createTime, actTime, fileName, hashName, status)
                    OUTPUT INSERTED.ID 
                    VALUES
                    (@roomId, @userId, @userType, @type, @text, getdate(), null, @fileName, @hashName, 1);
                    IF EXISTS (SELECT 1 FROM A_ChatRoom WHERE id=@roomId AND type=3)
                    BEGIN
                        UPDATE A_ChatRoomUser SET status = 1 WHERE roomId = @roomId AND status = 3;
                    END
                ";
                var id = cn.QueryFirst<int>(strSql, chagMsg);
                strSql = "SELECT * FROM A_ChatMsg WHERE id=@id";
                result = cn.QueryFirst<ChatMsg>(strSql, new { id });
                return result;
            }
        }
        public ChatMsg Get(int id)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = "SELECT * FROM A_ChatMsg WHERE id=@id";
                var result = cn.QueryFirst<ChatMsg>(sql, new { id });
                return result;
            }
        }
        public List<ChatMsg> GetUpdate(DateTime time) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    SELECT * FROM A_ChatMsg 
                        WHERE 
                            (actTime is not null and actTime > actTime) 
                            OR
                            (createTime is not null and createTime > @createTime) 
                    ";
                return cn.Query<ChatMsg>(sql, new { actTime = time, createTime = time }).ToList<ChatMsg>();
            }
        }
    }
}
