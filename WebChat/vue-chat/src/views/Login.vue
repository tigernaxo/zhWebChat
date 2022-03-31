<template>
  <!-- 版面參考https://web.telegram.im/#/login?p= -->
  <div class="login">
    <div class="login_head"></div>
    <div class="login_page" style="">
      <div class="login_form_wrap">
        <form>
          <h3 class="login_form_head">登入</h3>
          <!-- {{ loginInfo }} -->
          <p class="login_form_lead">
            請輸入您的登入資訊
          </p>
          <div class="login_form_input_group">
            <div class="login_form_input_wraper">
              <div class="login_form_input_label">UserId</div>
              <input class="login_form_input" v-model="loginInfo.userId" />
            </div>
          </div>
          <div class="login_form_input_group">
            <div class="login_form_input_wraper">
              <label class="login_form_input_label"> Password </label>
              <input
                class="login_form_input"
                type="password"
                v-model="loginInfo.password"
              />
            </div>
          </div>
        </form>
        <div class="login_form_btn_wrapper">
          <button class="login_form_btn" @click="signIn()">
            <i class="fas fa-angle-double-right login_form_btn_icon"></i>
          </button>
        </div>
      </div>
      <div class="login_footer_wrap">
        <p>Welcome to WebChat Powered by Yu-Cheng, Chen.</p>
        <a class="logo_footer_link" href="https://www.zhtech.com.tw/" target="_blank">
          Zhtech
        </a>
      </div>
    </div>
  </div>
</template>

<script>
// import axios from 'axios'
import axios from '@/zh/axios'
import logger from '@/zh/logger'
import { inject, reactive } from 'vue'
import env from '@/zh/env'
import jwt from '@/zh/jwt'
export default {
  setup() {
    let store = inject('store')

    let loginInfo = reactive({
      userId: env.isDev ? 'zhtech' : '',
      password: env.isDev ? '24369238' : ''
    })

    async function signIn() {
      try {
        let url = '/api/Token/signin'
        let url2 = 'api/Token/signin'
        console.log('start to signin', url, url2)
        let res = await axios.post(url, loginInfo)
        if (res.data.resultCode == '01') {
          throw res.data.error
        }
        store.system.token = res.data.token
        localStorage.setItem('token', res.data.token)
        let payload = jwt.parseJwt(res.data.token)

        store.system.userId = parseInt(payload.id)
        store.system.loginId = payload.loginId
        store.system.userName = payload.userName

        store.system.isLogin = true
      } catch (e) {
        logger.error(e)
      }
    }
    return { signIn, loginInfo }
  }
}
</script>

<style lang="scss" scoped>
.login {
  background: rgb(231, 235, 240);
  height: 100%;
  width: 100%;
}
.login_head {
  background: #5682a3;
  height: 226px;
}
.login_page {
  margin: -131px auto 90px;
  max-width: 404px;
}
.login_form_wrap {
  // 輸入表單外框陰影
  box-shadow: 0 1px 1px rgba(97, 127, 152, 0.2), 1px 0 0 rgba(97, 127, 152, 0.1),
    -1px 0 0 rgba(97, 127, 152, 0.1);
  border-radius: 5px;
  border: 0;
  background: #fff;
  padding: 44px 65px;
}
.login_form_head {
  color: #222;
  margin: 0 0 20px;
  // font-size: 15px;
  font-size: 1.2em;
  font-weight: 700;
}
.login_form_lead {
  color: #999;
  margin: 15px 0 30px;
  font-size: 1.1em;
  line-height: 160%;
  // border-bottom: rgb(211, 29, 29) 1px solid;
}
.login_form_input_group {
  // height: 50px;
  height: 3.2em;
  border-bottom: 1px solid #e6e6e6;
  padding: 0;
  margin: 0 0 22px;
}
.login_form_input_label {
  transform: scale(0.9);
  font-weight: 400;
  color: #999;
  cursor: pointer;
  display: block;
  font-size: 1.1em;
  z-index: 1;
  pointer-events: none;
  -webkit-font-smoothing: antialiased;
  transform-origin: left center;
  -webkit-transform-origin: left center;
}
.login_form_input {
  color: #000;
  background: #eee;
  display: inline-block;
  border: 0;
  outline: 0;
  font-size: 1.1em;
  padding: 3px 0;
  margin: 3px 0 0;
  width: 100%;
  resize: none;
  -webkit-box-shadow: none;
  -moz-box-shadow: none;
  box-shadow: none;
}
.login_footer_wrap {
  color: #84a2bc;
  font-size: 13px;
  line-height: 16px;
  margin-top: 25px;
  text-align: center;
}
.logo_footer_link {
  color: #84a2bc;
  font-weight: 600;
  text-decoration: none;
}
.logo_footer_link:active,
.logo_footer_link:focus,
.logo_footer_link:hover {
  color: #437ab8;
  font-weight: 800;
  text-decoration: underline;
}
.login_form_btn_wrapper {
  margin-top: 35px;
  text-align: center;
}
.login_form_btn_icon:active,
.login_form_btn_icon:focus,
.login_form_btn_icon:hover {
  font-weight: 700;
  color: #e7ebf0;
}
/* Style buttons */
.login_form_btn {
  border-radius: 90%;
  background-color: #e7ebf0; /* Blue background */
  border: none; /* Remove borders */
  color: #437ab8;
  padding: 12px 16px; /* Some padding */
  font-size: 16px; /* Set a font size */
  cursor: pointer; /* Mouse pointer on hover */
}
.login_form_btn_icon_wrapper {
  text-align: center;
}
.login_form_btn_icon {
  font-size: 1.5em;
  cursor: pointer;
}

/* Darker background on mouse-over */
.login_form_btn:hover {
  background-color: #5682a3;
}
</style>
