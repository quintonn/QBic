using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestInputView : ViewForInput
    {
        public override string Description
        {
            get
            {
                return "Test View";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddStringColumn("Value", "Value");
        }

        //public override IEnumerable GetData(GetDataSettings settings)
        //{
        //    var result = new List<object>();
        //    for (var i = 0; i < 10; i++)
        //    {
        //        result.Add(new
        //        {
        //            Name = "Item " + i,
        //            Value = i
        //        });
        //    }
        //    return result;
        //}

        //public override int GetDataCount(GetDataSettings settings)
        //{
        //    return 10;
        //}

        public override EventNumber GetId()
        {
            return new EventNumber(5368);
        }
    }
}