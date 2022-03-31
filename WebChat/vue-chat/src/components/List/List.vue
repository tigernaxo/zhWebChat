<template>
  <div class="main_list">
    <ListHeader />
    <ListBody />
    <ListFooter />
  </div>
</template>

<script>
import ListBody from './ListBody.vue'
import ListHeader from './ListHeader.vue'
import ListFooter from './ListFooter.vue'
export default {
  components: {
    ListHeader,
    ListBody,
    ListFooter
  }
}
</script>

<style lang="scss" scoped>
.main_list * {
  display: scroll;
}
.main_list {
  flex: 0 0 20%;
  max-height: 100vh;
  min-height: 0px;

  display: flex;
  flex-direction: column;
  align-items: stretch;

  border-right: 1px solid #e9ebed;
  min-width: 10%;
  background-color: #ffffff;
}
</style>
