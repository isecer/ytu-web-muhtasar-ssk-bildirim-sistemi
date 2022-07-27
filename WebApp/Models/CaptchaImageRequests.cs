
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WebApp.Models
{
    public class CaptchaImageRequests
    {
        public class RequestCaptcha
        {
            public RequestCaptcha()
            {
                this.RequestTime = DateTime.Now;
            } 
            public string RequestCode { get; set; } 
            public DateTime RequestTime { get; set; } 
            public string Text { get; set; }
        }

        public static object lockOjbect = new object();
        private static Dictionary<string, RequestCaptcha> Requests = new Dictionary<string, RequestCaptcha>();
        public static string GetCode(string RequestCode)
        {
            string str;
            Monitor.Enter(lockOjbect);
            try
            {
                if (Requests.ContainsKey(RequestCode))
                {
                    return Requests[RequestCode].Text;
                }
                str = "";
            }
            catch
            {
                str = "";
            }
            finally
            {
                Monitor.Exit(lockOjbect);
            }
            return str;
        }

        public static void RemoveCode(string RequestCode)
        {
            Monitor.Enter(lockOjbect);
            try
            {
                if (Requests.ContainsKey(RequestCode))
                {
                    Requests.Remove(RequestCode);
                }
            }
            catch { }
            Monitor.Exit(lockOjbect);
        }

        public static void AddCode(string RequestCode, string CaptchaText)
        {
            Monitor.Enter(lockOjbect);
            try
            {
                if (Requests.ContainsKey(RequestCode))
                {
                    Requests[RequestCode].Text = CaptchaText;
                }
                else
                {
                    RequestCaptcha captcha = new RequestCaptcha {
                        Text = CaptchaText,
                        RequestCode = RequestCode
                    };
                    Requests.Add(RequestCode, captcha);
                }                
            }
            catch 
            {
               
            }
            finally
            {
                Monitor.Exit(lockOjbect);
            }
        }
    }
}