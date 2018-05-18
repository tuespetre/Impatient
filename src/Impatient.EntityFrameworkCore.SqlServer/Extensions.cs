using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal static class Extensions
    {
        public static MemberInfo GetReadableMemberInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase.PropertyInfo != null && propertyBase.PropertyInfo.CanRead)
            {
                return propertyBase.PropertyInfo.DeclaringType.GetProperty(
                    propertyBase.PropertyInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            var foundProperty
                = propertyBase.DeclaringType.ClrType.GetProperty(
                    propertyBase.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (foundProperty != null && foundProperty.CanRead)
            {
                return foundProperty;
            }

            if (propertyBase.FieldInfo != null)
            {
                return propertyBase.FieldInfo.DeclaringType.GetField(
                    propertyBase.FieldInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return null;
        }

        public static MemberInfo GetWritableMemberInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase.PropertyInfo != null && propertyBase.PropertyInfo.CanWrite)
            {
                return propertyBase.PropertyInfo.DeclaringType.GetProperty(
                    propertyBase.PropertyInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            var foundProperty
                = propertyBase.DeclaringType.ClrType.GetProperty(
                    propertyBase.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (foundProperty != null && foundProperty.CanWrite)
            {
                return foundProperty;
            }

            if (propertyBase.FieldInfo != null)
            {
                return propertyBase.FieldInfo.DeclaringType.GetField(
                    propertyBase.FieldInfo.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return null;
        }
    }
}
