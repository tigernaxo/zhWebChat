<template>
  <div class="px-5">
    <v-form ref="form" lazy-validation class="mx-5 my-5">
      <v-row no-gutters>
        <v-select
          class="mr-2"
          :items="$store.state.types.chatMsg"
          label="Solo field"
          solo
          v-model="searchObj.type"
        >
        </v-select>
        <v-text-field
          v-show="searchObj.type === 11"
          class="mr-2"
          v-model="searchObj.text"
          label="訊息文字"
        ></v-text-field>
        <v-text-field
          v-show="searchObj.type !== 11"
          class="mr-2"
          v-model="searchObj.fileName"
          label="檔案名稱"
        ></v-text-field>
        <v-text-field
          class="mr-2"
          v-model="searchObj.title"
          label="房間名稱"
        ></v-text-field>
        <v-btn :disabled="throttle" color="primary" class="mt-3 mr-2" @click="search">
          搜尋
        </v-btn>
      </v-row>
    </v-form>
    <v-data-table
      v-model="selected"
      :headers="headerCompu"
      :items="items"
      :single-select="false"
      item-key="id"
      show-select
      class="elevation-1 pa-3"
    >
      <template v-slot:top>
        <div class="my-2">
          <v-btn
            v-for="(btn, idx) in headerBtn"
            :key="idx"
            :disabled="btn.disabled"
            color="primary"
            class="mr-2"
            @click="setStatus(btn.status)"
          >
            {{ btn.text }}
          </v-btn>
        </div>
        <v-divider></v-divider>
      </template>
      <template v-slot:item.type="{ item }">
        {{ item.type === 11 ? '文字' : '檔案' }}
      </template>
      <template v-slot:item.status="{ item }">
        {{ item.status === 0 ? '停用' : '可用' }}
      </template>
      <template v-slot:item.userType="{ item }">
        {{ item.userType === 0 ? '匿名' : '一般' }}
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      throttle: false,
      searchObj: {
        title: '',
        // timeStart: '',
        // timeEnd: '',
        text: '',
        type: 11,
        fileName: ''
      },
      selected: [],
      headers: [
        { text: 'id', value: 'id' },
        { text: 'roomId', value: 'roomId' },
        { text: 'roomTitle', value: 'title' },
        { text: 'userId', value: 'loginId' },
        { text: '種類', value: 'type' },
        { text: '文字', value: 'text' },
        { text: '建立時間', value: 'createTime' },
        { text: '修改時間', value: 'actTime' },
        { text: '檔案名稱', value: 'fileName' },
        // { text: '檔案名稱2', value: 'hashName' },
        { text: '狀態', value: 'status' },
        { text: '使用者', value: 'userType' }
      ],
      items: []
    }
  },
  computed: {
    headerBtn() {
      let disabled = this.throttle || this.selected.length === 0
      return [
        { text: '停用', status: 0, disabled: disabled },
        { text: '可用', status: 1, disabled: disabled }
      ]
    },
    headerCompu() {
      return this.headers.filter(x => {
        let showFile = this.searchObj.type !== 11
        let isFileOnlyCol = ['fileName', 'hashName'].includes(x.value)
        let isTextOnlyCol = ['text'].includes(x.value)
        let isCommon = !(isFileOnlyCol || isTextOnlyCol)
        return showFile ? isFileOnlyCol || isCommon : isTextOnlyCol || isCommon
      })
    }
  },
  methods: {
    empty2Null(str) {
      return str && str.length > 0 ? str : null
    },
    async search() {
      this.throttle = true
      try {
        let url = '/api/A00020/Search'
        let data = {}
        for (let [key, value] of Object.entries(this.searchObj)) {
          window.console.log(`${key}, ${value}`)
          data[key] = typeof value === 'string' ? this.empty2Null(value) : value
          switch (this.searchObj.type) {
            case 11:
              data['fileName'] = null
              break
            default:
              data['text'] = null
          }
        }
        window.console.log(data)
        let res = await this.axios.post(url, data)
        window.console.log(res.data)
        this.items = res.data.chatMsgs
      } catch (error) {
        window.console.error(error)
      }
      this.throttle = false
    },
    async setStatus(status) {
      // 1:未刪除 2:刪除
      this.throttle = true
      try {
        let url = '/api/A00020/SetStatus'
        let data = {
          status: status,
          ids: this.selected.map(x => x.id)
        }
        let res = await this.axios.post(url, data)
        window.console.log(res.data)
        this.items.forEach(x => {
          x.status = data.ids.includes(x.id) ? status : x.status
        })
        this.selected.splice(0)
      } catch (error) {
        window.console.error(error)
      }
      this.throttle = false
    }
  }
}
</script>
<style lang="scss" scoped></style>
