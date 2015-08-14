var siteMenu = {

    buildMenu: function (menuList)
    {
        menuBuilder.addMenuButton("Home", function ()
        {
            //navigation.processUserMenuResponse(menuList);
            //navigation.entryPoint(menuList);
            menuBuilder.buildMenu(menuList);
        });
        
        for (var key in menuList)
        {
            var id = key;
            var label = menuList[key];

            var buttonClickEvent = function()
            {
                siteMenu.executeUIAction(id);
            };

            menuBuilder.addMenuButton(label, buttonClickEvent);
        }
    },

    executeUIAction: function(actionId, params)
    {
        main.makeWebCall(main.webApiURL + "executeUIAction/" + actionId, "POST", siteMenu.processUIActionResponse, params);
    },

    processUIActionResponse: function (responseItems)
    {
        var response = responseItems[0]; // Get the first item
            
        responseItems.splice(0, 1); // Remove the first item
        

        var callback = function () { };
        if (responseItems.length > 0)
        {
            callback = (function (items)
            {
                return function ()
                {
                    console.log('response callback of process ui action');
                    siteMenu.processUIActionResponse(items);
                }
            })(responseItems);
        }

        //var x = responseItems
        //var y = responseItems.splice(0, 1);
        //console.log(y);
        //console.log(response);
        var settings = response;//.UIAction;

        var actionType = -1;
        if (settings != null && settings.ActionType != null)
        {
            actionType = settings.ActionType;
        }

        var data = response;

        switch (actionType)
        {
            case -1:
                inputDialog.showMessage(data, callback);
                /// Should not really be here
                break;
            case 0: /// DataView
                var viewData = response.ViewData;
                siteMenu.populateView(viewData, settings);
                callback();
                break;
            case 1: /// User Input
                siteMenu.buildInput(settings);
                callback();
                break;
            case 4: // Cancel Input Dialog
                inputDialog.cancelInput();
                callback();
                break;
            case 5: // Show Message
                inputDialog.showMessage(data, callback);
                break;
            case 6: // Execute action
                //console.log('executing ' + settings.UIActionId);
                siteMenu.executeUIAction(settings.EventNumber, null);
                callback();
                break;
            default:
                inputDialog.showMessage('unknown action type: ' + actionType, callback);
        }
    },

    buildInput: function(settings)
    {
        inputDialog.loadInputPage("InputDialog.html", function ()
        {
            var title = document.getElementById('pageTitle');
            title.innerHTML = settings.Name;

            var inputTable = document.getElementById('inputTable');
            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var row = document.createElement('tr');

                var inputField = settings.InputFields[i];
                switch (inputField.InputType)
                {
                    case 0: /// Text
                    case 1: /// Password
                        //var label = document.createElement('label');
                        //label.innerHTML = inputField.InputLabel;
                        var labelCell = document.createElement('td');
                        labelCell.innerHTML = inputField.InputLabel;
                        row.appendChild(labelCell);

                        var inp = document.createElement('input');
                        if (inputField.InputType == 1)
                        {
                            //inp.setAttribute('type', 'password');
                            //alert('d');
                            inp.type = "password";
                        }
                        else
                        {
                            inp.type = "text";
                        }
                        inp.id = "_" + inputField.InputName;
                        if (inputField.DefaultValue != null && inputField.DefaultValue.length > 0)
                        {
                            inp.value = inputField.DefaultValue;
                        }
                        var inputCell = document.createElement('td');
                        inputCell.appendChild(inp);
                        row.appendChild(inputCell);

                        break;
                    case 2: ///Hidden input
                        inputDialog.addHiddenField("_" + inputField.InputName, inputField.DefaultValue);
                        break;
                    default:
                        inputDialog.showMessage('Unknown input type: ' + inputField.InputType);
                        continue;
                        break;
                }
                inputTable.appendChild(row);
            }

            var buttonRow = document.createElement('tr');
            var buttonCell = document.createElement('td');
            buttonCell.colSpan = 2;
            for (var i = 0; i < settings.InputButtons.length; i++)
            {
                var buttonItem = settings.InputButtons[i];
                var button = document.createElement('button');
                button.style.margin = "10px";
                button.innerHTML = buttonItem.MenuLabel;

                button.onclick = (function (id, actionType, uiAction)
                {
                    return function ()
                    {
                        if (actionType == 4) /// Cancel Dialog
                        {
                            inputDialog.cancelInput();
                        }
                        else
                        {
                            var data = { };


                            for (var j = 0; j < uiAction.InputFields.length; j++)
                            {
                                var inputField = uiAction.InputFields[j];
                                var inputValue = document.getElementById("_" + inputField.InputName).value;
                                
                                data[inputField.InputName] = inputValue;
                            }
                            
                            data = JSON.stringify(data);
                            
                            siteMenu.executeUIAction(id, data);
                        }
                        //inputDialog.cancelInput();
                    }
                })(buttonItem.Id, buttonItem.ActionType, settings);

                buttonCell.appendChild(button);
            }
            buttonCell.style.textAlign = "center";
            buttonCell.style.verticalAlign = "middle";
            inputTable.appendChild(buttonCell);
        });
    },

    populateView: function(data, settings, callback)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            var table = views.getTable();
            var row = document.createElement("tr");

            for (var j = 0; j < settings.Columns.length; j++)
            {
                var headCell = document.createElement("th");
                headCell.innerHTML = settings.Columns[j].ColumnLabel;
                row.appendChild(headCell);
            }
            table.appendChild(row);
            
            for (var i = 0; i < data.length; i++)
            {
                var row = document.createElement("tr");
                
                for (var j = 0; j < settings.Columns.length; j++)
                {
                    var column = settings.Columns[j];
                    var cell = document.createElement("td");

                    var value = "";
                    if (column.ColumnName != null && column.ColumnName.length > 0)
                    {
                        value = data[i][column.ColumnName];
                    }

                    if (column.ColumnType == 1) /// Boolean
                    {
                        if (value == true)
                        {
                            value = column.TrueValueDisplay;
                        }
                        else if (value == false)
                        {
                            value = column.FalseValueDisplay;
                        }
                        cell.innerHTML = value;
                    }
                    else if (column.ColumnType == 2) // Button
                    {
                        var button = document.createElement('button');

                        button.onclick = (function (index, ind)
                        {
                            return function ()
                            {
                                var id = data[index]["Id"];
                                
                                data = JSON.stringify(data[index]);
                                var theColumn = settings.Columns[ind];
                                
                                inputDialog.showMessage(theColumn.UIAction, null, data);
                            }
                        })(i,j);

                        if (column.ButtonTextSource == 0) //Fixed button text
                        {
                            button.innerHTML = column.ButtonText;
                        }
                        else
                        {
                            inputDialog.showMessage("Unhandled ButtonTextSource: " + column.ButtonTextSource);
                            button.innerHTML = "????";
                        }

                        cell.appendChild(button);
                    }
                    else if (column.ColumnType == 3) /// Link
                    {
                        var a = document.createElement('a');
                        a.href = "#";
                        a.innerHTML = column.LinkLabel;
                        a.onclick = (function (col, index)
                        {
                            return function ()
                            {
                                var formData = data[index][col.KeyColumn];
                                
                                var id = col.EventNumber;
                                siteMenu.executeUIAction(id, formData);
                            }
                        })(column, i);
                        cell.appendChild(a);
                    }
                    else
                    {
                        //console.log("Column type = " + column.ColumnType);
                        /// Don't do anything to the value

                        cell.innerHTML = value;
                    }

                    if (column.ColumnSetting != null)
                    {
                        //console.log(column.ColumnSetting);
                        if (column.ColumnSetting.ColumnSettingType == 0) /// Show/Hide column
                        {
                            var show = column.ColumnSetting.Display == 0;
                            var otherColumnValue = data[i][column.ColumnSetting.OtherColumnToCheck].toString();
                            var showHideValue = column.ColumnSetting.OtherColumnValue.toString();
                            //console.log('showHideValue = ' + showHideValue);
                            //console.log("value = " + otherColumnValue);
                            //console.log("Show = " + show);
                            
                            if ((otherColumnValue == showHideValue && show == true) ||
                                 (otherColumnValue != showHideValue && show == false))
                            {
                                var cellValue = cell.innerHTML;
                                var div = document.createElement('div');
                                div.innerHTML = cellValue;
                                
                                var newCell = document.createElement('td');
                                newCell.appendChild(div);
                                div.style.display = 'none';
                                cell = newCell;
                            }
                        }
                    }
                    
                    row.appendChild(cell);
                }
                table.appendChild(row);
            }

            var viewMenu = document.getElementById("viewsMenu");
            menuBuilder.clearNode('viewsMenu');

            for (var i = 0; i < settings.ViewMenu.length; i++)
            {
                var menu = settings.ViewMenu[i];
                var button = document.createElement('button');
                button.innerHTML = menu.MenuLabel;

                button.onclick = (function (id)
                {
                    return function ()
                    {
                        siteMenu.executeUIAction(id);
                    }
                })(menu.Id);

                viewMenu.appendChild(button);
            }
            if (callback)
            {
                callback();
            }
        });
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
            inputDialog.showMessage(data);
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
        alert('delete user');
        var callback = function ()
        {
            inputDialog.showMessage('User successfully deleted');
            siteMenu.getUsers();
        };
        main.makeWebCall(main.menuApiUrl + "deleteUser/" + userId, "DELETE", callback);
    },

    createAddUserButton: function ()
    {
        alert('create add user button');
        var viewMenu = document.getElementById("viewsMenu");
        var button = document.createElement("button");
        button.innerHTML = "Add";
        button.onclick = function ()
        {
            siteMenu.getUserRoles();
        };
        viewMenu.appendChild(button);
    },
};