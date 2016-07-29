(function (inputDialog, $, undefined)
{
    inputDialog.conditionIsMet = function (condition, value)
    {
        if (condition.Comparison == 0)  // Equals
        {
            return value === condition.ColumnValue;
        }
        else if (condition.Comparison == 1) // Not-Equals
        {
            return value != condition.ColumnValue;
        }
        else if (condition.Comparison == 2) // Contains
        {
            return value.indexOf(condition.ColumnValue) > -1;
        }
        else if (condition.Comparison == 3) // IsNotNull
        {
            return value != null && (value + "").length > 0;
        }
        else if (condition.Comparison == 4) // IsNull
        {
            return value == null || (value + "").length == 0;
        }
        else
        {
            alert("Unknown comparison " + condition.Comparison);
            return false;
        }
    };

    inputDialog.buildInput = function (settings)
    {
        var title = settings.Description;
        var buttons = settings.InputButtons;
        var data = settings.InputData; // Not sure what this is for - not suppose to be sent to client possibly.
        var inputs = settings.InputFields;

        var model = new inputDialogModel(title, settings.Id, settings.Parameters);

        var tabs = {};
        var setDefaults = [];
        $.each(inputs, function (indx, inp)
        {
            var tabName = $.trim(inp.TabName || "");
            
            if (tabs[tabName] == null)
            {
                tabs[tabName] = new tabModel(tabName, model);
            }
            
            var inpModel = new inputFieldModel(inp, model);
            inpModel.inputValue.subscribe(function ()
            {
                var inModel = inpModel;
                var value = inModel.getInputValue();

                var inputsToHide = $.grep(inputs, function (inp, indx)
                {
                    var conditionList = $.grep(inp.VisibilityConditions, function (condition, indx2)
                    {
                        return condition.ColumnName == inModel.setting.InputName;
                    });
                    return conditionList.length > 0;
                });
                for (var i = 0; i < inputsToHide.length; i++)
                {
                    var inp = inputsToHide[i];

                    var matchedConditions1 = $.grep(inp.VisibilityConditions, function (condition, indx2)
                    {
                        return condition.ColumnName == inModel.setting.InputName;
                    });
                    var matchedConditions2 = $.grep(inp.VisibilityConditions, function (condition, indx2)
                    {
                        return condition.ColumnName == inModel.setting.InputName && inputDialog.conditionIsMet(condition, inModel.getInputValue());
                    });
                    
                    // This might need revision - test with multiple conditions
                    var showInput = matchedConditions1.length == matchedConditions2.length;
                    
                    model.toggleInputVisibility(inp.InputName, showInput);
                }
            });

            // All of this is very inefficient, including the method inside tabs to toggle visibility

            // This should happen after all inputs have been added to tabs and 
            // after all tabs have been added to the model - because the on propertychange / visibility check is 
            //  not working because not all items have been set.
            var action = function ()
            {
                //inpModel.setInputValue(inp.DefaultValue);
                inpModel.initialize(inp.DefaultValue);
            };
            setDefaults.push(action); // This is not a great solution - i don't  like it, smells bad
            
            tabs[tabName].inputs.push(inpModel);
        });

        $.each(tabs, function (indx, tab)
        {
            if (indx == "")
            {
                model.combinedTab(tab);
            }
            else
            {
                model.tabs.push(tab);
            }
        });

        if (model.tabs().length > 0)
        {
            var tab = model.tabs()[0];
            model.currentTab(tab);
        }

        $.each(buttons, function (indx, button)
        {
            var bModel = new inputButtonModel(button.ActionNumber, button.Label, button.ValidateInput);
            model.buttons.push(bModel);
        });
        
        dialog.showDialogWithId('InputDialog', model).then(function()
        {
            for (var i = 0; i < setDefaults.length; i++)
            {
                var action = setDefaults[i];
                action();
            }
        });

        return Promise.resolve();
    }

    function inputDialogModel(title, eventId, params)
    {
        var self = this;
        self.eventId = eventId;
        self.title = ko.observable(title);

        self.tabs = ko.observableArray([]);

        self.currentTab = ko.observable();
        self.combinedTab = ko.observable();

        self.buttons = ko.observableArray([]);

        self.params = params;

        self.closeClick = function ()
        {
            dialog.closeModalDialog();
        };

        self.getInputs = function (validateInput)
        {
            var tabs = self.tabs().slice(0);
            tabs.push(self.combinedTab());

            var getInputs = function (tab)
            {
                return tab.getInputs(validateInput);
            }

            return Promise.all(tabs.map(getInputs));
        }

        self.buttonClick = function(btn, evt)
        {
            dialog.showBusyDialog("Processing...");
            self.getInputs(btn.validateInput).then(function (inputs)
            {
                var res = {};
                $.each(inputs, function (indx, inp)
                {
                    $.extend(res, res, inp);
                });
                res["parameters"] = self.params;
                
                return mainApp.processEvent(self.eventId, btn.actionNumber, res).then(function ()
                {
                    return dialog.closeBusyDialog();
                });
            })
            .catch(function (err)
            {
                if (err == "X")
                {
                    // This is fine, it means there was a mandatory input and a dialog is shown to the user.
                }
                else
                {
                    mainApp.handleError(err);
                }
            });
        }

        self.toggleInputVisibility = function (inputName, showInput)
        {
            var result;
            var tabs = self.tabs().slice(0);
            var com = self.combinedTab();
            if (com != null)
            {
                tabs.push(com);
            }

            var inputItem = $.each(tabs, function (indx, tab)
            {
                if (tab == null)
                {
                    return false;
                }
                
                $.each(tab.inputs(), function (indx2, inp)
                {
                    if (inp == null)
                    {
                        return false;
                    }
                    
                    if (inp.setting.InputName == inputName)
                    {
                        result = inp;
                        return false;
                    }
                });
                if (result != null)
                {
                    return false;
                }
            });
            if (result != null)
            {
                result.visible(showInput);
            }
        }
    }

    function tabModel(tabName, inpDlgModel)
    {
        var self = this;
        self.inputDialogModel = inpDlgModel;

        self.tabName = ko.observable(tabName);

        self.selected = ko.computed(function ()
        {
            return self.inputDialogModel.currentTab() == self;
        }, self);

        self.setCurrentTab = function ()
        {
            self.inputDialogModel.currentTab(self);
        };

        self.colorClass = ko.computed(function ()
        {
            return self.selected() == true ? "w3-grey" : "w3-light-grey";
        }, self);

        self.inputs = ko.observableArray([]);

        self.getInputs = function(validateInput)
        {
            var results = {};
            
            for (var i = 0; i < self.inputs().length; i++)
            {
                var inp = self.inputs()[i];
                var value = inp.getInputValue();
                
                if (validateInput &&  (value == null || value.length == 0) && inp.mandatory == true && inp.visible() == true)
                {
                    dialog.closeBusyDialog();
                    return dialog.showMessage("Warning", inp.setting.InputName + ' is mandatory').then(function ()
                    {
                        return Promise.reject('X');
                    });
                }
                results[inp.setting.InputName] = value;
            }
            
            return Promise.resolve(results);
        }
    }

    function inputFieldModel(inputSetting, inpDlgModel)
    {
        var self = this;
        self.setting = inputSetting;
        self.inputDialogModel = inpDlgModel;

        self.inputLabel = ko.observable(inputSetting.InputLabel);
        self.inputValue = ko.observable();

        self.inputType = inputSetting.InputType;
        self.mandatory = inputSetting.Mandatory;
        self.raisePropertyChangeEvent = inputSetting.RaisePropertyChangedEvent;
        self.visible = ko.observable(inputSetting.InputType != 2); // 2 - hidden input

        self.propertyChanged = function ()
        {
            if (self.raisePropertyChangeEvent == true)
            {
                // Need to call raise property changed event
            }

            // need to check if other inputs need to become visible or not - VisibilityConditions
        }
        self.options = ko.computed(function ()
        {
            if (self.inputType != 3)
            {
                return [];
            }
            var results = [];

            for (var i = 0; i < self.setting.ListItems.length; i++)
            {
                var item = self.setting.ListItems[i];
                var model = new optionModel(item.Value, item.Key);
                results.push(model);
            }
            return results;
        }, self);

        self.setInputValue = function (value)
        {
            switch (self.inputType)
            {
                case 3: // Combobox
                    //TODO: Don't like this either, using knockout there must be a better way
                    var opt = self.options();
                    for (var i = 0; i < opt.length; i++)
                    {
                        if (opt[i].value == value)
                        {
                            self.inputValue(opt[i]);
                            break;
                        }
                    }
                    break;
                case 5: // List selection / list source
                    console.log('should do nothing here');
                    break;
                default:
                    self.inputValue(value);
                    break;
            }
        };

        self.getInputValue = function ()
        {
            var value = self.inputValue();

            switch (self.inputType)
            {
                case 3:  // combo box
                    if (value != null)
                    {
                        value = value.value;
                    }
                    break;
                case 4: // Boolean
                    if (value == null)
                    {
                        value = false + "";
                    }
                    value = value + "";
                case 5: // List selection / list source
                    var listSource = self.listSource();
                    //Get only selected items
                    listSource = $.grep(listSource, function (item, index)
                    {
                        return item.selected() == true;
                    });
                    // obtain the values of selected items
                    var values = $.map(listSource, function (item)
                    {
                        return item.value;
                    });
                    
                    value = values;
                    break;
                default:
                    break;
            }
            
            return value;
        }

        self.id = inputSetting.InputName;
        self.html = ko.observable();

        self.listSource = ko.observableArray([]);

        self.selectAll = ko.observable(false);
        self.selectAllClicked = function ()
        {
            var list = self.listSource();
            $.each(list, function (indx, item)
            {
                item.selected(self.selectAll());
            });
            return true;
        };
        self.listSourceSelect = function (item, evt)
        {
            self.selectAll(self.isAllListSelected());
            return true;
        };
        self.isAllListSelected = function()
        {
            var list = self.listSource();
            var newList = $.grep(list, function (item, indx)
            {
                return item.selected() == true;
            });
            return list.length == newList.length;
        }

        self.initialize = function (defaultValue)
        {
            var inputSetting = self.setting;

            switch (self.inputType)
            {
                case 5: // List Source
                    var defaultList = defaultValue || [];
                    var listSource = inputSetting.ListSource; // (Key, Value)
                    listSource = $.map(listSource, function (item)
                    {
                        var selected = defaultList.indexOf(item.Key) > -1;
                        return new listSourceItemModel(selected, item.Value, item.Key);
                    });
                    self.listSource(listSource);

                    self.selectAll(self.isAllListSelected());
                    break;
                case 8: // View
                    views.showView(inputSetting.ViewForInput, true, inputSetting.ViewForInput.Id).then(function (model)
                    {
                        self.html(model.html());

                        model.applyKoBindings();

                        model.updateViewData(inputSetting.ViewForInput.Id);
                    });
                    break;
                default:
                    self.setInputValue(defaultValue);
                    break;
            }
        }
    }

    function listSourceItemModel(isSelected, displayValue, itemValue)
    {
        var self = this;
        self.selected = ko.observable(isSelected);
        self.label = ko.observable(displayValue);
        self.value = itemValue;
    }

    function optionModel(displayText, value)
    {
        var self = this;
        self.displayText = displayText;
        self.value = value;
    }

    function inputButtonModel(actionNumber, label, validateInput)
    {
        var self = this;

        self.label = label;
        self.actionNumber = actionNumber;
        self.validateInput = validateInput;
    }

}(window.inputDialog = window.inputDialog || {}, jQuery));