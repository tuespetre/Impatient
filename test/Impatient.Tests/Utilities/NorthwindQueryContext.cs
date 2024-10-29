using Impatient.Tests.Northwind;
using System.Linq;
using static Impatient.Tests.Utilities.QueryExpressionHelper;

namespace Impatient.Tests.Utilities
{
    public class NorthwindQueryContext
    {
        public const string ConnectionString = 
            @"Server=.\sqlexpress; Database=Northwind; Trusted_Connection=True; TrustServerCertificate=true";

        private readonly ImpatientQueryProvider impatient;
        private readonly TestDbCommandExecutorFactory executor;

        public NorthwindQueryContext(ImpatientQueryProvider impatient, TestDbCommandExecutorFactory executor)
        {
            this.impatient = impatient;
            this.executor = executor;
        }

        public string SqlLog => executor.Log.ToString();

        public IQueryable<Customer> Customers => impatient.CreateQuery<Customer>(CreateQueryExpression<Customer>());

        public IQueryable<Order> Orders => impatient.CreateQuery<Order>(CreateQueryExpression<Order>());

        public IQueryable<OrderDetail> OrderDetails => impatient.CreateQuery<OrderDetail>(CreateQueryExpression<OrderDetail>());

        public void ClearLog() => executor?.Log?.Clear();
    }
}
