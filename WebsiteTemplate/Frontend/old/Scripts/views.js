var views = {

    getTable: function ()
    {
        return document.getElementById('tblView');
    },

    populateRow: function (row, settings, data, rowId, args, isNew)
    {
        if (data['rowId'] == null || data['rowId'].length == 0)
        {
            data['rowId'] = rowId;
        }
        for (var j = 0; j < settings.Columns.length; j++)
        {
            var column = settings.Columns[j];
            var cell;
            if (isNew == true)
            {
                cell = document.createElement("td");
            }
            else
            {
                cell = row.cells[j];
            }
            
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

            if (value === undefined && column.ColumnName.length != 0 && isNew == false)
            {
                //console.log('value for column ' + column.ColumnName + ' is undefined inside populate view');
                continue;
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
                if (isNew == false)
                {
                    continue;
                }
                var button = document.createElement('button');

                button.onclick = (function (ind)
                {
                    return function ()
                    {
                        var theColumn = settings.Columns[ind];

                        var id = data["Id"];

                        var formData =
                            {
                                Id: data[theColumn.KeyColumn],
                            };

                        formData['rowData'] = data;
                        var thisRowId = this.getAttribute('rowId');
                        formData['rowData']['rowId'] = thisRowId;

                        if (theColumn.Event.ActionType == 5)
                        {
                            inputDialog.showMessage(theColumn.Event, null, formData, args);
                        }
                        else if (theColumn.Event.ActionType == 6)
                        {
                            var eventId = theColumn.Event.EventNumber;
                            var formData = data["Id"];

                            var viewSettings =
                            {
                                "currentPage": settings.CurrentPage,
                                "linesPerPage": settings.LinesPerPage,
                                "totalLines": settings.TotalLines
                            };

                            formData =
                                {
                                    data: formData,
                                    viewSettings: ""
                                };
                            
                            siteMenu.executeUIAction(eventId, formData, data["Id"]);
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
                var a;
                if (isNew == true)
                {
                    a = document.createElement('a');
                    a.href = "#";
                    a.innerHTML = column.LinkLabel;
                }
                else
                {
                    a = row.cells[j].firstChild;
                }
                
                a.onclick = (function (col, xData)
                {
                    return function ()
                    {
                        var formData =
                            {
                                Id: xData[col.KeyColumn],
                            };

                        if (settings.ActionType == 7) /// View for input
                        {
                            formData['rowData'] = xData;
                            var thisRowId = this.getAttribute('rowId');
                            //formData['rowId'] = thisRowId;
                            formData['rowData']['rowId'] = thisRowId;
                        };

                        var viewSettings =
                            {
                                "currentPage": settings.CurrentPage,
                                "linesPerPage": settings.LinesPerPage,
                                "totalLines": settings.TotalLines
                            };
                        formData["viewSettings"] = "";
                        var id = col.EventNumber;
                        siteMenu.executeUIAction(id, formData, args);
                    }
                })(column, data);
                a.setAttribute('rowId', rowId);
                if (isNew == true)
                {
                    cell.appendChild(a);
                }
            }
            else if (column.ColumnType == 5) /// Date
            {
                if (value != null && value.length > 0)
                {
                    value = new Date(value);
                    var month = "0" + (value.getMonth() + 1);
                    month = month.substring(month.length - 2);
                    var day = "0" + value.getDate();
                    day = day.substr(day.length - 2);
                    value = value.getFullYear() + "-" + month  + "-" + day; // TODO: should be configurable in settings somewhere
                }
                cell.style.whiteSpace = "nowrap";
                cell.innerHTML = value;
            }
            else
            {
                /// Replace new line characters with HTML breaks
                if (value == null)
                {
                    value = "";
                }

                if (typeof value === 'object')
                {
                    value = JSON.stringify(value);
                }
                else
                {
                    value = value.toString();
                    value = value.replace(/\r/g, ','); //TODO: not sure if this is the greatest idea
                    value = value.replace(/\n/g, ',');
                    // OR
                    //value = value.replace(/\r/g, ''); 
                    //value = value.replace(/\n/g, '<br/>');
                    // OR
                    // Make this a setting
                }

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

            if (column.ColumnType == 4) /// Hidden column
            {
                cell.style.display = "none";
            }
            if (isNew)
            {
                row.appendChild(cell);
            }
        }
    },

    populateViewWithData: function (table, data, settings, args, isMainView)
    {
        /// Add headings to table
        var headerRow = document.createElement("tr");

        for (var j = 0; j < settings.Columns.length; j++)
        {
            var headCell = document.createElement("th");
            headCell.innerHTML = settings.Columns[j].ColumnLabel;
            headCell.setAttribute('columnName', settings.Columns[j].ColumnName);
            if (settings.Columns[j].ColumnType == 4) /// Hidden column
            {
                headCell.style.display = "none";
            }
            headerRow.appendChild(headCell);
        }
        table.appendChild(headerRow);


        /// Add the actual data
        for (var i = 0; i < data.length; i++)
        {
            var row = document.createElement("tr");

            views.populateRow(row, settings, data[i], i, args, true);

            table.appendChild(row);
        }

        if (isMainView == true)
        {
            views.createFooterRow(table, settings);
        }
    },

    createFooterRow: function(table, settings)
    {
        var footerRow = document.createElement('tr');
        var footerCell = document.createElement('td');
        footerCell.colSpan = 100;
        footerCell.style.alignContent = "center";
        footerCell.style.textAlign = "center";
        footerCell.style.paddingBottom = "0";
        var container = document.createElement('span');

        var lastPage = Math.floor(settings.TotalLines / settings.LinesPerPage);
        if ((settings.TotalLines % settings.LinesPerPage) > 0)
        {
            lastPage += 1;
        }

        var makeButton = function(html, func)
        {
            var button = document.createElement('button');
            button.innerHTML = html;

            switch (func)
            {
                case 0:
                case 1:
                    if (settings.CurrentPage <= 1)
                    {
                        button.disabled = "disabled";
                    }
                    break;
                case 2:
                case 3:
                    if (settings.CurrentPage >= lastPage)
                    {
                        button.disabled = "disabled";
                    }
                    break;
                default:
                    dialog.showMessage("Unhandled case in footer button: " + func);
                    break;
            }

            button.onclick = (function (sett)
            {
                return function()
                {
                    var data =
                    {
                        currentPage: sett.CurrentPage,
                        linesPerPage: sett.LinesPerPage,
                        totalLines: sett.TotalLines
                    };
                    switch( func)
                    {
                        case 0:
                            data.currentPage = 1; 
                            break;
                        case 1:
                            data.currentPage -= 1;
                            break;
                        case 2:
                            data.currentPage += 1;
                            break;
                        case 3:
                            data.currentPage = lastPage;
                            break;
                        default:
                            dialog.showMessage("Unhandled case in footer button: " + func);
                            break;
                    }
                    data =
                        {
                            viewSettings: data
                        };
                    siteMenu.executeUIAction(sett.Id, data);
                }
            })(settings);
            return button;
        }
        var createOption = function (value)
        {
            var option = document.createElement('option');
            option.innerHTML = value + "";
            option.value = value;
            return option;
        };

        var firstButton = makeButton("<<", 0);
        var prevButton = makeButton("<", 1);
        var rowCountSelect = document.createElement('select');
        rowCountSelect.style.marginRight = "5px";
        rowCountSelect.style.width = "75px";
        
        rowCountSelect.appendChild(createOption(10));
        rowCountSelect.appendChild(createOption(25));
        rowCountSelect.appendChild(createOption(50));
        rowCountSelect.appendChild(createOption(100));
        rowCountSelect.appendChild(createOption("All"));

        var lpp = settings.LinesPerPage;
        if (lpp > 100)
        {
            lpp = "All";
        }
        rowCountSelect.value = lpp;

        rowCountSelect.onchange = (function (sett)
        {
            return function()
            {
                var lines = this.options[this.selectedIndex].value;
                
                if (lines == "All")
                {
                    lines = 1000;
                }
                var data =
                        {
                            viewSettings: 
                            {
                                currentPage: sett.CurrentPage,
                                linesPerPage: lines,
                                totalLines: sett.TotalLines
                            }
                        };
                siteMenu.executeUIAction(sett.Id, data);
            }
        })(settings);

        var nextButton = makeButton(">", 2);
        var lastButton = makeButton(">>", 3);

        container.appendChild(firstButton);
        container.appendChild(prevButton);
        container.appendChild(rowCountSelect);
        container.appendChild(nextButton);
        container.appendChild(lastButton);

        footerCell.appendChild(container);
        footerRow.appendChild(footerCell);

        var footerInfoRow = document.createElement('tr');
        var footerInfoCell = document.createElement('td');
        footerInfoCell.style.borderTop = "none";
        footerInfoCell.style.paddingTop = "0";
        footerInfoCell.style.alignContent = "center";
        footerInfoCell.style.textAlign = "center";
        footerInfoCell.colSpan = 100;
        
        var infoContainer = document.createElement("span");
        infoContainer.innerHTML = "Showing page " + settings.CurrentPage + " of " + lastPage;

        footerInfoCell.appendChild(infoContainer);
        footerInfoRow.appendChild(footerInfoCell);

        table.appendChild(footerRow);
        table.appendChild(footerInfoRow);
    },

    populateViewMenu: function (viewMenu, settings, args, isInputView)
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
                    var data =
                        {
                            data: vm.ParametersToPass,                            
                        }
                    siteMenu.executeUIAction(id, data, isInputView == true ? args : vm.ParametersToPass);
                }
            })(menu.EventNumber, i);

            viewMenu.appendChild(button);
        }
    },

    populateView: function (data, settings, callback, args, isMainView)
    {
        navigation.loadHtmlBody('mainContent', 'Views.html', function ()
        {
            var viewTitle = document.getElementById('viewsTitle');
            viewTitle.innerHTML = settings.Description;

            var table = views.getTable();

            views.populateViewWithData(table, data, settings, args, isMainView);

            if (isMainView)
            {
                var searchDiv = document.getElementById('searchDiv');
                searchDiv.style.display = "";

                var searchButton = document.getElementById('btnSearch');
                searchButton.onclick = (function (sett, searchArgs)
                {
                    return function()
                    {
                        var filter = document.getElementById('txtFilter').value;
                        
                        var tmpData =
                        {
                            viewSettings:
                            {
                                currentPage: sett.CurrentPage,
                                linesPerPage: sett.LinesPerPage,
                                totalLines: sett.TotalLines
                            },
                            filter: filter,
                            data: searchArgs
                        };

                        siteMenu.executeUIAction(sett.Id, tmpData, searchArgs);
                    }
                })(settings, args);

                var filter = document.getElementById('txtFilter');
                filter.value = settings.Filter;
            }

            var viewMenu = document.getElementById("viewsMenu");

            views.populateViewMenu(viewMenu, settings, args);

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

    deleteRowFromTable: function (table, rowId, isEdit)
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

                var rowIdAttribute = '-1';
                if (aItem.hasAttribute('rowId'))
                {
                    rowIdAttribute = aItem.getAttribute('rowId');
                }
                rowIdAttribute = parseInt(rowIdAttribute);

                if (rowIdAttribute > -1)
                {
                    var aRowId = rowIdAttribute;

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
            
            for (var j = 0; j < buttons.length; j++)
            {
                var aItem = buttons[j];
                var rowIdAttribute = '-1';
                if (aItem.hasAttribute('rowId'))
                {
                    rowIdAttribute = aItem.getAttribute('rowId');
                }
                
                rowIdAttribute = parseInt(rowIdAttribute);

                if (rowIdAttribute > -1)
                {
                    var aRowId = rowIdAttribute;

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
        }
        if (rowToDelete > -1)// && isEdit == false)
        {
            if (isEdit == false)
            {
                table.deleteRow(rowToDelete);
            }
            else
            {
                return rowToDelete;
            }
        }
        return realRowIdDeleted;
    },
};