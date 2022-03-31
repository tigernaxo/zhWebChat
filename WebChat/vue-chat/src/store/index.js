/* eslint-disable no-prototype-builtins */
import { computed, reactive } from 'vue'
// import logger from '@/zh/logger'
// import ws from '@/zh/ws'

export let store = {
  db: null,
  system: reactive({
    announce: '', // 系統聲明
    // 前端欄位 要放這裡?
    isLogin: false, // 是否登入
    token: '', // token
    roomId: null, // 目前打開的 roomId
    userId: null, // userId
    userType: 1,
    loginId: '', // loginId
    userName: '', // 顯示名稱
    showConfig: false, // 是否正在顯示設定頁面
    wsState: '',
    timer: null // setInterval
  }),
  users: reactive({
    /*
    public int id { get; set; } 
    public string loginId { get; set; } 
    public string password { get; set; }
    public string? userName { get; set; }
    public string? key { get; set; }
    public string phone { get; set; } // not null, unique
    public string? photo { get; set; }
    public int status {get;set;}  // 1:正常 2:封鎖 0:停用
    public DateTime createTime { get; set; }
    public DateTime actTime { get; set; }  // 判斷聊天室是否被刪除
  */
  }),
  userRelationShips: reactive({
    /*
    public int userId { get; set; } // userId
    public int userId2 { get; set; } // target userId
    public int type { get; set; } // 1:朋友 2:封鎖
    public DateTime? createTime { get; set; }
  */
  }),
  chatRooms: reactive({
    /*
    public int id { get; set; }
    public string title { get; set; }
    public string? announce { get; set; } // 聊天室聲明
    // 0:?? 1:公開聊天室 2:私人聊天室 3:1對1聊天室(兩個使用者之間如果已有，則不可開啟其他聊天室)
    public int type { get; set; }  
    public int status { get; set; }  // 1:正常 2:刪除 
    public DateTime? createTime { get; set; }
    public DateTime? actTime { get; set; }
 */
    // 佔位用，避免 v-model 綁定到 null
    0: {
      id: 0,
      title: '未選擇聊天室',
      announce: '',
      type: 0,
      status: 1,
      createTime: '',
      actTime: ''
    }
  }),
  // chatRooms 的前端專屬欄位
  chatRooms_Meta: reactive({
    // 佔位用，避免 v-model 綁定到 null
    0: {
      draftMsg: '',
      // 前次未讀的訊息，作為聊天室中顯示以下未讀的依序
      // 更新時機：當使用者離開聊天室時，將非選中的聊天室 readMsgOld 同步到 chatRoomUser.readMsgId
      readMsgIdOld: null,
      messages: [
        /* array of messsage id */
      ],
      chatRoomUsers: [
        /* array of chatRoomUser key */
      ]
    }
  }),
  chatRoom_Current: computed(() => store.chatRooms[store.system.roomId] ?? null),
  chatRoom_Meta_Current: computed(
    () => store.chatRooms_Meta[store.system.roomId] ?? store.chatRooms_Meta[0]
  ),
  chatRoomUsers: reactive({
    /* pk = 'userId@type@roomId'
    public int roomId { get; set; }
    public int userId { get; set; }
    public int userType { get; set; } // 1:登入的使用者 2:匿名使用者
    public bool isAdmin { get; set; } // 是否為聊天室管理員
    public ulong? readMsgId { get; set; } // 該使用者上線已讀訊息的最大Id，訊息Id超過此數字會被標記為未讀
    public int status { get; set; } // 1:未封鎖 2:封鎖 如果是 1v1 聊天室，則不允許封鎖
    public DateTime? createTime { get; set; }
    public DateTime? actTime { get; set; }
  */
  }),
  // 使用者的 chatRoomUser
  chatRoomUsers_User: computed(() => {
    return store.chatRoomUsers['3@1@1']
    // let userId = store.system.userId
    // let roomId = store.system.roomId
    // let key = userId + '@' + roomId
    // if (roomId !== null && store.chatRoomUsers.hasOwnProperty(key))
    //   return store.chatRoomUsers['3@1@1']
    // return null
  }),
  // 1v1 聊天室對方的 chatRoomUser
  chatRoomUsers_Target: computed(() => {
    let userId = store.system.userId
    let roomId = store.system.roomI
    let chatRoomUser = Object.values(store.chatRoomUsers).find(
      x => x.roomId === roomId && x.userId !== userId
    )
    return chatRoomUser ? chatRoomUser.readMsgId : null
  }),
  chatMsgs: reactive({
    /*
    public ulong id { get; set; } 
    public int roomId { get; set; } // Reference: ChatRoom -> id
    public int userId { get; set; } // Reference: User -> id
    public int type { get; set; } // 11文字/12圖片/13檔案/14已編輯文字/21加入聊天/22離開聊天
    public string? text { get; set; } // 如果是圖片或檔案就 NULL，如果是文字就直接存文字
    public int status { get; set; } // not null 1:未刪除 2:刪除
    public string? fileName { get; set; }  // 原本上傳的檔案名稱
    public string? hashName { get; set; }  // 上傳之後重新編碼過的檔案命稱
    public DateTime? createTime { get; set; } // 訊息建立的時間 
    public DateTime? actTime { get; set; } 
    */
  }),
  chatMsgTypes: reactive({
    /*
    public int id { set; get; }
    public string typeName { set; get; }
    */
  })
}

export default { store }
