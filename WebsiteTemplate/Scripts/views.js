var views = {

    getTable: function()
    {
        return document.getElementById('tblView');
    },

    addDataToTable: function (settings, data, table)
    {
        alert('x');
        table = table || views.getTable();
        var row = document.createElement("tr");

        for (var j = 0; j < settings.length; j++)
        {
            var cell = document.createElement("th");
            cell.innerHTML = settings[j].ColumnLabel;
            row.appendChild(cell);
        }
        table.appendChild(row);

        if (data != null && data.length > 0)
        {
            
        }
        else
        {
            data = [];
        }

        for (var i = 0; i < data.length; i++)
        {
            var item = data[i];

            row = document.createElement("tr");

            for (var j = 0; j < settings.length; j++)
            {
                var setting = settings[j];

                if (setting.ConditionalCheckCallback != null)
                {
                    var showInfo = setting.ConditionalCheckCallback(i, data);
                    if (showInfo == false)
                    {
                        views.addCellToRow("", row, setting.Type);
                        continue;
                    }
                }
                
                if (setting.Type == "button")
                {
                    var cell = document.createElement("td");
                    var button = document.createElement("button");
                    button.innerHTML = setting.Name;
                    button.onclick = (function (x, s)
                    {
                        return function ()
                        {
                            s.Callback(x, data);
                        };
                    })(i, setting);

                    cell.appendChild(button);
                    row.appendChild(cell);
                }
                else if (setting.Type == "a")
                {
                    var cell = document.createElement("td");
                    var a = document.createElement("a");
                    var value = setting.Name;
                    if (value[0] == "#")
                    {
                        value = value.substring(1, value.length - 1);
                    }
                    else
                    {
                        value = views.getPropertyValue(item, setting.Name);
                    }
                    a.innerHTML = value;
                    a.href = "#";
                    a.onclick = (function (x, s)
                    {
                        return function ()
                        {
                            s.Callback(x, data);
                        };
                    })(i, setting);

                    cell.appendChild(a);
                    row.appendChild(cell);
                }
                else
                {
                    //var value = null;
                    //var fields = setting.Name.split("/");
                    
                    //for (var x = 0; x < fields.length; x++)
                    //{
                    //    var f = fields[x];
                        
                    //    if (value == null)
                    //    {
                    //        if (x == 0)
                    //        {
                    //            value = item[f];
                    //        }
                    //        else
                    //        {
                    //            value = "";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        value = value[f];
                    //    }
                    //}
                    
                    //if (setting.Name.toLowerCase() == "dob" || setting.Name.toLowerCase().indexOf('date') > -1)
                    //{
                    //    value = value.substring(0, 10);
                    //}
                    var value = views.getPropertyValue(item, setting.Name);
                    views.addCellToRow(value, row, setting.Type);
                }
            }

            table.appendChild(row);
        }
    },

    getPropertyValue: function(item, propertyName)
    {
        var value = null;
        var fields = propertyName.split("/");

        for (var x = 0; x < fields.length; x++)
        {
            var f = fields[x];

            if (value == null)
            {
                if (x == 0)
                {
                    value = item[f];
                }
                else
                {
                    value = "";
                }
            }
            else
            {
                value = value[f];
            }
        }

        if (propertyName.toLowerCase() == "dob" || propertyName.toLowerCase().indexOf('date') > -1)
        {
            value = value.substring(0, 10);
        }
        return value;
    },

    addCellToRow: function (cellData, row, dataType)
    {
        var cell = document.createElement("td");
        switch (dataType)
        {
            case "bool":
                cell.innerHTML = (cellData == true || cellData == "true" || cellData =="True") ? "Yes" : "No";
                break;
            case "string":
            default:
                cell.innerHTML = cellData;
                break;
        };
        row.appendChild(cell);
    },
};

views.viewSetting = function (label, name, type, callback, conditionalCheckCallback)
{
    this.ColumnLabel = label;
    this.Name = name;
    this.Type = type || "string";
    this.Callback = callback;
    this.ConditionalCheckCallback = conditionalCheckCallback;
};

