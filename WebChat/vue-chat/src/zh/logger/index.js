let logger = {
  debug: function(msg) {
    if (process.env.NODE_ENV === 'development') {
      if (typeof msg === 'object') {
        window.console.log(msg)
      } else {
        window.console.log('[Debug] ' + msg.toString())
      }
    }
  },
  error: function(msg) {
    if (process.env.NODE_ENV === 'development') {
      window.console.error(msg)
    } else {
      window.console.error('[Error] ' + msg.toString())
    }
  },
  log: function(msg) {
    if (typeof msg === 'object') {
      window.console.log(msg)
    } else {
      window.console.log('[Log] ' + msg.toString())
    }
  }
}

export default logger
