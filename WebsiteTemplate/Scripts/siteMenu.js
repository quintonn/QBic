var siteMenu = {

    buildMenu: function (menuList)
    {
        menuBuilder.addMenuButton("Home", function ()
        {
            navigation.entryPoint();
        });

        for (var key in menuList)
        {
            var id = key;
            var label = menuList[key];

            var buttonClickEvent = (function (actionId)
            {
                return function ()
                {
                    siteMenu.executeUIAction(actionId);
                }
            })(id);

            menuBuilder.addMenuButton(label, buttonClickEvent);
        }
    },

    processEvent: function (eventId, params, actionId, args, actionData)
    {
        var data =
            {
                Data: params || "",
                ActionId: actionId,
                ActionData: actionData || ""
            };
        data = JSON.stringify(data);
        
        main.makeWebCall(main.webApiURL + "processEvent/" + eventId, "POST", siteMenu.processUIActionResponse, data, args);
    },

    executeUIAction: function(actionId, params, actionData)
    {
        var data =
            {
                ActionData: actionData || "",
                Data: params || ""
            };
        
        data = JSON.stringify(data);
        
        main.makeWebCall(main.webApiURL + "executeUIAction/" + actionId, "POST", siteMenu.processUIActionResponse, data, params);
    },

    processUIActionResponse: function (responseItems, args) /// args is for data passed between calls
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
                    siteMenu.processUIActionResponse(items, args);
                }
            })(responseItems);
        }

        var settings = response;
        
        var actionType = -1;
        if (settings != null && settings.ActionType != null)
        {
            actionType = settings.ActionType;
        }

        var data = response;
        
        switch (actionType)
        {
            case -1:
                inputDialog.showMessage(data, callback, null, settings.ActionData);
                /// Should not really be here
                break;
            case 0: /// DataView
                var viewData = response.ViewData;
                siteMenu.populateView(viewData, settings, null, args);
                callback();
                break;
            case 1: /// User Input
                siteMenu.buildInput(settings, args);
                callback();
                break;
            case 4: // Cancel Input Dialog
                inputDialog.cancelInput();
                callback();
                break;
            case 5: // Show Message
                inputDialog.showMessage(data, callback, null, settings.ActionData);
                break;
            case 6: // Execute action
                siteMenu.executeUIAction(settings.EventNumber, args, settings.ActionData);
                callback();
                break;
            default:
                inputDialog.showMessage('unknown action type: ' + actionType, callback, null, settings.ActionData);
        }
    },

    buildInput: function(settings, args)
    {
        inputDialog.loadInputPage("InputDialog.html", function ()
        {
            var title = document.getElementById('pageTitle');
            title.innerHTML = settings.Description;

            var inputTable = document.getElementById('inputTable');
            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var row = document.createElement('tr');

                var inputField = settings.InputFields[i];
                switch (inputField.InputType)
                {
                    case 0: /// Text
                    case 1: /// Password
                        var labelCell = document.createElement('td');
                        labelCell.innerHTML = inputField.InputLabel;
                        row.appendChild(labelCell);

                        var inp = document.createElement('input');
                        if (inputField.InputType == 1)
                        {
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
                    case 2: /// Hidden input
                        inputDialog.addHiddenField("_" + inputField.InputName, inputField.DefaultValue);
                        break;
                    case 3: /// Combo Box
                        var labelCell = document.createElement('td');
                        labelCell.innerHTML = inputField.InputLabel;
                        row.appendChild(labelCell);

                        var combo = document.createElement('select');
                        combo.id = "_" + inputField.InputName;
                        var array = inputField.ListItems;
                        for (var i = 0; i < array.length; i++) {
                            var option = document.createElement("option");
                            option.value = array[i];
                            option.text = array[i];
                            combo.appendChild(option);
                        }

                        var inputCell = document.createElement('td');
                        inputCell.appendChild(combo);
                        row.appendChild(inputCell);

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
                button.innerHTML = buttonItem.Label;

                button.onclick = (function (id, uiAction)
                {
                    return function ()
                    {
                        var data = {};


                        for (var j = 0; j < uiAction.InputFields.length; j++)
                        {
                            var inputField = uiAction.InputFields[j];
                            var inputValue = document.getElementById("_" + inputField.InputName).value;

                            data[inputField.InputName] = inputValue;
                        }

                        if (args != null && args.length > 0)
                        {
                            data["parentId"] = args;
                        }
                        
                        
                        siteMenu.processEvent(settings.Id, data, id, args, settings.ActionData);
                    }
                })(buttonItem.ActionNumber, settings);

                buttonCell.appendChild(button);
            }
            buttonCell.style.textAlign = "center";
            buttonCell.style.verticalAlign = "middle";
            inputTable.appendChild(buttonCell);
        });
    },

    populateView: function(data, settings, callback, args)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            var table = views.getTable();
            var row = document.createElement("tr");

            var viewTitle = document.getElementById('viewsTitle');
            viewTitle.innerHTML = settings.Description;

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
                        value = data[i];
                        var colName = column.ColumnName;
                        while (colName.indexOf('.') > -1)
                        {
                            var index = colName.indexOf('.');
                            var partName = colName.substring(0, index);
                            
                            value = value[partName];
                            colName = colName.substring(index+1);
                        }
                        value = value[colName];
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
                                
                                var formData = JSON.stringify(data[index]);
                                var theColumn = settings.Columns[ind];

                                if (theColumn.Event.ActionType == 5)
                                {
                                    inputDialog.showMessage(theColumn.Event, null, formData, settings.ActionData);
                                }
                                else if (theColumn.Event.ActionType == 6)
                                {
                                    var eventId = theColumn.Event.EventNumber;
                                    var formData = data[index]["Id"];
                                    
                                    siteMenu.executeUIAction(eventId, formData, settings.ActionData);
                                }
                                else
                                {
                                    inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
                                }
                            }
                        })(i, j);
                        //alert(settings.ActionData);

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
                        //alert(column.LinkLabel);
                        a.innerHTML = column.LinkLabel;
                        a.onclick = (function (col, index)
                        {
                            return function ()
                            {
                                var formData = data[index][col.KeyColumn];
                                
                                var id = col.EventNumber;
                                siteMenu.executeUIAction(id, formData, settings.ActionData);
                            }
                        })(column, i);
                        cell.appendChild(a);
                    }
                    else
                    {
                        /// Don't do anything to the value
                        
                        cell.innerHTML = value;
                    }

                    if (column.ColumnSetting != null)
                    {
                        if (column.ColumnSetting.ColumnSettingType == 0) /// Show/Hide column
                        {
                            var show = column.ColumnSetting.Display == 0;
                            var otherColumnValue = data[i][column.ColumnSetting.OtherColumnToCheck].toString();
                            var showHideValue = column.ColumnSetting.OtherColumnValue.toString();
                            
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
                button.innerHTML = menu.Label;

                button.onclick = (function (id)
                {
                    return function ()
                    {
                        siteMenu.executeUIAction(id, args, settings.ActionData);
                    }
                })(menu.EventNumber);

                viewMenu.appendChild(button);
            }

            var viewFooter = document.getElementById('viewsFooter');
            if (settings.ViewMessage != null && settings.ViewMessage.length > 0)
            {
                viewFooter.innerHTML = settings.ViewMessage;
            }
            else
            {
                viewFooter.innerHTML = "";
            }

            if (callback)
            {
                callback();
            }
        });
    },
};