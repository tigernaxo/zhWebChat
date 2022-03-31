<template>
  <div class="card py-2 menu-info-card mb-2">
    <div class="row g-0">
      <div class="card-body">
        <div class="card-title ms-2 mt-1 menu-info-head" :style="menuInfoHeadStyle"></div>
        <div class="card-text pt-3">
          在學園都市擔任電磁砲
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, inject } from 'vue'
export default {
  setup() {
    const store = inject('store')
    let menuInfoHeadStyle = computed(() => {
      let userId = store.system.userId
      let userType = store.system.userType
      let fileName = store.users[`${userId}@${userType}`]?.photo
      let path = `/user/${userId}/head/${fileName}`
      let imgPath = fileName ? `url('${path}')` : `url('')`
      return {
        'background-image': imgPath
      }
    })
    return { store, menuInfoHeadStyle }
  }
}
</script>

<style lang="scss" scoped>
.menu-info-card {
  height: 200px;
  background-image: url('https://ok.166.net/16163/2018-11-01/mm18/1541040886502_jy1pxv.jpg');
  background-repeat: no-repeat;
  background-size: cover;
  border-radius: 0;
  color: white;
}
.menu-info-head {
  height: 80px;
  width: 80px;
  border-radius: 50%;
  // background-image: url('https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSFkI2ibEgcyv9fFARVdVCxqH_yBU64f6JSAg&usqp=CAU');
  // background-image: url('/user/3/head/20210111151259956-cbd3f7cd-6873-4899-b0af-1dbcb50165c2.png');
  background-repeat: no-repeat;
  background-size: cover;
}
</style>
