using RedLineLanka_Enterprise.Areas.Admin.Models;
using RedLineLanka_Enterprise.Areas.Base.Controllers;
using RedLineLanka_Enterprise.Areas.Base.Models;
using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.WebPages;

namespace System.Web
{
    public static class WebHelpers
    {
        public static Tentity GetEntity<Tentity, Tmodel>(this IModel<Tentity, Tmodel> mapper)
        {
            Type retTyp = typeof(Tentity);
            Tentity retObj = (Tentity)Activator.CreateInstance(retTyp);
            var tempModel = (IModel<Tentity, Tmodel>)Activator.CreateInstance(typeof(Tmodel));

            foreach (var prop in mapper.GetType().GetProperties())
            {
                if (!prop.CanRead || (prop.GetIndexParameters().Length > 0))
                { continue; }

                PropertyInfo other = retTyp.GetProperty(prop.Name);
                var val = prop.GetValue(mapper, null);

                if (IsAssignable(val, other))
                { other.SetValue(retObj, val); }
            }

            foreach (var map in tempModel.mappings)
            {
                var member = ((dynamic)map).entityProperty.Body as MemberExpression;
                if (member == null)
                {
                    var unary = ((dynamic)map).entityProperty.Body as UnaryExpression;
                    if (unary != null)
                    { member = unary.Operand as MemberExpression; }
                }
                if (member == null)
                { continue; }

                var propInfo = member.Member as PropertyInfo;
                if (propInfo == null)
                { continue; }

                object valObj = null;
                try { valObj = ((dynamic)map).modelProperty.Compile().Invoke((Tmodel)mapper); }
                catch { }

                if (propInfo.ReflectedType.Equals(retTyp) && IsAssignable(valObj, propInfo))
                { propInfo.SetValue(retObj, valObj); }
            }

            return retObj;
        }

        private static bool IsAssignable(object fromValue, PropertyInfo toPI)
        {
            if (fromValue == null || toPI == null || !toPI.CanWrite)
            { return false; }

            var fromType = fromValue.GetType();

            if (!toPI.PropertyType.IsAssignableFrom(fromType))
            {
                var toType = toPI.PropertyType;
                Type fromInnerType = null, toInnerType = null;
                if (toType.IsGenericType)
                { toInnerType = toType.GetGenericTypeDefinition(); }

                if (fromType.IsGenericType)
                { fromInnerType = fromType.GetGenericTypeDefinition(); }

                if (fromInnerType == null && toInnerType == null)
                { return false; }

                if (toInnerType != null &&
                    !toInnerType.IsAssignableFrom(fromType) &&
                    !toInnerType.IsAssignableFrom(fromInnerType))
                { return false; }

                if (!toType.IsAssignableFrom(fromType) &&
                    !toType.IsAssignableFrom(fromInnerType))
                { return false; }
            }

            return true;
        }

        public static void SetEntity<Tentity, Tmodel>(this IModel<Tentity, Tmodel> mapper, Tentity obj, params string[] properties)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (!prop.CanRead || (prop.GetIndexParameters().Length > 0))
                { continue; }

                PropertyInfo other = mapper.GetType().GetProperty(prop.Name);
                var val = prop.GetValue(obj, null);

                if (IsAssignable(val, other))
                { other.SetValue(mapper, val); }
            }

