window.config = {
  transpiler: 'plugin-babel',
  meta: {
    '*.vue': {
      loader: 'vue-loader',
    },
  },
  map: {
    'vue': 'npm:vue@3.0.0/dist/vue.esm-browser.js',
    'vue-loader': 'npm:dx-systemjs-vue-browser@1.0.15/index.js',
    'plugin-babel': 'npm:systemjs-plugin-babel@0.0.25/plugin-babel.js',
    'systemjs-babel-build': 'npm:systemjs-plugin-babel@0.0.25/systemjs-babel-browser.js',
  },
  packages: {
  },
  packageConfigPaths: [
    'npm:@devextreme/*/package.json',
    'npm:@devextreme/runtime@2.3.14/inferno/package.json',
  ],
  babelOptions: {
    sourceMaps: false,
    stage0: true,
  },
};
System.config(window.config);