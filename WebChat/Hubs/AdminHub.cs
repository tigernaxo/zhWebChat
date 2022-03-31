using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using WebChat.BLL;

namespace WebChat.Hubs
{
    public class AdminHub : Hub<IAdminHub>
    {
        private readonly ChatHubFactory chatHubFactory;
        public AdminHub( ChatHubFactory _chatHubFactory )
        {
            chatHubFactory = _chatHubFactory;
        }
        public async Task query()
        {
            // 計算使用者數量
            var userCount = chatHubFactory.userMap.Keys.Count + chatHubFactory.anonymousMap.Keys.Count;
            var connectCount = 0;
            foreach (var key in chatHubFactory.userMap.Keys)
            {
                connectCount += chatHubFactory.userMap[key].conIdSet.Count;
            }
            foreach (var key in chatHubFactory.anonymousMap.Keys)
            {
                connectCount += chatHubFactory.anonymousMap[key].conIdSet.Count;
            }
            // 計算連線數量
            Clients.Caller.QueryResponse(new
            {
                connectCount,
                userCount
            });
        }
        #region onConnect/onDisconnect
        [Authorize(Roles = "Admin")]
        public override Task OnConnectedAsync()
        {
            try
            {
                var a = "";
            }
            catch (Exception ex)
            {
                Clients.Caller.Error(ex.Message);
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        #endregion
    }
    public interface IAdminHub
    {
        Task Error(string msg);
        Task QueryResponse(dynamic d);
    }
}
