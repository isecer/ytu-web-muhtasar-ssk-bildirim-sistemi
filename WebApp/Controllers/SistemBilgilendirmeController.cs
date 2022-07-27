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
    [Authorize(Roles = RoleNames.SistemBilgilendirme)]
    public class SistemBilgilendirmeController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmSistemBilgilendirme() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmSistemBilgilendirme model)
        {
            var q = from s in db.SistemBilgilendirmes
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID into defK
                    from kd in defK.DefaultIfEmpty()
                    select new
                    {
                        s.SistemBilgiID,
                        s.BilgiTipi,
                        s.Kategori,
                        s.Message,
                        s.StackTrace,
                        s.IslemYapanID,
                        AdSoyad = s.IslemYapanID.HasValue ? (kd.Ad + " " + kd.Soyad) : (string)null,
                        KullaniciAdi = s.IslemYapanID.HasValue ? "[" + kd.KullaniciAdi + "]" : (string)null,
                        s.IslemTarihi,
                        s.IslemYapanIP
                    };

            if (model.IslemZamani.HasValue)
            {
                var mintar = model.IslemZamani.TodateToShortDate().Value;
                var maxtar = model.IslemZamani.Value.TodateToShortDate().AddDays(1);
                q = q.Where(p => p.IslemTarihi >= mintar && p.IslemTarihi < maxtar);

            }
            if (!model.Message.IsNullOrWhiteSpace()) q = q.Where(p => p.Message.Contains(model.AdSoyad) || p.StackTrace.Contains(model.Message));
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.KullaniciAdi.Contains(model.AdSoyad) || p.IslemYapanIP.Contains(model.AdSoyad));
            if (model.BilgiTipi.HasValue) q = q.Where(p => p.BilgiTipi == model.BilgiTipi);

            model.RowCount = q.Count();

            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(o => o.IslemTarihi);

            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;

            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).Select(s => new FrSistemBilgilendirme
            {
                SistemBilgiID = s.SistemBilgiID,
                BilgiTipi = s.BilgiTipi,
                Kategori = s.Kategori,
                Message = s.Message,
                StackTrace = s.StackTrace,
                KullaniciAdi = s.KullaniciAdi,
                AdSoyad = s.AdSoyad,
                IslemZamani = s.IslemTarihi,
                IpAdresi = s.IslemYapanIP
            }).ToArray();
            var btip = new BilgiTipleri();
            ViewBag.BilgiTipi = new SelectList(btip.BilgiTip.Select(s => new { s.BilgiTipID, s.BilgiTipAdi }).ToList(), "BilgiTipID", "BilgiTipAdi", model.BilgiTipi);
            ViewBag.IndexModel = IndexModel;
            return View(model);
        }
        public ActionResult getDetay(int SistemBilgiID)
        {
            var q = db.SistemBilgilendirmes.Where(p => p.SistemBilgiID == SistemBilgiID).First();
            return View(q);
        }



    }
}
