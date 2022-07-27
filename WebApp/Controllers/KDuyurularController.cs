using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BiskaUtil;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]

    public class KDuyurularController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmDuyurular() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmDuyurular model)
        {
            var q = from s in db.Duyurulars
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    where s.IsAktif && (s.YayinSonTarih.HasValue ? s.YayinSonTarih.Value >= DateTime.Now : 1 == 1) 
                    select new
                    {   
                        s.DuyuruID,
                        s.Tarih,
                        s.Baslik,
                        s.Aciklama,
                        s.AciklamaHtml,
                        DuyuruYapan = k.Ad + " " + k.Soyad,
                        s.IslemYapanIP,
                        EkSayisi = s.DuyuruEkleris.Count,
                        Ekler = s.DuyuruEkleris
                    };

            if (!model.Baslik.IsNullOrWhiteSpace()) q = q.Where(p => p.Baslik.Contains(model.Baslik));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));

            if (model.Tarih.HasValue)
            {
                var t1 = model.Tarih.Value.TodateToShortDate();
                var t2 = Convert.ToDateTime(model.Tarih.Value.ToShortDateString() + " 23:59:59");
                q = q.Where(p => p.Tarih >= t1 && p.Tarih <= t2);

            }
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(o => o.Tarih);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).Select(s => new FrDuyurular
            {
                DuyuruID = s.DuyuruID,
                Baslik = s.Baslik,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                Tarih = s.Tarih,
                DuyuruYapan = s.DuyuruYapan,
                IslemYapanIP = s.IslemYapanIP,
                EkSayisi = s.EkSayisi,
                DuyuruEkleris = s.Ekler
            }).ToList();
            return View(model);
        }


        public ActionResult getDuyuruJson(int PopupTipID)
        {

            var fModel = new FmDuyurular();
            var q = from s in db.Duyurulars
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    where s.IsAktif && s.Tarih <= DateTime.Now && (s.YayinSonTarih.HasValue ? s.YayinSonTarih.Value >= DateTime.Now : 1 == 1)
                    select new
                    { 
                        s.DuyuruID,
                        s.Tarih,
                        s.Baslik,
                        s.Aciklama,
                        s.AciklamaHtml,
                        DuyuruYapan = k.Ad + " " + k.Soyad,
                        s.IslemYapanIP,
                        EkSayisi = s.DuyuruEkleris.Count,
                        Ekler = s.DuyuruEkleris,
                        s.AnaSayfadaGozuksun,
                        s.AnaSayfaPopupAc, 
                        s.YayinSonTarih,
                        s.IsAktif
                    };
            if (PopupTipID == DuyuruPopupTipleri.AnaSayfa) q = q.Where(p => p.AnaSayfaPopupAc); 
            fModel.Data = q.Select(s => new FrDuyurular
            {  
                DuyuruID = s.DuyuruID,
                Baslik = s.Baslik,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                Tarih = s.Tarih,
                DuyuruYapan = s.DuyuruYapan,
                IslemYapanIP = s.IslemYapanIP,
                EkSayisi = s.EkSayisi,
                DuyuruEkleris = s.Ekler,
                AnaSayfadaGozuksun = s.AnaSayfadaGozuksun,
                AnaSayfaPopupAc = s.AnaSayfaPopupAc, 
                YayinSonTarih = s.YayinSonTarih
            }).OrderByDescending(o => o.Tarih).ToList();

            string htmlDuyuru = Management.RenderPartialView("KDuyurular", "DuyuruHtml", fModel);
            return Json(new { ShowMessage = fModel.Data.Count() > 0, HtmlMessage = htmlDuyuru });
        }
        public ActionResult DuyuruHtml(FmDuyurular model)
        {

            return View(model);
        }

    }
}
