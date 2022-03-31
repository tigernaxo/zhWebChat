<template>
  <div class="px-5">
    <v-form ref="form" lazy-validation class="mx-5 my-5">
      <v-row no-gutters>
        <v-text-field class="mr-2" v-model="user.loginId" label="Id"></v-text-field>
        <v-text-field class="mr-2" v-model="user.userName" label="名稱"></v-text-field>
        <v-btn :disabled="throttle" color="primary" class="mt-3 mr-2" @click="submit">
          搜尋
        </v-btn>
      </v-row>
    </v-form>
    <v-data-table
      v-model="selected"
      :headers="headers"
      :items="items"
      :single-select="false"
      item-key="id"
      show-select
      class="elevation-1 pa-3"
    >
      <template v-slot:top>
        <div class="my-2">
          <v-btn
            :disabled="throttle || selected.length === 0"
            color="primary"
            class="mr-2"
            @click="setStatus(0)"
          >
            停用
          </v-btn>
          <v-btn
            :disabled="throttle || selected.length === 0"
            color="primary"
            class="mr-2"
            @click="setStatus(1)"
          >
            可用
          </v-btn>
          <v-btn
            :disabled="throttle || selected.length === 0"
            color="primary"
            class="mr-2"
            @click="setStatus(2)"
          >
            黑名單
          </v-btn>
        </div>
        <v-divider></v-divider>
      </template>

      <template v-slot:item.status="{ item }">
        {{ status[item.status] }}
      </template>
      <template v-slot:item.photo="{ item }">
        <a :href="`/user/${item.id}/head/${item.photo}`" target="_blank">
          {{ item.photo }}
        </a>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      status: {
        0: '可用',
        1: '停用',
        2: '封鎖'
      },
      throttle: false,
      user: { userName: '', loginId: '' },
      selected: [],
      headers: [
        { text: '登入Id', value: 'loginId' },
        { text: '顯示名稱', value: 'userName' },
        { text: '手機', value: 'phone' },
        { text: '狀態', value: 'status' },
        { text: '顯示圖片', value: 'photo' },
        { text: '建立時間', value: 'createTime' },
        { text: '修改時間', value: 'actTime' }
      ],
      items: []
    }
  },
  methods: {
    async submit() {
      this.throttle = true
      window.console.log('submit start')
      try {
        let url = '/api/A00010/Search'
        let fd = new FormData()
        fd.append('loginId', this.user.loginId)
        fd.append('userName', this.user.userName)
        let res = await this.axios.post(url, fd)
        window.console.log(res.data)
        this.items = res.data.users
        this.clearSelect()
      } catch (error) {
        window.console.error(error)
      }
      this.throttle = false
    },
    async setStatus(status) {
      // 沒有選中的話就不做任何動作
      if (this.selected.length === 0) return

      this.throttle = true
      try {
        window.console.log(status)
        window.console.log(this.selected)

        let ids = this.selected.map(x => x.id)
        let fd = new FormData()
        fd.append('status', status)
        fd.append('ids', ids)
        let url = '/api/A00010/SetStatus'
        let res = await this.axios.post(url, {
          status: status,
          ids: this.selected.map(x => x.id)
        })
        window.console.log(res.data)
        this.items.forEach(x => {
          x.status = ids.includes(x.id) ? status : x.status
        })
        this.clearSelect()
      } catch (error) {
        window.console.error(error)
      }
      this.throttle = false
    },
    clearSelect() {
      this.selected.splice(0)
    }
  }
}
</script>
<style lang="scss" scoped></style>
