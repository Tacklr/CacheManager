using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Web;
using TacklR.CacheManager.Controllers;
using TacklR.CacheManager.HttpHandlers;
using System.Collections.Specialized;

namespace TacklR.CacheManager
{
    internal class Route
    {
        internal Route(Type controller, string action, params Tuple<string, object>[] parameters)
        {
            Controller = controller;
            Action = action;
            Parameters = parameters.ToDictionary(p => p.Item1, p => p.Item2);
        }

        //internal Route(Type controller, string action, params KeyValuePair<string, object>[] parameters)
        //{
        //    Controller = controller;
        //    Action = action;
        //    //Parameters = parameters.ToList();//default?
        //    Parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
        //}

        internal Type Controller { get; set; }
        internal string Action { get; set; }
        internal IDictionary<string, object> Parameters { get; set; }
    }

    internal class RouteTable
    {
        private static Dictionary<string, Route> Routes { get; set; }

        internal RouteTable()
        {
            Routes = new Dictionary<string, Route>();
        }

        internal void MapRoute(string url, Route route)
        {
            Routes.Add(url.ToLowerInvariant(), route);
        }

        internal bool TryGetRoute(HttpRequest request, out Route route)
        {
            var type = request.RequestType;
            var path = (request.PathInfo ?? String.Empty).TrimStart('/');
            return Routes.TryGetValue((type + ":" + path).ToLowerInvariant(), out route);
        }
    }


    internal class CacheManagerViewFactory : IHttpHandlerFactory
    {
        internal static Assembly Assembly { get; set; }
        internal static RouteTable RouteTable { get; set; }//hold this someplace else? case sensitivity of urls/params?

        public CacheManagerViewFactory()
        {
            if (Assembly == null)//Do we need this check?
                Assembly = this.GetType().Assembly;

            //Query string handling? Model binding? We could do it in a simple way using json
            if (RouteTable == null)
            {
                HttpContext.Current.Cache.Add("This/Is-a/very/deep/entry/to-test/how/far/over/it/goes aaaaaaaaaaaaa aaaaaaaaaaaa aaaaaaaaaa", "derp", null, DateTime.UtcNow.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.AboveNormal, null);


                RouteTable = new RouteTable();
                //Does it matter of we instantiate here or not? might make a difference if we intend to pass constructor parameters per-request.
                RouteTable.MapRoute("GET:", new Route(typeof(ManagerController), "Index"));
                RouteTable.MapRoute("GET:api/v1/cache", new Route(typeof(ApiController), "Cache"));
                RouteTable.MapRoute("GET:api/v1/stats", new Route(typeof(ApiController), "Stats"));
                RouteTable.MapRoute("GET:api/v1/settings", new Route(typeof(ApiController), "Settings"));
                RouteTable.MapRoute("GET:api/v1/combined", new Route(typeof(ApiController), "Combined"));
                RouteTable.MapRoute("GET:api/v1/serialize", new Route(typeof(ApiController), "Serialize"));
                RouteTable.MapRoute("POST:api/v1/delete", new Route(typeof(ApiController), "Delete"));

                //Generic routes? parameters? can the server default on mime types?
                RouteTable.MapRoute("GET:bundles/combined.min.css", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "combined.min.css")));
                RouteTable.MapRoute("GET:bundles/combined.min.js", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "combined.min.js")));

                RouteTable.MapRoute("GET:content/img/favicon.ico", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "favicon.ico")));

                //TODO: Replace with trimmed down fontaweosme or other set.
                RouteTable.MapRoute("GET:content/fonts/fontawesome-webfont.eot", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "fontawesome-webfont.eot")));
                RouteTable.MapRoute("GET:content/fonts/fontawesome-webfont.svg", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "fontawesome-webfont.svg")));
                RouteTable.MapRoute("GET:content/fonts/fontawesome-webfont.ttf", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "fontawesome-webfont.ttf")));
                RouteTable.MapRoute("GET:content/fonts/fontawesome-webfont.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "fontawesome-webfont.woff")));

                RouteTable.MapRoute("GET:content/fonts/OpenSans-Bold.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans-Bold.woff")));
                RouteTable.MapRoute("GET:content/fonts/OpenSans-Extrabold.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans-Extrabold.woff")));
                RouteTable.MapRoute("GET:content/fonts/OpenSans-Italic.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans-Italic.woff")));
                RouteTable.MapRoute("GET:content/fonts/OpenSans-Light.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans-Light.woff")));
                RouteTable.MapRoute("GET:content/fonts/OpenSans-Semibold.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans-Semibold.woff")));
                RouteTable.MapRoute("GET:content/fonts/OpenSans.woff", new Route(typeof(ResourceHandler), "Resource", Tuple.Create<string, object>("name", "OpenSans.woff")));
            }
        }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            var request = context.Request;
            if (!request.IsLocal && !Configuration.AllowRemoteAccess)//More complex auth checks like elmah?
                return new ErrorController().Error403();

            Route handler;
            if (RouteTable.TryGetRoute(context.Request, out handler))
            {
                //this method instead?
                //var constructor = handler.Controller.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
                //var _controller = constructor.Invoke(null);
                var controller = Activator.CreateInstance(handler.Controller);
                var method = handler.Controller.GetMethod(handler.Action, BindingFlags.Instance | BindingFlags.ExactBinding | BindingFlags.NonPublic);
                if (controller != null && method != null)
                {
                    //What if we want query string params in a post request??
                    var collection = request.RequestType == "POST" ? request.Form : request.QueryString;

                    //really simplistic parameter binding
                    var methodParameters = new List<object>();
                    var parameters = method.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        var handlerValue = default(object);
                        var postValue = collection[parameter.Name];
                        if (postValue != null)
                            methodParameters.Add(Convert.ChangeType(postValue, parameter.ParameterType));
                        else if (handler.Parameters.TryGetValue(parameter.Name, out handlerValue))
                            methodParameters.Add(Convert.ChangeType(handlerValue, parameter.ParameterType));
                        else
                            methodParameters.Add(Type.Missing);
                    }

                    try
                    {
                        return method.Invoke(controller, methodParameters.ToArray()) as IHttpHandler;
                    }
                    catch (ArgumentException ex)
                    {
                        //bad arguments
                        return new ErrorController().Error500();
                    }
                }
            }

            return new ErrorController().Error404();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}