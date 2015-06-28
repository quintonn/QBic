var navigation = {

    entryPoint: function (userInfo)
    {
        navigation.loadHtmlBody("mainContent", "Home.html");

        navigation.getUserMenu();
    },

    getUserMenu: function()
    {
        main.makeWebCall(main.webApiURL + "getUserMenu", "GET", navigation.processUserMenuResponse);
    },

    processUserMenuResponse: function(data)
    {
        var menuList = data;
        // figure out what page to go to //TODO
        menuBuilder.buildMenu(menuList);
    },

    clearSettings: function ()
    {
        localStorage.removeItem(main.tokenName);
        localStorage.removeItem(main.userSettingName);
    },

    showLoginPage: function ()
    {
        menuBuilder.clearMenu();
        navigation.loadHtmlBody("loginContainer", "Login.html", navigation.loadLoginPageCompleted);
    },

    loadLoginPageCompleted: function ()
    {
        document.getElementById('txtName').onkeypress = function (e)
        {
            if (e.keyCode == 13)
            {
                document.getElementById('txtPassword').focus();
            }
        };
        document.getElementById('txtPassword').onkeypress = function (e)
        {
            if (e.keyCode == 13)
            {
                auth.doLogin();
            }
        };
        document.getElementById("loginContainer").style.visibility = "visible";
        document.getElementById("txtName").focus();
    },

    loadHtmlBody: function (nodeName, pageName, callback)
    {
        var url = "pages/" + pageName + '?v=' + main.version;
        var loadCompleted = function (resp, nodeName)
        {
            navigation.loadHtmlBodyResponse(resp, nodeName);
            if (callback)
            {
                callback();
            }
        };
        main.makeWebCall(url, "GET", loadCompleted, null, nodeName);
    },

    loadHtmlBodyResponse: function (response, nodeName)
    {
        var div = document.getElementById(nodeName);
        div.innerHTML = response;
    },
};