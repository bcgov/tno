const path = require('path');

module.exports = {
  entry: './src/index.ts',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    alias: {
      '@assets': path.resolve(__dirname, '../../../../app/subscriber/public/assets'),
    },
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'dist'),
    sourceMapFilename: '[name].js.map',
  },
  devtool: 'source-map',
};

// Add this to print the resolved alias path
console.log(
  'Resolved @assets path:',
  path.resolve(__dirname, '../../../../app/subscriber/public/assets'),
);
