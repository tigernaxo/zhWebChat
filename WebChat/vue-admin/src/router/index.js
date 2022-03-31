import Vue from 'vue'
import VueRouter from 'vue-router'
import Admin from '../views/Admin'
import A00000 from '../views/A00000'
import A00010 from '../views/A00010'
// import A00012 from '../views/A00012'
import A00020 from '../views/A00020'
import Login from '../views/Login'
// import store from '../store'

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    component: Admin,
    children: [
      { path: '', name: 'Admin', component: A00000 },
      { path: 'A00010', name: 'A00010', component: A00010 },
      // { path: 'A00002', name: 'A00002', component: A00002},
      { path: 'A00020', name: 'A00020', component: A00020 }
    ]
  },
  { path: '/Login', name: 'Login', component: Login }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
