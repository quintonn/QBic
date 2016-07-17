var _applicationModel;

$(document).ready(function ()
{
    _applicationModel = new applicationModel();
    ko.applyBindings(_applicationModel, $('body')[0]);

    mainApp.initialize();
});

(function (mainApp, $, undefined)
{
    //Private Property
    //var isHot = true;
    var apiVersion = "v1";
    
    mainApp.baseURL = "https://" + window.location.host + window.location.pathname;
    
    mainApp.apiURL = "api/" + apiVersion + "/";

    mainApp.version = "";

    mainApp.initialize = function ()
    {
        dialog.showBusyDialog("Initializing...");
        return mainApp.makeWebCall(mainApp.apiURL + "\initializeSystem").then(function(data)
        {
            var appName = data['ApplicationName'];
            var version = data['Version'];
            mainApp.version = version;
            _applicationModel.applicationName(appName);

            document.title = appName + " " + version;
            
            return mainApp.startApplication();

        }).catch(function ()
        {
            //dialog.closeBusyDialog();
        }).then(function(data)
        {
            dialog.closeBusyDialog();
        });
        
        //main.makeWebCall(main.webApiURL + "initializeSystem", "GET", main.processInitSystemResponse);
    };

    mainApp.startApplication = function ()
    {
        return auth.initialize().then(function ()
        {
            return mainApp.makeWebCall(mainApp.apiURL + "\initialize").then(function (data)
            {
                var userName = data['User'];
                var role = data['Role'];
                var id = data['Id'];
                var user = new userModel(id, userName, role);
                _applicationModel.user(user);
                return Promise.resolve();
            });
        });
    };

    mainApp.handleError = function (err)
    {
        console.log(err);
        return Promise.resolve();
    };

    mainApp.makeWebCall = function (url, method, data)
    {
        return new Promise(function (resolve, reject)
        {
            $.ajax(
                {
                    //contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                    url: mainApp.baseURL +"/"+ url + "?v="+mainApp.version,
                    data: data,
                    method: method || "GET",
                    cache: false,
                    beforeSend: function (xhr)
                    {
                        xhr.setRequestHeader("Authorization", "Bearer " + auth.accessToken);
                    }
                })
                .done(function (data)
                {
                    resolve(data);
                })
                .fail(function (error, err, errorText)
                {
                    console.log("Error while trying to call /" + url);
                    console.log(error.status + ': ' + error.statusText);
                    
                    if (error.status == 401)  // not logged in
                    {
                        // this is where i can try and refresh the access token
                        dialog.showLoginDialog();
                    }
                    else if (error.status == 400 && error.responseText.indexOf('invalid_grant') > -1)
                    {
                        alert('invalid login');
                    }
                    else
                    {
                        //reject(err);
                        mainApp.handleError(error);
                    }
                    dialog.closeBusyDialog();
                });
        });
    }
}(window.mainApp = window.mainApp || {}, jQuery));