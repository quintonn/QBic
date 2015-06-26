var inputDialog = {

    loadInputPage: function (pageName, callback)
    {
        menuBuilder.clearNode('dlgInput');
        document.getElementById('inputContent').style.display = 'inline';

        callback = callback || function() { };

        navigation.loadHtmlBody('dlgInput', pageName, callback);
    },

    cancelInput: function()
    {
        document.getElementById('inputContent').style.display = 'none';
        menuBuilder.clearNode('dlgInput');
    },

    addHiddenField: function (fieldName, fieldValue)
    {
        var inputDiv = document.getElementById("dlgInput");
        var hidden = document.createElement("input");
        hidden.type = "hidden";
        hidden.value = fieldValue;
        hidden.id = fieldName;
        inputDiv.appendChild(hidden);
    },

    getHiddenField: function(fieldName)
    {
        var input = document.getElementById(fieldName);
        if (input == null)
        {
            return "";
        }
        return input.value;
    }
};