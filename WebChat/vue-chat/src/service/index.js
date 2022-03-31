import { store } from '@/store'
import { logger, ws } from '@/zh'
import { watch } from 'vue'
import Dexie from 'dexie'

// 定義 ws 客戶端/伺服器端事件
let EventServer = {
  InitDB: 'InitDB',
  MessageSend: 'MessageSend',
  SocialUserGet: 'SocialUserGet',
  SocialUserGetALL: 'SocialUserGetALL',
  SocialRelationGetALL: 'SocialRelationGetALL',
  ChatRoomGetAll: 'ChatRoomGetAll',
  ChatRoomGet: 'ChatRoomGet',
  ChatRoomUserGetAll: 'ChatRoomUserGetAll',
  ChatRoomUserGet: 'ChatRoomUserGet',
  MessageUserRead: 'MessageUserRead',
  ChatRoomCreateGroup: 'ChatRoomCreateGroup',
  ChatRoomRemove: 'ChatRoomRemove',
  ChatRoomCreateOneToOne: 'ChatRoomCreateOneToOne',
  ChatMsgUpdateRequest: 'ChatMsgUpdateRequest',
  AnonymousGetUser: 'AnonymousGetUser'
}
let EventClient = {
  // ws 接收訊息事件
  SystemSetRoom: {
    Name: 'SystemSetRoom',
    Handler: async function(roomId) {
      logger.debug(`SystemSetRoom data: ${roomId}`)
      store.system.roomId = roomId
    }
  },
  MessageReceive: {
    Name: 'MessageReceive',
    Handler: async function(chatMsg) {
      logger.debug('MessageReceive data:')
      logger.debug(chatMsg)
      if (store.db) store.db.chatMsgs.add(chatMsg) // 將訊息存入 indexedDB
      // 將取得的訊息加入房間的 message list
      if (!store.chatRooms[chatMsg.roomId]) {
        // 請求
        await ws.invoke(EventServer.ChatRoomGet, chatMsg.roomId)
        await ws.invoke(EventServer.ChatRoomUserGet, chatMsg.roomId)
      }
      let room_meta = store.chatRooms_Meta[chatMsg.roomId]
      room_meta.messages.push(chatMsg.id)
      store.chatMsgs[chatMsg.id] = chatMsg
      // 檢查前端儲存聯絡人資訊是否存在，否則呼叫伺服器取得
      // eslint-disable-next-line no-prototype-builtins
      if (!store.users.hasOwnProperty(`${chatMsg.userId}@${chatMsg.userType}`)) {
        await ws.invoke(EventServer.SocialUserGet, [chatMsg.userId])
      }
      let isSelfMsg = chatMsg.userId === store.system.userId
      let isCurrentRoom = store.system.roomId === chatMsg.roomId
      logger.debug(`isSelfMsg: ${isSelfMsg}`)
      logger.debug(`isCurrentRoom: ${isCurrentRoom}`)
      logger.debug(`chatMsg:`)
      logger.debug(chatMsg)
      if (isSelfMsg) {
        // 如果是自已送的訊息，locale 端設定為已讀就好(server 端 MessageSend 被呼叫的時候已經設定過)
        store.chatRoomUsers[
          `${chatMsg.userId}@${store.system.userType}@${store.system.roomId}`
        ].readMsgId = chatMsg.id
        store.chatRooms_Meta[store.system.roomId].readMsgIdOld = chatMsg.id
      } else if (isCurrentRoom) {
        store.chatRooms_Meta[store.system.roomId].readMsgIdOld = chatMsg.id
        // 如果不是自己送的訊息，但訊息是傳送到被選中的房間，通知伺服器該訊息已讀
        setReadMsgId()
      }
    }
  },
  SocialUserGetResponse: {
    Name: 'SocialUserGetResponse',
    Handler: users => {
      logger.log('SocialUserGetResponse data:')
      logger.log(users)
      users.forEach(u => (store.users[`${u.id}@1`] = u)) // 添加到 user 資訊當中
    }
  },
  SocialUserGetALLResponse: {
    Name: 'SocialUserGetALLResponse',
    Handler: users => {
      logger.log('SocialUserGetALLResponse data:')
      logger.log(users)
      users.forEach(u => (store.users[`${u.id}@1`] = u)) // 添加到 user 資訊當中
    }
  },
  SocialRelationGetALLResponse: {
    Name: 'SocialRelationGetALLResponse',
    Handler: function(list) {
      logger.log('SocialRelationGetALLResponse')
      logger.log(list)
      list.forEach(x => {
        let key = `${x.userId}@${x.userId2}@${x.type}`
        store.userRelationShips[key] = x
      })
    }
  },
  ChatRoomGetAllResponse: {
    Name: 'ChatRoomGetAllResponse',
    Handler: roomList => {
      logger.log('ChatRoomGetAllResponse data:')
      logger.log(roomList)
      roomList.forEach(r => {
        storeService.ChatRoom_Set(r)
        // 前端專有的 meta data
        storeService.ChatRoom_Meta_Set(r.id)
      })
    }
  },
  ChatRoomUserGetAllResponse: {
    Name: 'ChatRoomUserGetAllResponse',
    Handler: data => {
      logger.log('ChatRoomUserGetAllResponse data:')
      logger.log(data)
      data.forEach(x => {
        let key = `${x.userId}@${x.userType}@${x.roomId}`
        store.chatRoomUsers[key] = x
        // logger.log(store.chatRooms_Meta[x.roomId].chatRoomUsers)
        // ??? 這裡為何無法判斷呢？
        // if (store.chatRooms_Meta[x.roomId].chatRoomUsers.find(key) === undefined)
        // logger.log(`assigning chatRooms_Meta.chatRoomUsers., key:${key}`)
        store.chatRooms_Meta[x.roomId].chatRoomUsers.push(key)
        // 如果是自己的已讀資訊，就同步到 chatRooms_Meta
        if (x.userId === store.system.userId) {
          store.chatRooms_Meta[x.roomId].readMsgIdOld = x.readMsgId
        }
      })
      logger.log('store.chatRooms_Meta')
      logger.log(store.chatRooms_Meta)
      logger.log('store.chatRoomUsers')
      logger.log(store.chatRoomUsers)
    }
  },
  ChatRoomAdd: {
    Name: 'ChatRoomAdd',
    Handler: function(chatRoom, chatRoomUsers) {
      logger.log('ChatRoomAdd Start')
      wsService.addChatRoom(chatRoom, chatRoomUsers)
      logger.log('ChatRoomAdd End')
    }
  },
  ChatRoomAddUser: {
    Name: 'ChatRoomAddUser',
    Handler: function(chatRoomUser) {
      logger.log('ChatRoomAdd Start')
      wsService.addChatRoomUser(chatRoomUser)
      logger.log('ChatRoomAdd End')
    }
  },
  ChatRoomRemove: {
    Name: 'ChatRoomRemove',
    Handler: function(id) {
      logger.log(`ChatRoomRemove..., id:${id}`)
      delete store.chatRooms[id]
      store.system.roomId = null
    }
  },
  ChatRoomRemoveUser: {
    Name: 'ChatRoomRemoveUser',
    Handler: function(userId, userType, roomId) {
      logger.log(
        `ChatRoomRemoveUser..., userId:${userId}, userType: ${userType}, roomId:${roomId}`
      )
      store.chatRoomUsers[`${userId}@${userType}@${roomId}`].status = 3
    }
  },
  UserAdd: {
    Name: 'UserAdd',
    Handler: function(payload) {
      logger.log('UserAdd called, payload:')
      logger.log(payload)
      let key = `${payload.id}@${payload.userType}`
      store.users[key] = payload
    }
  },
  UserUpdate: {
    Name: 'UserUpdate',
    Handler: function(payload) {
      logger.log('UserUpdate called, payload:')
      logger.log(payload)
      store.users[`${payload.id}@1`].userName = payload.userName
      store.users[`${payload.id}@1`].photo = payload.photo
    }
  },
  Test: {
    Name: 'Test',
    Handler: function(payload) {
      logger.log('Test called, payload:')
      logger.log(payload)
    }
  },
  ChatMsgUpdate: {
    Name: 'ChatMsgUpdate',
    Handler: function(payload) {
      logger.log(`ChatMsgUpdate start`)
      logger.log(payload)
      payload.forEach(async x => {
        // 如果本來不在前端資料庫中，就補上 room_meta 的 messages
        if (!(await store.db.chatMsgs.get(x.id))) {
          // 將取得的訊息加入房間的 message list
          let room_meta = store.chatRooms_Meta[x.roomId]
          if (room_meta) room_meta.messages.push(x.id)
        }
        // 一律以回傳的取代前端資料庫與 js 內容
        store.db.chatMsgs.put(x)
        store.chatMsgs[x.id] = x

        // 檢查前端儲存聯絡人資訊是否存在，否則呼叫伺服器取得
        // eslint-disable-next-line no-prototype-builtins
        if (!store.users.hasOwnProperty(`${x.userId}@${x.userType}`)) {
          await ws.invoke(EventServer.SocialUserGet, [x.userId])
        }
      })
      logger.log(`ChatMsgUpdate end`)
      // 把拿到的 chatMsg 更新到 indexedDB
      // 把拿到的 chatMsg 添加到 js 緩存
    }
  },
  BanUser: {
    Name: 'BanUser',
    Handler: function(payload) {
      logger.log('BanContact called, payload:')
      logger.log(payload)
      storeService.UserRelationShips_Set(payload.userRelationShip)
      storeService.Users_Set(payload.user)
    }
  },
  UnBanUser: {
    Name: 'UnBanUser',
    Handler: function(payload) {
      logger.log('UnBanContact called, payload:')
      logger.log(payload)
      let key = `${store.system.userId}@${payload}@2`
      delete store.userRelationShips[key]
    }
  },
  ChatRoomCreateOneToOne: {
    Name: 'ChatRoomCreateOneToOne',
    Handler: function(payload) {
      logger.log(`[Start] ChatRoomCreateOneToOne call by server, payload:`)
      logger.log(payload)
      let room = payload.chatRoom
      storeService.ChatRoom_Set(room)
      storeService.ChatRoom_Meta_Set(room.id)
      payload.chatRoomUsers.forEach(x => {
        let key = `${x.userId}@${x.userType}@${x.roomId}`
        store.chatRoomUsers[key] = x
        store.chatRooms_Meta[x.roomId].chatRoomUsers.push(key)
      })
      store.system.roomId = room.id
    }
  },
  InitDB: {
    Name: 'InitDB',
    Handler: async function() {
      // 嘗試建立資料庫，名稱為 webchat_${userId}
      store.db = dbService.newDB(`webchat_${store.system.userId}`, 1)
      // 嘗試從 DB 撈取聊天訊息，先全部撈出來
      let arr = await store.db.chatMsgs.toArray()
      // 初期先把全部的撈出來
      logger.log(`store.db.chatMsgs count: ${arr.length}`)
      let usersToFetch = []
      arr.forEach(async x => {
        try {
          // 將取得的訊息加入房間的 message list
          let room_meta = store.chatRooms_Meta[x.roomId]
          // 如果房間還存在就加入記憶體(有可能被刪除)
          if (room_meta) {
            room_meta.messages.push(x.id)
            store.chatMsgs[x.id] = x
            // 檢查前端儲存聯絡人資訊是否存在，否則呼叫伺服器取得
            // eslint-disable-next-line no-prototype-builtins
            if (!store.users.hasOwnProperty(`${x.userId}@${x.userType}`)) {
              usersToFetch.push(x.userId)
            }
          }
        } catch (error) {
          logger.error(error)
          logger.log(x)
        }
        // EventClient.MessageReceive.Handler(x)
      })
      await ws.invoke(EventServer.SocialUserGet, usersToFetch)
      // 取得最後的時間並向伺服器要求更新
      let date = (await store.db.lastOnline.get(1))?.time || new Date('2000-01-01')
      let timeStr = getTimeStr(date)
      logger.log('timeStr:')
      logger.log(timeStr)
      // After this construct a string with the above results as below
      await ws.invoke(EventServer.ChatMsgUpdateRequest, timeStr)
      dbService.onlinePeekerTryAdd()
      // 開始將上線時間持續記錄到 indexedDB
      // 設置一個每秒更新前端上線時間的 timer (用來判斷哪些 indexedDB 資料需要更新)
      if (!store.system.timer) {
        store.system.timer = setInterval(() => {
          store.db.lastOnline.update(1, { id: 1, time: new Date() })
        }, 1000)
      }
    }
  }
}
//  註冊 ws 客戶端事件
Object.values(EventClient).forEach(event => {
  ws.on(event.Name, event.Handler)
})

