﻿(function (views, $, undefined)
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
            var data =
                {
                    data: params,
                }
            dialog.showBusyDialog("Processing...").then(function ()
            {
                return mainApp.executeUIAction(eventId, data);
            }).then(dialog.closeBusyDialog);
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
            var tmpData =
                        {
                            viewSettings:
                            {
                                currentPage: self.settings.CurrentPage,
                                linesPerPage: self.settings.LinesPerPage,
                                totalLines: self.settings.TotalLines
                            },
                            filter: self.filterText(),
                            //Id: self.settings.Id
                            parameters: self.settings.Parameters,
                            eventParameters: self.settings.EventParameters
                            //data: searchArgs
                        };

            //siteMenu.executeUIAction(sett.Id, tmpData, searchArgs);
            mainApp.executeUIAction(self.settings.Id, tmpData);
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);

        self.click = function (rowItem, colIndex, rowIndex, evt)
        {
            var cellItem = rowItem.cells()[colIndex];
            var data = self.settings.ViewData[rowIndex];
            var theColumn = self.settings.Columns[colIndex];

            var id = data[theColumn.KeyColumn];

            var viewSettings =
                {
                    "currentPage": self.settings.CurrentPage,
                    "linesPerPage": self.settings.LinesPerPage,
                    "totalLines": self.settings.TotalLines
                };

            var formData =
                {
                    Id: id, //data[theColumn.KeyColumn],
                    data: data,
                    viewSettings: "",  // Why is this not included in the call?
                    parameters: theColumn.ParametersToPass,
                    eventParameters: self.settings.EventParameters
                };
            console.log('Parameters to pass: ' + theColumn.ParametersToPass);

            if (theColumn.Event == null || theColumn.Event.ActionType == 6)
            {
                var eventId = theColumn.Event == null ? theColumn.EventNumber : theColumn.Event.EventNumber;

                //formData =
                //    {
                //        data: formData,
                //        viewSettings: "" 
                //    };
                //if (theColumn.ParametersToPass != null && theColumn.ParametersToPass.length > 0)
                //{
                //    console.log(theColumn.ParametersToPass);
                //    formData = theColumn.ParametersToPass;
                //    alert(formData);
                //}
                mainApp.executeUIAction(eventId, formData);
            }
            else if(theColumn.Event.ActionType == 5) /// ShowMessage
            {
                //dialog.showMessage(theColumn.Event, null, formData, args);
                dialog.showMessage("Info", "TODO: view button on click action type = 5");
                // Need to be able to have a .then on showMessage. So i can have yes/no/cancel buttons and process their events
            }
            else
            {
                inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
            }
            
        };
    }

}(window.views = window.views || {}, jQuery));