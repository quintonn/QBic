const MergeIntoSingleFilePlugin = require('webpack-merge-and-include-globally');
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const glob = require("glob");

module.exports = {
    entry: glob.sync("./Scripts/*.js"),
    plugins: [
        new MergeIntoSingleFilePlugin({
            files: {
                "vendor.js": [
                    'Scripts/jquery-3.1.0.min.js',
                    'Scripts/jquery-ui.min.js',
                    'Scripts/base64.js?v=3',
                    'Scripts/knockout-3.4.0.js',
                    'Scripts/knockout-jqueryui.min.js',
                    'Scripts/promise-7.0.4.min.js',
                    'Scripts/promise-done-7.0.4.min.js',
                    'Scripts/date.js',
                    'Scripts/applicationModel.js',
                    'Scripts/dialog.js',
                    'Scripts/inputDialog.js',
                    'Scripts/auth.js',
                    'Scripts/menus.js',
                    'Scripts/processing.js?v=4',
                    'Scripts/views.js',
                    'Scripts/mainApplication.js'
                ],
                "vendor.css": [
                    'css/*.css'
                ]
            }
        }),
    ],
    externals: {
        // require("jquery") is external and available
        //  on the global var jQuery
        "jquery": "jQuery",
    }
};