namespace QBic.Core.Models
{
    /// <summary>
    /// This class is so we can use reflection to find only the dynamic classes.
    /// This is used when dynamically mapping classes to NHibernate using FluentMapping.
    /// </summary>
    public class DynamicClass : BaseClass
    {
    }
}