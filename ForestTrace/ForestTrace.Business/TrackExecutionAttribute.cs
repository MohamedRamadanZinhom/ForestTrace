using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Xml.Linq;

namespace ForestTrace.Business
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TrackExecutionAttribute : Attribute, IActionFilter
    {
        public string? Name { get; set; }

        public TrackExecutionAttribute(string? name = null)
        {
            Name = name;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(TreeExecutionLogger)) as TreeExecutionLogger;
            var methodName = !string.IsNullOrEmpty(Name) ? Name : context.ActionDescriptor.DisplayName;
            logger?.Start(methodName);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(TreeExecutionLogger)) as TreeExecutionLogger;
            logger?.End();
        }
    }
}
