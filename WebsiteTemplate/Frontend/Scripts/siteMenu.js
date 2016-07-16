var siteMenu = {

    findMenu: function (menuId, menuList)
    {
        for (var key in menuList)
        {
            var m = menuList[key];
            if (m.Id == menuId)
            {
                return m.SubMenus;
            }
            var tmp = siteMenu.findMenu(menuId, m.SubMenus);
            if (tmp != null)
            {
                return tmp;
            }
        }
        return null;
    },

    buildMenu: function (menuList, menuId, parentId)
    {
        menuBuilder.addMenuButton("Home", function ()
        {
            navigation.entryPoint();
        });
        if (menuId != null)
        {
            menuBuilder.addMenuButton("<<", function ()
            {
                menuBuilder.clearNode("menuDiv");
                menuBuilder.clearNode('viewsMenu');
                
                siteMenu.buildMenu(menuList, parentId);
            });
        }
        var theMenu = menuList;
        if (menuId != null)
        {
            theMenu = siteMenu.findMenu(menuId, menuList);
        }
        for (var key in theMenu)
        {
            var id = key;
            var menu = theMenu[key];
            var label = menu.Name;

            var buttonClickEvent = (function (actionId)
            {
                return function ()
                {
                    siteMenu.executeUIAction(actionId);
                }
            })(menu.Event);

            if (menu.Event == null)
            {
                buttonClickEvent = (function (m)
                {
                    return function ()
                    {
                        menuBuilder.clearNode('menuDiv');
                        siteMenu.buildMenu(menuList, m.Id, menuId);
                    }
                })(menu);
            }

            menuBuilder.addMenuButton(label, buttonClickEvent);
        }
    },

    processEvent: function (eventId, params, actionId, args)
    {
        var data =
            {
                Data: params || "",
                ActionId: actionId,
            };
        data = JSON.stringify(data);

        main.makeWebCall(main.webApiURL + "processEvent/" + eventId, "POST", siteMenu.processUIActionResponse, data, args);
    },

    executeUIAction: function (actionId, params, args)
    {
        var data =
            {
                Data: params || ""
            };

        data = JSON.stringify(data);

        main.makeWebCall(main.webApiURL + "executeUIAction/" + actionId, "POST", siteMenu.processUIActionResponse, data, args);
    },

    processUIActionResponse: function (responseItems, args) /// args is for data passed between calls
    {
        if (responseItems == null || responseItems.length == 0)
        {
            /// Eg, when OnRaisePropertyChanged returns nothing -- This shouldn't really happen though
            return;
        }

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
                inputDialog.showMessage(data, callback, null);
                /// Should not really be here
                break;
            case 0: /// DataView
                var viewData = response.ViewData;
                views.populateView(viewData, settings, null, args, true);
                callback();
                break;
            case 1: /// User Input
                inputDialog.buildInput(settings, args);
                callback();
                break;
            case 4: // Cancel Input Dialog
                inputDialog.cancelInput();
                callback();
                break;
            case 5: // Show Message
                inputDialog.showMessage(data, callback, settings.Data, args);
                break;
            case 6: // Execute action
                siteMenu.executeUIAction(settings.EventNumber, settings.ParametersToPass);
                callback();
                break;
            case 7: // InputDataView  -> View on input screen
                
                var viewId = "_" + args.InputName;
                var mainViewDiv = document.getElementById(viewId);
                var menuDiv = document.createElement('div');

                var table = document.createElement('table');
                table.className = 'inputViewTable';

                var data = settings.ViewData;

                views.populateViewMenu(menuDiv, settings, args, true);
                views.populateViewWithData(table, data, settings, args);

                mainViewDiv.appendChild(menuDiv);
                mainViewDiv.appendChild(table);
                
                callback();
                break;
            case 8:   /// Update Input view
                var viewId = "_" + args.InputName;
                var div = document.getElementById(viewId);
                var table = div.getElementsByTagName('table')[0];

                var rowId = settings.RowId;
                if (rowId == null)
                {
                    rowId = data['rowId'];
                }
                
                var updateType = settings.UpdateType;

                if (updateType == 0) /// Add or modify
                {
                    var json = settings.JsonDataToUpdate;
                    var data = JSON.parse(json);

                    var viewSettings = args.ViewForInput;

                    var row;

                    var isNew = rowId == -1;
                    
                    if (isNew)  /// New item
                    {
                        row = table.insertRow(table.rows.length);
                        rowId = table.rows.length - 2;
                    }
                    else /// Update row
                    {
                        var tableRowId = views.deleteRowFromTable(table, rowId, true);
                        rowId = Math.max(tableRowId, 0);
                        //row = table.insertRow(rowId + 1);
                        row = table.rows[tableRowId];
                    }
                    views.populateRow(row, viewSettings, data, rowId, args, isNew);

                    callback();
                }
                else if (updateType == 1) /// Delete row
                {
                    views.deleteRowFromTable(table, rowId, false);
                }
                else
                {
                    inputDialog.showMessage('unknown update type: ' + updateType, callback, null);
                }
                break;
            //case 9:  /// Delete input view item
            //    var viewId = "_" + args.InputName;
            //    var rowId = settings.RowId;
            //    var div = document.getElementById(viewId);
                
            //    var table = div.getElementsByTagName('table')[0];
            //    console.log('deleting row ' + rowId + ' from table:');
            //    console.log(table);
            //    views.deleteRowFromTable(table, rowId, false);
            //    break;
            case 10: /// Update Data Source Combo Box data source
                var comboId = "_" + settings.InputName;
                var combo = document.getElementById(comboId);
                var selectedItem = combo.value;
                
                while (combo.options.length > 0)
                {
                    combo.remove(0);
                }
                var selectedIndex = -1;
                for (var j = 0; j < settings.ListItems.length; j++)
                {
                    var option = document.createElement("option");
                    option.value = settings.ListItems[j].Key;
                    option.text = settings.ListItems[j].Value;
                    if (option.value == selectedItem)
                    {
                        selectedIndex = j;
                    }
                    combo.appendChild(option);
                    if (selectedIndex > -1)
                    {
                        combo.selectedIndex = selectedIndex;
                    }
                }
                callback();
                break;
            case 11: /// ViewFile
                {
                    var callback = function (data, args, contentType)
                    {
                        var dataURL = "data:" + contentType + ";base64," + data;
                        var url = dataURL;
                        window.open(url, "_blank");
                    };
                    
                    main.makeWebCall(main.webApiURL + settings.DataUrl, "POST", callback, settings.RequestData);
                    break;
                }
            case 12: /// UpdateInput
                var inputId = "_" + settings.InputName;
                var input = document.getElementById(inputId);
                input.value = settings.InputValue;
                break;
            default:
                inputDialog.showMessage('unknown action type: ' + actionType, callback, null);
        }
    },

    conditionIsMet: function(condition, value)
    {
        if (condition.Comparison == 0)  // Equals
        {
            return value === condition.ColumnValue;
        }
        else if (condition.Comparison == 1) // Not-Equals
        {
            return value != condition.ColumnValue;
        }
        else if (condition.Comparison == 2) // Contains
        {
            return value.indexOf(condition.ColumnValue) > -1;
        }
        else if (condition.Comparison == 3) // IsNotNull
        {
            return value != null && (value + "").length > 0;
        }
        else if (condition.Comparison == 4) // IsNull
        {
            return value == null || (value + "").length == 0;
        }
        else
        {
            alert("Unknown comparison " + condition.Comparison);
            return false;
        }
    },
};