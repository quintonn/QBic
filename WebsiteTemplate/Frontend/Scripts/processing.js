(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
        data = data || [];
        return Promise.all(data.map(function (item)
        {
            var actionType = item.ActionType;

            var params = item.Parameters;
            if (params != null && params.length > 0)
            {
                params = JSON.parse(params);
            }
            else
            {
                params = {};
            }

            switch (actionType)
            {
                case 0: // Show a view
                    
                    var viewItems = _applicationModel.views();
                    
                    var existingModel = $.grep(viewItems, function (v, indx)
                    {
                        return v.id == item.Id;
                    });
                    if (existingModel.length == 0)
                    {
                        return views.showView(item, false, item.Id).then(function (m)
                        {
                            _applicationModel.addView(m);
                        });
                    }
                    else
                    {
                        _applicationModel.addView(existingModel[0]);
                        
                        existingModel[0].gotoPage(1);
                        return Promise.resolve();
                    }
                case 1: // Get Input / User Input
                    return inputDialog.buildInput(item);
                //case 2: // Submenu
                    
                    //case 3: // DoSomething

                case 4: // Close input dialog -- Not sure i need this anymore.
                    return dialog.closeModalDialog();
                case 5:
                    return dialog.getUserConfirmation(item, item.Data, params);
                case 6: // Execute UI action
                    return mainApp.executeUIAction(item.EventNumber, item.ParametersToPass);
                //case 7:  Input Data View
                case 8: // Update input view (view in input screen)
                case 9: // DeleteInputViewItem
                    var viewId = params.ViewId;
                    
                    var rowId = params.RowId;

                    if (rowId == null)
                    {
                        rowId = -1;
                    }
                    
                    var updateType = item.UpdateType; // 0 - add / 1 - delete

                    var dataToUpdate = item.JsonDataToUpdate;
                    dataToUpdate = JSON.parse(dataToUpdate);
                    var view = document.getElementById('view_' + viewId);
                    
                    var model = ko.contextFor(view).$rawData;
                    if (updateType == 0 && actionType == 8)
                    {
                        return model.updateRow(rowId, dataToUpdate);
                    }
                    else
                    {
                        return model.deleteRow(rowId);
                    }
                    //return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);

                    break;
                    //case 9:  DeleteInputViewItem

                    // case 10: UpdateDataSourceComboBox
                    // case 11: View File
                case 12: //UpdateInput
                    var inputName = item.InputName;
                    var value = item.InputValue;
                    return inputDialog.updateInput(inputName, value);
                    break;
                default:
                    console.log(item);
                    return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);
            }
        })).catch(function (err)
        {
            return mainApp.handleError(err);
        });
    };

    processing.updateViewData = function (eventId, params)
    {
        var data =
            {
                Data: params || ""
            };

        data = JSON.stringify(data);

        var url = mainApp.apiURL + "updateViewData/" + eventId;
        return mainApp.makeWebCall(url, "POST", data);
    };

    processing.loadViewMenu = function (eventId, model)
    {
        dialog.showBusyDialog("Loading view menu...");
        var url = mainApp.apiURL + "getViewMenu/" + eventId;
        return mainApp.makeWebCall(url, "GET").then(function (data)
        {
            var menuItems = data || [];

            var params = {};
            params["ViewId"] = model.id;

            for (var i = 0; i < menuItems.length; i++)
            {
                var menu = menuItems[i];
                var mModel = new views.viewMenuModel(menu.Label, menu.EventNumber, menu.ParametersToPass, params);
                model.viewMenus.push(mModel);
            }
            dialog.closeBusyDialog();
        });
    };

    processing.cellIsVisible = function (column, data)
    {
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

                    //var actualValue = data[colName] || "";
                    //actualValue = actualValue.toString();
                    var actualValue = processing.parseColumnName(colName, data)+"";

                    switch (comparison)
                    {
                        case 0: // Equals
                            compareResult = compareResult && actualValue == colVal;
                            break;
                        case 1: // Not Equals
                            compareResult = compareResult && actualValue != colVal;
                            break;
                        case 2: // Contains
                            break;
                        case 3: // IsNotNull
                            compareResult = compareResult && actualValue != null && actualValue.length > 0;
                            break;
                        case 4: // IsNull
                            compareResult = compareResult && (actualValue == null || actualValue.length == 0);
                            break;
                        default:
                            dialog.showMessage("Error", "Unknown condition comparison type: " + comparison);
                            break;
                    }
                }

                if ((compareResult == false && show == true) || (compareResult == true && show == false))
                {
                    return false;
                }
            }
        }
        return true;
    };

    processing.parseColumnName = function (columnName, data)
    {
        var value = "";
        if (columnName != null && columnName.length > 0)
        {
            value = data;
            var colName = columnName;
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
        return value;
    };

    processing.getColumnValue = function(column, rowData)
    {
        var columnName = column.ColumnName;
        var columnType = column.ColumnType;
        var value = processing.parseColumnName(columnName, rowData);

        switch (columnType)
        {
            case 1: /// Boolean
                if (value == true)
                {
                    value = column.TrueValueDisplay;
                }
                else if (value == false)
                {
                    value = column.FalseValueDisplay;
                }
                break;
            case 2: /// Button
            case 3: /// Link
                value = column.LinkLabel;
                break;
            default:
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
                    value = value.replace(/\n/g, ','); //      maybe i can just add text-wrapping to the table and show the full text on hover-over
                    // OR
                    //value = value.replace(/\r/g, ''); 
                    //value = value.replace(/\n/g, '<br/>');
                    // OR
                    // Make this a setting
                }
                break;
        }

        return value;
    }
}(window.processing = window.processing || {}, jQuery));