// 監聽切換房間的行為
watch(
  () => store.system.roomId,
  () => {
    logger.log(`切換至房間${store.system.roomId}`)
    setReadMsgId()
    syncReadMsgId()
  }
)
const syncReadMsgId = function() {
  // 除了當下選中的 room 之外都同步
  logger.debug(`判斷要不要同步 chatRooms_Meta 中的已讀訊息`)
  let userId = store.system.userId
  let userType = store.system.userType
  let roomId
  Object.keys(store.chatRooms).forEach(key => {
    roomId = parseInt(key)
    if (roomId === store.system.roomId) return
    let readMsgId = store.chatRoomUsers[`${userId}@${userType}@${roomId}`]?.readMsgId
    logger.log(`userId: ${userId};roomId: ${roomId};readMsgId: ${readMsgId}`)
    logger.log(`store.chatRooms_Meta[roomId]: `)
    logger.log(store.chatRooms_Meta[roomId])
    store.chatRooms_Meta[roomId].readMsgIdOld = readMsgId
  })
}
// 更新 locale/remote 端的 chatRoomUser readMsgId
const setReadMsgId = function() {
  let userId = store.system.userId
  let roomId = store.system.roomId // 取得 roomId
  let userType = store.system.userType
  if (roomId === null) return
  // 取得該房間設置的最大 message  Id 作為新的 readMsgId
  let readMsgIdnew = store.chatRoom_Meta_Current?.value?.messages.slice(-1)[0]
  if (readMsgIdnew === undefined) return
  // 目前已讀的訊息
  let chatRoomUser = store.chatRoomUsers[`${userId}@${userType}@${roomId}`]
  let readMsgId = chatRoomUser.readMsgId
  // 如果有更新的訊息就通知
  if (readMsgId !== readMsgIdnew) {
    // 重新設定已讀的 messageId
    readMsgId = chatRoomUser.readMsgId = readMsgIdnew
    // 呼叫伺服器更新 ChatRoomUser
    ws.invoke(EventServer.MessageUserRead, roomId, readMsgId)
  }
  logger.log(`store.system.roomId:${store.system.roomId}`)
}
// 撰寫 service Layer，暴露給元件的服務層
// 如果是在 tab 頁面直接 f5，無法清除 timer，所以要先清除所有的 timer
var interval_id = window.setInterval('', 9999)
for (var i = 1; i < interval_id; i++) {
  window.clearInterval(i)
}
window.console.log('store.system.timer:')
window.console.log(store.system.timer)
ws.onclose(e => {
  window.console.error('[Func] ws.onclose')
  window.console.error(e)
  // 清除持續寫入資料庫的 timer
  if (store.system.timer) window.clearInterval(store.system.timer)
  store.system.wsState = ws.state
})
let wsService = {
  wsMessageSend: async function() {
    const errorBase = '@/service/index.js...[wsService][wsMessageSend]'
    try {
      let room = store.chatRoom_Current.value
      let room_meta = store.chatRoom_Meta_Current.value
      let userId = store.system.userId
      logger.log(store)
      logger.log(room)
      logger.log(room_meta)
      logger.log(userId)
      // 檢查 null
      if (room === null || room_meta === null) {
        throw `無法標記目前的聊天室`
      }
      if (userId === null) {
        throw `無法標記目前的使用者`
      }
      //送出訊息並重置 draftMsg
      let payload = {
        roomId: room.id,
        message: room_meta.draftMsg
      }
      if (room_meta.draftMsg?.length === 0) return
      logger.log(`invoking ${EventServer.MessageSend}, payload:`)
      logger.log(payload)
      await ws.invoke(EventServer.MessageSend, payload)
      room_meta.draftMsg = ''
    } catch (e) {
      logger.error(`${errorBase}:${e}`)
    }
  },
  start: async function() {
    await ws.start()
    store.system.wsState = ws.state
    store.system.ws = ws
    await ws.invoke(EventServer.ChatRoomGetAll)
    await ws.invoke(EventServer.ChatRoomUserGetAll)
    if (store.system.userType === 1) {
      await ws.invoke(EventServer.SocialUserGetALL)
      await ws.invoke(EventServer.SocialRelationGetALL)
      await ws.invoke(EventServer.InitDB)
    }
    if (store.system.userType === 2) {
      await ws.invoke(EventServer.AnonymousGetUser)
    }
  },
  addGroup: async function(title, announce, isPrivate, users) {
    await ws.invoke(
      EventServer.ChatRoomCreateGroup,
      title?.length === 0 ? null : title,
      announce?.length === 0 ? null : announce,
      isPrivate,
      users
    )
  },
  addChatRoomUser: function(chatRoomUser) {
    logger.log('addChatRoomUser')
    let key = `${chatRoomUser.userId}@${chatRoomUser.userType}@${chatRoomUser.roomId}`
    store.chatRoomUsers[key] = chatRoomUser
    if (!store.chatRooms_Meta[chatRoomUser.roomId].chatRoomUsers.includes(key))
      store.chatRooms_Meta[chatRoomUser.roomId].chatRoomUsers.push(key)
  },
  addChatRoom: function(chatRoom, chatRoomUsers) {
    logger.log('addChatRoom')
    storeService.ChatRoom_Set(chatRoom)
    storeService.ChatRoom_Meta_Set(chatRoom.id)
    chatRoomUsers.forEach(x => {
      logger.log('wsService.addChatRoomUser(x)')
      logger.log(x)
      wsService.addChatRoomUser(x)
    })
  },
  // 使用者刪除對話或離開群組
  removeCurrentChatRoom: function() {
    let id = store.chatRoom_Current.value?.id
    logger.log(`removeCurrentChatRoom...id:${id}`)
    ws.invoke(EventServer.ChatRoomRemove, id)
  },
  chatRoomCreateOneToOne: function(id) {
    logger.log(`chatRoomCreateOneToOne client side:id=${id}`)
    // 如果已經有和對方的 1v1 聊天室就直接進入該聊天室
    let room = storeService.ChatRoom_OneToOne_Get(id)
    if (room !== undefined) {
      store.system.roomId = room.id
      return
    }
    ws.invoke(EventServer.ChatRoomCreateOneToOne, id)
  }
}
let dbService = {
  name: '',
  prefix: 'webchat_',
  store: {
    chatMsgs: [
      'id',
      'roomId',
      'userId',
      'type',
      'text',
      'status',
      'fileName',
      'hashName',
      'createTime',
      'actTime'
    ].join(','),
    lastOnline: 'id, time'
  },
  async onlinePeekerTryAdd() {
    if (!(await store.db.lastOnline.get(1)))
      store.db.lastOnline.add({ id: 1, time: new Date() })
  },
  newDB(dbName, version) {
    this.name = dbName
    const db = new Dexie(this.name)
    db.version(parseInt(version)).stores(this.store) // 建立 table
    return db
  }
}
let storeService = {
  ChatRoom_Set(room) {
    store.chatRooms[room.id] = room
  },
  ChatRoom_Meta_Set(roomId) {
    store.chatRooms_Meta[roomId] = {
      draftMsg: '',
      readMsgIdOld: null,
      messages: [],
      chatRoomUsers: []
    }
  },
  // 只能找登入的 userId
  ChatRoom_OneToOne_Get(userId2) {
    // 從 chatRoom 找出 type === 3 的
    // 找出 chatRooms_Meta 的 chatRoomUsers 有 roomId@1@userId2 的
    let oneToOneRoomList = Object.values(store.chatRooms).filter(r => r.type === 3)
    logger.log('oneToOneRoomList')
    logger.log(oneToOneRoomList)
    return oneToOneRoomList.find(r =>
      store.chatRooms_Meta[r.id].chatRoomUsers.some(e => e.startsWith(`${userId2}@1`))
    )
  },
  UserRelationShips_Set(x) {
    let key = `${x.userId}@${x.userId2}@${x.type}`
    store.userRelationShips[key] = x
  },
  Users_Set(x) {
    let key = `${x.id}@${x.userType}`
    store.users[key] = x
  }
}
function padLeft(str, length, char) {
  let cnt = length - str.toString().length
  str = cnt > 0 ? `${char.toString().repeat(cnt)}${str}` : str
  return str
}
function getTimeStr(time) {
  var day = padLeft(time.getDate(), 2, '0') // yields date
  var month = padLeft(time.getMonth() + 1, 2, '0') // yields month (add one as '.getMonth()' is zero indexed)
  var year = padLeft(time.getFullYear(), 4, '0') // yields year
  var hour = padLeft(time.getHours(), 2, '0') // yields hours
  var minute = padLeft(time.getMinutes(), 2, '0') // yields minutes
  var second = padLeft(time.getSeconds(), 2, '0') // yields seconds
  return `${day}/${month}/${year} ${hour}:${minute}:${second}`
}
export { wsService, storeService }
