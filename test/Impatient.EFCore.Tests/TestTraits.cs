using Xunit.Abstractions;
using Xunit.Sdk;

namespace Impatient.EFCore.Tests;

internal class TestCaseRewrittenAttribute() : CustomTraitAttribute("Category", "Rewritten");

internal class TranslationExceedsEFCoreAttribute() : CustomTraitAttribute("Translation", "Exceeds EF Core");
internal class DisagreeWithEFCoreAttribute() : CustomTraitAttribute("Translation", "Disagree with EF Core");

#region infrastructure

[TraitDiscoverer("Impatient.EFCore.Tests.CustomTraitDiscoverer", "Impatient.EFCore.Tests")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal abstract class CustomTraitAttribute : Attribute, ITraitAttribute
{
    public CustomTraitAttribute(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Name = name;
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }
}

internal class CustomTraitDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        if (traitAttribute is ReflectionAttributeInfo reflectionAttributeInfo
            && reflectionAttributeInfo.Attribute is CustomTraitAttribute customTraitAttribute)
        {
            yield return new(customTraitAttribute.Name, customTraitAttribute.Value);
        }
    }
}

#endregion