            foreach (var map in mapper.mappings)
            {
                var member = ((dynamic)map).modelProperty.Body as MemberExpression;
                if (member == null)
                {
                    var unary = ((dynamic)map).modelProperty.Body as UnaryExpression;
                    if (unary != null)
                    { member = unary.Operand as MemberExpression; }
                }
                if (member == null)
                { continue; }

                var propInfo = member.Member as PropertyInfo;
                if (propInfo == null)
                { continue; }

                if (properties.Count() > 0 && !properties.Contains(propInfo.Name))
                { continue; }

                object valObj = null;
                try { valObj = ((dynamic)map).entityProperty.Compile().Invoke(obj); }
                catch { }

                if (IsAssignable(valObj, propInfo))
                { propInfo.SetValue(mapper, valObj); }
            }
        }

        public static void LoadReferences<TEntity, TModel>(this IModel<TEntity, TModel> obj, params Expression<Func<TEntity, object>>[] exprs) where TEntity : class
        {
            using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
            {
                var ent = obj.GetEntity();
                var objCtxt = (dbctx as IObjectContextAdapter).ObjectContext;
                var entType = typeof(TEntity);

                var entBase = objCtxt.MetadataWorkspace.GetEntityContainer(objCtxt.DefaultContainerName, DataSpace.CSpace).BaseEntitySets.FirstOrDefault(x => x.ElementType.Name == entType.Name);

                var navProps = ((EntityType)entBase.ElementType).NavigationProperties.ToList();
                if (exprs.Length > 0)
                { navProps = navProps.Join(exprs, x => x.Name, x => ((x as LambdaExpression).Body as MemberExpression).Member.Name, (x, y) => x).ToList(); }

                foreach (var navProp in navProps)
                {
                    var depProps = navProp.GetDependentProperties();
                    if (depProps.Count() == 0)
                    { continue; }
                    object[] keyVals = depProps.Select(x => entType.GetProperty(x.Name).GetValue(ent)).ToArray();

                    var refEnt = dbctx.Set(Type.GetType(entType.Namespace + "." + navProp.ToEndMember.Name)).Find(keyVals);
                    entType.GetProperty(navProp.Name).SetValue(ent, refEnt);
                }
                obj.SetEntity(ent);
            }
        }

        public static string GetAreaName(this RouteBase route)
        {
            var irwa = route as IRouteWithArea;
            if (irwa != null)
            { return irwa.Area; }

            var objRoute = route as Route;
            if ((objRoute != null) && (objRoute.DataTokens != null))
            {
                return (objRoute.DataTokens["area"] as string);
            }
            return null;
        }

        public static string GetAreaName(this RouteData routeData)
        {
            object obj;
            if (routeData.DataTokens.TryGetValue("area", out obj))
            {
                return (obj as string);
            }
            return routeData.Route.GetAreaName();
        }

        public static string GetLocalUrl(this ControllerContext filterContext)
        {
            string qryStr = filterContext.HttpContext.Request.QueryString.Count > 0 ? "?" + filterContext.HttpContext.Request.QueryString : "";
            string area = !filterContext.RouteData.GetAreaName().IsBlank() ? "/" + filterContext.RouteData.GetAreaName() : "";
            string id = filterContext.Controller.ControllerContext.RouteData.Values.ContainsKey("id") ? "/" + filterContext.Controller.ControllerContext.RouteData.Values["id"].ToString() : "";

            var retStr = VirtualPathUtility.ToAbsolute(string.Format("~{0}/{1}/{2}{3}{4}", area, filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString(),
                filterContext.Controller.ControllerContext.RouteData.Values["action"], id, qryStr));
            return retStr;
        }

        public static WebGrid GetGrid<T>(this BaseViewModel<T> model, string ajaxContainer = null)
        {
            WebGrid grid;
            if (model.PageSize == 0)
            { grid = new WebGrid(canPage: false, canSort: true, ajaxUpdateContainerId: ajaxContainer, ajaxUpdateCallback: "DocReadyFunc"); }
            else
            { grid = new WebGrid(rowsPerPage: model.PageSize, canPage: true, canSort: true, ajaxUpdateContainerId: ajaxContainer, ajaxUpdateCallback: "DocReadyFunc"); }

            grid.Bind(source: (IEnumerable<dynamic>)model.objList, autoSortAndPage: false, rowCount: model.TotalRecords);
            grid.SortColumn = model.sort;
            grid.SortDirection = model.sortdir == "DESC" ? SortDirection.Descending : SortDirection.Ascending;
            return grid;
        }
    }
}

namespace System.Web.Mvc.Html
{
    public static class MvcHtmlHelpers
    {
        public static int GetCurUserID(this HtmlHelper htmlHelper)
        {
            HttpContext context = HttpContext.Current;
            return context.Session[BaseController.sskCurUsrID].ConvertTo<int>();
        }

        //public static Employee GetCurEmployee(this HtmlHelper htmlHelper)
        //{
        //    using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
        //    {
        //        var usr = dbctx.Users.Find(htmlHelper.GetCurUserID());
        //        return usr == null ? null : usr.Employee;
        //    }
        //}

        public static List<Menu> GetAllMenus(this HtmlHelper htmlHelper)
        {
            using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
            { return dbctx.Menus.ToList(); }
        }

        public static List<Menu> GetAccessibleMenus(this HtmlHelper htmlHelper)
        {
            List<Menu> lst = null;
            using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
            {
                lst = dbctx.Menus.ToList();
                var usrId = (int)htmlHelper.ViewContext.HttpContext.Session[BaseController.sskCurUsrID];
                lst = dbctx.Menus
                    .Where(x => x.RoleMenuAccesses
                    .Where(y => y.Role.UserRoles
                    .Where(z => z.UserID == usrId).Count() > 0).Count() > 0).ToList();
            }

            return lst;
        }

        public static UserVM GetCurrentUserVM(this HtmlHelper htmlHelper)
        {
            using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
            {
                var usrId = (int)htmlHelper.ViewContext.HttpContext.Session[BaseController.sskCurUsrID];
                User user = dbctx.Users.Find(usrId);
                return new UserVM(user);
            }
        }

        public static WebGridColumn SortColumn(this WebGrid grid, string columnName = null, string header = null, Func<dynamic, object> format = null, string style = null, bool canSort = true)
        {
            if (grid.SortColumn == columnName && grid.SortDirection == SortDirection.Ascending)
            {
                header += " ▲";
            }
            else if (grid.SortColumn == columnName && grid.SortDirection == SortDirection.Descending)
            {
                header += " ▼";
            }

            return grid.Column(columnName: columnName, header: header, format: format, style: style, canSort: canSort);
        }

        public static MvcHtmlString EnumEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var pi = GetPropertyInfo(expression);

            Type typ = null;
            if (pi.PropertyType.IsEnum)
            { typ = pi.PropertyType; }
            if (pi.PropertyType.IsGenericType &&
                pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                pi.PropertyType.GetGenericArguments()[0].IsEnum)
            { typ = pi.PropertyType.GetGenericArguments()[0]; }

