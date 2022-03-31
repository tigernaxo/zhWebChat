import logger from '../logger'
let jwt = {
  validitySec: function(token) {
    logger.log('checking token expire day: ' + token)
    let exp = parseInt(this.parseJwt(token).exp)
    return Math.floor((exp * 1000 - Date.now()) / 1000)
  },
  parseJwt: function(token) {
    var base64Url = token.split('.')[1]
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    var jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        })
        .join('')
    )

    return JSON.parse(jsonPayload)
  }
}
export default jwt
