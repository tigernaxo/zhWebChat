<template>
  <div class="main-list-body-wrapper">
    <div class="main-list-body">
      <ListBodyLi
        v-for="room in store.chatRooms"
        :key="room.id"
        @click="chooseRoom(room.id)"
        :room="room"
      />
    </div>
  </div>
</template>

<script>
import { inject } from 'vue'
import ListBodyLi from './ListBodyLi.vue'
export default {
  components: {
    ListBodyLi
  },
  setup() {
    let store = inject('store')
    let chooseRoom = function(roomId) {
      store.system.roomId = roomId
    }

    // 當實際上的 lastMsg 改變時，要重新開始計算出新的顯示訊息
    return { store, chooseRoom }
  }
}
</script>

<style lang="scss" scoped>
.main-list-body-wrapper {
  flex: 1 1 auto;
  min-height: 0px;
  overflow-y: auto;
  display: flex;
  box-shadow: 0 1px 1px rgba(97, 127, 152, 0.2), 1px 0 0 rgba(97, 127, 152, 0.1),
    -1px 0 0 rgba(97, 127, 152, 0.1);
}
.main-list-body {
  max-width: 100%;
  flex: 1 1 auto;
  margin: 0.1em;
  padding: 3px;
  min-height: 0px;
  background-color: #fff;
  display: flex;
  flex-direction: column;
}
.main-list-body-wrapper::-webkit-scrollbar {
  display: none;
}
</style>
