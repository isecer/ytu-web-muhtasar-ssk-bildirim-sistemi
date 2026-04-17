using System;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class HomeController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();

    
        public ActionResult Index(string MesajGroupID)
        {

            #region duyurular 
            try
            {
                var DuyuruList = (from s in db.Duyurulars
                                  join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                                  where s.IsAktif && s.Tarih <= DateTime.Now && (s.YayinSonTarih.HasValue ? s.YayinSonTarih.Value >= DateTime.Now : 1 == 1) && s.AnaSayfadaGozuksun
                                  select new FrDuyurular
                                  {
                                      DuyuruID = s.DuyuruID,
                                      Baslik = s.Baslik,
                                      Aciklama = s.Aciklama,
                                      AciklamaHtml = s.AciklamaHtml,
                                      Tarih = s.Tarih,
                                      DuyuruYapan = k.Ad + " " + k.Soyad,
                                      IslemYapanIP = s.IslemYapanIP,
                                      EkSayisi = s.DuyuruEkleris.Count,
                                      DuyuruEkleris = s.DuyuruEkleris,
                                      AnaSayfadaGozuksun = s.AnaSayfadaGozuksun,
                                      AnaSayfaPopupAc = s.AnaSayfaPopupAc,
                                      YayinSonTarih = s.YayinSonTarih
                                  }).OrderByDescending(o => o.Tarih).ToList();
                ViewBag.Duyurular = DuyuruList;
            }
            catch
            {
                ViewBag.Duyurular = new Duyurular[0];
            }

            //YeniDersKontrol();
            #endregion
            if (MesajGroupID.IsNullOrWhiteSpace() == false)
            {
                var SecilenMesaj = db.Mesajlars.Where(p => p.GroupID == MesajGroupID).FirstOrDefault();
                ViewBag.MesajGroupID = SecilenMesaj != null ? MesajGroupID : "";
            }
            else ViewBag.MesajGroupID = ""; 
            return View();

        }

       
        public ActionResult AuthenticatedControl()
        {
            if (Request.Browser.IsMobileDevice) { }
            return Json(UserIdentity.Current.IsAuthenticated, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
