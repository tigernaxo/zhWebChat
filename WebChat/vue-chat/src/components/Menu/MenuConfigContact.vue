<template>
  <ModalContainer ref="configTmplage" @close="$emit('close')">
    <template #content>
      <div
        class="modal-dialog modal-dialog-scrollable d-flex align-items-center justify-content-center"
      >
        <div class="modal-content">
          <ContactAdd ref="contactAddRef" />
          <ContactInfo ref="contactInfoRef" />
          <div class="modal-header d-block">
            <div class="row g-0 px-3 mx-2">
              <button type="button" class="btn btn-light" @click.stop="userFunc.search()">
                <i class="far fa-user-plus text-secondary me-2"></i>
                邀請聯絡人
              </button>
              <div
                class="alert alert-warning mb-0 mt-2"
                role="alert"
                v-if="alert.showing"
              >
                {{ alert.text }}
              </div>
            </div>
          </div>
          <div class="modal-body zh-config-scrollbar">
            <ul class="list-group list-group-flush mx-2">
              <div
                v-for="user in contacts"
                :key="user.id"
                class="list-group-item list-group-item-action py-3 px-3 menu-list-li text-center"
                :class="{ blocked: isBlock(user.id) }"
              >
                <i v-if="isBlock(user.id)" class="fal fa-user-slash text-danger"></i>
                <i v-else class="fal fa-user-alt text-success"></i>
                <div class="d-inline-block">
                  <span class="px-3"> {{ user.userName }}({{ user.loginId }}) </span>
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.survey(user.id)"
                  >
                    <i class="far fa-search"></i>
                  </button>
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.startChat(user.id)"
                  >
                    <i class="far fa-comment-dots"></i>
                  </button>
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.delUser(user.id)"
                  >
                    <i class="fal fa-user-minus"></i>
                  </button>
                  <button
                    v-if="!isBlock(user.id)"
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.banUser(user.id)"
                    :disabled="actionList.banUser.includes(user.id)"
                  >
                    <i
                      v-if="!actionList.banUser.includes(user.id)"
                      class="fal fa-user-lock"
                    ></i>
                    <i v-else class="fas fa-spinner fa-pulse"></i>
                  </button>
                  <button
                    v-else
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.unBanUser(user.id)"
                    :disabled="actionList.unBanUser.includes(user.id)"
                  >
                    <i
                      v-if="!actionList.unBanUser.includes(user.id)"
                      class="fal fa-user-unlock"
                    ></i>
                    <i v-else class="fas fa-spinner fa-pulse"></i>
                  </button>
                </div>
              </div>
            </ul>
          </div>
        </div>
      </div>
    </template>
  </ModalContainer>
</template>

<script>
import { computed, inject, reactive, ref } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import logger from '../../zh/logger'
import ContactAdd from './MenuConfigContactAdd'
import ContactInfo from './MenuConfigContactInfo'
// import axios from 'axios'
import axios from '@/zh/axios'
export default {
  components: {
    ModalContainer,
    ContactAdd,
    ContactInfo
  },
  setup(props, { emit }) {
    let data = reactive({ searchLoginId: null })
    let configTmplage = ref(null)
    function open() {
      configTmplage.value.open()
    }
    const store = inject('store')
    const contacts = computed(() =>
      Object.values(store.users).filter(x => {
        if (x.userType === 2) return false // 如果是匿名使用者就不會是朋友
        let key = `${store.system.userId}@${x.id}@1`
        let isFriend = store.userRelationShips[key] !== undefined
        return isFriend
      })
    )
    let actionList = reactive({
      unBanUser: [],
      banUser: []
    })
    function isBlock(id) {
      let key = `${store.system.userId}@${id}@2`
      return store.userRelationShips[key] !== undefined
    }
    let contactInfoRef = ref(null)
    let contactAddRef = ref(null)
    let axiosConfig = {
      headers: { Authorization: `Bearer ${store.system.token}` }
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
    async function callAPI(url, payload, funcName) {
      let id = payload.id
      // 設定為正在動作
      if (!actionList[funcName].includes(id)) !actionList[funcName].push(id)
      try {
        let res = await axios.post(url, payload, axiosConfig)
        if (res.data.resultCode === '01') throw res.data.error
      } catch (error) {
        logger.error(error.toString())
        addAlert(error.toString())
      }
      // 解除正在動作
      let idx = actionList[funcName].indexOf(id)
      logger.log(`idx:${idx}`)
      if (idx > -1) actionList[funcName].splice(idx, 1)
    }
    const wsService = inject('wsService')
    let userFunc = {
      // 查詢使用者 ok
      search: function() {
        logger.log('search')
        contactAddRef.value.open()
      },
      // 查看使用者 ok
      survey: function(id) {
        logger.log('survey')
        contactInfoRef.value.data.userId = id
        contactInfoRef.value.open()
      },
      // 開始聊天
      startChat: function(id) {
        logger.log('startChat')
        try {
          wsService.chatRoomCreateOneToOne(id)
          configTmplage.value.isShow = false // 關閉 modal
          store.system.showConfig = false // 直接關閉外層的 config 頁面
          emit('close')
        } catch (error) {
          logger.error(error)
          addAlert(error)
        }
      },
      // 封鎖使用者 ok
      banUser: async function(id) {
        logger.log('banUSer')
        callAPI('/api/user/BanUser', { id }, 'banUser')
      },
      // 解封使用者 ok
      unBanUser: async function(id) {
        logger.log('unBanUSer')
        await callAPI('/api/user/UnBanUser', { id }, 'unBanUser')
      },
      // 刪除使用者
      delUser: async function(id) {
        logger.log('delUSer')
        try {
          let url = '/api/user/DelContact'
          let config = {
            headers: { Authorization: `Bearer ${store.system.token}` }
          }
          let res = await axios.post(url, { id }, config)
          if (res.data.resultCode === '01') throw res.data.error
          let key = `${store.system.userId}@${id}@1`
          delete store.userRelationShips[key]
        } catch (error) {
          logger.error(error.toString())
        }
      }
    }

    return {
      open,
      configTmplage,
      store,
      contacts,
      isBlock,
      data,
      alert,
      userFunc,
      actionList,
      contactInfoRef,
      contactAddRef
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
  height: 100%;
}
.blocked {
  background-color: #eee;
}
.blocked:hover {
  background-color: #ddd;
}

.blocked span {
  color: rgb(255, 70, 70);
}
</style>
