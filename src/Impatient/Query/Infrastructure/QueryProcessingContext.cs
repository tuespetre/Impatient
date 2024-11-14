using Impatient.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class QueryProcessingContext
{
    public QueryProcessingContext(
        IQueryProvider queryProvider,
        DescriptorSet descriptorSet,
        ImpatientCompatibility compatibility)
    {
        QueryProvider = queryProvider;
        DescriptorSet = descriptorSet;
        Compatibility = compatibility;
        ParameterMapping = new Dictionary<object, ParameterExpression>();
    }

    public IQueryProvider QueryProvider { get; }

    public DescriptorSet DescriptorSet { get; }

    public ImpatientCompatibility Compatibility { get; }

    public IDictionary<object, ParameterExpression> ParameterMapping { get; }
}
