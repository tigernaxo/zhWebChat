import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    system: {
      isLogin: false,
      userId: null,
      loginId: '',
      userName: '',
      photo: '',
      token: ''
    },
    types: {
      chatMsg: [
        { value: 11, text: '文字' },
        { value: 13, text: '檔案' }
      ]
    }
  },
  mutations: {},
  actions: {},
  modules: {}
})
