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
    [Authorize(Roles = RoleNames.YevmiyeSendikaToplamlari)]
    public class YevmiyeSendikaToplamlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            return Index(new FmYevmiyeSendikaToplamlari { Yil = Yil },  export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeSendikaToplamlari model, bool export = false)
        {
            var HesapKods = db.YevmiyelerSendikaBilgileris.Select(s => s.HesapKod).ToList();
            var q = (from sb in db.YevmiyelerSendikaBilgileris
                     join Yb in db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && HesapKods.Contains(p.HesapKod)) on new { sb.HesapKod } equals new { HesapKod = (Yb.YevmiyeSendikaBilgiID.HasValue ? Yb.YevmiyelerSendikaBilgileri.HesapKod : Yb.HesapKod) } into defYb
                     from Yb in defYb.DefaultIfEmpty()
                     group new { Borc = (Yb == null ? 0 : Yb.Borc), Alacak = (Yb == null ? 0 : Yb.Alacak) } by new { sb.YevmiyeSendikaBilgiID, sb.HesapKod, sb.VergiKimlikNo, sb.AdSoyad, sb.IBanNo, sb.KisaAdi, sb.Aciklama } into g1
                     select new
                     {
                         g1.Key.YevmiyeSendikaBilgiID,
                         g1.Key.HesapKod,
                         g1.Key.VergiKimlikNo,
                         g1.Key.AdSoyad,
                         g1.Key.IBanNo,
                         g1.Key.KisaAdi,
                         g1.Key.Aciklama,
                         Borc = g1.Sum(sm => sm.Borc),
                         Alacak = g1.Sum(sm => sm.Alacak),
                         Kalan = g1.Sum(sm => sm.Borc) - g1.Sum(sm => sm.Alacak)
                     }).AsQueryable();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.AdSoyad);
            model.Data = q.Select(s => new FrYevmiyeSendikaToplamlari
            {
                YevmiyeSendikaBilgiID = s.YevmiyeSendikaBilgiID,
                HesapKod = s.HesapKod,
                VergiKimlikNo = s.VergiKimlikNo,
                AdSoyad = s.AdSoyad,
                IBanNo = s.IBanNo,
                KisaAdi = s.KisaAdi,
                Aciklama = s.Aciklama,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Kalan = s.Alacak - s.Borc
            }).ToArray();
            #region export
            if (export && model.Data.Any())
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Select(s => new
                {
                    s.HesapKod,
                    s.VergiKimlikNo,
                    s.AdSoyad,
                    s.IBanNo,
                    s.KisaAdi,
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

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_SendikaToplamlari_" + model.Yil + ".xls");
            }
            #endregion
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            return View(model);
        }




    }
}