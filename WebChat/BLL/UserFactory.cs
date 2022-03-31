using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebChat.Models;

namespace WebChat.BLL
{
    public class UserFactory
    {
        public enum UserType { User = 1, Anonymous = 2 }
        private IOptions<JwtOptions> jwtOptions;
        private IWebHostEnvironment webhostEnvironment;
        private readonly string conStr;
        private readonly zhUtil.JWT jwt;
        public UserFactory(
            IOptions<JwtOptions> _jwtOptions,
            IWebHostEnvironment _webhostEnvironment,
            IOptions<DALOptions> DALOptions,
            zhUtil.JWT _jwt
            )
        {
            jwtOptions = _jwtOptions;
            webhostEnvironment = _webhostEnvironment;
            conStr = DALOptions.Value.ConnectionString;
            jwt = _jwt;
        }
        public User Get(string loginId, string password)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
					SELECT  id, loginId, userName, phone, photo 
					from S10_users 
					WHERE loginId=@loginId ;
					";
                return cn.QuerySingleOrDefault<User>(sql, new { loginId, password });
            }
        }
        // 選出與匿名使用者同一房間(1v1)的使用者資訊 
        public List<dynamic> GetByAnonymous(int anonymousId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
					SELECT  t.id, t.loginId, t.userName, t.phone, t.photo from S10_users t
					left join A_ChatRoomUser t2 on t.id = t2.userId
					left join A_ChatRoomUser t3 on t2.roomId = t3.roomId
					where t2.userType=1 and t3.userId=@anonymousId and t3.userType=2
					;";
                return cn.Query<dynamic>(sql, new { anonymousId }).ToList();
            }
        }
        public List<dynamic> GetAvaiable(int userId, List<int> userIdList)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
					SELECT id, loginId, userName, photo
					FROM S10_users 
					WHERE status=1 AND id IN @users 
					";
                    //AND id NOT IN ( select userId from A_UserRelationShip where userId2=@userId and type=2 )
                return cn.Query<dynamic>(sql, new { userId, users = userIdList.ToArray() }).ToList();
            }
        }
        public List<Group> GetGroupById(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
					SELECT t.* FROM S10_group t
					LEFT JOIN S10_userGroup t1 on t.id=t1.groupId 
					WHERE t1.userId = @userId;
					";
                return cn.Query<Group>(sql, new { userId }).ToList();
            }
        }
        public List<dynamic> GetAllAvaiable(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 選出[ @userId 的關係人](允許 @userId 搜尋該聯絡人資訊)以及[自己]
                var sql = @" 
                    SELECT id, loginId, 1 userType, userName, photo FROM S10_users 
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
        public void Update(int userId, string userName)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 更新
                var sql = @" UPDATE S10_users SET userName=@userName WHERE id=@userId; ";
                cn.Execute(sql, new { userId, userName });
            }
        }
        public User? Search(int userId, string loginId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 搜尋出該 ID (無論是否被對方封鎖) 
                var sql = @"
					select id, userName, loginId from S10_users  
					where 
					loginId = @loginId 
					;";
                // 判斷 user 是否被該 ID 封鎖
                //and id not in ( select userId from A_UserRelationShip where userId2=@userId and type=2)
                return cn.QueryFirstOrDefault<User?>(sql, new { userId, loginId });
            }
        }
        public List<User> SearchFuzzy(int userId, string loginId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                // 搜尋出該 ID (無論是否被對方封鎖) 
                var sql = @"
					select id, userName, loginId from S10_users  
					where 
					loginId like '%' + @loginId + '%'
					;";
                // 判斷 user 是否被該 ID 封鎖
                //and id not in ( select userId from A_UserRelationShip where userId2=@userId and type=2)
                return cn.Query<User>(sql, new { userId, loginId }).ToList();
            }
        }
        public List<UserRelationShip> RelationGet(int userId) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" 
                    SELECT [userId] ,[userId2] ,[type] ,[createTime] 
                    FROM [dbo].[A_UserRelationShip] 
                    WHERE userId=@userId 
                    ";
                return cn.Query<UserRelationShip>(sql, new { userId }).ToList();
            }
        }
        public dynamic ContactAdd(int userId, int userId2)
        {
            using (var cn = new SqlConnection(conStr))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    var par = new { userId, userId2 };
                    var sql = @"
                    INSERT INTO [dbo].[A_UserRelationShip]
                        ([userId] ,[userId2] ,[type] ,[createTime])
                    VALUES
                        (@userId ,@userId2 ,1 ,getdate())
                ;";
                    cn.Execute(sql, par, tran);
                    sql = @"SELECT [userId] ,[userId2] ,[type] ,[createTime]
                        FROM [dbo].[A_UserRelationShip]
                        WHERE userId=@userId AND userId2=@userId2 AND type=1
                ";
                    var userRelationShip = cn.QueryFirstOrDefault<UserRelationShip>(sql, par, tran);
                    sql = @" 
					SELECT id, loginId, 1 userType, userName, photo
					FROM S10_users 
					WHERE 
					status=1 AND id=@userId2
                    ";
                    var user = cn.QueryFirstOrDefault(sql, new { userId2 }, tran);
                    tran.Commit();
                    return new { user, userRelationShip };
                }
            }
        }
        public void ContactDel(int userId, int userId2)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var par = new { userId, userId2 };
                var sql = @"
                    DELETE FROM [dbo].[A_UserRelationShip]
                    WHERE userId=@userId and userId2=@userId2 and type=1
                    ;";
                cn.Execute(sql, par);
            }
        }
        public void UnBan(int userId, int userId2)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    DELETE FROM A_UserRelationShip 
                    WHERE userId=@userId AND userId2=@userId2 AND type=2;
                    ";
                var par = new { userId, userId2 };
                cn.Execute(sql, par);
            }
        }
        public dynamic Ban(int userId, int userId2) {
            using (var cn = new SqlConnection(conStr))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    var sql = @"
                    INSERT INTO [dbo].[A_UserRelationShip]
                           ([userId] ,[userId2] ,[type] ,[createTime])
                    VALUES
                           (@userId, @userId2, 2, getdate())
                    ;";
                    var par = new { userId, userId2 };
                    cn.Execute(sql, par, tran);
                    sql = @"
					SELECT [userId] ,[userId2] ,[type] ,[createTime]
					FROM [dbo].[A_UserRelationShip]
					WHERE userId=@userId AND userId2=@userId2 AND type=2
					;";
                    var userRelationShip = cn.QueryFirstOrDefault<UserRelationShip>(sql, par, tran);
                    sql = @"
                        SELECT id, loginId, 1 userType, userName, photo FROM S10_users 
                        WHERE id=@userId2
                    ";
                    var user = cn.QueryFirstOrDefault<dynamic>(sql, new { userId2 }, tran);
                    tran.Commit();
                    return new { userRelationShip, user };
                }
            }
        }
        // 沒有多弄一個 TokenFactory，因此暫寄此處
        public string GetToken(User user)
        {
            // 設定要加入到 JWT Token 中的聲明資訊(Claims)
            List<Claim> claims = new List<Claim>();

            if (isUserAdmin(user.id))
            {
                claims.Add(new Claim("roles", "Admin")); // 可自行擴充 "roles" 加入登入者該有的角色
            }
            claims.Add(new Claim("roles", "User"));
            claims.Add(new Claim("photo", user.photo ?? ""));
            claims.Add(new Claim("id", Convert.ToString(user.id))); // User.Identity.Name
            claims.Add(new Claim("loginId", user.loginId));
            claims.Add(new Claim("userName", user.userName ?? ""));
            claims.Add(new Claim("userType", "1"));

            return jwt.GET(claims, user.loginId);
        }
        public string HeadImageGetName(string fileName)
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff")
                + "-" + Guid.NewGuid().ToString()
                + Path.GetExtension(fileName);
        }
        public string HeadImageSave(int userId, IFormFile file)
        {
            var fileName = HeadImageGetName(file.FileName); // 取得 hash image file name

            var dir = getUserPhotoPath(webhostEnvironment, userId); // 組裝資料夾路徑

            Directory.CreateDirectory(dir); // 確保資料夾被建立
            ZH.Util.zhFile.removeAllChild(dir); // 刪除資料夾路徑下所有的舊檔案

            var path = Path.Combine(dir, fileName); // 將檔名加入資料夾路徑
            ZH.Util.zhFile.SaveFormFile(file, path); // 寫入檔案 
            // 寫入資料庫
            using (var cn = new SqlConnection(conStr))
            {
                // 更新
                var sql = @" UPDATE S10_users SET photo=@fileName WHERE id=@userId; ";
                cn.Execute(sql, new
                {
                    userId,
                    fileName
                });
            }
            return  fileName; // 回傳檔案名稱
        }
        public bool isUserAdmin(int userId)
        {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = "SELECT CASE WHEN COUNT(*)=0 THEN 0 ELSE 1 END FROM S10_userGroup WHERE userId=3;";
                return cn.QueryFirst<bool>(sql, new { userId });
            }
        }
        private static string getUserPhotoPath(IWebHostEnvironment webhostEnvironment, int userId)
        {
            var rootPath = webhostEnvironment.WebRootPath;
            //var rootPath = webhostEnvironment.ContentRootPath;
            rootPath = Path.Combine(rootPath, "user");
            var dir = Path.Combine(rootPath, Convert.ToString(userId), "head");
            return dir;
        }
    }
}
