<template>
  <router-view />
</template>

<script>
export default {
  async created() {
    // 動態載入主要顏色
    // this.$vuetify.theme.themes.light.primary = this.$store.state.vuetify.theme.themes.light.primary
    // 進入登入頁面
    this.$router.push({ name: 'Login' })
  }
}
</script>

<style lang="scss"></style>
