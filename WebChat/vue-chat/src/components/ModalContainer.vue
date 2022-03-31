<template>
  <div class="modal py-5" :style="styleObj" ref="thisRef">
    <slot name="content"></slot>
  </div>
</template>

<script>
import { computed, ref } from 'vue'
import logger from '@/zh/logger'
export default {
  setup(props, { emit }) {
    let isShow = ref(false) // 此元件是否開啟
    let thisRef = ref(null)
    let styleObj = computed(() => {
      return {
        display: isShow.value ? 'block' : 'none'
      }
    })
    const open = function() {
      isShow.value = true
    }
    document.addEventListener('click', function(e) {
      // 如果 model 正在顯示，判斷是否需要關閉
      if (isShow.value) {
        // 好像無法直接取得 slot 內的 dom ref，用 slots 會取得 vdom，因此用上層dom間接取得
        let el = thisRef.value.querySelectorAll('*')[0]
        // // 判斷是否點選到外面的元素
        let isClickOutSide = !el.contains(e.target) && el !== e.target
        // 如果點選到外面的元素，就關掉 model
        if (isClickOutSide) {
          logger.log('closing config page...')
          isShow.value = false
          emit('close')
        }
      }
    })
    return { open, styleObj, thisRef, isShow }
  }
}
</script>

<style lang="scss" scoped>
.menu-config-contact {
  display: block;
  margin: auto;
}
</style>
