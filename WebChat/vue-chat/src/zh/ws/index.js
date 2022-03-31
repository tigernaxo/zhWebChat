import { HubConnectionBuilder } from '@microsoft/signalr'
import { store } from '@/store'
import logger from '../logger'
import myConfig from '../../../vue.configExt'

// 設置連線 url
let wsUrl = myConfig.appPath + 'chathub'
if (process.env.NODE_ENV === 'development') {
  // 靠 proxy 連線有點慢，所以直接設置
  wsUrl = 'https://localhost:44364/chathub'
}

// 設置 websocket 連線
let ws = new HubConnectionBuilder()
  .withUrl(wsUrl, {
    accessTokenFactory: () => store.system.token
  })
  // .withAutomaticReconnect()
  .build()

let wsWapper = Object.create(ws)
wsWapper.start = async function() {
  if (this.state === 'Disconnected') {
    try {
      await ws.start()
      logger.log('ws connected')
    } catch (err) {
      logger.error(err.toString())
    }
  }
}

export default wsWapper
