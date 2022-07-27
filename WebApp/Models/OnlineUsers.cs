using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebApp.Models
{
    public class OnlineUser
    {
        public bool IsAuthenticated { get; set; }
        public int? KullaniciID { get; set; }
        public string Tc { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Platform { get; set; }
        public string Browser { get; set; }
        public string Version { get; set; }
        public bool IsYetkiyeniye { get; set; }
        public DateTime LoginTime { get; set; }
        public string YetkiGrupAdi { get; set; }
        public string UniqueId { get; set; }
        public string ResimAdi { get; set; }
        public string Ip { get; set; }
    }
    public static class OnlineUsers
    {
        public static List<OnlineUser> users { get; set; }
        public static int OnlineUserCount = 0;
        static object lockObject = new object();
        public static void AddUser(string UserId, string ip)
        {

            Monitor.Enter(lockObject);
            try
            {
                var clientIP = ip ?? HttpContext.Current.Request.UserHostAddress;
                //string localIP=System.Web.HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

                if (OnlineUsers.users.Where(p => p.UniqueId == UserId).Count() == 0)
                    OnlineUsers.users.Add(new OnlineUser { Tc = "Misafir", Name = "Misafir", LoginTime = DateTime.Now, UniqueId = UserId, Ip = clientIP });


                OnlineUserCount = users.Count;
            }
            catch { }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }
        public static void RemoveUser(string UserId)
        {

            Monitor.Enter(lockObject);
            try
            {
                OnlineUsers.users.RemoveAll(p => p.UniqueId == UserId);
                OnlineUserCount = users.Count;
            }
            catch { }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }
        public static void YetkiYenile(int KullaniciID)
        {

            Monitor.Enter(lockObject);
            try
            {
                var Usr = OnlineUsers.users.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
                if (Usr != null) Usr.IsYetkiyeniye = true;
            }
            catch { }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }

    }
}