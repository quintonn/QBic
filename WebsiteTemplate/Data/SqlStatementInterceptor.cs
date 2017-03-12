using NHibernate;

namespace WebsiteTemplate.Data
{
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            //System.Diagnostics.Trace.WriteLine("??" + sql.ToString() + "??");
            //System.Diagnostics.Debug.WriteLine(sql.ToString());
            //System.Console.WriteLine(sql.ToString());
            return sql;
        }
    }
}