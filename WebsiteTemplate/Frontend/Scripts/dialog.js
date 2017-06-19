(function (dialog, $, undefined)
{
    var loginHtml = "";

    dialog.showBusyDialog = function (text)
    {
        _applicationModel.addProgressIndicator(text);

        return Promise.resolve();
    };

    dialog.getUserConfirmation = function (settings, data, params)
    {
        dialog.closeBusyDialog(); /// This is because the busy dialog covers the confirmation dialog. TODO: need to fix this
        var cancelText = settings.CancelButtonText;
        var confirmText = settings.ConfirmationButtonText;
        var message = settings.ConfirmationMessage;
        var confirmEvent = settings.OnConfirmationUIAction;
        var cancelEvent = settings.OnCancelUIAction;

        return new Promise(function (resolve, reject)
        {
            data = data || {};
            data["parameters"] = params;
            
            var model = new confirmationModel(message, confirmText, confirmEvent, cancelText, cancelEvent, resolve, data);
            return dialog.showDialogWithId('confirmation', model);
        });
    };

    dialog.closeAllBusyDialogs = function ()
    {
        while (_applicationModel.busyContainers().length > 0)
        {
            _applicationModel.busyContainers.pop();
        }
    };

    dialog.closeBusyDialog = function ()
    {
        var model = _applicationModel.busyContainers()[_applicationModel.busyContainers().length - 1];
        if (model == null)
        {
            console.warn('closing busy dialog called, but model is null');
            console.log('this is because of the code inside getUserConfirmation');
        }
        _applicationModel.busyContainers.pop();

       return Promise.resolve();
    };

    dialog.closeModalDialog = function ()
    {
        _applicationModel.modalDialogs.pop();
        return Promise.resolve();
    };

    dialog.showDialogWithId = function (pageId, viewModel)
    {
        return mainApp.makeWebCall("frontend/pages/" + pageId + ".html").then(function (data)
        {
            return dialog.showDialog(data, viewModel);
        });
    };

    dialog.showDialog = function (htmlContent, viewModel)
    {
        var model = new modalDialogModel(htmlContent, viewModel, _applicationModel.modalDialogs().length + 1);

        _applicationModel.addModalDialog(model);
        return Promise.resolve();
    };

    dialog.showLoginDialog = function ()
    {
        if (loginHtml.length == 0)
        {
            return mainApp.makeWebCall("Frontend/Pages/Login.html").then(function (data)
            {
                loginHtml = data;
                return addLoginModel();
            });
        }
        else
        {
            return addLoginModel();
        }
    };

    dialog.showPasswordResetDialog = function ()
    {
        return mainApp.makeWebCall("Frontend/Pages/PasswordResetRequest.html").then(function (data)
        {
            return addPasswordResetModel(data);
        });
    };

    dialog.showMessage = function (type, message)
    {
        return new Promise(function (resolve, reject)
        {
            var dlgSetting = new dialogSetting(type, message, resolve);
            dialog.showDialogWithId('dialog', dlgSetting);
        });
    };

    function addPasswordResetModel(data)
    {
        var resetModel = new passwordResetModel();
        var model = new modalDialogModel(data, resetModel, _applicationModel.modalDialogs().length + 1);
        _applicationModel.addModalDialog(model);
        return Promise.resolve();
    }

    function addLoginModel()
    {
        try
        {
            var lModel = new loginModel();
            var model = new modalDialogModel(loginHtml, lModel, _applicationModel.modalDialogs().length + 1);

            _applicationModel.addModalDialog(model);
        }
        catch (err)
        {
            console.error(err);
        }
        
        return Promise.resolve();
    }

    function confirmationModel(message, confirmButtonText, confirmEvent, cancelButtonText, cancelEvent, callback, data)
    {
        var self = this;
        self.message = ko.observable(message);
        self.confirmationText = ko.observable(confirmButtonText);
        self.cancelText = ko.observable(cancelButtonText);

        self.confirmClick = function ()
        {
            dialog.closeModalDialog();
            if (confirmEvent > 0)
            {
                dialog.showBusyDialog("Processing...");
                mainApp.executeUIAction(confirmEvent, data).then(dialog.closeBusyDialog).then(callback);
            }
            else
            {
                callback();
            }
        };
        self.cancelClick = function ()
        {
            dialog.closeModalDialog();
            if (cancelEvent > 0)
            {
                dialog.showBusyDialog("Processing...");
                mainApp.executeUIAction(cancelEvent, data).then(dialog.closeBusyDialog).then(callback);
            }
            else
            {
                callback();
            }
        };
    }

    function dialogSetting(heading, message, onCloseCallback)
    {
        var self = this;
        self.heading = heading;
        self.message = message;

        self.closeClick = function ()
        {
            dialog.closeModalDialog();
            onCloseCallback();
        };

        self.errorTypeColor = ko.computed(function ()
        {
            switch (self.heading.toLowerCase())
            {
                case "info":
                    return "w3-green";
                case "warn":
                case "warning":
                    return "w3-orange";
                case "error":
                    return "w3-red";
                default:
                    return "w3-green";
            }
        }, self);
    }

    function modalDialogModel(html, model, id)
    {
        var self = this;
        self.html = ko.observable(html);
        self.model = model;
        self.myid = ko.observable(id);
    }

}(window.dialog = window.dialog || {}, jQuery));