<template>
  <ul class="list-group list-group-flush mx-2">
    <li
      v-for="(item, idx) in props.items"
      :key="idx"
      @click.stop="$emit('select', item.id)"
      class="list-group-item list-group-item-action py-3 ps-4 menu-list-li"
      :class="{ disabled: item.selected }"
    >
      <i :class="item.class"></i>
      <span class="px-3"> {{ item.text }} </span>
    </li>
  </ul>
</template>

<script>
export default {
  props: {
    items: Array
  },
  setup(props) {
    return { props }
  }
}
</script>

<style lang="scss" scoped>
.menu-list-li {
  cursor: pointer;
}
</style>
