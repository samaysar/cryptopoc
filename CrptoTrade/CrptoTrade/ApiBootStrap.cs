using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dependencies;
using CrptoTrade.Trading;
using Microsoft.Practices.Unity;
using Owin;

namespace CrptoTrade
{
    public class ApiBootStrap
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var di = new UnityContainer();
            var xchgs = new IExchange[]
            {
                //Pass it audthenticated client
                new GdaxExchange(new HttpClient(), 0)
                //Add new exchanges
            };

            di.RegisterInstance(new TradingFactory(xchgs));
            //.. more DI
            config.DependencyResolver = new UnityResolver(di);
            
            //other config related stuff... like cors, messageHandler, formatters etc

            app.UseWebApi(config);
        }
    }

    public class UnityResolver : IDependencyResolver
    {
        private readonly IUnityContainer _container;

        public UnityResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = _container.CreateChildContainer();
            return new UnityResolver(child);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _container.Dispose();
        }
    }
}