<template>
  <ModalContainer ref="configTmplateRef" @close="$emit('close')">
    <template #content>
      <div class="modal-dialog modal-dialog-scrollable">
        <div class="modal-content">
          <BanListAdd ref="banListAddRef" />
          <div class="modal-header d-block">
            <div class="row g-0 px-3 mx-2">
              <button type="button" class="btn btn-light" @click.stop="userFunc.search()">
                <i class="fal fa-user-minus text-secondary me-2"></i>
                封鎖聯絡人
              </button>
            </div>
          </div>
          <div class="modal-body zh-config-scrollbar">
            <ul class="list-group list-group-flush mx-2">
              <div
                v-for="user in contacts"
                :key="user.id"
                class="list-group-item list-group-item-action py-3 px-3 menu-list-li text-center"
              >
                <div class="d-inline-block">
                  <span class="px-3"> {{ user.userName }}({{ user.loginId }}) </span>
                  <button
                    type="button"
                    class="btn btn-outline-secondary ms-1"
                    @click.stop="userFunc.unBanUser(user.id)"
                  >
                    <i class="fal fa-user-unlock"></i>
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
import { computed, inject, ref } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import BanListAdd from './MenuConfigBanListAdd'
import logger from '../../zh/logger'
// import axios from 'axios'
import axios from '@/zh/axios'
export default {
  components: {
    ModalContainer,
    BanListAdd
  },
  setup() {
    const store = inject('store')
    // inner add modal
    let banListAddRef = ref(null)
    // this modal
    let configTmplateRef = ref(null)

    function open() {
      configTmplateRef.value.open()
    }
    const contacts = computed(() =>
      Object.values(store.users).filter(x => {
        if (x.userType === 2) return false // 如果是匿名使用者就不會是朋友
        let keyPrefix = `${store.system.userId}@${x.id}@`
        let isBaned = store.userRelationShips[`${keyPrefix}2`] !== undefined
        let isFriend = store.userRelationShips[`${keyPrefix}1`] !== undefined
        return isBaned && !isFriend
      })
    )
    let userFunc = {
      // 封鎖使用者
      search: function() {
        logger.log('search')
        banListAddRef.value.open()
      },
      unBanUser: async function(id) {
        logger.log('unBanUSer')
        let url = '/api/user/UnBanUser'
        let payload = { id }
        let axiosConfig = {
          headers: { Authorization: `Bearer ${store.system.token}` }
        }
        try {
          let res = await axios.post(url, payload, axiosConfig)
          if (res.data.resultCode === '01') throw res.data.error
        } catch (error) {
          logger.error(error.toString())
        }
      }
    }

    return {
      open,
      configTmplateRef,
      banListAddRef,
      store,
      contacts,
      userFunc
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
</style>
