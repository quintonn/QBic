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
        main.makeWebCall(main.menuApiUrl + "getUsers", "GET", siteMenu.showUsers);
    },

    showUsers: function (data)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            siteMenu.createAddUserButton();

            var settings = [];
            settings.push(new views.viewSetting('Id', 'Id'));
            settings.push(new views.viewSetting('Name', 'UserName'));
            settings.push(new views.viewSetting('Email', 'Email'));
            settings.push(new views.viewSetting('Role', 'UserRole/Name'));
            settings.push(new views.viewSetting('Email Confirmed', 'EmailConfirmed', 'bool'));
            settings.push(new views.viewSetting("", "X", "button", siteMenu.confirmDeleteUser));

            views.addDataToTable(settings, data);
        });
    },

    confirmDeleteUser: function(index, data)
    {
        var userToDelete = data[index];
        var doDelete = confirm("Delete user " + userToDelete.UserName + "?");
        if (doDelete == true)
        {
            siteMenu.deleteUser(userToDelete.Id);
        }
    },

    deleteUser: function(userId)
    {
        var callback = function ()
        {
            alert('User successfully deleted');
            siteMenu.viewUsers();
        };
        main.makeWebCall(main.menuApiUrl + "deleteUser/" + userId, "DELETE", callback, userId);
    },

    createAddUserButton: function ()
    {
        var viewMenu = document.getElementById("viewsMenu");
        var button = document.createElement("button");
        button.innerHTML = "Add";
        button.onclick = function ()
        {
            siteMenu.getUserRoles();
        };
        viewMenu.appendChild(button);
    },

    getUserRoles: function ()
    {
        main.makeWebCall(main.menuApiUrl + "getUserRoles", "GET", siteMenu.showCreateUserScreen);
    },

    showCreateUserScreen: function(data)
    {
        var userRoles = data;
        
        inputDialog.loadInputPage("CreateUser.html", function ()
        {
            var roleSelect = document.getElementById('txtUserRole');
            menuBuilder.clearNode('txtUserRole');

            for (var i = 0; i < userRoles.length; i++)
            {
                var userRole = userRoles[i];
                var option = document.createElement('option');
                option.innerHTML = userRole.Name;
                option.value = userRole.Id;
                roleSelect.appendChild(option);
            }

            document.getElementById('txtUserName').focus();
            //dialog.addHiddenField("userType", userType);
        });
    },

    createUser: function()
    {
        var userName = document.getElementById("txtUserName").value;
        var email = document.getElementById("txtEmail").value;
        var pass = document.getElementById("txtPassword").value;
        var confirmPass = document.getElementById("txtConfirmPassword").value;
        
        var userRoleSelect = document.getElementById('txtUserRole');
        var userRoleId = userRoleSelect.options[userRoleSelect.selectedIndex].value;

        if (userName.length == 0 || email.length == 0 ||
            pass.length == 0 || userRoleId.length == 0)
        {
            alert("Not all fields are filled in");
            return;
        }

        var data =
        {
            name: userName,
            email: email,
            password: pass,
            confirmPassword: confirmPass,
            userRoleId: userRoleId
        };

        var callback = function ()
        {
            alert('User created successfully');
            inputDialog.cancelInput();
            
            siteMenu.viewUsers();
        };
        
        var dataToSend = JSON.stringify(data);
        
        dataToSend = encodeURIComponent(dataToSend);
        
        main.makeWebCall(main.menuApiUrl + "createUser", "POST", callback, dataToSend);
    }
};