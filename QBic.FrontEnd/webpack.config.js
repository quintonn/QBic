﻿const MergeIntoSingleFilePlugin = require('webpack-merge-and-include-globally');
const fs = require('fs')
const fse = require('fs-extra');
const PostCompile = require('post-compile-webpack-plugin');
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const glob = require("glob");
const CopyPlugin = require('copy-webpack-plugin');
const WatchExternalFilesPlugin = require('webpack-watch-files-plugin').default;

module.exports = env =>
{
    return {
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
                    { from: './Frontend/Pages', to: 'pages' },
                    { from: './Frontend/css/siteOverrides.css' },
                    { from: './readme.txt' },
                ]
            }),
            new PostCompile(() =>
            {
                fs.unlinkSync(__dirname + "/_wwwroot/main"); // delete the main file created by webpack

                console.log('-----------------');
                console.log(__dirname);
                console.log('-----------------');

                var destDirs = [];

                destDirs.push("../WebsiteTemplate.Test/wwwroot");

                if (env && env.prod && env.prod == true)
                {
                    destDirs.push("../WebsiteTemplate/_wwwroot");
                }

                // copy files to other folders too
                destDirs.forEach(f =>
                {
                    // copy site override css file to temp buffer
                    var tmp = fs.readFileSync(f + "/siteOverrides.css");

                    console.log('copying to ' + f);
                    fse.copySync(__dirname + "/_wwwroot", f, { overwrite: true }, function (err)
                    {
                        if (err)
                        {
                            console.error(err);
                        }
                        else
                        {
                            console.log("success!");
                        }
                    });

                    // put site override css file back
                    fs.writeFileSync(f + "/siteOverrides.css", tmp);
                });
            }),
            // Also watch for any changes. CSS files not auto watched by webpack watch flag.
            new WatchExternalFilesPlugin({
                files: [
                    './Frontend/Scripts/*.js',
                    './Frontend/css/*.css'
                ]
            })
        ],
        output: {
            filename: '[name]',
            path: path.resolve(__dirname, '_wwwroot'),
        },
        externals: {
            // fix references to jquery. Without this the build crashes
            "jquery": "jQuery",
        },
    }
};