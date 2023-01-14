using NHibernate;
using QBic.Core.Models;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudMenuItem
    {
        Type InnerType { get; }

        EventNumber GetId();

        Dictionary<string, string> GetColumnsToShowInView();

        void ConfigureAdditionalColumns(ColumnConfiguration columnConfig);

        Dictionary<string, string> GetInputProperties();

        string UniquePropertyName { get; }

        EventNumber GetBaseMenuId();

        string GetBaseItemName();

        void OnModifyInternal(object item, bool isNew);

        void OnDeleteInternal(ISession session, object item);

        IQueryOver OrderQueryInternal(IQueryOver query);
    }

    public abstract class BasicCrudMenuItem<T> : Event, IBasicCrudMenuItem where T : BaseClass
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.CancelInputDialog;
            }
        }

        public Type InnerType
        {
            get
            {
                return typeof(T);
            }
        }

        public override string Description
        {
            get
            {
                return String.Empty;
            }
        }

        public override EventNumber GetId()
        {
            return 0;
        }

        public virtual string UniquePropertyName
        {
            get
            {
                return String.Empty;
            }
        }

        public IQueryOver OrderQueryInternal(IQueryOver query)
        {
            return OrderQuery(query as IQueryOver<T, T>);
        }

        public virtual IQueryOver<T> OrderQuery(IQueryOver<T, T> query)
        {
            return query;
        }

        /// <summary>
        /// The columns to show in the view. Value is Label and Key is Column Name.
        /// For DynamicClass fields, use 'field.subField' as the Key (e.g. User.Name)
        /// </summary>
        /// <returns>Columns to show in the view</returns>
        public abstract Dictionary<string, string> GetColumnsToShowInView();

        /// <summary>
        /// The fields to input when modifying the item.
        /// For DynamicClass fields, override their .ToString() methods, which is what is used as the display value in the combo box
        /// </summary>
        /// <returns>Dictionary of inputs and their labels on the screen. The second value is the input label, first is the key.</returns>
        public abstract Dictionary<string, string> GetInputProperties();

        public abstract EventNumber GetBaseMenuId();

        public abstract string GetBaseItemName();

        public virtual void ConfigureAdditionalColumns(ColumnConfiguration columnConfig)
        {
        }

        public void OnModifyInternal(object item, bool isNew)
        {
            OnModify(item as T, isNew);
        }

        public virtual void OnModify(T item, bool isNew)
        {

        }

        public void OnDeleteInternal(ISession session, object item)
        {
            OnDelete(session, item as T);
        }

        public virtual void OnDelete(ISession session, T item)
        {

        }
    }
}