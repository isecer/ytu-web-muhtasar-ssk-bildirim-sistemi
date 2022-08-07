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
    [Authorize(Roles = RoleNames.YevmiyelerHarcamaBirimiToplamlari)]
    public class YevmiyeHarcamaBirimiToplamlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyeEkHbToplamlari { Yil = DateTime.Now.Year });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeEkHbToplamlari model)
        {
            var HesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari).Select(s => s.HesapKod).ToList();
            var q = (from Hb in db.YevmiyelerHarcamaBirimleris
                     join Yb in db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == model.Yil && HesapKods.Contains(p.HesapKod)) on new {Hb.YevmiyeHarcamaBirimID } equals new { YevmiyeHarcamaBirimID= Yb .EKYevmiyeHarcamaBirimID??Yb.YevmiyeHarcamaBirimID} into defYb
                     from Yb in defYb.DefaultIfEmpty()
                     group new { Borc = (Yb == null ? 0 : Yb.Borc), Alacak = (Yb == null ? 0 : Yb.Alacak) } by new { Hb.YevmiyeHarcamaBirimID, Hb.VergiKimlikNo, Hb.BirimAdi } into g1
                     select new
                     {
                         g1.Key.YevmiyeHarcamaBirimID,
                         g1.Key.VergiKimlikNo,
                         g1.Key.BirimAdi,
                         Borc = g1.Sum(sm => sm.Borc),
                         Alacak = g1.Sum(sm => sm.Alacak),
                         Kalan = g1.Sum(sm => sm.Alacak) - g1.Sum(sm => sm.Borc)
                     }).AsQueryable();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.BirimAdi);
            model.Data = q.Select(s => new FrYevmiyeEkHbToplamlari
            {
                YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                VergiKimlikNo = s.VergiKimlikNo,
                BirimAdi = s.BirimAdi,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Kalan = s.Alacak - s.Borc,
                KayitEdilen = db.YevmiyelerHarcamaBirimleriTutarKayits.Where(p => p.Yil == model.Yil && p.YevmiyeHarcamaBirimID == s.YevmiyeHarcamaBirimID).Sum(sm => (decimal?)sm.Tutar) ?? 0
            }).ToArray();
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            return View(model);
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