using Castle.DynamicProxy;
using MyLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestTrace.Business
{
    public static class ProxyFactory
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();

        public static T Create<T>(T instance, TreeExecutionLogger logger) where T : class
        {
            var interceptor = new ExecutionInterceptor(logger);
            return _generator.CreateInterfaceProxyWithTargetInterface(instance, interceptor);
        }
    }
}
