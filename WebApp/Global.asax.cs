using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Database;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using WebApp.Jobs;

namespace WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void Log(string log)
        {
            System.IO.File.AppendAllText(@"C:\inetpub\wwwroot\MusskWeb\Log\log.txt", log + "\r\n");
        }

        public static IScheduler _quartzScheduler;
        protected void Application_Start()
        {
            DevExpress.XtraReports.Web.WebDocumentViewer.Native.WebDocumentViewerBootstrapper.SessionState = System.Web.SessionState.SessionStateBehavior.Disabled;
            AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BiskaUtil.Membership.OnRequireUserIdentity += Membership_OnRequireUserIdentity;
            BiskaUtil.SystemInformation.OnEvent += SystemInformation_OnEvent;
            Management.Update();
            OnlineUsers.users = new List<OnlineUser>();
            _quartzScheduler = ConfigureQuartz();
            var geciciDosyalarOtomatikOlarakKaldirilsin = SistemAyar.GeciciDosyalarOtomatikOlarakKaldirilsin.getAyar("false").ToBooleanObj().Value;
            if (geciciDosyalarOtomatikOlarakKaldirilsin) OnStartQuartz();
            
        




        }
        public static IScheduler ConfigureQuartz()
        {
            NameValueCollection props = new NameValueCollection
            {
                {"Quartz.serializer.type","binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            var scheduler = factory.GetScheduler().Result;
            scheduler.Start().Wait();
            return scheduler;
        }
        private static void OnShutDown()
        {
            if (!_quartzScheduler.IsShutdown) _quartzScheduler.Shutdown();
        }
        private void OnStartQuartz()
        {
            _quartzScheduler.Clear();
            _quartzScheduler.Start();
            _quartzScheduler.ResumeAll();
            IJobDetail job = Jobs.Jobs.JobDetailGeciciDosyaTemizleme();
            ITrigger trigger = Jobs.Jobs.TriggerGeciciDosyaTemizleme();
            _quartzScheduler.ScheduleJob(job, trigger);
        }
        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var err = Server.GetLastError();
        //    if (HttpContext.Current.Response != null)
        //    {
        //        var sCode = HttpContext.Current.Response.StatusCode;
        //        if (sCode == 404 || sCode == 200)
        //        {
        //           // Response.Redirect("/PageNotFound/Index");
        //        }

        //    } 

        //}

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception == null && Context.AllErrors != null && Context.AllErrors.Length > 0)
            {
                exception = Context.AllErrors[0];
            }
             
            RouteData routeData = new RouteData();
            IController errorController = new WebApp.Controllers.HomeController();
            if (exception == null)
            {

                routeData.Values.Add("controller", "Home");
                routeData.Values.Add("action", "Index");
            }
            else //It's an Http Exception, Let's handle it.
            {
                var errCode = HttpContext.Current.Response.StatusCode;
                if (errCode == HttpDurumKod.NotFound || errCode == HttpDurumKod.Unauthorized)
                {
                    var url = HttpContext.Current.Request.Url;
                    routeData.Values.Add("error", url);
                    routeData.Values.Add("ErrC", errCode);
                    errorController = new WebApp.Controllers.AppEventController();
                    routeData.Values.Add("controller", "AppEvent");
                    routeData.Values.Add("action", "PageNotFound");
                    Response.TrySkipIisCustomErrors = true;
                    Server.ClearError();
                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
                else
                {
                    //routeData.Values.Add("controller", "Home");
                    //routeData.Values.Add("action", "Index");
                    errorController = new WebApp.Controllers.AppEventController();
                    var url = HttpContext.Current.Request.Url;
                    routeData.Values.Add("error", url);
                    routeData.Values.Add("ErrC", errCode);
                    routeData.Values.Add("controller", "AppEvent");
                    routeData.Values.Add("action", "Error");
                    routeData.Values.Add("exception", exception);

                    Response.TrySkipIisCustomErrors = true;
                    Server.ClearError();
                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }

            }
        }
        protected void Application_EndRequest(Object sender, EventArgs e)
        {

            //if (HttpContext.Current.Response.Status.StartsWith(HttpDurumKod.MovedTemporarily.ToString()))
            //{
            //    HttpContext.Current.Response.ClearContent();
            //    var url = HttpContext.Current.Request.Url;
            //    IController loginC = new WebApp.Controllers.AccountController();
            //    RouteData routeData = new RouteData();
            //    var culture = "";
            //    var reqC = Request.RawUrl.Split('/').ToList();
            //    foreach (var item in reqC)
            //    { 
            //        if (item.IsContainsCulture())
            //        {
            //            culture = item.ToLower();
            //            break;
            //        }
            //    }
            //    Response.Clear();
            //    routeData.Values.Add("controller", "Account");
            //    routeData.Values.Add("action", "Login");
            //    routeData.Values.Add("ReturnUrl", url);
            //    routeData.Values.Add("Culture", culture);
            //    loginC.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            //    // Server.Execute("/account/login?ReturnUrl" + url);
            //}
        }
        void SystemInformation_OnEvent(BiskaUtil.SystemInformation info)
        {
            Management.AddMessage(info);
        }

        void Membership_OnRequireUserIdentity(string UserName, ref BiskaUtil.UserIdentity userIdentity)
        {
            userIdentity = Management.GetUserIdentity(UserName);

        }
        protected void Application_AcquireRequestState(Object sender, EventArgs e)
        {
            BiskaUtil.UserIdentity.SetCurrent();
            var session = HttpContext.Current.Session;

            var Ouser = HttpContext.Current.User;
            if (session != null)
            {
                string browser = "";
                string platform = "";
                string version = "";
                if (HttpContext.Current.Request.Browser.IsMobileDevice)
                { platform = HttpContext.Current.Request.Browser.MobileDeviceManufacturer + " " + HttpContext.Current.Request.Browser.MobileDeviceModel; }
                else { platform = HttpContext.Current.Request.Browser.Platform; }
                browser = HttpContext.Current.Request.Browser.Browser;
                version = HttpContext.Current.Request.Browser.Version;
                //var q = HttpContext.Current.Request.UserAgent.ToString().toDeviceType();  

                //var userAgent = HttpContext.Current.Request.UserAgent; 
                var UniqueId = Session["UserId"].ToStrObj();

                if (UniqueId != null)
                {
                    var usr = OnlineUsers.users.Where(p => p.UniqueId == UniqueId).FirstOrDefault();
                    if (usr != null)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (usr.IsYetkiyeniye)
                            {
                                BiskaUtil.UserIdentity.SetCurrent(usr.UserName);
                                usr.IsYetkiyeniye = false;
                            }
                            var user = Management.GetUser();
                            usr.KullaniciID = user.KullaniciID;
                            usr.Name = user.Ad + " " + user.Soyad;
                            usr.UserName = user.KullaniciAdi;
                            usr.Platform = platform;
                            usr.Browser = browser;
                            usr.Version = version;
                            usr.YetkiGrupAdi = user.YetkiGrupAdi;
                            usr.ResimAdi = user.ResimAdi.ToKullaniciResim();
                            usr.IsAuthenticated = true;
                        }
                        else
                        {
                            usr.Name = "Misafir";
                            usr.ResimAdi = "".ToKullaniciResim();
                            usr.YetkiGrupAdi = "";
                            usr.Platform = platform;
                            usr.Browser = browser;
                            usr.Version = version;
                            usr.IsAuthenticated = false;
                        }
                    }
                }
            }

        }
        void Session_Start(object sender, EventArgs e)
        {
            var UniqueId = Guid.NewGuid().ToString();
            Session["UserId"] = UniqueId;
            OnlineUsers.AddUser(UniqueId, null);

            
        }
        void Session_End(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                var UniqueId = Session["UserId"].ToString();
                var oUser = OnlineUsers.users.Where(p => p.UniqueId == UniqueId).FirstOrDefault();
                if (oUser != null && oUser.KullaniciID.HasValue)
                {
                    using (var db = new MusskDBEntities())
                    {
                        var kul = db.Kullanicilars.Where(p => p.KullaniciID == oUser.KullaniciID).FirstOrDefault();
                        if (kul != null)
                        {
                            kul.LastLogonDate = DateTime.Now;
                            db.SaveChanges();
                        }
                    }

                }
                OnlineUsers.RemoveUser(UniqueId);
                //Response.Redirect("/Home/Index");
            }
        }
    }
}
