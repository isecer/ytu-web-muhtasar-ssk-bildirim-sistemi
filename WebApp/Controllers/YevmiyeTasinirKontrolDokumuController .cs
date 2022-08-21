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
    [Authorize(Roles = RoleNames.YevmiyelerTasinirKontrolDokumu)]
    public class YevmiyeTasinirKontrolDokumuController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            return Index(new FmTasinirKontrolDokumu { Yil = Yil, PageSize = 50 },   export);
        }
        [HttpPost]
        public ActionResult Index(FmTasinirKontrolDokumu model, bool export = false)
        {
            var TasinirKontrolKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari).Select(s => s.HesapKod).ToList();

            var q = (from yv in db.Yevmiyelers.Where(p => TasinirKontrolKods.Contains(p.HesapKod) && p.YevmiyeTarih.Year == model.Yil)
                     join hb in db.YevmiyelerHarcamaBirimleris on yv.YevmiyeHarcamaBirimID equals hb.YevmiyeHarcamaBirimID
                     group new
                     {
                         yv.Alacak,
                         yv.Borc
                     } by new
                     {
                         hb.VergiKimlikNo,
                         hb.BirimAdi,
                         hb.YevmiyeHarcamaBirimID,
                         yv.HesapKod,
                         yv.HesapAdi,

                     } into g1
                     select new
                     {
                         g1.Key.YevmiyeHarcamaBirimID,
                         g1.Key.VergiKimlikNo,
                         g1.Key.BirimAdi,
                         g1.Key.HesapKod,
                         g1.Key.HesapAdi,
                         Alacak = g1.Sum(sm => sm.Alacak),
                         Borc = g1.Sum(sm => sm.Borc),
                         Kalan = g1.Sum(sm => sm.Borc) - g1.Sum(sm => sm.Alacak),

                     });
            if (model.YevmiyeHarcamaBirimID.HasValue) q = q.Where(p => p.YevmiyeHarcamaBirimID == model.YevmiyeHarcamaBirimID);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod);
            if (!model.HesapAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapAdi == model.HesapAdi);

            model.RowCount = q.Count();

            #region export
            if (export && model.RowCount > 0)
            {
                var gv = new GridView();
                gv.DataSource = q.Select(s => new
                {
                    s.VergiKimlikNo,
                    s.BirimAdi,
                    s.HesapKod,
                    s.HesapAdi,
                    s.Alacak,
                    s.Borc,
                    s.Kalan,
                }).ToList();
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_TasinirKontrolDokumu_" + model.Yil + ".xls");
            }
            #endregion

            model.BorcToplam = q.Sum(s => (decimal?)s.Borc) ?? 0;
            model.AlacakToplam = q.Sum(s => (decimal?)s.Alacak) ?? 0;
            model.KalanToplam = model.BorcToplam - model.AlacakToplam;



            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.BirimAdi).ThenBy(t => t.HesapAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).Select(s => new FrTasinirKontrolDokumu
            {
                VergiKimlikNo = s.VergiKimlikNo,
                BirimAdi = s.BirimAdi,
                HesapKod = s.HesapKod,
                HesapAdi = s.HesapAdi,
                Alacak = s.Alacak,
                Borc = s.Borc,
                Kalan = s.Kalan,
            }).ToArray();
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);
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