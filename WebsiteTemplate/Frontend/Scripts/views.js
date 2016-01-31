var views = {

    getTable: function()
    {
        return document.getElementById('tblView');
    },

    populateRow: function(row, settings, data, rowId, args)
    {
        for (var j = 0; j < settings.Columns.length; j++)
        {
            var column = settings.Columns[j];
            var cell = document.createElement("td");

            var value = "";

            if (column.ColumnName != null && column.ColumnName.length > 0)
            {
                value = data;//[i];
                var colName = column.ColumnName;
                while (colName.indexOf('.') > -1)
                {
                    var index = colName.indexOf('.');
                    var partName = colName.substring(0, index);
                    if (value == null)
                    {
                        break;
                    }
                    value = value[partName];
                    colName = colName.substring(index + 1);
                }
                if (value != null)
                {
                    value = value[colName];
                }
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

                button.onclick = (function (ind)
                {
                    return function ()
                    {
                        var theColumn = settings.Columns[ind];
                        
                        var id = data["Id"];

                        //var formData = data;//JSON.stringify(data);
                        var formData =
                            {
                                Id: data[theColumn.KeyColumn],
                            };

                        formData['rowData'] = data;
                        var thisRowId = this.getAttribute('rowId');
                        formData['rowId'] = thisRowId;

                        if (theColumn.Event.ActionType == 5)
                        {
                            inputDialog.showMessage(theColumn.Event, null, formData, args);
                        }
                        else if (theColumn.Event.ActionType == 6)
                        {
                            var eventId = theColumn.Event.EventNumber;
                            var formData = data["Id"];
                            
                            siteMenu.executeUIAction(eventId, formData);
                        }
                        else
                        {
                            inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
                        }
                    }
                })(j);

                if (column.ButtonTextSource == 0) //Fixed button text
                {
                    button.innerHTML = column.ButtonText;
                }
                else
                {
                    inputDialog.showMessage("Unhandled ButtonTextSource: " + column.ButtonTextSource);
                    button.innerHTML = "????";
                }
                button.setAttribute('rowId', rowId);
                cell.appendChild(button);
            }
            else if (column.ColumnType == 3) /// Link
            {
                var a = document.createElement('a');
                a.href = "#";
                a.innerHTML = column.LinkLabel;
                a.onclick = (function (col)
                {
                    return function ()
                    {
                        var formData =
                            {
                                Id: data[col.KeyColumn],
                            };
                        
                        if (settings.ActionType == 7) /// View for input
                        {
                            formData['rowData'] = data;
                            var thisRowId = this.getAttribute('rowId');
                            formData['rowId'] = thisRowId;
                        };

                        var id = col.EventNumber;
                        siteMenu.executeUIAction(id, formData, args);
                    }
                })(column);
                a.setAttribute('rowId', rowId);
                cell.appendChild(a);
            }
            else
            {
                /// Replace new line characters with HTML breaks
                if (value == null)
                {
                    value = "";
                }
                
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

                        var actualValue = data[colName] || "";
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
    },

    populateViewWithData: function(table, data, settings, args)
    {
        /// Add headings to table
        var headerRow = document.createElement("tr");

        for (var j = 0; j < settings.Columns.length; j++)
        {
            var headCell = document.createElement("th");
            headCell.innerHTML = settings.Columns[j].ColumnLabel;
            headCell.setAttribute('columnName', settings.Columns[j].ColumnName);
            headerRow.appendChild(headCell);
        }
        table.appendChild(headerRow);


        /// Add the actual data
        for (var i = 0; i < data.length; i++)
        {
            var row = document.createElement("tr");

            views.populateRow(row, settings, data[i], i, args);

            table.appendChild(row);
        }
    },

    populateViewMenu: function(viewMenu, settings, args)
    {
        while (viewMenu.firstChild)
        {
            viewMenu.removeChild(viewMenu.firstChild);
        }

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
                    siteMenu.executeUIAction(id, vm.ParametersToPass, args);
                }
            })(menu.EventNumber, i);

            viewMenu.appendChild(button);
        }
    },

    populateView: function (data, settings, callback, args)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            var viewTitle = document.getElementById('viewsTitle');
            viewTitle.innerHTML = settings.Description;

            var table = views.getTable();
            
            views.populateViewWithData(table, data, settings);

            var viewMenu = document.getElementById("viewsMenu");
            //menuBuilder.clearNode('viewsMenu');

            views.populateViewMenu(viewMenu, settings);

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

    deleteRowFromTable: function(table, rowId, isEdit)
    {
        var realRowIdDeleted = -1;
        var rowToDelete = -1;
        for (var i = 0; i < table.rows.length; i++)
        {
            var deleteRow = false;
            var row = table.rows[i];
            
            var aList = row.getElementsByTagName('a');
            var buttons = row.getElementsByTagName('button');
            
            for (var j = 0; j < aList.length; j++)
            {
                var aItem = aList[j];
                if (aItem.hasAttribute('rowId'))
                {
                    var aRowId = parseInt(aItem.getAttribute('rowId'));

                    if (rowId == aRowId)
                    {
                        if (deleteRow == false)
                        {
                            deleteRow = true;
                            rowToDelete = i;
                            realRowIdDeleted = aRowId;
                        }
                    }
                    else if (aRowId > rowId && (isEdit == null || isEdit == false))
                    {
                        aRowId = aRowId - 1;
                        aItem.setAttribute('rowId', aRowId);
                    }
                }
            }
            if (rowToDelete > -1)
            {
                table.deleteRow(rowToDelete);
                return realRowIdDeleted;
            }
            else
            {
                for (var j = 0; j < buttons.length; j++)
                {
                    var aItem = buttons[j];
                    if (aItem.hasAttribute('rowId'))
                    {
                        var aRowId = parseInt(aItem.getAttribute('rowId'));

                        if (rowId == aRowId)
                        {
                            if (deleteRow == false)
                            {
                                deleteRow = true;
                                rowToDelete = i;
                                realRowIdDeleted = aRowId;
                            }
                        }
                        else if (aRowId > rowId && (isEdit == null || isEdit == false))
                        {
                            aRowId = aRowId - 1;
                            aItem.setAttribute('rowId', aRowId);
                        }
                    }
                }
                if (rowToDelete > -1)
                {
                    table.deleteRow(rowToDelete);
                    return realRowIdDeleted;
                }
            }
        }
        
        return realRowIdDeleted;
    },
};