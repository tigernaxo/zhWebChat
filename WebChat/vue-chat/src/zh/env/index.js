let env = {
  get isDev() {
    return process.env.NODE_ENV === 'development'
  }
}

export default env
