﻿namespace WebsiteTemplate.Menus.ViewItems
{
    public class HiddenColumn : ViewColumn
    {
        public HiddenColumn(string columnLabel, string columnName)
            : base(columnLabel, columnName)
        {
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ColumnType.Hidden;
            }
        }
    }
}