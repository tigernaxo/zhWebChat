import { HubConnectionBuilder } from '@microsoft/signalr'
// import { store } from '@/store'
import logger from '../logger'

// 設置連線 url
let wsUrl = '/adminhub'
if (process.env.NODE_ENV === 'development') {
  // 靠 proxy 連線有點慢，所以直接設置
  wsUrl = 'https://localhost:44364/adminhub'
}

// 設置 websocket 連線
let ws
//  = new HubConnectionBuilder()
//   .withUrl(wsUrl, {
//     accessTokenFactory: () => token
//   })
//   .withAutomaticReconnect()
//   .build()

let wsWapper = {
  ws,
  start: async function(token) {
    ws = new HubConnectionBuilder()
      .withUrl(wsUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build()

    if (ws.state === 'Disconnected') {
      try {
        await ws.start()
        logger.log('ws connected')
      } catch (err) {
        logger.error(err.toString())
      }
    }
  },
  // 保留直接呼叫 ws 的功能
  on: function() {
    ws.on(...arguments)
  },
  invoke: async function() {
    // logger.log(...arguments)
    await ws.invoke(...arguments)
  }
}
export default wsWapper
