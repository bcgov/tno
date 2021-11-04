// customize CRA configuration, add compression to js.
const { override, addExternalBabelPlugins } = require('customize-cra');
const CompressionPlugin = require('compression-webpack-plugin'); //gzip

const addCompressionPlugin = () => config => {
  if (process.env.NODE_ENV === 'production') {
    config.devtool = false;
    config.plugins.push(
      new CompressionPlugin({
        //gzip plugin
        test: /\.(js|css|html|svg)$/,
      }),
    );
  }
  return config;
};

module.exports = {
  webpack: override(
    ...addExternalBabelPlugins('@babel/plugin-proposal-nullish-coalescing-operator'),
    addCompressionPlugin(),
  ),
};
