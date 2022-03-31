<template>
  <v-app id="inspire">
    <!-- <v-navigation-drawer v-model="drawer" :clipped="$vuetify.breakpoint.lgAndUp" app> -->
    <!-- :clipped="$vuetify.breakpoint.smAndUp" -->
    <v-navigation-drawer v-model="drawer" clipped :mobile-breakpoint="768" app>
      <v-list dense expand class="z-app-list">
        <MenuUserInfo
          :img="`/user/${$store.state.system.userId}/head/${$store.state.system.photo}`"
          :text="$store.state.system.userName"
        />
        <v-divider></v-divider>
        <menu-btn
          v-for="(menu, idx) in menus"
          :key="idx"
          :icon="menu.icon"
          :text="menu.text"
          :isLink="menu.isLink"
          :to="menu.to"
        />
        <v-divider></v-divider>
        <MenuBtn :icon="'mdi-logout'" :text="'登出'" :isLink="false" @click="logout()" />
      </v-list>
    </v-navigation-drawer>
    <!-- app-bar-->
    <v-app-bar :clipped-left="$vuetify.breakpoint.smAndUp" app color="primary" dark>
      <!-- drawer 收闔 icon -->
      <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
      <!-- drawer 收闔 icon 旁邊的 title 文字 -->
      <v-toolbar-title style="width: 300px" class="ml-0 pl-4">
        <span class="hidden-sm-and-down">WebChat Dashboard</span>
      </v-toolbar-title>
      <!-- 中間的分隔，讓旁邊的元素往靠左右兩邊靠攏，以上靠左，以下靠右 -->
      <v-spacer />
      <!-- 如果有元素要放在最右邊就放這 -->
      <!-- <v-btn icon large>
        <v-avatar size="32px" item class="zh-avatar">
          <v-img :src="$store.state.logo" alt="Vuetify" />
        </v-avatar>
      </v-btn> -->
    </v-app-bar>
    <!-- 用 v-main 包起來，渲染時就會先排除 appbar、drawer 佔去的版面，才不會被蓋住 -->
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script>
import MenuBtn from '../components/MenuBtn.vue'
import MenuUserInfo from '../components/MenuUserInfo.vue'
export default {
  components: {
    MenuBtn,
    MenuUserInfo
  },
  props: {
    source: String
  },
  data: () => {
    return {
      drawer: null,
      menus: [
        { icon: 'mdi-chart-areaspline', text: 'Dashboard', isLink: true, to: '/' },
        { icon: 'mdi-face', text: '使用者管理', isLink: true, to: '/A00010' },
        // { icon: 'mdi-person', text: '匿名使用者管理', isLink: true, to: '/A00012' },
        { icon: 'mdi-message', text: '對話管理', isLink: true, to: '/A00020' }
      ]
    }
  },
  methods: {
    logout() {
      window.console.log('logout')
      this.$router.push({ name: 'Login' })
      return
      // 將狀態設定為登出
      // this.$store.state.user.isLogin = false

      // // 以下 ajax 解除 cookie 登入資訊
      // let url = this.$store.state.api + '/Logout'

      // // let data = Object.assign({ queryType: '123' })
      // // this.axios.post(url, this.qs.stringify(data)).then(res => {
      // this.axios.get(url).then(res => {
      //   window.console.log(res.data)
      //   if (res.data.resultCode == '10') {
      //     this.$router.push({ name: 'Login' })
      //   } else {
      //     alert(res.data.error)
      //   }
      // })
    }
  },
  computed: {
    user() {
      return {
        // name: this.$store.state.user.name,
        // img: this.$store.state.user.img
      }
    }
  }
}
</script>

<style lang="scss">
// 加大 drawer menu list 裡面的間距
.z-app-list .v-list-item {
  padding-top: 4px !important;
  padding-bottom: 4px !important;
}
// @keyframes onhover {
//   0% {
//   }
//   50% {
//     transform: rotateY(360deg);
//   }
//   100% {
//     transform: rotateY(0deg);
//   }
// }
// .zh-avatar:hover {
//   animation-name: onhover;
//   animation-duration: 0.6s;
// }
</style>
