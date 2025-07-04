using Castle.DynamicProxy;
using NLog;
using System;
using System.Collections.Generic;

namespace ForestTrace.Business
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
                    // Log full exception
                    _treeLogger.Log($"EXCEPTION: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                    throw; // rethrow to keep original behavior
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
