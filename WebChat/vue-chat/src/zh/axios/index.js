import axios from 'axios'
// import vueConfig from '../../../vue.config'
import myConfig from '../../../vue.configExt'
const request = axios.create({ baseURL: myConfig.appPath })
// Add a request interceptor
request.interceptors.request.use(function(config) {
    // Do something before request is sent
    console.log('vueConfig.publicPath:', myConfig.appPath)
    console.log('baseUrl:', config.baseURL)
    console.log('config:', config)
    return config
  },
  function(error) {
    // Do something with request error
    return Promise.reject(error)
  })
export default request
