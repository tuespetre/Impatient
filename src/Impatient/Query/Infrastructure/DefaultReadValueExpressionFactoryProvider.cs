using System.Collections.Generic;

namespace Impatient.Query.Infrastructure
{
    public class DefaultReadValueExpressionFactoryProvider : IReadValueExpressionFactoryProvider
    {
        public IEnumerable<IReadValueExpressionFactory> GetReadValueExpressionFactories()
        {
            yield return new DefaultScalarReadValueExpressionFactory();

            // TODO: Pull this from the Default provider.
            yield return new SqlServerForJsonReadValueExpressionFactory();
        }
    }
}
