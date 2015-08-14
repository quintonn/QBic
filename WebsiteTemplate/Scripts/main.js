var main = {
    applicationName: "websiteTemplate",
    baseURL: "https://localhost/WebsiteTemplate/",
    webApiURL: "api/v1/",
    menuApiUrl: "",
    tokenNameSuffix: "UserToken",
    tokenName: "",
    userSettingSuffix: "UserSetting",
    userSettingName: "",
    version: "0.0.3",
    scriptLoaded: false,

    bodyOnLoad: function ()
    {
        main.init();
    },

    init: function ()
    {
        menuBuilder.clearMenu();

        main.tokenName = main.applicationName + "_" + main.tokenNameSuffix + "_" + main.version;
        main.userSettingName = main.applicationName + "_" + main.userSettingSuffix + "_" + main.version;

        main.menuApiUrl = main.webApiURL + "menu/";

        var confirmedUser = navigation.getParameterByName('confirmed');
        if (confirmedUser != null && confirmedUser.length > 0)
        {
            inputDialog.showMessage("Email for " + confirmedUser + " successfully confirmed.\nYou can now log in with the credentials for " + confirmedUser);
            /// Remove the 'confirm' parameter from the URL
            var url = window.location.href;
            var index = url.lastIndexOf('?');
            url = url.substring(0, index);
            window.history.pushState(url, main.applicationName, url);
        }
        
        main.makeWebCall(main.webApiURL + "initialize", "GET", main.processInitResponse);
    },

    processInitResponse: function(data)
    {
        var userInfo = data;
        
        document.getElementById('aUserName').innerHTML = userInfo.User;
        navigation.entryPoint(userInfo);

        auth.startRefreshTimer();
    },

    makeWebCall: function (url, method, callback, params, args)
    {
        main.showBusyIndicator1();

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
        
        var userToken = auth.getAccessToken();
        
        webRequest.setRequestHeader("Authorization", "Bearer " + userToken);

        //var contentType = "application/x-www-form-urlencoded";
        //params = encodeURI(params);
        
        webRequest.send(params);
    },

    processWebRequest: function (req, callback, args)
    {
        if (req.readyState !== 4)
        {
            return;
        }
        main.hideBusyIndicator();
        switch (req.status)
        {
            case 200:
                
                var contentType = req.getResponseHeader('content-type');
                contentType = contentType || "";

                var response = req.responseText;

                if (contentType.indexOf("application/json") > -1)
                {
                    response = JSON.parse(response);
                }
                
                callback(response, args);
                break;
            case 400:
                var contentType = req.getResponseHeader('content-type');
                contentType = contentType || "";
                
                var respData = req.response;
                if (contentType.indexOf("application/json") > -1)
                {
                    respData = JSON.parse(respData);
                }
                
                if (respData && respData.error && respData.error_description)
                {
                    switch (respData.error)
                    {
                        case "invalid_grant":
                            var loginError = document.getElementById("loginErrorMessage");
                            if (loginError == null)
                            {
                                inputDialog.showMessage('Not on login page. But expected to be on login page');
                                main.init();
                            }
                            loginError.innerHTML = respData.error_description;
                            break;
                        default:
                            inputDialog.showMessage("unknown error description: " + respData.error_description);
                            break;
                    }
                }
                else
                {
                    inputDialog.showMessage(respData.Message);
                }
                break;
            case 401: //Unauthorized: Need to show login screen
                navigation.showLoginPage();
                break;
            case 403: //Forbidden
                inputDialog.showMessage("You are not authorized to perform the requested action.\n" + req.statusText);
                break;
            case 500:
                var err = JSON.parse(req.response);
                err = err.ExceptionMessage || err;
                inputDialog.showMessage("Internal Server Error:\n" + err);
                break;
            default:
                
                menuBuilder.clearNode('menuContainer');
                menuBuilder.clearNode('userDetail');
                //document.getElementById('menuContainer').style.display = "none";
                
                navigation.loadHtmlBodyResponse(req.response, "mainContent");
                break;
        }
    },

    showBusyIndicator1: function()
    {
        var container = document.getElementById('busyContainer');
        container.style.visibility = 'visible';
    },

    hideBusyIndicator: function()
    {
        var container = document.getElementById('busyContainer');
        container.style.visibility = 'hidden';
    },
};