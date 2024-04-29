
using BiskaUtil;
using Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;

namespace WebApp.Models
{
    public struct DateTimeSpan
    {
        private readonly int years;
        private readonly int months;
        private readonly int days;
        private readonly int hours;
        private readonly int minutes;
        private readonly int seconds;
        private readonly int milliseconds;

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            this.years = years;
            this.months = months;
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.milliseconds = milliseconds;
        }

        public int Years { get { return years; } }
        public int Months { get { return months; } }
        public int Days { get { return days; } }
        public int Hours { get { return hours; } }
        public int Minutes { get { return minutes; } }
        public int Seconds { get { return seconds; } }
        public int Milliseconds { get { return milliseconds; } }

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date1;
            int years = 0;
            int months = 0;
            int days = 0;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }
                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                        }
                        else
                        {
                            months++;
                        }
                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }
                        break;
                }
            }

            return span;
        }


    }
    public static class Management
    {
        public static string Tuz = "@BİSKAmcumu";
        public static int UniversiteYtuKod { get; } = 67;

        public const int ImageMinimumBytes = 512;
        #region Extensions
        public static bool IsImage(this HttpPostedFileBase postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.InputStream.CanRead)
                {
                    return false;
                }

                if (postedFile.ContentLength < Management.ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static string ToIntToStrAy_Gun(this int AyID)
        {
            var strAy = AyID.ToString();
            return strAy.Length == 1 ? "0" + strAy : strAy;
        }
        public static int ToHaftaNo(this DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static int ToGunSiralamaDegeri(this DayOfWeek gun)
        {
            var gunx = (int)gun;
            return gunx == 0 ? 7 : gunx;
        }
        public static int ToGunDegeri(this DayOfWeek gun)
        {
            return (int)gun;
        }


        public static DateTime ToAyinIlkPt(this DateTime tarih)
        {
            tarih = tarih.AddDays(1 - tarih.Day);
            if (tarih.DayOfWeek != DayOfWeek.Monday)
            {
                tarih = tarih.AddDays(1 - tarih.DayOfWeek.ToGunSiralamaDegeri());
            }
            return tarih;
        }
        public static DateTime ToHaftaIlkPt(this DateTime tarih)
        {
            if (tarih.DayOfWeek != DayOfWeek.Monday)
            {
                tarih = tarih.AddDays(-(tarih.DayOfWeek.ToGunSiralamaDegeri() - 1));
                return tarih;
            }
            else return tarih;

        }
        public static DateTime ToAyinSonPz(this DateTime tarih)
        {
            tarih = tarih.AddMonths(1).AddDays(-tarih.Day);
            if (tarih.DayOfWeek != DayOfWeek.Sunday)
            {
                tarih = tarih.AddDays(-tarih.DayOfWeek.ToGunSiralamaDegeri());
            }
            return tarih;
        }

        public static DateTime ToAyinSnGn(this DateTime tarih)
        {
            return new DateTime(tarih.Year, tarih.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);
            int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1)
            { weekOfYear -= 1; }
            return firstMonday.AddDays(weekOfYear * 7);
        }
        public static int ToAyHaftaNo(this DateTime Tarih)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int yil = Tarih.Year;
            int ay = Tarih.Month;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(Tarih, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (weekNum > 51)
                weekNum = 1;
            if ((weekNum == 1) && (ay == 1))
            {
                DateTime tmpdate = FirstDateOfWeek(yil, weekNum);
                if ((tmpdate.Day > 1) && (tmpdate.Month == 1))
                    weekNum = 0;
            }
            return weekNum;
        }
        public static decimal? ToMoney(this string moneyString)
        {
            var groupSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
            var decimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
            return ToMoney(moneyString, decimalSeparator, groupSeparator);
        }
        public static decimal ToMoney(this string moneyString, decimal defaultValue)
        {
            var ms = ToMoney(moneyString);
            return (ms ?? defaultValue);
        }
        public static decimal? ToMoney(this string moneyString, string decimalSeparator, string groupSeparator)
        {
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var moneyStr = string.Join("",
                                          moneyString
                                          .ToCharArray()
                                          .Where(p => (p.ToString() == groupSeparator || p.ToString() == decimalSeparator || numbers.Contains(p))).ToArray()
                                      );
            decimal def;
            if (decimal.TryParse(moneyStr, out def))
            {
                return def;
            }
            return null;
        }

        public static bool ToIsValidEmail(this string Email)
        {
            return !Regex.IsMatch(Email,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
                RegexOptions.IgnoreCase);
        }
        public static string ToExceptionMessage(this Exception ex)
        {
            int ix = 1;
            Dictionary<int, string> msgs = new Dictionary<int, string>() { { ix, ex.Message } };
            var innException = ex;
            while ((innException = innException.InnerException) != null)
            {
                ix++;
                msgs.Add(ix, innException.Message);
            }
            var returnMsg = string.Join("\r\n", msgs.Select(s => s.Key + "- " + s.Value).ToArray());

            if (ex is DbEntityValidationException)
            {
                var msgsVex = new List<string>();
                var exV = (DbEntityValidationException)ex;
                foreach (var eve in exV.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        msgsVex.Add(string.Format("State: {0} Property: {1}, Error: {2}", eve.Entry.State, ve.PropertyName, ve.ErrorMessage));
                    }
                }
                if (msgsVex.Any())
                {
                    msgsVex.Insert(0, "Veri Giriş Hataları:");
                    returnMsg += "\r\n" + string.Join("\r\n", msgsVex);
                }
            }
            return returnMsg;
        }
        public static string ToExceptionStackTrace(this Exception ex)
        {
            Dictionary<int, string> stck = new Dictionary<int, string>();

            int ix = 1;
            var innException = ex;
            stck.Add(ix, ex.StackTrace);
            while ((innException = innException.InnerException) != null)
            {
                ix++;
                stck.Add(ix, innException.StackTrace);
            }
            return string.Join("\r\n", stck.Select(s => s.Key + "- " + s.Value).ToArray());
        }


        public static string RenderPartialView(string controllerName, string partialView, object model)
        {
            if (HttpContext.Current == null)
                HttpContext.Current = new HttpContext(
                                        new HttpRequest(null, "http://www.WebApp.yildiz.edu.tr", null),
                                        new HttpResponse(null));
            var context = new HttpContextWrapper(System.Web.HttpContext.Current) as HttpContextBase;
            var routes = new System.Web.Routing.RouteData();
            routes.Values.Add("controller", controllerName);
            var requestContext = new System.Web.Routing.RequestContext(context, routes);
            string requiredString = requestContext.RouteData.GetRequiredString("controller");
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();
            var controller = controllerFactory.CreateController(requestContext, requiredString) as ControllerBase;
            controller.ControllerContext = new ControllerContext(context, routes, controller);
            var ViewData = new ViewDataDictionary();
            var TempData = new TempDataDictionary();
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialView);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }


        public static IHtmlString ToRenderPartialViewHtml(this object model, string controllerName, string partialView)
        {
            var strView = RenderPartialView(controllerName, partialView, model);
            return new HtmlString(strView);
        }



        public static List<string> ToIkiKarakterArasiKodBul(this string body, string start, string end)
        {
            List<string> matched = new List<string>();

            int indexStart = 0;
            int indexEnd = 0;

            bool exit = false;
            while (!exit)
            {
                indexStart = body.IndexOf(start);
                indexEnd = body.IndexOf(end);
                if (indexStart != -1 && indexEnd != -1)
                {
                    matched.Add(body.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length));
                    body = body.Substring(indexEnd + end.Length);
                }
                else
                {
                    exit = true;
                }
            }

            return matched;
        }
        public static string ReplaceSpecialCharacter(this string gelenStr)
        {
            return Regex.Replace(gelenStr, @"([^a-zA-Z0-9_]|^\s)", string.Empty);

        }

        public static bool IsSpecialCharacterCheck(this string gelenStr)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            return regexItem.IsMatch(gelenStr);
        }

        public static Image CreateQrCode(this string Kod, int Width = 360, int Height = 360)
        {
            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", Kod, Width, Height);
            WebResponse response = default(WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            WebRequest request = WebRequest.Create(url);
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            System.Drawing.Image img = System.Drawing.Image.FromStream(remoteStream);

            response.Close();
            remoteStream.Close();
            readStream.Close();
            return img;
        }

        public static bool IsImage(this string Uzanti)
        {
            return new List<string>() { "Png", "Jpg", "Bmp", "Tif", "Gif" }.Contains(Uzanti);
        }
        public static DateTime TodateToShortDate(this DateTime Tarih)
        {
            var data1 = Tarih.ToDateString().ToDate().Value;
            return data1;
        }
        public static DateTime? TodateToShortDate(this DateTime? Tarih)
        {
            if (Tarih != null) return Tarih.ToDateString().ToDate().Value;
            else return null;
        }
        public static string ToBelirtilmemis(this int? Sayi)
        {
            if (!Sayi.HasValue) return "Belirtilmemiş";
            else return Sayi.Value.ToString();

        }
        public static string ToCinsiyet(this bool? IsErkek)
        {
            var cins = "";
            if (!IsErkek.HasValue) cins = "Belirtilmemiş";
            else if (IsErkek.Value) cins = "Erkek";
            else cins = "Kadın";
            return cins;

        }
        public static string ToEvliBekar(this bool? IsEvli)
        {
            var mHal = "";
            if (!IsEvli.HasValue) mHal = "Belirtilmemiş";
            else if (IsEvli.Value) mHal = "Evli";
            else if (!IsEvli.Value) mHal = "Bekar";
            else mHal = IsEvli.Value.ToString();
            return mHal;
        }
        public static string ToBelirtilmemis(this double? MetreKare)
        {
            if (!MetreKare.HasValue) return "Belirtilmemiş";
            else return MetreKare.Value.ToString() + "m2";

        }
        public static string ToBelirtilmemis(this string str)
        {
            if (str.IsNullOrWhiteSpace()) return "Belirtilmemiş";
            else return str;

        }

        public static string ToFormatDateAndTime(this DateTime datetime)
        {
            if (datetime == DateTime.MinValue) return "";
            else return datetime.ToString("dd-MM-yyyy HH:mm");

        }
        public static string ToFormatDateAndTime(this DateTime? datetime)
        {
            if (datetime == DateTime.MinValue || datetime.HasValue == false) return "";
            else return datetime.ToString("dd-MM-yyyy HH:mm");

        }
        public static string ToFormatDate(this DateTime datetime)
        {
            if (datetime == DateTime.MinValue) return "";
            else return datetime.ToString("dd-MM-yyyy");

        }
        public static string ToFormatDate(this DateTime? datetime)
        {
            if (!datetime.HasValue) return "";
            else return datetime.ToString("dd-MM-yyyy");

        }
        public static string ToHizmetYili(this int ToplamGun)
        {
            var Hyil = ToplamGun / 360;
            var HAy = (ToplamGun % 360) / 30;
            var hGun = ToplamGun % (30);
            var str = "";
            if (Hyil > 0) str += Hyil.ToString() + " Yıl ";
            if (HAy > 0) str += HAy.ToString() + " Ay ";
            if (hGun > 0) str += hGun.ToString() + " Gün";
            if (Hyil == 0 && hGun == 0 && HAy == 0) str = "0";
            return str;
        }
        public static string ToHizmetYili(this double ToplamGun)
        {
            var toplamgun = (int)ToplamGun;
            var Hyil = toplamgun / 360;
            var HAy = (toplamgun % 360) / 30;
            var hGun = toplamgun % (30);
            var str = "";
            if (Hyil > 0) str += Hyil.ToString() + " Yıl ";
            if (HAy > 0) str += HAy.ToString() + " Ay ";
            if (hGun > 0) str += hGun.ToString() + " Gün";
            if (Hyil == 0 && hGun == 0 && HAy == 0) str = "0";
            return str;
        }
        public static TimeSpan ToKalanSure(this DateTime GirisTarihi, DateTime BulundugumuzGun, int ToplamOturacagiAy)
        {
            var tms = new TimeSpan();
            var CikmasiGerekenTarih = GirisTarihi.AddMonths(ToplamOturacagiAy);
            tms = CikmasiGerekenTarih - BulundugumuzGun.TodateToShortDate();
            return tms;
        }
        public static Image ToResizeImage(this Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)sourceWidth / (float)size.Width);
            nPercentH = ((float)sourceHeight / (float)size.Height);

            if (nPercentH > nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth / nPercent);
            int destHeight = (int)(sourceHeight / nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.Bicubic;
            b.SetResolution(200, 200);
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }
        public static string ToKullaniciResim(this string ResimAdi)
        {
            var rsm = ResimAdi.IsNullOrWhiteSpace() ? (Management.GetRoot() + SistemAyar.getAyar(SistemAyar.KullaniciDefaultResim)) : (Management.GetRoot() + SistemAyar.getAyar(SistemAyar.KullaniciResimYolu) + "/" + ResimAdi);
            return rsm;
        }
        public static JsonResult ToJsonResult(this object obj)
        {
            var jsr = new JsonResult
            {
                ContentEncoding = System.Text.Encoding.UTF8,
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = obj
            };
            return jsr;
        }
        public static string ToJsonText(this object obj)
        {
            var jss = new JavaScriptSerializer();
            var table = jss.Serialize(obj);

            return table;
        }

        public static string ToEmptyStringZero(this object obj)
        {
            string retval = "";
            if (obj != null && obj.ToString() != "0") retval = obj.ToString();
            return retval;
        }
        public static int? ToNullIntZero(this object obj)
        {
            int? retval = null;
            if (obj != null && obj.ToString() != "0") retval = obj.ToString().ToInt();
            return retval;
        }
        public static int? ToIntObj(this object obj)
        {
            if (obj != null && obj.IsNumber()) return Convert.ToInt32(Math.Round(Convert.ToDouble(obj)));
            else return (int?)null;
        }
        public static int? ToIntObjTrimSpace(this object obj)
        {
            if (obj != null) obj = obj.ToString().Trim();
            if (obj.IsNumber()) return Convert.ToInt32(obj);
            else return (int?)null;
        }
        public static double? ToDoubleObj(this object obj)
        {
            if (obj != null && obj.IsNumber()) return Convert.ToDouble(obj);
            else return (double?)null;
        }
        public static bool? ToBooleanObj(this object obj)
        {
            if (obj == null) return null;
            if (obj.ToString() == "1") obj = "true";
            else if (obj.ToString() == "0") obj = "false";
            bool dgr;
            if (obj != null && bool.TryParse(obj.ToString(), out dgr))
            {
                return Convert.ToBoolean(obj);
            }
            else return (bool?)null;
        }
        public static decimal? ToDecimalObj(this object obj)
        {
            if (obj != null && obj.IsNumber()) return Convert.ToDecimal(obj);
            else return (decimal?)null;
        }
        public static DateTime? ToDatetimeObj(this object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToDateTime(obj.ToString());
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else return null;
        }
        public static string ToStrObj(this object obj)
        {
            if (obj != null) return Convert.ToString(obj);
            else return (string)null;
        }
        public static string ToStrObjEmptString(this object obj)
        {
            if (obj != null) return Convert.ToString(obj).Trim();
            else return "";
        }
        public static bool IsNumber(this object value)
        {
            double sayi;
            return double.TryParse(value.ToString(), out sayi);
        }

        public static bool IsNumberX(this object value)
        {
            int Deger;
            var durum = int.TryParse(value.ToStrObj(), out Deger);
            return durum;
        }
        public static bool IsURL(this string source)
        {
            return Uri.IsWellFormedUriString(source, UriKind.RelativeOrAbsolute);
        }

        public static bool IsValidUrl(this string urlString)
        {
            if (urlString.IsNullOrWhiteSpace()) return false;
            Uri uri;
            return Uri.TryCreate(urlString, UriKind.RelativeOrAbsolute, out uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                 || uri.Scheme == Uri.UriSchemeHttps
                 || uri.Scheme == Uri.UriSchemeFtp
                 || uri.Scheme == Uri.UriSchemeMailto
                 );
        }

        public static string ToDeviceType(this string ua)
        {
            string ret = "";
            // Check if user agent is a smart TV - http://goo.gl/FocDk
            if (Regex.IsMatch(ua, @"GoogleTV|SmartTV|Internet.TV|NetCast|NETTV|AppleTV|boxee|Kylo|Roku|DLNADOC|CE\-HTML", RegexOptions.IgnoreCase))
            {
                ret = "Tv";
            }
            // Check if user agent is a TV Based Gaming Console
            else if (Regex.IsMatch(ua, "Xbox|PLAYSTATION.3|Wii", RegexOptions.IgnoreCase))
            {
                ret = "Tv";
            }
            // Check if user agent is a Tablet
            else if ((Regex.IsMatch(ua, "iP(a|ro)d", RegexOptions.IgnoreCase) || (Regex.IsMatch(ua, "tablet", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "RX-34", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "FOLIO", RegexOptions.IgnoreCase))))
            {
                ret = "Tablet";
            }
            // Check if user agent is an Android Tablet
            else if ((Regex.IsMatch(ua, "Linux", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Android", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Fennec|mobi|HTC.Magic|HTCX06HT|Nexus.One|SC-02B|fone.945", RegexOptions.IgnoreCase)))
            {
                ret = "Tablet";
            }
            // Check if user agent is a Kindle or Kindle Fire
            else if ((Regex.IsMatch(ua, "Kindle", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "Mac.OS", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Silk", RegexOptions.IgnoreCase)))
            {
                ret = "Tablet";
            }
            // Check if user agent is a pre Android 3.0 Tablet
            else if ((Regex.IsMatch(ua, @"GT-P10|SC-01C|SHW-M180S|SGH-T849|SCH-I800|SHW-M180L|SPH-P100|SGH-I987|zt180|HTC(.Flyer|\\_Flyer)|Sprint.ATP51|ViewPad7|pandigital(sprnova|nova)|Ideos.S7|Dell.Streak.7|Advent.Vega|A101IT|A70BHT|MID7015|Next2|nook", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "MB511", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "RUTEM", RegexOptions.IgnoreCase)))
            {
                ret = "Tablet";
            }
            // Check if user agent is unique Mobile User Agent
            else if ((Regex.IsMatch(ua, "BOLT|Fennec|Iris|Maemo|Minimo|Mobi|mowser|NetFront|Novarra|Prism|RX-34|Skyfire|Tear|XV6875|XV6975|Google.Wireless.Transcoder", RegexOptions.IgnoreCase)))
            {
                ret = "Mobile";
            }
            // Check if user agent is an odd Opera User Agent - http://goo.gl/nK90K
            else if ((Regex.IsMatch(ua, "Opera", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "Windows.NT.5", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, @"HTC|Xda|Mini|Vario|SAMSUNG\-GT\-i8000|SAMSUNG\-SGH\-i9", RegexOptions.IgnoreCase)))
            {
                ret = "Mobile";
            }
            // Check if user agent is Windows Desktop
            else if ((Regex.IsMatch(ua, "Windows.(NT|XP|ME|9)")) && (!Regex.IsMatch(ua, "Phone", RegexOptions.IgnoreCase)) || (Regex.IsMatch(ua, "Win(9|.9|NT)", RegexOptions.IgnoreCase)))
            {
                ret = "Desktop";
            }
            // Check if agent is Mac Desktop
            else if ((Regex.IsMatch(ua, "Macintosh|PowerPC", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Silk", RegexOptions.IgnoreCase)))
            {
                ret = "Desktop";
            }
            // Check if user agent is a Linux Desktop
            else if ((Regex.IsMatch(ua, "Linux", RegexOptions.IgnoreCase)) && (Regex.IsMatch(ua, "X11", RegexOptions.IgnoreCase)))
            {
                ret = "Desktop";
            }
            // Check if user agent is a Solaris, SunOS, BSD Desktop
            else if ((Regex.IsMatch(ua, "Solaris|SunOS|BSD", RegexOptions.IgnoreCase)))
            {
                ret = "Desktop";
            }
            // Check if user agent is a Desktop BOT/Crawler/Spider
            else if ((Regex.IsMatch(ua, "Bot|Crawler|Spider|Yahoo|ia_archiver|Covario-IDS|findlinks|DataparkSearch|larbin|Mediapartners-Google|NG-Search|Snappy|Teoma|Jeeves|TinEye", RegexOptions.IgnoreCase)) && (!Regex.IsMatch(ua, "Mobile", RegexOptions.IgnoreCase)))
            {
                ret = "Desktop";
            }
            // Otherwise assume it is a Mobile Device
            else
            {
                ret = "Mobile";
            }
            return ret;
        }



        public static UrlInfoModel ToUrlInfo(this Uri uri)
        {
            var model = new UrlInfoModel
            {
                Root = Management.GetRoot(),
                AbsolutePath = uri.AbsolutePath.Replace("I", "i").ToLower()
            };
            var spl = model.AbsolutePath.Split('/').Where(p => p != "").ToList();

            if (spl.Count == 0)
            {
                model.LastPath = model.Root + "home" + "/index";
            }

            else if (spl.Count >= 1)
            {
                model.LastPath = model.Root + spl[0] + "/index";
            }
            model.Query = uri.Query;



            return model;
        }


        // Stores user-entered primitives like X = 10.
        public static Dictionary<string, string> Primatives;
        public enum Precedence
        {
            None = 11,
            Unary = 10,     // Not actually used.
            Power = 9,      // We use ^ to mean exponentiation.
            Times = 8,
            Div = 7,
            Modulus = 6,
            Plus = 5,
        }
        // Evaluate the expression.
        public static double EvaluateExpression(this string expression)
        {
            int best_pos = 0;
            int parens = 0;

            // Remove all spaces.
            string expr = expression.Replace(" ", "");
            int expr_len = expr.Length;
            if (expr_len == 0) return 0;

            // If we find + or - now, then it's a unary operator.
            bool is_unary = true;

            // So far we have nothing.
            Precedence best_prec = Precedence.None;

            // Find the operator with the lowest precedence.
            // Look for places where there are no open
            // parentheses.
            for (int pos = 0; pos < expr_len; pos++)
            {
                // Examine the next character.
                string ch = expr.Substring(pos, 1);

                // Assume we will not find an operator. In
                // that case, the next operator will not
                // be unary.
                bool next_unary = false;

                if (ch == " ")
                {
                    // Just skip spaces. We keep them here
                    // to make the error messages easier to
                }
                else if (ch == "(")
                {
                    // Increase the open parentheses count.
                    parens += 1;

                    // A + or - after "(" is unary.
                    next_unary = true;
                }
                else if (ch == ")")
                {
                    // Decrease the open parentheses count.
                    parens -= 1;

                    // An operator after ")" is not unary.
                    next_unary = false;

                    // If parens < 0, too many )'s.
                    if (parens < 0)
                        throw new FormatException(
                            "Too many close parentheses in '" +
                            expression + "'");
                }
                else if (parens == 0)
                {
                    // See if this is an operator.
                    if ((ch == "^") || (ch == "*") ||
                        (ch == "/") || (ch == "\\") ||
                        (ch == "%") || (ch == "+") ||
                        (ch == "-"))
                    {
                        // An operator after an operator
                        // is unary.
                        next_unary = true;

                        // See if this operator has higher
                        // precedence than the current one.
                        switch (ch)
                        {
                            case "^":
                                if (best_prec >= Precedence.Power)
                                {
                                    best_prec = Precedence.Power;
                                    best_pos = pos;
                                }
                                break;

                            case "*":
                            case "/":
                                if (best_prec >= Precedence.Times)
                                {
                                    best_prec = Precedence.Times;
                                    best_pos = pos;
                                }
                                break;

                            case "%":
                                if (best_prec >= Precedence.Modulus)
                                {
                                    best_prec = Precedence.Modulus;
                                    best_pos = pos;
                                }
                                break;

                            case "+":
                            case "-":
                                // Ignore unary operators
                                // for now.
                                if ((!is_unary) &&
                                    best_prec >= Precedence.Plus)
                                {
                                    best_prec = Precedence.Plus;
                                    best_pos = pos;
                                }
                                break;
                        } // End switch (ch)
                    } // End if this is an operator.
                } // else if (parens == 0)

                is_unary = next_unary;
            } // for (int pos = 0; pos < expr_len; pos++)

            // If the parentheses count is not zero,
            // there's a ) missing.
            if (parens != 0)
            {
                throw new FormatException(
                    "Missing close parenthesis in '" +
                    expression + "'");
            }

            // Hopefully we have the operator.
            if (best_prec < Precedence.None)
            {
                string lexpr = expr.Substring(0, best_pos);
                string rexpr = expr.Substring(best_pos + 1);
                switch (expr.Substring(best_pos, 1))
                {
                    case "^":
                        return Math.Pow(
                            EvaluateExpression(lexpr),
                            EvaluateExpression(rexpr));
                    case "*":
                        return
                            EvaluateExpression(lexpr) *
                            EvaluateExpression(rexpr);
                    case "/":
                        return
                            EvaluateExpression(lexpr) /
                            EvaluateExpression(rexpr);
                    case "%":
                        return
                            EvaluateExpression(lexpr) %
                            EvaluateExpression(rexpr);
                    case "+":
                        return
                            EvaluateExpression(lexpr) +
                            EvaluateExpression(rexpr);
                    case "-":
                        return
                            EvaluateExpression(lexpr) -
                            EvaluateExpression(rexpr);
                }
            }

            // if we do not yet have an operator, there
            // are several possibilities:
            //
            // 1. expr is (expr2) for some expr2.
            // 2. expr is -expr2 or +expr2 for some expr2.
            // 3. expr is Fun(expr2) for a function Fun.
            // 4. expr is a primitive.
            // 5. It's a literal like "3.14159".

            // Look for (expr2).
            if (expr.StartsWith("(") && expr.EndsWith(")"))
            {
                // Remove the parentheses.
                return EvaluateExpression(expr.Substring(1, expr_len - 2));
            }

            // Look for -expr2.
            if (expr.StartsWith("-"))
            {
                return -EvaluateExpression(expr.Substring(1));
            }

            // Look for +expr2.
            if (expr.StartsWith("+"))
            {
                return EvaluateExpression(expr.Substring(1));
            }

            // Look for Fun(expr2).
            if (expr_len > 5 && expr.EndsWith(")"))
            {
                // Find the first (.
                int paren_pos = expr.IndexOf("(");
                if (paren_pos > 0)
                {
                    // See what the function is.
                    string lexpr = expr.Substring(0, paren_pos);
                    string rexpr = expr.Substring(paren_pos + 1, expr_len - paren_pos - 2);
                    switch (lexpr.ToLower())
                    {
                        case "sin":
                            return Math.Sin(EvaluateExpression(rexpr));
                        case "cos":
                            return Math.Cos(EvaluateExpression(rexpr));
                        case "tan":
                            return Math.Tan(EvaluateExpression(rexpr));
                        case "sqrt":
                            return Math.Sqrt(EvaluateExpression(rexpr));
                        case "factorial":
                            return Factorial(EvaluateExpression(rexpr));
                            // Add other functions (including
                            // program-defined functions) here.
                    }
                }
            }
            Primatives = new Dictionary<string, string>();
            Primatives.Add("Pi", "3.14159265");
            // See if it's a primitive.
            if (Primatives.ContainsKey(expr))
            {
                // Return the corresponding value,
                // converted into a Double.
                try
                {
                    // Try to convert the expression into a value.
                    return double.Parse(Primatives[expr]);
                }
                catch (Exception)
                {
                    throw new FormatException(
                        "Primative '" + expr +
                        "' has value '" +
                        Primatives[expr] +
                        "' which is not a Double.");
                }
            }

            // It must be a literal like "2.71828".
            try
            {
                // Try to convert the expression into a Double.
                return double.Parse(expr);
            }
            catch (Exception)
            {
                throw new FormatException(
                    "Error evaluating '" + expression +
                    "' as a constant.");
            }
        }

        // Return the factorial of the expression.
        public static double Factorial(double value)
        {
            // Make sure the value is an integer.
            if ((long)value != value)
            {
                throw new ArgumentException(
                    "Parameter to Factorial function must be an integer in Factorial(" +
                    value.ToString() + ")");
            }

            double result = 1;
            for (int i = 2; i <= value; i++)
            {
                result *= i;
            }
            return result;
        }
        #endregion
        #region Yetki/Kimlik
        public static void Update()
        {
            UpdateRoles2();
            UpdateMenus2();
        }
        static void UpdateRoles2()
        {
            var roleAttrs = Membership.Roles();
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var dbRoller = db.Rollers.ToArray();
                foreach (var attr in roleAttrs)
                {
                    var dbrole = dbRoller.FirstOrDefault(p => p.RolID == attr.RolID);

                    if (dbrole == null)
                    {
                        db.Rollers.Add(new Roller
                        {
                            RolID = attr.RolID,
                            GorunurAdi = attr.GorunurAdi,
                            Aciklama = attr.Aciklama,
                            Kategori = attr.Kategori,
                            RolAdi = attr.RolAdi
                        });
                    }
                    else
                    {
                        dbrole.RolID = attr.RolID;
                        dbrole.GorunurAdi = attr.GorunurAdi;
                        dbrole.Aciklama = attr.Aciklama;
                        dbrole.Kategori = attr.Kategori;
                        dbrole.RolAdi = attr.RolAdi;
                    }
                    db.SaveChanges();
                }
            }
        }
        static void UpdateMenus2()
        {
            var menuAttrs = Membership.Menus();
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var err = new List<string>();
                var dbMenus = db.Menulers.ToArray();
                foreach (var attr in menuAttrs)
                {
                    var dbmenu = dbMenus.FirstOrDefault(p => p.MenuID == attr.MenuID);
                    if (dbmenu == null)
                    {
                        var yeniMenu = new Menuler
                        {
                            MenuID = attr.MenuID,
                            MenuUrl = attr.MenuUrl,
                            BagliMenuID = attr.BagliMenuID,
                            MenuAdi = attr.MenuAdi,
                            MenuCssClass = attr.MenuCssClass,
                            MenuIconUrl = attr.MenuIconUrl,
                            DilCeviriYap = attr.DilCeviriYap,
                            YetkisizErisim = attr.YetkisizErisim,
                            AuthenticationControl = attr.AuthenticationControl,
                            SiraNo = attr.SiraNo
                        };
                        db.Menulers.Add(yeniMenu);
                        if (attr.BagliRoller != null && attr.BagliRoller.Length > 0)
                        {
                            var dbRoller = db.Rollers.Where(p => attr.BagliRoller.Contains(p.RolAdi)).ToArray();
                            foreach (var dbRole in dbRoller)
                            {
                                yeniMenu.Rollers.Add(dbRole);
                            }

                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        dbmenu.MenuUrl = attr.MenuUrl;
                        dbmenu.BagliMenuID = attr.BagliMenuID;
                        dbmenu.MenuAdi = attr.MenuAdi;
                        dbmenu.MenuCssClass = attr.MenuCssClass;
                        dbmenu.MenuIconUrl = attr.MenuIconUrl;
                        dbmenu.DilCeviriYap = attr.DilCeviriYap;
                        dbmenu.YetkisizErisim = attr.YetkisizErisim;
                        dbmenu.AuthenticationControl = attr.AuthenticationControl;
                        dbmenu.SiraNo = attr.SiraNo;
                        if (attr.BagliRoller != null && attr.BagliRoller.Length > 0)
                        {
                            var dbRoller = db.Rollers.Where(p => attr.BagliRoller.Contains(p.RolAdi)).ToArray();
                            var nRols = dbRoller.Select(s => s.RolID).ToList();
                            var Yeni = dbmenu.Rollers.Where(a => !nRols.Contains(a.RolID)).ToList();
                            var varolan = dbmenu.Rollers.Where(a => nRols.Contains(a.RolID)).ToList();

                            foreach (var dbRole in Yeni)
                            {
                                dbmenu.Rollers.Add(dbRole);
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }
        public static Roller[] GetAllRoles(bool TumuOrYetkiIstenenler = true)
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                //return db.Rollers.ToArray();//-----------------------------------------------------------<<<Silinecek
                var roller = db.Rollers.AsQueryable();
                if (!TumuOrYetkiIstenenler)
                {
                    roller = db.Rollers.Where(p => p.Menulers.Any(a => a.YetkisizErisim.HasValue && a.YetkisizErisim.Value == false));
                }

                return roller.OrderBy(o => o.Kategori).ThenBy(t => t.RolAdi).ToArray();

            }
        }



        public static URoles GetUserRoles()
        {
            string UserName = HttpContext.Current.User.Identity.Name;
            var rolls = new URoles();
            using (MusskDBEntities db = new MusskDBEntities())
            {
                //return db.Rollers.ToArray();//-----------------------------------------------------------<<<Silinecek

                var kull = db.Kullanicilars.Where(p => p.KullaniciAdi == UserName).FirstOrDefault();
                if (kull != null)
                {
                    var kullRoll = kull.Rollers.ToList();

                    var ygRols = kull.YetkiGruplari.YetkiGrupRolleris.Select(s => s.Roller).ToList();
                    rolls.YetkiGrupID = kull.YetkiGrupID;
                    rolls.YetkiGrupAdi = kull.YetkiGruplari.YetkiGrupAdi;
                    rolls.YetkiGrupRolleri = ygRols;
                    rolls.TumRoller.AddRange(ygRols);
                    rolls.TumRoller.AddRange(kullRoll.Where(p => !ygRols.Any(a => a.RolID == p.RolID)));
                    rolls.EklenenRoller.AddRange(rolls.TumRoller.Where(p => rolls.YetkiGrupRolleri.Any(a => a.RolID == p.RolID) == false));
                    return rolls;


                }
                else
                {
                    FormsAuthenticationUtil.SignOut();
                    throw new SecurityException("Kullanıcı Tanımlı Değil");
                }

            }
        }
        public static URoles GetUserRoles(int KullaniciID)
        {
            var rolls = new URoles();
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var kull = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
                if (kull != null)
                {
                    var dRoll = kull.Rollers.ToList();

                    var ygRols = kull.YetkiGruplari.YetkiGrupRolleris.Select(s => s.Roller).ToList();
                    rolls.YetkiGrupID = kull.YetkiGrupID;
                    rolls.YetkiGrupAdi = kull.YetkiGruplari.YetkiGrupAdi;
                    rolls.YetkiGrupRolleri = ygRols;
                    rolls.TumRoller.AddRange(ygRols);
                    rolls.TumRoller.AddRange(dRoll.Where(p => !ygRols.Any(a => a.RolID == p.RolID)).Distinct());
                    rolls.EklenenRoller.AddRange(rolls.TumRoller.Where(p => rolls.YetkiGrupRolleri.Any(a => a.RolID == p.RolID) == false));
                    return rolls;
                }
                else
                    throw new SecurityException("Kullanıcı Tanımlı Değil");
            }
        }
        public static List<Roller> GetYetkiGrupRoles(int YetkiGrupID)
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var kull = db.YetkiGrupRolleris.Where(p => p.YetkiGrupID == YetkiGrupID).ToList();

                var rolIDs = kull.Select(s => s.RolID).ToList();
                return db.Rollers.Where(p => rolIDs.Contains(p.RolID)).ToList();


            }
        }


        public static bool InRole(this string userRole, int KullaniciID)
        {
            using (var db = new MusskDBEntities())
            {
                return db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).Any(a => a.Rollers.Any(a2 => a2.RolAdi == userRole) || a.YetkiGruplari.YetkiGrupRolleris.Any(a1 => a1.Roller.RolAdi == userRole));
            }
        }
        public static void SetUserRoles(int KullaniciID, List<int> RolIDs, int YetkiGrupID)
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var k = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
                if (k != null)
                {
                    var droles = k.Rollers.ToArray();
                    foreach (var drole in droles)
                        k.Rollers.Remove(drole);
                    k.YetkiGrupID = YetkiGrupID;
                    db.SaveChanges();
                    var uRoles = Management.GetUserRoles(k.KullaniciID);
                    RolIDs = RolIDs.Where(p => !uRoles.YetkiGrupRolleri.Any(a => a.RolID == p)).ToList();

                    if (RolIDs != null && RolIDs.Count > 0)
                    {
                        var newRoles = db.Rollers.Where(p => RolIDs.Contains(p.RolID));
                        foreach (var nr in newRoles)
                            k.Rollers.Add(nr);
                        db.SaveChanges();
                    }
                }
                else
                    throw new SecurityException("Kullanıcı Tanımlı Değil");
            }
        }
        public static Menuler[] GetUserMenus()
        {
            //return new LABSISDBEntities().Menulers.ToArray(); //-----------------------------------------------------------<<<Silinecek
            string UserName = HttpContext.Current.User.Identity.Name;
            if (UserName.IsNullOrWhiteSpace()) return new Menuler[] { };
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var menus = new List<Menuler>();
                var kull = db.Kullanicilars.Where(p => p.KullaniciAdi == UserName).FirstOrDefault();
                if (kull == null) FormsAuthenticationUtil.SignOut();
                var kullRoll = kull.Rollers.SelectMany(s => s.Menulers).Distinct().OrderBy(o => o.SiraNo).ToList();
                var ygRoll = kull.YetkiGruplari.YetkiGrupRolleris.SelectMany(s => s.Roller.Menulers).Distinct().OrderBy(o => o.SiraNo).ToList();
                menus.AddRange(kullRoll);
                menus.AddRange(ygRoll.Where(p => !kullRoll.Any(a => a.MenuID == p.MenuID)));
                return menus.ToArray();
            }
        }
        public static Menuler[] GetAllMenu()
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                return db.Menulers.OrderBy(o => o.SiraNo).ToArray();

            }
        }


        public static UserIdentity GetUserIdentity(string UserName)
        {
            var kull = new FrKullanicilar();
            if (UserName != null) kull = Management.GetUser(UserName);
            else kull = Management.GetUser();
            if (kull == null)
            {
                FormsAuthenticationUtil.SignOut();
                return null;
            }


            var roller = Management.GetUserRoles(kull.KullaniciID);

            UserIdentity ui = new UserIdentity(UserName);
            ui.YetkiGrupID = kull.YetkiGrupID;
            ui.Id = kull.KullaniciID;
            ui.Roles.AddRange(roller.TumRoller.Select(s => s.RolAdi).ToArray());
            ui.YGRoles.AddRange(roller.YetkiGrupRolleri.Select(s => s.RolAdi).ToArray());
            ui.AdSoyad = kull.Ad + " " + kull.Soyad;
            ui.Description = kull.Aciklama;
            ui.EMail = kull.EMail;
            ui.IsAdmin = kull.IsAdmin;
            ui.BirimID = kull.BirimID;
            ui.BirimYetkileri = kull.BirimYetkileri.Select(s => s.BirimID).ToList();
            ui.BirimYetkileriRapor = kull.KullaniciBirimleriRapors.Select(s => s.BirimID).ToList();
            ui.YevmiyeHesapKodTurYetkileri = kull.KullaniciYevmiyeHesapKodTurYetkileris.Select(s => s.YevmiyeHesapKodTurID).ToList();
            ui.SeciliBirimID = kull.SeciliBirimID;
            ui.SeciliVASurecID = kull.SeciliVASurecID;
            ui.SeciliAyID = kull.SeciliAyID;
            ui.SeciliYil = kull.SeciliYil;
            ui.HasToChahgePassword = kull.SifresiniDegistirsin;
            ui.IsActiveDirectoryImpersonateWorking = false;
            ui.IsActiveDirectoryUser = kull.IsActiveDirectoryUser;
            ui.ImagePath = kull.ResimAdi.ToKullaniciResim();
            ui.Informations.Add("FixedHeader", kull.FixedHeader);
            ui.Informations.Add("FixedSidebar", kull.FixedSidebar);
            ui.Informations.Add("ScrollSidebar", kull.ScrollSidebar);
            ui.Informations.Add("RightSidebar", kull.RightSidebar);
            ui.Informations.Add("CustomNavigation", kull.CustomNavigation);
            ui.Informations.Add("ToggledNavigation", kull.ToggledNavigation);
            ui.Informations.Add("BoxedOrFullWidth", kull.BoxedOrFullWidth);
            ui.Informations.Add("ThemeName", kull.ThemeName);
            ui.Informations.Add("BackgroundImage", kull.BackgroundImage);

            return ui;
            //return RedirectToAction("HomePage", "Home");             
            throw new Exception("Not Impletemented Method for Account Logon");
        }
        public static FrKullanicilar GetUser()
        {
            string UserName = HttpContext.Current.User.Identity.Name;
            return GetUser(null, UserName);

        }
        public static FrKullanicilar GetUser(int KullaniciID)
        {
            return GetUser(KullaniciID, null);
        }
        public static FrKullanicilar GetUser(string KullaniciAdi)
        {
            return GetUser(null, KullaniciAdi);
        }
        public static FrKullanicilar GetUser(int? KullaniciID = null, string KullaniciAdi = null)
        {
            using (var db = new MusskDBEntities())
            {

                var q = (from s in db.Kullanicilars
                         join ktl in db.YetkiGruplaris on s.YetkiGrupID equals ktl.YetkiGrupID
                         where KullaniciID.HasValue ? s.KullaniciID == KullaniciID.Value : s.KullaniciID == s.KullaniciID && KullaniciAdi != null ? s.KullaniciAdi == KullaniciAdi : s.KullaniciID == s.KullaniciID
                         select new FrKullanicilar
                         {
                             KullaniciID = s.KullaniciID,
                             YetkiGrupID = s.YetkiGrupID,
                             YetkiGrupAdi = ktl.YetkiGrupAdi,
                             Ad = s.Ad,
                             Soyad = s.Soyad,
                             Tel = s.Tel,
                             EMail = s.EMail,
                             BirimID = s.BirimID,
                             BirimAdi = s.Birimler.BirimAdi,
                             UnvanID = s.UnvanID,
                             UnvanAdi = s.Unvanlar.UnvanAdi,
                             ResimAdi = s.ResimAdi,
                             KullaniciAdi = s.KullaniciAdi,
                             Sifre = s.Sifre,
                             FixedHeader = s.FixedHeader,
                             FixedSidebar = s.FixedSidebar,
                             ScrollSidebar = s.ScrollSidebar,
                             RightSidebar = s.RightSidebar,
                             CustomNavigation = s.CustomNavigation,
                             ToggledNavigation = s.ToggledNavigation,
                             BoxedOrFullWidth = s.BoxedOrFullWidth,
                             KullaniciBirimleris = s.KullaniciBirimleris,
                             ThemeName = s.ThemeName,
                             BackgroundImage = s.BackgroundImage,
                             SifresiniDegistirsin = s.SifresiniDegistirsin,
                             IsAktif = s.IsAktif,
                             IsActiveDirectoryUser = s.IsActiveDirectoryUser,
                             IsAdmin = s.IsAdmin,
                             Aciklama = s.Aciklama,
                             ParolaSifirlamaKodu = s.ParolaSifirlamaKodu,
                             ParolaSifirlamGecerlilikTarihi = s.ParolaSifirlamGecerlilikTarihi,
                             OlusturmaTarihi = s.OlusturmaTarihi,
                             LastLogonDate = s.LastLogonDate,
                             LastLogonIP = s.LastLogonIP,
                             IslemTarihi = s.IslemTarihi,
                             IslemYapanIP = s.IslemYapanIP,
                             KullaniciBirimleriRapors = s.KullaniciBirimleriRapors,
                             KullaniciYevmiyeHesapKodTurYetkileris = s.KullaniciYevmiyeHesapKodTurYetkileris

                         }).FirstOrDefault();
                if (q != null)
                {
                    q.BirimTreeAdi = db.sp_BirimAgaciGetBr(q.BirimID).FirstOrDefault().BirimTreeAdi;
                    var KullaniciBirimIds = q.KullaniciBirimleris.Select(s => s.BirimID).ToList();

                    var KullaniciBirimleri = db.Birimlers.Where(p => KullaniciBirimIds.Contains(p.BirimID)).ToList();



                    foreach (var item in KullaniciBirimleri)
                    {
                        item.BirimAdi = db.sp_BirimAgaciGetBr(item.BirimID).FirstOrDefault().BirimTreeAdi + (item.IsVeriGirisiYapilabilir ? (" (" + item.VeriGirisTipleri.VeriGirisTipAdi + ")") : "");
                    }
                    q.BirimYetkileri = KullaniciBirimleri.OrderBy(o => o.BirimAdi).ToList();




                    // Veri Girisi 
                    q.SeciliBirimID.Add(RoleNames.VeriGirisi, KullaniciBirimIds.Count > 0 ? KullaniciBirimIds.First() : 0);
                    var SeciliSurecID = GetAktifSurecID();
                    if (SeciliSurecID.HasValue == false)
                    {
                        var Surec = db.VASurecleris.OrderByDescending(o => o.BaslangicTarihi).FirstOrDefault();
                        if (Surec != null) SeciliSurecID = Surec.VASurecID;
                    }

                    q.SeciliVASurecID.Add(RoleNames.VeriGirisi, SeciliSurecID);


                    q.SeciliBirimID.Add(RoleNames.Yevmiyeler, null);
                    q.SeciliYil.Add(RoleNames.Yevmiyeler, DateTime.Now.Year);

                }
                return q;

            }
        }
        public static Kullanicilar Login(string Uid, string Pwd)
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var sifre = Pwd.ComputeHash(Tuz);
                var u = db.Kullanicilars.Where(p => p.KullaniciAdi == Uid && p.Sifre == sifre).FirstOrDefault();
                return u;
            }
        }

        public static void CreateAdmin()
        {
            using (MusskDBEntities db = new MusskDBEntities())
            {
                if (db.Kullanicilars.Where(p => p.IsAktif == true).Count() == 0)
                {
                    var adm = db.Kullanicilars.Where(p => p.KullaniciAdi == "admin").FirstOrDefault();
                    if (adm == null)
                    {
                        #region Default Admin
                        db.Kullanicilars.Add(new Kullanicilar
                        {
                            KullaniciAdi = "admin",
                            IsAdmin = true,
                            Sifre = "123".ComputeHash(Management.Tuz),
                            IsAktif = true,
                            ResimAdi = "Images/avatars/DefaultUserImage.png",
                            Ad = "Administrator",
                            Aciklama = "Yönetici",
                            IsActiveDirectoryUser = false,
                            SifresiniDegistirsin = false,
                            LastLogonIP = ""
                        });
                    }
                    else
                    {
                        adm.IsAktif = true;
                        adm.Sifre = "123".ComputeHash(Management.Tuz);
                        db.SaveChanges();
                    }
                    db.SaveChanges();
                    #endregion
                }
            }
        }
        public static bool SetLastLogon()
        {
            string UserName = HttpContext.Current.User.Identity.Name;
            using (MusskDBEntities db = new MusskDBEntities())
            {
                var kull = db.Kullanicilars.Where(p => p.KullaniciAdi == UserName).FirstOrDefault();
                if (kull != null)
                {
                    kull.IslemYapanID = UserIdentity.Current.Id;
                    kull.LastLogonDate = DateTime.Now;
                    kull.LastLogonIP = UserIdentity.Ip;
                    db.SaveChanges();
                    return true;
                }
                else return false;
            }
        }

        public static void AddMessage(SystemInformation sis)
        {
            int? currid = UserIdentity.Current == null ? null : (int?)UserIdentity.Current.Id;
            using (MusskDBEntities db = new MusskDBEntities())
            {
                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = (byte)sis.InfoType,
                    Kategori = sis.Category,
                    Message = sis.Message,
                    StackTrace = sis.StackTrace,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    IslemYapanID = currid
                });
                db.SaveChanges();
            }
        }

        public static string GetRoot()
        {
            var root = HttpRuntime.AppDomainAppVirtualPath;
            root = root.EndsWith("/") ? root : root + "/";
            return root;
        }


        #endregion
        #region Functions    




        public static Func<int, List<int>, List<int>> getfncParentBirimIDs = (BirimID, Lst) =>
         {
             using (var db = new MusskDBEntities())
             {
                 Lst.Add(BirimID);
                 var loks = db.Birimlers.Where(p => p.BirimID == BirimID).FirstOrDefault();
                 if (loks != null && loks.UstBirimID.HasValue)
                     getfncParentBirimIDs(loks.UstBirimID.Value, Lst);
                 return Lst;
             }

         };

        public static List<int> GetSubBirimIDs(this int BirimID, List<int> Liste = null)
        {
            if (Liste == null) Liste = new List<int>();
            var bids = getfncSubBirimIDs(BirimID, Liste);
            return bids;
        }
        public static Func<int, List<int>, List<int>> getfncSubBirimIDs = (y, Lst) =>
        {
            using (var db = new MusskDBEntities())
            {
                Lst.Add(y);
                var loks = db.Birimlers.Where(p => p.UstBirimID == y).ToList();
                foreach (var item2 in loks)
                {
                    getfncSubBirimIDs(item2.BirimID, Lst);
                }
                return Lst;

            }

        };
        public static List<int> GetSubAkademikBirimIDs(this int BirimID, List<int> Liste = null)
        {
            if (Liste == null) Liste = new List<int>();
            var bids = getfncSubAkademikBirimIDs(BirimID, Liste);
            return bids;
        }
        public static Func<int, List<int>, List<int>> getfncSubAkademikBirimIDs = (y, Lst) =>
        {
            using (var db = new MusskDBEntities())
            {

                Lst.Add(y);
                var loks = db.Birimlers.Where(p => p.UstBirimID == y).ToList();
                foreach (var item2 in loks)
                {
                    getfncSubAkademikBirimIDs(item2.BirimID, Lst);
                }
                return Lst;

            }

        };
        #endregion
        #region Combobox


        public static List<ComboModelInt> CmbSurecKayitYillari(bool bosSecimVar = true, int BaslangicYil = 2018)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new MusskDBEntities())
            {
                for (int i = 2018; i <= DateTime.Now.Year + 1; i++)
                {
                    dct.Add(new ComboModelInt { Value = i, Caption = i + " Yılı Süreci" });
                }
            }
            return dct.OrderByDescending(o => (!o.Value.HasValue ? int.MaxValue : o.Value)).ToList();

        }

        public static List<ComboModelInt> CmbAylar(bool bosSecimVar = true, string BosSecimAdi = "")
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = BosSecimAdi });

            using (var db = new MusskDBEntities())
            {
                var data = db.Aylars.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.AyID, Caption = item.AyAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbVASurecleriAylar(int VASurecID)
        {
            var dct = new List<ComboModelInt>();

            using (var db = new MusskDBEntities())
            {
                var data = db.VASurecleriAylars.Where(p => p.VASurecID == VASurecID).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.AyID, Caption = item.Aylar.AyAdi + " (" + item.AyDurumlari.DurumAdi + ")" });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbAyDurumlari(bool bosSecimVar = true, string BosSecimAdi = "")
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = BosSecimAdi });

            using (var db = new MusskDBEntities())
            {
                var data = db.AyDurumlaris.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.AyDurumID, Caption = item.DurumAdi });
                }
            }
            return dct;

        }






        public static List<ComboModelInt> CmbYetkiGruplari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new MusskDBEntities())
            {
                var data = db.YetkiGruplaris.OrderBy(o => o.YetkiGrupAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YetkiGrupID, Caption = item.YetkiGrupAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelBool> CmbIsActiveDirectoryUserData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Active Directory" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Lokal Şifre" });
            return dct;

        }
        public static List<ComboModelBool> CmbAktifPasifData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Aktif" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Pasif" });
            return dct;

        }



        public static List<ComboModelBool> CmbAcikKapaliData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Kapalı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Açık" });
            return dct;

        }
        public static List<ComboModelBool> CmbDoluBosData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Dolu" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Boş" });
            return dct;

        }



        public static List<ComboModelBool> CmbItirazAktifPasifData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Açık Olan İtirazlar" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Kapalı Olan İtirazlar" });
            return dct;

        }



        public static List<ComboModelInt> CmbUnvanlar(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var data = db.Unvanlars.OrderBy(o => o.UnvanID == 1 ? 0 : 1).ThenBy(o => o.UnvanAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.UnvanID, Caption = item.UnvanAdi });
                }
            }
            return dct;

        }


        public static List<ComboModelInt> CmbBirimler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var data = db.Birimlers.OrderBy(o => o.BirimAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbBirimlerUniversiteIsYerleri(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var data = db.YevmiyelerHarcamaBirimleris.OrderBy(o => o.BirimAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeHarcamaBirimID, Caption = item.BirimAdi + " (" + item.VergiKimlikNo + ")" });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbKullaniciBirimlerTree(bool bosSecimVar = true, int? HaricBirimID = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var b = db.Birimlers.Where(p => !p.IsVeriGirisiYapilabilir).AsQueryable();
                if (HaricBirimID.HasValue)
                {
                    var subBID = HaricBirimID.Value.GetSubBirimIDs();
                    b = b.Where(p => subBID.Contains(p.BirimID) == false);
                }
                var data = b.OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbKullaniciRaporAnaBirimlerTree(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "Tüm Birimler" });
            using (var db = new MusskDBEntities())
            {
                var RBirimIDs = db.KullaniciBirimleriRapors.Where(p => p.KullaniciID == UserIdentity.Current.Id).Select(s => s.Birimler.UstBirimID).ToList();

                var data = db.Birimlers.Where(p => RBirimIDs.Contains(p.BirimID)).OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbKullaniciRaporBirimlerTree(bool bosSecimVar = true, int? HaricBirimID = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "Tüm Birimler" });
            using (var db = new MusskDBEntities())
            {
                var b = db.Birimlers.Where(p => !p.IsVeriGirisiYapilabilir && p.KullaniciBirimleriRapors.Any(a => a.KullaniciID == UserIdentity.Current.Id)).AsQueryable();
                if (HaricBirimID.HasValue)
                {
                    var subBID = HaricBirimID.Value.GetSubBirimIDs();
                    b = b.Where(p => subBID.Contains(p.BirimID) == false);
                }
                var data = b.OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbBirimlerTree(bool bosSecimVar = true, int? HaricBirimID = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var b = db.Birimlers.AsQueryable();
                if (HaricBirimID.HasValue)
                {
                    var subBID = HaricBirimID.Value.GetSubBirimIDs();
                    b = b.Where(p => subBID.Contains(p.BirimID) == false);
                }
                var data = b.OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbUstBirimlerTree(bool bosSecimVar = true, int? HaricBirimID = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var b = db.Birimlers.AsQueryable();
                if (HaricBirimID.HasValue)
                {
                    var subBID = HaricBirimID.Value.GetSubBirimIDs();
                    b = b.Where(p => subBID.Contains(p.BirimID) == false && p.IsVeriGirisiYapilabilir == false);
                }
                var data = b.OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbVeriGirisTipleri(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var data = db.VeriGirisTipleris.OrderBy(o => o.VeriGirisTipID).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VeriGirisTipID, Caption = item.VeriGirisTipAdi });
                }
            }
            return dct;

        }



        public static List<ComboModelInt> CmbMailSablonlari(bool bosSecimVar = true, bool? SistemMailFiltre = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var data = db.MailSablonlaris.Where(p => p.IsAktif && p.MailSablonTipleri.SistemMaili == (SistemMailFiltre ?? p.MailSablonTipleri.SistemMaili)).OrderBy(o => o.SablonAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MailSablonlariID, Caption = item.SablonAdi });
                }
            }

            return dct;

        }
        public static List<ComboModelInt> CmbMailSablonTipleri(bool? SistemMaili = null, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var data = db.MailSablonTipleris.Where(p => p.SistemMaili == (SistemMaili ?? p.SistemMaili)).OrderBy(o => o.MailSablonTipID).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MailSablonTipID, Caption = item.SablonTipAdi });
                }
            }

            return dct;

        }
        public static List<ComboModelBool> CmbHarcamaBirimiDegistiData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Harcama Birimi Değişti" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Harcama Birimi Değişmedi" });
            return dct;

        }
        public static List<ComboModelBool> CmbSendikaBilgisiDegistiData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Sendika Bilgisi Değişti" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Sendika Bilgisi Değişmedi" });
            return dct;

        }
        public static List<ComboModelBool> CmbBesBilgisiDegistiData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Bes Bilgisi Değişti" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Bes Bilgisi Değişmedi" });
            return dct;

        }
        public static List<ComboModelBool> CmbVeriGirisiTamamlandiData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Veri Girişi Tamamlandı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Veri Girişi Tamamlanmadı" });
            return dct;

        }
        public static List<ComboModelBool> CmbHesaplamayaGirmeDurumData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Hesaplamaya Girecek Kayıtlar" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Hesaplamaya Girmeyecek Kayıtlar" });
            return dct;

        }
        public static List<ComboModelBool> CmbGelirKaydiListeDurumData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "22-31 Gelir Yevmiyelerini Getir" });
            dct.Add(new ComboModelBool { Value = false, Caption = "22-31 Gelir Yevmiyeleri Harcindekilerini Getir" });
            return dct;

        }
        public static List<ComboModelBool> CmbGelirKaydiDurumData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "22-31 Gelir Kaydı Yapıldı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "22-31 Gelir Kaydı Yapılmadı" });
            return dct;

        }
        public static List<ComboModelBool> CmbHesapKoduHesapKoduDegistirildiData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Hesap Kodu Değiştirildi" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Hesap Kodu Değiştirilmedi" });
            return dct;

        }
        public static List<ComboModelBool> CmbBankaVeriGirisDurumData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "İşlem Gördü" });
            dct.Add(new ComboModelBool { Value = false, Caption = "İşlem Görmedi" });
            return dct;

        }
        public static List<ComboModelBool> CmbEvetHayirData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Evet" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Hayir" });
            return dct;

        }

        public static List<ComboModelBool> CmbVarYokData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Var" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Yok" });
            return dct;

        }
        public static List<ComboModelInt> CmbMaddeDurum(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });

            dct.Add(new ComboModelInt { Value = 1, Caption = "Veri Girişi Tamamlananlar" });
            dct.Add(new ComboModelInt { Value = 0, Caption = "Veri Girişi Tamamlanmayanlar" });

            return dct;

        }
        public static List<ComboModelBool> CmbEslesenEslesmeyenData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Eşleşen" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Eşleşmeyen" });
            return dct;

        }



        public static List<ComboModelInt> CmbMesajKategorileri(bool bosSecimVar = true, bool? IsAktif = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new MusskDBEntities())
            {
                var qdata = db.MesajKategorileris.AsQueryable();
                if (IsAktif.HasValue) qdata = qdata.Where(p => p.IsAktif == IsAktif.Value);
                var data = qdata.OrderBy(o => o.KategoriAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MesajKategoriID, Caption = item.KategoriAdi });
                }
            }
            return dct;

        }

        #endregion
        #region GetData
        public static List<ComboModelInt> CmbVASurecler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new MusskDBEntities())
            {
                var nowDate = DateTime.Now;
                var data = db.VASurecleris.OrderByDescending(o => o.Yil).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VASurecID, Caption = item.Yil + " Yılı Süreci (" + (item.BaslangicTarihi <= nowDate && item.BitisTarihi >= nowDate ? "Aktif" : "Pasif") + ")" });
                }
            }
            return dct;

        }
        public static int? GetAktifSurecID()
        {
            using (var db = new MusskDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var Donem = db.VASurecleris.Where(a => (a.BaslangicTarihi <= nowDate && a.BitisTarihi >= nowDate)).FirstOrDefault();
                if (Donem != null) return Donem.VASurecID;
                else return null;
            }
        }


        public static List<ComboModelInt> CmbYetkiliBirimlerKullanici(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var BirimIDs = UserIdentity.Current.BirimYetkileri;
                var data = db.Vw_BirimlerTree.Where(p => p.IsVeriGirisiYapilabilir && BirimIDs.Contains(p.BirimID)).OrderBy(o => o.BirimTreeAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimTreeAdi });
                }
            }
            return dct;

        }


        public static List<ComboModelInt> CmbYetkiliVASurecBirimlerKullanici(int VASurecID, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var BirimIDs = UserIdentity.Current.BirimYetkileri;

                var data = (from s in db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && BirimIDs.Contains(p.BirimID))
                            join vgt in db.VeriGirisTipleris on s.VeriGirisTipID equals vgt.VeriGirisTipID
                            join b in db.Vw_BirimlerTree on s.BirimID equals b.BirimID
                            select new
                            {
                                b.BirimID,
                                BirimTreeAdi = b.BirimTreeAdi + " (" + vgt.VeriGirisTipAdi + ")"
                            }).OrderBy(o => o.BirimTreeAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimTreeAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbYevmiyeHesapKodTurleri(bool bosSecimVar = true, List<int> YevmiyeHesapKodTurIDs = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {
                var q = db.YevmiyelerHesapKodTurleris.Where(p => p.IsYevmiyedeGozuksun);
                if (YevmiyeHesapKodTurIDs != null && YevmiyeHesapKodTurIDs.Any()) q = q.Where(p => YevmiyeHesapKodTurIDs.Contains(p.YevmiyeHesapKodTurID));
                var data = q.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeHesapKodTurID, Caption = item.HesapKodTurAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbHesapKoduEslestirmeHesapKodTurleri(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerHesapKodTurleris.Where(p => p.IsHesapKoduEslestirmedeGozuksun).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeHesapKodTurID, Caption = item.HesapKodTurAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbYevmiyeKdvKodlari(string HesapKodu, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerKdvKodlaris.Where(p => p.HesapKod == HesapKodu || p.HesapKod == "" || p.HesapKod == null).OrderBy(p => p.KdvKodu).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeKdvKodID, Caption = item.KdvKodu + " - " + item.KdvOrani + " - " + item.KdvAdi });
                }
            }
            return dct;
        }
        public static List<ComboModelString> CmbYevmiyeDigerKdvKodlari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelString>();
            if (bosSecimVar) dct.Add(new ComboModelString { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerKdvKodlaris.Where(p => p.IsDigerKdvler).FirstOrDefault();
                if (data != null && !data.KdvOrani.IsNullOrWhiteSpace())
                {
                    var Kods = data.KdvOrani.Split(',').Select(s => s.Trim()).ToList();
                    foreach (var item in Kods)
                    {

                        dct.Add(new ComboModelString { Value = item, Caption = item });
                    }
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbYevmiyeBelgeKodlari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerBelgeKodlaris.OrderBy(p => p.BelgeKodu).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeBelgeKodID, Caption = item.BelgeKodu + " " + item.BelgeAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbKdvOranlari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            dct.Add(new ComboModelInt { Value = 1, Caption = "1" });
            dct.Add(new ComboModelInt { Value = 8, Caption = "8" });
            dct.Add(new ComboModelInt { Value = 10, Caption = "10" });
            dct.Add(new ComboModelInt { Value = 18, Caption = "18" });
            dct.Add(new ComboModelInt { Value = 20, Caption = "20" });
            return dct;

        }
        public static List<ComboModelInt> CmbYevmiylerYil(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            for (int i = DateTime.Now.Year + 1; i >= 2017; i--)
            {
                dct.Add(new ComboModelInt { Value = i, Caption = i + " Yılı" });
            }
            return dct;
        }
        public static List<ComboModelInt> CmbYevmiyelerBirim(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerHarcamaBirimleris.Where(p => !p.IsAltBirim).Select(s => new
                {
                    s.YevmiyeHarcamaBirimID,
                    BirimAdi = s.BirimAdi + " " + s.VergiKimlikNo,
                }).OrderBy(o => o.BirimAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YevmiyeHarcamaBirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbYevmiyelerHesapKodlari(int? YevmiyeHesapKodTurID, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new MusskDBEntities())
            {

                var data = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == (YevmiyeHesapKodTurID ?? p.YevmiyeHesapKodTurID)).Select(s => new
                {
                    s.YevmiyeHesapKodID,
                    s.VergiKodu,
                    s.HesapKod,
                    s.HesapAdi
                }).OrderBy(o => o.VergiKodu).ToList();
                foreach (var item in data)
                {
                    var HesapAdi = "";
                    if (!item.VergiKodu.IsNullOrWhiteSpace()) HesapAdi += item.VergiKodu + " - ";
                    HesapAdi += item.HesapAdi;
                    if (!item.HesapKod.IsNullOrWhiteSpace()) HesapAdi += " (" + item.HesapKod + ")";
                    dct.Add(new ComboModelInt { Value = item.YevmiyeHesapKodID, Caption = HesapAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbSurecBirimDurum()
        {
            var dct = new List<ComboModelInt>();
            dct.Add(new ComboModelInt { Caption = "Tüm Birimler" });
            dct.Add(new ComboModelInt { Value = 1, Caption = "Veri Girişi Olanlar" });
            dct.Add(new ComboModelInt { Value = 0, Caption = "Veri Girişi Olmayanlar" });
            return dct;

        }
        public static List<ComboModelInt> CmbSurecMaddeDurum()
        {
            var dct = new List<ComboModelInt>();
            dct.Add(new ComboModelInt { Caption = "Tüm Maddeler" });
            dct.Add(new ComboModelInt { Value = 1, Caption = "Veri Girişin Tamamlanan" });
            dct.Add(new ComboModelInt { Value = 0, Caption = "Veri Girişin Tamamlanmayan" });
            return dct;

        }
        public static List<ComboModelBool> CmbVeriGirisOnayDurum(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Onaylandı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Onaylanmadı" });
            return dct;

        }


        public static ComboModelInt GetCevaplanmamisMesajCount()
        {
            var model = new ComboModelInt();
            using (var db = new MusskDBEntities())
            {
                var Liste = db.Mesajlars.Where(p => p.UstMesajID.HasValue == false && !p.IsAktif && p.Silindi == false).OrderByDescending(o => (o.Mesajlar1.Any() ? o.Mesajlar1.Select(s => s.Tarih).Max() : o.Tarih)).ToList();
                var htmlContent = "";
                foreach (var item in Liste)
                {

                    var kul = item.Kullanicilar;
                    var birimAdi = item.KullaniciID.HasValue ? db.sp_BirimAgaciGetBr(kul.BirimID).FirstOrDefault().BirimTreeAdi : "";
                    htmlContent += "<a href='javascript:void(0);' class='list-group-item' style='padding-top:0px;padding-bottom:0px;padding-left:2px;padding-right:-1px;'>" +
                                        "<table style='table-layout:fixed;width:100%;'>" +
                                                "<tr>" +
                                                    "<td width='40'><img style='width:40px;height:40px;' src ='" + ((item.KullaniciID.HasValue ? item.Kullanicilar.ResimAdi : "").ToKullaniciResim()) + "' class='pull-left' ></td>" +
                                                    "<td><span class='contacts-title' title='" + (birimAdi) + "'>" + item.AdSoyad + "</span><span style='float:right;font-size:8pt;'><b>" + (item.Mesajlar1.Any() ? item.Mesajlar1.Select(s => s.Tarih).Max().ToFormatDateAndTime() : item.Tarih.ToFormatDateAndTime()) + "</b></span><p><b>Konu:</b> " + item.Konu + "</p></td>" +
                                                "</tr>" +
                                            "</table>" +
                                    "</a>";
                }
                model.Value = Liste.Count;
                model.Caption = htmlContent;
                return model;

            }
        }




        public static FrSurecIslemleri GetVASurecKontrol(int VASurecID, int? SecilenAyID = null)
        {
            using (var db = new MusskDBEntities())
            {
                var nowDate = DateTime.Now;
                var xD = (from s in db.VASurecleris.Where(p => p.VASurecID == VASurecID)
                          join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                          select new FrSurecIslemleri
                          {
                              VASurecID = s.VASurecID,
                              SurecYilAdi = s.Yil + " Yılı Süreci",
                              Yil = s.Yil,
                              BaslangicTarihi = s.BaslangicTarihi,
                              BitisTarihi = s.BitisTarihi,
                              IsAktif = s.IsAktif,
                              IslemYapanID = s.IslemYapanID,
                              IslemYapan = k.KullaniciAdi,
                              IslemTarihi = s.IslemTarihi,
                              IslemYapanIP = s.IslemYapanIP,
                              AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate),
                              VASurecleriAylars = s.VASurecleriAylars,
                          }).FirstOrDefault();
                if (SecilenAyID.HasValue)
                {
                    var secilenAy = db.VASurecleriAylars.Where(p => p.VASurecID == VASurecID && p.AyID == SecilenAyID).First();
                    xD.SecilenAyBilgi = new FrSecilenAyBilgi
                    {
                        VASurecleriAyID = secilenAy.VASurecleriAyID,
                        VASurecID = secilenAy.VASurecID,
                        AyID = secilenAy.AyID,
                        AyAdi = secilenAy.Aylar.AyAdi,
                        AyDurumID = secilenAy.AyDurumID,
                        AyDurumAdi = secilenAy.AyDurumlari.DurumAdi,
                        IsVeriGirilebilir = secilenAy.AyDurumlari.IsVeriGirilebilir,

                    };
                }
                return xD;
            }
        }


        public static List<Birimler> GetBirimler()
        {

            using (var db = new MusskDBEntities())
            {
                return db.Birimlers.OrderBy(o => o.BirimAdi).ToList();

            }
        }



        #endregion
        #region SetData
        public static void SistemBilgisiKaydet(Exception ex, byte BilgiTipi)
        {
            using (var db = new MusskDBEntities())
            {


                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = BilgiTipi,
                    Message = ex.ToExceptionMessage(),
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = ex.ToExceptionStackTrace()
                });
                db.SaveChanges();
            }
        }

        public static void SistemBilgisiKaydet(Exception ex, string Message, byte BilgiTipi)
        {
            using (var db = new MusskDBEntities())
            {


                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = BilgiTipi,
                    Message = Message,
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = ex.ToExceptionStackTrace()
                });
                db.SaveChanges();
            }
        }
        public static void SistemBilgisiKaydet(string Mesaj, string StakTrace, byte BilgiTipi)
        {
            using (var db = new MusskDBEntities())
            {
                var kulID = (UserIdentity.Ip == "" || UserIdentity.Current.Id <= 0 ? (int?)null : UserIdentity.Current.Id);

                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    Message = Mesaj,
                    BilgiTipi = BilgiTipi,
                    IslemYapanID = kulID,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = StakTrace
                });
                db.SaveChanges();
            }
        }

        public static void SistemBilgisiKaydet(string Mesaj, string StakTrace, byte BilgiTipi, int? KullaniciID, string KullaniciIP)
        {
            using (var db = new MusskDBEntities())
            {
                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    Message = Mesaj,
                    BilgiTipi = BilgiTipi,
                    IslemYapanID = KullaniciID,
                    IslemYapanIP = !KullaniciIP.IsNullOrWhiteSpace() ? KullaniciIP : UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = StakTrace
                });
                db.SaveChanges();
            }
        }
        #endregion
        #region Diger

        public static ComboModelString ResimKaydet(HttpPostedFileBase Resim)
        {
            try
            {
                string mimeType = Resim.ContentType;
                Stream fileStream = Resim.InputStream;
                Bitmap bmp = new Bitmap(fileStream);

                string folderName = SistemAyar.getAyar(SistemAyar.KullaniciResimYolu);
                bool RotasYonDegisimLog = SistemAyar.getAyar(SistemAyar.RotasyonuDegisenResimleriLogla).ToBoolean().Value;
                bool Boyutlandirma = SistemAyar.getAyar(SistemAyar.KullaniciResimKaydiBoyutlandirma).ToBoolean().Value;
                bool KaliteOpt = SistemAyar.getAyar(SistemAyar.KullaniciResimKaydiKaliteOpt).ToBoolean().Value;




                var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
                int i = 0;
                string fileName = "";
                //var ext = "." + Resim.FileName.Split('.').Last();
                foreach (var item in Path.GetFileName(Resim.FileName).Split('.'))
                {
                    i++;
                    if (i != Path.GetFileName(Resim.FileName).Split('.').Count()) fileName += item;
                    //else fileName += ".jpg";
                }

                fileName = unqCode.ReplaceSpecialCharacter() + "_" + fileName.ReplaceSpecialCharacter() + ".jpg";
                var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/" + folderName), fileName);


                if (Boyutlandirma)
                {
                    try
                    {
                        var uzn = SistemAyar.getAyar(SistemAyar.KullaniciResimKaydiHeightPx);
                        var gens = SistemAyar.getAyar(SistemAyar.KullaniciResimKaydiWidthPx);

                        int Uzunluk = uzn.IsNullOrWhiteSpace() ? 560 : uzn.ToInt().Value;
                        int Genislik = gens.IsNullOrWhiteSpace() ? 560 : gens.ToInt().Value;
                        var img = bmp.ToResizeImage(new Size(Genislik, Uzunluk));
                        img.Save(path, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        Management.SistemBilgisiKaydet(ex, "Resmin boyutlandırma işlemi yapılıp kayıt edilirken bir hata oluştu.\r\n Hata:" + ex.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                    }
                }
                else
                {
                    bmp.Save(path, ImageFormat.Jpeg);
                }

                if (KaliteOpt)
                {
                    #region Quality check
                    try
                    {

                        Bitmap bmp_Q = new Bitmap(path);

                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);


                        Int64 quality = 100L;
                        if (Resim.ContentLength >= 80000 && Resim.ContentLength < 200000) quality = 80;
                        else if (Resim.ContentLength >= 200000 && Resim.ContentLength < 400000) quality = 70;
                        else if (Resim.ContentLength >= 400000 && Resim.ContentLength < 600000) quality = 60;
                        else if (Resim.ContentLength >= 600000 && Resim.ContentLength < 800000) quality = 50;
                        else if (Resim.ContentLength >= 800000 && Resim.ContentLength < 1000000) quality = 40;
                        else if (Resim.ContentLength >= 1000000) quality = 30;
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        var path2 = path + Guid.NewGuid().ToString().Substr(0, 4).ToString() + ".jpg";
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        bmp_Q.Save(path2, jpgEncoder, myEncoderParameters);
                        bmp_Q.Dispose();
                        if (File.Exists(path))
                            File.Delete(path);
                        var imgTmp = Image.FromFile(path2);
                        imgTmp.Save(path, ImageFormat.Jpeg);
                        imgTmp.Dispose();
                        File.Delete(path2);
                    }
                    catch (Exception errQuality)
                    {
                        Management.SistemBilgisiKaydet(errQuality, "Resmin kalitesi değiştirilirken hata oluştu.\r\n Hata:" + errQuality.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                    }
                    #endregion
                }

                #region Rotation
                try
                {

                    Image img1 = Image.FromFile(path);
                    var prop = img1.PropertyItems.Where(p => p.Id == 0x0112).FirstOrDefault();
                    if (prop != null)
                    {
                        int orientationValue = img1.GetPropertyItem(prop.Id).Value[0];
                        RotateFlipType rotateFlipType = GetOrientationToFlipType(orientationValue);
                        img1.RotateFlip(rotateFlipType);
                        var path2 = path + Guid.NewGuid().ToString().Substr(0, 4).ToString() + ".jpg";
                        img1.Save(path2);
                        img1.Dispose();
                        if (File.Exists(path))
                            File.Delete(path);
                        var imgTmp = Image.FromFile(path2);
                        imgTmp.Save(path, ImageFormat.Jpeg);
                        imgTmp.Dispose();
                        File.Delete(path2);
                        if (RotasYonDegisimLog)
                        {
                            Management.SistemBilgisiKaydet("Rotasyon farklılığı görünen resim düzeltildi! Resim:" + path, "Management/resimKaydet", BilgiTipi.Bilgi);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Management.SistemBilgisiKaydet(ex, "Hesap kayıt sırasında resim rotasyonu yapılırken bir hata oluştu.\r\n Hata:" + ex.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                }
                #endregion


                return new ComboModelString { Caption = fileName, Value = folderName + "/" + fileName };
            }
            catch (Exception ex)
            {
                Management.SistemBilgisiKaydet("Resim kaydedilirken bir hata oluştu! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, null, UserIdentity.Ip);
                return new ComboModelString { Caption = "", Value = "" };
            }
        }

        private static RotateFlipType GetOrientationToFlipType(int orientationValue)
        {
            RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

            switch (orientationValue)
            {
                case 1:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rotateFlipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rotateFlipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rotateFlipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rotateFlipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rotateFlipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return rotateFlipType;
        }


        public static void VaryQualityLevel(string path)
        {
            // Get a bitmap.
            Bitmap bmp1 = new Bitmap(path);
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(@"d:\TestPhotoQualityFifty.jpg", jpgEncoder, myEncoderParameters);

            myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(@"d:\TestPhotoQualityHundred.jpg", jpgEncoder, myEncoderParameters);

            // Save the bitmap as a JPG file with zero quality level compression.
            myEncoderParameter = new EncoderParameter(myEncoder, 0L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(@"d:\TestPhotoQualityZero.jpg", jpgEncoder, myEncoderParameters);

        }
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


        public static ComboModelPageIndex SetStartRowInx(int srIndex, int pgIndex, int pgCount, int rwCount, int pgSize)
        {
            int setStartRowInx = 0;
            if (rwCount <= srIndex) setStartRowInx = rwCount / pgSize;
            else setStartRowInx = srIndex;
            int setPageInx = pgIndex;
            if ((decimal)rwCount / (decimal)pgSize == 0 || pgCount < pgIndex) setPageInx = 1;
            return new ComboModelPageIndex { StartRowIndex = setStartRowInx, PageIndex = setPageInx };



        }



        #endregion




    }





}