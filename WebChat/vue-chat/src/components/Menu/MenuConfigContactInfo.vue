<template>
  <ModalContainer ref="configTmplage" @close="$emit('close')">
    <template #content>
      <div
        class="modal-dialog modal-dialog-scrollable d-flex align-items-center justify-content-center"
      >
        <div class="card menu-info-card">
          <div class="row g-0">
            <div class="card-body">
              <div
                class="card-title ms-2 mt-1 menu-info-head"
                :style="menuInfoHeadStyle"
              ></div>
              <div class="card-text pt-3 menu-info-text">
                {{ user?.userName }}({{ user?.loginId }})
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </ModalContainer>
</template>

<script>
import ModalContainer from '../ModalContainer.vue'
import { inject, ref, computed, reactive } from 'vue'
import config from '../../../vue.configExt'
export default {
  components: {
    ModalContainer
  },
  setup() {
    let data = reactive({ userId: null })
    let configTmplage = ref(null)
    function open() {
      configTmplage.value.open()
    }
    const store = inject('store')
    // 聯絡人資訊只會是 user 不會是  Anonymous
    let user = computed(() => store.users[`${data.userId}@1`])
    let altImg = '/user/default/head/default.jpg'
    let menuInfoHeadStyle = computed(() => {
      let fileName = store.users[`${data.userId}@1`]?.photo
      let path = `${config.appPath}/user/${data.userId}/head/${fileName}`
      let imgPath = fileName ? `${path}` : ''
      return {
        'background-image': `url('${imgPath}') ,url('${altImg}')`
      }
    })

    return { open, configTmplage, store, menuInfoHeadStyle, data, user }
  }
}
</script>

<style lang="scss" scoped>
.modal-dialog {
  border: none;
  animation-name: show-config-content;
  animation-duration: 0.5s;
  width: 90%;
}
.menu-info-card {
  min-width: 75%;
  max-width: 90%;
  height: 200px;
  background-image: url('https://ok.166.net/16163/2018-11-01/mm18/1541040886502_jy1pxv.jpg');
  background-repeat: no-repeat;
  background-size: cover;
  border: #ddd 2px solid;
  border-radius: 10px;
  color: #fff;
}
.menu-info-head {
  display: block;
  height: 80px;
  width: 80px;
  border-radius: 50%;
  // background-image: url('https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSFkI2ibEgcyv9fFARVdVCxqH_yBU64f6JSAg&usqp=CAU');
  // background-image: url('../user/3/head/20210111151259956-cbd3f7cd-6873-4899-b0af-1dbcb50165c2.png');
  background-repeat: no-repeat;
  background-size: cover;
}
.menu-info-head img {
  width: 800px;
}
</style>
