(function (menus, $, undefined)
{
    menus.loadMenu = function ()
    {
        return mainApp.makeWebCall(mainApp.apiURL + "getUserMenu").then(function (data)
        {
            var menuItems = menus.getMenus(data, null);
            _applicationModel.menuContainer().allMenus(menuItems);
        });
    };

    menus.getMenus = function(menuData, parentMenu)
    {
        var menuItems = [];
        for (var i = 0; i < menuData.length; i++)
        {
            var menu = menuData[i];
            var eventId = menu.Event;
            var id = menu.Id;
            var name = menu.Name;

            var model = new menuModel(id, name, eventId);
            model.parentMenu = parentMenu;
            
            var subMenus = menu.SubMenus;
            menuItems.push(model);
            if (subMenus != null)
            {
                subMenus = menus.getMenus(subMenus, model);
                menuItems = menuItems.concat(subMenus);
            }
        }
        return menuItems;
    }

}(window.menus = window.menus || {}, jQuery));

function menuModel(id, name, eventId)
{
    var self = this;
    self.id = id;
    self.name = ko.observable(name);
    self.eventId = eventId;

    self.parentMenu = null;

    self.menuClick = function ()
    {
        if (self.eventId != "back" && self.eventId != null)
        {
            _applicationModel.closeNavBar();
        }
        dialog.showBusyDialog("Processing...");
        if (self.eventId == "home")
        {
            _applicationModel.menuContainer().currentParent(null);
            mainApp.startApplication().then(dialog.closeBusyDialog);
        }
        else if (self.eventId == 'back')
        {
            var currentParent = _applicationModel.menuContainer().currentParent();
            _applicationModel.menuContainer().currentParent(currentParent.parentMenu);
            dialog.closeBusyDialog();
        }
        else if (self.eventId == null)
        {
            _applicationModel.menuContainer().currentParent(self);
            dialog.closeBusyDialog();
        }
        else
        {
            mainApp.executeUIAction(self.eventId).then(dialog.closeBusyDialog);
        }
        return Promise.resolve();
    };
}

function menuContainer()
{
    var self = this;
    
    self.allMenus = ko.observableArray([]);

    self.currentParent = ko.observable();

    self.currentMenuList = ko.computed(function ()
    {
        var currentParent = self.currentParent();
        var allMenus = self.allMenus();
        
        allMenus = $.grep(allMenus, function (m)
        {
            if (m.parentMenu == null || currentParent == null)
            {
                return currentParent == null && m.parentMenu == null;
            }
            return m.parentMenu.id == currentParent.id;
        });
        return allMenus;
    }, self);

    self.home = ko.observable(new menuModel(null, 'Home', 'home'));
    self.back = ko.observable(new menuModel(null, '<<', 'back'));
}