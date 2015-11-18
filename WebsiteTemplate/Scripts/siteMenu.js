﻿var siteMenu = {

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

    executeUIAction: function(actionId, params)
    {
        var data =
            {
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
                inputDialog.showMessage(data, callback, null);
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
                inputDialog.showMessage(data, callback, null);
                break;
            case 6: // Execute action
                siteMenu.executeUIAction(settings.EventNumber, settings.ParametersToPass);
                callback();
                break;
            default:
                inputDialog.showMessage('unknown action type: ' + actionType, callback, null);
        }
    },

    buildInput: function(settings, args)
    {
        inputDialog.loadInputPage("InputDialog.html", function ()
        {
            var title = document.getElementById('pageTitle');
            title.innerHTML = settings.Description;
            var inputTable = document.getElementById('inputTable');

            var conditionList = [];

            var conditionListContains = function (inputName)
            {
                for (var i = 0; i < conditionList.length; i++)
                {
                    var item = conditionList[i];
                    if (item.TriggerInputName == inputName)
                    {
                        return true;
                    }
                }
                return false;
            };

            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var inputField = settings.InputFields[i];
                if (inputField.VisibilityConditions != null && inputField.VisibilityConditions.length > 0)
                {
                    var inputName = inputField.InputName;
                    
                    for (var j = 0; j < inputField.VisibilityConditions.length; j++)
                    {
                        var condition = inputField.VisibilityConditions[j];
                        //var inputName = condition.ColumnName;
                        var conditionItem =
                        {
                            InputName: inputName,
                            Condition: condition,
                            TriggerInputName: condition.ColumnName
                        };
                        conditionList.push(conditionItem);
                    }
                }
            }

            var onChangeFunc = function (input, inputName)
            {
                var value = input.value;
                if (input.type == "checkbox")
                {
                    value = input.checked;
                }
                value = value + "";
                for (var i = 0; i < conditionList.length; i++)
                {
                    var item = conditionList[i];
                    if (item.TriggerInputName != inputName)
                    {
                        continue;
                    }
                    var inputFieldToUpdate = "_" + item.InputName;
                    var showInput = true;
                    if (item.Condition.Comparison == 0)
                    {
                        showInput = value === item.Condition.ColumnValue;
                    }
                    else if (item.Condition.Comparison == 1)
                    {
                        showInput = value != item.Condition.ColumnValue;
                    }
                    else
                    {
                        alert("Unknown comparison " + item.Condition.Comparison);
                    }
                    var input = document.getElementById(inputFieldToUpdate);
                    
                    while (input.tagName != "TR")
                    {
                        input = input.parentNode;
                    }
                    
                    input.style.display = showInput ? "" : "none";
                    //input.style.visibility = showInput ? "visible" : "hidden";
                }
            };

            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var row = document.createElement('tr');
                conditionList.push(i);
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

                        if (conditionListContains(inputField.InputName))
                        {
                            inp.onchange = (function (inputName)
                            {
                                return function ()
                                {
                                    onChangeFunc(this, inputName);
                                }
                            })(inputField.InputName);
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
                        if (conditionListContains(inputField.InputName))
                        {
                            combo.onchange = (function (inputName)
                            {
                                return function ()
                                {
                                    onChangeFunc(this, inputName);
                                }
                            })(inputField.InputName);
                        }
                        combo.id = "_" + inputField.InputName;
                        var array = inputField.ListItems;
                        for (var j = 0; j < array.length; j++) {
                            var option = document.createElement("option");
                            option.value = array[j];
                            option.text = array[j];
                            combo.appendChild(option);
                        }

                        var inputCell = document.createElement('td');
                        inputCell.appendChild(combo);
                        row.appendChild(inputCell);

                        break;
                    case 4: /// Boolean Input
                        var labelCell = document.createElement('td');
                        labelCell.innerHTML = inputField.InputLabel;
                        row.appendChild(labelCell);

                        var inp = document.createElement('input');
                        if (conditionListContains(inputField.InputName))
                        {
                            inp.onchange = (function (inputName)
                            {
                                return function ()
                                {
                                    onChangeFunc(this, inputName);
                                }
                            })(inputField.InputName);
                        }
                        inp.type = "checkbox";
                        
                        inp.id = "_" + inputField.InputName;
                        if (inputField.DefaultValue != null)
                        {
                            inp.checked = inputField.DefaultValue;
                        }

                        //inp.onchange = function ()
                        //{
                        //    alert(conditionList.length);
                        //};
                        
                        var inputCell = document.createElement('td');
                        inputCell.appendChild(inp);
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
                            var theInput = document.getElementById("_" + inputField.InputName);
                            var inputValue = theInput.value;
                            if (theInput.type == "checkbox")
                            {
                                inputValue = theInput.checked;
                            }
                            console.log(theInput);

                            data[inputField.InputName] = inputValue;
                        }

                        if (args != null && args.length > 0)
                        {
                            //data["parentId"] = args;
                        }
                        
                        siteMenu.processEvent(settings.Id, data, id, args);
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
                                    inputDialog.showMessage(theColumn.Event, null, formData);
                                }
                                else if (theColumn.Event.ActionType == 6)
                                {
                                    var eventId = theColumn.Event.EventNumber;
                                    var formData = data[index]["Id"];
                                    
                                    siteMenu.executeUIAction(eventId, formData);
                                }
                                else
                                {
                                    inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
                                }
                            }
                        })(i, j);

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
                                siteMenu.executeUIAction(id, formData);
                            }
                        })(column, i);
                        cell.appendChild(a);
                    }
                    else
                    {
                        /// Don't do anything to the value
                        
                        cell.innerHTML = value;
                    }

                    if (column.ColumnSetting != null) {
                        if (column.ColumnSetting.ColumnSettingType == 0) /// Show/Hide column
                        {
                            var show = column.ColumnSetting.Display == 0;
                            var compareResult = true;
                            
                            for (var p = 0; p < column.ColumnSetting.Conditions.length; p++) {
                                var condition = column.ColumnSetting.Conditions[p];
                                var colName = condition.ColumnName;
                                var comparison = condition.Comparison;
                                var colVal = condition.ColumnValue;

                                var actualValue = data[i][colName] || "";
                                actualValue = actualValue.toString();
                                
                                if (comparison == 0) {
                                    compareResult = compareResult && actualValue == colVal;
                                }
                                else if (comparison == 1) {
                                    compareResult = compareResult && actualValue != colVal;
                                }
                                else {
                                    alert("Unknown comparison: " + comparison);
                                }
                            }

                            if ((compareResult == false && show == true) || (compareResult == true && show == false)) {

                                var show = column.ColumnSetting.Display == 0;

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

                button.onclick = (function (id, index)
                {
                    return function ()
                    {
                        var vm = settings.ViewMenu[index];
                        siteMenu.executeUIAction(id, vm.ParametersToPass);
                    }
                })(menu.EventNumber, i);

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