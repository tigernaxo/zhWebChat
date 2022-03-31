<template>
  <v-list-item @click="$emit('click')" :link="isLink" :to="to">
    <v-list-item-action>
      <v-icon>{{ icon }}</v-icon>
    </v-list-item-action>
    <v-list-item-content>
      <v-list-item-title>{{ text }}</v-list-item-title>
    </v-list-item-content>
  </v-list-item>
</template>

<script>
export default {
  props: {
    icon: String,
    text: String,
    isLink: { type: Boolean, default: false },
    to: { type: String, default: '' }
  }
}
</script>
<style lang="scss" scoped>
.v-list-item__title {
  font-size: 1em !important;
}
.v-list-item__action i {
  font-size: 1.8em;
}
.v-list-item__action {
  margin-right: 1em !important;
}
</style>
