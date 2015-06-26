var views = {

    getTable: function()
    {
        return document.getElementById('tblView');
    },

    addDataToTable: function (settings, data, table)
    {
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
                //if (setting.ColumnLabel[0] == "#")
                if (setting.Type == "button")
                {
                    var cell = document.createElement("td");
                    var button = document.createElement("button");
                    button.innerHTML = setting.Name;//.substring(1, setting.ColumnLabel.length);
                    button.onclick = (function (x)
                    {
                        return function ()
                        {
                            setting.Callback(x, data);
                        };
                    })(i);

                    cell.appendChild(button);
                    row.appendChild(cell);
                }
                else if (setting.Type == "a")
                {
                    var cell = document.createElement("td");
                    var a = document.createElement("a");
                    a.innerHTML = setting.Name;//.substring(1, setting.ColumnLabel.length);
                    a.href = "#";
                    a.onclick = (function (x)
                    {
                        return function ()
                        {
                            setting.Callback(x, data);
                        };
                    })(i);

                    cell.appendChild(a);
                    row.appendChild(cell);
                }
                else
                {
                    var value = null;
                    var fields = setting.Name.split("/");
                    
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
                    
                    if (setting.Name.toLowerCase() == "dob" || setting.Name.toLowerCase().indexOf('date') > -1)
                    {
                        value = value.substring(0, 10);
                    }
                    views.addCellToRow(value, row, setting.Type);
                }

            }

            table.appendChild(row);
        }
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

views.viewSetting = function (label, name, type, callback)
{
    this.ColumnLabel = label;
    this.Name = name;
    this.Type = type || "string";
    this.Callback = callback;
};

