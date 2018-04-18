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

        void AddParameter(Expression expression, Func<string, string> parameterNameFormatter);

        void AddDynamicParameter(string fragment, Expression expression, Func<string, string> parameterNameFormatter);

        LambdaExpression Build();
    }
}
