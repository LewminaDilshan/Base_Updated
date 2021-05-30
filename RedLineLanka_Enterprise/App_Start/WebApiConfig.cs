using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RedLineLanka_Enterprise
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Remove the XM Formatter from the web api
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "API/{controller}/{action}/{id}",
                defaults: new { action = "Get", id = RouteParameter.Optional }
            );
        }
    }
}
