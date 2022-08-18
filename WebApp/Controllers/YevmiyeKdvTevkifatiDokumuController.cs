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
    [Authorize(Roles = RoleNames.Yevmiyeler1003AMuhtasarDokumu)]
    public class YevmiyeKdvTevkifatiDokumuController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmKdvTevkifatiDokumu { Yil = DateTime.Now.Year, AyID = DateTime.Now.Month });
        }
        [HttpPost]
        public ActionResult Index(FmKdvTevkifatiDokumu model)
        {
            var KdvTevkifatKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari).Select(s => s.HesapKod).ToList();

            var q = (from yv in db.Yevmiyelers.Where(p => KdvTevkifatKods.Contains(p.HesapKod))
                     join ktd in db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.FaturaYil == model.Yil && p.FaturaAyID == model.AyID) on yv.YevmiyeID equals ktd.YevmiyeID
                     group new
                     {
                         ktd.Matrah,
                         ktd.TevkifatTutari
                     } by new
                     {
                         ktd.FaturaYil,
                         ktd.FaturaAyID,
                         ktd.YevmiyelerKdvKodlari.KdvAdi,
                         ktd.KdvKodu,
                         ktd.KdvOrani,
                         ktd.TevkifatOranBolen,
                         ktd.TevkifatOranBolunen,
                     } into g1
                     select new
                     {
                         g1.Key.FaturaYil,
                         g1.Key.FaturaAyID,
                         g1.Key.KdvAdi,
                         g1.Key.KdvKodu,
                         g1.Key.KdvOrani,
                         BolumOran = g1.Key.TevkifatOranBolunen + "/" + g1.Key.TevkifatOranBolen,
                         Matrah = g1.Sum(sm => sm.Matrah),
                         TevkifatTutari = g1.Sum(sm => sm.TevkifatTutari),

                     });

            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.KdvKodu).ThenBy(t => t.KdvOrani);
            model.Data = q.Select(s => new FrKdvTevkifatiDokumu
            {

                FaturaYil = s.FaturaYil,
                FaturaAyID = s.FaturaAyID,
                KdvAdi = s.KdvAdi,
                KdvKodu = s.KdvKodu,
                KdvOrani = s.KdvOrani,
                BolumOran = s.BolumOran,
                Matrah = s.Matrah,
                TevkifatTutari = s.TevkifatTutari,

            }).ToArray();
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