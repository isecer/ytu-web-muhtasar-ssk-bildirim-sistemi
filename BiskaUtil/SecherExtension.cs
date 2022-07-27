using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Permissions;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Xml;
using System.Web.Security;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Web.Mvc;
using System.Threading;
using BiskaUtil;
using System.Drawing;

namespace BiskaUtil
{
    public static class SecherExtension
    {
        public class HttpParameter
        {
            string prefix = "";
            public HttpParameter() { }
            public HttpParameter(string Prefix)
            {
                this.prefix = Prefix;
            }
            public string Key { get; set; }
            public string Value { get; set; }
            public bool IsQueryString { get; set; }
        }


        [ValidateInput(false)]
        public static HttpParameter[] GetHttpParameters(string Prefix = null)
        {
            List<HttpParameter> parameters = new List<HttpParameter>();
            if (HttpContext.Current == null || HttpContext.Current.Request == null) return parameters.ToArray();

            var ctx = HttpContext.Current.Request;
            try
            {
                foreach (var key in ctx.QueryString.AllKeys)
                {
                    if (string.IsNullOrWhiteSpace(Prefix))
                        parameters.Add(new HttpParameter { IsQueryString = true, Key = key, Value = ctx.QueryString[key] });
                    else if (key.StartsWith(Prefix))
                        parameters.Add(new HttpParameter { IsQueryString = true, Key = key, Value = ctx.QueryString[key] });
                }
            }
            catch { }
            try
            {
                if (string.IsNullOrEmpty(Prefix))
                    foreach (var key in ctx.Params.AllKeys)
                        parameters.Add(new HttpParameter { IsQueryString = false, Key = key, Value = ctx.Params[key] });
                else
                    foreach (var key in ctx.Params.AllKeys.Where(p => p.StartsWith(Prefix)))
                        parameters.Add(new HttpParameter { IsQueryString = false, Key = key, Value = ctx.Params[key] });
            }
            catch { }
            try
            {
                if (!string.IsNullOrEmpty(Prefix))
                {
                    foreach (var p in parameters)
                        p.Key = p.Key.Substring(Prefix.Length);
                }
            }
            catch { }
            return parameters.ToArray();
        }
        public static HttpParameter[] GetHttpParameters(this HttpRequestBase ctx, string Prefix = null)
        {
            List<HttpParameter> parameters = new List<HttpParameter>();
            foreach (var key in ctx.QueryString.AllKeys)
            {
                if (string.IsNullOrWhiteSpace(Prefix))
                    parameters.Add(new HttpParameter { IsQueryString = true, Key = key, Value = ctx.QueryString[key] });
                else if (key.StartsWith(Prefix))
                    parameters.Add(new HttpParameter { IsQueryString = true, Key = key, Value = ctx.QueryString[key] });
            }
            if (string.IsNullOrEmpty(Prefix))
                foreach (var key in ctx.Params.AllKeys)
                    parameters.Add(new HttpParameter { IsQueryString = false, Key = key, Value = ctx.Params[key] });
            else
                foreach (var key in ctx.Params.AllKeys.Where(p => p.StartsWith(Prefix)))
                    parameters.Add(new HttpParameter { IsQueryString = false, Key = key, Value = ctx.Params[key] });
            if (!string.IsNullOrEmpty(Prefix))
            {
                foreach (var p in parameters)
                    p.Key = p.Key.Substring(Prefix.Length);
            }
            return parameters.ToArray();
        }

