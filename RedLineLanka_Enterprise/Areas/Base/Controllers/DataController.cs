using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace RedLineLanka_Enterprise.Areas.Base.Controllers
{
    public class DataController : Controller
    {
        private ActionResult GetDataPaginated<T>(IQueryable<T> qry, string sortBy = null, bool inReverse = false, int startIndex = 0, int pageSize = 5, Dictionary<string, string> lstSortColMap = null, Func<T, object> selFunc = null)
        {
            int rowCount = qry.Count();
            if (pageSize <= 0)
            {
                pageSize = 10;
                startIndex = 0;
            }

            if (startIndex > rowCount)
            { startIndex = 0; }

            var qrySortBy = (lstSortColMap ?? new Dictionary<string, string>()).Where(x => x.Key == sortBy).Select(x => x.Value).FirstOrDefault() ?? sortBy;

            qry = qry.OrderBy("(" + qrySortBy + ")" + (inReverse ? " DESC" : "")).Skip(startIndex);

            if (pageSize > 0)
            { qry = qry.Take(pageSize); }

            var data = qry.ToList().Select(selFunc ?? (x => x)).ToList();

            var obj = new { RowCount = rowCount, SortBy = sortBy, InReverse = inReverse, Data = data };
            return Json(obj);
        }

        public ActionResult GetUserRoles(string filter = null, string sortBy = null, bool inReverse = false, int startIndex = 0, int pageSize = 5, bool searchForKey = false, List<int> idsToExcluede = null)
        {
            using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
            {
                var qry = dbctx.Roles.AsQueryable();

                if (!filter.IsBlank())
                { qry = qry.Where(searchForKey ? "RoleID.ToString().Contains(@0)" : "Name.ToLower().Contains(@0)", filter.ToLower()); }
                if (idsToExcluede != null)
                {
                    foreach (var id in idsToExcluede)
                    { qry = qry.Where("RoleID != @0", id); }
                }

                int rowCount = qry.Count();
                if (pageSize <= 0)
                {
                    pageSize = 10;
                    startIndex = 0;
                }

                if (startIndex > rowCount)
                { startIndex = 0; }

                if (sortBy.IsBlank())
                { sortBy = "Name"; }

                var lstSortColMap = new Dictionary<string, string>()
                {
                    { "Role_ID", "RoleID" },
                    { "Role_Name", "Name" }
                };

                return GetDataPaginated(qry, sortBy, inReverse, startIndex, pageSize, lstSortColMap,
                    x => new
                    {
                        Role_ID = x.RoleID,
                        Role_Name = x.Name
                    });
            }
        }

        //public ActionResult GetEmployees(string filter = null, string sortBy = null, bool inReverse = false, int startIndex = 0, int pageSize = 5, bool searchForKey = false)
        //{
        //    using (dbRedlineLankaEntities dbctx = new dbRedlineLankaEntities())
        //    {
        //        var qry = dbctx.Employees.AsQueryable();

        //        if (!filter.IsBlank())
        //        {
        //            qry = qry.Where("EmployeeID.ToString().Contains(@0)" + (searchForKey ? "" : " || (FirstName+LastName+NICNo).ToLower().Contains(@0)"), filter.ToLower());
        //        }

        //        if (sortBy.IsBlank())
        //        { sortBy = "First_Name"; }

        //        var lstSortColMap = new Dictionary<string, string>()
        //        {
        //            { "Employee_ID","EmployeeID" } ,
        //            { "Full_Name", "Title+'. '+FirstName+' '+LastName" } ,
        //            { "First_Name", "FirstName" } ,
        //            { "Last_Name", "LastName" } ,
        //            { "NIC_No", "NICNo" },
        //        };

        //        return GetDataPaginated(qry, sortBy, inReverse, startIndex, pageSize, lstSortColMap,
        //            x => new
        //            {
        //                Employee_ID = x.EmployeeID,
        //                Full_Name = x.Title + ". " + x.FirstName + " " + x.LastName,
        //                First_Name = x.FirstName,
        //                Last_Name = x.LastName,
        //                NIC_No = x.NICNo
        //            });
        //    }
        //}

    }
}