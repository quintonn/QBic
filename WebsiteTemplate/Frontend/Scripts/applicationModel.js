function applicationModel()
{
    var self = this;

    self.applicationName = ko.observable();

    self.user = ko.observable(new userModel());
    self.showUser = ko.computed(function ()
    {
        return self.user().name() != null && self.user().name().length > 0
    }, self);

    self.busyContainers = ko.observableArray([]);
    self.addProgressIndicator = function (text)
    {
        var model = new busyDialogModel(text);
        self.busyContainers.push(model);
    };

    self.logoutClick = function ()
    {
        auth.logout();
    };

    self.modalDialogs = ko.observableArray([]);
    self.addModalDialog = function (model)
    {
        self.modalDialogs.push(model);

        var div = document.getElementById('modalDlg_' + _applicationModel.modalDialogs().length);
        div = div.firstChild;

        ko.cleanNode(div);

        ko.applyBindings(model.model, div);
    };
}

function busyDialogModel(text)
{
    var self = this;
    self.busyText = ko.observable(text);

    //self.click = function ()
    //{
    //    _applicationModel.busyContainers.remove(self);
    //};
}

function userModel(id, name, role)
{
    var self = this;

    self.id = id;
    self.name = ko.observable(name);
    self.role = role;
}

function modalDialogModel(html, model, id)
{
    var self = this;
    self.html = ko.observable(html);
    self.model = model;
    self.myid = ko.observable(id);
}