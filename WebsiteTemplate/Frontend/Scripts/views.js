﻿var views = {

    getTable: function()
    {
        return document.getElementById('tblView');
    },

    populateViewWithData: function(table, data, settings)
    {
        /// Add headings to table
        var headerRow = document.createElement("tr");

        for (var j = 0; j < settings.Columns.length; j++)
        {
            var headCell = document.createElement("th");
            headCell.innerHTML = settings.Columns[j].ColumnLabel;
            headerRow.appendChild(headCell);
        }
        table.appendChild(headerRow);


        /// Add the actual data
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
    },

    populateViewMenu: function(viewMenu, settings)
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
                    siteMenu.executeUIAction(id, vm.ParametersToPass);
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
    }
};