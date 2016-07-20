(function (views, $, undefined)
{
    views.showView = function (viewData)
    {
        dialog.showBusyDialog();
        console.log('showing view:');
        console.log(viewData);

        return mainApp.makeWebCall("frontend/pages/Views.html?v=" + mainApp.version).then(function (data)
        {
            var model = new viewModel('Test title', data, _applicationModel.views().length + 1);
            _applicationModel.addView(model);

            var menuItems = viewData.ViewMenu;
            for (var i = 0; i < menuItems.length; i++)
            {
                var menu = menuItems[i];
                var mModel = new viewMenuModel(menu.Label, menu.EventNumber, menu.ParametersToPass);
                model.viewMenus.push(mModel);
            }
            console.log(viewData);
            var columns = viewData.Columns;

            for (var i = 0; i < columns.length; i++)
            {
                var col = columns[i];
                var colModel = createColumn(col, i);
                model.columns.push(colModel);
            }

            var items = viewData.ViewData;
            for (var j = 0; j < items.length; j++)
            {
                var record = items[j];
                var rowItem = new rowModel(record);
                for (var k = 0; k < columns.length; k++)
                {
                    var col = columns[k];
                    var value = processing.getColumnValue(col, record);
                    //console.log('create a row_col item');
                    var cellIsVisible = processing.cellIsVisible(col, record) && colModel.showColumn();
                    
                    var cell = new cellModel(value, cellIsVisible, col.ColumnType);
                    rowItem.cells.push(cell);
                }
                model.rows.push(rowItem);
            }

            return dialog.closeBusyDialog();
        });
    };

    function viewMenuModel(label, eventId, params)
    {
        var self = this;
        self.label = ko.observable(label);
        self.eventId = eventId;
        self.params = params;

        self.menuClick = function()
        {
            dialog.showMessage("Info", self.eventId + " - " + self.params);
        }
    }

    function cellModel(value, cellIsVisible, columnType)
    {
        var self = this;
        self.value = ko.observable(value);

        self.showCell = ko.observable(cellIsVisible);

        self.columnType = ko.observable(cellIsVisible == false ? -1 : columnType);

        self.click = function (rowData, viewColumn, evt)
        {
            console.log(rowData);
            var colType = viewColumn.columnType(); /// Will use this to decide if button or link etc.
            var data = rowData.data; /// Contains all info for the row
        };
    }

    function rowModel(data)
    {
        var self = this;

        self.data = data;

        self.cells = ko.observableArray([]);
    }

    function viewModel(title, html, id)
    {
        var self = this;

        self.myid = ko.observable(id);

        self.viewTitle = ko.observable(title);
        self.viewMenus = ko.observableArray([]);
        self.filterText = ko.observable();
        self.filterSearchClick = function ()
        {
            console.log('filter search clicked with value: ' + self.filterText());
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);
    }

    function createColumn(column, index)
    {
        switch (column.ColumnType)
        {
            case 2: /// Button
                return new buttonColumn(column, index);
            default:
                return new viewColumnModel(column, index);
        }
    }

    function viewColumnModel(column, index)
    {
        var self = this;
        self.index = index;
        
        self.columnType = ko.observable(column.ColumnType);
        self.showColumn = ko.computed(function ()
        {
            return self.columnType() != 4;
        }, self);

        self.columnLabel = ko.computed(function ()
        {
            return self.showColumn() == true ? column.ColumnLabel : "";
        }, self);
    }

    function buttonColumn(column, index)
    {
        var self = new viewColumnModel(column, index);

        var label = "????";
        if (column.ButtonTextSource == 0)
        {
            label = column.ButtonText;
        }
        self.buttonText = ko.observable(label);

        return self;
    }

}(window.views = window.views || {}, jQuery));