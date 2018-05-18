using Impatient.Query.Expressions;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IDbCommandExpressionBuilder
    {
        void Append(string commandText);

        void AppendLine();

        void IncreaseIndent();

        void DecreaseIndent();

        void StartCapture();

        string StopCapture();

        void AddParameter(SqlParameterExpression expression);

        void AddDynamicParameters(string fragment, Expression expression);

        LambdaExpression Build();
    }
}
