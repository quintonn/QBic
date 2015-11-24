namespace WebsiteTemplate.SiteSpecific
{
    public enum UserRole
    {
        //Admin = 0,
        ViewUsers = 0,
        SendConfirmationEmail = 1,
        AddUser = 2,
        ViewUserRoleAssociations = 3,
        ViewMenus = 4,

        AnyOne = 999, /// Any user can do this...
    }
}