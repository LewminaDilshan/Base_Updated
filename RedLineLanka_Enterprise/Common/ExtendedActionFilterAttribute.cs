using RedLineLanka_Enterprise.Areas.Base.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RedLineLanka_Enterprise.Common
{
    public class ExtendedActionFilterAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cas = filterContext.ActionDescriptor.GetCustomAttributes(true);
            
            if (cas.Where(x => x is AllowAnonymousAttribute).Count() == 0 && !filterContext.IsChildAction)
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session[BaseController.sskCurUsrID] == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                        { "action", "SignIn" },
                        { "controller", "Home" },
                        { "area", "Base" },
                        { "ReturnUrl", filterContext.GetLocalUrl() }
                    });
                    return;
                }
            }
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.IsChildAction)
            {
                HttpStatusCodeResult httpStatusCodeResult = filterContext.Result as HttpStatusCodeResult;
                int statusCode;
                if (httpStatusCodeResult != null && (statusCode = httpStatusCodeResult.StatusCode).In(404, 400))
                {
                    HttpContext.Current.Server.TransferRequest("~/Base/Home/Error/" + statusCode, true);
                    filterContext.ExceptionHandled = true;
                }
            }
            base.OnActionExecuted(filterContext);
        }

        public void OnException(ExceptionContext filterContext)
        {
            HttpException httpException = filterContext.Exception as HttpException;
            int statusCode;
            if (httpException != null && (statusCode = httpException.GetHttpCode()).In(404, 400))
            {
                HttpContext.Current.Server.TransferRequest("~/Base/Home/Error/" + statusCode, true);
                filterContext.ExceptionHandled = true;
            }
        }
    }
}