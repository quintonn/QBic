var inputDialog = {

    loadInputPage: function (pageName, callback)
    {
        menuBuilder.clearNode('dlgInput');
        document.getElementById('inputContent').style.display = 'inline';

        callback = callback || function () { };
        var newCallback = function ()
        {
            callback();
        };

        navigation.loadHtmlBody('dlgInput', pageName, newCallback);
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
    },

    cancelMessage: function ()
    {
        document.getElementById('messageContent').style.display = 'none';
        menuBuilder.clearNode('dlgMessage');
    },

    showMessage: function (message)
    {
        /// TODO: Need to store the current state of the input dialog 
        menuBuilder.clearNode('dlgMessage');

        message = message || "";

        var message = message.replace(/\n/g, "<br/>");

        document.getElementById('messageContent').style.display = 'inline';
        
        navigation.loadHtmlBody('dlgMessage', "MessageDialog.html", function ()
        {
            var title = document.getElementById('msgPageTitle');
            title.innerHTML = "Message";
            var inputTable = document.getElementById('msgTable');

            var row1 = document.createElement('tr');
            var cell1 = document.createElement('td');
            cell1.colSpan = 2;
            cell1.innerHTML = message;
            cell1.style.textAlign = "center";
            row1.appendChild(cell1);
            inputTable.appendChild(row1);

            var buttonRow = document.createElement('tr');
            var buttonCell = document.createElement('td');
            buttonCell.colSpan = 2;
            buttonCell.style.textAlign = "center";

            var okButton = document.createElement('button');
            okButton.innerHTML = "OK";
            okButton.onclick = function ()
            {
                inputDialog.cancelMessage();
            };
            buttonCell.appendChild(okButton);
            buttonRow.appendChild(buttonCell);
            inputTable.appendChild(buttonRow);
        });
    },
};