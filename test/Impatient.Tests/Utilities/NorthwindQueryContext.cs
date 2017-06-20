using Impatient.Tests.Northwind;
using System.Linq;
using System.Text;
using static Impatient.Tests.Utilities.QueryExpressionHelper;

namespace Impatient.Tests.Utilities
{
    public class NorthwindQueryContext
    {
        private readonly ImpatientQueryProvider impatient;

        private readonly StringBuilder commandLog = new StringBuilder();

        public NorthwindQueryContext(ImpatientQueryProvider impatient)
        {
            this.impatient = impatient;

            impatient.DbCommandInterceptor = command =>
            {
                if (commandLog.Length > 0)
                {
                    commandLog.AppendLine().AppendLine();
                }

                commandLog.Append(command.CommandText);
            };
        }

        public string SqlLog => commandLog.ToString();

        public IQueryable<Customer> Customers => impatient.CreateQuery<Customer>(CreateQueryExpression<Customer>());

        public IQueryable<Order> Orders => impatient.CreateQuery<Order>(CreateQueryExpression<Order>());

        public IQueryable<OrderDetail> OrderDetails => impatient.CreateQuery<OrderDetail>(CreateQueryExpression<OrderDetail>());

        public void ClearLog() => commandLog.Clear();
    }
}
