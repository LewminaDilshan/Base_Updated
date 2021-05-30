using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Dynamic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using RedLineLanka_Enterprise.Common.DB;
using System.Linq.Expressions;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

namespace System
{
    public static class SysExtensions
    {
        public static bool In<TSource>(this TSource src, params TSource[] values)
        {
            foreach (TSource val in values)
            {
                if ((src is string && src.ToString().Equals(Convert.ToString(val), StringComparison.CurrentCultureIgnoreCase)) ||
                    (src == null && val == null) ||
                    (src != null && src.Equals(val)))
                { return true; }
            }
            return false;
        }
        public static bool Between(this int src, int FromValue, int ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this decimal src, decimal FromValue, decimal ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this float src, float FromValue, float ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this double src, double FromValue, double ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this short src, short FromValue, short ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this long src, long FromValue, long ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static bool Between(this DateTime src, DateTime FromValue, DateTime ToValue)
        {
            return src >= FromValue && src <= ToValue;
        }
        public static string ToEnumValueString(this Enum src)
        {
            if (src == null)
            { return ""; }

            long x = Convert.ToInt64(src);
            return x.ToString();
        }
        public static bool IsNullableEnum(this Type typ)
        {
            return typ.IsGenericType &&
                typ.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                typ.GetGenericArguments()[0].IsEnum;
        }
        public static string ToEnumChar(this object src, string defaultVal = null)
        {
            if (src == null || !src.GetType().IsEnum)
            { return defaultVal; }

            FieldInfo fi = src.GetType().GetField(src.ToString());
            if (fi == null)
            { return defaultVal; }

            var attribute = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute != null)
            { return attribute.Description; }

            return defaultVal ?? src.ToString();
        }
        public static TResult GetEnumVal<TResult>(this object src)
        { return src.GetEnumVal(default(TResult)); }
        public static TResult GetEnumVal<TResult>(this object src, TResult DefaultValue)
        { return src.GetEnumVal(typeof(TResult), DefaultValue); }
        private static dynamic GetEnumVal(this object src, Type RetType, dynamic DefaultValue)
        {
            Type typ = RetType;
            if (!typ.IsEnum)
            { typ = typ.GetGenericArguments()[0]; }

            short? val = src.ConvertTo<short?>();
            if ((val != null && Enum.IsDefined(typ, val)) ||
                (Enum.IsDefined(typ, src)))
            { return Enum.Parse(typ, src.ToString()); }
            else
            {
                foreach (var field in typ.GetFields())
                {
                    var attribute = Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attribute != null)
                    {
                        if (attribute.Description.Equals(src.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        { return field.GetValue(null); }
                    }
                    else
                    {
                        if (field.Name.Equals(src.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        { return field.GetValue(null); }
                    }
                }
                return DefaultValue;
            }
        }
        public static TResult ConvertTo<TResult>(this object src, params string[] DateTimeFormats)
        { return src.ConvertTo<TResult>(default(TResult), DateTimeFormats); }
        public static TResult ConvertTo<TResult>(this object src, TResult DefaultValue, params string[] DateTimeFormats)
        { return (TResult)src.ConvertTo(typeof(TResult), default(TResult), DateTimeFormats); }
        public static dynamic ConvertTo(this object src, Type RetType, params string[] DateTimeFormats)
        {
            object defVal;
            if (RetType.IsValueType)
            { defVal = Activator.CreateInstance(RetType); }
            defVal = null;

            return src.ConvertTo(RetType, defVal, DateTimeFormats);
        }
        public static dynamic ConvertTo(this object src, Type RetType, dynamic DefaultValue, params string[] DateTimeFormats)
        {
            if (RetType.IsEnum || RetType.IsNullableEnum())
            { return src.GetEnumVal(RetType, (object)DefaultValue); }

            string _src = (src ?? "").ToString();

            var ActDict = new Dictionary<Type, Func<string, object>>
            {
                { typeof(string), x => { return Convert.ToString(x); }},
                { typeof(int), x => {int val; if (int.TryParse(x, out val)) return val; else return DefaultValue; }},
                { typeof(decimal), x => {decimal val; if (decimal.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(float), x => {float val; if (float.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(double), x => {double val; if (double.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(short), x => {short val; if (short.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(long), x => {long val; if (long.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(bool), x => {bool val; if (bool.TryParse(x, out val)) return val; else return DefaultValue;} },
                { typeof(int?), x => {int val; if (int.TryParse(x, out val)) return val; else return null;} },
                { typeof(decimal?), x => {decimal val; if (decimal.TryParse(x, out val)) return val; else return null;} },
                { typeof(float?), x => {float val; if (float.TryParse(x, out val)) return val; else return null;} },
                { typeof(double?), x => {double val; if (double.TryParse(x, out val)) return val; else return null;} },
                { typeof(short?), x => {short val; if (short.TryParse(x, out val)) return val; else return null;} },
                { typeof(long?), x => {long val; if (long.TryParse(x, out val)) return val; else return null;} },
                { typeof(bool?), x => {bool val; if (bool.TryParse(x, out val)) return val; else return null;} }
            };

            if (RetType.In(typeof(bool), typeof(bool?)))
            {
                if (_src.Trim().ToUpper() == "Y" ||
                    src.ConvertTo<decimal>() != 0)
                { return (object)true; }
            }

            if (ActDict.Keys.Contains(RetType))
            { return ActDict[RetType](_src); }

            if (RetType.In(typeof(DateTime), typeof(DateTime?)))
            {
                DateTime dat;
                bool valid;
                if (DateTimeFormats.Count() == 0)
                {
                    string r = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
                    string str = _src.Substring(0, Math.Min(_src.Length, 10)).Replace("/", r).Replace("-", r).Replace(".", r).Replace(",", r) + (_src.Length > 10 ? _src.Substring(10) : "");
                    valid = DateTime.TryParse(str, out dat);
                }
                else
                {
                    valid = DateTime.TryParseExact(_src.Trim().Replace("-", "/").Replace(".", "/").Replace(",", "/"),
                        DateTimeFormats.Select(x => x.ToString().Replace("-", "/").Replace(".", "/").Replace(",", "/")).ToArray(),
                        CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out dat);
                }

                if (valid)
                { return (object)dat; }

                return DefaultValue;
            }
            return src;
        }
        public static string ArrabicWinToOra(this string src)
        {
            // 1252 = "Western European (Windows)"
            // 1256 = "Arabic (Windows)"
            return Encoding.GetEncoding(1252).GetString(Encoding.GetEncoding(1256).GetBytes(src));
        }
        public static string ArrabicOraToWin(this string src)
        {
            // 1252 = "Western European (Windows)"
            // 1256 = "Arabic (Windows)"
            return Encoding.GetEncoding(1256).GetString(Encoding.GetEncoding(1252).GetBytes(src));
        }
        public static bool IsValidDate(this string src, string format)
        {
            CultureInfo cul = CultureInfo.InstalledUICulture;
            //string format = ;
            DateTime result;
            if (DateTime.TryParseExact(src, format, cul, DateTimeStyles.None, out result))
            { return true; }
            else
            { return false; }
        }
        public static bool IsBlank(this string src)
        { return src == null || src.Trim() == string.Empty; }
        public static string Replace(this string src, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = src.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1)
                    break;

                src = src.Substring(0, startIndex) + newValue + src.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }

            return src;
        }
        public static string ToCamelCase(this string src)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(src);
        }
        public static string ToDateString(this Nullable<DateTime> src, string DateFormat = "dd/MM/yyyy")
        {
            if (src == null)
            { return string.Empty; }

            return src.Value.ToString(DateFormat);
        }
        public static T CopyContent<T>(this T src, T res, string propsToIncl = null, string propsToExcl = null)
        {
            Type resTyp = res.GetType();
            string[] propsToInclude = (propsToIncl ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] propsToExclude = (propsToExcl ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var prop in src.GetType().GetProperties())
            {
                if (propsToInclude.Length > 0 && !propsToInclude.Contains(prop.Name))
                { continue; }

                if (propsToExclude.Contains(prop.Name))
                { continue; }

                if (!prop.CanRead || (prop.GetIndexParameters().Length > 0))
                { continue; }

                PropertyInfo other = resTyp.GetProperty(prop.Name);
                if (other != null && prop.PropertyType == other.PropertyType && other.CanWrite)
                { other.SetValue(res, prop.GetValue(src, null), null); }
            }
            return res;
        }
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            if (value != null)
            {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                    expando.Add(property.Name, property.GetValue(value));
            }

            return expando as ExpandoObject;
        }
        public static object GetDynamicValue(this object src, string propertyName, object defaultValue)
        {
            Type objType = src.GetType();

            if (objType == typeof(ExpandoObject))
            {
                object obj;
                if (!((IDictionary<string, object>)src).TryGetValue(propertyName, out obj))
                { obj = defaultValue; }
                return obj;
            }

            var pi = objType.GetProperty(propertyName);
            if (pi == null)
            { return defaultValue; }

            return pi.GetValue(src);
        }
        public static Dictionary<string, object> ToDictionary(this object src)
        {
            var dct = new Dictionary<string, object>();

            foreach (var pi in src.GetType().GetProperties())
            { dct.Add(pi.Name, pi.GetValue(src, null)); }

            return dct;
        }
        public static Exception GetInnerException(this Exception ex)
        {
            var exp = ex;
            while (exp.InnerException != null)
            { exp = exp.InnerException; }
            return exp;
        }
        public static string ToWords(this decimal number)
        {
            var integ = (int)Math.Truncate(number);
            var intStr = integ.ToWords();
            var dec = (int)Math.Truncate((number - integ) * 100);

            if (dec == 0)
            { return intStr.Trim(); }

            return intStr.Trim() + " and " + dec.ToWords().Trim() + " Cents";
        }
        public static string ToWords(this int number)
        {
            if (number == 0)
            { return "Zero"; }

            if (number < 0)
            { return "Minus " + Math.Abs(number).ToWords(); }

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += (number / 1000000).ToWords() + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += (number / 1000).ToWords() + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += (number / 100).ToWords() + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        public static string Encrypt(this string Source)
        {
            SymmetricAlgorithm EncodeMethod = new RijndaelManaged();
            if (Source == null || Source.Length == 0)
                return null;

            if (EncodeMethod == null) return "Under Construction";

            long lLen;
            int nRead, nReadTotal;
            byte[] buf = new byte[3];
            byte[] srcData;
            byte[] encData;
            MemoryStream sin;
            System.IO.MemoryStream sout;
            CryptoStream encStream;

            srcData = Encoding.ASCII.GetBytes(Source);
            sin = new MemoryStream();
            sin.Write(srcData, 0, srcData.Length);
            sin.Position = 0;
            sout = new MemoryStream();

            EncodeMethod.InitializeEncodeMethod();

            encStream = new CryptoStream(sout,
                EncodeMethod.CreateEncryptor(),
                CryptoStreamMode.Write);
            lLen = sin.Length;
            nReadTotal = 0;
            while (nReadTotal < lLen)
            {
                nRead = sin.Read(buf, 0, buf.Length);
                encStream.Write(buf, 0, nRead);
                nReadTotal += nRead;
            }
            encStream.Close();

            encData = sout.ToArray();
            return Convert.ToBase64String(encData);
        }
        public static string Decrypt(this string Source)
        {
            SymmetricAlgorithm EncodeMethod = new RijndaelManaged();
            if (Source == null || Source.Length == 0)
                return null;

            if (EncodeMethod == null) return "Under Construction";

            long lLen;
            int nRead, nReadTotal;
            byte[] buf = new byte[3];
            byte[] decData;
            byte[] encData;
            MemoryStream sin;
            MemoryStream sout;
            CryptoStream decStream;

            try { encData = System.Convert.FromBase64String(Source); }
            catch (Exception)
            { return Source; }
            sin = new MemoryStream(encData);
            sout = new MemoryStream();

            EncodeMethod.InitializeEncodeMethod();

            decStream = new CryptoStream(sin,
                EncodeMethod.CreateDecryptor(),
                CryptoStreamMode.Read);

            lLen = sin.Length;
            nReadTotal = 0;
            while (nReadTotal < lLen)
            {
                nRead = decStream.Read(buf, 0, buf.Length);
                if (0 == nRead) break;

                sout.Write(buf, 0, nRead);
                nReadTotal += nRead;
            }

            decStream.Close();

            decData = sout.ToArray();

            ASCIIEncoding ascEnc = new System.Text.ASCIIEncoding();
            return ascEnc.GetString(decData);
        }
        private static void InitializeEncodeMethod(this SymmetricAlgorithm EncodeMethod)
        {
            //Never change this value !!!important
            string Key = "N!bmC!rcu!to@2017";
            string sTemp;
            if (EncodeMethod.LegalKeySizes.Length > 0)
            {
                int lessSize = 0, moreSize = EncodeMethod.LegalKeySizes[0].MinSize;
                // key sizes are in bits

                while (Key.Length * 8 > moreSize &&
                    EncodeMethod.LegalKeySizes[0].SkipSize > 0 &&
                    moreSize < EncodeMethod.LegalKeySizes[0].MaxSize)
                {
                    lessSize = moreSize;
                    moreSize += EncodeMethod.LegalKeySizes[0].SkipSize;
                }

                if (Key.Length * 8 > moreSize)
                    sTemp = Key.Substring(0, (moreSize / 8));
                else
                    sTemp = Key.PadRight(moreSize / 8, ' ');
            }
            else
            { sTemp = Key; }

            // convert the secret key to byte array
            EncodeMethod.Key = Encoding.ASCII.GetBytes(sTemp);

            if (Key.Length > EncodeMethod.IV.Length)
            {
                EncodeMethod.IV = Encoding.ASCII.GetBytes(Key.Substring(0, EncodeMethod.IV.Length));
            }
            else
            {
                EncodeMethod.IV = Encoding.ASCII.GetBytes(Key.PadRight(EncodeMethod.IV.Length, ' '));
            }
        }
        public static T DeProxyClone<T>(this T src, int diveInLength = 2) where T : class
        {
            Type retTyp = src.GetType();
            T retObj = (T)Activator.CreateInstance(retTyp);

            foreach (var prop in retTyp.GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite || (prop.GetIndexParameters().Length > 0))
                { continue; }

                var val = prop.GetValue(src, null);

                if (val != null && val.GetType().FullName.StartsWith("System.Data.Entity.DynamicProxies"))
                    continue;

                try
                {
                    var propType = prop.PropertyType;
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        propType = Nullable.GetUnderlyingType(propType);

                    if (propType != typeof(string) &&
                        !propType.IsValueType &&
                        !propType.IsPrimitive &&
                        !propType.IsEnum)
                    {
                        if (diveInLength <= 0)
                            continue;

                        if (prop.PropertyType.GetInterface("IEnumerable") != null)
                        {
                            var makeGenericType = typeof(List<>).MakeGenericType(prop.PropertyType.GetGenericArguments());
                            var tempCollection = (IList)Activator.CreateInstance(makeGenericType);

                            foreach (var obj in (IEnumerable)val)
                            {
                                tempCollection.Add(obj.DeProxyClone(diveInLength - 1));
                            }
                            val = tempCollection;
                        }
                        else if (prop.PropertyType.IsClass)
                            val = val.DeProxyClone(diveInLength - 1);
                    }

                    prop.SetValue(retObj, val, null);
                }
                catch (Exception)
                { }
            }
            return retObj;
        }
    }

    public static class EntityExtensions
    {
        public static TEntity FindOne<TEntity>(this DbSet<TEntity> source, Func<TEntity, bool> predicate) where TEntity : class
        {
            TEntity obj = source.Local.Where(predicate).SingleOrDefault();
            if (obj == null)
            { obj = source.Where(predicate).SingleOrDefault(); }
            return obj;
        }

        public static void Detach(this DbContext ctx, object entity)
        {
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)ctx).ObjectContext.Detach(entity);
        }

        public static void UndoChanges(this DbContext ctx)
        {
            // Undo the changes of the all entries. 
            foreach (DbEntityEntry entry in ctx.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    // Under the covers, changing the state of an entity from  
                    // Modified to Unchanged first sets the values of all  
                    // properties to the original values that were read from  
                    // the database when it was queried, and then marks the  
                    // entity as Unchanged. This will also reject changes to  
                    // FK relationships since the original value of the FK  
                    // will be restored. 
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    // If the EntityState is the Deleted, reload the date from the database.   
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    default: break;
                }
            }
        }
    }
    public static class CustomExtensions
    {
        private static JavaScriptSerializer _Serializer;
        private static JavaScriptSerializer Serializer
        {
            get
            {
                if (_Serializer == null)
                { _Serializer = new JavaScriptSerializer(); }
                return _Serializer;
            }
        }

        public static string SerializeToJson(this object src)
        {
            return Serializer.Serialize(src);
        }
        public static T DeserializeJson<T>(this string src)
        {
            return Serializer.Deserialize<T>(src);
        }
        public static List<string> GetNicList(this string src)
        {
            src = src?.Trim()?.ToUpper() ?? "";
            var lst = new List<string>() { src };

            if (src.Length == 10 && src.Substring(9).In("V", "X"))
            {
                int yr = src.Substring(0, 2).ConvertTo<int>();
                yr = yr > 15 ? 1900 + yr : 2000 + yr;
                lst.Add(yr.ToString() + src.Substring(2, 3) + "0" + src.Substring(5, 4));
            }
            else if (src.Length == 12 && src.ConvertTo<long?>() != null)
            {
                int yr = src.Substring(0, 4).ConvertTo<int>();
                if (yr <= 2015)
                {
                    lst.Add(src.Substring(2, 5) + src.Substring(8, 4) + "V");
                    lst.Add(src.Substring(2, 5) + src.Substring(8, 4) + "X");
                }
            }
            return lst;
        }
        public static bool IsValidNIC(this string src)
        {
            src = src.Trim().ToUpper();
            return (src.Length == 10 && src.Substring(9).In("V", "X")) || (src.Length == 12 && src.ConvertTo<long?>() != null);
        }
    }
}

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.MinOrDefault(default(TSource));
        }
        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource defVal)
        {
            if (source.Count() > 0)
            { return source.Min(); }
            else
            { return defVal; }
        }
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.MinOrDefault(selector, default(TResult));
        }
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defVal)
        {
            if (source.Count() > 0)
            { return source.Min(selector); }
            else
            { return defVal; }
        }
        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.MaxOrDefault(default(TSource));
        }
        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource defVal)
        {
            if (source.Count() > 0)
            { return source.Max(); }
            else
            { return defVal; }
        }
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.MaxOrDefault(selector, default(TResult));
        }
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defVal)
        {
            if (source.Count() > 0)
            { return source.Max(selector); }
            else
            { return defVal; }
        }
        public static TSource AggregateOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            if (source.Count() > 0)
            { return source.Aggregate(func); }
            else
            { return default(TSource); }
        }
        public static List<T> ToList<T>(this ArrayList arrayList)
        {
            List<T> list = new List<T>(arrayList.Count);
            foreach (var instance in arrayList)
            {
                list.Add(instance.ConvertTo<T>());
            }
            return list;
        }
        public static List<T> MergeInnerLists<T>(this IEnumerable<IEnumerable<T>> lists)
        {
            IEnumerable<T> retLst = new List<T>();

            foreach (var lst in lists)
            {
                retLst = retLst.Concat(lst);
            }
            return retLst.ToList();
        }
        public static string GetMemberName(this Expression expr)
        {
            if (expr is MemberExpression)
            { return ((MemberExpression)expr).Member.Name; }

            if (expr is UnaryExpression)
            {
                UnaryExpression uex = expr as UnaryExpression;
                while (uex.Operand is UnaryExpression)
                { uex = uex.Operand as UnaryExpression; }

                if (uex.Operand is MemberExpression)
                { return ((MemberExpression)uex.Operand).Member.Name; }
            }

            return string.Empty;
        }
        public static IEnumerable<TResult> FullJoinDistinct<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> leftItems, IEnumerable<TRight> rightItems,
            Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector, Func<TLeft, TRight, TResult> resultSelector)
        {
            if (leftItems == null)
            { leftItems = new List<TLeft>(); }
            if (rightItems == null)
            { rightItems = new List<TRight>(); ; }

            var leftJoin = from left in leftItems
                           join right in rightItems on leftKeySelector(left) equals rightKeySelector(right) into temp
                           from right in temp.DefaultIfEmpty()
                           select resultSelector(left, right);

            var rightJoin = from right in rightItems
                            join left in leftItems on rightKeySelector(right) equals leftKeySelector(left) into temp
                            from left in temp.DefaultIfEmpty()
                            select resultSelector(left, right);

            return leftJoin.Union(rightJoin);
        }
    }
}

