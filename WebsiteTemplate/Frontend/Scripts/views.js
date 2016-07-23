(function (views, $, undefined)
{
    views.showView = function (viewData)
    {
        dialog.showBusyDialog();

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

        self.settings = settings;

        var lastPage = Math.floor(settings.TotalLines / settings.LinesPerPage);
        if ((settings.TotalLines % settings.LinesPerPage) > 0)
        {
            lastPage += 1;
        }
        self.lastPage = ko.observable(lastPage);
        self.currentPage = ko.observable(settings.CurrentPage);
        self.linesPerPage = ko.observable(settings.LinesPerPage);

        self.footerText = ko.observable("Showing page " + settings.CurrentPage + " of " + lastPage);

        self.viewTitle = ko.observable(title);
        self.viewMenus = ko.observableArray([]);
        self.filterText = ko.observable(settings.Filter);
        self.filterSearchClick = function ()
        {
            dialog.showBusyDialog("Searching...");
            var tmpData =
                        {
                            viewSettings:
                            {
                                currentPage: self.settings.CurrentPage,
                                linesPerPage: self.settings.LinesPerPage,
                                totalLines: self.settings.TotalLines
                            },
                            filter: self.filterText(),
                            parameters: self.settings.Parameters,
                            eventParameters: self.settings.EventParameters
                        };

            mainApp.executeUIAction(self.settings.Id, tmpData).then(dialog.closeBusyDialog);
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);

        self.click = function (rowItem, colIndex, rowIndex, evt)
        {
            dialog.showBusyDialog("Processing...");
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
                    Id: id,
                    data: data,
                    viewSettings: "",  // Why is this not included in the call?
                    parameters: theColumn.ParametersToPass,
                    eventParameters: self.settings.EventParameters
                };

            if (theColumn.Event == null || theColumn.Event.ActionType == 6) // 6 = execute UI Action
            {
                var eventId = theColumn.Event == null ? theColumn.EventNumber : theColumn.Event.EventNumber;

                mainApp.executeUIAction(eventId, formData).then(dialog.closeBusyDialog);
            }
            else if (theColumn.Event.ActionType == 5) /// ShowMessage
            {
                dialog.closeBusyDialog().then(function ()
                {
                    return dialog.getUserConfirmation(theColumn.Event, formData);
                });
            }
            else
            {
                dialog.closeBusyDialog.then(function ()
                {
                    return inputDialog.showMessage("Unknown action type " + theColumn.Event.ActionType);
                });
            }
        };

        self.gotoPage = function (pageNum)
        {
            var data =
                        {
                            viewSettings:
                            {
                                currentPage: pageNum,
                                linesPerPage: self.linesPerPage(),
                                totalLines: self.settings.TotalLines
                            },
                            filter: self.filterText(),
                            parameters: self.settings.Parameters,
                            eventParameters: self.settings.EventParameters
                        };
            dialog.showBusyDialog("Processing...");
            mainApp.executeUIAction(self.settings.Id, data).then(dialog.closeBusyDialog);
        }

        self.firstClick = function()
        {
            self.gotoPage(1);
        }
        self.prevClick = function()
        {
            self.gotoPage(self.currentPage() - 1);
        }
        self.nextClick = function()
        {
            self.gotoPage(self.currentPage() + 1);
        }
        self.lastClick = function ()
        {
            self.gotoPage(self.lastPage());
        }
        self.linesPerPageChange = function(obj, event)
        {
            if (event.originalEvent)
            { //user changed
                self.gotoPage(1);
            }
        }

        self.visibleColumnLength = ko.computed(function ()
        {
            var cols = self.columns().filter(function (column)
            {
                return column.visible() == true;
            });
            return cols.length;
        }, self);
    }

}(window.views = window.views || {}, jQuery));