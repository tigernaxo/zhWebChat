<template>
  <div>
    <v-card class="mx-auto my-5" max-width="600" outlined>
      <v-list-item three-line>
        <v-list-item-content>
          <div class="overline mb-4"></div>
          <v-list-item-title class="headline mb-1">
            <div>
              目前在線使用者數:{{
                ws.state === 'Connected' ? info.userCount : '尚未連線'
              }}
            </div>
            <div class="my-1">
              目前連線數:{{ ws.state === 'Connected' ? info.connectCount : '尚未連線' }}
            </div>
            <div>
              <span>
                今日使用量:
                {{
                  conTimeInfo.totalSec === null
                    ? '尚未取得資料'
                    : `${conTimeInfo.totalSec}`
                }}
              </span>
              <span class="caption">
                {{ conTimeInfo.totalSec === null ? '' : 'con*sec' }}
                {{ conTimeInfo.updateTime === null ? '' : conTimeInfo.updateTime }}</span
              >
              <v-btn
                class="ms-2"
                outlined
                rounded
                text
                :disabled="conTimeInfo.throttle"
                @click="getConTime"
              >
                <v-icon> mdi-update </v-icon>
              </v-btn>
            </div>
          </v-list-item-title>
          <v-list-item-subtitle> </v-list-item-subtitle>
        </v-list-item-content>
        <!-- <v-list-item-avatar tile size="80" color="grey"></v-list-item-avatar> -->
      </v-list-item>

      <v-card-actions>
        <v-btn outlined rounded text :disabled="!btnState.play" @click="toggle">
          <v-icon> mdi-play-circle </v-icon>
        </v-btn>
        <v-btn outlined rounded text :disabled="!btnState.stop" @click="toggle">
          <v-icon> mdi-stop-circle </v-icon>
        </v-btn>
      </v-card-actions>
    </v-card>
    <!-- <div>
      {{ ws }}
    </div>
    <div>
      {{ info }}
    </div> -->
  </div>
</template>

<script>
import { HubConnectionBuilder } from '@microsoft/signalr'
import logger from '../zh/logger'
import axios from 'axios'
// import { logger } from '@/zh'

let wsUrl =
  process.env.NODE_ENV === 'development'
    ? 'https://localhost:44364/adminhub'
    : `${window.location.origin}/adminhub`

export default {
  data() {
    return {
      throttle: false,
      ws: {},
      info: {
        intervalID: '',
        querying: true,
        connectCount: 0,
        userCount: 0,
        updateTime: ''
      },
      conTimeInfo: {
        throttle: false,
        totalSec: null,
        updateTime: null
      }
    }
  },
  methods: {
    async getConTime() {
      this.conTimeInfo.throttle = true
      try {
        let url = '/api/Admin/GetConTime'
        let res = await axios.get(url)
        window.console.log(res.data)
        let date = new Date(res.data.updateTime)
        this.conTimeInfo.totalSec = res.data.totalSec
        this.conTimeInfo.updateTime = `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`
      } catch (error) {
        window.console.error(error)
      }
      this.conTimeInfo.throttle = false
    },
    async toggle() {
      switch (this.ws.state) {
        case 'Connected':
          window.clearInterval(this.info.intervalID)
          await this.ws.stop()
          break
        case 'Disconnected':
          await this.ws.start()
          await this.ws.invoke('query')
          this.info.intervalID = window.setInterval(
            async function() {
              await this.ws.invoke('query')
            }.bind(this),
            3000
          )
          break
        default:
          // Connecting
          // Disconnecting
          // Reconnecting
          return
      }
    }
  },
  created() {
    window.console.log('created')
    this.ws = new HubConnectionBuilder()
      .withUrl(wsUrl, { accessTokenFactory: () => this.$store.state.system.token })
      .withAutomaticReconnect()
      .build()
    this.ws.on(
      'QueryResponse',
      function(payload) {
        logger.log(payload)
        Object.assign(this.info, { ...payload })
      }.bind(this)
    )
  },
  computed: {
    btnState() {
      return {
        play: this.ws.state === 'Disconnected',
        pause: this.ws.state === 'Connected' && this.info.querying,
        stop: this.ws.state === 'Connected'
      }
    }
  },
  async destroyed() {
    window.clearInterval(this.info.intervalID)
    if (this.ws.state === 'Connected') await this.ws.stop()
    delete this.ws
  }
}
</script>
<style lang="scss" scoped></style>