            if (typ == null)
            { return htmlHelper.EditorFor(expression, new { htmlAttributes = htmlAttributes }); }

            TModel mdl = htmlHelper.ViewData.Model;
            var sval = mdl == null ? null : (object)expression.Compile()(mdl);

            var sel = new TagBuilder("select");
            sel.Attributes.Add("id", pi.Name);
            sel.Attributes.Add("name", pi.Name);

            object attrs = htmlAttributes.ToDynamic();
            foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
            { sel.Attributes.Add(kvp.Key, kvp.Value.ToString()); }
            StringBuilder sb = new StringBuilder();

            if (!pi.PropertyType.IsEnum)
            {
                var opt = new TagBuilder("option");
                opt.Attributes.Add("value", "");
                sb.Append(opt.ToString());
            }

            foreach (var enm in Enum.GetValues(typ))
            {
                var opt = new TagBuilder("option");
                opt.Attributes.Add("value", Convert.ToInt64(enm).ToString());
                if (sval != null && enm.Equals(Enum.Parse(typ, sval.ToString())))
                { opt.Attributes.Add("selected", "selected"); }
                opt.InnerHtml = enm.ToEnumChar();
                sb.Append(opt.ToString());
            }

            sel.InnerHtml = sb.ToString();
            return new MvcHtmlString(sel.ToString());
        }

        public static MvcHtmlString EnumDisplayFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var pi = GetPropertyInfo(expression);

            Type typ = null;
            if (pi.PropertyType.IsEnum)
            { typ = pi.PropertyType; }
            if (pi.PropertyType.IsGenericType &&
                pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                pi.PropertyType.GetGenericArguments()[0].IsEnum)
            { typ = pi.PropertyType.GetGenericArguments()[0]; }

            if (typ == null)
            { return htmlHelper.DisplayFor(expression); }

            TModel mdl = htmlHelper.ViewData.Model;
            var val = mdl == null ? null : (object)expression.Compile()(mdl);
            if (mdl == null || val == null)
            { return new MvcHtmlString(string.Empty); }

