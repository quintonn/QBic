namespace WebsiteTemplate.Backend.TestItems
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