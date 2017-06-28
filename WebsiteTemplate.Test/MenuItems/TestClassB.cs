namespace WebsiteTemplate.Test.MenuItems
{
    public class TestClassB : TestClassA
    {
        public override int BaseValue
        {
            get
            {
                return 10;
            }
        }

        public virtual byte[] FileData { get; set; }
    }
}