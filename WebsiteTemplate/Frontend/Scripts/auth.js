﻿(function (auth, $, undefined)
{
    auth.accessToken = "";
    auth.refreshToken = "";

    var accessTokenName = "";
    var refreshTokenName = "";

    auth.initialize = function ()
    {
        return new Promise(function (resolve, reject)
        {
            accessTokenName = _applicationModel.applicationName() + "_accessToken_v" + mainApp.version;
            refreshTokenName = _applicationModel.applicationName() + "refreshToken" + mainApp.version;

            auth.accessToken = localStorage.getItem(accessTokenName);
            auth.refreshToken = localStorage.getItem(refreshTokenName);
            resolve();
        });
    }

    auth.handleLoginSuccess = function(data)
    {
        console.log('auth success');
        console.log(data);
        _applicationModel.user().name(data.userName);

        auth.accessToken = data.access_token;
        auth.refreshToken = data.refresh_token;

        localStorage.setItem(accessTokenName, auth.accessToken);
        localStorage.setItem(refreshTokenName, auth.refreshToken);

        return Promise.resolve();
    }

    auth.logout = function()
    {
        auth.accessToken = "";
        auth.refreshToken = "";
        localStorage.setItem(accessTokenName, "");
        localStorage.setItem(refreshTokenName, "");
        _applicationModel.user(new userModel());
        return mainApp.startApplication();
    }

}(window.auth = window.auth || {}, jQuery));

function loginModel(callback)
{
    var self = this;
    self.userName = ko.observable();
    self.password = ko.observable();
    self.loginClick = function ()
    {
        //var url = main.webApiURL + "token";
        var url = mainApp.apiURL + "token";
        var data = "grant_type=password&username=" + self.userName() + "&password=" + self.password() + "&client_id=" + _applicationModel.applicationName;

        dialog.showBusyDialog("Logging in...");
        mainApp.makeWebCall(url, "POST", data)
               .then(auth.handleLoginSuccess)
               .then(dialog.closeBusyDialog)
               .then(dialog.closeModalDialog)
               .then(callback);
    };
}