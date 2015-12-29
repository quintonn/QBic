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

    executeUIAction: function (actionId, params)
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
                inputDialog.showMessage(data, callback, settings.Data);
                break;
            case 6: // Execute action
                siteMenu.executeUIAction(settings.EventNumber, settings.ParametersToPass);
                callback();
                break;
            default:
                inputDialog.showMessage('unknown action type: ' + actionType, callback, null);
        }
    },

    buildInput: function (settings, args)
    {
        inputDialog.loadInputPage("InputDialog.html", function ()
        {
            //var header = document.getElementById('dlgInput' + inputDialog.inputCount).getElementsByTagName('h1')[0];
            //header.id = 'pageTitle' + inputDialog.inputCount;

            //var title = document.getElementById('pageTitle');
            //var title = header;
            var title = document.getElementById('dlgInput' + inputDialog.inputCount).getElementsByTagName('h1')[0];
            title.innerHTML = settings.Description;

            var pageDiv = document.createElement('div');
            pageDiv.style.width = '100%';
            pageDiv.style.height = '100%';

            var name = 'dlgInput' + inputDialog.inputCount;
            
            document.getElementById('dlgInput' + inputDialog.inputCount).appendChild(pageDiv);

            var tabButtonRowDiv = document.createElement('div');
            tabButtonRowDiv.style.width = '100%';
            tabButtonRowDiv.style.margin = '10px';
            pageDiv.appendChild(tabButtonRowDiv);

            /// Get tab names
            var emptyTabNamesPresent = false;
            var nonEmptyTabNamesPresent = false;
            var tabNames = [];
            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var inputField = settings.InputFields[i];
                if (inputField.InputType == 2)  // Hidden input
                {
                    continue;
                }
                //inputField.TabName = inputField.TabName || "X"; // userfull for testing
                if (inputField.TabName != null && inputField.TabName.length > 0)
                {
                    if (tabNames.indexOf(inputField.TabName) == -1)
                    {
                        nonEmptyTabNamesPresent = true;
                        tabNames.push(inputField.TabName);
                    }
                }
                else
                {
                    emptyTabNamesPresent = true;
                }
            }
            if (nonEmptyTabNamesPresent && emptyTabNamesPresent)
            {
                alert('Error: If tab names are used, all input fields should have tab names');
            }
            if (tabNames.length == 0)
            {
                //tabNames.push("");
            }
            
            if (tabNames.length > 1)
            {
                for (var tabCount = 0; tabCount < tabNames.length; tabCount++)
                {
                    var tabDiv = document.createElement('div');
                    tabDiv.id = 'TabDiv' + tabCount;
                    tabDiv.setAttribute('tabName', tabNames[tabCount]);

                    var tabButton = document.createElement('button');
                    tabButton.style.marginRight = "10px";
                    tabButton.id = 'TabButton' + tabCount;
                    tabButton.innerHTML = tabNames[tabCount];

                    tabButton.onclick = (function (tabIndex)
                    {
                        return function ()
                        {
                            for (var j = 0; j < tabNames.length; j++)
                            {
                                var _tabDiv = document.getElementById('TabDiv' + j);
                                var _tabButton = document.getElementById('TabButton' + j);

                                _tabDiv.style.display = j == tabIndex ? 'block' : 'none';
                                _tabButton.disabled = j == tabIndex;
                            }
                        };
                    })(tabCount);

                    if (tabCount == 0)
                    {
                        tabButton.disabled = true;
                    }

                    tabButtonRowDiv.appendChild(tabButton);

                    if (tabCount > 0)
                    {
                        tabDiv.style.display = 'none';
                    }

                    pageDiv.appendChild(tabDiv);
                }
            }

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
                    if (item.Condition.Comparison == 0)  // Equals
                    {
                        showInput = value === item.Condition.ColumnValue;
                    }
                    else if (item.Condition.Comparison == 1) // Not-Equals
                    {
                        showInput = value != item.Condition.ColumnValue;
                    }
                    else if (item.Condition.Comparison == 2) // Contains
                    {
                        showInput = value.indexOf(item.Condition.ColumnValue) > -1;
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

                    if (showInput == true)
                    {
                        break;
                    }
                    //input.style.visibility = showInput ? "visible" : "hidden";
                }
            };

            var addInputsToNode = function (node, tabName)
            {
                for (var i = 0; i < settings.InputFields.length; i++)
                {
                    var inputField = settings.InputFields[i];
                    if (inputField.TabName != null && inputField.TabName.length > 0)
                    {
                        if (inputField.TabName != tabName && tabNames.length > 1)
                        {
                            continue;
                        }
                    }
                    var row = document.createElement('tr');
                    //conditionList.push(i);

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
                            else if (inputField.MultiLineText != null && inputField.MultiLineText == true)
                            {
                                inp = document.createElement('textarea');
                                inp.rows = 5;
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

                            inp.onclick = function ()
                            {
                                this.select();
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

                            for (var j = 0; j < array.length; j++)
                            {
                                var option = document.createElement("option");
                                option.value = array[j].Key;
                                option.text = array[j].Value;
                                combo.appendChild(option);
                            }

                            if (inputField.DefaultValue != null && inputField.DefaultValue.length > 0)
                            {
                                combo.value = inputField.DefaultValue;
                            }
                            else
                            {
                                combo.value = "";
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

                            var inputCell = document.createElement('td');
                            inputCell.appendChild(inp);
                            row.appendChild(inputCell);
                            break;
                        case 5: /// List selection input

                            var labelCell = document.createElement('td');

                            labelCell.innerHTML = inputField.InputLabel;
                            row.appendChild(labelCell);
                            inputTable.appendChild(row);

                            row = document.createElement('tr');
                            var table = document.createElement('table');
                            table.style.fontSize = "75%";
                            table.frame = "box";
                            table.cellPadding = "0";
                            table.style.padding = "0";

                            var row1 = document.createElement('tr');
                            var row2 = document.createElement('tr');
                            var row3 = document.createElement('tr');
                            var row4 = document.createElement('tr');

                            var label1 = document.createElement('td');
                            label1.innerHTML = inputField.SelectedItemsLabel;
                            var spacer = document.createElement("td");
                            var label2 = document.createElement("td");
                            label2.innerHTML = inputField.AvailableItemsLabel;

                            row1.appendChild(label1);
                            row1.appendChild(spacer);
                            row1.appendChild(label2);

                            var select1 = document.createElement('select');
                            select1.multiple = true;
                            select1.id = "_" + inputField.InputName + "_1";
                            select1.size = 5;
                            select1.style.width = "100%";

                            var select2 = document.createElement('select');
                            select2.multiple = true;
                            select2.id = "_" + inputField.InputName + "_2";
                            select2.size = 5;
                            select2.style.width = "100%";

                            for (var p = 0; p < inputField.ListSource.length; p++)
                            {
                                var item = inputField.ListSource[p];
                                var option = document.createElement('option');
                                option.value = item.Key;
                                option.text = item.Value;

                                var isDefault = false;
                                var defaultValues = inputField.DefaultValue || [];
                                for (var q = 0; q < defaultValues.length; q++)
                                {
                                    var defaultItem = defaultValues[q];
                                    if (defaultItem == item.Key)
                                    {
                                        isDefault = true;
                                        break;
                                    }
                                }

                                if (isDefault)
                                {
                                    select1.appendChild(option);
                                }
                                else
                                {
                                    select2.appendChild(option);
                                }
                            }

                            var container1 = document.createElement('td');
                            container1.rowSpan = 3;
                            container1.appendChild(select1);
                            row2.appendChild(container1);

                            var buttonContainer1 = document.createElement('td');
                            buttonContainer1.rowSpan = 3;

                            var button1 = document.createElement("button");
                            button1.innerHTML = "<<";
                            button1.onclick = (function (inputName)
                            {
                                return function ()
                                {
                                    var select1Name = "_" + inputName + "_1";
                                    var select2Name = "_" + inputName + "_2";
                                    var select1 = document.getElementById(select1Name);
                                    var select2 = document.getElementById(select2Name);

                                    for (var k = select2.options.length - 1; k >= 0; k--)
                                    {
                                        if (select2.options[k].selected)
                                        {
                                            select1.appendChild(select2.options[k]);
                                        }
                                    }
                                }
                            })(inputField.InputName);
                            buttonContainer1.appendChild(button1);

                            var br = document.createElement('br');
                            buttonContainer1.appendChild(br);

                            var button2 = document.createElement('button');
                            button2.innerHTML = ">>";
                            button2.onclick = (function (inputName)
                            {
                                return function ()
                                {
                                    var select1Name = "_" + inputName + "_1";
                                    var select2Name = "_" + inputName + "_2";
                                    var select1 = document.getElementById(select1Name);
                                    var select2 = document.getElementById(select2Name);

                                    for (var k = select1.options.length - 1; k >= 0; k--)
                                    {
                                        if (select1.options[k].selected)
                                        {
                                            select2.appendChild(select1.options[k]);
                                        }
                                    }
                                }
                            })(inputField.InputName);
                            buttonContainer1.appendChild(button2);

                            row2.appendChild(buttonContainer1);

                            var container2 = document.createElement('td');
                            container2.rowSpan = 3;
                            container2.appendChild(select2);
                            row2.appendChild(container2);

                            table.appendChild(row1);
                            table.appendChild(row2);
                            table.appendChild(row3);
                            table.appendChild(row4);

                            var tableCell = document.createElement('td');
                            tableCell.colSpan = 3;
                            tableCell.appendChild(table);
                            row.appendChild(tableCell);

                            break;
                        case 6: /// Date
                            var labelCell = document.createElement('td');
                            labelCell.innerHTML = inputField.InputLabel;
                            row.appendChild(labelCell);

                            var inp = document.createElement('input');

                            var id = "_" + inputField.InputName;
                            inp.type = "text";

                            inp.id = id;

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

                            if (Modernizr.inputtypes.date == true)
                            {
                                inp.type = "date";
                            }
                            else
                            {
                                MaskedInput({
                                    elm: inp,
                                    format: 'YYYY/MM/DD',  //   MM/DD/YYYY
                                    separator: '\/',
                                    typeon: 'MDY',
                                    allowedfx: (function (inputName)
                                    {
                                        return function (ch, idx)
                                        {
                                            var str = document.getElementById('_' + inputName).value;
                                            switch (idx)
                                            {
                                                case 6: // First month character
                                                    return ('01'.indexOf(ch) > -1);
                                                case 7:
                                                    if (str[5] === '1')
                                                    {
                                                        // Ensure month does not exceed 12
                                                        return ('012'.indexOf(ch) > -1);
                                                    }
                                                    break;
                                                case 9:
                                                    return ('0123'.indexOf(ch) > -1);
                                                case 10:
                                                    if (str[8] === '3')
                                                    {
                                                        // Ensure day does not exceed 31
                                                        return ('01'.indexOf(ch) > -1);
                                                    }
                                                    break;
                                                case 7:
                                                    return ('12'.indexOf(ch) > -1);
                                            }
                                            return true;
                                        }
                                    })(inputField.InputName)
                                });
                                inp.onclick = function ()
                                {
                                    this.select();
                                };
                            }

                            var inputCell = document.createElement('td');
                            inputCell.appendChild(inp);
                            row.appendChild(inputCell);

                            break;
                        case 7: /// Masked Input
                            var labelCell = document.createElement('td');
                            labelCell.innerHTML = inputField.InputLabel;
                            row.appendChild(labelCell);

                            var inp = document.createElement('input');

                            var id = "_" + inputField.InputName;
                            inp.type = "text";

                            inp.id = id;

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

                            var allowedText = "";
                            for (var charCode = 0; charCode <= 10175; charCode++)
                            {
                                allowedText += String.fromCharCode(charCode);
                            }

                            var validSeparators = '\/- ';

                            var typeOnCharacters = inputField.InputMask;
                            for (var charX = 0; charX < validSeparators.length; charX++)
                            {
                                var re = new RegExp(validSeparators[charX], 'g');
                                typeOnCharacters = typeOnCharacters.replace(re, '');
                            }

                            MaskedInput({
                                elm: inp,
                                format: inputField.InputMask,
                                separator: validSeparators,
                                allowed: allowedText,
                                typeon: typeOnCharacters,
                                allowedfx: (function (iValue)
                                {
                                    return function (ch, idx)
                                    {
                                        var inputField = settings.InputFields[iValue];
                                        var str = document.getElementById('_' + inputField.InputName).value;

                                        var maskValue = inputField.InputMask[idx - 1];

                                        if (maskValue == "n") // Number
                                        {
                                            return '0123456789'.indexOf(ch) > -1;
                                        }
                                        else if (maskValue == "_") // Alpha-numeric
                                        {
                                            return true;
                                        }
                                        else if (validSeparators.indexOf(ch))
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                })(i)
                            });

                            inp.onclick = function ()
                            {
                                this.select();
                            };

                            var inputCell = document.createElement('td');
                            inputCell.appendChild(inp);
                            row.appendChild(inputCell);

                            break;
                        default:
                            inputDialog.showMessage('Unknown input type: ' + inputField.InputType);
                            continue;
                            break;
                    }
                    node.appendChild(row);
                }
            };

            if (tabNames.length > 1)
            {
                for (var tabCount = 0; tabCount < tabNames.length; tabCount++)
                {
                    var tabDiv = document.getElementById('TabDiv' + tabCount);
                    var tabName = tabDiv.getAttribute('tabName', tabName);

                    var inputTable = document.createElement('table');
                    inputTable.className = 'inputTable';

                    addInputsToNode(inputTable, tabName);

                    tabDiv.appendChild(inputTable);
                }
            }
            else
            {
                var inputTable = document.createElement('table');
                inputTable.className = 'inputTable';

                var tabName = "";
                if (tabNames.length > 0)
                {
                    tabName = tabNames[0];
                }

                addInputsToNode(inputTable, tabName);
                
                pageDiv.appendChild(inputTable);
            }

            var buttonRowDiv = document.createElement('div');
            for (var i = 0; i < settings.InputButtons.length; i++)
            {
                var buttonItem = settings.InputButtons[i];
                var button = document.createElement('button');
                button.style.margin = "10px";
                button.innerHTML = buttonItem.Label;

                button.onclick = (function (id, uiAction, btnIndex)
                {
                    return function ()
                    {
                        var data = {};

                        for (var j = 0; j < uiAction.InputFields.length; j++)
                        {
                            var inputField = uiAction.InputFields[j];
                            var theInput;
                            var inputValue;
                            if (inputField.InputType == 5) // List Selection
                            {
                                inputValue = [];
                                theInput = document.getElementById("_" + inputField.InputName + "_1");
                                var options = theInput.options;

                                for (var k = 0; k < options.length; k++)
                                {
                                    inputValue.push(options[k].value);
                                }
                            }
                            else
                            {
                                theInput = document.getElementById("_" + inputField.InputName);

                                if (theInput == null)
                                {
                                    continue;
                                }
                                inputValue = theInput.value;
                                if (theInput.type == "checkbox")
                                {
                                    inputValue = theInput.checked;
                                }
                            }
                            inputValue = inputValue || "";

                            var buttonItem = settings.InputButtons[btnIndex];
                            
                            if (buttonItem.ValidateInput == true)
                            {
                                if (inputValue.length == 0 && inputField.Mandatory == true)
                                {
                                    inputDialog.showMessage(inputField.InputName + ' is mandatory');
                                    return;
                                }
                            }

                            data[inputField.InputName] = inputValue;
                        }

                        siteMenu.processEvent(settings.Id, data, id, args);
                    }
                })(buttonItem.ActionNumber, settings, i);

                buttonRowDiv.appendChild(button);
            }
            buttonRowDiv.style.textAlign = "center";
            buttonRowDiv.style.verticalAlign = "middle";

            document.getElementById('dlgInput' + inputDialog.inputCount).appendChild(buttonRowDiv);

            for (var i = 0; i < settings.InputFields.length; i++)
            {
                var inputField = settings.InputFields[i];
                var name = "_" + inputField.InputName;
                var inputItem = document.getElementById(name);
                if (i == 0)
                {
                    setTimeout(function ()
                    {
                        document.getElementById('_' + settings.InputFields[0].InputName).focus();
                        document.getElementById('_' + settings.InputFields[0].InputName).select();
                    }, 10);
                }
                if (inputItem == null)
                {
                    continue;
                }
                onChangeFunc(inputItem, inputField.InputName);
            }
            //document.getElementById('dlgInput').appendChild(pageDiv);
        });
    },

    populateView: function (data, settings, callback, args)
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
                            colName = colName.substring(index + 1);
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
                        a.innerHTML = column.LinkLabel;
                        a.onclick = (function (col, index)
                        {
                            return function ()
                            {
                                var formData =
                                    {
                                        Id: data[index][col.KeyColumn]
                                    };

                                var id = col.EventNumber;
                                siteMenu.executeUIAction(id, formData);
                            }
                        })(column, i);
                        cell.appendChild(a);
                    }
                    else
                    {
                        /// Replace new line characters with HTML breaks
                        value = value || "";
                        value = value.toString();
                        value = value.replace(/\r/g, ',');
                        value = value.replace(/\n/g, ',');

                        /// Don't do anything to the value
                        cell.innerHTML = value;
                    }

                    if (column.ColumnSetting != null)
                    {
                        if (column.ColumnSetting.ColumnSettingType == 0) /// Show/Hide column
                        {
                            var show = column.ColumnSetting.Display == 0;
                            var compareResult = true;

                            for (var p = 0; p < column.ColumnSetting.Conditions.length; p++)
                            {
                                var condition = column.ColumnSetting.Conditions[p];
                                var colName = condition.ColumnName;
                                var comparison = condition.Comparison;
                                var colVal = condition.ColumnValue;

                                var actualValue = data[i][colName] || "";
                                actualValue = actualValue.toString();

                                if (comparison == 0)
                                {
                                    compareResult = compareResult && actualValue == colVal;
                                }
                                else if (comparison == 1)
                                {
                                    compareResult = compareResult && actualValue != colVal;
                                }
                                else
                                {
                                    alert("Unknown comparison: " + comparison);
                                }
                            }

                            if ((compareResult == false && show == true) || (compareResult == true && show == false))
                            {

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