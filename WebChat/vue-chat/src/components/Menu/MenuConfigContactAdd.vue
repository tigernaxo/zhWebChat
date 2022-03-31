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
                @click.stop="search()"
                :disabled="data.loading"
              >
                <i v-if="!data.loading" class="far fa-search text-secondary"></i>
                <i v-else class="fas fa-spinner fa-pulse"></i>
              </button>
            </div>
          </div>
          <div class="modal-body zh-config-scrollbar">
            <div class="py-3 text-center">
              <div class="card" v-if="data.user !== null">
                <div class="card-body">
                  {{ data.user?.loginId }}({{ data.user?.userName }})
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="add(data.user?.id)"
                    :disabled="isUserFriend"
                  >
                    <template v-if="isUserFriend">
                      已經是聯絡人
                    </template>
                    <template v-else>
                      <i v-if="!data.adding" class="fal fa-user-plus"></i>
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
import { inject, ref, reactive, computed } from 'vue'
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
      user: null,
      loading: false,
      adding: false
    })
    let isUserFriend = computed(() => {
      let key = `${store.system.userId}@${data.user?.id}@1`
      return store.userRelationShips[key] !== undefined
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
      data.user = null
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
        let url = '/api/user/SearchByLoginId'
        let config = {
          headers: { Authorization: `Bearer ${store.system.token}` }
        }
        let res = await axios.post(url, { loginId: data.searchLiginId }, config)
        data.user = res.data.user
        // 找不到資料
        if (data.user === null) throw '找不到使用者'
        if (res.data.resultCode === '01') throw res.data.error
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
      data.loading = false
    }
    async function add(id) {
      data.adding = true
      try {
        logger.log(`add user...userId:${id}`)
        if (id === store.system.userId) throw '無法將自己添加為聯絡人'
        let url = '/api/user/AddContact'
        let config = {
          headers: { Authorization: `Bearer ${store.system.token}` }
        }
        let res = await axios.post(url, { id: data.user?.id }, config)
        if (res.data.resultCode === '01') throw res.data.error
        // 取得聯絡人資訊
        logger.log('res.data:')
        logger.log(res.data)
        let user = res.data.user
        let userRelationShip = res.data.userRelationShip
        // 添加聯絡人只跟 user 有關，不需要考慮把 Anonymous 加入聯絡人的情況
        store.users[`${user.id}@1`] = user // 加入聯絡人
        store.userRelationShips[`${store.system.userId}@${id}@1`] = userRelationShip // 加入聯絡資訊
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
      data.adding = false
    }
    return { open, configTmplage, store, data, search, add, alert, isUserFriend }
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
