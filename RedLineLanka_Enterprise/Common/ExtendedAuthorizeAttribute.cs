using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RedLineLanka_Enterprise.Common
{
    public class ExtendedAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.HttpContext.User.Identity.IsAuthenticated &&
                filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "action", "AccessDenied" },
                    { "controller", "Home" },
                    { "area", "Base" },
                    { "ReturnUrl", filterContext.GetLocalUrl() }
                });
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!Roles.IsBlank() && !Roles.Split(',').Contains(UserRoles.Admin))
            { Roles = Roles + "," + UserRoles.Admin; }
            bool isUserAuthorized = base.AuthorizeCore(httpContext);
            return isUserAuthorized;
        }
    }

    public class ExtRole
    {
        public List<string> roles = new List<string>();

        public ExtRole(string role)
        {
            roles.Add(role.Trim());
        }
        public ExtRole(IEnumerable<string> roles)
        {
            this.roles.AddRange(roles);
        }
        public static implicit operator ExtRole(string x)
        {
            return new ExtRole(x);
        }
        public static implicit operator string(ExtRole x)
        {
            return string.Join(",", x.roles);
        }
        public static ExtRole operator |(ExtRole c1, ExtRole c2)
        {
            return new ExtRole( c1.roles.Concat(c2.roles));
        }

        public static bool operator ==(ExtRole c1, string c2)
        {
            return c1.roles.Contains(c2);
        }

        public static bool operator !=(ExtRole c1, string c2)
        {
            return !c1.roles.Contains(c2);
        }

        public override bool Equals(object o)
        {
            try
            {
                return roles.Contains(o.ToString());
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}