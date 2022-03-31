<template>
  <div class="row g-0 main">
    <List v-if="store.system.userType === 1" />
    <Chat />
    <Menu v-if="store.system.userType === 1" ref="menuRef" @close="onMenuClose()" />
  </div>
</template>
<script>
import { inject, onMounted, ref, watch } from 'vue'
import logger from '@/zh/logger'
import List from '../components/List/List.vue'
import Chat from '../components/Chat/Chat.vue'
import Menu from '../components/Menu/Menu.vue'

export default {
  name: 'Main',
  components: {
    List,
    Chat,
    Menu
  },
  setup() {
    const store = inject('store')
    // const storeService = inject('storeService')
    const onMenuClose = function() {
      store.system.showConfig = false
    }

    const openConfig = function() {
      if (menuRef?.value) menuRef?.value.open()
    }
    watch(
      () => store.system.showConfig,
      () => {
        if (store.system.showConfig) openConfig()
      }
    )

    let menuRef = ref(null)
    // 送出訊息處理邏輯
    const wsService = inject('wsService')
    onMounted(async () => {
      logger.log('[Main]Starting websocket...')
      await wsService.start() // 開始連線
      // storeService.DB_Init() // 初始化 indexedDB
    })
    return { menuRef, onMenuClose, openConfig, store }
  }
}
</script>

<style lang="scss" scoped>
.main {
  // align-items: stretch;
  // overflow: hidden;
  width: 100%;
  height: 100%;
}
</style>
