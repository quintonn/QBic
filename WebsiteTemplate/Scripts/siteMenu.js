var siteMenu = {
    buildMenu: function (userInfo)
    {
        switch (userInfo.role)
        {
            case "Admin":

                menuBuilder.addMenuButton("Home", function ()
                {
                    navigation.entryPoint(userInfo);
                });
                menuBuilder.addMenuButton("Users", siteMenu.getUsers);
                menuBuilder.addMenuButton("User Roles", siteMenu.getUserRolesForView);

                break;
            default:
                break;
        }
    },

    getUserRolesForView: function ()
    {
        main.makeWebCall(main.menuApiUrl + "getUserRoles", "GET", siteMenu.showUserRoles);
    },

    showUserRoles: function (data)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            //siteMenu.createAddUserButton();
            var settings = [];
            settings.push(new views.viewSetting('Id', 'Id'));
            settings.push(new views.viewSetting('Name', 'Name'));//, 'a', siteMenu.editUserRole));
            settings.push(new views.viewSetting('Description', 'Description'));
            //settings.push(new views.viewSetting("", "X", "button", siteMenu.confirmDeleteUserRole));

            views.addDataToTable(settings, data);
        });
    },

    getUsers: function()
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
            settings.push(new views.viewSetting('Name', 'UserName', 'a', siteMenu.editUser));
            settings.push(new views.viewSetting('Email', 'Email'));
            settings.push(new views.viewSetting('Role', 'UserRole/Name'));
            settings.push(new views.viewSetting('Email Confirmed', 'EmailConfirmed', 'bool'));
            settings.push(new views.viewSetting("", "#Resend Confirmation Email", "a", siteMenu.resendConfirmationEmail, siteMenu.showResendConfirmationLink))
            settings.push(new views.viewSetting("", "X", "button", siteMenu.confirmDeleteUser));

            views.addDataToTable(settings, data);
        });
    },

    editUser: function(index, data)
    {
        var user = data[index];
        siteMenu.getUserRoles(user);
    },

    showResendConfirmationLink: function(index, data)
    {
        var user = data[index];
        return user.EmailConfirmed == false;
    },

    resendConfirmationEmail: function(index, data)
    {
        var userToDelete = data[index];

        var callback = function (data)
        {
            alert(data);
            siteMenu.getUsers();
        };
        main.makeWebCall(main.menuApiUrl + "resendConfirmationEmail/" + userToDelete.Id, "POST", callback);
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
            siteMenu.getUsers();
        };
        main.makeWebCall(main.menuApiUrl + "deleteUser/" + userId, "DELETE", callback);
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

    getUserRoles: function (editItem)
    {
        var callback = function (data)
        {
            if (editItem == null)
            {
                siteMenu.showCreateUserScreen(data, editItem);
            }
            else
            {
                siteMenu.showEditUserScreen(data, editItem);
            }
            
        };
        main.makeWebCall(main.menuApiUrl + "getUserRoles", "GET", callback);
    },

    showCreateUserScreen: function(data, editItem)
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
        });
    },

    showEditUserScreen: function (data, editItem)
    {
        var userRoles = data;

        inputDialog.loadInputPage("EditUser.html", function ()
        {
            var roleSelect = document.getElementById('txtUserRole');
            menuBuilder.clearNode('txtUserRole');

            var selectedIndex = 0;
            for (var i = 0; i < userRoles.length; i++)
            {
                var userRole = userRoles[i];
                var option = document.createElement('option');
                option.innerHTML = userRole.Name;
                option.value = userRole.Id;
                roleSelect.appendChild(option);
                if (editItem.UserRole == userRole.Name)
                {
                    selectedIndex = 0;
                }
            }

            roleSelect.selectedIndex = selectedIndex;

            document.getElementById('txtUserName').value = editItem.UserName;
            document.getElementById('txtEmail').value = editItem.Email;

            inputDialog.addHiddenField("userId", editItem.Id);

            document.getElementById('txtUserName').focus();
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
            userRoleId: userRoleId,
        };

        var callback = function (data)
        {
            alert(data);
            inputDialog.cancelInput();
            
            siteMenu.getUsers();
        };
        
        var dataToSend = JSON.stringify(data);
        
        dataToSend = encodeURIComponent(dataToSend);
        
        main.makeWebCall(main.menuApiUrl + "createUser", "POST", callback, dataToSend);
    },

    updateUser: function ()
    {
        var userId = inputDialog.getHiddenField('userId');

        var userName = document.getElementById("txtUserName").value;
        var email = document.getElementById("txtEmail").value;

        var userRoleSelect = document.getElementById('txtUserRole');
        var userRoleId = userRoleSelect.options[userRoleSelect.selectedIndex].value;

        if (userName.length == 0 || email.length == 0)
        {
            alert("Not all fields are filled in");
            return;
        }

        var data =
        {
            name: userName,
            email: email,
            userRoleId: userRoleId,
            //userId: userId
        };

        var callback = function (data)
        {
            alert(data);
            inputDialog.cancelInput();

            siteMenu.getUsers();
        };

        var dataToSend = JSON.stringify(data);

        dataToSend = encodeURIComponent(dataToSend);
        
        main.makeWebCall(main.menuApiUrl + "updateUser/" + encodeURI(userId), "POST", callback, dataToSend);
    }
};