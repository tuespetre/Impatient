using System;

namespace Impatient.Query.Expressions
{
    public abstract class SetOperatorTableExpression : AliasedTableExpression
    {
        private const string SetOperatorAlias = "set";

        public SetOperatorTableExpression(SelectExpression set1, SelectExpression set2)
            : base(SetOperatorAlias, set1?.Type ?? typeof(object))
        {
            Set1 = set1 ?? throw new ArgumentNullException(nameof(set1));
            Set2 = set2 ?? throw new ArgumentNullException(nameof(set2));

            if (set1.Type != set2.Type)
            {
                throw new ArgumentException("set1.Type and set2.Type must match");
            }
        }

        public SelectExpression Set1 { get; }

        public SelectExpression Set2 { get; }
    }
}
