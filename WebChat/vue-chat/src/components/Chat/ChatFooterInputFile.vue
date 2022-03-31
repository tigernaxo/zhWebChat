<template>
  <div v-if="file !== null" class="col d-flex justify-content-center align-items-center">
    <span class="badge bg-secondary">
      <span class="fs-5 my-auto align-middle">
        {{ fileName }}
      </span>
      <button type="button" class="btn btn-secondary py-1 px-2" @click="$emit('cancel')">
        <i class="far fa-times"></i>
      </button>
    </span>
  </div>
</template>

<script>
export default {
  props: {
    fileName: String
  }
}
</script>

<style lang="scss" scoped>
div {
  user-select: none;
  border-radius: 5px;
  background-color: #ddd;
}
</style>
