<template>
  <div class="main_chat_header">
    <ChatHeaderQRcode ref="qrcodeRef" :dataURL="data.ChatHeaderQRcode.dataURL" />
    <!-- 離開此聊天室按鈕，手機畫面才有，之後再做功能 -->
    <!-- ... 打開的選單 -->
    <button class="main-chat-header-btn" style="display:none">
      <i class="fas fa-arrow-left"></i>
    </button>
    <div class="row g-0 justify-content-between align-items-center h-100">
      <div class="col-auto ps-3">{{ title }}</div>
      <template v-if="store.system.userType === 1">
        <button
          type="button"
          class="main-chat-header-btn col-auto"
          @click.stop="data.showMenu = true"
          ref="btnToggleMenu"
        >
          <i class="fas fa-ellipsis-v"></i>
        </button>
      </template>
    </div>
    <template v-if="store.system.userType === 1">
      <div class="main-chat-header-menu-wrapper">
        <div class="main-chat-header-menu" ref="menu" :style="menuStyle">
          <ul class="list-group list-group-flush">
            <template v-if="store.roomId !== null">
              <template v-for="(item, idx) in menuItems" :key="idx">
                <li v-if="item.show" class="list-group-item" @click.stop="item.onclick">
                  <i :class="item.class"></i> {{ item.text }}
                </li>
              </template>
            </template>
          </ul>
        </div>
      </div>
    </template>
    <chat-header-group-invite
      ref="chatHeaderGroupInviteRef"
      @clole="$emit('close')"
    ></chat-header-group-invite>
  </div>
</template>

