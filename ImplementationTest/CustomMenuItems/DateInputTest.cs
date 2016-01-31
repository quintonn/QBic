using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace ImplementationTest.CustomMenuItems
{
    public class DateInputTest : GetInput
    {
        public override string Description
        {
            get
            {
                return "Date Input Test";
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1)
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name"));
                list.Add(new DateInput("Date", "Date"));

                return list;
            }
        }

        public override int GetId()
        {
            return 900;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            return new InitializeResult(true);
        }

        //public override async Task<IList<Event>> ProcessAction()
        //{
        //    var date = inputData["Date"];

        //    return new List<Event>()
        //        {
        //            new ShowMessage("Date = " + date),
        //            new CancelInputDialog(),
        //        };
        //}

        public override Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            throw new NotImplementedException();
        }
    }
}