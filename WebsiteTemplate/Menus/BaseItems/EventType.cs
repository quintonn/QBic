namespace WebsiteTemplate.Menus.BaseItems
{
    public enum EventType
    {
        DataView = 0,
        UserInput = 1,
        SubMenu = 2,
        DoSomething = 3,
        CancelInputDialog = 4,
        ShowMessage = 5,
        ExecuteAction = 6,
        InputDataView = 7,
        UpdateInputView = 8,
        DeleteInputViewItem = 9,
        UpdateComboBox = 10,
        ViewFile = 11,
        UpdateInput = 12,
        Logout = 13,
        ListView = 14, // todo: delete this (was for mobile view)
        DetailView = 15, //TODO; delete code related to this (was for mobile view)
        BackgroundEvent = 16,
        UpdateInputVisibility = 17
    }
}