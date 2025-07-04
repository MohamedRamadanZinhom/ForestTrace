using System;

namespace MyLogger
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
