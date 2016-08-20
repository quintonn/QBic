/// <reference path="mainApplication.js" />

(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
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
                        dialog.showBusyDialog("Processing...");
                        return mainApp.executeUIAction(item.EventNumber, item.ParametersToPass).then(dialog.closeBusyDialog);
                        //case 7:  Input Data View
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
                        //return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);

                        break;
                        //case 9:  DeleteInputViewItem

                    case 10: // UpdateDataSourceComboBox
                        var dlgs = _applicationModel.modalDialogs();
                        var inputDlg = dlgs[dlgs.length - 1].model;
                        var inp = inputDlg.findInputModelWithName(item.InputName);
                        return inp.getInputValue().then(function (inpValue)
                        {
                            inp.listItems(item.ListItems);
                            inp.setInputValue(inpValue);
                        });
                        break;
                    case 11:
                        return processing.showOrDownloadFile(item);
                        //main.makeWebCall(main.webApiURL + settings.DataUrl, "POST", callback, settings.RequestData);
                        break;
                        // case 11: View File
                    case 12: //UpdateInput
                        var inputName = item.InputName;
                        var value = item.InputValue;
                        return inputDialog.updateInput(inputName, value);
                        break;
                    default:
                        console.warn(item);
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
        return mainApp.makeWebCall(url, "POST", item.RequestData, ["content-type", "FileName"]).then(function (resp)
        {
            dialog.closeBusyDialog();
            var dataUrl = "data:" + resp['content-type'] + ";base64," + resp.data;
            var filename = resp['FileName'];
            
            if (processing.supportsBlob() == true)
            {
                var blobData = processing.base64ToBlob(resp.data, resp['content-type']);

                if (window.navigator.msSaveOrOpenBlob)
                {
                    window.navigator.msSaveOrOpenBlob(blobData, filename);
                }
                else
                {
                    var blobUrl = (window.webkitURL || window.URL).createObjectURL(blobData);

                    var a = document.createElement('a');
                    a.style = "display: none";
                    
                    a.href = blobUrl;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                    setTimeout(function ()
                    {
                        document.body.removeChild(a);
                        (window.webkitURL || window.URL).revokeObjectURL(blobUrl);
                    }, 100);
                }
            }
            else
            {
                // Try using data url
                var newWin = window.open(dataUrl, "_blank");
                if (!newWin || newWin.closed || typeof newWin.closed == 'undefined')
                {
                    dialog.showMessage("Info", "The content was blocked by your browser. Look in the top-right corner to allow popups on this site or to view the file this time only.");
                }
            }
        });
    };

    processing.supportsBlob = function ()
    {
        try
        {
            var svg = new Blob(["<svg xmlns='http://www.w3.org/2000/svg'></svg>"], { type: "image/svg+xml;charset=utf-8" });

            // Safari 6 uses "webkitURL".
            var url = window.webkitURL || window.URL;
            var objectUrl = url.createObjectURL(svg);
            
            if (/^blob:/.exec(objectUrl) === null)
            {
                // `URL.createObjectURL` created a URL that started with something other
                // than "blob:", which means it has been polyfilled and is not supported by
                // this browser.
                return false;
            } else
            {
                return true;
            }
        } catch (err)
        {
            console.error(err);
            return false;
        }
    };

    processing.base64ToBlob = function (b64Data, contentType, sliceSize)
    {
        contentType = contentType || '';
        sliceSize = sliceSize || 512;

        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize)
        {
            var slice = byteCharacters.slice(offset, offset + sliceSize);

            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++)
            {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            var byteArray = new Uint8Array(byteNumbers);

            byteArrays.push(byteArray);
        }

        try
        {
            var blob = new Blob(byteArrays, { type: contentType });
            return blob;
        } catch (e)
        {
            // The BlobBuilder API has been deprecated in favour of Blob, but older
            // browsers don't know about the Blob constructor
            // IE10 also supports BlobBuilder, but since the `Blob` constructor
            //  also works, there's no need to add `MSBlobBuilder`.
            var bb = new (window.WebKitBlobBuilder || window.MozBlobBuilder);
            bb.append(arraybuffer);
            var blob = bb.getBlob(contentType); // <-- Here's the Blob
            return blob;
        }
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