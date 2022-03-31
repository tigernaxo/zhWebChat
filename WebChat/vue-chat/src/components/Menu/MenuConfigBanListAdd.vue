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
                <div class="card-body" v-for="(user, idx) in data.userList" :key="idx">
                  {{ user.loginId }}({{ user.userName }})
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.banAction(user.id)"
                    :disabled="isUserFriend(user.id)"
                  >
                    <template v-if="isUserFriend(user.id)">
                      無法在此封鎖
                    </template>
                    <template v-else>
                      <i
                        v-if="!data.adding"
                        class="fal "
                        :class="{
                          'fa-user-unlock': user.isBaned,
                          'fa-user-lock': !user.isBaned
                        }"
                      ></i>
                      <i v-else class="fas fa-spinner fa-pulse"></i>
                    </template>
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
import { inject, ref, reactive } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import logger from '../../zh/logger'
// import axios from 'axios'
import axios from '@/zh/axios'
export default {
  components: {
    ModalContainer
  },
  setup() {
    const store = inject('store')
    let data = reactive({
      searchLiginId: null,
      userList: [],
      loading: false,
      adding: false
    })
    let isUserFriend = id => {
      let key = `${store.system.userId}@${id}@1`
      return store.userRelationShips[key] !== undefined
    }
    let isUserBaned = id => {
      let key = `${store.system.userId}@${id}@2`
      return store.userRelationShips[key] !== undefined
    }

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
        data.userList.forEach(x => (x.isBaned = isUserBaned(x.id)))
        // 找不到資料
        if (!data.userList[0]) throw '找不到使用者'
        if (res.data.resultCode === '01') throw res.data.error
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
      data.loading = false
    }

    async function banAction(id) {
      logger.debug('banAction')
      logger.debug(id)
      isUserBaned(id) ? unBan(id) : ban(id)
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

    // 封鎖使用者 ok
    async function ban(id) {
      logger.debug('ban()')
      let res = callAPI('/api/user/BanUser', { id })
      logger.log(res)
      let key = `${res.userId}@${res.userId2}@${res.type}`
      store.userRelationShips[key] = res
      logger.log(`id:${id}`)
      data.userList.find(x => x.id === id).isBaned = true
    }
    // 解封使用者 ok
    async function unBan(id) {
      logger.debug('unBan()')
      logger.log('unBanUSer')
      let res = callAPI('/api/user/UnBanUser', { id })
      logger.log(res)
      data.userList.find(x => x.id === id).isBaned = false
    }
    let userFunc = {
      search,
      banAction
    }
    return {
      open,
      configTmplage,
      store,
      data,
      userFunc,
      search,
      alert,
      isUserFriend,
      isUserBaned
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
</style>
