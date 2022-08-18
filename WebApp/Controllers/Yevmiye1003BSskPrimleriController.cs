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
    [Authorize(Roles = RoleNames.Yevmiyeler1003BSskPrimleri)]
    public class Yevmiye1003BSskPrimleriController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiye1003BSskPrimleri { Yil = DateTime.Now.Year, AyID = DateTime.Now.Month });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiye1003BSskPrimleri model)
        {
            var HesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B).Select(s => s.HesapKod).ToList();
            var YevmiyeAlacakHesapKods = new List<string> { "360.01.01.01", "360.01.01.02" };
            var DamgaVergiHesapKodu = "360.03.01";
            var q = (from yb in db.Yevmiyelers.Where(p => HesapKods.Contains(p.HesapKod))
                     join yba in db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yil == model.Yil && p.AyID == model.AyID) on yb.YevmiyeID equals yba.YevmiyeID
                     join hb in db.YevmiyelerHarcamaBirimleris on yba.YevmiyeHarcamaBirimID equals hb.YevmiyeHarcamaBirimID
                     join bk in db.YevmiyelerBelgeKodlaris on yba.YevmiyeBelgeKodID equals bk.YevmiyeBelgeKodID
                     group new
                     {
                         SskPrimTutarToplam = yba.SskPrimTutar,
                         //YevmiyeAlacakToplam = (YevmiyeAlacakHesapKods.Contains(yb.HesapKod) ? yb.Alacak : 0),
                         //YevmiyeDvToplam = yb.HesapKod == DamgaVergiHesapKodu ? yb.Alacak : 0,
                         YevmiyeMatrahToplam = yba.Matrah,
                         yb.YevmiyeNo,
                     } by new
                     {
                         yb.IslemTarihi.Year,
                         yb.IslemTarihi.Month,
                         hb.YevmiyeHarcamaBirimID,
                         hb.BirimAdi,
                         hb.VergiKimlikNo,
                         hb.IsyeriKodu,
                         bk.BelgeKodu
                     } into g1
                     select new
                     {
                         g1.Key.Year,
                         g1.Key.Month,
                         g1.Key.YevmiyeHarcamaBirimID,
                         g1.Key.BirimAdi,
                         g1.Key.VergiKimlikNo,
                         g1.Key.IsyeriKodu,
                         g1.Key.BelgeKodu,
                         SskPrimTutarToplam = g1.Sum(s => s.SskPrimTutarToplam),
                         //YevmiyeAlacakToplam = g1.Sum(s => s.YevmiyeAlacakToplam),
                         //YevmiyeDvToplam = g1.Sum(s => s.YevmiyeDvToplam),
                         YevmiyeMatrahToplam = g1.Sum(s => s.YevmiyeMatrahToplam),
                         YevmiyeNos = g1.Select(g => g.YevmiyeNo).ToList()

                     }).ToList();
            var YevmiyeNos = q.SelectMany(s => s.YevmiyeNos).ToList();
            var Yevmiyes = db.Yevmiyelers.Where(p => YevmiyeNos.Contains(p.YevmiyeNo) && (YevmiyeAlacakHesapKods.Contains(p.HesapKod) || p.HesapKod == DamgaVergiHesapKodu) && p.YevmiyeTarih.Year == model.Yil).ToList();


            var IsciBildirgeVerileri = db.VASurecleriBirimVerileris.Where(p => p.VASurecleriBirim.VASurecleri.Yil == model.Yil && p.AyID == model.AyID).Select(s => new
            {
                IsyeriKodu = s.IsyeriSiraNumarasi,
                BelgeKodu = s.BelgeTurKodu,
                s.GvMatrahi,
                s.GvKesinti,
                s.DvKesintisi,
                UcretIkramiyeToplam = (s.HakEdilenUcret + s.PrimIkramiyeIstihkak)

            }).ToList();
            var BelgeKodlaris = db.BelgeTurleris.Select(s => new { BelgeKodu = s.BelgeTurKodu, s.PrimYuzdesi }).ToList();





            var qDataL = (from s in q
                          join yvd in Yevmiyes on s.YevmiyeHarcamaBirimID equals yvd.YevmiyeHarcamaBirimID into defyvd
                          from yvd in defyvd.DefaultIfEmpty()
                          where s.YevmiyeNos.Contains(yvd.YevmiyeNo)
                          group new
                          {
                              YevmiyeAlacakToplam = (yvd != null && (YevmiyeAlacakHesapKods.Contains(yvd.HesapKod)) ? yvd.Alacak : 0),
                              YevmiyeDvToplam = yvd != null && yvd.HesapKod == DamgaVergiHesapKodu ? yvd.Alacak : 0
                          } by new
                          {

                              s.YevmiyeHarcamaBirimID,
                              s.BirimAdi,
                              s.VergiKimlikNo,
                              s.IsyeriKodu,
                              s.BelgeKodu,
                              s.SskPrimTutarToplam,
                              s.YevmiyeMatrahToplam

                          } into g1

                          select new
                          {
                              g1.Key.YevmiyeHarcamaBirimID,
                              g1.Key.BirimAdi,
                              g1.Key.VergiKimlikNo,
                              g1.Key.IsyeriKodu,
                              g1.Key.BelgeKodu,
                              g1.Key.SskPrimTutarToplam,
                              YevmiyeMatrahToplam = g1.Key.YevmiyeMatrahToplam ?? 0,
                              YevmiyeAlacakToplam = g1.Sum(s => s.YevmiyeAlacakToplam),
                              YevmiyeDvToplam = g1.Sum(s => s.YevmiyeDvToplam)

                          }).ToList();

            var qDataLx = (from s in qDataL
                           join bk in BelgeKodlaris on s.BelgeKodu equals bk.BelgeKodu
                           join ibv in IsciBildirgeVerileri on new { s.IsyeriKodu, s.BelgeKodu } equals new { ibv.IsyeriKodu, ibv.BelgeKodu } into defibv
                           from ibv in defibv.DefaultIfEmpty()
                           group new
                           {
                               GvMatrahi = ibv != null ? ibv.GvMatrahi : 0,
                               GvKesinti = ibv != null ? ibv.GvKesinti : 0,
                               DvKesintisi = ibv != null ? ibv.DvKesintisi : 0,
                               UcretIkramiyeToplam = ibv != null ? ibv.UcretIkramiyeToplam : 0
                           } by new
                           {

                               s.YevmiyeHarcamaBirimID,
                               s.BirimAdi,
                               s.VergiKimlikNo,
                               s.IsyeriKodu,
                               s.BelgeKodu,
                               s.SskPrimTutarToplam,
                               s.YevmiyeMatrahToplam,
                               s.YevmiyeAlacakToplam,
                               s.YevmiyeDvToplam,
                               bk.PrimYuzdesi

                           } into g1

                           select new
                           {
                               g1.Key.YevmiyeHarcamaBirimID,
                               g1.Key.BirimAdi,
                               g1.Key.VergiKimlikNo,
                               g1.Key.IsyeriKodu,
                               g1.Key.BelgeKodu,
                               g1.Key.SskPrimTutarToplam,
                               g1.Key.YevmiyeMatrahToplam,
                               g1.Key.YevmiyeAlacakToplam,
                               YevmiyeDvToplam = g1.Key.YevmiyeDvToplam,

                               MatrahToplam = g1.Key.YevmiyeMatrahToplam - g1.Sum(s => s.GvMatrahi ?? 0),
                               GvKesintiToplam = g1.Key.YevmiyeAlacakToplam - g1.Sum(s => s.GvKesinti ?? 0),
                               DvKesintiToplam = g1.Key.YevmiyeDvToplam - g1.Sum(s => s.DvKesintisi ?? 0),
                               SskTutarToplam = g1.Key.SskPrimTutarToplam - g1.Sum(s => s.UcretIkramiyeToplam) * (g1.Key.PrimYuzdesi / 100),

                           }).ToList();

            var qData = qDataLx.AsQueryable();

            if (!model.Sort.IsNullOrWhiteSpace()) qData = qData.OrderBy(model.Sort);
            else qData = qData.OrderBy(o => o.BirimAdi);
            model.Data = qData.Select(s => new FrYevmiye1003BSskPrimleri
            {
                YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                BirimAdi = s.BirimAdi,
                VergiKimlikNo = s.VergiKimlikNo,
                IsyeriKodu = s.IsyeriKodu,
                BelgeKodu = s.BelgeKodu,
                SskPrimTutarToplam = s.SskPrimTutarToplam,
                YevmiyeAlacakToplam = s.YevmiyeAlacakToplam,
                YevmiyeDvToplam = s.YevmiyeDvToplam,
                YevmiyeMatrahToplam = s.YevmiyeMatrahToplam,
                MatrahToplam = s.MatrahToplam,
                GvKesintiToplam = s.GvKesintiToplam,
                DvKesintiToplam = s.DvKesintiToplam,
                SskTutarToplam = s.SskTutarToplam

            }).ToArray();
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(false), "Value", "Caption", model.AyID);
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