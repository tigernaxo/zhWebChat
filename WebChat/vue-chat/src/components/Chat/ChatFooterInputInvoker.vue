<template>
  <div class="invoker">
    <template v-for="(invoker, idx) in iconInvokers" :key="idx">
      <svg height="24" width="24" viewBox="0 0 24 24" @click="invoker.onclick($event)">
        <path d="M0 0h24v24H0z" fill="none" />
        <path :d="invoker.path" />
      </svg>
    </template>
  </div>
</template>

<script>
import { inject } from 'vue'
export default {
  props: {},
  setup(props, { emit }) {
    const store = inject('store')
    let iconInvokers = [
      {
        name: 'emoji',
        path:
          'M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm3.5-9c.83 0 1.5-.67 1.5-1.5S16.33 8 15.5 8 14 8.67 14 9.5s.67 1.5 1.5 1.5zm-7 0c.83 0 1.5-.67 1.5-1.5S9.33 8 8.5 8 7 8.67 7 9.5 7.67 11 8.5 11zm3.5 6.5c2.33 0 4.31-1.46 5.11-3.5H6.89c.8 2.04 2.78 3.5 5.11 3.5z',
        onclick: e => emit('emoji', e)
      }
    ]
    // 如果不是匿名使用者才有檔案上傳
    if (store.system.userType === 1) {
      iconInvokers.push({
        name: 'file',
        path:
          'M16.5 6v11.5c0 2.21-1.79 4-4 4s-4-1.79-4-4V5c0-1.38 1.12-2.5 2.5-2.5s2.5 1.12 2.5 2.5v10.5c0 .55-.45 1-1 1s-1-.45-1-1V6H10v9.5c0 1.38 1.12 2.5 2.5 2.5s2.5-1.12 2.5-2.5V5c0-2.21-1.79-4-4-4S7 2.79 7 5v12.5c0 3.04 2.46 5.5 5.5 5.5s5.5-2.46 5.5-5.5V6h-1.5z',
        onclick: e => emit('file', e)
      })
    }
    return { store, iconInvokers }
  }
}
</script>

<style lang="scss" scoped>
.invoker {
  width: auto; // 移除 bootstrap 的 row>* 屬性干擾
  // 靠左下
  position: absolute;
  bottom: 0px;
  // 往右上抬
  padding: 0 0 10px 5px;
  // 顯示於最上層
  z-index: 1;
}
.invoker:hover,
.invoker > svg {
  cursor: pointer;
  fill: #b1c6d0;
}
</style>
