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
            dialog.showMessage("Error", "Unknown comparison: " + condition.Comparison);
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
                inModel.getInputValue().then(function (value)
                {
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

                        //var matchedConditions1 = $.grep(inp.VisibilityConditions, function (condition, indx2)
                        //{
                        //    return condition.ColumnName == inModel.setting.InputName;
                        //});
                        var matchedConditions2 = $.grep(inp.VisibilityConditions, function (condition, indx2)
                        {
                            return condition.ColumnName == inModel.setting.InputName && inputDialog.conditionIsMet(condition, value);
                        });

                        // This might need revision - might need a way to have AND, OR, etc in the conditions.
                        
                        //var showInput = matchedConditions1.length == matchedConditions2.length;
                        var showInput = matchedConditions2.length > 0;

                        model.toggleInputVisibility(inp.InputName, showInput);
                    }
                });
            });

            // All of this is very inefficient, including the method inside tabs to toggle visibility

            // This should happen after all inputs have been added to tabs and 
            // after all tabs have been added to the model - because the on propertychange / visibility check is 
            //  not working because not all items have been set.
            var action = function ()
            {
                return inpModel.initialize(inp.DefaultValue);
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
        
        return dialog.showDialogWithId('InputDialog', model).then(function()
        {
            var act = function (action)
            {
                return action();
            };
            
            return Promise.all($.map(setDefaults, act)).then(function ()
            {
                if (model.tabs().length > 0)
                {
                    model.currentTab();
                }
                return Promise.resolve();
            });
        });
    }

    inputDialog.updateInputVisibility = function (inputName, isVisible)
    {
        var modals = _applicationModel.modalDialogs();
        var dialogModel = modals[modals.length - 1];

        var inputDlgModel = dialogModel.model;

        var inputFldModel = inputDlgModel.findInputModelWithName(inputName);
        if (inputFldModel == null)
        {
            return dialog.showMessage("Error", "Unexpected error, no input found with name " + inputName);
        }

        inputFldModel.visible(isVisible);

        return Promise.resolve();
    }

    inputDialog.updateInput = function (inputName, inputValue)
    {
        var modals = _applicationModel.modalDialogs();
        var dialogModel = modals[modals.length - 1];
        
        var inputDlgModel = dialogModel.model;

        var inputFldModel = inputDlgModel.findInputModelWithName(inputName);
        if (inputFldModel == null)
        {
            return dialog.showMessage("Error", "Unexpected error, no input found with name " + inputName);
        }

        inputFldModel.setInputValue(inputValue);

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

            var getInputsFunc = function (tab)
            {
                if (tab != null)
                {
                    return tab.getInputs(validateInput);
                }
                else
                {
                    return [];
                }
            }

            return Promise.all(tabs.map(getInputsFunc));
        }

        // to use form data-https://stackoverflow.com/questions/21044798/how-to-use-formdata-for-ajax-file-upload
        // xxxxxxx
        self.buttonClick = function(btn, evt)
        {
            dialog.showBusyDialog("Processing...").then(function ()
            {
                return self.getInputs(btn.validateInput).then(function (inputs)
                {
                    var res = {};
                    $.each(inputs, function (indx, inp)
                    {
                        $.extend(res, res, inp);
                    });
                    res["parameters"] = self.params;

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
            }).then(dialog.closeBusyDialog)
            .catch(function(err)
            {
                console.error(err);
                mainApp.handleError(err);
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

        self.findInputModelWithName = function (inputName)
        {
            var tabs = self.tabs().slice(0) || [];
            tabs.push(self.combinedTab());
            
            var models = $.grep(tabs, function (tab)
            {
                var models = tab.findInputModelWithName(inputName);
                return models.length > 0;
            });

            if (models.length == 0)
            {
                //dialog.showMessage("Error", "Unexpected error, no inputs found with name: " + inputName);
                return null;
            }
            var inputs = models[0].findInputModelWithName(inputName);
            if (inputs.length == 0)
            {
                return null;
            }
            return inputs[0];
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
            if (self.inputDialogModel.currentTab() != self)
            {
                self.inputDialogModel.currentTab(self);

                var inputs = self.inputs();
                $.each(inputs, function (indx, inp)
                {
                    if (inp.viewModel != null)
                    {
                        inp.viewModel.applyKoBindings();
                    }
                });
            }
            else
            {
                self.inputDialogModel.currentTab(self);
            }
        };

        self.colorClass = ko.computed(function ()
        {
            return self.selected() == true ? "w3-grey" : "w3-light-grey";
        }, self);

        self.inputs = ko.observableArray([]);

        self.findInputModelWithName = function(inputName)
        {
            var models = $.grep(self.inputs(), function (inp)
            {
                return (inp.setting.InputName == inputName);
            });

            return models;
        }

        self.getInputs = function(validateInput)
        {
            var results = {};

            var mandatoryConditionFunction = function (condition)
            {
                return new Promise(function (resolve, reject)
                {
                    var inpName = condition.ColumnName;
                    var inpModel = self.inputDialogModel.findInputModelWithName(inpName);
                    if (inpModel == null)
                    {
                        console.error('inpModel is null');
                        resolve(false);
                    }
                    else
                    {
                        inpModel.getInputValue().then(function (actualValue)
                        {
                            var conditionMet = processing.isConditionMet(condition, actualValue);

                            resolve(conditionMet);
                        }).catch(reject);
                    }
                });
            };

            var getInputFunction = function (inp)
            {
                var doValidation = validateInput;

                var tmpProm = null;

                return new Promise(function (resolve, reject)
                {
                    /*return*/ inp.getInputValue().then(function (value)
                    {
                        if (doValidation && (value == null || value.length == 0) && inp.mandatory == true && inp.visible() == true)
                        {
                            dialog.closeBusyDialog();

                            return dialog.showMessage("Warning", inp.setting.InputLabel + ' is mandatory').then(function ()
                            {
                                reject('X');
                            });
                        }
                        else if (doValidation && (value == null || value.length == 0) && inp.setting.MandatoryConditions != null && inp.setting.MandatoryConditions.length > 0 && inp.visible() == true)
                        {
                            var acts = $.map(inp.setting.MandatoryConditions, mandatoryConditionFunction);
                            tmpProm = function()
                            {
                                return Promise.all(acts).then(function (actData)
                                {
                                    if (actData.indexOf(false) == -1) // i.e. all mandatory conditions have been met
                                    {
                                        dialog.closeBusyDialog();
                                        return dialog.showMessage("Warning", inp.setting.InputLabel + ' is mandatory').then(function ()
                                        {
                                            console.error("reject " + inp.setting.InputName);
                                            reject('X');
                                        });
                                    }
                                });
                            };
                        }

                        results[inp.setting.InputName] = value;

                        if (tmpProm != null)
                        {
                            tmpProm().then(resolve);
                        }
                        else
                        {
                            resolve();
                        }
                    }).catch(function (err)
                    {
                        console.error(err);
                        reject(err);
                    });
                });
            };
            
            var inputItems = self.inputs();
            var actions = $.map(inputItems, getInputFunction);

            return Promise.all(actions).then(function (xData)
            {
                return Promise.resolve(results);
            });
        }
    }

    function inputFieldModel(inputSetting, inpDlgModel)
    {
        var self = this;
        self.setting = inputSetting;
        self.inputDialogModel = inpDlgModel;

        self.inputLabel = ko.observable(inputSetting.InputLabel);
        self.inputValue = ko.observable();

        self.dateFormat = 'dd-mm-yy'; // Not the usual format. Specific format for datepicker
        
        if (inputSetting.InputType == 10) // Numeric
        {
            self.Step = inputSetting.Step;
            //var decimalPlaces = inputSetting.DecimalPlaces;
        }

        self.multiLine = inputSetting.MultiLineText;

        self.inputType = inputSetting.InputType;
        self.mandatory = inputSetting.Mandatory;
        self.raisePropertyChangeEvent = inputSetting.RaisePropertyChangedEvent;
        self.visible = ko.observable(inputSetting.InputType != 2); // 2 - hidden input

        self.propertyChanged = function (stringValue, actualValue)
        {
            if (self.raisePropertyChangeEvent == true)
            {
                dialog.showBusyDialog("Processing input...");
                self.getInputValue().then(function (value)
                {
                    var data =
                    {
                        Data:
                            {
                                PropertyName: self.setting.InputName,
                                PropertyValue: value,
                                EventId: self.inputDialogModel.eventId
                            }
                    };
                    return Promise.resolve(data);
                }).then(function (data)
                {
                    return mainApp.raisePropertyChanged(data, self.inputDialogModel.eventId);
                }).then(function ()
                {
                    dialog.closeBusyDialog();
                }).catch(mainApp.handleError);
            }
            // need to check if other inputs need to become visible or not - VisibilityConditions
        }

        self.listItems = ko.observableArray(inputSetting.ListItems != null ? inputSetting.ListItems : []);
        self.options = ko.computed(function ()
        {
            if (self.inputType != 3)
            {
                return [];
            }
            var results = [];
            var listItems = self.listItems();
            for (var i = 0; i < listItems.length; i++)
            {
                var item = listItems[i];
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
                    var valueSet = false;
                    var opt = self.options();
                    for (var i = 0; i < opt.length; i++)
                    {
                        if (opt[i].value == value)
                        {
                            self.inputValue(opt[i]);
                            valueSet = true;
                            break;
                        }
                    }
                    if (valueSet == false)
                    {
                        self.inputValue("");
                    }
                    break;
                case 5: // List selection / list source
                    break;
                case 6:
                    self.inputValue(value);
                    break;
                case 8: // Input view
                    break;
                case 9: // File input
                    self.inputValue(value);
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
                    return Promise.resolve(value);
                case 4: // Boolean
                    if (value == null)
                    {
                        value = false + "";
                    }
                    value = value + "";
                    return Promise.resolve(value);
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
                    return Promise.resolve(value);
                case 6: // Date Input
                    value = value || "";
                    return Promise.resolve(value);
                case 8: // Input view
                    var model = self.viewModel;
                    if (model != null)
                    {
                        var params = {};
                        model.addViewDataToParams(params);
                        return Promise.resolve(params);
                    }
                    else
                    {
                        return Promise.resolve(null);
                    }
                case 9: // File Input
                    var file = self.inputValue();
                    
                    if (file != null)
                    {
                        return new Promise(function (resolve, reject)
                        {
                            var reader = new FileReader();

                            var fileData = null;
                            reader.onload = (function (theFile)
                            {
                                return function (e)
                                {
                                    fileData = e.target.result;
                                    //console.log(fileData);
                                    fileData = window.btoa(fileData);  // base 64 encode
                                    var filename = theFile.name;
                                    var parts = filename.split('.');
                                    var extension = "";
                                    if (parts.length > 1)
                                    {
                                        extension = parts[1];
                                    }
                                    filename = parts[0];

                                    var filex =
                                        {
                                            Data: fileData,
                                            FileName: filename,
                                            MimeType: theFile.type,
                                            FileExtension: extension,
                                            Size: theFile.size
                                        };
                                    resolve(filex);
                                };
                            })(file);
                            reader.readAsBinaryString(file);
                            //reader.readAsDataURL(file);
                        }).catch(function (err)
                        {
                            console.error(err);
                            mainApp.handleError(err);
                        });
                    }
                    else
                    {
                        return Promise.resolve(null);
                    }
                    break;
                default:
                    return Promise.resolve(value);
            }
        }

        self.fileSelected = function (file)
        {
            self.setInputValue(file);
            self.propertyChanged();
        };

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

        self.initialized = false;
        self.viewModel = null;

        self.initialize = function (defaultValue)
        {
            return dialog.showBusyDialog("Initializing...").then(function ()
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
                            self.viewModel = model;
                            self.html(model.html());

                            model.applyKoBindings();

                            model.updateViewData(inputSetting.ViewForInput.Id, defaultValue);
                        }).catch(function (err)
                        {
                            mainApp.handleError(err);
                        });
                        break;
                    case 6: // Date
                        if (defaultValue != null && defaultValue.length > 0)
                        {
                            self.setInputValue(defaultValue);
                        }
                        break;
                    case 10: // Numeric
                        self.setInputValue(defaultValue);
                        break;
                    default:
                        self.setInputValue(defaultValue);
                        break;
                }

                self.propertyChanged(defaultValue);

                return dialog.closeBusyDialog();
            }).catch(function (err)
            {
                console.error(err);
                mainApp.handleError(err);
            });
            
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


function listSourceItemModel(isSelected, displayValue, itemValue)
{
    var self = this;
    self.selected = ko.observable(isSelected);
    self.label = ko.observable(displayValue);
    self.value = itemValue;
}