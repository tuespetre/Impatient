using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EFCore.Tests.Utilities
{
    public class ImpatientQueryAsserter : RelationalQueryAsserter
    {
        public ImpatientQueryAsserter(
            IQueryFixtureBase queryFixture, 
            Func<Expression, Expression> rewriteExpectedQueryExpression, 
            Func<Expression, Expression> rewriteServerQueryExpression, 
            bool ignoreEntryCount = false, 
            bool canExecuteQueryString = false) 
            : base(queryFixture, 
                  rewriteExpectedQueryExpression, 
                  rewriteServerQueryExpression, 
                  ignoreEntryCount, 
                  canExecuteQueryString)
        {
        }

        protected override void AssertRogueExecution(int expectedCount, IQueryable queryable)
        {
            // no-op. 😐
        }
    }
}
