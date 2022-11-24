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
    [Authorize(Roles = RoleNames.Yevmiyeler1003BSskPrimleri)]
    public class Yevmiye1003BSskPrimleriController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index(int? Yil = null, int? AyID = null, bool export = false)
        {
            if (!Yil.HasValue) Yil = DateTime.Now.Year;
            if (!AyID.HasValue) AyID = DateTime.Now.Month;
            return Index(new FmYevmiye1003BSskPrimleri { Yil = Yil, AyID = AyID }, export);
        }
        [HttpPost]
        public ActionResult Index(FmYevmiye1003BSskPrimleri model, bool export = false)
        {
            var HesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B).Select(s => s.HesapKod).ToList();
            var YevmiyeAlacakHesapKods = new List<string> { "360.01.01.01", "360.01.01.02" };
            var DamgaVergiHesapKodu = "360.03.01";
            var q = (from yb in db.Yevmiyelers.Where(p => HesapKods.Contains(p.HesapKod))
                     join yba in db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yil == model.Yil && p.AyID == model.AyID) on yb.YevmiyeID equals yba.YevmiyeID
                     join hb in db.YevmiyelerHarcamaBirimleris on yba.YevmiyeHarcamaBirimID equals hb.YevmiyeHarcamaBirimID
                     join bk in db.YevmiyelerBelgeKodlaris on yba.YevmiyeBelgeKodID equals bk.YevmiyeBelgeKodID
                     join be in db.Yevmiyeler1003BSskPrimBoclari on new { yba.Yil, yba.AyID, yba.YevmiyeHarcamaBirimID, yba.YevmiyeBelgeKodID } equals new { be.Yil, be.AyID, be.YevmiyeHarcamaBirimID, be.YevmiyeBelgeKodID } into defbe
                     from be in defbe.DefaultIfEmpty()
                     group new
                     {
                         SskPrimTutarToplam = yba.SskPrimTutar,
                         //YevmiyeAlacakToplam = (YevmiyeAlacakHesapKods.Contains(yb.HesapKod) ? yb.Alacak : 0),
                         //YevmiyeDvToplam = yb.HesapKod == DamgaVergiHesapKodu ? yb.Alacak : 0,
                         YevmiyeMatrahToplam = yba.Matrah,
                         yb.YevmiyeNo,

                         yb.YevmiyeHarcamaBirimID,


                     } by new
                     {
                         YBAYevmiyeHarcamaBirimID = hb.YevmiyeHarcamaBirimID,
                         hb.BirimAdi,
                         hb.VergiKimlikNo,
                         hb.IsyeriKodu,
                         bk.BelgeKodu,
                         bk.YevmiyeBelgeKodID,
                         SskBorcTutar = be != null ? be.Tutar : 0,
                     } into g1
                     select new
                     {
                         g1.Key.YBAYevmiyeHarcamaBirimID,
                         g1.Key.BirimAdi,
                         g1.Key.VergiKimlikNo,
                         g1.Key.IsyeriKodu,
                         g1.Key.BelgeKodu,
                         g1.Key.YevmiyeBelgeKodID,
                         SskPrimTutarToplam = g1.Sum(s => s.SskPrimTutarToplam),
                         //YevmiyeAlacakToplam = g1.Sum(s => s.YevmiyeAlacakToplam),
                         //YevmiyeDvToplam = g1.Sum(s => s.YevmiyeDvToplam),
                         YevmiyeMatrahToplam = g1.Sum(s => s.YevmiyeMatrahToplam),
                         SskBorcTutar = g1.Key.SskBorcTutar,
                         YevmiyeNos = g1.Select(g => g.YevmiyeNo).ToList(),
                         YevmiyeHarcamaBirimIDs = g1.Select(s => s.YevmiyeHarcamaBirimID).ToList()

                     }).ToList();


            var YevmiyeNos = q.SelectMany(s => s.YevmiyeNos).ToList();
            var Yevmiyes = db.Yevmiyelers.Where(p => YevmiyeNos.Contains(p.YevmiyeNo) && (YevmiyeAlacakHesapKods.Contains(p.HesapKod) || p.HesapKod == DamgaVergiHesapKodu) && p.YevmiyeTarih.Year == model.Yil).Select(s => new
            {
                s.YevmiyeNo,
                s.YevmiyeID,
                s.HesapKod,
                s.Alacak,
                s.YevmiyeHarcamaBirimID,
                HarcamaBirimIDs = s.Yevmiyeler1003BAyristirmalari.Select(sa => sa.YevmiyeHarcamaBirimID).ToList(),


            }).ToList();



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
                          group new
                          {
                              YevmiyeAlacakToplam = Yevmiyes.Where(p => s.YevmiyeNos.Contains(p.YevmiyeNo) && s.YevmiyeHarcamaBirimIDs.Contains(p.YevmiyeHarcamaBirimID) && YevmiyeAlacakHesapKods.Contains(p.HesapKod)).Sum(sm => sm.Alacak),
                              YevmiyeDvToplam = Yevmiyes.Where(p => s.YevmiyeNos.Contains(p.YevmiyeNo) && s.YevmiyeHarcamaBirimIDs.Contains(p.YevmiyeHarcamaBirimID) && DamgaVergiHesapKodu == p.HesapKod).Sum(sm => sm.Alacak),
                          } by new
                          {

                              s.YBAYevmiyeHarcamaBirimID,
                              s.BirimAdi,
                              s.VergiKimlikNo,
                              s.IsyeriKodu,
                              s.BelgeKodu,
                              s.YevmiyeBelgeKodID,
                              s.SskPrimTutarToplam,
                              s.YevmiyeMatrahToplam,
                              s.SskBorcTutar

                          } into g1

                          select new
                          {
                              g1.Key.YBAYevmiyeHarcamaBirimID,
                              g1.Key.BirimAdi,
                              g1.Key.VergiKimlikNo,
                              g1.Key.IsyeriKodu,
                              g1.Key.BelgeKodu,
                              g1.Key.YevmiyeBelgeKodID,
                              g1.Key.SskPrimTutarToplam,
                              YevmiyeMatrahToplam = g1.Key.YevmiyeMatrahToplam ?? 0,
                              YevmiyeAlacakToplam = g1.Sum(s => s.YevmiyeAlacakToplam),
                              YevmiyeDvToplam = g1.Sum(s => s.YevmiyeDvToplam),
                              g1.Key.SskBorcTutar,
                              KalanSskBorcTutar = g1.Key.SskPrimTutarToplam - g1.Key.SskBorcTutar
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

                               s.YBAYevmiyeHarcamaBirimID,
                               s.BirimAdi,
                               s.VergiKimlikNo,
                               s.IsyeriKodu,
                               s.BelgeKodu,
                               s.YevmiyeBelgeKodID,
                               s.SskPrimTutarToplam,
                               s.YevmiyeMatrahToplam,
                               s.YevmiyeAlacakToplam,
                               s.YevmiyeDvToplam,
                               bk.PrimYuzdesi,
                               s.SskBorcTutar,
                               s.KalanSskBorcTutar

                           } into g1

                           select new
                           {
                               g1.Key.YBAYevmiyeHarcamaBirimID,
                               g1.Key.BirimAdi,
                               g1.Key.VergiKimlikNo,
                               g1.Key.IsyeriKodu,
                               g1.Key.BelgeKodu,
                               g1.Key.YevmiyeBelgeKodID,
                               g1.Key.SskPrimTutarToplam,
                               g1.Key.YevmiyeMatrahToplam,
                               g1.Key.YevmiyeAlacakToplam,
                               YevmiyeDvToplam = g1.Key.YevmiyeDvToplam,

                               MatrahToplam = g1.Key.YevmiyeMatrahToplam - g1.Sum(s => s.GvMatrahi ?? 0),
                               GvKesintiToplam = g1.Key.YevmiyeAlacakToplam - g1.Sum(s => s.GvKesinti ?? 0),
                               DvKesintiToplam = g1.Key.YevmiyeDvToplam - g1.Sum(s => s.DvKesintisi ?? 0),
                               SskTutarToplam = g1.Key.SskPrimTutarToplam - g1.Sum(s => s.UcretIkramiyeToplam) * (g1.Key.PrimYuzdesi / 100),

                               g1.Key.SskBorcTutar,
                               g1.Key.KalanSskBorcTutar
                           }).ToList();

            var qData = qDataLx.AsQueryable();

            if (!model.Sort.IsNullOrWhiteSpace()) qData = qData.OrderBy(model.Sort);
            else qData = qData.OrderBy(o => o.BirimAdi);
            model.Data = qData.Select(s => new FrYevmiye1003BSskPrimleri
            {
                YevmiyeHarcamaBirimID = s.YBAYevmiyeHarcamaBirimID,
                BirimAdi = s.BirimAdi,
                VergiKimlikNo = s.VergiKimlikNo,
                IsyeriKodu = s.IsyeriKodu,
                BelgeKodu = s.BelgeKodu,
                YevmiyeBelgeKodID = s.YevmiyeBelgeKodID,
                SskPrimTutarToplam = s.SskPrimTutarToplam,
                YevmiyeAlacakToplam = s.YevmiyeAlacakToplam,
                YevmiyeDvToplam = s.YevmiyeDvToplam,
                YevmiyeMatrahToplam = s.YevmiyeMatrahToplam,
                MatrahToplam = s.MatrahToplam,
                GvKesintiToplam = s.GvKesintiToplam,
                DvKesintiToplam = s.DvKesintiToplam,
                SskTutarToplam = s.SskTutarToplam,
                SskBorcTutar = s.SskBorcTutar,
                KalanSskBorcTutar = s.KalanSskBorcTutar


            }).ToArray();

            var isyeriGroupData = (from s in model.Data
                                   group new
                                   {
                                       s.YevmiyeHarcamaBirimID,
                                       s.BirimAdi,
                                       s.VergiKimlikNo,
                                       s.IsyeriKodu,
                                       s.SskPrimTutarToplam,
                                       s.YevmiyeAlacakToplam,
                                       s.YevmiyeDvToplam,
                                       s.YevmiyeMatrahToplam,
                                       s.MatrahToplam,
                                       s.GvKesintiToplam,
                                       s.DvKesintiToplam,
                                       s.SskTutarToplam,
                                       s.SskBorcTutar,
                                       s.KalanSskBorcTutar
                                   } by new
                                   {

                                       s.YevmiyeHarcamaBirimID,
                                       s.BirimAdi,
                                       s.VergiKimlikNo,
                                       s.IsyeriKodu,
                                   } into g1

                                   select new FrYevmiye1003BSskPrimleri
                                   {
                                       YevmiyeHarcamaBirimID = g1.Key.YevmiyeHarcamaBirimID,
                                       BirimAdi = g1.Key.BirimAdi,
                                       VergiKimlikNo = g1.Key.VergiKimlikNo,
                                       IsyeriKodu = g1.Key.IsyeriKodu,
                                       SskPrimTutarToplam = g1.Sum(sm => sm.SskPrimTutarToplam),
                                       YevmiyeAlacakToplam = g1.Sum(sm => sm.YevmiyeAlacakToplam),
                                       YevmiyeDvToplam = g1.Sum(sm => sm.YevmiyeDvToplam),
                                       YevmiyeMatrahToplam = g1.Sum(sm => sm.YevmiyeMatrahToplam),
                                       MatrahToplam = g1.Sum(sm => sm.MatrahToplam),
                                       GvKesintiToplam = g1.Sum(sm => sm.GvKesintiToplam),
                                       DvKesintiToplam = g1.Sum(sm => sm.DvKesintiToplam),
                                       SskTutarToplam = g1.Sum(sm => sm.SskTutarToplam),
                                       SskBorcTutar = g1.Sum(sm => sm.SskBorcTutar),
                                       KalanSskBorcTutar = g1.Sum(sm => sm.KalanSskBorcTutar)
                                   }).ToList();
            ViewBag.isyeriGroupData = isyeriGroupData;
            #region export
            if (export && model.Data.Any())
            {
                var gv = new GridView();
                gv.DataSource = model.Data.Select(s => new
                {
                    s.BirimAdi,
                    s.VergiKimlikNo,
                    s.IsyeriKodu,
                    s.BelgeKodu,
                    s.YevmiyeBelgeKodID,
                    s.SskPrimTutarToplam,
                    s.YevmiyeAlacakToplam,
                    s.YevmiyeDvToplam,
                    s.YevmiyeMatrahToplam,
                    s.MatrahToplam,
                    s.GvKesintiToplam,
                    s.DvKesintiToplam,
                    s.SskTutarToplam,
                    s.SskBorcTutar,
                    s.KalanSskBorcTutar
                });
                gv.DataBind();
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);

                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Yevmiye_1003BSskPrimDokumu_" + model.Yil + "_" + model.AyID + ".xls");
            }
            #endregion


            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(false), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(false), "Value", "Caption", model.AyID);
            return View(model);
        }

        public ActionResult TutarEkle(int Yil, int AyID, int YevmiyeHarcamaBirimID, int YevmiyeBelgeKodID)
        {


            var Kayit = db.Yevmiyeler1003BSskPrimBoclari.Where(p => p.Yil == Yil && p.AyID == AyID && p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID && p.YevmiyeBelgeKodID == YevmiyeBelgeKodID).FirstOrDefault();
            if (Kayit == null)
            {
                Kayit = new Yevmiyeler1003BSskPrimBoclari();
                var Ay = db.Aylars.Where(p => p.AyID == AyID).First();
                var HarcamaBirimi = db.YevmiyelerHarcamaBirimleris.Where(p => p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID).First();
                var BelgeKodu = db.YevmiyelerBelgeKodlaris.Where(p => p.YevmiyeBelgeKodID == YevmiyeBelgeKodID).First();
                Kayit.Yil = Yil;
                Kayit.AyID = AyID;
                Kayit.Aylar = Ay;
                Kayit.YevmiyeHarcamaBirimID = YevmiyeHarcamaBirimID;
                Kayit.YevmiyelerHarcamaBirimleri = HarcamaBirimi;
                Kayit.YevmiyeBelgeKodID = YevmiyeBelgeKodID;
                Kayit.YevmiyelerBelgeKodlari = BelgeKodu;
            }
            return View(Kayit);
        }

        [HttpPost]
        public ActionResult TutarEkle(int Yil, int AyID, int YevmiyeHarcamaBirimID, int YevmiyeBelgeKodID, decimal? Tutar)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "SSk Prim Borç Tutar Ekleme İşlemi";
            MmMessage.MessageType = Msgtype.Warning;
            var Kayitlar = db.Yevmiyeler1003BSskPrimBoclari.Where(p => p.Yil == Yil && p.AyID == AyID && p.YevmiyeHarcamaBirimID == YevmiyeHarcamaBirimID && p.YevmiyeBelgeKodID == YevmiyeBelgeKodID).ToList();
            if (Kayitlar.Any())
            {
                db.Yevmiyeler1003BSskPrimBoclari.RemoveRange(Kayitlar);
            }
            if (Tutar != null && Tutar >= 0)
                db.Yevmiyeler1003BSskPrimBoclari.Add(new Yevmiyeler1003BSskPrimBoclari
                {
                    Yil = Yil,
                    AyID = AyID,
                    YevmiyeHarcamaBirimID = YevmiyeHarcamaBirimID,
                    YevmiyeBelgeKodID = YevmiyeBelgeKodID,
                    Tutar = Tutar.Value
                });

            db.SaveChanges();
            MmMessage.Messages.Add("Tutar Bilgileri Kayıt Edildi");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;


            return MmMessage.ToJsonResult();
        }


    }
}