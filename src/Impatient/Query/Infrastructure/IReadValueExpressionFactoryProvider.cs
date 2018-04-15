using System.Collections.Generic;

namespace Impatient.Query.Infrastructure
{
    public interface IReadValueExpressionFactoryProvider
    {
        IEnumerable<IReadValueExpressionFactory> GetReadValueExpressionFactories(); 
    }
}
