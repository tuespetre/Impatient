namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class QueryOptions
    {
        public QueryOptions(
            bool ignoreQueryFilters, 
            bool useTracking)
        {
            IgnoreQueryFilters = ignoreQueryFilters;
            UseTracking = useTracking;
        }

        public bool IgnoreQueryFilters { get; }

        public bool UseTracking { get; }
    }
}
