var menuBuilder = {

    buildMenu: function (role)
    {
        menuBuilder.clearMenu();

        document.getElementById("userDetail").style.display = "inline";

        navigation.loadHtmlBody('menuContainer', 'Menu.html', function()
        {
            menuBuilder.clearNode('menuDiv');
            siteMenu.buildMenu(role);
        });
    },

    addMenuButton: function(btnLabel, onClickEvent)
    {
        var menuDiv = document.getElementById('menuDiv');
        var button = menuBuilder.createMenuButton(btnLabel);

        var callback = function ()
        {
            button.disabled = false;
        };

        var onClickWrapper = function ()
        {
            button.disabled = true;
            //callback = callback || function () { };
            try
            {
                if (onClickEvent.length > 0)
                {
                    onClickEvent(callback);
                }
                else
                {
                    onClickEvent();
                    callback();
                }
            }
            catch (err)
            {
                console.log(err + "\n" + err.stack);;
            }
        };
        button.onclick = onClickWrapper;

        menuDiv.appendChild(button);
    },

    createMenuButton: function(btnLabel)
    {
        var button = document.createElement('button');
        button.innerHTML = btnLabel;
        return button;
    },
    
    clearMenu: function ()
    {
        menuBuilder.clearNode('menuContainer');

        document.getElementById("loginContainer").style.visibility = "hidden";
        document.getElementById("userDetail").style.display = "none";
        /// see: http://stackoverflow.com/questions/3955229/remove-all-child-elements-of-a-dom-node-in-javascript
    },

    clearNode: function(nodeName)
    {
        var node = document.getElementById(nodeName);
        while (node.firstChild)
        {
            node.removeChild(node.firstChild);
        }
    }
};