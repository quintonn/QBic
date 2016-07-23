(function (inputDialog, $, undefined)
{
    inputDialog.buildInput = function (settings)
    {
        console.log(settings);

        var title = settings.Description;
        var buttons = settings.InputButtons;
        var data = settings.InputData; // Not sure what this is for - not suppose to be in client possibly.
        var inputs = settings.InputFields;

        var model = new inputDialogModel(title);

        var tabs = {};
        $.each(inputs, function (indx, inp)
        {
            //console.log(indx);
            //console.log(inp);
            
            var tabName = $.trim(inp.TabName || "");
            
            if (tabs[tabName] == null)
            {
                tabs[tabName] = new tabModel(tabName, model);
            }
            
            tabs[tabName].inputs.push(new inputFieldModel(inp.InputLabel, inp.DefaultValue));
        });

        $.each(tabs, function (indx, tab)
        {
            console.log(indx);
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
            console.log('current tab: ');
            console.log(tab);
            model.currentTab(tab);
        }

        dialog.showDialogWithId('InputDialog', model);

        return Promise.resolve();
    }

    function inputDialogModel(title)
    {
        var self = this;
        self.title = ko.observable(title);

        self.tabs = ko.observableArray([]);

        self.currentTab = ko.observable();
        self.combinedTab = ko.observable();

        self.buttons = ko.observableArray([]);

        self.closeClick = function ()
        {
            dialog.closeModalDialog();
        };
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
            console.log('setting current tab');
            self.inputDialogModel.currentTab(self);
        };

        self.colorClass = ko.computed(function ()
        {
            return self.selected() == true ? "w3-grey" : "w3-light-grey";
        }, self);

        self.inputs = ko.observableArray([]);
    }

    function inputFieldModel(label, defaultValue)
    {
        var self = this;
        self.inputLabel = ko.observable(label);
        self.inputValue = ko.observable(defaultValue);
    }

}(window.inputDialog = window.inputDialog || {}, jQuery));