        public static Dictionary<string, string> ToDictionary(this FormCollection fc, string prefix = null)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var key in fc.AllKeys)
            {
                var ky = key;
                if (!string.IsNullOrEmpty(prefix) && key.StartsWith(prefix) == false) continue;
                else ky = ky.Substring(prefix.Length);
                dict.Add(ky, fc[key]);
            }
            return dict;
        }
        public static ViewDataDictionary GetViewData(this FormCollection fc, string prefix = null)
        {
            ViewDataDictionary dic = new ViewDataDictionary();
            foreach (var key in fc.AllKeys)
            {
                if (!string.IsNullOrEmpty(prefix) && key.StartsWith(prefix) == false) continue;
                dic[key] = fc[key];
            }
            return dic;
        }
        public static IQueryable<T> CustomFilter<T>(this IQueryable<T> query, Dictionary<string, string> Filters)
        {
            var result = query;
            foreach (var key in Filters.Keys)
            {
                if (string.IsNullOrEmpty(Filters[key])) continue;

                var t1 = new Type[] {typeof(int),typeof(byte),typeof(short),typeof(long),
                                     typeof(int?),typeof(byte?),typeof(short?),typeof(long?)};
                Type typeParameterType = typeof(T);
                var prop = (typeParameterType).GetProperty(key);
                if (prop == null) continue;
                var pType = prop.PropertyType;
                if (t1.Contains(pType))
                {
                    query = query.Where(key + "==" + Filters[key]);
                }
                else if (pType == typeof(bool) || pType == typeof(bool?))
                {
                    var b = Filters[key].ToBoolean();
                    if (b.HasValue)
                    {
                        query = query.Where(key + "==" + b.Value.ToString().ToLower());
                    }
                }
                else if (pType == typeof(DateTime) || pType == typeof(DateTime?))
                {
                    var f = Filters[key];
                    var t = DateTime.Now;
                    if (DateTime.TryParse(f, out t))
                    {
                        query = query.Where(string.Format("{0} == @0", key), t);
                    }
                }
                else
                {
                    query = query.Where(key + ".Contains(\"" + Filters[key] + "\")");
                }
                return query;
            }
            return result;
        }

        public static MvcHtmlString ToEvetHayir(this bool Bool)
        {
            return new MvcHtmlString((Bool ? "Evet" : "Hayır"));
        }
        public static MvcHtmlString ToEvetHayir(this bool? Bool)
        {
            return new MvcHtmlString(Bool == null ? "" : (Bool.Value ? "Evet" : "Hayır"));
        }
        public static MvcHtmlString ToAktifPasif(this bool Bool)
        {
            return new MvcHtmlString(Bool ? "Aktif" : "Pasif");
        }
        public static MvcHtmlString ToVarYok(this bool Bool)
        {
            return new MvcHtmlString(Bool ? "Var" : "Yok");
        }
        public static MvcHtmlString ToIsTelafiVar(this bool Bool)
        {
            return new MvcHtmlString(Bool ? "Telafi Yapılabilir" : "Telafi Yapılamaz");
        }
        public static MvcHtmlString ToIsYapilabilirYapilamaz(this bool Bool)
        {
            return new MvcHtmlString(Bool ? "Yapılabilsin" : "Yapılamasın");
        }
        public static MvcHtmlString ToIsSinavVar(this bool Bool)
        {
            return new MvcHtmlString(Bool ? "Sınav Var" : "Sınav Yok");
        }
        public static MvcHtmlString ToAktifPasif(this bool? Bool)
        {
            return new MvcHtmlString(Bool == null ? "" : (Bool.Value ? "Aktif" : "Pasif"));
        }


        /// <summary>
        /// DayOfWeek To String To TR
        /// </summary>
        /// <param name="dow"></param>
        /// <returns></returns>
        public static string ToTrString(this DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Friday:
                    return "Cuma";
                case DayOfWeek.Monday:
                    return "Pazartesi";
                case DayOfWeek.Saturday:
                    return "Cumartesi";
                case DayOfWeek.Sunday:
                    return "Pazar";
                case DayOfWeek.Thursday:
                    return "Perşembe";
                case DayOfWeek.Tuesday:
                    return "Salı";
                case DayOfWeek.Wednesday:
                    return "Çarşamba";
                default:
                    return "";
            }
        }
        public static bool IsNullOrEmpty(this string String)
        {
            return string.IsNullOrEmpty(String);
        }
        public static bool IsNullOrWhiteSpace(this string String)
        {
            return string.IsNullOrWhiteSpace(String);
        }
        public static string FixCurrSymbol(this string String)
        {
            var sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            return String.Replace(".", sep).Replace(",", sep);
        }
        public static bool? ToBoolean(this string String)
        {
            bool? ok = null;
            if (string.IsNullOrEmpty(String)) return ok;

            if (String.ToUpper() == true.ToString().ToUpper() ||
                String == "1" ||
                String.ToLower() == "evet" ||
                String.ToLower() == "var" ||
                String.ToLower() == "on")
                ok = true;
            if (String.ToUpper() == false.ToString().ToUpper() ||
                String == "0" ||
                String.ToLower() == "hayır" ||
                String.ToLower() == "yok" ||
                String.ToLower() == "off")
                ok = false;
            return ok;
        }
        public static bool ToBoolean(this string String, bool DefaultValue)
        {
            bool ok = DefaultValue;
            if (string.IsNullOrEmpty(String)) return ok;

            if (String.ToUpper() == true.ToString().ToUpper() ||
                String == "1" ||
                String.ToLower() == "evet" ||
                String.ToLower() == "var" ||
                String.ToLower() == "on")
                ok = true;
            if (String.ToUpper() == false.ToString().ToUpper() ||
                String == "0" ||
                String.ToLower() == "hayır" ||
                String.ToLower() == "yok" ||
                String.ToLower() == "off")
                ok = false;
            return ok;
        }

        public static int? ToInt(this string String)
        {
            int i = 0;
            return int.TryParse(String, out i) ? (int?)i : null;
        }
        public static int Abs(this int Sayi)
        {
            return Math.Abs(Sayi);
        }
        public static int? Abs(this int? Sayi)
        {
            return Sayi.HasValue ? (int?)Math.Abs(Sayi.Value) : null;

        }
        public static int ToInt(this string String, int Default)
        {
            var x = ToInt(String);
            x = x ?? Default;
            return x.Value;
        }
        public static double? ToDouble(this string String)
        {
            double dbl = 0;
            if (double.TryParse(String, out dbl))
                return dbl;
            return null;
        }
        public static double ToDouble(this string String, double DefaultValue)
        {
            double dbl = 0;
            if (double.TryParse(String, out dbl))
                return dbl;
            return DefaultValue;
        }
        public static decimal? ToDecimal(this string String)
        {
            decimal dbl = 0;
            if (decimal.TryParse(String, out dbl))
                return dbl;
            return null;
        }
        public static decimal ToDecimal(this string String, decimal DefaultValue)
        {
            decimal dbl = 0;
            if (decimal.TryParse(String, out dbl))
                return dbl;
            return DefaultValue;
        }

        public static T ToDefault<T>(this T obj, T defaultValue)
        {
            if (obj == null) return defaultValue;
            return obj;
        }

        public static double Abs(this double doubleSayi)
        {
            return Math.Abs(doubleSayi);
        }
        public static double? Abs(this double? doubleSayi)
        {
            if (doubleSayi.HasValue) return Math.Abs(doubleSayi.Value);
            return null;
        }
        public static long? ToLong(this string String)
        {
            long l = 0;
            if (long.TryParse(String, out l))
                return l;
            return null;
        }
        public static long ToLong(this string String, long DefaultValue)
        {
            if (string.IsNullOrWhiteSpace(String)) return DefaultValue;
            long l = 0;
            if (long.TryParse(String, out l)) return l;
            return DefaultValue;
        }

        public static bool IsInt(this string String)
        {
            return String.ToInt().HasValue;
        }
        public static bool IsDouble(this string String)
        {
            return String.ToDouble().HasValue;
        }
        public static T Copy<T>(this T x)
        {
            var availableTypes = new Type[]
            {
               typeof(byte),typeof(short),typeof(int),typeof(Int32),typeof(Int64),typeof(float),typeof(decimal),typeof(double),
               typeof(byte?),typeof(short?),typeof(int?),typeof(Int32?),typeof(Int64?),typeof(float?),typeof(decimal?),typeof(double?),
               typeof(string),
               typeof(DateTime),typeof(DateTime?),
               typeof(byte[]),
               typeof(Guid),typeof(Guid?),
               typeof(bool),typeof(bool?),
               typeof(uint),typeof(uint?),
               typeof(ushort),typeof(ushort?),
               typeof(UInt16),typeof(UInt16?),
               typeof(UInt32),typeof(UInt32?),
               typeof(UInt64),typeof(UInt64?),
               typeof(Single),typeof(Single?)
            };
            var type = x.GetType();
            object dstObject = Activator.CreateInstance(type);
            var props = x.GetType().GetProperties().Where(p => availableTypes.Contains(p.PropertyType)).ToArray();
            foreach (var prop in props)
            {
                try
                {
                    if (prop.CanRead == false || (prop.PropertyType.IsClass && prop.PropertyType.Name != "String")) continue;

                    var val = prop.GetValue(x, null);

                    var propTo = dstObject.GetType().GetProperty(prop.Name);
                    var pType = prop.PropertyType;// System.Data.Objects.DataClasses.EntityReference
                    if (propTo != null && propTo.CanWrite)
                    {
                        try
                        {
                            propTo.SetValue(dstObject, val, null);
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return (T)dstObject;

        }
        public static T SetNullTo<T>(this T obj)
        {
            return SetNullTo<T>(obj, 0, "", false);
        }
        public static T SetNullTo<T>(this T obj, int defaultIntValue, string defaultStringValue, bool defaultBoolValue)
        {
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetValue(obj, null) == null)
                {
                    if (prop.CanWrite && (prop.PropertyType == typeof(int?) || prop.PropertyType == typeof(byte?) || prop.PropertyType == typeof(short?)))
                    {
                        prop.SetValue(obj, defaultIntValue, null);
                    }
                    else if (prop.CanWrite && prop.PropertyType == typeof(bool?))
                    {
                        prop.SetValue(obj, defaultBoolValue, null);
                    }
                    else if (prop.CanWrite && prop.PropertyType.Name == "String")
                    {
                        prop.SetValue(obj, defaultStringValue, null);
                    }
                }
            }
            return (T)obj;
        }
        public static int[] ToIntArray(this string String, string seperator = ",")
        {
            var strArr = String.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
            return strArr.Select(s => s.ToInt()).Where(p => p.HasValue).Select(s => s.Value).ToArray();
        }
        public static int CharCount(this string String, params char[] chars)
        {
            var total = 0;
            foreach (var c in chars)
                total += String.ToCharArray().Where(p => p == c).Count();
            return total;
        }
        public static bool IsNumber(this string String)
        {
            if (string.IsNullOrEmpty(String)) return false;

            var isnotNumberExists = String.ToCharArray().Any(p => char.IsNumber(p) == false);
            return !isnotNumberExists;
        }
        public static DateTime? ToDate(this string String)
        {
            DateTime? result = null;
            DateTime tarih = DateTime.Today;
            if (string.IsNullOrWhiteSpace(String) == false && DateTime.TryParse(String, out tarih))
            {
                //result=tarih.Date;
                result = new DateTime(tarih.Year, tarih.Month, tarih.Day);
            }
            return result;
        }
        public static DateTime ToDate(this string String, DateTime DefaultDate)
        {
            var tarih = ToDate(String);
            if (tarih.HasValue) return tarih.Value;
            return DefaultDate;
        }
        public static DateTime? ToEndOfDay(this string String)
        {
            var t = ToDate(String);
            if (t.HasValue) return ToEndOfDay(t.Value);
            return null;
        }
        public static DateTime ToEndOfDay(this DateTime tarih)
        {
            var date = new DateTime(tarih.Year, tarih.Month, tarih.Day);
            return date.AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// Any object to ObjectType[] ,and first Element is this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyObject"></param>
        /// <returns></returns>
        public static T[] SingleToArray<T>(this T anyObject)
        {
            var arr = new T[] { anyObject };
            return arr;
        }
        public static IEnumerable<T> SingleToAsEnumarable<T>(this T anyObject)
        {
            var arr = new T[] { anyObject };
            return arr.AsEnumerable();
        }

        /// <summary>
        /// Any object to List<objectType>, and first Element is this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyObject"></param>
        /// <returns></returns>
        public static List<T> SingleToList<T>(this T anyObject)
        {
            var lst = new List<T>();
            lst.Add(anyObject);
            return lst;
        }

        /// <summary>
        /// Creates a List<objectType>,and no any elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyObject"></param>
        /// <returns></returns>
        public static List<T> CreateThisTypeList<T>(this T anyObject)
        {
            var lst = new List<T>();
            return lst;
        }
        public static string ToDatetTimeString(this DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("dd-MM-yyyy HH:mm:ss");
            }
            else
            {
                return "";
            }
        }
        public static string ToDateString(this DateTime date)
        {
            return date.ToString("dd-MM-yyyy");
        }
        public static string ToDateString(this DateTime? date)
        {
            return date.HasValue ? ToDateString(date.Value) : "";
        }
        public static string ToString(this DateTime? date, string format)
        {
            if (date.HasValue)
            {
                return date.Value.ToString(format);
            }
            else
            {
                return "";
            }
        }
        public static string ToString(this DateTime? date)
        {
            return ToString(date, "dd-MM-yyyy");
        }
        public static string ToString(this double? doubleValue, string format)
        {
            return doubleValue.HasValue ? doubleValue.Value.ToString(format) : "";
        }
        public static string ToString(this double? doubleValue)
        {
            return doubleValue.HasValue ? doubleValue.Value.ToString() : "";
        }


        /// <summary>
        /// this method doesn't use 'chart' type.For this,use override method of this.
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        public static object ToNearTypeValue(this string String)
        {
            return String.ToNearTypeValue(true, true);
        }

        public static object ToNearTypeValue(this string String, bool ignoreCharType, bool ignoreDateType)
        {
            //String = String.Replace("'","");            
            if (String.Contains("'"))
            {
                String = String.Replace("'", "");
                if (String.Length > 0)
                    return String[0];
                else return ' ';
            }
            else if (String.Contains("\""))
            {
                String = String.Replace("\"", "");
                return String;
            }

            object retwal = null;

            byte by = new byte();
            sbyte sby = new sbyte();
            char c = new char();
            decimal d = new decimal();
            double dbl = new double();
            float fl = new float();
            int i = new int();
            uint ui = new uint();
            long lng = new long();
            ulong ulng = new ulong();
            object o = new object();
            short shrt = new short();
            ushort ushrt = new ushort();
            bool b = false;
            DateTime t = DateTime.Now;

            if (bool.TryParse(String, out b)) retwal = b;
            else if (!ignoreDateType && (String.CharCount('.', '/') == 2) && DateTime.TryParse(String, out t)) retwal = t;


            else if (byte.TryParse(String, out by)) retwal = by;
            else if (sbyte.TryParse(String, out sby)) retwal = sby;

            else if (short.TryParse(String, out shrt)) retwal = shrt;
            else if (ushort.TryParse(String, out ushrt)) retwal = ushrt;

            else if (int.TryParse(String, out i)) retwal = i;
            else if (uint.TryParse(String, out ui)) retwal = ui;

            else if (long.TryParse(String, out lng)) retwal = lng;
            else if (ulong.TryParse(String, out ulng)) retwal = ulng;

            else if (decimal.TryParse(String, out d)) retwal = c;
            else if (float.TryParse(String, out fl)) retwal = fl;
            else if (double.TryParse(String, out dbl)) retwal = dbl;

            else if (ignoreCharType == false && char.TryParse(String, out c)) retwal = c;

            else retwal = String;
            return retwal;
        }


        private static bool QualifierRequired(Type type)
        {
            Type[] qualifierRequireds = new Type[] { typeof(Guid), typeof(DateTime), typeof(Guid?), typeof(DateTime?), typeof(Char?),
                                                     typeof(char), typeof(string), typeof(bool), typeof(bool?)
                                                     //typeof(double), typeof(float), typeof(decimal) ,
                                                     //typeof(double?), typeof(float?), typeof(decimal?)
                                                    };
            if (type.IsEnum) return true;
            return qualifierRequireds.Contains(type);
        }

        public static string ToFlexiJson(this object obje, string idField, params string[] propertyFields)
        {
            Func<object, string> fnc = new Func<object, string>(
           (o) =>
           {
               #region parse
               StringBuilder sb = new StringBuilder();
               bool hasNewField = false;
               var props = o.GetType().GetProperties();

               sb.Append("{");
               var idProp = props.Where(p => p.Name == idField).FirstOrDefault();
               if (idProp != null)
               {
                   var type = idProp.PropertyType;
                   var valx = idProp.GetValue(o, null);
                   if (QualifierRequired(type))
                   {
                       if (valx != null)
                       {
                           string valStr = valx.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                           sb.Append("\"" + idProp.Name + "\":\"" + valStr + "\"");
                       }
                       else
                           sb.Append("\"" + idProp.Name + "\":\"\"");
                   }
                   else
                   {
                       if (valx != null)
                           sb.Append("\"" + idProp.Name + "\":" + valx.ToString().Replace(",", "."));
                       else
                           sb.Append("\"" + idProp.Name + "\":null");
                   }
               }
               sb.Append(",\"cell\":");
               sb.Append("{");

               for (int i = 0; i < props.Length; i++)
               {
                   if (propertyFields != null && propertyFields.Length > 0 && propertyFields.Contains(props[i].Name) == false) continue;

                   var type = props[i].PropertyType;
                   if (!((type.IsValueType) || (type == typeof(String)))) continue;
                   if (type.IsEnum) continue;

                   var valx = props[i].GetValue(o, null);
                   if (QualifierRequired(type))
                   {
                       if (hasNewField) sb.Append(",");
                       if (valx != null)
                       {
                           string valStr = valx.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                           sb.Append("\"" + props[i].Name + "\":\"" + valStr + "\"");
                       }
                       else
                           sb.Append("\"" + props[i].Name + "\":\"\"");
                   }
                   else
                   {
                       if (hasNewField) sb.Append(",");
                       if (valx != null)
                           sb.Append("\"" + props[i].Name + "\":" + valx.ToString().Replace(",", "."));
                       else
                           sb.Append("\"" + props[i].Name + "\":null");
                   }
                   hasNewField = true;
               }
               sb.Append("}");
               sb.Append("}");
               #endregion
               return sb.ToString();
           });


            var typex = obje.GetType();
            if (obje is IList)
            {
                #region if List Type
                var lst = (obje as IList);
                StringBuilder sb2 = new StringBuilder();
                sb2.Append("[");
                for (int i = 0; i < lst.Count; i++)
                {
                    if (i > 0) sb2.Append(",");
                    sb2.AppendLine(fnc(lst[i]));
                }
                sb2.Append("]");
                return sb2.ToString();
                #endregion
            }
            else if (obje is IQueryable)
            {
                #region querable
                StringBuilder sb3 = new StringBuilder();
                sb3.Append("[");
                var pgdata = obje as IQueryable;
                bool ok = false;
                foreach (var pgd in pgdata)
                {
                    if (ok) { sb3.AppendLine(","); }
                    sb3.AppendLine(pgd.ToJson());
                    ok = true;
                }
                sb3.Append("]");
                return sb3.ToString();
                #endregion
            }
            else if (typex == typeof(string))
            {
                return obje.ToString();
            }
            else if (obje is IEnumerable)
            {
                #region enumarable
                StringBuilder sb4 = new StringBuilder();
                sb4.Append("[");
                var pgdata = obje as IEnumerable;
                bool ok = false;
                foreach (var pgd in pgdata)
                {
                    if (ok) { sb4.AppendLine(","); }
                    sb4.AppendLine(pgd.ToJson());
                    ok = true;
                }
                sb4.Append("]");
                return sb4.ToString();
                #endregion
            }
            else if (obje.GetType().IsValueType)
                return obje.ToString();
            else if (obje.GetType().IsEnum)
                return obje.ToString();
            else
                return fnc(obje);
        }

        private static string _tojson(object obje, string[] propertyFields)
        {
            Func<object, string> fnc = new Func<object, string>(
             (o) =>
             {
                 #region parse
                 StringBuilder sb = new StringBuilder();
                 bool hasNewField = false;
                 if (o.GetType() == typeof(string))
                 {
                     if (o == null) return "\"\"";
                     return "\"" + o.ToString().Replace("\"", "\\\"") + "\"";
                 }
                 else if (o.GetType().IsValueType)
                 {
                     if (o == null)
                     {
                         if (QualifierRequired(o.GetType())) return "\"\"";
                         else return "";
                     }
                     else
                     {
                         if (QualifierRequired(o.GetType()))
                         {
                             string valStr = o.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                             return "\"" + valStr + "\"";
                         }
                         else return o.ToString();
                     }
                 }
                 else if (o.GetType().IsEnum) { return "\"" + o.ToString().SpaceByCapitalLetters() + "\""; }

                 sb.Append("{");
                 var props = o.GetType().GetProperties();
                 for (int i = 0; i < props.Length; i++)
                 {
                     if (propertyFields != null && propertyFields.Length > 0 && propertyFields.Contains(props[i].Name) == false) continue;

                     var type = props[i].PropertyType;
                     if (!((type.IsValueType) || (type == typeof(String)))) continue;
                     if (type.IsEnum) continue;

                     var valx = props[i].GetValue(o, null);
                     if (QualifierRequired(type))
                     {
                         if (hasNewField) sb.Append(",");
                         if (valx != null)
                         {
                             string valStr = valx.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                             sb.Append("\"" + props[i].Name + "\":\"" + valStr + "\"");
                         }
                         else
                             sb.Append("\"" + props[i].Name + "\":\"\"");
                     }
                     else
                     {
                         if (hasNewField) sb.Append(",");
                         if (valx != null)
                             sb.Append("\"" + props[i].Name + "\":" + valx.ToString().Replace(",", "."));
                         else
                             sb.Append("\"" + props[i].Name + "\":null");
                     }
                     hasNewField = true;
                 }
                 sb.Append("}");
                 #endregion
                 return sb.ToString();
             });


            var typex = obje.GetType();
            if (obje is IList)
            {
                #region if List Type
                var lst = (obje as IList);
                StringBuilder sb2 = new StringBuilder();
                sb2.Append("[");
                for (int i = 0; i < lst.Count; i++)
                {
                    if (i > 0) sb2.Append(",");
                    sb2.AppendLine(fnc(lst[i]));
                }
                sb2.Append("]");
                return sb2.ToString();
                #endregion
            }
            else if (obje is IQueryable)
            {
                #region querable
                StringBuilder sb3 = new StringBuilder();
                sb3.Append("[");
                var pgdata = obje as IQueryable;
                bool ok = false;
                foreach (var pgd in pgdata)
                {
                    if (ok) { sb3.AppendLine(","); }
                    sb3.AppendLine(pgd.ToJson());
                    ok = true;
                }
                sb3.Append("]");
                return sb3.ToString();
                #endregion
            }
            else if (typex == typeof(string))
            {
                if (obje != null)
                {
                    string valStr = obje.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                    return valStr;
                }
                else
                    return "\"\"";
            }
            else if (obje is IEnumerable)
            {
                #region enumarable
                StringBuilder sb4 = new StringBuilder();
                sb4.Append("[");
                var pgdata = obje as IEnumerable;
                bool ok = false;
                foreach (var pgd in pgdata)
                {
                    if (ok) { sb4.AppendLine(","); }
                    sb4.AppendLine(pgd.ToJson());
                    ok = true;
                }
                sb4.Append("]");
                return sb4.ToString();
                #endregion
            }
            else if (obje.GetType().IsValueType)
            {
                if (QualifierRequired(obje.GetType()))
                {
                    if (obje != null)
                        return obje.ToString().Replace("\\", "\\\\").Replace("\"", @"\""");
                    return "";
                }
                else
                {
                    if (obje != null) return obje.ToString().Replace(",", ".");
                    else return "null";
                }
            }
            else if (obje.GetType().IsEnum)
                return "\"" + obje.ToString().SpaceByCapitalLetters() + "\"";
            else
                return fnc(obje);

        }
        public static string ToJson(this object Object)
        {
            var retwal = _tojson(Object, null);
            return retwal;
        }
        public static string ToJson(this object Object, params string[] PropertyFields)
        {
            var retwal = _tojson(Object, PropertyFields);
            return retwal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ObjectList">object[] source </param>
        /// <param name="RootPropertyField">ID</param>
        /// <param name="ParentPropertyField">ParentID</param>
        /// <returns></returns>
        public static string ToJsonTree<T>(this T[] ObjectList, string RootPropertyField, string ParentPropertyField, string ChildNewField, params string[] PropertyFields)
        {
            if (ObjectList == null) return "[]";
            //var LstObject = (object[])ObjectList;
            var LstObject = ObjectList;
            if (LstObject.Length == 0) return "[]";
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            var ids = Lst.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).ToArray();


            //            List<object> roots = new List<object>();
            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }


            Func<object, string> fxDetail = null;
            fxDetail = new Func<object, string>
                (
                   (parentid) =>
                   {
                       string returnVal = "";
                       var details = Lst.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();
                       foreach (var m in details)
                       {
                           var json = m.ToJson(PropertyFields);
                           if (returnVal != "") returnVal += ",";
                           returnVal += json;
                           var child = fxDetail(m.GetType().GetProperty(RootPropertyField).GetValue(m, null));
                           if (child != "" && child != "[]")
                           {
                               var lix = returnVal.LastIndexOf("}");
                               returnVal = returnVal.Substring(0, lix);
                               //returnVal += ",\"children\":" + child;
                               returnVal += ",\"" + ChildNewField + "\":" + child;
                               returnVal += "}";
                           }
                       }
                       return "[" + returnVal + "]";
                   }
                );

            string ReturnValue = "";
            foreach (var root in roots)
            {
                var json = root.ToJson(PropertyFields);
                if (ReturnValue != "") ReturnValue += ",";
                ReturnValue += json;
                var child = fxDetail(root.GetType().GetProperty(RootPropertyField).GetValue(root, null));
                if (child != "" && child != "[]")
                {
                    var lix = ReturnValue.LastIndexOf("}");
                    ReturnValue = ReturnValue.Substring(0, lix);
                    //ReturnValue += ",\"children\":" + child;
                    ReturnValue += ",\"" + ChildNewField + "\":" + child;
                    ReturnValue += "}";
                }
            }
            return "[" + ReturnValue + "]";

        }

        public static string ToJsonTree<T>(this T[] ObjectList, string RootPropertyField, string ParentPropertyField, string ChildNewField)
        {
            return ObjectList.ToJsonTree<T>(RootPropertyField, ParentPropertyField, ChildNewField, new string[] { });
            //return ToJsonTree(ObjectList, RootPropertyField, ParentPropertyField, ChildNewField, new string[] { });
        }

        //public static object ToJsonDataToObject(this string jsonstring, Type DestionationType)
        //{
        //    try
        //    {
        //        //System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //        //return oSerializer.Deserialize(jsonstring, DestionationType);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public static T Cast<T>(this object o)
        //{            
        //    return (T)o;
        //}
        //public static T GetInstance<T>(this Type type)
        //{
        //    return (T)Activator.CreateInstance(type);
        //}

        public static T[] GetRoots<T>(this T[] ObjectList, string RootPropertyField, string ParentPropertyField)
        {
            List<T> resultList = new List<T>();
            if (ObjectList == null) return resultList.ToArray();
            //var LstObject = (object[])ObjectList;            
            var LstObject = ObjectList;
            if (LstObject.Length == 0) return resultList.ToArray();
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            var ids = Lst.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).ToArray();

            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }
            return roots.ToArray();
        }

        /// <summary>
        /// Sorting Array Order By Tree Type
        /// </summary>
        /// <typeparam name="T">Source Type</typeparam>
        /// <param name="ObjectList">Source Array</param>
        /// <param name="RootPropertyField">Root Property : Such as ID field</param>
        /// <param name="ParentPropertyField">Parent Property :Such as ParentID</param>
        /// <returns></returns>
        public static T[] ToOrderedList<T>(this T[] ObjectList, string RootPropertyField, string ParentPropertyField)
        {
            List<T> resultList = new List<T>();
            if (ObjectList == null) return resultList.ToArray();
            //var LstObject = (object[])ObjectList;            
            var LstObject = ObjectList;
            if (LstObject.Length == 0) return resultList.ToArray();
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            var ids = Lst.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).ToArray();

            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }

            Func<T, int> fxDetail = null;
            fxDetail = new Func<T, int>
                (
                   (parent) =>
                   {
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent, null);
                       var details = Lst.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();
                       foreach (var m in details)
                       {
                           resultList.Add(m);
                           fxDetail(m);
                       }
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                resultList.Add(root);
                fxDetail(root);
            }
            return resultList.ToArray();

        }

        /// <summary>
        /// Sorting Array Order By Tree Type
        /// </summary>
        /// <typeparam name="T">Source Type</typeparam>
        /// <param name="ObjectList">Source Array</param>
        /// <param name="RootPropertyField">Root Property : Such as ID field</param>
        /// <param name="ParentPropertyField">Parent Property :Such as ParentID</param>
        /// <param name="TextPropertyField">Text Field Will Be Rename With Pad String. Default Pad String is ((char)160)</param>
        /// <returns></returns>
        /// 
        public static string _ToOrderedListPadChar = string.Concat(((char)160).ToString(), ((char)160).ToString(), ((char)160).ToString(), ((char)160).ToString());
        //public static string _ToOrderedListPadChar = "---";
        public static T[] ToOrderedList<T>(this IEnumerable<T> ObjectList, string RootPropertyField, string ParentPropertyField, string TextPropertyField)
        {
            //string padStr = ((char)160).ToString();
            //padStr = padStr + padStr + padStr;
            string padStr = _ToOrderedListPadChar;
            return ObjectList.ToOrderedList(RootPropertyField, ParentPropertyField, TextPropertyField, padStr);
        }

        /// <summary>
        /// Sorting Array Order By Tree Type
        /// </summary>
        /// <typeparam name="T">Source Type</typeparam>
        /// <param name="ObjectList">Source Array</param>
        /// <param name="RootPropertyField">Root Property : Such as ID field</param>
        /// <param name="ParentPropertyField">Parent Property :Such as ParentID</param>
        /// <param name="TextPropertyField">Text Field Will Be Rename With Pad String</param>
        /// <param name="PadString">Pad String</param>
        /// <returns></returns>
        public static T[] ToOrderedList<T>(this IEnumerable<T> ObjectList, string RootPropertyField, string ParentPropertyField, string TextPropertyField, string PadString)
        {
            List<T> resultList = new List<T>();
            if (ObjectList == null) return resultList.ToArray();
            //var LstObject = (object[])ObjectList;            
            var LstObject = ObjectList;
            if (LstObject.Count() == 0) return resultList.ToArray();
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            IEnumerable<string> ids = new string[] { };
            try
            {
                ids = Lst.Select(s => type.GetProperty(RootPropertyField).GetValue(s, null).ToString()).ToArray();
            }
            catch
            {
                return resultList.ToArray();
            }
            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }

            int deep = 0;
            Func<T, int> fxDetail = null;
            fxDetail = new Func<T, int>
                (
                   (parent) =>
                   {
                       deep++;
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent, null);
                       var details = Lst.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).OrderBy(TextPropertyField)
                           .AsEnumerable();
                       foreach (var m in details)
                       {
                           if (string.IsNullOrEmpty(TextPropertyField) == false)
                           {
                               var val = type.GetProperty(TextPropertyField).GetValue(m, null);
                               if (val != null)
                               {
                                   var str = val.ToString();
                                   if (str.StartsWith(PadString) == false)
                                       for (int i = 0; i < deep; i++)
                                           str = PadString + str;
                                   type.GetProperty(TextPropertyField).SetValue(m, str, null);
                               }
                           }
                           resultList.Add(m);
                           fxDetail(m);
                       }
                       deep--;
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                resultList.Add(root);
                fxDetail(root);
            }
            return resultList.ToArray();

        }
        public static T[] ToTreeList<T>(this IEnumerable<T> ObjectList, string RootPropertyField, string ParentPropertyField, string TextPropertyField)
        {
            List<T> resultList = new List<T>();
            if (ObjectList == null) return resultList.ToArray();
            //var LstObject = (object[])ObjectList;            
            var LstObject = ObjectList;
            if (LstObject.Count() == 0) return resultList.ToArray();
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            var ids = new List<string> { };
            try
            {
                foreach (var item in Lst)
                {
                    var rootVal = item.GetType().GetProperty(RootPropertyField).GetValue(item);
                    ids.Add(rootVal.ToString());
                }
            }
            catch
            {
                return resultList.ToArray();
            }
            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }

            int deep = 0;
            Func<T, int> fxDetail = null;
            fxDetail = new Func<T, int>
                (
                   (parent) =>
                   {
                       deep++;
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent, null);
                       object parentName = type.GetProperty(TextPropertyField).GetValue(parent, null);
                       var details = new List<T>();
                       foreach (var item in Lst)
                       {
                           var vall = item.GetType().GetProperty(ParentPropertyField).GetValue(item);
                           if (vall != null && vall.ToString() == parentid.ToString())
                           {
                               details.Add(item);
                           }
                       }
                       foreach (var m in details)
                       {
                           if (string.IsNullOrEmpty(TextPropertyField) == false)
                           {
                               var val = type.GetProperty(TextPropertyField).GetValue(m, null);
                               if (val != null)
                               {
                                   var str = val.ToString();
                                   str = parentName + " > " + str;
                                   type.GetProperty(TextPropertyField).SetValue(m, str, null);
                               }
                           }
                           resultList.Add(m);
                           fxDetail(m);
                       }
                       deep--;
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                resultList.Add(root);
                fxDetail(root);
            }
            return resultList.ToArray();

        }

        public static T[] ToOrderedList<T>(this T[] ObjectList, string RootPropertyField, string ParentPropertyField, string TextPropertyField, string PadString, string SetHasChildField)
        {
            List<T> resultList = new List<T>();
            if (ObjectList == null) return resultList.ToArray();
            //var LstObject = (object[])ObjectList;            
            var LstObject = ObjectList;
            if (LstObject.Length == 0) return resultList.ToArray();
            var Lst = LstObject.AsQueryable();

            var type = Lst.First().GetType();
            var ids = Lst.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).ToArray();

            List<T> roots = new List<T>();
            foreach (var l in Lst)
            {
                if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l);
                else
                {
                    var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString();
                    if (ids.Contains(bid) == false) roots.Add(l);
                }
            }

            int deep = 0;
            Func<T, int> fxDetail = null;
            fxDetail = new Func<T, int>
                (
                   (parent) =>
                   {
                       deep++;
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent, null);
                       var details = Lst.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();
                       if (!string.IsNullOrEmpty(SetHasChildField))
                       {
                           type.GetProperty(SetHasChildField).SetValue(parent, details.Length > 0, null);
                       }
                       foreach (var m in details)
                       {
                           var val = type.GetProperty(TextPropertyField).GetValue(m, null);
                           if (val != null)
                           {
                               var str = val.ToString();
                               for (int i = 0; i < deep; i++)
                                   str = PadString + str;
                               type.GetProperty(TextPropertyField).SetValue(m, str, null);
                           }
                           resultList.Add(m);
                           fxDetail(m);
                       }
                       deep--;
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                resultList.Add(root);
                fxDetail(root);
            }
            return resultList.ToArray();
        }


        public static string SpaceByCapitalLetters(this string String)
        {
            string result = "";
            for (int i = 0; i < String.Length; i++)
            {
                if (i > 0 && char.IsUpper(String[i])) result += " " + String[i].ToString().ToUpper();
                else result += String[i].ToString();
            }
            return result;
        }
        public static string[] SplitByCapitalLetters(this string String)
        {
            string result = "";
            string guid = Guid.NewGuid().ToString();
            for (int i = 0; i < String.Length; i++)
            {
                if (i > 0 && char.IsUpper(String[i])) result += guid + String[i].ToString().ToUpper();
                else result += String[i].ToString();
            }
            return result.Split(new string[] { guid }, StringSplitOptions.None);
        }



        public static string Format(this Double DoubleValue, string StringFormat)
        {
            return string.Format(StringFormat, DoubleValue);
        }
        public static string Format(this Double? doubleValue, string StringFormat)
        {
            if (doubleValue.HasValue) return Format(doubleValue.Value, StringFormat);
            else return "";
        }


        static readonly object lockObject = new object();

        static IEnumerable<string> cacheMimeTypes = new string[] { };
        public static string MimeContentType(this string extension)
        {
            IEnumerable<string> knownMimeTypes = new string[]{ "application/pdf;.pdf", "application/vnd.ms-excel;.xls", "application/msword;.doc", "image/jpeg;.jpeg", "image/jpeg;.jpg"
                                     ,"image/gif;.gif","image/png;.png","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;.xlsx",
                                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document;.docx","application/octet-stream;.bin"};
            var ext = "." + extension.Split('.').Last().ToLower();
            var mimetype = knownMimeTypes.Where(p => p.Split(';').Last() == ext).FirstOrDefault();
            if (mimetype.IsNullOrEmpty() == false) return mimetype.Split(';').FirstOrDefault();

            return "application/octet-stream";
        }

        public static string ToEngString(this string String)
        {
            return
                String.Replace("İ", "I")
                      .Replace("Ü", "U")
                      .Replace("Ö", "O")
                      .Replace("Ş", "S")
                      .Replace("Ğ", "G")
                      .Replace("Ç", "C")
                      .Replace("ı", "i")
                      .Replace("ü", "u")
                      .Replace("ö", "o")
                      .Replace("ş", "s")
                      .Replace("ğ", "g")
                      .Replace("ç", "c");
        }
        public enum KnownFileType { Pdf, Image, Doc, DocX, Xls, XlsX }
        public static bool IsImage(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "jpg", "jpeg", "gif", "png", "bmp", "tiff" };
            return exts.Contains(ext);
        }
        public static bool IsPdfFile(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "pdf" };
            return exts.Contains(ext);
        }
        public static bool IsDocFile(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "doc" };
            return exts.Contains(ext);
        }
        public static bool IsDocXFile(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "docx" };
            return exts.Contains(ext);
        }
        public static bool IsXlsFile(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "xls" };
            return exts.Contains(ext);
        }
        public static bool IsXlsXFile(this string filePath)
        {
            if (filePath.IsNullOrWhiteSpace()) return false;
            var ext = filePath.Split('.').Last().ToLower();
            var exts = new string[] { "xlsx" };
            return exts.Contains(ext);
        }
        public static bool IsKnownFileOf(this string filePath, params KnownFileType[] knownFileTypes)
        {
            if (knownFileTypes.Contains(KnownFileType.Pdf) && IsPdfFile(filePath)) return true;
            if (knownFileTypes.Contains(KnownFileType.Doc) && IsDocFile(filePath)) return true;
            if (knownFileTypes.Contains(KnownFileType.DocX) && IsDocXFile(filePath)) return true;
            if (knownFileTypes.Contains(KnownFileType.Xls) && IsXlsFile(filePath)) return true;
            if (knownFileTypes.Contains(KnownFileType.XlsX) && IsXlsXFile(filePath)) return true;
            if (knownFileTypes.Contains(KnownFileType.Image) && IsImage(filePath)) return true;
            return false;
        }
        public static System.Drawing.Size ImageSize(this string filePath)
        {
            if (filePath.IsImage())
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
                try
                {
                    return img.Size;
                }
                finally
                {
                    img.Dispose();
                }
            }
            else
            {
                return new System.Drawing.Size();
            }
        }
        /// <summary>
        /// Size to a inside max size
        /// </summary>
        /// <param name="size"></param>
        /// <param name="InSize">Yeni Max Boyutlar</param>
        /// <returns></returns>
        public static System.Drawing.Size ResizeFor(this System.Drawing.Size size, System.Drawing.Size InSize)
        {
            if (size.Width < InSize.Width && size.Height < InSize.Height)
            {
                return size;
            }

            float ratew = (float)InSize.Width / (float)size.Width;
            float rateh = (float)InSize.Height / (float)size.Height;
            double rate = (double)Math.Min(rateh, ratew);
            int w = (int)Math.Floor(((double)(size.Width * rate)));
            int h = (int)Math.Floor(((double)(size.Height * rate)));
            return new System.Drawing.Size(w, h);
        }
        public static IHtmlString ToRawHml(this string String, bool EmptyStringToHtmlEmpty = false)
        {
            if (String.IsNullOrWhiteSpace() && EmptyStringToHtmlEmpty) return new HtmlString("&nbsp;");
            return new HtmlString(String);
        }
        public static IHtmlString ToRawHtml(this int? Integer, bool NullValueToHtmlEmpty = true)
        {
            if (Integer.HasValue == false && NullValueToHtmlEmpty) return new HtmlString("&nbsp;");
            return new HtmlString(Integer.Value.ToString());
        }
        public static string ReplaceBetween(this string String, int startIndex, int endIndex, string existsvalue, string newvalue)
        {
            var part1 = String.Substring(0, startIndex);
            var part2 = String.Substring(startIndex, endIndex - startIndex);
            var part3 = String.Substring(endIndex);
            part2 = part2.Replace(existsvalue, newvalue);
            return part1 + part2 + part3;
        }
        public static string RemoveInvalidFileNameChars(this string String)
        {
            var invalids = "\\/:*?\"<>|".ToCharArray();
            foreach (var invalid in invalids)
                String = String.Replace(invalid, ' ');
            return String;
        }
        public static string WrapStr(this string String, int count)
        {
            if (String.IsNullOrWhiteSpace() == false && String.Length > count)
            {
                return String.Substring(0, count) + "...";
            }
            return String;
        }
        public static string ChangeExtension(this string String, string extension)
        {
            var lix = String.LastIndexOf('.');
            var ext = String.Substring(lix + 1);
            var exts = new string[] { "pdf", "doc", "docx", "txt", "rtf", "xls", "xlsx", "png", "gif", "jpeg", "jpg", "bmp" };
            if (exts.Contains(ext))
            {
                var str = String.Substring(0, lix) + "." + extension;
                return str;
            }
            else return String + "." + extension;
        }
        public static int StringCount(this string String, string searchString)
        {
            int cnt = 0;
            int ix = -1;
            while ((ix = String.IndexOf(searchString, searchString.Length * (ix + 1))) > -1)
            {
                cnt++;
            }
            return cnt;
        }
        public static bool ContainsAny(this string String, params string[] searchStrings)
        {
            if (String == null) return false;
            if (searchStrings == null) return false;
            foreach (var search in searchStrings)
                if (String.Contains(search)) return true;
            return false;
        }
        public static string[] SqlInvalidKeys()
        {
            return new string[] { "[use]", "[delete]", "[drop]", "[alter]", "[trigger]", "use ", " use ", "delete ", " delete ", "drop ", " drop ", "alter ", " alter ", "trigger ", " trigger " };
        }

        public static bool IsComplexPassword(this string String, int level = 1)
        {
            String = String.IsNullOrWhiteSpace() ? String : String.Trim();
            var chars = String.ToCharArray();
            var isComplex = String.IsNullOrWhiteSpace() == false && String.Length >= 4;//Min 4 Chars
            if (level > 0) isComplex = chars.Any(c => char.IsDigit(c)) && chars.Any(c => char.IsLetter(c));//Has Digit And Letter
            if (level > 1) isComplex &= chars.Any(c => char.IsUpper(c)) && chars.Any(c => char.IsLower(c));//Has upper or lower char too 
            if (level > 2) isComplex &= chars.Any(c => char.IsSymbol(c));                          // has symbol too
            if (level > 3) isComplex &= chars.Length > 7; //Min 8 Chars
            return isComplex;
        }
        public static string IsComplexDesc(this int level)
        {
            List<string> results = new List<string>() { "Şifreniz :" };
            if (level < 4) results.Add("En az 4 karakter uzunlukta olmalıdır");
            if (level > 0) results.Add("En az 1 karakter ve sayı içermelidir");
            if (level > 1) results.Add("En az 1 küçük ve büyük karakter içermelidir");
            if (level > 2) results.Add("En az 1 karakter sembol içermelidir");
            if (level > 3) results.Add("En az 8 karakter uzunlukta olmalıdır");
            return string.Join("<br/>", results);

        }

        public static double ToMB(this long Bytes)
        {
            return Math.Round((double)(Bytes / 1048576), 2);
        }
        public static double ToKB(this long Bytes)
        {
            return Math.Round((double)(Bytes / 1024), 2);
        }
        public static double ToGB(this long Bytes)
        {
            return Math.Round((double)(Bytes / 1073741824), 2);
        }
        public static double ToGB(this double KBytes)
        {
            return Math.Round((double)(KBytes / 1048576), 2);
        }
        public static string UrlEncode(this string String)
        {
            return HttpUtility.UrlEncode(String, Encoding.UTF8);
        }
        public static int FromPixelToMM(this int px)
        {
            double pxtomm = 0.264583333;
            return (int)Math.Round(px * pxtomm);
        }
        public static int FromPixelToPt(this int px)
        {
            double pxtopt = 0.75;
            return (int)Math.Round(px * pxtopt);
        }
        public static int FromPixelToPtPrinter(int px)
        {
            double pxtopt = 0.7528125;
            return (int)Math.Round(px * pxtopt);
        }

        public static int DayNumberOfWeek(this DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    return -1;
            }
        }
        public static string ToTrDay(this DateTime date)
        {

            //Bugün
            var bugun = DateTime.Today;
            //Dün
            var dun = bugun.AddDays(-1);
            //Bu Hafta
            var xg = bugun.DayOfWeek.DayNumberOfWeek();
            var buhaftaBas = bugun.AddDays(-1 * xg);
            var buhaftaSon = buhaftaBas.AddDays(6);
            //Geçen Hafta
            var gecenHaftaBas = buhaftaBas.AddDays(-7);
            var gecenHaftaSon = buhaftaBas.AddMilliseconds(-1);
            //Bu Ay
            var buayBas = new DateTime(bugun.Year, bugun.Month, 1);
            var buaySon = buayBas.AddMonths(1).AddMilliseconds(-1);
            //Geçen Ay
            var gecenAyBas = buayBas.AddMonths(-1);
            var gecenAySon = buayBas.AddMilliseconds(-1);
            //Son 3 Ay
            var gecen3AyBas = buayBas.AddMonths(-2);
            var gecen3AySon = buaySon.AddMilliseconds(-1);
            //Son 6 Ay
            var gecen6AyBas = buayBas.AddMonths(-5);
            var gecen6AySon = buaySon.AddMilliseconds(-1);
            //Bu Sene 
            var buseneBas = new DateTime(bugun.Year, 1, 1);
            var buseneSon = buseneBas.AddYears(1);
            //Geçen Sene
            var gecenSeneBas = new DateTime(bugun.Year - 1, 1, 1);
            var gecenSeneSon = buseneBas.AddMilliseconds(-1);
            //Daha Eskiler
            date = date.Date;


            if (date == bugun) return "Bugün";
            if (date == dun) return "Dün";
            if (date >= buhaftaBas && date <= buhaftaSon) return "Bu Hafta";
            if (date >= gecenHaftaBas && date <= gecenHaftaSon) return "Geçen Hafta";
            if (date >= buayBas && date <= buaySon) return "Bu Ay";
            if (date >= gecenAyBas && date <= gecenAySon) return "Geçen Ay";
            if (date >= gecen3AyBas && date <= gecen3AySon) return "Son 3 Ay";
            if (date >= gecen6AyBas && date <= gecen6AySon) return "Son 6 Ay";
            if (date >= buseneBas && date <= buseneSon) return "Bu Sene";
            if (date >= gecenSeneBas && date <= gecenSeneSon) return "Geçen Sene";
            return "Geçmiş Tarihli";
        }
        public static string ToTrDay(this DateTime? date)
        {
            if (date.HasValue == false) return "";
            else return ToTrDay(date.Value);
        }
        public static string CopyBetween(this String sourceText, string searchText, string nextSearchText)
        {
            if (string.IsNullOrEmpty(sourceText)) return sourceText;
            var ix1 = sourceText.IndexOf(searchText);
            if (ix1 > -1)
            {
                var ix2 = sourceText.IndexOf(nextSearchText, ix1 + searchText.Length);
                if (ix2 > ix1)
                {
                    var start = ix1 + searchText.Length;
                    var cnt = ix2 - start;
                    return sourceText.Substring(start, cnt);
                }
            }
            return "";
        }
        public static string Substr(this String str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }
        public static string GetConnectionStringFromEntityConnStr(string EntityName = null)
        {
            var connStr = "";
            if (EntityName.IsNullOrWhiteSpace())
                connStr = ConfigurationManager.ConnectionStrings[0].ConnectionString.ToLower();
            else connStr = ConfigurationManager.ConnectionStrings[EntityName].ConnectionString.ToLower();
            var dataSource = CopyBetween(connStr, "data source=", ";");
            if (dataSource.IsNullOrWhiteSpace())
                dataSource = CopyBetween(connStr, "server=", ";");
            var initialCatalog = CopyBetween(connStr, "initial catalog=", ";");
            if (initialCatalog.IsNullOrWhiteSpace())
                initialCatalog = CopyBetween(connStr, "database=", ";");
            var integratedSecurity = CopyBetween(connStr, "integrated security=", ";");
            if (integratedSecurity.IsNullOrWhiteSpace())
                integratedSecurity = CopyBetween(connStr, "trusted_connection", ";");
            var persistSecurityInfo = CopyBetween(connStr, "persist security info=", ";");
            if (persistSecurityInfo.IsNullOrWhiteSpace())
                persistSecurityInfo = CopyBetween(connStr, "trusted_connection", ";");
            string uid = CopyBetween(connStr, "user id=", ";");
            if (uid.IsNullOrWhiteSpace())
                uid = CopyBetween(connStr, "uid=", ";");
            string pwd = CopyBetween(connStr, "password", ";");
            if (pwd.IsNullOrWhiteSpace())
                pwd = CopyBetween(connStr, "pwd=", ";");

            return "Data Source=" + dataSource + ";Initial Catalog=" + initialCatalog +
                (((integratedSecurity == "sspi" || integratedSecurity == "true") && uid.IsNullOrWhiteSpace()) ? "integrated security=sspi" : "persisty security info=true;user id=" + uid + ";password=" + pwd + ";");
        }

        public static Dictionary<string, string> CssToDictionary(this String style)
        {
            Dictionary<string, string> xcss = new Dictionary<string, string>();
            var cssparts = style.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cssp in cssparts)
            {
                var csspxs = cssp.Split(':');
                var key = csspxs[0];
                var val = csspxs.Length > 1 ? csspxs[1] : "";
                if (xcss.ContainsKey(key)) xcss[key] = val;
                else xcss.Add(key, val);
            }
            return xcss;
        }
        public static string CssStyleMerge(this string Css, string newCss, bool overwrite = true)
        {
            Css = Css ?? "";
            newCss = newCss ?? "";
            var css1 = Css.CssToDictionary();
            var css2 = newCss.CssToDictionary();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var cs1 in css1)
                if (dict.ContainsKey(cs1.Key)) dict[cs1.Key] = cs1.Value;
                else dict.Add(cs1.Key, cs1.Value);

            foreach (var cs2 in css2)
                if (dict.ContainsKey(cs2.Key)) { if (overwrite) dict[cs2.Key] = cs2.Value; }
                else dict.Add(cs2.Key, cs2.Value);

            return string.Join(";", dict.Select(s => s.Key + ":" + s.Value).ToArray());
        }
        public static string CssStyleMergeForTag(this string str, string tag, string style, bool overwrite = true)
        {
            if (tag == null) tag = "";
            if (!tag.StartsWith("<")) tag = "<" + tag;

            var tagStart = str.IndexOf(tag);
            while (tagStart > -1)
            {
                var tagEnd = str.IndexOf(">", tagStart);
                var styleStart = str.IndexOf("style", tagStart);
                if (styleStart > tagStart && styleStart < tagEnd)
                {
                    var styleC1 = str.IndexOf("'", styleStart);
                    var styleC2 = str.IndexOf("\"", styleStart);
                    string styleChar = styleC1 > 0 && styleC1 > styleC2 ? "'" : "\"";

                    styleStart = str.IndexOf(styleChar, styleStart);
                    var styleEnd = str.IndexOf(styleChar, styleStart + 1);

                    var css = str.Substr(styleStart + 1, styleEnd);
                    var str1 = css.CssStyleMerge(style);
                    str = str.ReplaceBetween(styleStart, styleEnd, css, str1);
                }
                else
                {
                    str = str.Insert(tagEnd, " style=\"" + style.Replace("\"", "'") + "\"");
                }
                tagStart = str.IndexOf(tag, tagStart + 1);
            }
            return str;
        }
        public static string ToPx(this int i)
        {
            return i.ToString() + "px";
        }
        public static MvcHtmlString ToChecked(this bool? attrChecked)
        {
            return new MvcHtmlString(attrChecked.HasValue && attrChecked.Value ? "checked='checked'" : "");
        }
        public static MvcHtmlString ToChecked(this bool attrChecked)
        {
            return new MvcHtmlString(attrChecked ? "checked='checked'" : "");
        }
        public static MvcHtmlString ToChecked(this bool attrChecked, bool state)
        {
            return new MvcHtmlString(attrChecked == state ? "checked='checked'" : "");
        }

        public static MvcHtmlString ToChecked(this bool? attrChecked, bool state)
        {
            return new MvcHtmlString(attrChecked.HasValue && attrChecked.Value == state ? "checked='checked'" : "");
        }
        public static MvcHtmlString ToSelected(this bool? attSelected)
        {
            return new MvcHtmlString(attSelected.HasValue && attSelected.Value ? "selected='selected'" : "");
        }
        public static MvcHtmlString ToSelected(this bool attSelected)
        {
            return new MvcHtmlString(attSelected ? "selected='selected'" : "");
        }
        public static MvcHtmlString ToSelected(this bool attSelected, bool state)
        {
            return new MvcHtmlString(attSelected == state ? "selected='selected'" : "");
        }
        public static MvcHtmlString ToSelected(this bool? attSelected, bool state)
        {
            return new MvcHtmlString(attSelected.HasValue && attSelected.Value == state ? "selected='selected'" : "");
        }
        public static MvcHtmlString ToString(this bool boolValue, string IfTrue, string IfFalse)
        {
            return new MvcHtmlString(boolValue ? IfTrue : IfFalse);
        }
        public static MvcHtmlString ToString(this bool? boolValue, string IfTrue, string IfFalse, string IfNull)
        {
            return new MvcHtmlString(boolValue.HasValue == false ? IfNull : (boolValue.Value ? IfTrue : IfFalse));
        }


        public static string ImageToBase64(this Image image)
        {
            return ImageToBase64(image, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        public static string ImageToBase64(this Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
        public static string ImageToBase64SrcData(this Image image, System.Drawing.Imaging.ImageFormat format)
        {
            var imgFormat = format.ToString().ToLower();
            var base64 = ImageToBase64(image, format);
            return string.Format("data:image/{0};base64,{1}", imgFormat, base64);
        }


        public static bool InRole(this string userRole)
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                bool hasRole = HttpContext.Current.User.IsInRole(userRole);
                if (!hasRole && UserIdentity.Current != null)
                    hasRole = UserIdentity.Current.Roles.Contains(userRole);
                return hasRole;
            }
            return false;
        }
        public static string VirutalPath
        {
            get
            {
                var rootPath = HttpRuntime.AppDomainAppVirtualPath;
                rootPath = rootPath.EndsWith("/") ? rootPath : rootPath + "/";
                return rootPath;
            }
        }
        public static string WebSitePath
        {
            get
            {
                var webSite = "http://" + HttpContext.Current.Request.Url.Authority + "/";
                var wp = HttpContext.Current.Request.Url.AbsolutePath;
                var abs = HttpContext.Current.Request.Url.AbsoluteUri;
                if (wp != "/")
                {
                    var ix = abs.IndexOf(wp);
                    if (ix > -1) webSite = abs.Substring(0, ix);
                }
                else webSite = wp;
                return webSite;
            }
        }
        public static Int64 GetInt64HashCode(this string strText)
        {
            Int64 hashCode = 0;
            if (!string.IsNullOrEmpty(strText))
            {
                //Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                System.Security.Cryptography.SHA256 hash =
                new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                //32Byte hashText separate
                //hashCodeStart = 0~7  8Byte
                //hashCodeMedium = 8~23  8Byte
                //hashCodeEnd = 24~31  8Byte
                //and Fold
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return (hashCode);
        }
        public static int ToCrc32(this string strText)
        {
            var x = Crc32.CRC32String(strText);
            try
            {
                return (int)x;
            }
            catch
            {
                return (int)Math.Round((double)(x / 3));
            }
            // crc.CRC32String()
        }
        public static int ToCrc16(this string strText)
        {
            var x = Crc16.ComputeChecksum(strText);
            try
            {
                return (int)x;
            }
            catch
            {
                return ToCrc32(strText);
            }
        }
        public static T[] Add<T>(this T[] arrList, T Value)
        {
            var lst = arrList.ToList();
            lst.Add(Value);
            return lst.ToArray();
        }
        public static T[] Remove<T>(this T[] arrList, T Value)
        {
            var lst = arrList.ToList();
            lst.Remove(Value);
            return lst.ToArray();
        }
        public static object ParseTo(this object obj, Type toType)
        {
            if (obj.GetType() == toType)
                return obj;
            if (obj == null) return null;
            if (toType == typeof(int))
            {
                return ToInt(obj.ToString(), 0);
            }
            if (toType == typeof(long))
            {
                return ToLong(obj.ToString(), 0);
            }
            if (toType == typeof(short))
            {
                short x = 0;
                short.TryParse(obj.ToString(), out x);
                return x;
            }
            if (toType == typeof(byte))
            {
                byte x = 0;
                byte.TryParse(obj.ToString(), out x);
                return x;
            }
            if (toType == typeof(ushort))
            {
                ushort x = 0;
                ushort.TryParse(obj.ToString(), out x);
                return x;
            }
            if (toType == typeof(uint))
            {
                uint x = 0;
                uint.TryParse(obj.ToString(), out x);
                return x;
            }
            if (toType == typeof(bool))
            {
                return ToBoolean(obj.ToString(), false);
            }
            if (toType == typeof(string))
            {
                return obj.ToString();
            }
            if (toType == typeof(DateTime))
            {
                DateTime t = DateTime.Now;
                DateTime.TryParse(obj.ToString(), out t);
                return t;
            }
            else
            {
                try
                {
                    var objx = Convert.ChangeType(obj, toType);
                    return objx;
                }
                catch { }
            }
            return null;

        }
        public static T SetValues<T>(this T obj, FormCollection formCollection)
        {
            var props = typeof(T).GetProperties();
            foreach (var key in formCollection.AllKeys)
            {
                var xprop = props.Where(p => p.Name == key).FirstOrDefault();
                if (xprop != null)
                {
                    var objx = ParseTo(formCollection[key], xprop.PropertyType);
                    if (objx != null)
                        xprop.SetValue(obj, objx, null);
                }
            }
            return obj;
        }
        public static T SetValueFromHttpContext<T>(this T obj)
        {
            var props = typeof(T).GetProperties();
            foreach (var key in HttpContext.Current.Request.Params.AllKeys)
            {
                var xprop = props.Where(p => p.Name == key).FirstOrDefault();
                if (xprop != null)
                {
                    var objx = ParseTo(HttpContext.Current.Request.Params[key], xprop.PropertyType);
                    if (objx != null)
                        xprop.SetValue(obj, objx, null);
                }
            }
            return obj;
        }
        public static string GetFullPath(this string Path)
        {
            if (Path.IsNullOrWhiteSpace()) return "";
            var path = Path;
            if (Path.StartsWith("~"))
                path = HttpContext.Current.Server.MapPath(Path);
            path = path.EndsWith("\\") ? path.Substring(0, path.Length - 1) : path;
            return path;
        }
        public static string GetFullPathFromConfig(this string Path)
        {
            return GetFullPath(ConfigurationManager.AppSettings[Path]);
        }

        public static System.Dynamic.ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new System.Web.Routing.RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (System.Dynamic.ExpandoObject)expando;
        }

        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetAllFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }
}
