using System.Collections.Generic;

namespace Impatient.Query.Infrastructure
{
    public class DefaultReadValueExpressionFactoryProvider : IReadValueExpressionFactoryProvider
    {
        private readonly ITypeMappingProvider typeMappingProvider;

        public DefaultReadValueExpressionFactoryProvider(
            ITypeMappingProvider typeMappingProvider)
        {
            this.typeMappingProvider = typeMappingProvider;
        }

        public IEnumerable<IReadValueExpressionFactory> GetReadValueExpressionFactories()
        {
            yield return new DefaultScalarReadValueExpressionFactory(typeMappingProvider);

            // TODO: Pull this from the Default provider.
            yield return new SqlServerForJsonReadValueExpressionFactory(typeMappingProvider);
        }
    }
}
