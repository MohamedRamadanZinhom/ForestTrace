using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace MyLogger
{
    public class TreeExecutionLogger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static AsyncLocal<ExecutionContext?> _currentContext = new();

        public void Start(string methodName, Dictionary<string, object>? props = null)
        {
            var traceId = _currentContext.Value?.TraceId ?? Guid.NewGuid().ToString();
            var parent = _currentContext.Value;

            var context = new ExecutionContext(methodName, traceId)
            {
                Properties = props,
                Parent = parent
            };

            parent?.Children.Add(context);
            _currentContext.Value = context;
        }

        public void Log(string message)
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
            var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var fileName = $"logs/tree_{root.TraceId}_{DateTime.Now.ToShortDateString()}.json";
            Directory.CreateDirectory("logs");
            File.WriteAllText(fileName, json);
            _logger.Info($"JSON tree log saved: {fileName}");
        }

        private void SaveMermaid(ExecutionContext root)
        {
            var lines = new List<string> { "graph TD" };
            BuildMermaid(root, lines, null);
            var fileName = $"logs/tree_{root.TraceId}.mmd";
            Directory.CreateDirectory("logs");
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
                lines.Add($"{nodeId} --> {logId}[\"Log: {log}\"]");
            }

            foreach (var child in node.Children)
            {
                BuildMermaid(child, lines, nodeId);
            }
        }

        public void LogInfo(string message)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add($"INFO: {message}");
            }
            _logger.Info(message);
        }

        public void LogError(string message)
        {
            var context = _currentContext.Value;
            if (context != null)
            {
                context.Logs.Add($"ERROR: {message}");
            }
            _logger.Error(message);
        }

        public void LogDebug(string message)
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
