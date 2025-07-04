using System;
using System.Collections.Generic;

namespace MyLogger
{
    public class ExecutionContext
    {
        public string TraceId { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? DurationMs => EndTime.HasValue ? (EndTime.Value - StartTime).TotalMilliseconds : null;
        public List<ExecutionContext> Children { get; set; } = new();
        public List<string> Logs { get; set; } = new();
        public Dictionary<string, object>? Properties { get; set; }
        public ExecutionContext? Parent { get; set; }

        public ExecutionContext(string name, string traceId)
        {
            Name = name;
            TraceId = traceId;
            StartTime = DateTime.Now;
        }
    }
}
