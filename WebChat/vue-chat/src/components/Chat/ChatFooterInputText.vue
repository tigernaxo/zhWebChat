<template>
  <textarea
    ref="domRef"
    class="main_chat_footer_textarea col"
    rows="3"
    v-model="store.chatRoom_Meta_Current.value.draftMsg"
    @keypress.enter.exact="$emit('send')"
  ></textarea>
</template>

<script>
import { inject, ref } from 'vue'
export default {
  props: {},
  setup() {
    const store = inject('store')
    const domRef = ref(null)
    return { store, domRef }
  }
}
</script>

<style lang="scss" scoped>
textarea {
  border-radius: 5px;
  resize: none;
  border: 2px solid #bbbbbb;
}
textarea:focus {
  outline: none !important;
  border: 2px solid #8fb4ca;
}
</style>
