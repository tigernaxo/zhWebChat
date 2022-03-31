// vue.config.js
module.exports = {
  // options...
  publicPath: process.env.NODE_ENV === 'production' ? '/WebChat/Chat' : '/',
  // build 輸出資料夾
  outputDir: '../wwwRoot/Chat',
  devServer: {
    proxy: 'https://localhost:44364/'
  }
}
