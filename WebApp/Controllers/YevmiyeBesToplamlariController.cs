using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.YevmiyeBesToplamlari)]
    public class YevmiyeBesToplamlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            return Index(new FmYevmiyelerBesBankaHesapNumaralari { Yil = Yil }, export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerBesBankaHesapNumaralari model, bool export = false)
        {
            var IsYevmiyeDokumuAyriOlabilirHk = db.YevmiyelerBesBankaHesapNumaralaris.Where(p => p.IsYevmiyeDokumuAyriOlabilir).Select(s => s.HesapKod).ToList();
            var q = (from bs in db.YevmiyelerBesBankaHesapNumaralaris
                     join Yb in db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && p.BESIsYevmiyeOdendi != false) on new
                     {
                         bs.HesapKod,
                         VergiKimlikNo = (bs.IsYevmiyeDokumuAyriOlabilir ? null : bs.VergiKimlikNo)
                     } equals new
                     {
                         HesapKod = (Yb.BESYevmiyeHesapKodID.HasValue ? Yb.YevmiyelerHesapKodlari.HesapKod : Yb.HesapKod),
                         VergiKimlikNo = IsYevmiyeDokumuAyriOlabilirHk.Contains(Yb.HesapKod) ? null : Yb.VergiKimlikNo
                     }
                        into defYb
                     from Yb in defYb.DefaultIfEmpty()
                     group new { Borc = (Yb == null ? 0 : Yb.Borc), Alacak = (Yb == null ? 0 : Yb.Alacak) }
                    by new
                    {
                        bs.YevmiyeBesBankaHesapNumaraID,
                        bs.HesapKod,
                        bs.VergiKimlikNo,
                        bs.FirmaAdi,
                        bs.IBanNo,
                        bs.Aciklama
                    }
                    into g1
                     select new
                     {
                         g1.Key.YevmiyeBesBankaHesapNumaraID,
                         g1.Key.HesapKod,
                         g1.Key.VergiKimlikNo,
                         g1.Key.FirmaAdi,
                         g1.Key.IBanNo,
                         g1.Key.Aciklama,
                         Borc = g1.Sum(sm => sm.Borc),
                         Alacak = g1.Sum(sm => sm.Alacak),
                         Kalan = g1.Sum(sm => sm.Borc) - g1.Sum(sm => sm.Alacak)
                     }).AsQueryable();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.Aciklama);
            model.Data = q.Select(s => new FrYevmiyelerBesBankaHesapNumaralari
            {
                YevmiyeBesBankaHesapNumaraID = s.YevmiyeBesBankaHesapNumaraID,
                HesapKod = s.HesapKod,
                VergiKimlikNo = s.VergiKimlikNo,
                FirmaAdi = s.FirmaAdi,
                IBanNo = s.IBanNo,
                Aciklama = s.Aciklama,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Kalan = s.Alacak - s.Borc
            }).ToList();

            var HaricEklenecekler = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && p.BESIsYevmiyeOdendi == false).Select(s => new FrYevmiyelerBesBankaHesapNumaralari
            {
                HesapKod = s.HesapKod,
                // VergiKimlikNo = s.VergiKimlikNo,
                FirmaAdi = s.HesapAdi,
                Aciklama = s.Aciklama,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Kalan = s.Alacak - s.Borc
            }).ToList();
            model.Data.AddRange(HaricEklenecekler);

            #region export
            if (export && model.Data.Count > 0)
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Select(s=>new {
                    s.HesapKod,
                    s.VergiKimlikNo,
                    s.FirmaAdi,
                    s.IBanNo,
                    s.Aciklama,
                    s.Borc,
                    s.Alacak,
                    s.Kalan 
                });
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_BireyselEmeklilikToplamlari_" + model.Yil + ".xls");
            }
            #endregion


            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            return View(model);
        }




    }
}