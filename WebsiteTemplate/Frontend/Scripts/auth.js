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
            accessTokenName = _applicationModel.applicationName() + "_accessToken";
            refreshTokenName = _applicationModel.applicationName() + "_refreshToken";
            auth.accessToken = localStorage.getItem(accessTokenName);
            auth.refreshToken = localStorage.getItem(refreshTokenName);
            resolve();
        });
    }

    auth.handleLoginSuccess = function(data)
    {
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

    auth.performLogin = function (username, password)
    {
        var url = mainApp.apiURL + "token";
        var data = "grant_type=password&username=" + username + "&password=" + password + "&client_id=" + _applicationModel.applicationName();

        dialog.showBusyDialog("Logging in...");
        return mainApp.makeWebCall(url, "POST", data)
                      .then(auth.handleLoginSuccess)
                      .then(mainApp.startApplication)
                      .then(dialog.closeBusyDialog)
                      .then(dialog.closeModalDialog);
    }

    auth.performTokenRefresh = function ()
    {
        console.log('performing token refresh');
        //dialog.showBusyDialog("Refreshing token..."); // Todo: Maybe don't show this
        var url = mainApp.apiURL + "token";

        //var data = "grant_type=refresh_token&refresh_token=" + auth.refreshToken + "&client_id=" + _applicationModel.applicationName();

        var data =
       {
           "grant_type": "refresh_token",
           "refresh_token": auth.refreshToken,
           "client_id": _applicationModel.applicationName()
       };

        return new Promise(function (resolve, reject)
        {
            $.ajax(
                {
                    url: url,
                    method: "POST",
                    headers: {
                        "Authorization": auth.refreshToken
                    },
                    data: data
                }).done(function (resp)
                {
                    console.warn('successfully got new refresh token');
                    console.warn(resp);
                    auth.handleLoginSuccess(resp).then(dialog.closeBusyDialog).then(resolve);
                }).fail(function(error)
                {
                    console.warn('unable to refresh token');
                    console.warn(error);
                    reject(error);
                });
        });
    };

}(window.auth = window.auth || {}, jQuery));

function passwordResetModel()
{
    var self = this;
    self.emailOrUsername = ko.observable();
    self.cancelClick = function()
    {
        dialog.closeModalDialog();
        auth.logout();
    };
    self.resetPasswordClick = function ()
    {
        var url = mainApp.apiURL + "menu/RequestPasswordReset";
        //var data = "{usernameOrEmail:" + self.emailOrUsername() + "}";
        var data =
        {
            usernameOrEmail: self.emailOrUsername()
        };
        data = JSON.stringify(data);

        dialog.showBusyDialog("Resetting password...");
        mainApp.makeWebCall(url, "POST", data)
                      .then(function (responseData)
                      {
                          dialog.closeBusyDialog();
                          return dialog.showMessage("Info", responseData);
                      })
                      //.then(auth.handleLoginSuccess)
                      //.then(mainApp.startApplication)
                      .then(dialog.closeBusyDialog)
                      .then(dialog.closeModalDialog)
                      .then(auth.logout);
    };
}

function loginModel(callback)
{
    var self = this;
    self.userName = ko.observable();
    self.password = ko.observable();
    self.loginClick = function ()
    {
        auth.performLogin(self.userName(), self.password());
    };
    self.forgotPasswordClick = function ()
    {
        dialog.closeModalDialog().then(dialog.showPasswordResetDialog);
    };

    self.passwordKeyPressed = function (model, evt)
    {
        if (evt.keyCode == 13)
        {
            self.loginClick();
        }
        return true;
    };
}