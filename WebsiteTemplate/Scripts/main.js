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
    refreshTimerRunning: false,

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
        var userInfo = data;
        
        document.getElementById('aUserName').innerHTML = userInfo.User;
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
                    alert(respData.Message);
                    console.log(req);
                    
                    //console.log(req.response.message);
                    //console.log(req.responseTex.message);
                    //console.log(respData.message);
                    //console.log(respData.error_description);
                }
                break;
            case 401: //Unauthorized: Need to show login screen
                navigation.showLoginPage();
                break;
            case 403: //Forbidden
                alert("You are not authorized to perform the requested action.\n" + req.statusText);
                break;
            case 500:
                //console.log("500 response:\n" + JSON.stringify(req));
                var err = JSON.parse(req.response);
                err = err.ExceptionMessage || err;
                alert("Internal Server Error:\n" + err);
                break;
            default:
                
                menuBuilder.clearNode('menuContainer');
                menuBuilder.clearNode('userDetail');
                //document.getElementById('menuContainer').style.display = "none";
                
                navigation.loadHtmlBodyResponse(req.response, "mainContent");
                break;
        }
    },
};