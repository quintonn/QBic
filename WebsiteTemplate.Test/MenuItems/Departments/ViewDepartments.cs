using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test.MenuItems.Departments
{
    public class ViewDepartments : CoreView<Department>
    {
        public ViewDepartments(DataService dataService) : base(dataService)
        {
        }

        public override bool AllowInMenu => true;

        public override string Description => "View Departments";

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");

            columnConfig.AddLinkColumn("", "Id", "Edit", MenuNumber.EditDepartment);
            columnConfig.AddButtonColumn("", "Id", "X", new UserConfirmation("Delete selected item?")
            {
                OnConfirmationUIAction = MenuNumber.DeleteDepartment
            });

            columnConfig.AddLinkColumn("", "Id", "Test", MenuNumber.TestGoogleDriveBackup);
        }

        public override List<Expression<Func<Department, object>>> GetFilterItems()
        {
            return new List<Expression<Func<Department, object>>>()
            {
                x => x.Name,
            };
        }

        public override EventNumber GetId()
        {
            return MenuNumber.ViewDepartments;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>()
            {
                new MenuItem("Add", MenuNumber.AddDepartment)
            };
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            return base.GetData(settings);
        }

        public override EventNumber DetailSectionId => MenuNumber.DepartmentDetailsSection;
    }
}
