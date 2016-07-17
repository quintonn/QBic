(function (dialog, $, undefined)
{
    var loginHtml = "";

    dialog.showBusyDialog = function (text)
    {
        _applicationModel.addProgressIndicator(text);
        return Promise.resolve();
    };

    dialog.closeBusyDialog = function ()
    {
        _applicationModel.busyContainers.pop();
    };

    dialog.closeModalDialog = function ()
    {
        _applicationModel.modalDialogs.pop();
    };

    dialog.showDialogWithId = function (pageId, viewModel)
    {
        return mainApp.makeWebCall("pages/" + pageId + ".html?v=" + mainApp.version).then(function (data)
        {
            return dialog.showDialog(data, viewModel);
        });
    };

    dialog.showDialog = function (htmlContent, viewModel)
    {
        return Promise.resolve();
    };

    dialog.showLoginDialog = function ()
    {
        if (loginHtml.length == 0)
        {
            return mainApp.makeWebCall("Frontend/Pages/Login.html?v=" + mainApp.version).then(function (data)
            {
                console.log('showlogin call');
                loginHtml = data;
                return addLoginModel();
            });
        }
        else
        {
            return addLoginModel();
        }
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
        console.log('addLoginModel done');
        return Promise.resolve();
    }

}(window.dialog = window.dialog || {}, jQuery));