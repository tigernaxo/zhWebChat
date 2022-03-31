import Vue from 'vue'
import Vuetify from 'vuetify/lib/framework'

Vue.use(Vuetify)

export default new Vuetify({
  theme: {
    themes: {
      light: {
        primary: '#5682a3',
        secondary: '#b0bec5',
        anchor: '#8c9eff'
      }
    }
  }
})
