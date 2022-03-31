<template>
  <div
    :style="props.style"
    class="emoji-picker"
    v-if="display.visible"
    v-click-outside="hide"
  >
    <!-- <div class="emoji-picker__search"> -->
    <!-- 先不作 search -->
    <!-- <input type="text" v-model="search" v-focus /> -->
    <!-- <input type="text" /> -->
    <!-- </div> -->
    <!-- <div> -->
    <div v-for="(emojiGroup, category) in emojis" :key="category">
      <h5>{{ category }}</h5>
      <div class="emojis">
        <span
          v-for="(emoji, name) in emojiGroup"
          :key="name"
          @click="select(emoji)"
          :title="name"
        >
          {{ emoji }}
        </span>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, onMounted, onUnmounted, reactive } from 'vue'
import logger from '../logger'
// 改寫自 https://github.com/DCzajkowski/vue-emoji-picker
import emojiTable from './emojiTable'

export default {
  props: {
    style: Object
  },
  setup(props, { emit }) {
    // let thisStyle = computed(() => {
    //   return {
    //     position: 'fixed',
    //     left: `${props.style.left}px`,
    //     bottom: `${props.style.bottom}px`,
    //     width: `${props.style.width}px`
    //   }
    // })
    let isToggling = false
    const emojis = computed(() => {
      if (props.search) {
        const obj = {}
        for (const category in this.emojiTable) {
          obj[category] = {}
          for (const emoji in this.emojiTable[category]) {
            if (new RegExp(`.*${this.search}.*`).test(emoji)) {
              obj[category][emoji] = this.emojiTable[category][emoji]
            }
          }
          if (Object.keys(obj[category]).length === 0) {
            delete obj[category]
          }
        }
        return obj
      }
      return emojiTable
    })
    let display = reactive({
      visible: false,
      x: 0,
      y: 0
    })
    const select = function(emoji) {
      emit('emoji', emoji)
    }
    // 切換 emoji 列表是否顯示
    const toggle = function(e) {
      logger.log('isToggling...')
      logger.log(`display.visible:${display.visible}`)
      // display.visible = !display.visible
      display.visible = true
      isToggling = true
      logger.log(`display.visible:${display.visible}`)
      display.x = e.clientX
      display.y = e.clientY
    }
    // 隱藏 emoji 選擇介面
    const hide = function() {
      if (!isToggling) {
        display.visible = false
      }
      isToggling = false
    }
    // 註冊壓下 Esc 鍵的動作
    const escape = function(e) {
      if (display.visible === true && e.keyCode === 27) {
        display.visible = false
      }
    }
    onMounted(() => document.addEventListener('keyup', escape))
    onUnmounted(() => document.removeEventListener('keyup', escape))

    return { emojiTable, emojis, select, toggle, hide, escape, display, props }
  },
  directives: {
    'click-outside': {
      mounted(el, binding) {
        // 如果不是 handler，就不執行綁定
        if (typeof binding.value !== 'function') return

        // 監聽 document 如果點選到元件之外，就執行 handler
        const bubble = binding.modifiers.bubble
        const handler = e => {
          // 判斷是否點選到外面的元素
          let isClickOutSide = bubble || (!el.contains(e.target) && el !== e.target)
          // 如果點選到外面的元素，就執行外部作為 binding.value 導入的 handler
          if (isClickOutSide) binding.value(e)
        }
        // 把 handler 接到 dom 下，以便解除的時候使用
        // !! 竟然拿 DOM 當作傳遞物件的容器，之前竟然都沒想過
        el.__vueClickOutside__ = handler
        document.addEventListener('click', handler)
      },
      unmounted(el) {
        document.removeEventListener('click', el.__vueClickOutside__)
        el.__vueClickOutside__ = null
      }
    }
  }
}
</script>
<style lang="scss" scoped>
//  style 參考資料
// https://codepen.io/DCzajkowski/pen/jzLzWp

.emoji-picker {
  z-index: 1;
  font-family: Montserrat;
  border: 1px solid #ccc;
  height: 20rem;
  overflow: scroll;
  padding: 1rem;
  box-sizing: border-box;
  border-radius: 0.5rem;
  background: #fff;
  box-shadow: 1px 1px 8px #c7dbe6;
}

.emoji-picker h5 {
  margin-bottom: 0;
  color: #b1b1b1;
  text-transform: uppercase;
  font-size: 0.8rem;
  cursor: default;
}
.emoji-picker .emojis {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
}
.emoji-picker .emojis:after {
  content: '';
  flex: auto;
}
.emoji-picker .emojis span {
  padding: 0.2rem;
  cursor: pointer;
  border-radius: 5px;
}
.emoji-picker .emojis span:hover {
  background: #ececec;
  cursor: pointer;
}
</style>