namespace System.Data
{
    public static class DataExtensions
    {
        public static void SetAllowNull(this DataTable dt)
        { dt.SetAllowNull(true); }
        public static void SetAllowNull(this DataTable dt, bool RemoveKeys)
        {
            if (RemoveKeys)
            { dt.PrimaryKey = new DataColumn[] { }; }

            dt.Columns.Cast<DataColumn>().ToList().ForEach(x =>
            {
                if (!dt.PrimaryKey.Contains(x))
                { x.AllowDBNull = true; }
            });
        }
        public static DataRow Clone(this DataRow dr)
        {
            if (dr.RowState.In(DataRowState.Detached, DataRowState.Deleted))
            { return dr; }

            DataTable dt = dr.Table.Clone();
            dt.SetAllowNull();
            dt.ImportRow(dr);
            DataRow row = dt.Rows[0];

            dt.Columns.Cast<DataColumn>().ToList().ForEach(x => { row[x] = DBNull.Value; });
            return row;
        }
        public static object GetValue(this DataRow dr, string ColumnName)
        {
            if (dr.RowState == DataRowState.Deleted)
            { return dr[ColumnName, DataRowVersion.Original]; }
            else
            { return dr[ColumnName]; }
        }
        public static bool RenameColumn(this DataTable dt, string FromName, string ToName)
        {
            if (dt.Columns.Contains(FromName))
            {
                dt.Columns[FromName].ColumnName = ToName;
                return true;
            }
            return false;
        }
        public static MemoryStream GetExcelStream<T>(this List<T> lst, string Title = null, string SubHdr = null)
        {
            var workbook = new HSSFWorkbook();
            try
            {
                var props = typeof(T).GetProperties();
                var lstHdr = props.Select(x => x.Name);

                var sheet = (HSSFSheet)workbook.CreateSheet("DATA");

                var rowIndex = 0;
                IRow row;

                if (!Title.IsBlank())
                {
                    var hdrCellStyle = workbook.CreateCellStyle();
                    var hdrFont = workbook.CreateFont();
                    hdrFont.FontHeightInPoints = 14;
                    hdrFont.IsBold = true;
                    hdrCellStyle.SetFont(hdrFont);
                    hdrCellStyle.Alignment = HorizontalAlignment.Center;

                    row = sheet.CreateRow(rowIndex);
                    var cell = row.CreateCell(0);
                    cell.SetCellValue(Title);
                    cell.CellStyle = hdrCellStyle;
                    cell.CellStyle.WrapText = true;
                    cell.Row.Height = 400;
                    var cra = new CellRangeAddress(0, 0, 0, lstHdr.Count() - 1);
                    sheet.AddMergedRegion(cra);
                    rowIndex += 2;
                    HSSFRegionUtil.SetBorderTop(BorderStyle.Medium, cra, sheet, workbook);
                    HSSFRegionUtil.SetBorderRight(BorderStyle.Medium, cra, sheet, workbook);
                    HSSFRegionUtil.SetBorderBottom(BorderStyle.Medium, cra, sheet, workbook);
                    HSSFRegionUtil.SetBorderLeft(BorderStyle.Medium, cra, sheet, workbook);
                }

                if (!SubHdr.IsBlank())
                {
                    var subHdrCellStyle = workbook.CreateCellStyle();
                    var subHdrFont = workbook.CreateFont();
                    subHdrFont.FontHeightInPoints = 11;
                    subHdrFont.IsBold = true;
                    subHdrCellStyle.SetFont(subHdrFont);

                    row = sheet.CreateRow(rowIndex);
                    var cell = row.CreateCell(0);
                    cell.SetCellValue(SubHdr);
                    cell.CellStyle = subHdrCellStyle;
                    cell.CellStyle.WrapText = true;
                    cell.Row.Height = 400;
                    var cra = new CellRangeAddress(rowIndex, rowIndex, 0, lstHdr.Count() - 1);
                    sheet.AddMergedRegion(cra);
                    rowIndex += 2;
                }

                var titleCellStyle = workbook.CreateCellStyle();
                var titleFont = workbook.CreateFont();
                titleFont.FontHeightInPoints = 10;
                titleFont.IsBold = true;
                titleFont.Underline = FontUnderlineType.Single;
                titleCellStyle.SetFont(titleFont);

                row = sheet.CreateRow(rowIndex);
                for (int i = 0; i < lstHdr.Count(); i++)
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(lstHdr.ElementAt(i).Replace("_", " "));
                    cell.CellStyle = titleCellStyle;
                }
                rowIndex++;

                foreach (var item in lst)
                {
                    row = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < props.Count(); i++)
                    {
                        PropertyInfo pi = props[i];
                        row.CreateCell(i).SetCellValue(pi.GetValue(item, null).ConvertTo<string>());
                    }
                    rowIndex++;
                }

