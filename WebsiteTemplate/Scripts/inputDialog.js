var inputDialog = {

    loadInputPage: function (pageName, callback)
    {
        document.getElementById('inputContent').style.display = 'inline';
        //$("#inputContent").css("display", "inline");

        callback = callback || function() { };

        navigation.loadHtmlBody('dlgInput', pageName, callback);
    },

    cancelInput: function()
    {
        document.getElementById('inputContent').style.display = 'none';
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
};