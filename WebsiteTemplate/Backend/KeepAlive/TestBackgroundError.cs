using Unity;
using System;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.KeepAlive
{
    public class TestBackgroundError : BackgroundEvent
    {
        public TestBackgroundError(IUnityContainer container)
            : base(container)
        {
        }

        public override string Description
        {
            get
            {
                return "Background Error Testing Process";
            }
        }

        public override bool RunSynchronously
        {
            get
            {
                return false;
            }
        }

        public override bool RunImmediatelyFirstTime
        {
            get
            {
                return false;
            }
        }

        public override DateTime CalculateNextRunTime(DateTime? lastRunTime)
        {
            //return DateTime.Now.AddMinutes(0.5);
            return DateTime.Now.AddDays(10);
        }

        public override void DoWork()
        {
            throw new Exception("Abort");
            Console.WriteLine("Hello");
        }

        public override EventNumber GetId()
        {
            return 5000;
        }
    }
}