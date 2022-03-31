<template>
  <ModalContainer ref="configTmplate" @close="$emit('close')">
    <template #content>
      <div
        v-show="isLoading"
        class="d-flex justify-content-center align-items-center"
        :style="imgStyle"
      >
        <div class="loader"></div>
      </div>
      <img v-show="!isLoading" ref="imgRef" :src="dataURL" :style="imgStyle" />
    </template>
  </ModalContainer>
</template>

<script>
import { inject, reactive, ref } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import logger from '@/zh/logger'
// import axios from 'axios'
import axios from '@/zh/axios'
import qrcode from 'qrcode'
export default {
  components: {
    ModalContainer
  },
  setup() {
    let dataURL = ref('')
    let isLoading = ref(true)
    const store = inject('store')
    let configTmplate = ref(null)
    let imgRef = ref(null)
    let shrinkRatio = 0.7
    let imgStyle = reactive({
      position: 'fixed',
      top: '0px',
      left: '0px',
      width: 'auto',
      height: 'auto'
    })
    function open() {
      logger.log('opening invite qrcode')
      // 計算佔去視窗的比例
      let w = window.innerWidth
      let h = window.innerHeight
      let size = Math.ceil(w > h ? h * shrinkRatio : w * shrinkRatio)
      imgRef.value.width = imgRef.value.height = size
      imgStyle.left = Math.ceil((w - size) / 2) + 'px'
      imgStyle.top = Math.ceil((h - size) / 2) + 'px'
      imgStyle.width = imgStyle.height = size + 'px'

      // todo 取得 qrcode
      getQRCode()
      configTmplate.value.open()
    }
    async function getQRCode() {
      isLoading.value = true
      try {
        let config = {
          headers: { Authorization: `Bearer ${store.system.token}` }
        }
        let fd = new FormData()
        let res = await axios.post('/api/ChatRoom/OneToOneQRCode', fd, config)
        logger.log('getQRCode response:')
        logger.log(res.data)
        let token = res.data.token
        // 檢查 token
        let isValidToken = token?.length ?? 0 > 0
        if (!isValidToken) throw `不合法的 Token :${token}`
        var str = `${document.URL}?token=${token}`
        logger.log(str)
        dataURL.value = await qrcode.toDataURL(str)
      } catch (error) {
        logger.error(error)
      }
      isLoading.value = false
    }

    return { dataURL, isLoading, open, configTmplate, imgRef, imgStyle }
  }
}
</script>

<style lang="scss" scoped>
@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
.loader {
  border: 8px solid #00000000;
  border-radius: 50%;
  width: 50px;
  height: 50px;
  border-top: 8px solid #5682a3;
  animation: spin 1s linear infinite;
}
</style>
