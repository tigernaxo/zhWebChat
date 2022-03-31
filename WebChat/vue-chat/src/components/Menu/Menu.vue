<template>
  <div class="modal fade show" :style="modelStyleObj" ref="modelRef">
    <div class="modal-content-menu" ref="modelContentRef">
      <MenuInfo />
      <MenuList :items="items" @select="selectItem($event)" />
      <MenuConfigContact ref="menuConfigContactRef" @close="closeConfig()" />
      <MenuConfigGroup ref="menuConfigGroupRef" @close="closeConfig()" />
      <ModalQRcode ref="menuConfigAnonumousInvite" @close="closeConfig()" />
      <MenuConfigSetting ref="menuConfigSettingRef" @close="closeConfig()" />
      <MenuConfigBanList ref="menuConfigBanListRef" @close="closeConfig()" />
    </div>
  </div>
</template>

<script>
import { computed, inject, reactive, ref } from 'vue'
import logger from '@/zh/logger'
import MenuList from './MenuList.vue'
import MenuInfo from './MenuInfo.vue'
import MenuConfigContact from './MenuConfigContact.vue'
import MenuConfigBanList from './MenuConfigBanList.vue'
import MenuConfigGroup from './MenuConfigGroup.vue'
import MenuConfigSetting from './MenuConfigSetting.vue'
import ModalQRcode from './MenuConfigAnonymousInvite.vue'
export default {
  components: {
    ModalQRcode,
    MenuList,
    MenuInfo,
    MenuConfigContact,
    MenuConfigBanList,
    MenuConfigGroup,
    MenuConfigSetting
  },
  setup() {
    let menuConfigGroupRef = ref(null)
    let menuConfigContactRef = ref(null)
    let menuConfigSettingRef = ref(null)
    let menuConfigBanListRef = ref(null)
    let menuConfigAnonumousInvite = ref(null)
    let items = reactive([
      {
        id: 0,
        text: '建立群組',
        selected: false,
        class: ['fas', 'fa-users'],
        ref: menuConfigGroupRef
      },
      {
        id: 1,
        text: '聯絡人',
        selected: false,
        class: ['fas', 'fa-user'],
        ref: menuConfigContactRef
      },
      {
        id: 2,
        text: '封鎖名單',
        selected: false,
        class: ['fas', 'fa-users-slash'],
        ref: menuConfigBanListRef
      },
      {
        id: 3,
        text: '與匿名聊天',
        selected: false,
        class: ['fas', 'fa-qrcode'],
        ref: menuConfigAnonumousInvite
      },
      {
        id: 4,
        text: '設置',
        selected: false,
        class: ['fas', 'fa-cog'],
        ref: menuConfigSettingRef
      }
    ])
    function selectItem(id) {
      items.forEach(x => (x.selected = x.id === id ? true : false))
      items.find(x => x.id === id).ref.open()
    }
    function closeConfig() {
      items.forEach(x => (x.selected = false))
    }
    let modelRef = ref(null)
    let modelContentRef = ref(null)
    let store = inject('store')
    const open = function() {
      store.system.showConfig = true
    }

    let modelStyleObj = computed(() => {
      return {
        display: store.system.showConfig ? 'block' : 'none'
      }
    })
    document.addEventListener('click', function(e) {
      // 如果 model 正在顯示，判斷是否需要關閉
      if (store.system.showConfig) {
        let el = modelContentRef.value
        // // 判斷是否點選到外面的元素
        let isClickOutSide = !el.contains(e.target) && el !== e.target
        // ->判斷是否點選到 setting 頁面衍生出來的頁面
        // ->直接把衍生的頁面塞到 modelContentRef 裡面
        // 如果點選到外面的元素，就關掉 model
        if (isClickOutSide) {
          logger.log('closing Menu...')
          store.system.showConfig = false
        }
      }
    })
    return {
      modelRef,
      modelContentRef,
      modelStyleObj,
      open,
      items,
      selectItem,
      closeConfig,
      menuConfigContactRef,
      menuConfigGroupRef,
      menuConfigSettingRef,
      menuConfigBanListRef,
      menuConfigAnonumousInvite
    }
  }
}
</script>

<style lang="scss" scoped>
// Model 可參考 https://www.w3schools.com/howto/tryit.asp?filename=tryhow_css_modal
/* Modal Content */
.modal-content-menu {
  font-weight: bold;
  background-color: #fefefe;
  position: absolute;
  top: 0;
  left: 0;
  width: 300px;
  height: 100%;
  border: 1px solid #888;
  animation-name: show-menu-list;
  animation-duration: 0.2s;
}
@keyframes show-menu-list {
  from {
    left: -300px;
  }
  to {
    left: 0;
  }
}
</style>