            return new MvcHtmlString(val.ToEnumChar());
        }

        public static MvcHtmlString DetailsViewFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string dataFormat = null)
        {
            var pi = GetPropertyInfo(expression);

            Type typEnum = null;
            if (pi.PropertyType.IsEnum)
            { typEnum = pi.PropertyType; }
            if (pi.PropertyType.IsGenericType &&
                pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                pi.PropertyType.GetGenericArguments()[0].IsEnum)
            { typEnum = pi.PropertyType.GetGenericArguments()[0]; }

            string txt = string.Empty;

            TModel mdl = htmlHelper.ViewData.Model;
            var val = mdl == null ? null : (object)expression.Compile()(mdl);
            if (val != null)
            {
                if (val is DateTime)
                {
                    var dt = (DateTime)val;
                    if (!dataFormat.IsBlank())
                    { txt = dt.ToString(dataFormat); }
                    else if (dt == dt.Date)
                    { txt = dt.ToString("yyyy-MM-dd"); }
                    else
                    { txt = dt.ToString("yyyy-MM-dd hh:mm tt"); }
                }
                else if (val is TimeSpan)
                {
                    var dt = DateTime.MinValue.Add((TimeSpan)val);
                    if (!dataFormat.IsBlank())
                    { txt = dt.ToString(dataFormat); }
                    else
                    { txt = dt.ToString("HH:mm"); }
                }
                else if (typEnum == null)
                { txt = val.ToString(); }
                else
                { txt = val.ToEnumChar(); }
            }

            var isMulti = pi.GetCustomAttributes(typeof(DataTypeAttribute)).Cast<DataTypeAttribute>().Where(x => x.DataType == DataType.MultilineText).FirstOrDefault() != null;
            TagBuilder t;
            if (isMulti)
            {
                t = new TagBuilder("textarea");
                t.InnerHtml = txt;
                t.Attributes.Add("class", "form-control");
                t.Attributes.Add("readonly", "readonly");
            }
            else if (val is bool)
            {
                t = new TagBuilder("input");
                t.Attributes.Add("type", "checkbox");
                if ((bool)val)
                { t.Attributes.Add("checked", "checked"); }
                t.Attributes.Add("class", "form-checkbox");
                t.Attributes.Add("readonly", "readonly");
            }
            else
            {
                t = new TagBuilder("input");
                t.Attributes.Add("value", txt);
                t.Attributes.Add("type", "text");
                t.Attributes.Add("class", "form-control");
                t.Attributes.Add("readonly", "readonly");
            }

            var expText = ExpressionHelper.GetExpressionText(expression);
            t.Attributes.Add("id", TagBuilder.CreateSanitizedId(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expText)));
            t.Attributes.Add("name", expText);

            return new MvcHtmlString(t.ToString());
        }

        public static MvcHtmlString ConfirmSubmitButton(this HtmlHelper htmlHelper, string btnText, string message = "Are you sure you want to continue?",
            string title = "Please confirm", string buttonText = "Yes", object htmlAttributes = null, string buttonClass = "btn-danger", string submitAction = null, string jsFunction = null, string contentSelector = null)
        {
            var inp = new TagBuilder("button");
            inp.Attributes.Add("type", "submit");
            //inp.Attributes.Add("value", btnText);
            inp.Attributes.Add("data-message", message);
            inp.Attributes.Add("data-title", title);
            inp.Attributes.Add("data-button-text", buttonText);
            inp.Attributes.Add("data-button-class", buttonClass);
            inp.Attributes.Add("data-submit-action", submitAction);
            inp.Attributes.Add("data-js-function", jsFunction);
            inp.Attributes.Add("data-content-selector", contentSelector);
            inp.InnerHtml = btnText;

            object attrs = htmlAttributes.ToDynamic();
            string cls = attrs.GetDynamicValue("class", "dlgConfirmSubmit").ToString();
            cls = cls + (cls == "dlgConfirmSubmit" || cls.Contains(" dlgConfirmSubmit") || cls.Contains("dlgConfirmSubmit ") ? "" : (cls.IsBlank() ? "" : " ") + "dlgConfirmSubmit");

            inp.Attributes.Add("class", cls);

            foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
            {
                var ky = kvp.Key;
                if (ky.ToLower().StartsWith("data_"))
                { ky = ky.Replace("_", "-"); }

                if (kvp.Key != "class")
                { inp.Attributes.Add(ky, kvp.Value.ToString()); }
            }
            return new MvcHtmlString(inp.ToString());
        }

        public static MvcHtmlString PopUpSelectorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression,
            string dataArea, string dataController, string dataAction, object htmlAttributes = null, string title = "Please select", object dataParas = null,
            string emptyText = "--Select--", string dspFormat = "{0} - {1}", string valueMember = null, int popUpWidth = 500, string hiddenIndices = null)
        {
            var pi = GetPropertyInfo(expression);

            TModel mdl = htmlHelper.ViewData.Model;

            var s = new TagBuilder("select");
            var opt = new TagBuilder("option");

            object defVal = null;
            if (pi.PropertyType.IsValueType)
            { defVal = Activator.CreateInstance(pi.PropertyType); }
            object val = null;

            try { val = ((Func<Func<TModel, TProperty>>)Expression.Lambda(expression).Compile())()(mdl); }
            catch { }

            string selectedJsonObj = "";
            if (mdl == null || val == null || val.ConvertTo<string>() == defVal.ConvertTo<string>())
            {
                opt.InnerHtml = emptyText;
                opt.Attributes.Add("value", "");
            }
            else
            {
                opt.InnerHtml = GetDisplayText(dataController, dataAction, dataParas, val.ToString(), dspFormat, valueMember, emptyText, out selectedJsonObj);
                opt.Attributes.Add("value", val.ToString());
                opt.Attributes.Add("selected", "selected");
            }
            s.InnerHtml = opt.ToString();
            var expText = ExpressionHelper.GetExpressionText(expression);
            s.Attributes.Add("id", TagBuilder.CreateSanitizedId(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expText)));
            s.Attributes.Add("name", expText);

            object attrs = htmlAttributes.ToDynamic();
            string cls = attrs.GetDynamicValue("class", "dlgPopUpSelector").ToString();
            cls = cls + (cls == "dlgPopUpSelector" || cls.Contains(" dlgPopUpSelector") || cls.Contains("dlgPopUpSelector ") ? "" : (cls.IsBlank() ? "" : " ") + "dlgPopUpSelector");

            s.Attributes.Add("class", cls);
            s.Attributes.Add("data-para-json", new JavaScriptSerializer().Serialize(dataParas));
            s.Attributes.Add("data-selected-item", selectedJsonObj);
            s.Attributes.Add("data-empty-text", emptyText);
            s.Attributes.Add("data-dsp-format", dspFormat);
            s.Attributes.Add("data-popup-width", popUpWidth.ToString());
            if (!valueMember.IsBlank())
            { s.Attributes.Add("data-value-member", valueMember); }
            s.Attributes.Add("data-title", "<span class=\"fas fa-info-circle\" style=\"font-size:x-large;color:blue\"></span> " + title);
            s.Attributes.Add("data-hidden-indices", hiddenIndices);
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            RouteValueDictionary rvals = new RouteValueDictionary();
            rvals.Add("area", dataArea);
            string datUrl = urlHelper.Action(dataAction, dataController, rvals);

            rvals.Clear();
            rvals.Add("area", "Base");
            rvals.Add("dataUrl", datUrl);
            s.Attributes.Add("data-url", urlHelper.Action("GetPopUpSelector", "Base", rvals));

            foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
            {
                if (kvp.Key != "class")
                { s.Attributes.Add(kvp.Key, kvp.Value.ToString()); }
            }

            return new MvcHtmlString(s.ToString());
        }

        private static string GetDisplayText(string dataController, string dataAction, object dataParas, string value, string dspFormat, string valueMember, string emptyText, out string selectedJsonObj)
        {
            var typs = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Name.ToUpper().Contains((dataController + "Controller").ToUpper()));
            dynamic obj = null;
            var jser = new JavaScriptSerializer();
            foreach (var typ in typs)
            {
                var mInfo = typ.GetMethod(dataAction);
                if (mInfo == null)
                { continue; }

                var methodParameters = mInfo.GetParameters();
                var lstPara = new List<object>();
                var dctPara = (IDictionary<string, object>)dataParas.ToDynamic();

                foreach (var mPara in methodParameters)
                {
                    object defVal = null;
                    if (mPara.HasDefaultValue)
                    { defVal = mPara.DefaultValue; }
                    else if (mPara.ParameterType.IsValueType)
                    { defVal = Activator.CreateInstance(mPara.ParameterType); }

                    if (dctPara.ContainsKey(mPara.Name))
                    { lstPara.Add(dctPara[mPara.Name].ConvertTo(mPara.ParameterType, defVal)); }
                    else if (mPara.Name == "filter")
                    { lstPara.Add(value); }
                    else if (mPara.Name == "searchForKey")
                    { lstPara.Add(true); }
                    else
                    { lstPara.Add(defVal); }
                }

                obj = mInfo.Invoke(Activator.CreateInstance(typ), lstPara.ToArray());
                break;
            }

            selectedJsonObj = "";

            if (obj == null)
            { return emptyText; }

            var dct = jser.Deserialize<Dictionary<string, object>>(jser.Serialize(obj.Data));
            var lst = (ArrayList)((Dictionary<string, object>)dct)["Data"];
            var itm = lst.Cast<Dictionary<string, object>>().Where(x => (valueMember.IsBlank() ? x.Values.ElementAt(0) : x[valueMember]).ToString() == value).FirstOrDefault();

            if (itm == null)
            { return emptyText; }

            selectedJsonObj = jser.Serialize(itm);
            StringBuilder sb = new StringBuilder(dspFormat);

            for (int i = 0; i < itm.Count; i++)
            { sb.Replace("{" + i + "}", itm.Values.ElementAt(i).ConvertTo<string>()); }

            foreach (var item in itm)
            { sb.Replace("{" + item.Key + "}", item.Value.ConvertTo<string>()); }

            return sb.ToString();
        }

        public static MvcHtmlString NoEncodeActionLink(this HtmlHelper htmlHelper, string text, string title, string action, string controller = null,
            object routeValues = null, object htmlAttributes = null)
        {
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            TagBuilder builder = new TagBuilder("a");
            builder.InnerHtml = text;
            builder.Attributes["title"] = title;
            builder.Attributes["href"] = controller == null ? urlHelper.Action(action, routeValues) : urlHelper.Action(action, controller, routeValues);
            builder.MergeAttributes(new RouteValueDictionary(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)));

            return MvcHtmlString.Create(builder.ToString());
        }

        public static string ActionUrl(this HtmlHelper htmlHelper, string action, string controller = null, object routeValues = null)
        {
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            return controller == null ? urlHelper.Action(action, routeValues) : urlHelper.Action(action, controller, routeValues);
        }

        public static MvcHtmlString WrappedHtmlString(this HtmlHelper htmlHelper, string text, string seperator = ",", object htmlAttributes = null)
        {
            var tokens = (text ?? "").Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);

            return MvcHtmlString.Create(string.Join("<br>", tokens));
        }

        public static bool PropertyExists(this object obj, string property)
        {
            var dyn = obj as DynamicObject;
            if (dyn == null)
            { return false; }

            return dyn.GetDynamicMemberNames().Contains(property);
        }

        private static PropertyInfo GetPropertyInfo<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                var unary = expression.Body as UnaryExpression;
                if (unary != null)
                { member = unary.Operand as MemberExpression; }
            }
            if (member == null)
            { throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", expression.ToString())); }

            var pi = member.Member as PropertyInfo;
            if (pi == null)
            { throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", expression.ToString())); }

            return pi;
        }

        public static MvcHtmlString ThreeStateCheckBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null,
            string trueVal = "true", string falseVal = "false", string indeterminateVal = "")
        {
            var pi = GetPropertyInfo(expression);

            var cb = new TagBuilder("input");
            cb.Attributes.Add("type", "checkbox");
            cb.Attributes.Add("data-hf-name", pi.Name);
            cb.Attributes.Add("data-true-val", trueVal);
            cb.Attributes.Add("data-false-val", falseVal);
            cb.Attributes.Add("data-indet-val", indeterminateVal);

            var hf = new TagBuilder("input");
            hf.Attributes.Add("type", "hidden");
            hf.Attributes.Add("id", pi.Name);
            hf.Attributes.Add("name", pi.Name);

            object attrs = htmlAttributes.ToDynamic();
            foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
            { cb.Attributes.Add(kvp.Key, kvp.Value.ToString()); }

            TModel mdl = htmlHelper.ViewData.Model;
            var val = mdl == null ? null : pi.GetValue(mdl, null);
            var convVal = Convert.ToString(val).ToUpper();
            if (convVal == trueVal.ToUpper())
            { cb.Attributes.Add("checked", "checked"); }

            hf.Attributes.Add("value", convVal == trueVal.ToUpper() ? trueVal : convVal == falseVal.ToUpper() ? falseVal : indeterminateVal);

            return new MvcHtmlString(cb.ToString() + hf.ToString());
        }

        public static MvcHtmlString ExtEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool isEditable, object additionalViewData)
        {
            if (isEditable)
            { return htmlHelper.EditorFor(expression, additionalViewData); }
            else
            {
                IDictionary<string, object> newAttrs = new ExpandoObject();
                object attrs = additionalViewData.ToDynamic();
                foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
                { newAttrs.Add(kvp.Key, kvp.Key != "class" ? kvp.Value : kvp.Value.ToString().Replace("form-control", "")); }

                return htmlHelper.DisplayFor(expression, newAttrs);
            }
        }

        public static MvcHtmlString ExtEditor<TModel>(this HtmlHelper<TModel> htmlHelper, string name, object value, bool isEditable, object htmlAttributes)
        {
            if (isEditable)
            { return htmlHelper.TextBox(name, value, htmlAttributes); }
            else
            {
                IDictionary<string, object> newAttrs = new ExpandoObject();
                object attrs = htmlAttributes.ToDynamic();
                foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
                { newAttrs.Add(kvp.Key, kvp.Key != "class" ? kvp.Value : kvp.Value.ToString().Replace("form-control", "")); }

                return htmlHelper.Label(name, value.ToString(), newAttrs);
            }
        }

        public static MvcHtmlString CreateChart(this HtmlHelper htmlHelper, string canvasId, ChartConfig chartConfig, params string[] functions)
        {
            var tag = new TagBuilder("script");
            tag.Attributes.Add("type", "text/javascript");
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("function {0}_draw() {{", canvasId);
            stringBuilder.AppendFormat("var ctx = document.getElementById(\"{0}\").getContext(\"2d\");", canvasId);
            stringBuilder.AppendFormat("var config = JSON.parse('{0}');", chartConfig.SerializeToJson());

            foreach (var function in functions)
            {
                stringBuilder.Append("config." + function + ";");
            }
            stringBuilder.AppendFormat("var {0}_chart = new Chart(ctx, config);", canvasId);
            stringBuilder.AppendFormat("}} {0}_draw();", canvasId);
            tag.InnerHtml = stringBuilder.ToString();
            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString ImageCaptor(this HtmlHelper htmlHelper, string imageSource)
        {
            var vid = new TagBuilder("video");
            vid.Attributes.Add("id", "vidCam");
            vid.Attributes.Add("width", "240");
            vid.Attributes.Add("height", "180");
            vid.Attributes.Add("autoplay", "");
            vid.Attributes.Add("style", "display:none");

            var cnvs = new TagBuilder("canvas");
            cnvs.Attributes.Add("id", "vidCamCanvas");
            cnvs.Attributes.Add("width", "240");
            cnvs.Attributes.Add("height", "180");
            cnvs.Attributes.Add("style", "display:none");

            var imgDiv = new TagBuilder("div");
            imgDiv.Attributes.Add("style", "text-align:center;width:240px");
            var img = new TagBuilder("img");
            img.Attributes.Add("id", "imgPic");
            img.Attributes.Add("class", "btn");
            img.Attributes.Add("src", imageSource);
            img.Attributes.Add("style", "height:180px;");
            imgDiv.InnerHtml = img.ToString();

            var btnToolBar = new TagBuilder("div");
            btnToolBar.Attributes.Add("class", "btn-toolbar");
            btnToolBar.Attributes.Add("style", "min-width:240px;margin-top:5px;text-align:center;");

            var btnCam = new TagBuilder("input");
            btnCam.Attributes.Add("type", "button");
            btnCam.Attributes.Add("id", "btnCam");
            btnCam.Attributes.Add("class", "btn btn-success");
            btnCam.Attributes.Add("style", "min-width:75px; float:none");
            btnCam.Attributes.Add("data-mode", "R");
            btnCam.Attributes.Add("value", "RETAKE");

            var btnCancel = new TagBuilder("input");
            btnCancel.Attributes.Add("type", "button");
            btnCancel.Attributes.Add("id", "btnCancel");
            btnCancel.Attributes.Add("class", "btn btn-danger");
            btnCancel.Attributes.Add("style", "min-width:75px; float:none; display:none");
            btnCancel.Attributes.Add("value", "CANCEL");

            btnToolBar.InnerHtml = btnCam.ToString() + btnCancel.ToString();

            var script = new TagBuilder("script");
            script.Attributes.Add("type", "text/javascript");

            var str = @"
                var objVidCam = $('#vidCam');
                var objVidCamCanvas = document.getElementById('vidCamCanvas');
                var objBtnCam = $('#btnCam');
                var objBtnCancel = $('#btnCancel');
                var objPic = $('#imgPic');

                objPic.data('org-src', objPic.attr('src'));

                function AttachVideoListener(video)
                {
                    var mediaConfig = { video: true };
                    // Put video listeners into place
                    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                        navigator.mediaDevices.getUserMedia(mediaConfig).then(function (stream) {
                        //video.src = window.URL.createObjectURL(stream);
                        video.srcObject = stream;
                        video.play();
                    });
                    }

                    /* Legacy code below! */
                    else if (navigator.getUserMedia) { // Standard
                        navigator.getUserMedia(mediaConfig, function (stream) {
                            video.src = stream;
                            video.play();
                        }, errBack);
                    } else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
                        navigator.webkitGetUserMedia(mediaConfig, function (stream) {
                            video.src = window.webkitURL.createObjectURL(stream);
                            video.play();
                        }, errBack);
                    } else if (navigator.mozGetUserMedia) { // Mozilla-prefixed
                        navigator.mozGetUserMedia(mediaConfig, function (stream) {
                            video.src = window.URL.createObjectURL(stream);
                            video.play();
                        }, errBack);
                    }
                }

                objBtnCam.click(function () {
                    if (objBtnCam.data('mode') == 'R') {
                        objPic.hide();
                        objVidCam.show();
                        AttachVideoListener(objVidCam[0]);
                        objBtnCancel.show();
                        objBtnCam.val('CAPTURE');
                        objBtnCam.data('mode', 'C');
                    }
                    else {
                        var context = objVidCamCanvas.getContext('2d');
                        context.drawImage(objVidCam[0], 0, 0, 240, 180);

                        $.ajax({
                            url: AppRoot + 'Student/Enrollments/UploadPicStr',
                            type: 'POST',
                            data: { imgString: objVidCamCanvas.toDataURL() },
                            success: function(result) {
                                objPic.attr('src', objPic.data('org-src') + '?timestamp=' + new Date().getTime());
                                objPic.show();
                            },
                            error: function(data, status, jqXHR) {
                                if (IsJson(data.responseText)) { AlertIt('ERROR: ' + JSON.parse(data.responseText).Message); }
                                else { AlertIt('ERROR: ' + data.statusText); }
                            }
                        });
                        objProg.hide();
                        objVidCam.hide();

                        objBtnCancel.hide();
                        objBtnCam.val('RETAKE');
                        objBtnCam.data('mode', 'R');
                    }
                });

                objBtnCancel.click(function () {
                    objVidCam.hide();
                    objBtnCancel.hide();
                    objPic.show();
                    objBtnCam.data('mode', 'R');
                });
            ";

            script.InnerHtml = str;
            return new MvcHtmlString(vid.ToString() + cnvs.ToString() + imgDiv.ToString() + btnToolBar.ToString() + script.ToString());
        }

        public static MvcHtmlString RequiredLabelFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            TagBuilder astrik = new TagBuilder("span");
            astrik.SetInnerText("*");
            astrik.Attributes.Add("style", "color:red");

            TagBuilder builder = new TagBuilder("label");

            var pi = GetPropertyInfo(expression);

            builder.InnerHtml = GetPropertyDisplayName(expression) + " " + astrik.ToString();
            builder.Attributes.Add("for", pi.Name);

            object attrs = htmlAttributes.ToDynamic();
            foreach (KeyValuePair<string, object> kvp in (ExpandoObject)attrs)
            { builder.Attributes.Add(kvp.Key, kvp.Value.ToString()); }

            return new MvcHtmlString(builder.ToString());
        }

        private static string GetPropertyDisplayName<TModel, TValue>(Expression<Func<TModel, TValue>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetCustomAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            MemberExpression memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }
    }
}

