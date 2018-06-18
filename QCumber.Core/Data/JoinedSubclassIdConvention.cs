using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace QCumber.Core.Data
{
    public class JoinedSubclassIdConvention : IJoinedSubclassConvention, IJoinedSubclassConventionAcceptance
    {
        public void Apply(IJoinedSubclassInstance instance)
        {
            instance.Key.Column("Id");
        }

        public void Accept(IAcceptanceCriteria<IJoinedSubclassInspector> criteria)
        {
            criteria.Expect(x => true);
        }
    }
}