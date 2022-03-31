<template>
  <v-row>
    <v-list-item class="pl-6">
      <v-list-item-avatar class="zh-avatar">
        <v-img :src="img"></v-img>
      </v-list-item-avatar>
      <v-list-item-content class="pl-1 zh-name">
        <v-list-item-title>{{ text }}</v-list-item-title>
      </v-list-item-content>
    </v-list-item>
  </v-row>
</template>

<script>
export default {
  props: {
    img: String,
    text: String
  }
}
</script>

<style lang="scss" scoped>
.zh-avatar {
  margin-top: 1em;
  margin-bottom: 1em;
  height: 4em !important;
  width: 4em !important;
}
.zh-name .v-list-item__title {
  font-size: 1.3em;
  line-height: 1.3em;
  user-select: none;
}
.v-list-item {
  padding-left: 30px !important;
}
</style>
