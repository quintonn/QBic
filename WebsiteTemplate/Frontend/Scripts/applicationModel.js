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

    self.toggleNavBar = function ()
    {
        var x = document.getElementById("smallNavbar");
        if (x.className.indexOf("w3-show") == -1)
        {
            x.className += " w3-show";
        } else
        {
            x.className = x.className.replace(" w3-show", "");
        }
    };

    self.closeNavBar = function ()
    {
        var x = document.getElementById("smallNavbar");
        if (x.className.indexOf("w3-show") > -1)
        {
            x.className = x.className.replace(" w3-show", "");
        }
    };

    self.modalDialogs = ko.observableArray([]);
    self.addModalDialog = function (model)
    {
        self.modalDialogs.push(model);

        var div = document.getElementById('modalDlg_' + _applicationModel.modalDialogs().length);
        div = div.firstChild;

        div.focus();
        
        ko.cleanNode(div);

        ko.applyBindings(model.model, div);
    };

    self.views = ko.observableArray([]);
    self.currentView = ko.observableArray([]);
    self.addView = function (model)
    {
        var viewItems = self.views();

        var existingModel = null;
        for (var i = 0; i < viewItems.length; i++)
        {
            if (viewItems[i].id == model.id)
            {
                existingModel = model;
                break;
            }
        }
        //var existingModel = $.grep(viewItems, function (v, indx)
        //{
            //return v.id == model.id;
        //});

        if (existingModel == null)
        {
            //self.views.push(model);  // This breaks the bindings (test by clicking same menu twice)
            existingModel = model;
        }

        self.currentView([existingModel]);
        existingModel.applyKoBindings();
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