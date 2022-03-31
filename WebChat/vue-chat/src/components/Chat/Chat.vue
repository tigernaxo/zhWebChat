<template>
  <div class="main-chat" v-if="isRoomSelected">
    <ChatHeader ref="chatHeader" />
    <ChatBody ref="chatBody" />
    <ChatFooter ref="chatFooter" @send="onMsgSend" />
  </div>
  <div class="col main-chat main-chat-unselect" v-else>
    <!-- 如果 roomId === null，就顯示佔位畫面 -->
    <div>
      選擇聊天室以開始交談
    </div>
  </div>
</template>

<script>
import logger from '@/zh/logger'
import { ref, inject, computed } from 'vue'
import ChatHeader from '../Chat/ChatHeader.vue'
import ChatBody from '../Chat/ChatBody.vue'
import ChatFooter from '../Chat/ChatFooter.vue'

export default {
  props: {
    test: String
  },
  components: {
    ChatHeader,
    ChatBody,
    ChatFooter
  },
  setup() {
    // component ref
    let chatHeader = ref(null)
    let chatBody = ref(null)
    let chatFooter = ref(null)

    const store = inject('store')

    // 是否已選中聊天室
    const isRoomSelected = computed(() => store.system.roomId !== null)
    // 註冊接收訊息的行為
    const wsService = inject('wsService')
    function onMsgSend() {
      try {
        // 呼叫 chatStore 的 wsMessageSend()
        wsService.wsMessageSend()
      } catch (error) {
        logger.error('[Chat][onMsgSend]' + error.toString())
      }
    }
    return { chatBody, chatFooter, chatHeader, onMsgSend, isRoomSelected }
  }
}
</script>

<style lang="scss" scoped>
.main-chat {
  flex: 1;
  min-width: 40%;
  background-color: #fff;
  min-height: 0px; // 使 flex 正常發揮作用
  max-height: 100vh; // 限制高度，交談文字多時避免Chat超出視窗
  display: flex;
  flex-direction: column;
}
.main-chat-unselect {
  height: 100%;
  width: 100%;
  color: #5682a3;
  background-color: #dddddd;
  text-align: center;
  justify-content: center;
  font-weight: bold;
}
</style>
