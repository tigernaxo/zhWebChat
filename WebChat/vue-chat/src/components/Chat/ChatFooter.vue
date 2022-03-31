<template>
  <div class="row g-0 main-chat-footer">
    <InputFile v-if="file" :fileName="file.name" @cancel="fileCancel()"></InputFile>
    <InputText v-else @send="$emit('send')" :ref="e => (refs.draftMsg = e)"></InputText>
    <InputInvoker
      v-show="!file"
      @file="refs.iconFile.click()"
      @emoji="refs.iconEmoji.toggle($event)"
    ></InputInvoker>
    <EmojiPicker :ref="e => (refs.iconEmoji = e)" :style="pickerStyle" @emoji="addEmoji">
    </EmojiPicker>
    <input
      v-show="false"
      type="file"
      :ref="e => (refs.iconFile = e)"
      @change="fileSelect($event)"
    />
    <button
      type="button"
      class="col-1 btn-submit"
      @click.stop="file ? fileSend() : $emit('send')"
    >
      送出
    </button>
  </div>
</template>

<script>
import logger from '@/zh/logger'
import { inject, onMounted, onUnmounted, reactive, ref } from 'vue'
import EmojiPicker from '@/zh/emoji/EmojiPicker.vue'
import InputFile from './ChatFooterInputFile'
import InputText from './ChatFooterInputText'
import InputInvoker from './ChatFooterInputInvoker.vue'
// import axios from 'axios'
import axios from '@/zh/axios'
export default {
  components: { EmojiPicker, InputFile, InputText, InputInvoker },
  setup() {
    const store = inject('store')
    let refs = reactive({
      draftMsg: null,
      iconEmoji: null,
      iconFile: null
    })
    let file = ref(null)
    function fileSelect(e) {
      var files = e.target.files || e.dataTransfer.files
      if (!files.length) return
      file.value = files[0]
      logger.log(`已夾帶檔案:`)
      logger.log(file)
    }
    function fileCancel() {
      file.value = null
      refs.iconFile.value = ''
    }
    async function fileSend() {
      // todo sendfile
      let config = {
        headers: { Authorization: `Bearer ${store.system.token}` }
      }
      let url = '/api/ChatRoom/Upload'
      try {
        let fd = new FormData()
        fd.append('roomId', store.system.roomId)
        fd.append('file', file.value)
        let res = await axios.post(url, fd, config)
        if (res.data.resultCode === '01') throw res.data.error
        logger.log(res.data)
        fileCancel()
      } catch (error) {
        alert(error)
        logger.error(error)
      }
    }
    const addEmoji = function(emoji) {
      store.chatRoom_Meta_Current.value.draftMsg += emoji
    }

    // emoji priker Style
    let pickerStyle = reactive({
      height: '300px',
      width: '100px',
      position: 'fixed',
      left: '0px',
      bottom: '100px'
    })
    function recalculate() {
      logger.debug('recalculate')
      logger.debug(refs.draftMsg?.domRef.offsetWidth)
      logger.debug(window.innerWidth)
      pickerStyle.width = (refs.draftMsg.domRef.offsetWidth ?? 0) + 'px'
      pickerStyle.left = (refs.draftMsg.domRef.offsetLeft ?? 0) + 'px'
      pickerStyle.bottom = (refs.draftMsg.domRef.offsetHeight ?? 0) + 'px'
      pickerStyle.height = (refs.draftMsg.domRef.offsetHeight * 3 ?? 100) + 'px'
    }
    onMounted(() => {
      window.addEventListener('resize', recalculate)
      recalculate()
    })
    onUnmounted(() => window.removeEventListener('resize', recalculate))

    return {
      refs,
      file,
      fileSelect,
      fileSend,
      fileCancel,
      // iconInvokers,
      store,
      addEmoji,
      pickerStyle
    }
  }
}
</script>

<style lang="scss" scoped>
.main-chat-footer {
  box-sizing: border-box;
  height: 90px;
  margin: 0.1em;
  background-color: #fff;
  border: 1px solid #e9ebed;
  overflow: auto;
  flex: 0 0 auto;
  padding: 2px;
}
.btn-submit {
  border: none;
  background-color: #5682a3;
  color: white;
  padding: 15px 32px;
  text-align: center;
  text-decoration: none;
  display: inline-block;
  font-size: 16px;
  border-radius: 5px;
  margin-left: 2px;
  font-weight: bold;
}
.btn-submit:active {
  background-color: #3f5e75;
}
.btn-submit:focus {
  border: none;
  outline: none;
}
</style>
