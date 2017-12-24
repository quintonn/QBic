using Unity;
using System;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class BackgroundTest : BackgroundEvent
    {
        public BackgroundTest(IUnityContainer container)
            : base(container)
        {

        }

        public override string Description
        {
            get
            {
                return "Background test";
            }
        }

        public override DateTime CalculateNextRunTime(DateTime? lastRunTime)
        {
            //return DateTime.Now.AddMinutes(0.2);
            return DateTime.Now.AddDays(10);
        }

        public override void DoWork()
        {
            Console.WriteLine("whooo");
        }

        public override EventNumber GetId()
        {
            return 325;
        }

        public override bool RunImmediatelyFirstTime
        {
            get
            {
                return false;
            }
        }

        public override bool RunSynchronously
        {
            get
            {
                return false;
            }
        }
    }
}