(function (auth, $, undefined)
{
    auth.accessToken = "";
    auth.refreshToken = "";
    auth.lastTokenRefresh = ""; /* The date a new refresh token was last obtained */

    var accessTokenName = "";
    var refreshTokenName = "";
    var lastTokenRefreshName = "";

    auth.initialize = function ()
    {
        return new Promise(function (resolve, reject)
        {
            accessTokenName = _applicationModel.appNameAndVersion() + "_accessToken";
            refreshTokenName = _applicationModel.appNameAndVersion() + "_refreshToken";
            lastTokenRefreshName = _applicationModel.appNameAndVersion() + "_lastTokenRefresh";
            console.log('refresh storage names:\n', accessTokenName, '\n', refreshTokenName, '\n', lastTokenRefreshName);
            auth.accessToken = localStorage.getItem(accessTokenName);
            auth.refreshToken = localStorage.getItem(refreshTokenName);
            auth.lastTokenRefresh = localStorage.getItem(lastTokenRefreshName);

            /* Get new refresh token once per day */
            var lastRefreshDate = auth.lastTokenRefresh || "";
            //console.log('last refresh date = ', lastRefreshDate);
            if (lastRefreshDate == null || lastRefreshDate.length == 0)
            {
                lastRefreshDate = JSON.stringify(new Date());
            }
            lastRefreshDate = new Date(JSON.parse(lastRefreshDate));
            //console.log('last refresh date = ', lastRefreshDate);

            var today = new Date();
            //console.log('today: ', today);
            if (lastRefreshDate.setHours(0, 0, 0, 0) < today.setHours(0, 0, 0, 0))
            {
                console.log('last refresh was before today');
                auth.performTokenRefresh().catch(function (err)
                {
                    console.error("Error performing auth refresh", err);
                    resolve();
                }).then(resolve);
            }
            else
            {
                //console.log('last refresh was today');
                resolve();
            }

            //resolve();
        });
    }

    auth.handleLoginSuccess = function(data)
    {
        console.log('handling login success', data);
        _applicationModel.user().name(data.userName);

        auth.accessToken = data.access_token;
        auth.refreshToken = data.refresh_token;

        localStorage.setItem(accessTokenName, auth.accessToken);
        localStorage.setItem(refreshTokenName, auth.refreshToken);
        localStorage.setItem(lastTokenRefreshName, JSON.stringify(new Date()));

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
        
        var url = mainApp.apiURL + "token" + "?v=" + mainApp.version;

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
                    data: data
                }).done(function (resp)
                {
                    console.warn('successfully got new refresh token');
                    console.warn(resp);
                    localStorage.setItem(lastTokenRefreshName, JSON.stringify(new Date()));
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