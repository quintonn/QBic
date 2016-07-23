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

    self.views = ko.observableArray([]);
    self.currentView = ko.observable();
    self.addView = function (model)
    {
        model.myid(self.views().length + 1);
        while (self.views().length > 0)
        {
            self.views.pop();
        }
        //TODO: take this away, and have code add view if it's not been added before or focus it if it has been added before etc.
        ///     For this i will need to add the view's event id or something

        var viewItems = self.views();
        self.views.push(model);
        self.currentView([model]);

        var id = 'view_' + model.myid();
        
        var div = document.getElementById(id);
        div = div.firstChild;

        ko.cleanNode(div);

        ko.applyBindings(model, div);
    };

    self.menuContainer = ko.observable(new menuContainer());
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