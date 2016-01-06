var inputDialog = {

    inputCount: -0,

    loadInputPage: function (pageName, callback)
    {
        inputDialog.inputCount++;
        document.getElementById('inputContent').style.display = 'inline';

        callback = callback || function () { };
        var newCallback = function ()
        {
            callback();
        };

        /// disable everything on the page:
        var elems = document.body.getElementsByTagName('*');
        
        for (var i = 0; i < elems.length; i++)
        {
            elems[i].disabled = true;
        }
        document.getElementById('inputContent').disabled = false;  // For IE
        
        var container = document.createElement('div');
        container.className = 'dlgInputContainer';
        container.id = 'dlgContainer' + inputDialog.inputCount;
        container.style.display = 'inline';
        var dlgInput = document.createElement('div');
        dlgInput.id = 'dlgInput' + inputDialog.inputCount;
        dlgInput.className = 'dlgInput';
        
        container.appendChild(dlgInput);
        document.getElementById('inputContent').appendChild(container);

        navigation.loadHtmlBody('dlgInput' + inputDialog.inputCount, pageName, newCallback);
    },

    cancelInput: function()
    {
        var dlgInput = document.getElementById('dlgContainer' + inputDialog.inputCount);
        if (dlgInput == null)
        {
            inputDialog.cancelMessage();  // TODO: cancel message and cancelInput should be the same code. this is too confusing
            return;
        }
        
        dlgInput.parentNode.removeChild(dlgInput);

        if (inputDialog.inputCount == 1)
        {
            document.getElementById('inputContent').style.display = 'none';
            /// enable everything on the page:
            var elems = document.body.getElementsByTagName('*');
            
            for (var i = 0; i < elems.length; i++)
            {
                
                elems[i].disabled = false;
            }
        }
        else
        {
            /// enable all elements on the next dlgContainer
            var container = document.getElementById('dlgContainer' + (inputDialog.inputCount - 1));
            var elems = container.getElementsByTagName('*');
            
            for (var i = 0; i < elems.length; i++)
            {
                elems[i].disabled = false;
            }
        }

        inputDialog.inputCount--;
    },

    addHiddenField: function (fieldName, fieldValue)
    {
        var inputDiv = document.getElementById("dlgInput" + inputDialog.inputCount);
        var hidden = document.createElement("input");
        hidden.type = "hidden";
        hidden.value = fieldValue;
        hidden.id = fieldName;
        inputDiv.appendChild(hidden);
    },

    getHiddenField: function(fieldName)
    {
        var input = document.getElementById(fieldName);
        if (input == null)
        {
            return "";
        }
        return input.value;
    },

    cancelMessage: function ()
    {
        document.getElementById('messageContent').style.display = 'none';
        menuBuilder.clearNode('dlgMessage');
        //var input = document.getElementById(inputDialog.inputCount)
    },

    showMessage: function (settings, callback, args)
    {
        /// TODO: Need to store the current state of the input dialog 
        //        For eg, what I need to show a message box while on an input page
        menuBuilder.clearNode('dlgMessage');
        
        var message = settings.ConfirmationMessage;
        
        if (message == null)
        {
            message = settings;
        }
        
        message = message || "";
        message = message.toString().replace(/\n/g, "<br/>");
        
        document.getElementById('messageContent').style.display = 'inline';
        
        navigation.loadHtmlBody('dlgMessage', "MessageDialog.html", function ()
        {
            var title = document.getElementById('msgPageTitle');
            title.innerHTML = "Message";
            var inputTable = document.getElementById('msgTable');

            var row1 = document.createElement('tr');
            var cell1 = document.createElement('td');
            cell1.colSpan = 2;
            cell1.innerHTML = message;
            cell1.style.textAlign = "center";
            row1.appendChild(cell1);
            inputTable.appendChild(row1);

            var buttonRow = document.createElement('tr');

            var buttonAdded = false;
            
            /// Add the Confirmation button
            if (settings.ConfirmationButtonText != null && settings.ConfirmationButtonText.length > 0)
            {
                var okButtonEvent = function ()
                {
                    if (settings.OnConfirmationUIAction > 0)
                    {
                        siteMenu.executeUIAction(settings.OnConfirmationUIAction, args);
                    }
                    if (callback != null)
                    {
                        callback();
                    }
                };
                var buttonCell = inputDialog.createInputDialogButton(settings.ConfirmationButtonText, okButtonEvent);
                buttonRow.appendChild(buttonCell);
                
                buttonAdded = true;
            }


            /// Add the Cancel button
            if (settings.CancelButtonText != null && settings.CancelButtonText.length > 0)
            {
                var cancelButtonEvent = function ()
                {
                    if (settings.OnCancelUIAction > 0)
                    {
                        siteMenu.executeUIAction(settings.OnCancelUIAction, args);
                    }
                    if (callback != null && buttonAdded == false)
                    {
                        callback();
                    }
                };

                var buttonCell = inputDialog.createInputDialogButton(settings.CancelButtonText, cancelButtonEvent);
                buttonRow.appendChild(buttonCell);
                buttonAdded = true;
            }

            if (buttonAdded == false)
            {
                var buttonEvent = function()
                {
                    document.getElementById('messageContent').style.display = 'none';
                    if (callback != null)
                    {
                        callback();
                    }
                }
                var buttonCell = inputDialog.createInputDialogButton("Ok", buttonEvent);
                buttonRow.appendChild(buttonCell);
            }
            
            inputTable.appendChild(buttonRow);

            var firstButton = null;
            var firstButtonCell = buttonRow.children[0];
            if (firstButtonCell != null)
            {
                firstButton = firstButtonCell.children[0];
            }
            if (firstButton != null)
            {
                firstButton.focus();
            }
        });
    },

    createInputDialogButton: function(buttonText, callback)
    {
        var buttonCell = document.createElement('td');
        //buttonCell.colSpan = 2;
        buttonCell.style.textAlign = "center";

        var okButton = document.createElement('button');
        okButton.innerHTML = buttonText;
        okButton.onclick = function ()
        {
            inputDialog.cancelMessage();
            
            if (callback)
            {
                callback();
            }
        };
        buttonCell.appendChild(okButton);
        return buttonCell;
    },

    tabButtonClick: function (tabIndex, tabNames)
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
        }
    },

    conditionListContains: function (list, inputName)
    {
        return function()
        {
            for (var i = 0; i < list.length; i++)
            {
                var item = list[i];
                if (item.TriggerInputName == inputName)
                {
                    return true;
                }
            }
            return false;
        }
    },

    inputOnChangeFunc : function (inputName, conditionList)
    {
        return function()
        {
            var input = this;
            
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
                
                var showInput = siteMenu.conditionIsMet(item.Condition, value);
                
                var input = document.getElementById(inputFieldToUpdate);

                while (input.tagName != "TR")
                {
                    input = input.parentNode;
                }

                input.style.display = (showInput ? "" : "none");

                if (showInput == true)
                {
                    break;
                }
            }
        }
    },

    buildInput: function (settings, args)
    {
        inputDialog.loadInputPage("InputDialog.html", function ()
        {
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
                //alert('Error: If tab names are used, all input fields should have tab names');
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

                    tabButton.onclick = (inputDialog.tabButtonClick)(tabCount, tabNames);
                   
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

            var addInputsToNode = function (node, tabName, usingTabs)
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
                    else
                    {
                        if (tabName != null && tabName.length > 0 && usingTabs == true)
                        {
                            continue;
                        }
                    }
                    var row = document.createElement('tr');

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
                            
                            if (inputDialog.conditionListContains(conditionList, inputField.InputName))
                            {
                                inp.oninput = (inputDialog.inputOnChangeFunc)(inputField.InputName, conditionList);
                                inp.onpropertychange = inp.oninput; // for IE8
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
                            if (inputDialog.conditionListContains(conditionList, inputField.InputName))
                            {
                                combo.onchange = (inputDialog.inputOnChangeFunc)(inputField.InputName, conditionList);
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
                            if (inputDialog.conditionListContains(conditionList, inputField.InputName))
                            {
                                inp.oninput = (inputDialog.inputOnChangeFunc)(inputField.InputName, conditionList);
                                inp.onpropertychange = inp.oninput; // for IE8
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
                            {
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

                            if (inputDialog.conditionListContains(conditionList, inputField.InputName))
                            {
                                inp.oninput = (inputDialog.inputOnChangeFunc)(inputField.InputName, conditionList);
                                inp.onpropertychange = inp.oninput; // for IE8
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
                    }
                        case 7: /// Masked Input
                            {
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

                                if (inputDialog.conditionListContains(conditionList, inputField.InputName))
                                {
                                    inp.oninput = (inputDialog.inputOnChangeFunc)(inputField.InputName, conditionList);
                                    inp.onpropertychange = inp.oninput; // for IE8
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
                            }
                        
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

                    addInputsToNode(inputTable, tabName, true);

                    tabDiv.appendChild(inputTable);
                }
                /// TODO: add a table for the bottom/shared inputs

                var sharedDiv = document.createElement('div');
                var sharedTable = document.createElement('table');
                sharedTable.className = "inputTable";
                sharedDiv.appendChild(sharedTable);
                pageDiv.appendChild(sharedDiv);

                addInputsToNode(sharedTable, "", false);
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

                addInputsToNode(inputTable, tabName, false);

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

                            // Get the input value
                            var inputValue = inputDialog.getInputValue(inputField.InputName, inputField.InputType, settings);

                            var buttonItem = settings.InputButtons[btnIndex];

                            if (buttonItem.ValidateInput == true)
                            {
                                if (inputValue.length == 0)
                                {
                                    if (inputField.Mandatory == true)
                                    {
                                        inputDialog.showMessage(inputField.InputName + ' is mandatory');
                                        return;
                                    }
                                    var mandatory = false;
                                    for (var k = 0; k < inputField.MandatoryConditions.length; k++)
                                    {
                                        var condition = inputField.MandatoryConditions[k];
                                        var conditionalValue = inputDialog.getInputValue(condition.ColumnName, null, settings);

                                        mandatory = siteMenu.conditionIsMet(condition, conditionalValue);

                                        if (mandatory == true)
                                        {
                                            inputDialog.showMessage(inputField.InputName + ' is mandatory');
                                            return;
                                        }
                                    }
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
                if (inputField.InputType == 3) //Checkbox
                {
                    inputItem.onchange();
                }
                else
                {
                    inputItem.oninput();
                }
                
            }
        });
    },

    getInputValue : function (inputName, inputType, settings)
    {
        var theInput;
        var inputValue;
        if (inputType == null)
        {
            for (var k = 0; k < settings.InputFields.length; k++)
            {
                var inputField = settings.InputFields[k];
                if (inputField.InputName == inputName)
                {
                    inputType = inputField.InputType;
                    break;
                }
            }
        }
        if (inputType == 5) // List Selection
        {
            inputValue = [];
            theInput = document.getElementById("_" + inputName + "_1");
            var options = theInput.options;

            for (var k = 0; k < options.length; k++)
            {
                inputValue.push(options[k].value);
            }
        }
        else
        {
            theInput = document.getElementById("_" + inputName);

            if (theInput == null)
            {
                return null;
            }
            inputValue = theInput.value;
            if (theInput.type == "checkbox")
            {
                inputValue = theInput.checked + "";
            }
        }
        inputValue = inputValue || "";
        return inputValue;
    },
};