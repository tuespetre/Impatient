using Impatient.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class QueryProcessingContext
    {
        private readonly IDictionary<Type, object> extensions = new Dictionary<Type, object>();

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

        public TExtension GetExtension<TExtension>() where TExtension : class
        {
            if (extensions.TryGetValue(typeof(TExtension), out var result))
            {
                return (TExtension)result;
            }

            return default;
        }

        public void SetExtension<TExtension>(TExtension extension) where TExtension : class
        {
            extensions[typeof(TExtension)] = extension;
        }
    }
}
