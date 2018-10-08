namespace QBic.Core.Models
{
    /// <summary>
    /// This class is so we can use reflection to find only the dynamic classes.
    /// This is used when dynamically mapping classes to NHibernate using FluentMapping.
    /// </summary>
    public class DynamicClass : BaseClass, IDynamicClass
    {
        /// <summary>
        /// This value is used when Fluent NHibernate maps BaseClass objects and decides if the ID should be 
        /// generated or if it should be assigned. This is mostly used during a restore of a backup when the system needs to assign ID's.
        /// </summary>
        public static bool SetIdsToBeAssigned { get; set; } = false;
    }
}