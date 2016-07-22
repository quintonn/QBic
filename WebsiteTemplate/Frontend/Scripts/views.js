(function (views, $, undefined)
{
    views.showView = function (viewData)
    {
        dialog.showBusyDialog();
        console.log(viewData);

        return mainApp.makeWebCall("frontend/pages/Views.html?v=" + mainApp.version).then(function (data)
        {
            var model = new viewModel(viewData.Description, data, viewData);
            _applicationModel.addView(model);

            var menuItems = viewData.ViewMenu;
            for (var i = 0; i < menuItems.length; i++)
            {
                var menu = menuItems[i];
                var mModel = new viewMenuModel(menu.Label, menu.EventNumber, menu.ParametersToPass);
                model.viewMenus.push(mModel);
            }

            var columns = viewData.Columns;

            for (var i = 0; i < columns.length; i++)
            {
                var col = columns[i];
                var colModel = new columnModel(i, col.ColumnLabel, col.ColumnType != 4);
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
                    var cellIsVisible = col.ColumnType != 4 && processing.cellIsVisible(col, record);
                    
                    var cell = new cellModel(value, cellIsVisible, col.ColumnType);
                    rowItem.cells.push(cell);
                }
                model.rows.push(rowItem);
            }

            return dialog.closeBusyDialog();
        });
    };

    function columnModel(id, label, visible)
    {
        var self = this;
        self.id = id;
        self.label = ko.observable(label);
        self.visible = ko.observable(visible);
    }

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

        self.columnType = ko.observable(columnType);
    }

    function rowModel(data)
    {
        var self = this;

        self.data = data;

        self.cells = ko.observableArray([]);
    }

    function viewModel(title, html, settings)
    {
        var self = this;

        self.myid = ko.observable(-1);

        // Change buttonColumn to also have a KeyColumn property -> or better make link and button column more similar
        // ...also on click event i think i would want the original data. Maybe try without this for now

        self.settings = settings;

        self.viewTitle = ko.observable(title);
        self.viewMenus = ko.observableArray([]);
        self.filterText = ko.observable(settings.Filter);
        self.filterSearchClick = function ()
        {
            console.log('filter search clicked with value: ' + self.filterText());
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);

        self.click = function (rowItem, colIndex, rowIndex, evt)
        {
            console.clear();
            console.log(self.settings);
            console.log('click event:');
            console.log(rowItem);
            
            var cellItem = rowItem.cells()[colIndex];
            var data = self.settings.ViewData[rowIndex];
            var theColumn = self.settings.Columns[colIndex];

            var id = data["Id"];

            var formData =
                {
                    Id: id//data[theColumn.KeyColumn],
                };

            formData['rowData'] = data;
            //var thisRowId = this.getAttribute('rowId');
            //formData['rowData']['rowId'] = thisRowId;

            if (theColumn.Event.ActionType == 5) /// ShowMessage
            {
                //dialog.showMessage(theColumn.Event, null, formData, args);
                dialog.showMessage("Info", "TODO: view button on click action type = 5");
            }
            else if (theColumn.Event.ActionType == 6) ///Execute Action
            {
                var eventId = theColumn.Event.EventNumber;
                var formData = data["Id"];

                var viewSettings =
                {
                    "currentPage": self.settings.CurrentPage,
                    "linesPerPage": self.settings.LinesPerPage,
                    "totalLines": self.settings.TotalLines
                };

                formData =
                    {
                        data: formData,
                        viewSettings: "" // Why is this not included in the call?
                    };

                mainApp.executeUIAction(eventId, formData);
            }
            else
            {
                inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
            }
            
        };
    }

}(window.views = window.views || {}, jQuery));