<template>
  <div class="main-list-body-list" :class="{ selected: isSelected }" v-if="room.id !== 0">
    <div class="row g-0">
      <div class="col-2 align-middle">
        <i
          class="fas"
          :class="{
            'fa-users': isGroup,
            'fa-user-alt': !isGroup,
            'text-success': isSelected
          }"
        ></i>
      </div>
      <div class="col msg-title">{{ title }}</div>
    </div>
    <div class="row g-0">
      <div class="col last-msg" :class="{ 'unread-msg': unreadCnt !== 0 }">
        {{ lastMsg }}
      </div>
      <div class="col-2">
        <span class="badge rounded-pill " :class="{ 'unread-badge': unreadCnt !== 0 }">{{
          unreadCnt === 0 ? '&nbsp;' : unreadCnt
        }}</span>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, inject } from 'vue'
import file from '@/zh/file'
export default {
  props: {
    room: Object
  },
  setup(props) {
    let store = inject('store')
    let isGroup = computed(() => props.room.type !== 3)
    let title = computed(() => {
      if (isGroup.value) return props.room.title
      let userInfoArr = store.chatRooms_Meta[props.room.id]?.chatRoomUsers
        .find(x => !x.startsWith(store.system.userId))
        ?.split('@')
      if (!userInfoArr) return ''
      let userId2 = userInfoArr[0]
      let userType2 = userInfoArr[1]
      return `${
        parseInt(userType2) === 2
          ? '匿名使用者'
          : store.users[`${userId2}@${userType2}`]?.userName ?? '未知使用者'
      }`
    })
    let unreadCnt = computed(() => {
      let key = `${store.system.userId}@${store.system.userType}@${props.room.id}`
      let chatRoomUser = store.chatRoomUsers[key]
      if (!chatRoomUser) return null
      let readMsgId = chatRoomUser.readMsgId
      let roomMsgs = store.chatRooms_Meta[props.room.id]?.messages
      if (!roomMsgs) return 0
      let readMsgIdx = roomMsgs.findIndex(x => x === readMsgId)
      // // 計算未讀數量，已有考慮 readMsgIdx = -1 的情況
      let cnt = roomMsgs.length - (readMsgIdx + 1)
      // 以 string 回傳 cnt，如果超過 99 就設置為 99+
      return cnt > 99 ? '99+' : cnt
    })
    let lastMsg = computed(() => {
      let roomMsgs = store.chatRooms_Meta[props.room.id]?.messages
      if (!roomMsgs) return ''
      let msgId = roomMsgs.length > 0 ? roomMsgs[roomMsgs.length - 1] : null
      if (msgId === null) return ' '
      let msg = store.chatMsgs[msgId]
      let userName =
        msg.userType === 2
          ? `匿名${msg.userId}`
          : store.users[`${msg.userId}@${msg.userType}`]?.userName ?? '未知使用者'

      let text
      switch (msg.type) {
        case 11: // 文字
          text = msg.text
          break
        case 12: // 圖片
        case 13: // 檔案
          text = file.isImage(msg.fileName) ? '圖片' : '檔案'
          break
        default:
      }
      return ` ${userName}:${text}`
    })
    let isSelected = computed(() => store.system.roomId === props.room.id)

    return { isGroup, unreadCnt, props, lastMsg, isSelected, store, title }
  }
}
</script>

<style lang="scss" scoped>
.main-list-body-list {
  cursor: pointer;
  font-size: 1.1em;
  flex: 0 0 auto;
  color: #595959;
  border-bottom: #e9edf0 1px solid;
  padding: 6px;
}
.main-list-body-list:hover {
  background-color: #d8d8d8;
}
.selected {
  background-color: #e7e7e7;
}
.last-msg,
.msg-title {
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow-x: hidden;
}
.unread-badge {
  background-color: #3a7eb3;
}
.unread-msg {
  font-weight: bold;
}
</style>
