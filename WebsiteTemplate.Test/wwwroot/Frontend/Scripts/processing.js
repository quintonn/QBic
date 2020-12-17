/// <reference path="mainApplication.js" />
/// <reference path="views.js" />
/// <reference path="inputDialog.js" />

(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
        views.currentView = -1;
        data = data || [];
        return Promise.all(data.map(function (item)
        {
            try
            {
                var actionType = item.ActionType;

                var params = item.Parameters;
                
                if (params != null && params.length > 0)
                {
                    //params = JSON.parse(params);
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
                        return dialog.showBusyDialog("Processing...").then(function ()
                        {
                            return mainApp.executeUIAction(item.EventNumber, item.ParametersToPass).then(dialog.closeBusyDialog);
                        });
                    case 8: // Update input view (view in input screen)
                    case 9: // DeleteInputViewItem
                        params = JSON.parse(params);

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
                    case 10: // UpdateDataSourceComboBox
                        
                        var dlgs = _applicationModel.modalDialogs();
                        var inputDlg = dlgs[dlgs.length - 1].model;
                        var inp = inputDlg.findInputModelWithName(item.InputName);
                        return inp.getInputValue().then(function (inpValue)
                        {
                            if (inp.inputType == 5) // List Selection
                            {
                                var defaultList = inpValue || [];

                                var listSource = item.ListItems
                                listSource = $.map(listSource, function (item)
                                {
                                    var selected = defaultList.indexOf(item.Key) > -1;
                                    return new listSourceItemModel(selected, item.Value, item.Key);
                                });
                                inp.listSource(listSource);
                                inp.selectAll(inp.isAllListSelected());
                            }
                            else
                            {
                                inp.listItems(item.ListItems);
                                inp.setInputValue(inpValue);
                            }
                        });
                        break;
                    case 11: // View File
                        return processing.showOrDownloadFile(item);
                    case 12: //UpdateInput
                        var inputName = item.InputName;
                        var value = item.InputValue;
                        return inputDialog.updateInput(inputName, value);
                        break;
                    case 13: //Logout
                        {
                            dialog.closeModalDialog();
                            return auth.logout().then(function (x)
                            {
                                // Clear the URL
                                window.location.href = window.location.href.split("?")[0].split("#")[0];
                                
                                return Promise.resolve(x);
                            });
                            break;
                        }
                    case 14: // Show a list view

                        var viewItems = _applicationModel.views();

                        var existingModel = $.grep(viewItems, function (v, indx)
                        {
                            return v.id == item.Id;
                        });
                        if (existingModel.length == 0)
                        {
                            return views.showListView(item, false, item.Id).then(function (m)
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
                    case 15: // Show detail view
                        var viewItems = _applicationModel.views();

                        var existingModel = $.grep(viewItems, function (v, indx)
                        {
                            return v.id == item.Id;
                        });
                        if (existingModel.length == 0)
                        {
                            return views.showDetailsView(item, false, item.Id).then(function (m)
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
                    case 17: // Update input visibility
                        var inputName = item.InputName;
                        var isVisibile = item.InputIsVisible;
                        return inputDialog.updateInputVisibility(inputName, isVisibile);
                    default:
                        console.warn(item);
                        dialog.closeBusyDialog();
                        return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);
                }
            }
            catch (error)
            {
                return Promise.reject(error);
            }
        })).catch(function (err)
        {
            return mainApp.handleError(err);
        });
    };

    processing.showOrDownloadFile = function (item)
    {
        dialog.showBusyDialog("Downloading file...");
        var url = mainApp.apiURL + item.DataUrl;
        url = url.replace("//", "/");

        var dataToSend = "";
        if (item.RequestData != null)
        {
            dataToSend = base64.encode(item.RequestData);
        }

        return new Promise(function (res, rej)
        {
            $.ajax({
                url: url,
                type: "POST",
                data: dataToSend,
                xhrFields: {
                    responseType: 'blob'
                },
                dataType: 'binary',
                beforeSend: function (xhr)
                    {
                        xhr.setRequestHeader("Authorization", "Bearer " + auth.accessToken);
                    }

            }).done(function (xhr, xx, resp)
            {
                var filename;
                if (resp && resp.getResponseHeader)
                {
                    filename = resp.getResponseHeader("FileName");
                }
                if (filename == null || filename.length == 0)
                {
                    filename = "unknown file.zip";
                }
                if (window.navigator && window.navigator.msSaveBlob) // Microsoft
                {
                    window.navigator.msSaveBlob(xhr, filename);
                }
                else
                {
                    var objUrl = URL.createObjectURL(xhr);

                    var a = document.createElement('a');
                    a.href = objUrl;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                    URL.revokeObjectURL(objUrl);

                    a.remove();
                }
                
                dialog.closeBusyDialog();
                res();
            }).fail(function (jqXHR, textStatus, error)
            {
                console.error(error);
                dialog.showMessage("Error", "Error downloading file: \n" + error);
                dialog.closeBusyDialog();
                res();
            });
        }).catch(function (err)
        {
            console.error(err);
            dialog.showMessage("Error", "Error downloading file: \n" + err);
            dialog.closeBusyDialog();
            res();
        });

        //return dialog.closeBusyDialog();
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

    processing.loadViewMenu = function (eventId, model, dataForGettingMenu)
    {
        dialog.showBusyDialog("Loading view menu...");
        var url = mainApp.apiURL + "getViewMenu/" + eventId;
        var data = 
            {
                data: dataForGettingMenu
            };
        data = JSON.stringify(data);
        return mainApp.makeWebCall(url, "POST", data).then(function (data)
        {
            var menuItems = data || [];

            var params = {};
            params["ViewId"] = model.id;

            for (var i = 0; i < menuItems.length; i++)
            {
                var menu = menuItems[i];
                var mModel = new views.viewMenuModel(menu.Label, menu.EventNumber, menu.ParametersToPass, params, menu.IncludeDataInView);
                model.viewMenus.push(mModel);
            }
            dialog.closeBusyDialog();
        });
    };

    processing.cellIsVisible = function (column, data)
    {
        if (column.ColumnSetting != null)
        {
            console.log(column.ColumnSetting);
            if (column.ColumnSetting.ColumnSettingType == 0) /// Show/Hide column
            {
                var show = column.ColumnSetting.Display == 0;
                var compareResult = true;

                for (var p = 0; p < column.ColumnSetting.Conditions.length; p++)
                {
                    var condition = column.ColumnSetting.Conditions[p];
                    var colName = condition.ColumnName;

                    var actualValue = processing.parseColumnName(colName, data)+"";

                    compareResult = compareResult && processing.isConditionMet(condition, actualValue);
                }

                if ((compareResult == false && show == true) || (compareResult == true && show == false))
                {
                    return false;
                }
            }
        }
        return true;
    };

    processing.isConditionMet = function (condition, actualValue)
    {
        var colName = condition.ColumnName;
        var comparison = condition.Comparison;
        var colVal = condition.ColumnValue;

        var compareResult = true;
        switch (comparison)
        {
            case 0: // Equals
                return actualValue == colVal;
            case 1: // Not Equals
                return actualValue != colVal;
            case 2: // Contains
                return (actualValue.toLowerCase()).indexOf(colValue.toLowerCase()) > -1;
            case 3: // IsNotNull
                return actualValue != null && actualValue.length > 0;
            case 4: // IsNull
                return (actualValue == null || actualValue.length == 0);
            case 5: // Greater Than
                actualValue = parseInt(actualValue);
                return actualValue != null && actualValue > colVal;
            case 6: // Greater than or equal
                actualValue = parseInt(actualValue);
                return actualValue != null && actualValue >= colVal;
            case 7: // Less than
                actualValue = parseInt(actualValue);
                return actualValue != null && actualValue < colVal;
            case 8: // Less than or equal to
                actualValue = parseInt(actualValue);
                return actualValue != null && actualValue <= colVal;
            default:
                dialog.showMessage("Error", "Unknown condition comparison type: " + comparison);
                break;
        }
        return compareResult;
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