<script>
import { computed, inject, reactive, ref } from 'vue'
import logger from '@/zh/logger'
// import axios from 'axios'
import axios from '@/zh/axios'
import qrcode from 'qrcode'
import ChatHeaderQRcode from '../ModalQRcode.vue'
import ChatHeaderGroupInvite from './ChatHeaderGroupInvite.vue'
export default {
  components: {
    ChatHeaderQRcode,
    ChatHeaderGroupInvite
  },
  setup() {
    const store = inject('store')
    const wsService = inject('wsService')
    let data = reactive({
      showMenu: false,
      ChatHeaderQRcode: {
        dataURL: 'data:image/png;base64,'
      }
    })
    let qrcodeRef = ref(null)
    let chatHeaderGroupInviteRef = ref(null)
    // 使用者是否為本聊天室管理員
    let isGroupAdmin = computed(() => {
      let key = `${store.system.userId}@1@${store.system.roomId}`
      let isAdmin = store.chatRoomUsers[key]?.isAdmin ?? false
      let isGroup = store.chatRoom_Current?.value?.type !== 3
      return isAdmin && isGroup
    })
    function close() {
      data.showMenu = false
    }
    let btnToggleMenu = ref(null)
    let menu = ref(null)
    let axiosConf = {
      headers: { Authorization: `Bearer ${store.system.token}` }
    }
    let menuItems = computed(() => {
      // 判斷聊天室是否為群組
      let isGroup = store.chatRoom_Current?.value?.type !== 3
      return [
        {
          show: isGroupAdmin.value,
          text: '邀請匿名使用者',
          class: ['far', 'fa-qrcode'],
          onclick: async () => {
            try {
              close()
              let url = '/api/chatRoom/GroupQRCode'
              let fd = new FormData()
              fd.append('roomId', store.system.roomId)
              let res = await axios.post(url, fd, axiosConf)
              logger.debug(res.data)
              if (res.data.resultCode === '01') throw `${res.data.error}`
              let token = res.data.token
              // 檢查 token
              let isValidToken = token?.length ?? 0 > 0
              if (!isValidToken) throw `不合法的 Token :${token}`

              // 產生dataURL
              var str = `${window.location.origin}?token=${token}`
              logger.debug(str)
              data.ChatHeaderQRcode.dataURL = await qrcode.toDataURL(str)
              qrcodeRef?.value.open()
            } catch (error) {
              logger.error(error)
            }
          }
        },
        {
          show: isGroupAdmin.value,
          text: '邀請使用者',
          class: ['far', 'fa-user-plus'],
          onclick: () => {
            close()
            test('邀請使用者...')
            chatHeaderGroupInviteRef.value.open()
          }
        },
        // {
        //   show: true,
        //   text: '搜尋文字',
        //   class: ['far', 'fa-search'],
        //   onclick: () => {
        //     close()
        //     logger.log('searching...')
        //   }
        // },
        {
          show: isGroup,
          text: '離開群組',
          class: ['far', 'fa-sign-out-alt'],
          onclick: () => {
            close()
            wsService.removeCurrentChatRoom()
          }
        },
        {
          show: !isGroup,
          text: '刪除對話',
          class: ['fal', 'fa-trash-alt'],
          onclick: () => {
            close()
            wsService.removeCurrentChatRoom()
            test()
          }
        }
      ]
    })
    let menuStyle = computed(() => {
      return {
        display: data.showMenu ? 'block' : 'none'
      }
    })
    let isGroup = computed(() => store.chatRoom_Current.value?.type !== 3)
    let title = computed(() => {
      // 如果是群組就回傳群組標題
      if (isGroup.value) return store.chatRoom_Current.value?.title
      // 如果不是群組就取得對方的顯示名稱
      let userId2Idx = store.chatRoom_Meta_Current?.value.chatRoomUsers.find(
        x => !x.startsWith(store.system.userId)
      )
      let userId2 = userId2Idx?.split('@')[0]
      let userId2Type = userId2Idx?.split('@')[1]
      let userName = store.users[`${userId2}@${userId2Type}`]?.userName ?? '未知使用者'
      return `與${userName}的對話`
    })
    // 綁定事件：點擊外面就關閉 menu
    document.addEventListener('click', function(e) {
      // 如果 model 正在顯示，判斷是否需要關閉
      if (data.showMenu) {
        // 好像無法直接取得 slot 內的 dom ref，用 slots 會取得 vdom，因此用上層dom間接取得
        let el = menu.value.querySelectorAll('*')[0]
        // // 判斷是否點選到外面的元素
        let isClickOutSide = !el.contains(e.target) && el !== e.target
        // 如果點選到外面的元素，就關掉 model
        if (isClickOutSide) {
          logger.log('closing config page...')
          data.showMenu = false
        }
      }
    })
    function test(str) {
      str = str ?? 'test'
      logger.debug(str)
    }
    return {
      isGroupAdmin,
      store,
      data,
      title,
      btnToggleMenu,
      menu,
      menuStyle,
      close,
      menuItems,
      qrcodeRef,
      chatHeaderGroupInviteRef
    }
  }
}
</script>

<style lang="scss" scoped>
.main_chat_header {
  flex: 0 0 auto;
  height: 50px;
  word-wrap: none;
  background-color: #5682a3;
  color: #fff;
  font-size: 1.5em;
}
.main_chat_header_title_wrapper {
  display: inline-block;
  height: 100%;
}
.main_chat_header_title {
  display: flex;
  justify-content: center;
  height: 100%;
}
.main-chat-header-btn,
.main-chat-header-btn:active {
  display: inline-block;
  outline: none;
  height: 100%;
  cursor: pointer;
  background-color: #5682a3;
  color: #fff;
  border: none;
  margin: 0px 5px;
  padding: 0 15px;
}
.main-chat-header-menu-wrapper {
  position: relative;
}
.main-chat-header-menu {
  position: absolute;
  right: 0;
  top: 0;
  background-color: white;
  color: black;
  border-radius: 5px;
  padding: 5px;
  font-size: 1.3rem;
  box-shadow: rgba(0, 0, 0, 0.12) 0px 1px 3px, rgba(0, 0, 0, 0.24) 0px 1px 2px;
  z-index: 1;
}
.main-chat-header-menu li {
  cursor: pointer;
}
.list-group-item {
  font-size: 0.8em;
}
</style>
