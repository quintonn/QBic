var auth = {

    doLogin: function ()
    {
        var username = document.getElementById('txtName').value;
        //var username = $("#txtName").val();
        //var password = $("#txtPassword").val();
        var password = document.getElementById('txtPassword').value;

        var url = main.webApiURL + "token";
        var data = "grant_type=password&username=" + username + "&password=" + password + "&client_id=" + main.applicationName;

        //main.makeAjaxCall(url, "POST", data, auth.onLoginSuccess)
        main.makeWebCall(url, "POST", auth.onLoginSuccess, data);
    },

    onLoginSuccess: function (data)
    {
        console.log('login success: ' + data);
        auth.refreshTokenResponse(data);
        menuBuilder.clearNode("loginContainer");
        main.init();
    },

    makeRefreshCall: function()
    {
        var refreshToken = auth.getRefreshToken();

        var data = "grant_type=refresh_token&client_id=" + main.applicationName + "&refresh_token=" + refreshToken;

        main.makeWebCall(main.webApiURL + "token", "POST", auth.refreshTokenResponse, data);
    },

    getRefreshToken: function ()
    {
        try
        {
            var tokenData = localStorage.getItem(main.tokenName);
            tokenData = tokenData || "";
            if (tokenData.length > 0)
            {
                tokenData = JSON.parse(tokenData);
            }
            //var userToken = localStorage.getItem(main.tokenName);
            var userToken = "";
            if (tokenData != null && tokenData.refresh_token != null)
            {
                userToken = tokenData.refresh_token;
            }
            return userToken;
        }
        catch (err)
        {
            alert(err + "\n" + err.stack);
            localStorage.removeItem(main.tokenName);
        }
    },

    getAccessToken: function ()
    {
        try
        {
            var tokenData = localStorage.getItem(main.tokenName);
            tokenData = tokenData || "";
            if (tokenData.length > 0)
            {
                tokenData = JSON.parse(tokenData);
            }
            //var userToken = localStorage.getItem(main.tokenName);
            var userToken = "";
            if (tokenData != null && tokenData.access_token != null)
            {
                userToken = tokenData.access_token;
            }
            return userToken;
        }
        catch (err)
        {
            alert(err + "\n" + err.stack);
            localStorage.removeItem(main.tokenName);
        }
    },

    refreshTokenResponse: function(data)
    {
        var userToken = data.access_token;
        var userName = data.userName;

        localStorage.setItem(main.tokenName, JSON.stringify(data));

        //console.log(expireTimeout);
        
        auth.startRefreshTimer();
    },

    startRefreshTimer: function()
    {
        var data = localStorage.getItem(main.tokenName);

        data = JSON.parse(data);

        var expireTime = data[".expires"];
        console.log('expire: ' + expireTime);
        expireTime = new Date(expireTime);
        console.log('expire: ' + expireTime);

        var diff = Math.abs(new Date() - expireTime) - 5000; // refresh a few seconds before timeout
        var expireTimeout = diff;

        var refreshToken = data.refresh_token;
        //var expireTimeout = data.expires_in;

        //console.log(expireTimeout);

        //expireTimeout = parseInt(expireTimeout);
        
        //expireTimeout = expireTimeout - 1;

        //expireTimeout = expireTimeout * 1000;
        
        console.log("start refresh timer from refreshTokenResponse with timeout: " + expireTimeout / 1000 + " seconds / " + expireTimeout / 60000 + " minutes");
        setTimeout(auth.refreshTokenHandler, expireTimeout);
        main.refreshTimerRunning = true;

        console.log("TODO:xxxxxxxxx");
        ///TODO: Need to save the refreshTimerRunning to localStorage so that if another tab is opened we don't call refresh multiple times.
    },

    refreshTokenHandler: function ()
    {
        //console.log('refresh Token handler');
        
        auth.makeRefreshCall();
    },

    clearRefreshTokenHandler: function()
    {
        //console.log('******************************************************************************');
        //console.log('         clear and start refresh token handler              ');
        //console.log('******************************************************************************');
        clearTimeout(auth.refreshTokenHandler);
        main.refreshTimerRunning = false;
        //auth.refreshTokenHandler();
    },

    logout: function ()
    {
        auth.clearRefreshTokenHandler();
        navigation.clearSettings();
        //navigation.showLoginPage();
        main.init();
    },
};