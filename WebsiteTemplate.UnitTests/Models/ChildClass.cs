namespace WebsiteTemplate.UnitTests.Models
{
    public class ChildClass : ParentClass
    {
        public virtual int Number { get; set; }

        public override int BaseValue => 10;
    }
}
