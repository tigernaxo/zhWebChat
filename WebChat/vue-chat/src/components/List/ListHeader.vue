<template>
  <div class="main_list_header">
    <button class="main-list-header-btn" @click.stop="store.system.showConfig = true">
      <i class="fas fa-bars main-list-header-btn_icon"></i>
    </button>
    <div class="main-list-header-title-wrapper">
      <div class="align-middle main-list-header-title">
        <div>
          WebChat
          <i
            v-if="store.system.wsState !== 'Connected'"
            class="far fa-wifi-slash text-danger"
            >已離線</i
          >
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { inject } from 'vue'
export default {
  setup() {
    let store = inject('store')
    return { store }
  }
}
</script>

<style lang="scss" scoped>
.main_list_header {
  flex: 0 0 auto;
  word-wrap: none;
  background-color: #5682a3;
  font-size: 1.5em;
  color: #fff;
  height: 50px;
}
.main-list-header-btn,
.main-list-header-btn:active {
  display: inline-block;
  outline: none;
  height: 100%;
  cursor: pointer;
  background-color: #5682a3;
  color: #5682a3;
  border: none;
  margin: 0px 5px;
}
.main-list-header-btn_icon,
.main-list-header-btn_icon:focus {
  font-size: auto;
  border: none;
  color: #fff;
}
.main-list-header-title {
  color: #fff;
  font-size: 0.9em;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
}
.main-list-header-title-wrapper {
  display: inline-block;
  height: 100%;
}
</style>
