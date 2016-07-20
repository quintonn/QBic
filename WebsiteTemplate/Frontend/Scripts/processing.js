(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
        for (var i = 0; i < data.length; i++)
        {
            var item = data[i];

            var actionType = item.ActionType;

            switch (actionType)
            {
                case 0:
                    return views.showView(item);
                default:
                    return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);
            }
        }
    };

    processing.isCell

    processing.getColumnValue = function(column, rowData)
    {
        var columnName = column.ColumnName;
        var columnType = column.ColumnType;
        var value = "";
        if (columnName != null && columnName.length > 0)
        {
            value = rowData;//[i];
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
            case 2:
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