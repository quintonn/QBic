const MergeIntoSingleFilePlugin = require('webpack-merge-and-include-globally');
const fs = require('fs')
const PostCompile = require('post-compile-webpack-plugin');
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const glob = require("glob");
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
    entry: glob.sync("./Frontend/Scripts/*.js"),
    plugins: [
        new CleanWebpackPlugin(),
        new HtmlWebpackPlugin({
            title: 'Production',
            template: './Index.html',
            filename: 'Index.html'
        }),
        new MergeIntoSingleFilePlugin({
            files: {
                "qbic.js": [
                    'Frontend/Scripts/jquery-3.1.0.min.js',
                    'Frontend/Scripts/jquery-ui.min.js',
                    'Frontend/Scripts/**/*.js',
                ],
                "qbic.css": [
                    "Frontend/css/!(siteOverrides)*.css",
                    ],
            }
        }),
        new CopyPlugin({
            patterns: [
                { from: './Frontend/fonts', to: 'fonts' },
                { from: './Frontend/Pages', to: 'pages' }
            ]
        }),
        new PostCompile(() =>
        {
            fs.unlinkSync(__dirname + "/wwwroot/main"); // delete the main file created by webpack
        })
    ],
    output: {
        filename: '[name]',
        path: path.resolve(__dirname, 'wwwroot'),
    },
    externals: {
        // fix references to jquery. Without this the build crashes
        "jquery": "jQuery",
    },
};