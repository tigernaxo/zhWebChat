<template>
  <ModalContainer ref="configTmplage" @close="$emit('close')">
    <template #content>
      <div class="modal-dialog modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header d-block">
            <div class="row g-0 mx-2 align-items-end">
              <div class="col">
                <label>群組名稱</label>
                <input
                  class="form-check-input ms-3"
                  type="checkbox"
                  v-model="data.isPrivate"
                />
                <label> 私人群組 </label>
                <input class="form-control col" v-model="data.title" />
              </div>
              <div class="col-auto ps-3">
                <button type="button" class="btn btn-light" @click="addGroup()">
                  <i :class="btnIconClass"></i>
                </button>
              </div>
            </div>
            <div class="mx-2 mt-1">
              <label>聊天室聲明</label>
              <textarea class="form-control" v-model="data.announce"></textarea>
            </div>
          </div>
          <div class="modal-body zh-config-scrollbar">
            <ul class="list-group list-group-flush mx-2">
              <li
                v-for="user in contacts"
                :key="user.id"
                class="list-group-item list-group-item-action py-3 ps-4 menu-list-li contact-list text-center"
                @click="toogleUser(user.id)"
              >
                <div class="d-inline-block">
                  <template v-if="data.users.some(id => id === user.id)">
                    <i class="fas fa-user-check text-success"></i>
                  </template>
                  <template v-else>
                    <i class="fas fa-user text-secondary"></i>
                  </template>
                  <span class="px-3"> {{ user.userName }}({{ user.loginId }}) </span>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </template>
  </ModalContainer>
</template>

<script>
import { computed, inject, onMounted, reactive, ref } from 'vue'
import ModalContainer from '../ModalContainer.vue'
import logger from '@/zh/logger'
import ws from '@/zh/ws'
export default {
  components: {
    ModalContainer
  },
  setup(props, { emit }) {
    let loading = ref(false)
    let btnIconClass = computed(() => {
      return {
        fas: true,
        'fa-user-plus': !loading.value,
        'fa-spinner': loading.value,
        'fa-pulse': loading.value,
        'text-secondary': true
      }
    })

    let data = reactive({})
    onMounted(() => {
      resetData()
    })
    function resetData() {
      Object.assign(data, {
        title: null,
        announce: null,
        users: [],
        isPrivate: false
      })
    }
    function open() {
      resetData()
      configTmplage.value.open()
    }

    const wsService = inject('wsService')
    function addGroup() {
      loading.value = true // 把新增群按鈕 disable
      wsService.addGroup(data.title, data.announce, data.isPrivate, data.users)
      logger.log('addGroup data:')
      logger.log(data)
    }

    function toogleUser(id) {
      let idx = data.users.findIndex(x => x === id)
      idx !== -1 ? data.users.splice(idx, 1) : data.users.push(id)
    }
    ws.on('ChatRoomCreateGroupResponse', function(chatRoom, chatRoomUsers) {
      logger.log('ChatRoomCreateGroupResponse:')
      logger.log(chatRoom)
      logger.log(chatRoomUsers)
      wsService.addChatRoom(chatRoom, chatRoomUsers)
      resetData()
      // 接收 chatRoom、chatRoomUsers
      loading.value = false // 把新增群組按鈕重新激活
      configTmplage.value.isShow = false // 關閉新增群組頁面
      store.system.showConfig = false // 直接關閉外層的 config 頁面
      store.system.roomId = chatRoom.id
      emit('close')
    })
    let configTmplage = ref(null)

    const store = inject('store')
    const contacts = computed(() =>
      Object.values(store.users).filter(x => x.id !== store.system.userId)
    )
    return {
      open,
      configTmplage,
      contacts,
      logger,
      data,
      addGroup,
      toogleUser,
      btnIconClass
    }
  }
}
</script>

<style lang="scss" scoped>
.contact-list {
  cursor: pointer;
}
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
.form-control {
  resize: none;
}
</style>
