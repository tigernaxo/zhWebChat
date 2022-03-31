<template>
  <div>
    <template v-for="item in items">
      <!-- v-list-group 模板：套用有 children 屬性的資料 -->
      <v-list-group
        v-if="item.children"
        :key="item.text"
        v-model="item.model"
        :prepend-icon="item.model ? 'mdi-chevron-up' : 'mdi-chevron-down'"
        append-icon
      >
        <template v-slot:activator>
          <v-list-item-content>
            <v-list-item-title>{{ item.text }}</v-list-item-title>
          </v-list-item-content>
        </template>
        <v-list-item v-for="(child, i) in item.children" :key="i" link :to="child.link">
          <v-list-item-action v-if="child.icon">
            <v-icon>{{ child.icon }}</v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title>{{ child.text }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list-group>
      <!-- v-list-item 模板：套用未被上述條件捕獲資料 -->
      <v-list-item v-else :key="item.text" link :to="item.link">
        <v-list-item-action>
          <v-icon>{{ item.icon }}</v-icon>
        </v-list-item-action>
        <v-list-item-content>
          <v-list-item-title>{{ item.text }}</v-list-item-title>
        </v-list-item-content>
      </v-list-item>
    </template>
  </div>
</template>

<script>
export default {
  props: {
    items: Array
  }
}
</script>

<style lang="scss" scoped></style>
