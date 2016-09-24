namespace WebsiteTemplate.Models
{
    /// <summary>
    /// This class is so we can use reflection to find only the dynamic classes.
    /// This is used when dynamically mapping classes to NHibernate using FluentMapping.
    /// </summary>
    public class DynamicClass : BaseClass
    {
        internal static bool SetIdsToBeAssigned { get; set; } = false;
    }
}