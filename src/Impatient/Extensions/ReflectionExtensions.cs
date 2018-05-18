using Impatient.Query.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Extensions
{
    public static class ReflectionExtensions
    {
        #region Type extensions

        public static IEnumerable<MemberInfo> FindAllInstanceMembers(this Type type)
        {
            foreach (var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (member is FieldInfo || member is PropertyInfo)
                {
                    yield return member;
                }
            }

            if (type.GetTypeInfo().BaseType is Type baseType)
            {
                foreach (var member in FindAllInstanceMembers(baseType))
                {
                    yield return member;
                }
            }
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type UnwrapNullableType(this Type type)
        {
            if (type.IsNullableType())
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                return type;
            }
        }

        public static Type MakeNullableType(this Type type)
        {
            if (type.IsNullableType())
            {
                return type;
            }

            if (type.GetTypeInfo().IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        public static bool IsBooleanType(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static bool IsCollectionType(this Type type)
        {
            return type.IsGenericType(typeof(ICollection<>));
        }

        public static bool IsSequenceType(this Type type)
        {
            return type != typeof(string) && type.IsGenericType(typeof(IEnumerable<>));
        }

        public static Type GetSequenceType(this Type type)
        {
            return type.FindGenericType(typeof(IEnumerable<>))?.GenericTypeArguments[0];
        }

        public static Type MakeEnumerableType(this Type elementType)
        {
            return typeof(IEnumerable<>).MakeGenericType(elementType);
        }

        public static bool IsGenericType(this Type type, Type definition)
        {
            return type.FindGenericType(definition) != null;
        }

        public static Type FindGenericType(this Type type, Type definition)
        {
            if (type == null || type == typeof(object))
            {
                return null;
            }
            else if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == definition)
            {
                return type;
            }

            var definitionTypeInfo = definition.GetTypeInfo();

            if (definitionTypeInfo.IsInterface)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsConstructedGenericType && interfaceType.GetGenericTypeDefinition() == definition)
                    {
                        return interfaceType;
                    }
                }

                return null;
            }
            else
            {
                GetBaseType:

                var baseType = type.GetTypeInfo().BaseType;

                if (baseType == null || baseType == typeof(object))
                {
                    return null;
                }
                else if (baseType.IsConstructedGenericType && baseType.GetGenericTypeDefinition() == definition)
                {
                    return baseType;
                }
                else
                {
                    goto GetBaseType;
                }
            }
        }

        // TODO: Have all usages of IsScalarType/IsNumericType/IsTextType/etc. consult with an ITypeMapper instead

        public static bool IsScalarType(this Type type)
        {
            if (scalarTypes.Contains(type) || type.IsEnum())
            {
                return true;
            }

            if (type.IsNullableType())
            {
                var underlyingType = Nullable.GetUnderlyingType(type);

                return scalarTypes.Contains(underlyingType) || underlyingType.IsEnum();
            }

            return false;
        }

        private static readonly Type[] scalarTypes =
        {
            // Value types
            typeof(long), // bigint
            typeof(int), // int
            typeof(short), // smallint
            typeof(byte), // tinyint
            typeof(bool), // bit
            typeof(decimal), // decimal, money, numeric, smallmoney
            typeof(double), // float
            typeof(float), // real
            typeof(TimeSpan), // time
            typeof(DateTime), // datetime2, datetime, date, smalldatetime
            typeof(DateTimeOffset), // datetimeoffset
            typeof(Guid), // uniqueidentifer

            typeof(char), 

            // Reference types
            typeof(string), // nvarchar, varchar, nchar, char, ntext, text
            typeof(byte[]), // varbinary, binary, FILESTREAM, image, rowversion, timestamp

            // Unmapped types
            // - sql_variant
            // - xml

            // Supported enum types (for future reference):
            // - byte
            // - short
            // - int
            // - long
            // - string (explicitly)

            // Hmmmm
            typeof(uint),
            typeof(sbyte),
            typeof(ushort),
            typeof(ulong),
        };

        public static bool IsTimeType(this Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan);
        }

        public static bool IsTextType(this Type type)
        {
            return type == typeof(string)
                || type == typeof(char)
                || type == typeof(char?);
        }

        public static bool IsNumericType(this Type type)
        {
            return numericTypes.Contains(type.UnwrapNullableType());
        }

        private static readonly Type[] numericTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(short),
            typeof(uint),
            typeof(int),
            typeof(ulong),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        public static bool IsConstantLiteralType(this Type type)
        {
            return Array.IndexOf(constantLiteralTypes, type) > -1;
        }

        private static readonly Type[] constantLiteralTypes =
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(char),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
            typeof(sbyte?),
            typeof(byte?),
            typeof(short?),
            typeof(ushort?),
            typeof(int?),
            typeof(uint?),
            typeof(long?),
            typeof(ulong?),
            typeof(char?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?),
            typeof(bool?),
        };

        #endregion

        #region 

        public static FieldInfo FindBackingField(this PropertyInfo property)
        {
            var fields = property.DeclaringType.FindAllInstanceMembers().OfType<FieldInfo>();

            return fields.FirstOrDefault(f => f.Name == $"<{property.Name}>k__BackingField")
                ?? fields.FirstOrDefault(f => f.Name == $"<{property.Name}>i__Field")
                ?? fields.FirstOrDefault(f => f.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                ?? fields.FirstOrDefault(f => f.Name.Equals($"_{property.Name}", StringComparison.OrdinalIgnoreCase))
                ?? fields.FirstOrDefault(f => f.Name.Equals($"__{property.Name}", StringComparison.OrdinalIgnoreCase))
                ?? fields.FirstOrDefault(f => f.Name.Equals($"m_{property.Name}", StringComparison.OrdinalIgnoreCase));
        }

        public static MethodInfo GetGenericMethodDefinition<TArg, TResult>(Expression<Func<TArg, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();
        }

        public static MethodInfo GetMethodInfo<TResult>(Expression<Func<TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static bool HasSelector(this MethodInfo methodInfo)
        {
            switch (methodInfo?.Name)
            {
                case nameof(Queryable.Select):
                case nameof(Queryable.SelectMany):
                case nameof(Queryable.Join):
                case nameof(Queryable.GroupJoin):
                case nameof(Queryable.GroupBy):
                case nameof(Queryable.Zip):
                {
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }

        public static bool HasResultSelector(this MethodInfo methodInfo)
        {
            switch (methodInfo?.Name)
            {
                case nameof(Queryable.Select):
                case nameof(Queryable.Join):
                case nameof(Queryable.GroupJoin):
                case nameof(Queryable.Zip):
                {
                    return true;
                }

                case nameof(Queryable.SelectMany):
                case nameof(Queryable.GroupBy):
                {
                    return methodInfo.GetParameters().Any(p => p.Name == "resultSelector");
                }

                default:
                {
                    return false;
                }
            }
        }

        public static bool HasElementSelector(this MethodInfo methodInfo)
        {
            switch (methodInfo?.Name)
            {
                case nameof(Queryable.GroupBy):
                {
                    return methodInfo.GetParameters().Any(p => p.Name == "elementSelector");
                }

                default:
                {
                    return false;
                }
            }
        }

        public static bool IsQueryableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(Queryable);
        }

        public static bool IsEnumerableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(Enumerable);
        }

        public static bool IsQueryableOrEnumerableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.IsQueryableMethod() || methodInfo.IsEnumerableMethod();
        }

        public static bool IsOrderingMethod(this MethodInfo methodInfo)
        {
            return methodInfo.IsQueryableOrEnumerableMethod()
                && (methodInfo.Name == nameof(Queryable.OrderBy)
                    || methodInfo.Name == nameof(Queryable.OrderByDescending)
                    || methodInfo.Name == nameof(Queryable.ThenBy)
                    || methodInfo.Name == nameof(Queryable.ThenByDescending));
        }

        public static bool HasComparerArgument(this MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Select(p => p.ParameterType)
                .Any(t => t.IsGenericType(typeof(IEqualityComparer<>))
                    || t.IsGenericType(typeof(IComparer<>)));
        }

        public static string GetPathSegmentName(this MemberInfo memberInfo)
        {
            if (memberInfo is null)
            {
                return null;
            }

            return memberInfo.IsDefined(typeof(PathSegmentNameAttribute))
                ? memberInfo.GetCustomAttribute<PathSegmentNameAttribute>().Name
                : memberInfo.Name;
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                {
                    return propertyInfo.PropertyType;
                }

                case FieldInfo fieldInfo:
                {
                    return fieldInfo.FieldType;
                }

                default:
                {
                    return typeof(void);
                }
            }
        }

        private static bool TypesMatch(Type type1, Type type2)
        {
            if (type1 == type2)
            {
                return true;
            }
            else if (type1.IsConstructedGenericType && type2.IsConstructedGenericType)
            {
                var genericType1 = type1.GetGenericTypeDefinition();
                var genericType2 = type2.GetGenericTypeDefinition();

                return genericType1 == genericType2
                    && type1.GenericTypeArguments.Zip(type2.GenericTypeArguments, TypesMatch).All(b => b);
            }
            else if (type1.IsGenericParameter && type2.IsGenericParameter)
            {
                return type1.Name == type2.Name
                    && type1.GenericParameterPosition == type2.GenericParameterPosition;
            }
            else
            {
                return false;
            }
        }

        private static Type GetEnumerableEquivalentParameterType(ParameterInfo parameterInfo)
        {
            var type = parameterInfo.ParameterType;

            if (type.IsConstructedGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Expression<>))
                {
                    return type.GenericTypeArguments[0];
                }
                else if (genericTypeDefinition == typeof(IQueryable<>))
                {
                    return typeof(IEnumerable<>).MakeGenericType(type.GenericTypeArguments[0]);
                }
                else if (genericTypeDefinition == typeof(IOrderedQueryable<>))
                {
                    return typeof(IOrderedEnumerable<>).MakeGenericType(type.GenericTypeArguments[0]);
                }
                else
                {
                    return type;
                }
            }
            else if (type == typeof(IQueryable))
            {
                return typeof(IEnumerable);
            }
            else
            {
                return type;
            }
        }

        public static MethodInfo MatchQueryableMethod(MethodInfo method)
        {
            if (method.IsEnumerableMethod())
            {
                return method;
            }
            else if (!method.IsGenericMethod)
            {
                return (from m in typeof(Enumerable).GetTypeInfo().DeclaredMethods
                        where m.Name == method.Name
                            && m.ReturnType == method.ReturnType
                        let p1 = method.GetParameters()
                        let p2 = m.GetParameters()
                        where p1.Length == p2.Length
                        select m).FirstOrDefault();
            }

            var genericMethodDefinition = method.GetGenericMethodDefinition();

            var genericArguments = genericMethodDefinition.GetGenericArguments();

            var parameterTypes
                = genericMethodDefinition
                    .GetParameters()
                    .Select(GetEnumerableEquivalentParameterType)
                    .ToArray();

            var matching = (from m in enumerableMethods

                            where m.Name == method.Name

                            let parameters = m.GetParameters()
                            where parameters.Length == parameterTypes.Length
                            where m.GetParameters().Select(p => p.ParameterType).Zip(parameterTypes, TypesMatch).All(b => b)

                            let arguments = m.GetGenericArguments()
                            where arguments.Length == genericArguments.Length
                            where arguments.Zip(genericArguments, TypesMatch).All(b => b)

                            select m).ToList();

            var match = matching.SingleOrDefault();

            if (match != null)
            {
                return match.MakeGenericMethod(method.GetGenericArguments());
            }

            return method;
        }

        private static readonly IEnumerable<MethodInfo> enumerableMethods
            = typeof(Enumerable).GetTypeInfo().DeclaredMethods;

        public static IEnumerable<string> GetPropertyNamesForJson(this IEnumerable<MemberInfo> members)
        {
            foreach (var member in members)
            {
                var segment = member.Name;

                var attribute = member.GetCustomAttribute<JsonPropertyAttribute>(true);

                if (attribute != null)
                {
                    segment = attribute.PropertyName ?? segment;
                }

                yield return segment;
            }
        }

        #endregion
    }
}
