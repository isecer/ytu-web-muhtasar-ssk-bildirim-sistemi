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
    [Authorize(Roles = RoleNames.Yevmiyeler1003AMuhtasarDokumu)]
    public class Yevmiye1003AMuhatasarDokumuController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, int? AyID = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            if (!AyID.HasValue) AyID = DateTime.Now.Month;
            return Index(new FmYevmiye1003AMuhatasarDokumu { Yil = Yil, AyID = AyID }, export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiye1003AMuhatasarDokumu model, bool export = false)
        {
            var YevmiyeList = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && p.YevmiyeTarih.Month == model.AyID && p.Y1003AIsHesaplamayaGirecek == true).ToList();
            var YevmiyeHesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A).ToList();
            var Yevmiye1003AKayits = db.Yevmiyeler1003AMuhtasarKayitlari.Where(p => p.Yil == model.Yil && p.AyID == model.AyID).ToList();
            var q = (from yv in YevmiyeList
                     join hk in YevmiyeHesapKods on yv.HesapKod equals hk.HesapKod
                     join mk in Yevmiye1003AKayits on hk.VergiKodu equals mk.VergiKodu into defmk
                     from mk in defmk.DefaultIfEmpty()
                     group new
                     {
                         yv.Alacak,
                         yv.Borc
                     } by new
                     {
                         hk.VergiKodu,
                         GenelMatrahTutar = mk != null ? mk.GenelMatrahTutar : 0,
                         SksVergiTutar = mk != null ? mk.SksVergiTutar : 0,
                         ImidVergiTutar = mk != null ? mk.ImidVergiTutar : 0,
                         SksMatrahTutar = mk != null ? mk.SksMatrahTutar : 0,
                         ImidMatrahTutar = mk != null ? mk.ImidMatrahTutar : 0
                     } into g1
                     select new
                     {
                         g1.Key.VergiKodu,
                         Tutar = g1.Sum(sm => sm.Alacak) - g1.Sum(sm => sm.Borc),
                         g1.Key.SksVergiTutar,
                         g1.Key.ImidVergiTutar,
                         g1.Key.SksMatrahTutar,
                         g1.Key.ImidMatrahTutar,
                         g1.Key.GenelMatrahTutar,
                         KalanTutar = (g1.Sum(sm => sm.Alacak) - g1.Sum(sm => sm.Borc)) - (g1.Key.SksVergiTutar + g1.Key.ImidVergiTutar),
                         KalanMatrah = g1.Key.GenelMatrahTutar - (g1.Key.SksMatrahTutar + g1.Key.ImidMatrahTutar)

                     });

            model.Data = q.Select(s => new FrYevmiye1003AMuhatasarDokumu
            {

                VergiKodu = s.VergiKodu,
                Tutar = s.Tutar,
                SksVergiTutar = s.SksVergiTutar,
                ImidVergiTutar = s.ImidVergiTutar,
                KalanTutar = s.KalanTutar,
                ImidMatrahTutar = s.ImidMatrahTutar,
                SksMatrahTutar = s.SksMatrahTutar,
                GenelMatrahTutar = s.GenelMatrahTutar,
                KalanMatrah = s.KalanMatrah

            }).OrderBy(o => o.VergiKodu).ToList();
            #region export
            if (export && model.Data.Any())
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Select(s => new
                {
                    s.VergiKodu,
                    s.Tutar,
                    s.SksVergiTutar,
                    s.ImidVergiTutar,
                    s.KalanTutar,
                    s.ImidMatrahTutar,
                    s.SksMatrahTutar,
                    s.GenelMatrahTutar,
                    s.KalanMatrah
                });
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_1003AMuhtasarDokumu_" + model.Yil + "_" + model.AyID + ".xls");
            }

            var qD = (from yv in YevmiyeList
                      join hk in YevmiyeHesapKods on yv.HesapKod equals hk.HesapKod
                      group new
                      {
                          yv.Alacak,
                          yv.Borc
                      } by new
                      {
                          hk.HesapKod,
                          hk.HesapAdi
                      } into g1
                      select new
                      {
                          g1.Key.HesapKod,
                          g1.Key.HesapAdi,
                          Tutar = g1.Sum(sm => sm.Alacak) - g1.Sum(sm => sm.Borc),

                      });

            model.DataGelirKaydiToplam = qD.Select(s => new FrYevmiye1003AGelirKaydiToplam
            {

                HesapKod = s.HesapKod,
                HesapAdi = s.HesapAdi,
                Tutar = s.Tutar,

            }).OrderBy(o => o.HesapKod).ToList();

            #endregion
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(false), "Value", "Caption", model.AyID);
            return View(model);
        }


        public ActionResult TutarEkle(int Yil, int AyID, string VergiKodu)
        {
            var model = new Yevmiyeler1003AMuhtasarKayitlari() { Yil = Yil, AyID = AyID, VergiKodu = VergiKodu };
            var Kayit = db.Yevmiyeler1003AMuhtasarKayitlari.Where(p => p.Yil == Yil && p.AyID == AyID && p.VergiKodu == VergiKodu).FirstOrDefault();
            if (Kayit != null) model = Kayit;
            return View(model);
        }

        [HttpPost]
        public ActionResult TutarEkle(Yevmiyeler1003AMuhtasarKayitlari kModel)
        {

            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Harcama Birimi Tutar Ekleme İşlemi";
            MmMessage.MessageType = Msgtype.Warning;

            if (kModel.SksVergiTutar < 0)
            {
                MmMessage.Messages.Add("Sks Vergi Tutarı 0 dan küçük olamaz.");
            }
            if (kModel.ImidVergiTutar < 0)
            {
                MmMessage.Messages.Add("İmid Vergi Tutarı 0 dan küçük olamaz.");
            }
            if (kModel.SksMatrahTutar < 0)
            {
                MmMessage.Messages.Add("Sks Matrah Tutarı 0 dan küçük olamaz.");
            }
            if (kModel.ImidMatrahTutar < 0)
            {
                MmMessage.Messages.Add("İmid Matrah Tutarı 0 dan küçük olamaz.");
            }
            if (kModel.GenelMatrahTutar < 0)
            {
                MmMessage.Messages.Add("Genel Matrah Tutarı 0 dan küçük olamaz.");
            }
            if (MmMessage.Messages.Any() == false)
            {
                var Kayit = db.Yevmiyeler1003AMuhtasarKayitlari.Where(p => p.Yil == kModel.Yil && p.AyID == kModel.AyID && p.VergiKodu == kModel.VergiKodu).FirstOrDefault();
                if (Kayit == null)
                {
                    db.Yevmiyeler1003AMuhtasarKayitlari.Add(kModel);
                }
                else
                {
                    Kayit.SksVergiTutar = kModel.SksVergiTutar;
                    Kayit.ImidVergiTutar = kModel.ImidVergiTutar;
                    Kayit.SksMatrahTutar = kModel.SksMatrahTutar;
                    Kayit.ImidMatrahTutar = kModel.ImidMatrahTutar;
                    Kayit.GenelMatrahTutar = kModel.GenelMatrahTutar;
                }
                db.SaveChanges();
                MmMessage.Messages.Add("Tutar Bilgileri Kayıt Edildi.");
            }

            db.SaveChanges();
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;

            return MmMessage.ToJsonResult();
        }


    }
}