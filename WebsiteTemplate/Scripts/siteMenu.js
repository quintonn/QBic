var siteMenu = {
    buildMenu: function (role)
    {
        switch (role)
        {
            case "Admin":

                menuBuilder.addMenuButton("Users", siteMenu.viewUsers);

                break;
            default:
                break;
        }
    },

    viewUsers: function()
    {
        alert('todo');
    },

    testClick: function (callback)
    {
        main.makeWebCall(main.menuApiUrl + "test", "GET", function (d)
        {
            alert(d);
            callback();
        });
    },
};