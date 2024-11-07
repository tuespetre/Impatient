using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer;

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
        return (MemberInfo)propertyBase.GetReadablePropertyInfo() ?? propertyBase.GetReadableFieldInfo();
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
                return (MemberInfo)propertyBase.GetReadableFieldInfo()
                    ?? propertyBase.GetReadablePropertyInfo()
                    ?? throw new InvalidOperationException();
            }

            case PropertyAccessMode.PreferProperty:
            {
                return (MemberInfo)propertyBase.GetReadablePropertyInfo()
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
                return (MemberInfo)propertyBase.GetWritableFieldInfo()
                    ?? propertyBase.GetWritablePropertyInfo()
                    ?? throw new InvalidOperationException();
            }

            case PropertyAccessMode.PreferProperty:
            {
                return (MemberInfo)propertyBase.GetWritablePropertyInfo()
                    ?? propertyBase.GetWritableFieldInfo()
                    ?? throw new InvalidOperationException();
            }

            default:
            {
                throw new NotSupportedException();
            }
        }
    }

    private static PropertyInfo GetReadablePropertyInfo(this IPropertyBase propertyBase)
    {
        if (propertyBase.PropertyInfo?.CanRead is true)
        {
            return propertyBase.PropertyInfo.DeclaringType.GetProperty(propertyBase.PropertyInfo.Name, bindingFlags);
        }

        var foundProperty = propertyBase.DeclaringType.ClrType.GetProperty(propertyBase.Name, bindingFlags);

        return foundProperty?.CanRead is true ? foundProperty : null;
    }

    private static FieldInfo GetReadableFieldInfo(this IPropertyBase propertyBase)
    {
        return propertyBase.FieldInfo?.DeclaringType.GetField(propertyBase.FieldInfo.Name, bindingFlags);
    }

    private static PropertyInfo GetWritablePropertyInfo(this IPropertyBase propertyBase)
    {
        if (propertyBase.PropertyInfo?.CanWrite is true)
        {
            return propertyBase.PropertyInfo.DeclaringType.GetProperty(propertyBase.PropertyInfo.Name, bindingFlags);
        }

        var foundProperty = propertyBase.DeclaringType.ClrType.GetProperty(propertyBase.Name, bindingFlags);

        return foundProperty?.CanWrite is true ? foundProperty : null;
    }

    private static FieldInfo GetWritableFieldInfo(this IPropertyBase propertyBase)
    {
        return propertyBase.FieldInfo?.DeclaringType.GetField(propertyBase.FieldInfo.Name, bindingFlags);
    }

    public static IEnumerable<INavigation> FindDerivedNavigations(this IEntityType entityType, string name)
    {
        return entityType.GetDerivedTypes().Select(t => t.FindDeclaredNavigation(name)).Where(n => n != null);
    }

    public static INavigationBase FindNavigationBase(this IEntityType entityType, MemberInfo member)
    {
        INavigationBase navigation = entityType.FindNavigation(member);

        if (navigation is not null)
        {
            return navigation;
        }

        navigation = entityType.FindSkipNavigation(member);

        if (navigation is not null)
        {
            return navigation;
        }

        navigation
            = entityType
                .FindDerivedNavigations(member.Name)
                .SingleOrDefault(n => n.PropertyInfo == member || n.FieldInfo == member);

        return navigation;
    }

    public static IEntityType FindFirstEntityType(this IModel model, Type type)
    {
        return model.FindEntityTypes(type).FirstOrDefault();
    }
}
