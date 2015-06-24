var inputDialog = {

    loadInputPage: function (pageName, callback)
    {
        document.getElementById('inputContent').style.display = 'inline';
        //$("#inputContent").css("display", "inline");

        callback = callback || function() { };

        navigation.loadHtmlBody('dlgInput', pageName + "?v="+main.version, callback);
    },

    cancelInput: function()
    {
        document.getElementById('inputContent').style.display = 'none';
    }
};