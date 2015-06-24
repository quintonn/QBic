var main = {
    applicationName: "websiteTemplate",
    baseURL: "https://localhost/WebsiteTemplate/",
    webApiURL: "api/v1/",
    menuApiUrl: "",
    tokenNameSuffix: "UserToken",
    tokenName: "",
    userSettingSuffix: "UserSetting",
    userSettingName: "",
    version: "0.0.2",
    scriptLoaded: false,
    refreshTimerRunning: false,

    readypage: function()
    {
        main.init();
    },

    init: function ()
    {
        menuBuilder.clearMenu();
        main.tokenName = main.applicationName + "_" + main.tokenNameSuffix + "_" + main.version;
        
        main.userSettingName = main.applicationName + "_" + main.userSettingSuffix + "_" + main.version;
        
        main.menuApiUrl = main.webApiURL + "menu/";
        
        main.makeWebCall(main.webApiURL + "initialize", "GET", main.processInitResponse);
        
    },

    processInitResponse: function(data)
    {
        console.log('processInitResponse: refreshTimerRunning: ');
        console.log(main.refreshTimerRunning);
        var userInfo = JSON.parse(data);
        document.getElementById('aUserName').innerHTML = userInfo.user;
        navigation.entryPoint(userInfo);
        if (main.refreshTimerRunning == false)
        {
            //console.log("start refresh timer from init response");
            //auth.refreshTokenHandler();
            auth.startRefreshTimer();
        }
    },

    bodyOnLoad: function()
    {
        main.init();
    },

    makeWebCall: function (url, method, callback, params, args)
    {
        var webRequest;
        
        method = method || "GET";

        if (window.XMLHttpRequest)
        {
            // code for IE7+, Firefox, Chrome, Opera, Safari
            webRequest = new XMLHttpRequest();
        }
        else
        {
            // code for IE6, IE5
            webRequest = new ActiveXObject("Microsoft.XMLHTTP");
        }
        
        webRequest.onreadystatechange = function () { main.processWebRequest(webRequest, callback, args) };
        
        url = main.baseURL + url;
        webRequest.open(method, url, true);
        
        //var tokenData = localStorage.getItem(main.tokenName);
        //tokenData = tokenData || "";
        //if (tokenData.length > 0)
        //{
        //    tokenData = JSON.parse(tokenData);
        //}
        ////var userToken = localStorage.getItem(main.tokenName);
        //var userToken = "";
        //if (tokenData != null && tokenData.refresh_token != null)
        //{
        //    userToken = tokenData.refresh_token;
        //}
        var userToken = auth.getAccessToken();
        //console.log('making web call ' + url + ' with token:\n' + userToken);
        webRequest.setRequestHeader("Authorization", "Bearer " + userToken);
        webRequest.send(params);
    },

    processWebRequest: function (req, callback, args)
    {
        if (req.readyState !== 4)
        {
            return;
        }
        switch (req.status)
        {
            case 200:
                callback(req.responseText, args);
                break;
            case 400:
                //console.log(req);
                var respData = JSON.parse(req.responseText);
                //console.log(respData.error);
                //console.log(respData.error_description);
                if (respData && respData.error && respData.error_description)
                {
                    switch (respData.error)
                    {
                        case "invalid_grant":
                            var loginError = document.getElementById("loginErrorMessage");
                            if (loginError == null)
                            {
                                alert('Not on login page. But expected to be on login page');
                                main.init();
                            }
                            loginError.innerHTML = respData.error_description;
                            break;
                        default:
                            alert("unknown error description: " + respData.error_description);
                            break;
                    }
                }
                else
                {
                    alert("Bad request");
                    //console.log("Bad request");
                }
                break;
            case 401: //Unauthorized: Need to show login screen
                navigation.showLoginPage();
                break;
            case 403: //Forbidden
                alert("You are not authorized to perform the requested action.\n" + req.statusText);
                break;
            default:
                //alert("unknown web response status: " + req.status);
                //alert(JSON.stringify(req));
                navigation.loadHtmlBodyResponse(JSON.stringify(req), "mainContent");
                break;
        }
    },

    logout: function ()
    {
        auth.logout();
    },
};