module.exports = {
  // transpileDependencies: ["vuetify"], // 轉譯 vuetify
  // options...
  // publicPath: '/', // 不靈光，會出現空白頁面
  publicPath: process.env.NODE_ENV === 'production' ? '/admin/' : '/',
  // build 輸出資料夾
  outputDir: '../wwwRoot/Admin',
  devServer: {
    proxy: 'https://localhost:44364'
  }
}
