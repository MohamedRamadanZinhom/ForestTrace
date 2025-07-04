using Castle.DynamicProxy;
using NLog;
using System;
using System.Collections.Generic;

namespace MyLogger
{
    public class ExecutionInterceptor : IInterceptor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly TreeExecutionLogger _treeLogger;

        public ExecutionInterceptor(TreeExecutionLogger treeLogger)
        {
            _treeLogger = treeLogger;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var attr = method.GetCustomAttributes(typeof(TrackExecutionAttribute), true);

            if (attr.Length > 0)
            {
                string methodName = method.Name;
                _treeLogger.Start(methodName);

                try
                {
                    invocation.Proceed();
                }
                catch (Exception ex)
                {
                    _treeLogger.Log($"Exception: {ex.Message}");
                    throw;
                }
                finally
                {
                    _treeLogger.End();
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
