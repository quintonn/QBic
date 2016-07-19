(function (views, $, undefined)
{
    views.showView = function (viewData)
    {
        console.log('showing view:');
        console.log(viewData);

        return mainApp.makeWebCall("frontend/pages/Views.html?v=" + mainApp.version).then(function (data)
        {
            var model = new viewModel('Test title', data, _applicationModel.views().length + 1);
            _applicationModel.addView(model);

            return Promise.resolve();
        });
    };

    function viewModel(title, html, id)
    {
        var self = this;

        self.myid = ko.observable(id);

        self.viewTitle = ko.observable(title);
        self.viewMenus = ko.observableArray([]);
        self.filterText = ko.observable('filter Text test');
        self.filterSearchClick = function ()
        {
            console.log('filter search clicked with value: ' + self.filterText());
        };
        self.columns = ko.observableArray([]);
        self.rows = ko.observableArray([]);

        self.html = ko.observable(html);
    }

}(window.views = window.views || {}, jQuery));