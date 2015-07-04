var auth = {

    doLogin: function ()
    {
        var username = document.getElementById('txtName').value;
        var password = document.getElementById('txtPassword').value;

        var url = main.webApiURL + "token";
        var data = "grant_type=password&username=" + username + "&password=" + password + "&client_id=" + main.applicationName;

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
        console.log(refreshToken);
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
        
        console.log("start refresh timer from refreshTokenResponse with timeout: " + expireTimeout / 1000 + " seconds / " + expireTimeout / 60000 + " minutes");
        setTimeout(auth.refreshTokenHandler, expireTimeout);
    },

    refreshTokenHandler: function ()
    {
        auth.makeRefreshCall();
    },

    clearRefreshTokenHandler: function()
    {
        clearTimeout(auth.refreshTokenHandler);
    },

    logout: function ()
    {
        auth.clearRefreshTokenHandler();
        navigation.clearSettings();
        main.init();
    },
};