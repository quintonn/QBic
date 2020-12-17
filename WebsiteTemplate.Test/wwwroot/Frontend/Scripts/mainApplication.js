var _applicationModel;

$(document).ready(function ()
{
    _applicationModel = new applicationModel();
    ko.applyBindings(_applicationModel, $('body')[0]);

    mainApp.initializeApplication();
});

(function (mainApp, $, undefined)
{
    var apiVersion = "v1";

    var scheme = "https";
    
    mainApp.baseURL = scheme + "://" + window.location.host + window.location.pathname;
    
    mainApp.apiURL = "api/" + apiVersion + "/";
    
    mainApp.version = "";

    mainApp.initializeApplication = function ()
    {
        dialog.showBusyDialog("Initializing...");
        return mainApp.makeWebCall(mainApp.apiURL + "initializeSystem").then(function(data)
        {

            var appName = data['ApplicationName'];
            var version = data['Version'];
            var error = data['ConstructorError'];
            if (error != null && error.length > 0)
            {
                console.log('There was a constructor error:');
                console.log(error);
            }
            mainApp.version = version;
            _applicationModel.applicationName(appName);
            
            document.title = appName + " " + version;
            
            return mainApp.startApplication();

        }).then(dialog.closeBusyDialog);
    };

    mainApp.startApplication = function ()
    {
        _applicationModel.views([]);
        _applicationModel.currentView([]);
        console.clear();
        console.log('start app');
        return auth.initialize()
                   .then(mainApp.initialize)
                   .then(menus.loadMenu).catch(function(err)
                   {
                       dialog.closeBusyDialog();

                       console.error(err);
                       if (err.indexOf('AnonAction:') > -1)
                       {
                           var anonymousAction = mainApp.getParameterByName('anonAction');
                           var params = mainApp.getParameterByName("params");
                           
                           dialog.closeModalDialog(); // call this in case log in dialog is showing

                           //window.location.href = window.location.href.split("?")[0].split("#")[0];

                           return mainApp.executeUIAction(anonymousAction, params, true);
                       }

                       if (err.indexOf('Confirmed:') >-1)
                       {
                           var parts = err.split(":");
                           var user = parts[1];
                           
                           return dialog.showMessage("Info", "Thank you for confirming you email address. You can now log in using your username and password").then(function ()
                           {
                               window.location.href = window.location.href.split("?")[0].split("#")[0];
                               return auth.logout();
                           });
                       }
                       
                       return mainApp.handleError(err);
                   });
    };

    mainApp.initialize = function ()
    {
        var anonymousAction = mainApp.getParameterByName("anonAction");
        if (anonymousAction != null && anonymousAction.length > 0)
        {
            return Promise.reject('AnonAction:' + anonymousAction);
        }

        var confirmedUser = mainApp.getParameterByName("confirmed");
        if (confirmedUser != null && confirmedUser.length > 0)
        {
            return Promise.reject('Confirmed:' + confirmedUser);
        }

        var errors = mainApp.getParameterByName("errors");
        if (errors != null && errors.length > 0)
        {
            return Promise.reject(errors);
        }
        
        return mainApp.makeWebCall(mainApp.apiURL + "initialize").then(function (data)
        {
            var userName = data['User'];
            var role = data['Role'];
            var id = data['Id'];
            var user = new userModel(id, userName, role);
            _applicationModel.user(user);
            return Promise.resolve();
        });
    };

    mainApp.getParameterByName = function(name, url)
    {
        if (url == null || url.length == 0)
        {
            url = window.location.href;
        }
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    mainApp.handleError = function (err)
    {
        console.trace();
        console.error(err);
        var errMsg = err;
        if (err.responseText != null)
        {
            errMsg = err.responseText;
            
            try
            {
                var tmp = JSON.parse(errMsg);
                if (tmp.Message != null)
                {
                    errMsg = tmp.Message;
                }
            }
            catch (error)
            {
                console.log(error);
            }
        }
        if (err.stack != null)
        {
            errMsg += ":\n" + err.stack;
        }
        
        dialog.closeAllBusyDialogs();
        dialog.showMessage("Error", errMsg);
        return Promise.resolve();
    };

    mainApp.executeUIAction = function (eventId, params)
    {
        return new Promise(function (resolve, reject)
        {
            var data =
                {
                    Data: params || ""
                };

            data = JSON.stringify(data);

            var url = mainApp.apiURL + "executeUIAction/" + eventId;
            return mainApp.makeWebCall(url, "POST", data).then(function (resp)
            {
                return mainApp.processUIActionResult(resp, eventId);
            }).then(resolve);
        }).catch(function (err)
        {
            mainApp.handleError(err);
        });
    };

    mainApp.processEvent = function (eventId, actionId, params)
    {
        var data =
            {
                Data: params || "",
                ActionId: actionId,
            };
        data = JSON.stringify(data);

        var url = mainApp.apiURL + "processEvent/" + eventId;
        return mainApp.makeWebCall(url, "POST", data).then(function (resp)
        {
            return mainApp.processUIActionResult(resp, eventId);
        });
    };

    mainApp.raisePropertyChanged = function (params, eventId)
    {
        params = JSON.stringify(params);
        var url = mainApp.apiURL + "propertyChanged/" + eventId;
        return mainApp.makeWebCall(url, "POST", params).then(function (resp)
        {
            return mainApp.processUIActionResult(resp, eventId);
        });
    };

    mainApp.processUIActionResult = function (data, eventId)
    {
        return processing.processUIActionResult(data, eventId).catch(function (err)
        {
            return mainApp.handleError(err);
        });;
    };

    mainApp.makeWebCall = function (url, method, data, headersToInclude)
    {
        var urlToCall = url;
        if (url.indexOf("http") == -1)
        {
            urlToCall = mainApp.baseURL + url;
        }
        return new Promise(function (resolve, reject)
        {
            $.ajax(
                {
                    //contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                    url: urlToCall + "?v=" + mainApp.version,
                    data: data,
                    method: method || "GET",
                    crossDomain: true,
                    
                    //cache: false,
                    cache: url.indexOf("html") > -1,
                    beforeSend: function (xhr)
                    {
                        xhr.setRequestHeader("Authorization", "Bearer " + auth.accessToken);
                    }
                })
                .done(function (data, textStatus, request)
                {
                    if (headersToInclude == null || headersToInclude.length == 0)
                    {
                        resolve(data);
                    }
                    else
                    {
                        var resp = {};
                        resp['data'] = data;
                        for (var i = 0; i < headersToInclude.length; i++)
                        {
                            var head = headersToInclude[i];
                            resp[head] = request.getResponseHeader(head);
                        }
                        resolve(resp);
                    }
                })
                .fail(function (error, err, errorText)
                {
                    console.error("Error while trying to call /" + url);
                    console.error(error.status + ': ' + error.statusText);
                    
                    if (error.status == 401)  // not logged in
                    {
                        if (auth.refreshToken != null && auth.refreshToken.length > 0) // Try refresh the token
                        {
                            console.warn('auth no longer valid, trying to get a new access token');
                            return auth.performTokenRefresh().then(function ()
                            {
                                console.warn('refresh token updated successfully');
                                // If successfully refreshed the token, retry the web call we just tried
                                return mainApp.makeWebCall(url, method, data).then(function (retryData)
                                {
                                    resolve(retryData);
                                });

                            }).catch(function (err)
                            {
                                console.error(err);
                                dialog.closeBusyDialog();
                                //mainApp.handleError(err);
                                dialog.showLoginDialog();
                            });
                        }
                        else
                        {
                            dialog.showLoginDialog();
                        }
                    }
                    else if (error.status == 400 && error.responseText.indexOf('invalid_grant') > -1)
                    {
                        dialog.showMessage("Error", "Username or password incorrect, try again.");
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