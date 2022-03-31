using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using WebChat.Models;

namespace WebChat.BLL
{
    public class ConnectFactory
    {
        private readonly string conStr;
        private readonly zhUtil.JWT jwt;
        public ConnectFactory(
            IOptions<DALOptions> DALOptions,
            zhUtil.JWT _jwt
            )
        {
            conStr = DALOptions.Value.ConnectionString;
            jwt = _jwt;
        }
        public ConnectInfo Add(int userId, int userType, string conId, DateTime startTime) { 
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @"
                    INSERT INTO A_ConnectInfo
                        (userId ,userType ,conId ,isDisConnect ,startTime)
                    OUTPUT INSERTED.ID 
                    VALUES
                        (@userId ,@userType ,@conId ,0 , @startTime);";
                var id = cn.QueryFirst<Int64>(sql , new { userId, userType, conId, startTime });
                sql = "SELECT * FROM A_ConnectInfo WHERE id=@id;";
                return cn.QueryFirstOrDefault<ConnectInfo>(sql, new { id });
            }
        }
        public Int64 GetId(int userId, int userType, string conId, HashSet<long> connectionInfoIds) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = @" SELECT id FROM A_ConnectInfo WHERE userId=@userId AND userType=@userType AND conId=@conId AND id in @ids; ";
                return cn.QueryFirstOrDefault<long>(sql, new { userId, userType, conId, ids = connectionInfoIds });
            }
        }
        public void Update(long id)
        {
            using (var tranScope = new TransactionScope())
            {
                using (var cn = new SqlConnection(conStr))
                {
                    var sql = @" UPDATE A_ConnectInfo SET isDisConnect = 1, endTime=getdate() WHERE id=@id; ";
                    cn.Execute(sql, new { id });
                    sql = @"SELECT * FROM A_ConnectInfo WHERE id=@id;";
                    var info = cn.QueryFirstOrDefault<ConnectInfo>(sql, new { id });

                    // 檢核
                    if (DateTime.Compare(info.startTime, info.endTime) > 0)
                        throw new Exception("該連線的結束日期小於起始日期");

                    // 結算時間到 A_ConnectInfoDay
                    long totalSec = 0;
                    if (info.startTime.Date == info.endTime.Date)
                    {
                        // 如果是同一天，直接計算差值
                        totalSec = Convert.ToInt64((info.endTime - info.startTime).TotalSeconds);
                        AddOrUpdateInfoDay(info.startTime.ToString("yyyyMMdd"), totalSec, cn);
                    }
                    else
                    {
                        // 如果不同天
                        // 計算並存入 startTime 當天的總秒數
                        totalSec = Convert.ToInt64((info.startTime.Date.AddDays(1) - info.startTime).TotalSeconds);
                        AddOrUpdateInfoDay(info.startTime.ToString("yyyyMMdd"), totalSec, cn);
                        // 計算並存入中間整天的秒數
                        DateTime currentTime = info.startTime;
                        while (DateTime.Compare(currentTime.Date, info.endTime.Date) < 0)
                        {
                            currentTime = currentTime.Date.AddDays(1);
                            totalSec = Convert.ToInt64((info.endTime - info.endTime.Date).TotalSeconds);
                            AddOrUpdateInfoDay(currentTime.ToString("yyyyMMdd"), totalSec, cn);
                        }
                        // 計算並存入 endTime 當天的秒數
                        totalSec = Convert.ToInt64((info.endTime - info.endTime.Date).TotalSeconds);
                        AddOrUpdateInfoDay(currentTime.ToString("yyyyMMdd"), totalSec, cn);
                    }
                }
                tranScope.Complete();
            }
        }
        private void AddOrUpdateInfoDay(string yyyyMMdd, long totalSec, SqlConnection cn) {
            var sql = @"
IF (SELECT count(*) FROM A_ConnectInfoDay WHERE yyyyMMdd=@yyyyMMdd) = 0
BEGIN
    INSERT INTO A_ConnectInfoDay (yyyyMMdd, totalSec, actTime) VALUES (@yyyyMMdd, @totalSec, GETDATE())
    RETURN 
END
Else IF (SELECT count(*) FROM A_ConnectInfoDay) >= 1
BEGIN
    UPDATE A_ConnectInfoDay SET totalSec = totalSec + @totalSec WHERE yyyyMMdd=@yyyyMMdd
    RETURN 
END
;";
            cn.Execute(sql, new { yyyyMMdd, totalSec });
        }
        public Int64 GetDaySettleSec(string yyyyMMdd) {
            using (var cn = new SqlConnection(conStr))
            {
                var sql = "SELECT totalSec FROM A_ConnectInfoDay WHERE yyyymmdd=@yyyymmdd;";
                return cn.QueryFirstOrDefault<Int64>(sql , new { yyyyMMdd });
            }

        }
    }
}
