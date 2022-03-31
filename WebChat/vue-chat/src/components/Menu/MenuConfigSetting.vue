<template>
  <ModalContainer ref="configTmplage" @close="$emit('close')">
    <template #content>
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="card card-setting">
            <MenuInfo />
            <div class="card-body">
              <form class="row g-0 align-items-center">
                <div class="col-12">
                  <label for="formFile" class="form-label">更換顯示名稱</label>
                  <div class="input-group">
                    <div class="input-group-text">顯示名稱</div>
                    <input type="text" class="form-control" ref="userNameRef" />
                  </div>
                </div>
                <div class="col-12 mt-3">
                  <label for="formFile" class="form-label">更換大頭貼</label>
                  <input class="form-control" type="file" id="formFile" ref="headRef" />
                </div>
                <button
                  class="submit"
                  @click.prevent.stop="submit()"
                  :disabled="isLoading"
                >
                  <template v-if="isLoading">
                    <i class="fas fa-spinner fa-pulse"></i>
                  </template>
                  <template v-else>
                    送出
                  </template>
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </template>
  </ModalContainer>
</template>

<script>
import { inject, ref } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import MenuInfo from './MenuInfo.vue'
// import axios from 'axios'
import axios from '@/zh/axios'
import logger from '../../zh/logger'
export default {
  components: {
    ModalContainer,
    MenuInfo
  },
  setup(props, { emit }) {
    const store = inject('store')
    let isLoading = ref(null)
    let configTmplage = ref(null)
    let headRef = ref(null)
    let userNameRef = ref(null)
    function open() {
      headRef.value.value = ''
      userNameRef.value.value =
        store.users[`${store.system.userId}@${store.system.userType}`].userName
      configTmplage.value.open()
    }
    async function submit() {
      try {
        // todo 顯示名稱不可為空白(目前空白設定對後台無效)
        isLoading.value = true
        logger.log(`submit...`)
        let fd = new FormData()
        let headFile = headRef.value.files[0]
        if (headFile?.size > 200 * 1024) throw '檔案太大 > 200KB'
        fd.append('userName', userNameRef.value.value)
        fd.append('headFile', headFile)
        let res = await axios.post('/api/User/Update', fd, {
          headers: { Authorization: `Bearer ${store.system.token}` }
        })
        logger.log('res.data:')
        logger.log(res.data)
        if (res.data.resultCode === '01') throw res.data.error
        headRef.value.value = '' // 取消選擇檔案
        // 關閉頁面
        configTmplage.value.isShow = false
        emit('close')
      } catch (err) {
        logger.error(err.toString())
      }
      isLoading.value = false
    }
    return { open, configTmplage, submit, headRef, userNameRef, isLoading }
  }
}
</script>

<style lang="scss" scoped>
.card {
  border: none;
  border-top-left-radius: 0.25rem;
  border-top-right-radius: 0.25rem;
}
.card-setting {
  border: none;
  border-radius: 0.25rem;
}
.modal-content {
  border: none;
  animation-name: show-config-content;
  animation-duration: 0.5s;
}
.submit:active {
  background-color: steelblue;
  border: none;
  outline: none;
}
.submit {
  user-select: none;
  cursor: pointer;
  margin-top: 1em;
  padding: 0.5em;
  border-radius: 5px;
  color: #fff;
  background-color: #5682a3;
  text-align: center;
  border: none;
}
</style>
