using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AppEventController : Controller
    {
        //
        // GET: /PageNotFound/
        public ActionResult PageNotFound(string error,int ErrC)
        {
            ViewBag.SayfaAdi = error;
            ViewBag.ErrC = ErrC;
            return View();
        }

        public ActionResult Error(string url, int ErrC,Exception exception)
        {
            ViewBag.SayfaAdi = url;
            ViewBag.ErrC = ErrC;
            using (var db = new MusskDBEntities())
            {
                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = BilgiTipi.Hata,
                    Message = exception.ToExceptionMessage(),
                    IslemYapanID = (UserIdentity.Current.IsAuthenticated ? UserIdentity.Current.Id : (int?)null),
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = exception.ToExceptionStackTrace()
                });
                db.SaveChanges();
            }
            return View(exception);
        }
    }
}