import CompressionPlugin from "compression-webpack-plugin"
import CopyWebpackPlugin from "copy-webpack-plugin"
//@ts-ignore
import DashboardPlugin from "webpack-dashboard/plugin"
import HtmlWebpackPlugin from "html-webpack-plugin"
import MiniCssExtractPlugin from "mini-css-extract-plugin"
import path from "path"
import webpack from "webpack"
import WebpackShellPluginNext from "webpack-shell-plugin-next"
import { BundleAnalyzerPlugin } from "webpack-bundle-analyzer"

const mode: webpack.Configuration["mode"] = "development"
const base = path.resolve(__dirname, "out")

const config: webpack.Configuration = {
  target: "node",
  mode,
  entry: [path.resolve(__dirname, "src/entry.ts")],
  resolve: {
    extensions: [".tsx", ".ts", ".js"],
  },
  module: {
    rules: [
      {
        enforce: "pre",
        test: /\.js$/,
        loader: "source-map-loader",
      },
      {
        test: /\.tsx?$/,
        use: ["awesome-typescript-loader", "eslint-loader"],
        exclude: /node_modules/,
      },
      {
        test: /\.css$/,
        use: [
          MiniCssExtractPlugin.loader,
          "css-modules-typescript-loader",
          {
            loader: "css-loader",
            options: {
              modules: {
                localIdentName: "[local]",
              },
              localsConvention: "camelCaseOnly",
            },
          },
        ],
      },
      {
        test: /\.(png|jpe?g|gif)$/,
        use: {
          loader: "file-loader",
          options: {
            name: "[name].[ext]",
          },
        },
      },
      {
        test: /\.exec\.js$/,
        use: "script-loader",
      },
    ],
  },
  output: {
    path: base,
  },
  plugins: [
    new DashboardPlugin({}),
    new BundleAnalyzerPlugin({
      analyzerMode: "static",
    }),
    // new WebpackShellPluginNext({
    //   onBuildEnd: {
    //     scripts: ["node ./out/main.js"]
    //   }
    // }),
  ],
  devServer: {
    disableHostCheck: true,
    contentBase: "out",
    hot: true,
    overlay: true,
    compress: true,
    writeToDisk: true,
    host: "0.0.0.0",
    port: 80,
  },
}

export default config
