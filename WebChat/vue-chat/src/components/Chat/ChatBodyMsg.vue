<template>
  <div class="d-flex mt-1">
    <template v-if="!status.isSelfMsg">
      <div class="flex-shrink-0 text-start">
        <template v-if="msg.userType === 2">
          {{ `匿名使用者${msg.userId}` }}
        </template>
        <template v-else>
          [{{
            store.users[`${msg.userId}@${msg.userType}`]?.userName ?? '顯示名稱無法辨識'
          }}
          ({{
            store.users[`${msg.userId}@${msg.userType}`]?.loginId ?? '登入Id無法辨識'
          }})]
        </template>
      </div>
    </template>
    <div class="flex-grow-1 d-flex " :class="{ 'justify-content-end': status.isSelfMsg }">
      <div class="msg-box" :class="classObj2.msgbox">
        <template v-if="!status.isFile">
          <div class="text-break" v-for="(el, idx) in msgLines" :key="idx">
            {{ el }}
          </div>
        </template>
        <template v-else>
          <div v-if="!status.isFileImage">
            <a
              :href="`/room/${msg.roomId}/${msg.hashName}`"
              class="m-0"
              target="_blank"
              :download="msg.fileName"
            >
              <span class="badge bg-secondary p-2">
                {{ msg.fileName }}
                <i class="fas fa-download ms-2"></i>
              </span>
            </a>
          </div>
          <div v-else>
            <img
              class="mw-100"
              v-if="status.isFileImage"
              :src="`/room/${msg.roomId}/${msg.hashName}`"
            />
          </div>
        </template>
        <div>
          {{ formatTime(msg.createTime) }}
        </div>
      </div>
    </div>
    <template v-if="status.isSelfMsg">
      <div class="flex-shrink-0 text-end">
        [{{ store.users[`${msg.userId}@${msg.userType}`]?.userName ?? '未知' }} ({{
          store.users[`${msg.userId}@${msg.userType}`]?.loginId ?? ''
        }})]
      </div>
    </template>
  </div>
</template>

<script>
import { inject, onMounted } from 'vue'
import file from '@/zh/file'
export default {
  props: {
    msg: Object
  },
  setup(props, { emit }) {
    onMounted(() => emit('mounted'))
    // 獲取聯絡人資料
    let store = inject('store')
    // 取得傳送該訊息的使用者資訊
    let msgLines = ''
    let status = {
      isSelfMsg: store.system.userId === props.msg.userId,
      isFileImage: false,
      isFile: false,
      isEvent: false
    }
    // 11文字/12圖片/13檔案/14已編輯文字/21加入聊天/22離開聊天
    // let arr
    switch (props.msg.type) {
      case 11:
        msgLines = props.msg.text.split('\n')
        break
      case 12:
      case 13:
        status.isFile = true
        status.isFileImage = file.isImage(props.msg?.hashName)
        break
      case 21:
      case 22:
        break
      default:
    }

    let classObj2 = {
      msgbox: {
        'msg-box-self': status.isSelfMsg,
        'msg-box-other': !status.isSelfMsg
      }
    }
    // function padLeft(str, cnt) {
    // }
    function formatTime(timeStr) {
      let newStr = timeStr.replace('T', ' ')
      newStr = newStr.substr(0, 19)
      return newStr
    }
    return {
      classObj2,
      status,
      store,
      msgLines,
      formatTime
    }
  }
}
</script>

<style lang="scss" scoped>
// 自己對話框的顏色 #5d86aa
// 對方對話框的顏色 #ffffff
.msg-box-self,
.msg-box-other {
  padding: 0.5em;
  position: relative;
  border-radius: 5px;
}
.msg-box-other {
  margin-left: 15px;
  background-color: #ffffff;
  color: black;
}
.msg-box-other::after {
  content: '';
  width: 0px;
  height: 0px;
  position: absolute;
  border-left: 4px solid transparent;
  border-right: 4px solid #fff;
  border-top: 4px solid #fff;
  border-bottom: 4px solid transparent;
  left: -8px;
  top: 6px;
}
.msg-box-self {
  margin-right: 15px;
  background-color: #5d86aa;
  color: white;
}
.msg-box-self a {
  margin-right: 15px;
  // background-color: #5d86aa;
  color: white;
  text-decoration: none;
}
.msg-box-self::before {
  content: '';
  width: 0px;
  height: 0px;
  position: absolute;
  border-left: 4px solid #5d86aa;
  border-right: 4px solid transparent;
  border-top: 4px solid #5d86aa;
  border-bottom: 4px solid transparent;
  right: -8px;
  top: 6px;
}
</style>
