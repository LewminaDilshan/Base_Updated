using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Helpers;

namespace RedLineLanka_Enterprise.Common
{
    [Serializable]
    public class BaseViewModel<T>
    {
        public BaseViewModel()
        {
            PageSize = 10;
        }

        public int? page { get; set; }
        public string sort { get; set; }
        public string sortdir { get; set; }
        public int PageSize { get; set; }
        public string FilterBy { get; set; }
        public string Filter { get; set; }
        public int TotalRecords { get; set; }
        public List<T> objList { get; set; }
        public decimal numericVar1 { get; set; }
        public string stringVar1 { get; set; }
        //public int? numericVar2 { get; set; }

        public void SetList<E>(IQueryable<E> qry, string DefaultSort, params object[] properties)
        {
            SetList(qry, DefaultSort, SortDirection.Ascending, properties);
        }

        public void SetList<E>(IQueryable<E> qry, string DefaultSort, SortDirection DefaultSortDir = SortDirection.Ascending, params object[] properties)
        {
            var startPage = 0;
            if (page.HasValue && page.Value > 0)
            { startPage = page.Value - 1; }

            if (sort.IsBlank())
            { sort = DefaultSort; }
            if (sortdir.IsBlank())
            { sortdir = DefaultSortDir == SortDirection.Ascending ? "ASC" : "DESC"; }

            dynamic exprSort = null, exprFilterBy = null;

            if (typeof(T).GetInterfaces()[0].GetGenericTypeDefinition() == typeof(IModel<,>))
            {
                dynamic objT = (T)Activator.CreateInstance(typeof(T));
                foreach (var map in objT.mappings)
                {
                    string mbrName = ((Expression)map.modelProperty.Body).GetMemberName();

                    if (mbrName == FilterBy)
                    { exprFilterBy = map.entityProperty; }
                    if (mbrName == sort)
                    { exprSort = map.entityProperty; }
                }
            }

            var miObjToEnumChar = typeof(object).GetExtensionMethod("ToEnumChar", new Type[] { typeof(string) });

            if (exprSort == null)
            {
                ParameterExpression argParam = Expression.Parameter(typeof(E), "s");
                Expression baseExpr = Expression.Property(argParam, sort);
                if (baseExpr.Type.IsEnum || baseExpr.Type.IsNullableEnum())
                {
                    baseExpr = GetEnumExpression(baseExpr);
                    exprSort = (Expression<Func<E, string>>)Expression.Lambda(baseExpr, argParam);
                }
            }

            if (exprSort == null)
            { qry = qry.OrderBy(sort + " " + sortdir); }
            else
            {
                exprSort = ExpressionMethodReplacer.Replace((Expression)exprSort, miObjToEnumChar, GetEnumExpression);

                if (sortdir == "ASC")
                { qry = Queryable.OrderBy(qry, exprSort); }
                else
                { qry = Queryable.OrderByDescending(qry, exprSort); }
            }

            if (!Filter.IsBlank())
            {
                if (exprFilterBy != null)
                {
                    ParameterExpression[] argParam = ((IEnumerable<ParameterExpression>)exprFilterBy.Parameters).ToArray();
                    var baseExpr = ExpressionMethodReplacer.Replace((Expression)exprFilterBy.Body, miObjToEnumChar, GetEnumExpression);

                    var expr = GetWhereExpression<E>(baseExpr, argParam);
                    qry = qry.Where(expr);
                }
                else if (!FilterBy.IsBlank())
                {
                    ParameterExpression argParam = Expression.Parameter(typeof(E), "s");
                    var baseExpr = Expression.Property(argParam, FilterBy);

                    var expr = GetWhereExpression<E>(baseExpr, argParam);
                    qry = qry.Where(expr);
                }
            }

            TotalRecords = qry.Count();
            if (PageSize == 0)
            { PageSize = 1; }
            var pageCnt = (int)Math.Ceiling((decimal)TotalRecords / PageSize);
            if (page.HasValue && page.Value > pageCnt)
            {
                page = pageCnt;
                startPage = page.Value - 1;
            }

            Func<E, T> GetModelObject = (x) =>
            {
                if (properties.Count() == 0)
                    return (T)Activator.CreateInstance(typeof(T), x);
                else
                {
                    var props = properties.ToList();
                    props.Insert(0, x);
                    return (T)Activator.CreateInstance(typeof(T), props.ToArray());
                }
            };

            if (PageSize > 0)
            { objList = qry.Skip(startPage * PageSize).Take(PageSize).ToList().Select(x => GetModelObject(x)).ToList(); }
            else
            { objList = qry.ToList().Select(x => GetModelObject(x)).ToList(); }
        }

