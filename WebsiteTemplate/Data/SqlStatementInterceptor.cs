using NHibernate;

namespace WebsiteTemplate.Data
{
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            System.Diagnostics.Trace.WriteLine("??" + sql.ToString() + "??");
            //Console.WriteLine(sql.ToString());
            return sql;
        }
    }
}