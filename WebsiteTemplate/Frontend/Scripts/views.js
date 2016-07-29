(function (views, $, undefined)
{
    views.getViewContent = function ()
    {
        return mainApp.makeWebCall("frontend/pages/Views.html?v=" + mainApp.version);
    };

    views.showView = function (viewData, isEmbeddedView, id)
    {
        dialog.showBusyDialog('Loading data...');
        return mainApp.makeWebCall("frontend/pages/Views.html?v=" + mainApp.version).then(function (data)
        {
            data = data.replace("##view_id##", "view_" + id);

            var model = new viewModel(viewData.Description, data, viewData, isEmbeddedView || false, id);
            
            processing.loadViewMenu(id, model);
            
            var columns = viewData.Columns;

            for (var i = 0; i < columns.length; i++)
            {
                var col = columns[i];
                var colModel = new columnModel(i, col.ColumnLabel, col.ColumnType != 4);
                
                model.columns.push(colModel);
            }

            model.addData(viewData.ViewData);

            return dialog.closeBusyDialog().then(function ()
            {
                return Promise.resolve(model);
            });
        });
    };

    function columnModel(id, label, visible)
    {
        var self = this;
        self.id = id;
        self.label = ko.observable(label);
        self.visible = ko.observable(visible);
    }

    views.viewMenuModel = function(label, eventId, menuParams, viewParams)
    {
        var self = this;
        self.label = ko.observable(label);
        self.eventId = eventId;
        self.menuParams = menuParams;
        self.viewParams = viewParams;
    }

    function cellModel(value, cellIsVisible, columnType)
    {
        var self = this;
        self.value = ko.observable(value);

        self.showCell = ko.observable(cellIsVisible);

        self.columnType = ko.observable(columnType);
    }

    function rowModel(data, id)
    {
        var self = this;

        self.id = id;
        self.data = data;
        self.cells = ko.observableArray([]);
    }

    function viewModel(title, html, settings, isEmbeddedView, id)
    {
        var self = this;

        self.settings = settings;

        self.id = id;

        self.linesPerPage = ko.observable(settings.LinesPerPage);

        self.isEmbeddedView = isEmbeddedView;

        self.lastPage = ko.computed(function()
        {
            var lastPage = Math.floor(self.settings.TotalLines / self.linesPerPage());
            if ((self.settings.TotalLines % self.linesPerPage()) > 0)
            {
                lastPage += 1;
            }
            return lastPage
        }, self);
        self.currentPage = ko.observable(self.settings.CurrentPage);
        
        self.footerText = ko.computed(function ()
        {
            return "Showing page " + self.settings.CurrentPage + " of " + self.lastPage();
        }, self);

        self.viewTitle = ko.observable(title);

        self.viewMenus = ko.observableArray([]);
        self.menuClick = function (menu, index, evt)
        {
            var data =
                {
                    data: menu.menuParams,
                    parameters: menu.viewParams
                }

            if (self.settings.ActionType == 7) // View Input
            {
                self.addViewDataToParams(data);
            }

            dialog.showBusyDialog("Processing...").then(function ()
            {
                return mainApp.executeUIAction(menu.eventId, data);
            }).then(dialog.closeBusyDialog);
        };

        self.addViewDataToParams = function (params)
        {
            var rows = self.rows();
            var rowsData = $.map(rows, function (r)
            {
                return r.data;
            });

            var mainData = params['data'];
            if (mainData == null || mainData.length == 0)
            {
                mainData = {};
            }
            mainData = JSON.parse(JSON.stringify(mainData)); // without this a circular reference is created by the next line
            mainData["ViewData"] = rowsData;

            params['data'] = mainData;
            //params['ViewData'] = rowsData;
        };

        self.filterText = ko.observable(settings.Filter);
        self.filterSearchClick = function ()
        {
            var tmpData =
                        {
                            viewSettings:
                            {
                                currentPage: self.settings.CurrentPage,
                                linesPerPage: self.settings.LinesPerPage,
                                totalLines: -1//self.settings.TotalLines
                            },
                            filter: self.filterText(),
                            parameters: self.settings.Parameters,
                            eventParameters: self.settings.EventParameters
                        };

            self.updateViewData(self.settings.Id, tmpData);
        };
        self.filterKeyPressed = function (model, evt)
        {
            if (evt.keyCode == 13)
            {
                self.filterSearchClick();
            }
            return true;
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);

        self.click = function (rowItem, colIndex, rowIndex, evt)
        {
            dialog.showBusyDialog("Processing...");
            var cellItem = rowItem.cells()[colIndex];
            //var data = self.settings.ViewData[rowIndex];
            var data = rowItem.data;
            var theColumn = self.settings.Columns[colIndex];

            var id = data[theColumn.KeyColumn];

            var viewSettings =
                {
                    "currentPage": self.settings.CurrentPage,
                    "linesPerPage": self.settings.LinesPerPage,
                    "totalLines": self.settings.TotalLines
                };

            var params = theColumn.ParametersToPass || {};
            
            params["ViewId"] = self.id;
            params["RowId"] = rowItem.id;

            var formData =
                {
                    Id: id,
                    data: data,
                    viewSettings: "",  // Why is this not included in the call?
                    //parameters: theColumn.ParametersToPass,
                    parameters: params,
                    eventParameters: self.settings.EventParameters
                };

            if (self.settings.ActionType == 7) // View Input
            {
                self.addViewDataToParams(formData);
            }

            if (theColumn.Event == null || theColumn.Event.ActionType == 6) // 6 = execute UI Action
            {
                var eventId = theColumn.Event == null ? theColumn.EventNumber : theColumn.Event.EventNumber;

                mainApp.executeUIAction(eventId, formData).then(dialog.closeBusyDialog);
            }
            else if (theColumn.Event.ActionType == 5) /// ShowMessage
            {
                dialog.closeBusyDialog().then(function ()
                {
                    return dialog.getUserConfirmation(theColumn.Event, formData, params);
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
            self.updateViewData(self.settings.Id, data);
        }

        self.updateViewData = function (eventId, params)
        {
            //return Promise.resolve();
            dialog.showBusyDialog("Searching...");
            processing.updateViewData(eventId, params).then(function (resp)
            {
                self.settings = resp;

                self.currentPage(resp.CurrentPage);
                self.filterText(resp.Filter);
                self.linesPerPage(resp.LinesPerPage);
                
                self.addData(resp.ViewData, resp.Columns);

                self.lastPage.notifySubscribers();

                dialog.closeBusyDialog();
            }).catch(function(err)
            {
                dialog.closeBusyDialog();
                mainApp.handleError(err);
            });
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

        self.addData = function (data)
        {
            self.rows([]);
            var items = data || [];
            for (var j = 0; j < items.length; j++)
            {
                var record = items[j];
                self.addRow(record, j);
            }
        };

        self.addRow = function (data, rowId)
        {
            var rowItem = new rowModel(data, rowId);
            var columns = self.settings.Columns;

            for (var k = 0; k < columns.length; k++)
            {
                var col = columns[k];
                var value = processing.getColumnValue(col, data);

                var cellIsVisible = col.ColumnType != 4 && processing.cellIsVisible(col, data);

                var cell = new cellModel(value, cellIsVisible, col.ColumnType);
                rowItem.cells.push(cell);
            }
            self.rows.push(rowItem);
        };

        self.deleteRow = function (rowId)
        {
            return new Promise(function (resolve, reject)
            {
                self.rows.remove(function (row)
                {
                    return row.id == rowId;
                });
                resolve();
            });
        };

        self.updateRow = function (rowId, data)
        {
            return new Promise(function (resolve, reject)
            {
                if (rowId > -1)
                {
                    var rows = self.rows();

                    var rowModel = $.grep(rows, function (r, indx)
                    {
                        return r.id == rowId;
                    })[0];

                    rowModel.data = data;

                    var columns = self.settings.Columns;

                    for (var k = 0; k < columns.length; k++)
                    {
                        var col = columns[k];
                        var value = processing.getColumnValue(col, data);

                        var cellIsVisible = col.ColumnType != 4 && processing.cellIsVisible(col, data);

                        var cell = rowModel.cells()[k];
                        cell.showCell(cellIsVisible);
                        cell.value(value);
                    }
                }
                else
                {
                    $.each(self.rows(), function (indx, row)
                    {
                        rowId = Math.max(rowId, row.id);
                    });
                    rowId += 1;

                    self.addRow(data, rowId);
                }
                resolve();
            });
        };

        self.applyKoBindings = function ()
        {
            var div = document.getElementById("view_" + self.id);
            ko.cleanNode(div);
            ko.applyBindings(self, div);
        };
    }

}(window.views = window.views || {}, jQuery));