(function (menus, $, undefined)
{
    menus.loadMenu = function ()
    {
        console.log('loadMenu');
        dialog.showBusyDialog("Loading menu");
        return mainApp.makeWebCall(mainApp.apiURL + "\getUserMenu").then(function (data)
        {
            var menuItems = menus.getMenus(data, null);
            
            _applicationModel.menuContainer().allMenus(menuItems);
            dialog.closeBusyDialog();
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
            //var parentMenu = menu.ParentMenu;
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
        if (self.eventId == "home")
        {
            mainApp.startApplication();
        }
        else if (self.eventId == 'back')
        {
            var currentParent = _applicationModel.menuContainer().currentParent();
            _applicationModel.menuContainer().currentParent(currentParent.parentMenu);
        }
        else if (self.eventId == null)
        {
            _applicationModel.menuContainer().currentParent(self);
        }
        else
        {
            console.log('clicked menu with eventId ' + self.eventId);
            //TODO: 
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