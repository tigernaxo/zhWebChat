<template>
  <div class="main_chat_body" ref="msgContainer">
    <div class="flex-shrink-0 flex-grow-0 px-3">
      <!-- todo:如果 isOneToOneRoom &&  readMsgTarget null 就渲染對方已讀的 icon -->
      <!-- 如果使用者沒有紀錄已讀訊息，且聊天室有訊息，就在開頭渲染以下未讀的訊息 -->
      <template v-if="store.chatRoom_Meta_Current.value !== null">
        <div
          class="d-flex justify-content-center"
          v-if="isOneToOneRoom && !readMsgIdOld && roomHaveMessages"
        >
          <div class="unread d-inline-block">以下為未讀訊息</div>
        </div>
        <template
          v-for="(msgId, idx) in store.chatRoom_Meta_Current?.value?.messages"
          :key="msgId"
        >
          <!-- debug -->
          <!-- {{ store.chatMsgs[msgId] }} -->
          <ChatBodyMsg :msg="store.chatMsgs[msgId]" @mounted="bottomizeChat" />
          <!-- 如果本則訊息為使用者已讀訊息，且有更新的訊息，就渲染以下未讀的訊息 -->
          <div
            class="d-flex justify-content-center"
            v-if="
              msgId === readMsgIdOld &&
                idx + 1 !== store.chatRoom_Meta_Current?.value?.messages.length
            "
          >
            <div class="unread d-inline-block">
              以下為未讀訊息
            </div>
          </div>
          <!-- todo:如果 isOneToOneRoom && msg.id === readMsgTarget 就渲染對方已讀的 icon -->
        </template>
      </template>
    </div>
  </div>
</template>

<script>
import { onMounted, ref, inject, computed } from 'vue'
import ChatBodyMsg from './ChatBodyMsg.vue'

export default {
  components: {
    ChatBodyMsg
  },
  setup() {
    const msgContainer = ref(null)
    const store = inject('store')

    // 使用者已讀的 readMsgId
    let readMsgIdOld = computed(() => store.chatRoom_Meta_Current?.value?.readMsgIdOld)
    // 對方已讀的 readMsgId
    const readMsgIdTarget = computed(() => store.chatRoomUsers_Target?.readMsgId ?? null)
    const isOneToOneRoom = store.chatRoom_Current?.value?.type === 3
    let roomHaveMessages = computed(
      () => store.chatRoom_Meta_Current?.value?.messages.length > 0
    )

    // [function] 重新置底對話
    function bottomizeChat() {
      msgContainer.value.scrollTop = msgContainer.value.scrollHeight
    }
    // App 掛載時將對話置底
    onMounted(() => bottomizeChat())
    return {
      msgContainer,
      bottomizeChat,
      store,
      readMsgIdOld,
      readMsgIdTarget,
      isOneToOneRoom,
      roomHaveMessages
    }
  }
}
</script>

<style lang="scss" scoped>
.main_chat_body {
  flex: 1 1 auto;
  min-height: 0;
  min-width: 1;
  margin: 0.1em;
  background-color: #e8e8e8;
  border: 1px solid #e9ebed;
  overflow: auto;
}
.unread {
  padding: 1px 15px;
  border-radius: 20px;
  background-color: rgb(228, 228, 228);
  font-size: 0.9rem;
  text-align: center;
  filter: drop-shadow(1px 1px 3px rgba(0, 0, 0, 0.7));
}
</style>
