<template>
  <ModalContainer ref="configTmplate" @close="$emit('close')">
    <template #content>
      <img ref="imgRef" :src="props.dataURL" :style="imgStyle" />
    </template>
  </ModalContainer>
</template>

<script>
import { reactive, ref } from 'vue'
import ModalContainer from './ModalContainer.vue'
export default {
  components: {
    ModalContainer
  },
  props: {
    dataURL: String
  },
  setup(props) {
    let configTmplate = ref(null)
    let imgRef = ref(null)
    let shrinkRatio = 0.7
    let imgStyle = reactive({
      position: 'fixed',
      top: '0px',
      left: '0px'
    })

    function open() {
      // 計算佔去視窗的比例
      let w = window.innerWidth
      let h = window.innerHeight
      let size = Math.ceil(w > h ? h * shrinkRatio : w * shrinkRatio)
      imgRef.value.width = imgRef.value.height = size
      imgStyle.left = Math.ceil((w - size) / 2) + 'px'
      imgStyle.top = Math.ceil((h - size) / 2) + 'px'

      configTmplate.value.open()
    }

    return { props, open, configTmplate, imgRef, imgStyle }
  }
}
</script>

<style lang="scss" scoped></style>