                for (int i = 0; i < props.Count(); i++)
                { sheet.AutoSizeColumn(i); }

                var ms = new MemoryStream();
                workbook.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            finally
            { workbook.Close(); }
        }
    }
}

namespace System.Web.UI.WebControls
{

    public static class WebControlExtensions
    {
        public static void BindDropDown(this DropDownList ctrl, object ds, string ValueField, string TextField)
        {
            ctrl.DataValueField = ValueField;
            ctrl.DataTextField = TextField;
            ctrl.DataSource = ds;
            ctrl.DataBind();
            ctrl.SelectedIndex = 0;
        }
    }
}

namespace System.Reflection
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type, string extensionsAssembly = null)
        {
            Assembly asm;
            if (extensionsAssembly.IsBlank())
            { asm = Assembly.GetExecutingAssembly(); }
            else
            { asm = Assembly.Load(extensionsAssembly); }

            var query = asm.GetTypes().Where(t => !t.IsGenericType && !t.IsNested)
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) && m.GetParameters()[0].ParameterType == type));

            return query;
        }

        public static MethodInfo GetExtensionMethod(this Type type, string name, string extensionsAssembly = null)
        {
            return type.GetExtensionMethods(extensionsAssembly).FirstOrDefault(m => m.Name == name);
        }

        public static MethodInfo GetExtensionMethod(this Type type, string name, Type[] types, string extensionsAssembly = null)
        {
            var methods = (from m in type.GetExtensionMethods(extensionsAssembly)
                           where m.Name == name
                           && m.GetParameters().Count() == types.Length + 1 // + 1 because extension method parameter (this)
                           select m).ToList();

            if (!methods.Any())
            {
                return default(MethodInfo);
            }

            if (methods.Count() == 1)
            {
                return methods.First();
            }

            foreach (var methodInfo in methods)
            {
                var parameters = methodInfo.GetParameters();

                bool found = true;
                for (byte b = 0; b < types.Length; b++)
                {
                    found = true;
                    if (parameters[b].GetType() != types[b])
                    {
                        found = false;
                    }
                }

                if (found)
                {
                    return methodInfo;
                }
            }

            return default(MethodInfo);
        }
    }
}