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
    [Authorize(Roles = RoleNames.YevmiyelerEmekliKesenekToplamlari)]
    public class YevmiyeEmekliKesenekToplamlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            return Index(new FmYevmiyeEkHbToplamlari { Yil = DateTime.Now.Year }, export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeEkHbToplamlari model, bool export = false)
        {
            var HesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari).Select(s => s.HesapKod).ToList();
            var q = (from Hb in db.YevmiyelerHarcamaBirimleris.Where(p => !p.IsAltBirim)
                     join Yb in db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && HesapKods.Contains(p.HesapKod)) on new { Hb.YevmiyeHarcamaBirimID } equals new { YevmiyeHarcamaBirimID = Yb.EKYevmiyeHarcamaBirimID ?? Yb.YevmiyeHarcamaBirimID } into defYb
                     from Yb in defYb.DefaultIfEmpty()
                     group new { Borc = (Yb == null ? 0 : Yb.Borc), Alacak = (Yb == null ? 0 : Yb.Alacak) } by new { Hb.YevmiyeHarcamaBirimID, Hb.SaymanlikKod, Hb.VergiKimlikNo, Hb.BirimAdi } into g1
                     select new
                     {
                         g1.Key.YevmiyeHarcamaBirimID,
                         g1.Key.VergiKimlikNo,
                         g1.Key.SaymanlikKod,
                         g1.Key.BirimAdi,
                         Borc = g1.Sum(sm => sm.Borc),
                         Alacak = g1.Sum(sm => sm.Alacak),
                         YevmiyelerHarcamaBirimleriTutarKayits = db.YevmiyelerHarcamaBirimleriTutarKayits.Where(p => p.Yil == model.Yil && p.YevmiyeHarcamaBirimID == g1.Key.YevmiyeHarcamaBirimID).ToList()

                     }).AsQueryable();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.BirimAdi);
            model.Data = q.Select(s => new FrYevmiyeEkHbToplamlari
            {
                YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                VergiKimlikNo = s.VergiKimlikNo,
                SaymanlikKod = s.SaymanlikKod,
                BirimAdi = s.BirimAdi,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Kalan = s.Alacak - s.Borc,
                YevmiyelerHarcamaBirimleriTutarKayits = s.YevmiyelerHarcamaBirimleriTutarKayits,
                KayitToplam = (s.YevmiyelerHarcamaBirimleriTutarKayits.Sum(sm => (decimal?)sm.Tutar) ?? 0),
                KalanTutar = s.Alacak - s.Borc - (s.YevmiyelerHarcamaBirimleriTutarKayits.Sum(sm => (decimal?)sm.Tutar) ?? 0)

            }).ToArray();
            #region export
            if (export && model.Data.Any())
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Select(s => new
                {
                    s.VergiKimlikNo,
                    s.SaymanlikKod,
                    s.BirimAdi,
                    s.Borc,
                    s.Alacak,
                    s.Kalan,
                    Tutar1 = s.YevmiyelerHarcamaBirimleriTutarKayits.Any() ? s.YevmiyelerHarcamaBirimleriTutarKayits[0].Tutar : 0,
                    Tutar2 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 1 ? s.YevmiyelerHarcamaBirimleriTutarKayits[1].Tutar : 0,
                    Tutar3 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 2 ? s.YevmiyelerHarcamaBirimleriTutarKayits[2].Tutar : 0,
                    Tutar4 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 3 ? s.YevmiyelerHarcamaBirimleriTutarKayits[3].Tutar : 0,
                    Tutar5 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 4 ? s.YevmiyelerHarcamaBirimleriTutarKayits[4].Tutar : 0,
                    Tutar6 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 5 ? s.YevmiyelerHarcamaBirimleriTutarKayits[5].Tutar : 0,
                    Tutar7 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 6 ? s.YevmiyelerHarcamaBirimleriTutarKayits[6].Tutar : 0,
                    Tutar8 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 7 ? s.YevmiyelerHarcamaBirimleriTutarKayits[7].Tutar : 0,
                    Tutar9 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 8 ? s.YevmiyelerHarcamaBirimleriTutarKayits[8].Tutar : 0,
                    Tutar10 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 9 ? s.YevmiyelerHarcamaBirimleriTutarKayits[9].Tutar : 0,
                    Tutar11 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 10 ? s.YevmiyelerHarcamaBirimleriTutarKayits[10].Tutar : 0,
                    Tutar12 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 11 ? s.YevmiyelerHarcamaBirimleriTutarKayits[11].Tutar : 0,
                    Tutar13 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 12 ? s.YevmiyelerHarcamaBirimleriTutarKayits[12].Tutar : 0,
                    Tutar14 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 13 ? s.YevmiyelerHarcamaBirimleriTutarKayits[13].Tutar : 0,
                    Tutar15 = s.YevmiyelerHarcamaBirimleriTutarKayits.Count() > 14 ? s.YevmiyelerHarcamaBirimleriTutarKayits[14].Tutar : 0,
                    HesapKayitEdilen = s.KayitToplam,
                    HesapKalanNet = s.KalanTutar,
                   
                });
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_EmekliKesenekToplamlari_" + model.Yil + ".xls");
            }
            #endregion
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            return View(model);
        }
        public ActionResult YevmiyeBirimExport(int Yil, int YevmiyeHarcamaBirimID)
        {
            var HesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari).Select(s => s.HesapKod).ToList();
            var Data = db.Yevmiyelers.Where(p => HesapKods.Contains(p.HesapKod) && p.YevmiyeTarih.Year == Yil && p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID).Select(s => new
            {
                s.YevmiyeTarih,
                s.YevmiyeNo,
                s.VergiKimlikNo,
                s.YevmiyelerHarcamaBirimleri.BirimAdi,
                s.HarcamaBirimKod,
                s.HesapKod,
                s.HesapAdi,
                s.Borc,
                s.Alacak,
                s.Aciklama
            }).OrderBy(o => o.YevmiyeNo).ToList();
            if (Data.Any())
            {
                var BirimAdi = Data.First().BirimAdi;
                var gv = new GridView();
                gv.DataSource = Data;
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_EKesenek_" + BirimAdi + "_" + Yil + ".xls");
            }
            else return null;
        }
        public ActionResult TutarEkle(int Yil, int YevmiyeHarcamaBirimID)
        {
            var HarcamaBirim = db.YevmiyelerHarcamaBirimleris.Where(p => p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID).First();
            ViewBag.HarcamaBirim = HarcamaBirim;
            ViewBag.Yil = Yil;
            var Kayitlar = db.YevmiyelerHarcamaBirimleriTutarKayits.Where(p => p.Yil == Yil && p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID).ToList();
            return View(Kayitlar);
        }

        [HttpPost]
        public ActionResult TutarEkle(int Yil, int YevmiyeHarcamaBirimID, List<decimal?> Tutar)
        {
            Tutar = Tutar ?? new List<decimal?>();
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Harcama Birimi Tutar Ekleme İşlemi";
            MmMessage.MessageType = Msgtype.Warning;
            var Kayitlar = db.YevmiyelerHarcamaBirimleriTutarKayits.Where(p => p.Yil == Yil && p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID).ToList();
            db.YevmiyelerHarcamaBirimleriTutarKayits.RemoveRange(Kayitlar);
            foreach (var item in Tutar.Where(p => p.HasValue && p.Value > 0))
            {
                db.YevmiyelerHarcamaBirimleriTutarKayits.Add(new YevmiyelerHarcamaBirimleriTutarKayit { Yil = Yil, YevmiyeHarcamaBirimID = YevmiyeHarcamaBirimID, Tutar = item.Value });
            }

            db.SaveChanges();
            MmMessage.Messages.Add("Tutar Bilgileri Kayıt Edildi");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;


            return MmMessage.ToJsonResult();
        }


    }
}