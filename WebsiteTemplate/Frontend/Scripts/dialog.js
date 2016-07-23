(function (dialog, $, undefined)
{
    var loginHtml = "";

    dialog.showBusyDialog = function (text)
    {
        _applicationModel.addProgressIndicator(text);
        return Promise.resolve();
    };

    dialog.getUserConfirmation = function (settings, data)
    {
        var cancelText = settings.CancelButtonText;
        var confirmText = settings.ConfirmationButtonText;
        var message = settings.ConfirmationMessage;
        var confirmEvent = settings.OnConfirmationUIAction;
        var cancelEvent = settings.OnCancelUIAction;

        return new Promise(function (resolve, reject)
        {
            var model = new confirmationModel(message, confirmText, confirmEvent, cancelText, cancelEvent, resolve, data);
            return dialog.showDialogWithId('confirmation', model);
        });
    };

    dialog.closeBusyDialog = function ()
    {
        console.log('close busy dialog');
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
        return mainApp.makeWebCall("frontend/pages/" + pageId + ".html?v=" + mainApp.version).then(function (data)
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
            return mainApp.makeWebCall("Frontend/Pages/Login.html?v=" + mainApp.version).then(function (data)
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

    dialog.showMessage = function (type, message)
    {
        var dlgSetting = new dialogSetting(type, message);
        //dialog.showDialog(data, dlgSetting);
        return dialog.showDialogWithId('dialog', dlgSetting);

    };

    function addLoginModel()
    {
        try
        {
            var lModel = new loginModel();
            var model = new modalDialogModel(loginHtml, lModel, _applicationModel.modalDialogs().length + 1);

            _applicationModel.addModalDialog(model);
        } catch (err)
        {
            console.log(err);
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
                mainApp.executeUIAction(theColumn.Event.cancelEvent, data).then(dialog.closeBusyDialog).then(callback);
            }
            else
            {
                callback();
            }
        };
    }

    function dialogSetting(heading, message)
    {
        var self = this;
        self.heading = heading;
        self.message = message;

        self.closeClick = function ()
        {
            dialog.closeModalDialog();
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