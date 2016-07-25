(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
        return Promise.all(data.map(function (item)
        {
            var actionType = item.ActionType;

            switch (actionType)
            {
                case 0: // Show a view
                    return views.showView(item);
                case 1: // Get Input / User Input
                    return inputDialog.buildInput(item);
                    break;
                case 4: // Close input dialog -- Not sure i need this anymore.
                    return dialog.closeModalDialog();
                case 5:
                    return dialog.getUserConfirmation(item, item.Data);
                case 6: // Execute UI action
                    return mainApp.executeUIAction(item.EventNumber, item.ParametersToPass);
                default:
                    return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);
            }
        })).catch(function (err)
        {
            return mainApp.handleError(err);
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