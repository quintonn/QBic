namespace WebsiteTemplate.Menus.InputItems
{
    public interface IViewInputValue
    {
        /// <summary>
        /// Each row in a view input must have a unique rowId value and determines the ordering of items in the view
        /// </summary>
        public int rowId { get; set; }
    }
}
