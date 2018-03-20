using Impatient.Metadata;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class QueryProcessingContext
    {
        public QueryProcessingContext(
            IQueryProvider queryProvider,
            DescriptorSet descriptorSet,
            IDictionary<object, ParameterExpression> parameterMapping,
            ParameterExpression executionContextParameter)
        {
            QueryProvider = queryProvider;
            DescriptorSet = descriptorSet;
            ParameterMapping = new ReadOnlyDictionary<object, ParameterExpression>(parameterMapping);
            ExecutionContextParameter = executionContextParameter;
        }

        public IQueryProvider QueryProvider { get; }

        public DescriptorSet DescriptorSet { get; }

        public IReadOnlyDictionary<object, ParameterExpression> ParameterMapping { get; }

        public ParameterExpression ExecutionContextParameter { get; }
    }
}
