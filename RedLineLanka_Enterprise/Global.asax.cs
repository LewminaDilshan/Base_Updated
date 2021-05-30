using RedLineLanka_Enterprise.Areas.Base.Controllers;
using RedLineLanka_Enterprise.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace RedLineLanka_Enterprise
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure();
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || authCookie.Value == "")
            { return; }

            FormsAuthenticationTicket authTicket;
            try { authTicket = FormsAuthentication.Decrypt(authCookie.Value); }
            catch
            { return; }

            var jser = new JavaScriptSerializer();
            var dct = jser.Deserialize<Dictionary<string, object>>(authTicket.UserData);

            if (dct == null)
            { return; }
            Context.User = new GenericPrincipal(new GenericIdentity(dct["userName"].ToString(), "Forms"), ((ArrayList)dct["roles"]).Cast<string>().ToArray());
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            bool isAjaxCall = string.Equals("XMLHttpRequest", Context.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
            if (isAjaxCall)
            { return; }

            Exception lastExcp = Server.GetLastError();
            int? errCode = null;

            Exception excp = lastExcp.GetBaseException();

            string stackEntryDelimiter = " at ";
            string topStackEntry = String.Empty;
            string stackTrace = excp.StackTrace;
            try
            {
                int nextStackEntry = stackTrace.IndexOf(stackEntryDelimiter, stackEntryDelimiter.Length);
                if (nextStackEntry > 0)
                {
                    topStackEntry = stackTrace.Substring(0, nextStackEntry);
                }
                else
                {
                    topStackEntry = stackTrace;
                }
            }
            catch (Exception) { }

            try
            {
                string eventLogFormat = "Error in {0} User: {1} Error Message: {2} Line: {3}";
                string[] eventLogArgs = { Request.Url.ToString(), Context.User.Identity.Name, excp.Message, topStackEntry };
                string logMessage = String.Format(eventLogFormat, eventLogArgs);

                log4net.ILog logger = log4net.LogManager.GetLogger(this.GetType());
                logger.Error(logMessage);
            }
            catch (Exception) { }

            Server.ClearError();

            if (HttpContext.Current.User.Identity.IsAuthenticated)
                Server.TransferRequest("~/Base/Home/Error/" + errCode, true);
            else
                HttpContext.Current.Response.Redirect("~/Base/Home/SignIn");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = 60;
        }
    }
}
