export let FrontEvents = {
  App: {
    Init: [
      '進入登入頁面 Login.vue' //ok
    ]
  },
  Login: [
    '將 Token 紀載到前端的 javascript 變數上', // ok
    '取得聯絡人清單(Users)', // ok SocialContactGet
    '取得可得的房間清單(ChatRoom)', // ok ChatRoomGetAll
    '取得[使用者]和[1v1聊天室對方使用者]的 ChatRoomUser list,  ', // ok ChatRoomUserGetAll
    '顯示前端頁面 Main.vue' // ok
  ],
  ChatRoom: {
    Enter: [
      '通知伺服器已讀所有訊息(ChatRoomUser設定為前端最大的 message Id)', // MessageUserRead
      '更新其他聊天室的已讀訊息(目的為更新其他聊天室顯示的以下未讀)',
      '伺服器通知其他使用者該使用者已讀'
    ],
    Leave: ['將非選中的聊天室 ChatRoom_Meta.readMsgOld 同步到 chatRoomUser.readMsgId']
  },
  Message: {
    Send: [
      '將發送的訊息本身設定為已讀' // todo
    ],
    Receive: [
      '如果收到訊息時顯示訊息的 ChatRoom 被選中，通知伺服器使用者已讀取該訊息' // ok MessageUserRead
    ]
  },
  User: {
    Add: ['添加一筆 user relation 資料']
  }
}
export const ServerEvents = {
  Message: {
    Send: [
      // MessageSend
      '將使用者自行發送的訊息設定為已讀',
      '將訊息發送給監聽該房間的線上使用者'
    ],
    UserRead: [
      // MessageUserRead
      '在 ChatRoomUser 資料表更新 readMsgId',
      '若是 1v1 聊天室發出的已讀通知就通知對方已讀取'
    ]
  }
}

export let issue = {
  1: {
    done: false,
    desc: '上線時對使用者傳送訊息，使用者可能打開或未打開聊天室，如何判斷為已讀？',
    proposal: [
      '定案:伺服器端端新增一個 ChatRoomUserSetReadMsgId 供使用者呼叫，設定已讀訊息，收到訊息時打開該聊天室視窗就算已讀'
    ]
  },
  2: {
    done: false,
    desc: '如何知道其他使用者已讀取的訊息範圍？ 要儲存其他使用者的 ChatRoomUser 嗎？',
    proposal: [
      '方案1:儲存其他使用者的 ChatRoomUser，此方法可以追蹤哪些使用者已讀取',
      '方案2:在 ChatMessage 新增一欄位，為已讀使用者數，速度快，但無法追蹤哪些使用者已讀取、易與真實情況脫鉤' // deprecated
    ],
    resolve: [
      '一般來說 post 才需要顯示已觀看的使用者細節本案不實作，只需在兩個人的聊天室中顯示對方是否已讀',
      '階段1:單人聊天是否已讀較簡單(單純取得對方的 ChatRoomUser 即可)',
      '階段2:多人聊天，消耗較多系統資源決定本案不實做' // deprecated
    ]
  },
  3: {
    done: false,
    desc: '如何載入歷史資料(部分或全部)？載入的時機點？',
    proposal: [
      '階段1：一律不載入', // ok 目前情況
      '階段2:應先實作前端快取，登入時候載入全部，會造成很大的系統負擔',
      '階段3:應先實作前端快取、防顫，登入時載入部分，隨滑鼠滾動依賴載入其餘部分'
    ]
  },
  4: {
    done: false,
    desc: '單方面添加聯絡人關係，應該於聯絡人清單顯示嗎？',
    proposal: [
      '階段1: 一率先顯示，只有被封鎖才不顯示', // 較簡單先實做
      '階段2: 開表格紀錄使用者邀請訊息，接受邀請之後兩方建立關係 (UserRelationShip)，之後才能互相看到'
    ]
  },
  5: {
    done: false,
    desc: '封鎖與被封鎖可傳訊？',
    proposal: [
      '被封鎖:訊息只能傳到 server 無法到對方，但可以看見自己的訊息',
      '封鎖對方:對方訊息只能傳到 server 無法到己方，對方可以看到對方傳的訊息',
      '結論：被封鎖的人1v1聊天訊息不發送給對方，或1v1聊天不接收封鎖的使用者'
    ]
  },
  6: {
    done: false,
    desc: '使用者已刪除的1v1聊天室(沒有被封鎖)，傳訊是否要顯示?',
    proposal: [
      '不顯示？那如何加回去',
      '應該顯示：上線時應該納入監聽，一旦對方丟訊息，使用者收到發現沒有房間，通知 server 把 chatRoomUser 調整成未刪除'
    ]
  }
}

// 撰寫本 App 遵守原則
export let princeple = [
  '使用者呼叫伺服器預期獲得 Response(類似 http)，伺服器丟給使用者不預期獲得 Response(廣播)',
  '參考 window.open MDN API 說明，windowFeatures 參數不為空時預設像 Hangouts 隱藏上面的 Tab 等等'
]

export let todos = [
  '前台設定頁面(基本資料/大頭貼/黑名單/聯絡人)',
  '前台設定頁面-建立聊天群組', // ok
  '前台設定頁面-前台大頭貼顯示', // ok
  '聯絡人清單(點選以開始1v1聊天)',
  '聯絡人清單-黑名單設定',
  '聯絡人清單-在線/離線顯示', // 不做
  '聊天室-檔案上傳',
  '聊天室-已讀/未讀訊息',
  '聊天室-訊息(文字/圖片/檔案)搜尋',
  '聊天室-QRCode 讓匿名使用者進入對話',
  '聊天室-事件紀錄',
  'WebChat 後台...'
]

export let toImprove = {
  釐清State架構層: [
    'ChatHub 所記錄的 in memory state (userMap、roomInfo) 應視為 Repo 操作',
    'in memory state 一樣上面可以有Service(這裡是Factory)，未來才方便抽換 DAL'
  ],
  避免過度設計: [
    '以三層式架構存取複雜sql造成大多BLL直接轉DAL形成，下次可考慮用 ORM 搭配 BLL，直接拿掉 separate DAL',
    '在多層式架構中 DAL 若只負責基本的 CRUD，會過度依賴 BLL 的 unit of work 造成效能極差',
    '承上，所以在多層式架構中 ORM 資料庫查詢可以使用複雜語法沒問題',
    '若要使用三層式架構可適時將相關的表格合併成一個 Repo/BLL (例如明細檔和主檔使用同一個 Repo/BLL)'
  ]
}
