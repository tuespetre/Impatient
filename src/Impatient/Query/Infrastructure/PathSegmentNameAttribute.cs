using System;

namespace Impatient.Query.Infrastructure
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class PathSegmentNameAttribute : Attribute
    {
        public PathSegmentNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
