<template>
  <ModalContainer ref="configTmplage" @close="$emit('close')">
    <template #content>
      <div
        class="modal-dialog d-flex align-items-center justify-content-center modal-dialog-scrollable"
      >
        <div class="modal-content">
          <div class="modal-header d-block">
            <div class="row g-0 px-3 mx-2">
              <label class="col-auto col-form-label">搜尋聯絡人</label>
              <input class="form-control col mx-3" v-model="data.searchLiginId" />
              <button
                type="button"
                class="btn btn-light col-auto"
                @click.stop="userFunc.search()"
                :disabled="data.loading"
              >
                <i v-if="!data.loading" class="far fa-search text-secondary"></i>
                <i v-else class="fas fa-spinner fa-pulse"></i>
              </button>
            </div>
          </div>
          <div class="modal-body zh-config-scrollbar">
            <div class="py-3 text-center">
              <div class="card" v-if="data.userList[0]">
                <div
                  class="card-body fs-5"
                  v-for="(user, idx) in data.userList"
                  :key="idx"
                >
                  {{ user.loginId }}({{ user.userName }})
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.inviteAction(user.id)"
                    :disabled="user.isMember"
                  >
                    <template v-if="user.isMember"> 已經是成員 </template>
                    <i v-else class="far fa-user-plus"></i>
                  </button>
                </div>
              </div>
              <div
                class="alert alert-warning mb-0 mt-2"
                role="alert"
                v-if="alert.showing"
              >
                {{ alert.text }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </ModalContainer>
</template>

<script>
import { ref, inject, reactive, onMounted } from 'vue'
// import axios from 'axios'
import axios from '@/zh/axios'
import { logger } from '@/zh'
import ModalContainer from '../ModalContainer.vue'
export default {
  components: { ModalContainer },
  setup() {
    const store = inject('store')
    let data = reactive({
      searchLiginId: null,
      userList: [],
      loading: false,
      adding: false
    })
    onMounted(() => {
      data.userList = []
    })
    // alert box 相關邏輯
    let alert = reactive({ timer: null, showing: false, duration: 1000, text: '' })
    function addAlert(text) {
      alert.showing = true
      alert.text = text
      clearTimeout(alert.timer)
      alert.timer = setTimeout(function() {
        alert.showing = false
      }, 1500)
    }
    let configTmplage = ref(null)
    function open() {
      data.userList = []
      data.searchLiginId = null
      configTmplage.value.open()
    }
    async function search() {
      data.loading = true
      try {
        logger.log('searching user...')
        // 檢查搜尋 Id 是否為空白
        if (!data.searchLiginId || data.searchLiginId?.length === 0)
          throw '搜尋 id 不可為空白'
        // 向 server 要資料
        let url = '/api/user/SearchByLoginIdFuzzy'
        let config = {
          headers: { Authorization: `Bearer ${store.system.token}` }
        }
        let res = await axios.post(url, { loginId: data.searchLiginId }, config)
        data.userList = res.data.userList
        data.userList.forEach(x => {
          logger.log('debug...')
          logger.log(x)
          let chatRoomUser = store.chatRoomUsers[`${x.id}@1@${store.system.roomId}`]
          logger.log(chatRoomUser)
          x.isMember = isGroupMember(x.id)
        })
        // 找不到資料
        if (!data.userList[0]) throw '找不到使用者'
        if (res.data.resultCode === '01') throw res.data.error
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
      data.loading = false
    }

    function isGroupMember(id) {
      let chatRoomUser = store.chatRoomUsers[`${id}@1@${store.system.roomId}`]
      return chatRoomUser?.status === 1 ? true : false
    }
    async function callAPI(url, payload) {
      let axiosConfig = {
        headers: { Authorization: `Bearer ${store.system.token}` }
      }
      // 設定為正在動作
      try {
        let res = await axios.post(url, payload, axiosConfig)
        if (res.data.resultCode === '01') throw res.data.error
        return res.data
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
    }
    async function inviteAction(id) {
      logger.debug('invite')
      logger.debug(id)
      data.userList.find(x => x.id === id).isMember ? unInvite(id) : invite(id)
    }
    // 封鎖使用者 ok
    async function invite(id) {
      logger.debug('invite(id)...')
      let res = callAPI('/api/user/InviteUser', {
        roomId: store.system.roomId,
        userId: id
      })
      logger.log(res)
      data.userList.find(x => x.id === id).isMember = true
    }
    // 解封使用者 ok
    async function unInvite(id) {
      logger.debug('unInvite()')
      logger.log('unInvite')
      let res = callAPI('/api/user/UnInviteUser', { id })
      logger.log(res)
      data.userList.find(x => x.id === id).isMember = false
    }
    let userFunc = {
      search,
      inviteAction
    }
    return {
      open,
      configTmplage,
      store,
      data,
      userFunc,
      search,
      alert
    }
  }
}
</script>

<style lang="scss" scoped>
.modal-header,
.modal-footer {
  color: white;
  background-color: #5682a3;
}
.modal-content {
  border: none;
  animation-name: show-config-content;
  animation-duration: 0.5s;
  width: 90%;
}
.card-body {
  color: #212529;
}
</style>
