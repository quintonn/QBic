(function (processing, $, undefined)
{
    processing.processUIActionResult = function (data, eventId)
    {
        for (var i = 0; i < data.length; i++)
        {
            var item = data[i];

            var actionType = item.ActionType;

            switch (actionType)
            {
                case 0:
                    return views.showView(item);
                default:
                    return dialog.showMessage("Error", "Unknown action type: " + actionType + " for event " + eventId);
            }
        }
    };
}(window.processing = window.processing || {}, jQuery));