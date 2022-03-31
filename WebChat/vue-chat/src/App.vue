<template>
  <Main v-if="store.system.isLogin" />
  <Login v-else />
</template>

<script>
import { onMounted, provide } from 'vue'
import Main from '@/views/Main.vue'
import Login from '@/views/Login.vue'
import { store } from '@/store'
import { wsService, storeService } from '@/service'
import jwt from '@/zh/jwt'
import logger from './zh/logger'
import axios from 'axios'
import config from '../vue.config'

axios.defaults.baseURL = config.baseURL
export default {
  components: {
    Main,
    Login
  },
  setup() {
    provide('wsService', wsService)
    provide('store', store)
    provide('storeService', storeService)
    provide('axios', axios)

    onMounted(() => {
      logger.log('onMounted')
      // 取得 token
      let path = document.URL
      let origin = window.location.origin
      path = path.substring(origin.length)
      path = path.startsWith('/') ? path.substring(1) : path
      path = path.startsWith('chat') ? path.substring(4) : path
      path = path.startsWith('/') ? path.substring(1) : path
      let searchParams = new URLSearchParams(path)
      // 如果有 token 就進行下面的動作
      if (searchParams.has('token')) {
        let token = searchParams.get('token')
        let payload = jwt.parseJwt(token)
        logger.log('payload')
        logger.log(payload)

        Object.assign(store.system, {
          isLogin: true,
          token,
          userType: 2,
          userId: parseInt(payload.id),
          userName: payload.userName,
          roomId: payload.roomId
        })
        logger.log('store.system')
        logger.log(store.system)
      }
    })

    /* 為安全起見暫時不儲存 token
    // 掛載時對 localStorage 所紀載 Token 的行為
    onMounted(async () => {
      // 從 localStoreage 取出 token
      let token = (store.system.token = localStorage.getItem('token'))
      logger.log('APP.onMount, store.system.token: ' + token)

      // 如果有 token，嘗試刷新驗證
      if (token === '') return
      let validitySec = jwt.validitySec(token)
      logger.debug('找到儲存的 token，嘗試驗證...，有效期間剩餘秒數：' + validitySec)

      // 如果 token 有效期間大於等於10秒，嘗試驗證並刷新 token...
      // 驗證通過就允許更新 token，所以只會同時發生或不發生
      if (validitySec < 10) return
      logger.debug('token 有效期間大於等於10秒，嘗試驗證並刷新 token...')
      let res = await axios.post('/api/Token/refresh', { token })
      if (res.data.resultCode == '01') {
        throw res.data.error
      }
      // 如果驗證並刷新成功，isLogin = true、直接進 Main
      // 如果驗證並刷新不通過，isLogin = fasle、清除 token，回到 Login 等待
    })
      */
    return { store }
  }
}
</script>
<style>
#app {
  position: absolute;
  left: 0;
  top: 0;
  height: 100% !important;
  width: 100% !important;
}
</style>
