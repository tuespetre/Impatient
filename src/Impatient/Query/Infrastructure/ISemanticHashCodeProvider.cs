namespace Impatient.Query.Infrastructure
{
    public interface ISemanticHashCodeProvider
    {
        int GetSemanticHashCode(ExpressionEqualityComparer comparer);
    }
}
