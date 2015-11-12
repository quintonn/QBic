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

    showMessage: function (settings, callback, args)
    {
        /// TODO: Need to store the current state of the input dialog 
        //        For eg, what I need to show a message box while on an input page
        menuBuilder.clearNode('dlgMessage');

        var message = settings.ConfirmationMessage;

        if (message == null)
        {
            message = settings;
        }

        message = message || "";
        //console.log("Message = " + JSON.stringify(message));
        message = message.toString().replace(/\n/g, "<br/>");

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

            var buttonAdded = false;

            /// Add the Confirmation button
            if (settings.ConfirmationButtonText != null && settings.ConfirmationButtonText.length > 0)
            {
                var okButtonEvent = function ()
                {
                    if (settings.OnConfirmationUIAction > -1)
                    {
                        siteMenu.executeUIAction(settings.OnConfirmationUIAction, args);
                    }
                    if (callback != null)
                    {
                        callback();
                    }
                };
                var buttonCell = inputDialog.createInputDialogButton(settings.ConfirmationButtonText, okButtonEvent);
                buttonRow.appendChild(buttonCell);
                
                buttonAdded = true;
            }


            /// Add the Cancel button
            if (settings.CancelButtonText != null && settings.CancelButtonText.length > 0)
            {
                var cancelButtonEvent = function ()
                {
                    if (settings.OnCancelUIAction > -1)
                    {
                        alert('2=' + settings.OnCancelUIAction);
                        siteMenu.executeUIAction(settings.OnCancelUIAction, args);
                    }
                    if (callback != null && buttonAdded == false)
                    {
                        callback();
                    }
                };

                var buttonCell = inputDialog.createInputDialogButton(settings.CancelButtonText, cancelButtonEvent);
                buttonRow.appendChild(buttonCell);
                buttonAdded = true;
            }

            if (buttonAdded == false)
            {
                var buttonEvent = function()
                {
                    inputDialog.cancelInput();
                    if (callback != null)
                    {
                        callback();
                    }
                }
                var buttonCell = inputDialog.createInputDialogButton("Ok", buttonEvent);
                buttonRow.appendChild(buttonCell);
            }
            
            inputTable.appendChild(buttonRow);
        });
    },

    createInputDialogButton: function(buttonText, callback)
    {
        var buttonCell = document.createElement('td');
        //buttonCell.colSpan = 2;
        buttonCell.style.textAlign = "center";

        var okButton = document.createElement('button');
        okButton.innerHTML = buttonText;
        okButton.onclick = function ()
        {
            inputDialog.cancelMessage();
            
            if (callback)
            {
                callback();
            }
        };
        buttonCell.appendChild(okButton);
        return buttonCell;
    },
};