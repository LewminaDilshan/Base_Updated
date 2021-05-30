using RedLineLanka_Enterprise.Areas.Base.Models;
using RedLineLanka_Enterprise.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedLineLanka_Enterprise.Areas.Base.Controllers
{
    public class DashBoardController : BaseController
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            ViewBag.DashBoardAction = this.ControllerContext.RouteData.Values["action"].ToString();
        }

        public ActionResult Home()
        {
            var vm = new DashBoardVM()
            {
            };
            return View(vm);
        }
    }
}