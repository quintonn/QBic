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
        console.log(settings);

        var title = settings.Description;
        var buttons = settings.InputButtons;
        var data = settings.InputData; // Not sure what this is for - not suppose to be sent to client possibly.
        var inputs = settings.InputFields;

        var model = new inputDialogModel(title, settings.Id);

        var tabs = {};
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
                    
                    console.log(showInput);
                    
                    model.toggleInputVisibility(inp.InputName, showInput);
                }
            });

            // All of this is very inefficient, including the method inside tabs to toggle visibility

            // This should happen after all inputs have been added to tabs and 
            // after all tabs have been added to the model - because the on propertychange / visibility check is 
            //  not working because not all items have been set.
            inpModel.inputValue(inp.DefaultValue);
            
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
        
        dialog.showDialogWithId('InputDialog', model);

        return Promise.resolve();
    }

    function inputDialogModel(title, eventId)
    {
        var self = this;
        self.eventId = eventId;
        self.title = ko.observable(title);

        self.tabs = ko.observableArray([]);

        self.currentTab = ko.observable();
        self.combinedTab = ko.observable();

        self.buttons = ko.observableArray([]);

        self.closeClick = function ()
        {
            dialog.closeModalDialog();
        };

        self.getInput = function (inputName)
        {
            alert('?????');
            //var result = null;
            //var tabs = self.tabs();
            //$.each(tabs, function (indx, tab)
            //{
            //    var inputs = tab.inputs();
            //    $.each(inputs, function (idx, inp)
            //    {
            //        if (inp.setting.InputName == inputName)
            //        {
            //            result = inp;
            //            return false;
            //        }
            //    });
            //});
            //console.log('result');
            //console.log(result);
            //return result;
        }

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
            self.getInputs(btn.validateInput).then(function (inputs)
            {
                var res = {};
                $.each(inputs, function (indx, inp)
                {
                    $.extend(res, res, inp);
                });
                
                return mainApp.processEvent(self.eventId, btn.actionNumber, res);
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
        //self.inputValue.subscribe(function (val)
        //{
        //    var value = self.getInputValue();
            
        //});
        self.inputType = inputSetting.InputType;
        self.mandatory = inputSetting.Mandatory;
        self.raisePropertyChangeEvent = inputSetting.RaisePropertyChangedEvent;
        self.visible = ko.observable(inputSetting.InputType != 2); // 2 - hidden input
        //inputSetting.MandatoryConditions;
        //inputSetting.VisibilityConditions;
        self.propertyChanged = function()
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

        self.getInputValue = function ()
        {
            var value = self.inputValue();
            
            if (self.inputType == 3 && value != null) // Combobox
            {
                value = value.value;
            }
            else if (self.inputType == 4)
            {
                if (value == null)
                {
                    return false + "";
                }
                return value + "";
            }
            return value;
        }
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