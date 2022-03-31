using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WebChat.Models
{
    public class System
    {
        public string announce {get;set;} // 系統聲明
    }
    public class User
    {
        public int id { get; set; }  // id
        public string loginId { get; set; }  // 使用者用來登入的 Id
        public string password { get; set; } // 密碼
        public string? userName { get; set; } // 顯示名稱
        public string phone { get; set; } // not null, unique
        public string? photo { get; set; }
        public int status { get; set; }  // 1:正常 2:封鎖 0:停用
        public string key { get; set; } // key for front end indexedDB Storage
        public DateTime createTime { get; set; }
        public DateTime actTime { get; set; }  // 判斷聊天室是否被刪除
    }
    public class Group
    {
        public int id { get; set; }
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int status { get; set; }
        public DateTime createTime { get; set; }
        public int createUser { get; set; }
        public DateTime actTime { get; set; }
        public int actUser { get; set; }
    }
    // 與 User/Group 起始對話的 Token
    public class UserAnonymous
    {
        public int id { get; set; }
        public string token { get; set; } // should be unique
        public int type { get; set; } // 1: 1v1 聊天室的邀情 2: 來自群組的邀請
        public int? userId { get; set; } // 哪個使用者發出的邀請
        public int? roomId { get; set; } // 紀載這個 token 用於哪個聊天室
        public bool isUsed { get; set; } // 是否使用過 not null
        public bool isBaned { get; set; } // 是否被封鎖 not null
        public string? userName { get; set; } // 匿名使用者設置的顯示名稱
        public DateTime useTime { get; set; } // 匿名使用者使用該 token 登入的時間
        public DateTime createTime { get; set; }
        public DateTime actTime { get; set; }  //  用來推算 use time
    }
    // 記錄使用者之間的關係 pk: userId, userId2, type (可同時是聯絡人且封鎖)
    public class UserRelationShip { 
        public int userId { get; set; } // userId
        public int userId2 { get; set; } // target userId
        public int type { get; set; } // 1:朋友 2:封鎖
        public DateTime? createTime { get; set; }
    }
    // 聊天室資料表
    public class ConnectInfo
    {
        public Int64 id { get; set; }
        public int userId { get; set; }
        public int userType { get; set; }
        public string conId { get; set; }
        public bool isDisConnect { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }
    public class ConnectInfoDay
    {
        public string yyyymmdd { get; set; }
        public int totalConSec { get; set; }
        public DateTime actTime { get; set; }
    }
    public class ChatRoom {
        public int id { get; set; }
        public string? title { get; set; } // 如果是群聊就必須要有標題
        public string? announce { get; set; } // 聊天室聲明
        public int type { get; set; }  // 0:?? 1:公開聊天室 2:私人聊天室 3:1對1聊天室(兩個使用者之間如果已有，則不可開啟其他聊天室)
        public int status { get; set; }  // 1:正常 2:刪除 
        public DateTime? createTime { get; set; }
        public DateTime? actTime { get; set; }
    }
    // 紀載在聊天室內的使用者
    public class ChatRoomUser
    {
        public int roomId { get; set; }
        public int userId { get; set; }
        public int userType { get; set; } // 使用者種類：1 登入使用者 2 匿名使用者 not null
        public bool isAdmin { get; set; } // 使用是不是該聊天室管理員
        public ulong? readMsgId { get; set; } // 該使用者上線已讀訊息的最大Id，訊息Id超過此數字會被標記為未讀
        // 1:未被封鎖且未刪除
        // 2:被封鎖且未刪除    
        // 3:未被封鎖且刪除
        // 4:被封鎖且刪除
        // 封鎖 1->2 或 3->4
        // 刪除 1->3 或 2->4
        public int status { get; set; } 
        public DateTime? createTime { get; set; }
        public DateTime? actTime { get; set; }
    }
    public class ChatMsg  // pk =  id
    {
        public int id { get; set; } 
        public int roomId { get; set; } // Reference: ChatRoom -> id
        public int userId { get; set; } // Reference: User -> id
        public int userType { get; set; }
        public int type { get; set; } // 11文字/12圖片/13檔案/14已編輯文字/21加入聊天/22離開聊天
        public string? text { get; set; } // 如果是圖片或檔案就 NULL，如果是文字就直接存文字
        public int status { get; set; } // not null 1:未刪除 2:刪除
        public string? fileName { get; set; }  // 原本上傳的檔案名稱
        public string? hashName { get; set; }  // 上傳之後重新編碼過的檔案命稱
        public DateTime? createTime { get; set; } // 訊息建立的時間 
        public DateTime? actTime { get; set; } 
    }
    // 訊息種類
    public class ChatMsgType  // pk =  id
    {
        public int id { set; get; }
        public string typeName { set; get; }
    }
}
