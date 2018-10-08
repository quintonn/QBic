namespace QBic.Core.Models
{
    public interface IDynamicClass
    {
        string Id { get; set; }

        bool CanDelete { get; set; }
    }
}
