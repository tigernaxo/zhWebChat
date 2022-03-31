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
    public class UserAnonymousFactory
    {

        public enum InviteType { Personal = 1, Group = 2 }
        private readonly string conStr;
        public UserAnonymousFactory(IOptions<DALOptions> DALOptions)
        {
            conStr = DALOptions.Value.ConnectionString;
        }
        public UserAnonymous Add(InviteType inviteType)
        {
            return Add(inviteType, null);
        }
        public UserAnonymous Add(InviteType inviteType, int? roomId) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    INSERT INTO A_UserAnonymous
                        (type ,roomId ,isUsed ,userName ,useTime ,createTime ,actTime ,isBaned)
                    OUTPUT INSERTED.ID 
                    VALUES (@type, @roomId, 0, null, null, getdate(), null, 0)
                    ";
                int type = Convert.ToInt32(inviteType);
                var id = cn.QueryFirstOrDefault<int>(sql, new { type, roomId });
                sql = "SELECT * FROM A_UserAnonymous WHERE id=@id";
                return cn.QueryFirstOrDefault<UserAnonymous>(sql, new { id });
            }
        }
        public UserAnonymous? Get(int id)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = "SELECT * FROM A_UserAnonymous WHERE id=@id";
                return cn.QueryFirstOrDefault<UserAnonymous>(sql, new { id });
            }
        }
        public List<dynamic> GetAvaiable(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 選出[ @userId 的關係人](允許 @userId 搜尋該聯絡人資訊)以及[自己]
                var sql = @" 
                    SELECT * FROM A_UserAnonymous 
                    WHERE 
                    (
                        status=1 AND id IN 
                        ( SELECT userId2 FROM A_UserRelationShip WHERE userId = @userId ) 
                    )
                    OR id=@userId
                    ;";
                return cn.Query(sql, new { userId }).ToList();
            }
        }
    }
}