        public void SetList(List<T> lst, string DefaultSort, SortDirection DefaultSortDir = SortDirection.Ascending)
        {
            if (sort.IsBlank())
            { sort = DefaultSort; }
            if (sortdir.IsBlank())
            { sortdir = DefaultSortDir == SortDirection.Ascending ? "ASC" : "DESC"; }

            var startPage = 0;
            if (page.HasValue && page.Value > 0)
            { startPage = page.Value - 1; }

            TotalRecords = lst.Count();
            var pageCnt = (int)Math.Ceiling((decimal)TotalRecords / PageSize);
            if (page.HasValue && page.Value > pageCnt)
            {
                page = pageCnt;
                startPage = page.Value - 1;
            }
            if (PageSize > 0)
            { objList = lst.Skip(startPage * PageSize).Take(PageSize).ToList(); }
            else
            { objList = lst; }
        }

        private Expression<Func<E, bool>> GetWhereExpression<E>(Expression baseExpr, params ParameterExpression[] argParam)
        {
            var miObjToString = typeof(object).GetMethod("ToString", new Type[] { });
            var miStrToLower = typeof(string).GetMethod("ToLower", new Type[] { });
            var miStrContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Expression<Func<E, bool>> expr;
            DateTime? dat;
            if (baseExpr.Type.In(typeof(DateTime), typeof(DateTime?)) && (dat = Filter.ConvertTo<DateTime?>()) != null)
            {
                expr = Expression.Lambda<Func<E, bool>>(Expression.Equal(baseExpr, Expression.Constant(dat)), argParam);
            }
            else
            {
                Expression exp1 = GetEnumExpression(baseExpr);

                if (exp1.Type != typeof(string))
                { exp1 = Expression.Call(exp1, miObjToString); }

                var exp2 = Expression.Call(exp1, miStrToLower);
                var lastExp = Expression.Call(exp2, miStrContains, Expression.Constant(Filter.ToLower()));
                expr = Expression.Lambda<Func<E, bool>>(lastExp, argParam);
            }
            return expr;
        }

        private Expression GetEnumExpression(Expression expr)
        {
            Expression baseExpr = expr;
            if (expr is MethodCallExpression)
            {
                baseExpr = ((MethodCallExpression)expr).Arguments[0];

                while (baseExpr is UnaryExpression)
                { baseExpr = ((UnaryExpression)baseExpr).Operand; }
            }

            Expression exp1 = null;
            if (baseExpr.Type.IsEnum || baseExpr.Type.IsNullableEnum())
            {
                Type typ = baseExpr.Type.IsEnum ? baseExpr.Type : baseExpr.Type.GetGenericArguments()[0];
                baseExpr = Expression.Convert(baseExpr, typeof(int));

                foreach (Enum enm in Enum.GetValues(typ))
                {
                    var expEq = Expression.Equal(baseExpr, Expression.Constant(Convert.ToInt32(enm)));

                    if (exp1 == null)
                    { exp1 = Expression.Condition(expEq, Expression.Constant(enm.ToEnumChar()), Expression.Constant("")); }
                    else
                    { exp1 = Expression.Condition(expEq, Expression.Constant(enm.ToEnumChar()), exp1); }
                }
            }
            return exp1 ?? expr;
        }
    }

    public interface IModel<Tentity, Tmodel>
    {
        ObjMappings<Tentity, Tmodel> mappings { get; set; }
    }

    [Serializable]
    public class ObjMappings<Tentity, Tmodel> : IEnumerable<object>
    {
        private List<object> lspMappings = new List<object>();
        public ObjMap<Tentity, Tmodel, Tret> Add<Tret>(Expression<Func<Tentity, Tret>> fEntity, Expression<Func<Tmodel, Tret>> fModel)
        {
            var map = new ObjMap<Tentity, Tmodel, Tret>() { entityProperty = fEntity, modelProperty = fModel };
            lspMappings.Add(map);
            return map;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return lspMappings.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lspMappings.GetEnumerator();
        }
    }

    [Serializable]
    public class ObjMap<Tentity, Tmodel, Tret>
    {
        [NonSerialized]
        public Expression<Func<Tentity, Tret>> entityProperty;

        [NonSerialized]
        public Expression<Func<Tmodel, Tret>> modelProperty;
    }

    public static class ExpressionMethodReplacer
    {
        public static Expression Replace(Expression expression, MethodInfo method, Func<MethodCallExpression, Expression> replacer)
        {
            return new ParameterReplacerVisitor(method, replacer).Visit(expression);
        }

        private class ParameterReplacerVisitor : ExpressionVisitor
        {
            private MethodInfo _method;
            private Func<MethodCallExpression, Expression> _replacer;

            public ParameterReplacerVisitor(MethodInfo method, Func<MethodCallExpression, Expression> replacer)
            {
                _method = method;
                _replacer = replacer;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method == _method)
                { return _replacer(node); }

                return base.VisitMethodCall(node);
            }
        }
    }
}