using System;

namespace ForestTrace.Business
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TrackExecutionAttribute : Attribute
    {
        public string? Name { get; set; }

        public TrackExecutionAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
