using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal static class Extensions
    {
        private const BindingFlags bindingFlags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        /// <summary>
        /// Returns the first matching readable property, or the first matching readable field.
        /// This should be used for general expression manipulations.
        /// </summary>
        public static MemberInfo GetSemanticReadableMemberInfo(this IPropertyBase propertyBase)
        {
            return propertyBase.GetReadablePropertyInfo() ?? propertyBase.GetReadableFieldInfo();
        }

        /// <summary>
        /// Returns the first matching readable member as determined by <see cref="PropertyAccessMode"/>.
        /// This should be used only for accessing entity values during materialization, only if necessary.
        /// </summary>
        public static MemberInfo GetReadableMemberInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase is null)
            {
                throw new ArgumentNullException(nameof(propertyBase));
            }

            switch (propertyBase.GetPropertyAccessMode())
            {
                case PropertyAccessMode.Field:
                case PropertyAccessMode.FieldDuringConstruction:
                {
                    return propertyBase.GetReadableFieldInfo()
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.Property:
                {
                    return propertyBase.GetReadablePropertyInfo() 
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.PreferField:
                case PropertyAccessMode.PreferFieldDuringConstruction:
                {
                    return propertyBase.GetReadableFieldInfo()
                        ?? propertyBase.GetReadablePropertyInfo()
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.PreferProperty:
                {
                    return propertyBase.GetReadablePropertyInfo()
                        ?? propertyBase.GetReadableFieldInfo()
                        ?? throw new InvalidOperationException();
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static MemberInfo GetWritableMemberInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase is null)
            {
                throw new ArgumentNullException(nameof(propertyBase));
            }

            switch (propertyBase.GetPropertyAccessMode())
            {
                case PropertyAccessMode.Field:
                case PropertyAccessMode.FieldDuringConstruction:
                {
                    return propertyBase.GetWritableFieldInfo()
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.Property:
                {
                    return propertyBase.GetWritablePropertyInfo()
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.PreferField:
                case PropertyAccessMode.PreferFieldDuringConstruction:
                {
                    return propertyBase.GetWritableFieldInfo()
                        ?? propertyBase.GetWritablePropertyInfo()
                        ?? throw new InvalidOperationException();
                }

                case PropertyAccessMode.PreferProperty:
                {
                    return propertyBase.GetWritablePropertyInfo()
                        ?? propertyBase.GetWritableFieldInfo()
                        ?? throw new InvalidOperationException();
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static MemberInfo GetReadablePropertyInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase.PropertyInfo?.CanRead is true)
            {
                return propertyBase.PropertyInfo.DeclaringType.GetProperty(propertyBase.PropertyInfo.Name, bindingFlags);
            }

            var foundProperty = propertyBase.DeclaringType.ClrType.GetProperty(propertyBase.Name, bindingFlags);

            return foundProperty?.CanRead is true ? foundProperty : null;
        }

        private static MemberInfo GetReadableFieldInfo(this IPropertyBase propertyBase)
        {
            return propertyBase.FieldInfo?.DeclaringType.GetField(propertyBase.FieldInfo.Name, bindingFlags);
        }

        private static MemberInfo GetWritablePropertyInfo(this IPropertyBase propertyBase)
        {
            if (propertyBase.PropertyInfo?.CanWrite is true)
            {
                return propertyBase.PropertyInfo.DeclaringType.GetProperty(propertyBase.PropertyInfo.Name, bindingFlags);
            }

            var foundProperty = propertyBase.DeclaringType.ClrType.GetProperty(propertyBase.Name, bindingFlags);

            return foundProperty?.CanWrite is true ? foundProperty : null;
        }

        private static MemberInfo GetWritableFieldInfo(this IPropertyBase propertyBase)
        {
            return propertyBase.FieldInfo?.DeclaringType.GetField(propertyBase.FieldInfo.Name, bindingFlags);
        }
    }
}