namespace System.Web.Helpers
{
    public static class WebGridExtensions
    {
        public static HelperResult PagerList(
            this WebGrid webGrid,
            WebGridPagerModes mode = WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric,
            int pageSize = 10,
            int totalRecords = 0,
            int? page = null,
            string firstText = null,
            string previousText = null,
            string nextText = null,
            string lastText = null,
            int numericLinksCount = 5)
        {
            if (totalRecords == 0)
            { return new HelperResult(writer => { writer.Write(""); }); }

            int currentPage = page == null ? webGrid.PageIndex : page.Value - 1;
            int totalPages = webGrid.PageCount;

            //int lastPage = totalPages - 1;
            int lastPage = (totalPages - 1) < 0 ? 0 : (totalPages - 1);

            var footDiv = new TagBuilder("div");
            footDiv.MergeAttribute("class", "text-center");
            var btnsDiv = new TagBuilder("div");
            btnsDiv.MergeAttribute("class", "btn-group");
            var tags = new List<string>();

            if (webGrid.PageCount > 1)
            {
                #region Paging Controls
                if (ModeEnabled(mode, WebGridPagerModes.FirstLast))
                {
                    if (String.IsNullOrEmpty(firstText))
                    {
                        firstText = "First";
                    }

                    var tag = GridLink(webGrid, webGrid.GetPageUrl(0), firstText, 0);
                    tag.MergeAttribute("class", "btn btn-default btn-sm");

                    if (currentPage == 0)
                    {
                        tag.MergeAttribute("disabled", "disabled");
                    }

                    tags.Add(tag.ToString(TagRenderMode.Normal));
                }

                if (ModeEnabled(mode, WebGridPagerModes.NextPrevious))
                {
                    if (String.IsNullOrEmpty(previousText))
                    {
                        previousText = "Prev";
                    }

                    int curPage = currentPage == 0 ? 0 : currentPage - 1;

                    var tag = GridLink(webGrid, webGrid.GetPageUrl(curPage), previousText, curPage);
                    tag.MergeAttribute("class", "btn btn-default btn-sm");

                    if (currentPage == 0)
                    {
                        tag.MergeAttribute("disabled", "disabled");
                    }

                    tags.Add(tag.ToString(TagRenderMode.Normal));
                }

                if (ModeEnabled(mode, WebGridPagerModes.Numeric) && (totalPages > 1))
                {
                    int last = currentPage + (numericLinksCount / 2);
                    int first = last - numericLinksCount + 1;
                    if (last > lastPage)
                    {
                        first -= last - lastPage;
                        last = lastPage;
                    }
                    if (first < 0)
                    {
                        last = Math.Min(last + (0 - first), lastPage);
                        first = 0;
                    }
                    for (int i = first; i <= last; i++)
                    {
                        var pageText = (i + 1).ToString(CultureInfo.InvariantCulture);

                        var tag = GridLink(webGrid, webGrid.GetPageUrl(i), pageText, i);

                        if (i == currentPage)
                        {
                            tag.MergeAttribute("class", "btn btn-primary btn-sm");
                            tag.MergeAttribute("style", "font-weight:bolder");
                            tag.MergeAttribute("disabled", "disabled");
                        }
                        else
                        {
                            tag.MergeAttribute("class", "btn btn-default btn-sm");
                        }

                        tags.Add(tag.ToString(TagRenderMode.Normal));
                    }
                }

                if (ModeEnabled(mode, WebGridPagerModes.NextPrevious))
                {
                    if (String.IsNullOrEmpty(nextText))
                    {
                        nextText = "Next";
                    }

                    int curPage = currentPage == lastPage ? lastPage : currentPage + 1;

                    var tag = GridLink(webGrid, webGrid.GetPageUrl(curPage), nextText, curPage);
                    tag.MergeAttribute("class", "btn btn-default btn-sm");

                    if (currentPage == lastPage)
                    {
                        tag.MergeAttribute("disabled", "disabled");
                    }

                    tags.Add(tag.ToString(TagRenderMode.Normal));
                }

                if (ModeEnabled(mode, WebGridPagerModes.FirstLast))
                {
                    if (String.IsNullOrEmpty(lastText))
                    {
                        lastText = "Last";
                    }

                    var tag = GridLink(webGrid, webGrid.GetPageUrl(lastPage), lastText, lastPage);
                    tag.MergeAttribute("class", "btn btn-default btn-sm");

                    if (currentPage == lastPage)
                    {
                        tag.MergeAttribute("disabled", "disabled");
                    }

                    tags.Add(tag.ToString(TagRenderMode.Normal));
                }
                #endregion
            }

            #region Page Size Combo
            var sel = new TagBuilder("select");
            sel.Attributes.Add("id", "PageSize");
            sel.Attributes.Add("name", "PageSize");
            sel.Attributes.Add("class", "custom-select");
            sel.Attributes.Add("style", "margin-left:10px");
            sel.Attributes.Add("data-prev-val", pageSize.ToString());
            if (totalRecords > 50)
            { sel.Attributes.Add("data-warn-show-all", "warn"); }

            StringBuilder sb = new StringBuilder();
            var arrPageSize = new List<int>() { 5, 10, 20 };
            if (totalRecords > 100)
            { arrPageSize.Add(50); }

            foreach (var ps in arrPageSize)
            {
                var opt = new TagBuilder("option");
                opt.Attributes.Add("value", ps.ToString());
                if (ps == pageSize)
                { opt.Attributes.Add("selected", "selected"); }
                opt.InnerHtml = ps.ToString() + " Rows Per Page";
                sb.Append(opt.ToString());
            }

            if (totalRecords <= 100)
            {
                var optAll = new TagBuilder("option");
                optAll.Attributes.Add("value", "0");
                if (pageSize == 0)
                { optAll.Attributes.Add("selected", "selected"); }
                optAll.InnerHtml = "Show all";
                sb.Append(optAll.ToString());
            }

            sel.InnerHtml = sb.ToString();
            #endregion

            btnsDiv.InnerHtml = string.Join("", tags);
            footDiv.InnerHtml = btnsDiv.ToString() + sel.ToString();

            var html = "";
            if (webGrid.IsAjaxEnabled)
            {
                var span = new TagBuilder("span");
                span.MergeAttribute("data-swhgajax", "true");
                span.MergeAttribute("data-swhgcontainer", webGrid.AjaxUpdateContainerId);
                span.MergeAttribute("data-swhgcallback", webGrid.AjaxUpdateCallback);

                span.InnerHtml = footDiv.ToString();
                html = span.ToString();

            }
            else
            {
                html = footDiv.ToString();
            }

            return new HelperResult(writer =>
            {
                writer.Write(html);
            });
        }

        private static TagBuilder GridLink(WebGrid webGrid, string url, string text, int index)
        {
            TagBuilder builder = new TagBuilder("a");
            builder.SetInnerText(text);
            builder.MergeAttribute("href", url);
            builder.Attributes.Add("data-page-id", (index + 1).ToString());
            if (webGrid.IsAjaxEnabled)
            {
                builder.MergeAttribute("data-swhglnk", "true");
            }
            return builder;
        }

        private static bool ModeEnabled(WebGridPagerModes mode, WebGridPagerModes modeCheck)
        {
            return (mode & modeCheck) == modeCheck;
        }
    }
}