using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Enumerable;
using static System.Math;

namespace Impatient
{
    public static class ValueTupleHelper
    {
        public static Type CreateTupleType(IEnumerable<Type> types)
        {
            var lastSize = types.Count() % 7;

            if (lastSize == 0)
            {
                lastSize = 7;
            }

            var initialType
                = lastSize == 1 ? typeof(ValueTuple<>)
                : lastSize == 2 ? typeof(ValueTuple<,>)
                : lastSize == 3 ? typeof(ValueTuple<,,>)
                : lastSize == 4 ? typeof(ValueTuple<,,,>)
                : lastSize == 5 ? typeof(ValueTuple<,,,,>)
                : lastSize == 6 ? typeof(ValueTuple<,,,,,>)
                : typeof(ValueTuple<,,,,,,>);

            types = types.Reverse().ToArray().AsEnumerable();

            var resultType
                = initialType.MakeGenericType(
                    types.Take(lastSize).Reverse().ToArray());

            types = types.Skip(lastSize);

            while (types.Any())
            {
                resultType
                    = typeof(ValueTuple<,,,,,,,>).MakeGenericType(
                        types.Take(7).Reverse().Append(resultType).ToArray());

                types = types.Skip(7);
            }

            return resultType;
        }

        public static NewExpression CreateNewExpression(Type type, IEnumerable<Expression> arguments)
        {
            var typeInfo = type.GetTypeInfo();
            var constructor = typeInfo.DeclaredConstructors.Single();
            var fields = typeInfo.DeclaredFields.ToArray();
            var newArguments = new List<Expression>(fields.Length);

            foreach (var argument in arguments.Take(Min(7, fields.Length)))
            {
                newArguments.Add(argument);
            }

            if (fields.Length == 8)
            {
                newArguments.Add(CreateNewExpression(fields[7].FieldType, arguments.Skip(7)));
            }

            return Expression.New(constructor, newArguments, fields);
        }

        public static Expression CreateMemberExpression(Type type, Expression expression, int index)
        {
            for (var i = 0; i < index / 7; i++)
            {
                var restField = type.GetRuntimeField("Rest");
                expression = Expression.MakeMemberAccess(expression, restField);
                type = restField.FieldType;
            }

            var itemField = type.GetTypeInfo().DeclaredFields.ElementAt(index % 7);

            return Expression.MakeMemberAccess(expression, itemField);
        }
    }
}
