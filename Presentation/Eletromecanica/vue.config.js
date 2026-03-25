const { VuetifyPlugin } = require('webpack-plugin-vuetify')

module.exports = {
  transpileDependencies: ['vuetify'],
  configureWebpack: {
    plugins: [new VuetifyPlugin()]
  }
}
