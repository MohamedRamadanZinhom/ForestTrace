using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ForestTrace.Business
{
    public class TreeExecutionLogger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static AsyncLocal<ExecutionContext?> _currentContext = new();
        private Dictionary<string, object> _globalProperties = new();
        public void AddGlobalProperty(string key, object value)
        {
            _globalProperties[key] = value;
        }

        public void Start(string methodName, Dictionary<string, object>? props = null)
        {
            var traceId = _currentContext.Value?.TraceId ?? Guid.NewGuid().ToString();
            var parent = _currentContext.Value;


            var context = new ExecutionContext(methodName, traceId)
            {
                Properties = props,
                Parent = parent
            };

            context.Properties = new Dictionary<string, object>(_globalProperties);

            if (props != null)
            {
                foreach (var kv in props)
                {
                    context.Properties[kv.Key] = kv.Value;
                }
            }

            parent?.Children.Add(context);
            _currentContext.Value = context;
        }

        public void Log(string message , Dictionary<string, object>? props = null)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add(message);
            }
            _logger.Info(message);
        }

        public void End(bool saveJson = true, bool saveGraph = true)
        {
            var context = _currentContext.Value;
            if (context == null) return;

            context.EndTime = DateTime.Now;

            if (context.Parent == null)
            {
                if (saveJson)
                    SaveJson(context);

                if (saveGraph)
                    SaveMermaid(context);
            }

            _currentContext.Value = context.Parent;
        }

        private void SaveJson(ExecutionContext root)
        {
            var logsDir = $"logs/{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(logsDir);

            var filePath = Path.Combine(logsDir, $"tree_all_{DateTime.Now:yyyyMMdd}.json");

            List<ExecutionContext> allTrees = new();

            if (File.Exists(filePath))
            {
                var existingJson = File.ReadAllText(filePath);
                allTrees = JsonSerializer.Deserialize<List<ExecutionContext>>(existingJson) ?? new List<ExecutionContext>();
            }

            allTrees.Add(root);

            var newJson = JsonSerializer.Serialize(allTrees, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, newJson);

            _logger.Info($"Appended tree log to {filePath}");
        }


        private void SaveMermaid(ExecutionContext root)
        {
            var safeDate = DateTime.Now.ToString("yyyyMMdd");
            var lines = new List<string> { "graph TD" };
            BuildMermaid(root, lines, null);

            var logsDir = $"logs/{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(logsDir);

            var fileName = Path.Combine(logsDir, $"LogTree_{safeDate}.mmd");
            File.WriteAllLines(fileName, lines);
            _logger.Info($"Mermaid graph saved: {fileName}");
        }

        private void BuildMermaid(ExecutionContext node, List<string> lines, string? parentId)
        {
            var nodeId = node.TraceId.Substring(0, 8) + "_" + node.Name.Replace(" ", "_");
            var label = $"{node.Name}\\n{node.DurationMs?.ToString("F1")}ms";

            if (parentId != null)
                lines.Add($"{parentId} --> {nodeId}[\"{label}\"]");

            foreach (var log in node.Logs)
            {
                var logId = Guid.NewGuid().ToString("N").Substring(0, 8);
                lines.Add($"{nodeId} --> {logId}[\"Log: {EscapeMermaid(log)}\"]");
            }

            foreach (var child in node.Children)
            {
                BuildMermaid(child, lines, nodeId);
            }
        }

        private string EscapeMermaid(string text)
        {
            return text.Replace("\"", "\\\"");
        }

        public void LogInfo(string message, Dictionary<string, object>? props = null)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add($"INFO: {message}");
            }
            _logger.Info(message);
        }

        public void LogError(string message, Dictionary<string, object>? props = null)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add($"ERROR: {message}");
            }
            _logger.Error(message);
        }

        public void LogDebug(string message, Dictionary<string, object>? props = null)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add($"DEBUG: {message}");
            }
            _logger.Debug(message);
        }
    }
}
