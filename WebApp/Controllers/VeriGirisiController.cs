using BiskaUtil;
using Database;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
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
    [Authorize(Roles = RoleNames.VeriGirisi)]
    public class VeriGirisiController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: VeriGirisi
        public ActionResult Index()
        {

            var BirimID = UserIdentity.Current.SeciliBirimID[RoleNames.VeriGirisi];
            var VASurecID = UserIdentity.Current.SeciliVASurecID[RoleNames.VeriGirisi];
            var AyID = UserIdentity.Current.SeciliAyID.Any(a => a.Key == RoleNames.VeriGirisi) ? UserIdentity.Current.SeciliAyID[RoleNames.VeriGirisi] : DateTime.Now.Month;
            if (!db.VASurecleris.Any(a => a.VASurecID == VASurecID)) VASurecID = db.VASurecleris.OrderByDescending(o => o.Yil).Select(s => s.VASurecID).FirstOrDefault();
            if (!BirimID.HasValue) BirimID = UserIdentity.Current.BirimYetkileri.Any() ? UserIdentity.Current.BirimYetkileri.First() : 0;
            return Index(new FmVeriGiris { PageSize = 15, VASurecID = VASurecID, Expand = VASurecID.HasValue, BirimID = BirimID, AyID = AyID });
        }

        [HttpPost]
        public ActionResult Index(FmVeriGiris model, bool export = false)
        {
            model.AyID = model.AyID ?? 0;
            model.VASurecID = model.VASurecID ?? 0;
            model.BirimID = model.BirimID ?? 0;
            var BirimIDs = UserIdentity.Current.BirimYetkileri;
            UserIdentity.Current.SeciliBirimID[RoleNames.VeriGirisi] = model.BirimID;
            UserIdentity.Current.SeciliVASurecID[RoleNames.VeriGirisi] = model.VASurecID;
            UserIdentity.Current.SeciliAyID[RoleNames.VeriGirisi] = model.AyID;

            var SurecBilgi = Management.GetVASurecKontrol(model.VASurecID.Value, model.AyID);

            model.IsAktif = SurecBilgi.IsAktif && SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir;
            var q = (from s in db.VASurecleriBirims.Where(p => p.VASurecID == model.VASurecID && BirimIDs.Contains(p.BirimID) && p.BirimID == model.BirimID)
                     join vbv in db.VASurecleriBirimVerileris on s.VASurecleriBirimID equals vbv.VASurecleriBirimID
                     join vas in db.VASurecleris on s.VASurecID equals vas.VASurecID
                     join b in db.Vw_BirimlerTree on s.BirimID equals b.BirimID
                     join ay in db.Aylars on vbv.AyID equals ay.AyID
                     join bmt in db.BelgeMahiyetTipleris on vbv.BelgeMahiyetTipID equals bmt.BelgeMahiyetTipID
                     join bt in db.BelgeTurleris on vbv.BelgeTurID equals bt.BelgeTurID
                     join vgt in db.VeriGirisTipleris on vbv.VeriGirisTipID equals vgt.VeriGirisTipID
                     join icn in db.IstenCikisNedenleris on vbv.IstenCikisNedenID equals icn.IstenCikisNedenID into deficn
                     from Icn in deficn.DefaultIfEmpty()
                     join end in db.EksikGunNedenleris on vbv.EksikGunNedenID equals end.EksikGunNedenID into defend
                     from End in defend.DefaultIfEmpty()
                     join mt in db.MeslekTurleris on vbv.MeslekTurID equals mt.MeslekTurID
                     where ay.AyID == model.AyID
                     select new FrVeriGirisi
                     {
                         VASurecID = s.VASurecID,
                         VASurecleriBirimVeriID = vbv.VASurecleriBirimVeriID,
                         VASurecleriBirimID = vbv.VASurecleriBirimID,
                         AyID = vbv.AyID,
                         AyAdi = ay.AyAdi,
                         HizmetDonemYil = vbv.HizmetDonemYil,
                         HizmetDonemAy = vbv.HizmetDonemAy,
                         BelgeMahiyetTipID = vbv.BelgeMahiyetTipID,
                         BelgeMahiyetTipAdi = bmt.BelgeMahiyetTipAdi,
                         BelgeMahiyetTipKodu = vbv.BelgeMahiyetTipKodu,
                         BelgeTurID = vbv.BelgeTurID,
                         BelgeTurAdi = bt.BelgeTurAdi,
                         PrimYuzdesi = bt.PrimYuzdesi,
                         BelgeTurKodu = vbv.BelgeTurKodu,
                         DEsasKanunNo = vbv.DEsasKanunNo,
                         BirimAdi = b.BirimTreeAdi,
                         YeniUniteKodu = vbv.YeniUniteKodu,
                         EskiUniteKodu = vbv.EskiUniteKodu,
                         IsyeriSiraNumarasi = vbv.IsyeriSiraNumarasi,
                         IlKodu = vbv.IlKodu,
                         AltIsverenNumarasi = vbv.AltIsverenNumarasi,
                         VeriGirisTipID = vbv.VeriGirisTipID,
                         VeriGirisTipAdi = vgt.VeriGirisTipAdi,
                         SSKSicilNo = vbv.SSKSicilNo,
                         TcKimlikNo = vbv.TcKimlikNo,
                         Ad = vbv.Ad,
                         Soyad = vbv.Soyad,
                         PrimOdemeGun = vbv.PrimOdemeGun,
                         HakEdilenUcret = vbv.HakEdilenUcret,
                         PrimIkramiyeIstihkak = vbv.PrimIkramiyeIstihkak,
                         IseGirisGun = vbv.IseGirisGun,
                         IseGirisGunStr = vbv.IseGirisGunStr,
                         IseGirisAy = vbv.IseGirisAy,
                         IseGirisAyStr = vbv.IseGirisAyStr,
                         IstenCikisGun = vbv.IstenCikisGun,
                         IstenCikisGunStr = vbv.IstenCikisGunStr,
                         IstenCikisAy = vbv.IstenCikisAy,
                         IstenCikisAyStr = vbv.IstenCikisAyStr,
                         IstenCikisNedenID = vbv.IstenCikisNedenID,
                         IstenCikisNedenKodu = vbv.IstenCikisNedenKodu,
                         IstenCikisNedenAdi = Icn != null ? Icn.IstenCikisNedenAdi : null,
                         EksikGunSayisi = vbv.EksikGunSayisi,
                         EksikGunNedenID = vbv.EksikGunNedenID,
                         EksikGunNedenKodu = vbv.EksikGunNedenKodu,
                         EksikGunNedenAdi = End != null ? End.EksikGunNedenAdi : null,
                         MeslekTurID = vbv.MeslekTurID,
                         MeslekTurKodu = vbv.MeslekTurKodu,
                         MeslekTurAdi = mt.MeslekTurAdi,
                         IstirahatSurecindeCalismamistir = vbv.IstirahatSurecindeCalismamistir,
                         TahakkukNedeni = vbv.TahakkukNedeni,
                         GvDenMuafmi = vbv.GvDenMuafmi,
                         AsgariGecimIndirimi = vbv.AsgariGecimIndirimi,
                         GvMatrahi = vbv.GvMatrahi,
                         GvEngellilikOrani = vbv.GvEngellilikOrani,
                         GvKesinti = vbv.GvKesinti,
                         AsgariGecimIstisnaGvTutar = vbv.AsgariGecimIstisnaGvTutar,
                         AsgariGecimIstisnaDvTutar = vbv.AsgariGecimIstisnaDvTutar,
                         DvKesintisi = vbv.DvKesintisi,
                         HesaplananGv = vbv.HesaplananGv,
                         UzaktanCalismaGun = vbv.UzaktanCalismaGun,

                     }).AsQueryable();
            if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.TcKimlikNo == model.Aranan || (p.Ad + " " + p.Soyad).Contains(model.Aranan));
            if (model.VASurecID.HasValue)
            {
                q = q.Where(p => p.VASurecID == p.VASurecID);
            }
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.Ad).ThenBy(t => t.Soyad);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi() { Toplam = 0, Pasif = 0, Aktif = 0 };
            ViewBag.IndexModel = IndexModel;
            ViewBag.VASurecID = new SelectList(Management.CmbVASurecler(false), "Value", "Caption", model.VASurecID);
            ViewBag.BirimID = new SelectList(Management.CmbYetkiliVASurecBirimlerKullanici(model.VASurecID.Value, false), "Value", "Caption", model.BirimID);
            ViewBag.VeriGirisTipID = new SelectList(Management.CmbVeriGirisTipleri(true), "Value", "Caption", model.VeriGirisTipID);
            ViewBag.AyID = new SelectList(Management.CmbVASurecleriAylar(model.VASurecID.Value), "Value", "Caption", model.AyID);

            if (model.VASurecID.HasValue && model.BirimID.HasValue && model.AyID.HasValue)
            {
                #region ToplamsalVerileri
                var TvModel = new FmVASurecleriBirimVerileriAylikToplam { VASurecID = model.VASurecID.Value, BirimID = model.BirimID.Value, AyID = model.AyID.Value };
                TvModel.IsAktif = model.IsAktif;
                TvModel.Data = db.VASurecleriBirimVerileriAylikToplams.Where(p => p.VASurecleriBirim.BirimID == TvModel.BirimID && p.VASurecleriBirim.VASurecID == TvModel.VASurecID && p.AyID == TvModel.AyID).Select(s => new FrVASurecleriBirimVerileriAylikToplam
                {
                    IsGirilenOrHesaplananToplam = true,
                    VASurecleriBirimVerileriAylikToplamID = s.VASurecleriBirimVerileriAylikToplamID,
                    VASurecleriBirimID = s.VASurecleriBirimID,
                    AyID = s.AyID,
                    AsgariGecimIndirimiToplam = s.AsgariGecimIndirimiToplam,
                    GvKesintiToplam = s.GvKesintiToplam,
                    GvMatrahiToplam = s.GvMatrahiToplam,
                    PrimTutarToplam = s.PrimTutarToplam,
                    IslemTarihi = s.IslemTarihi,

                }).ToList();
                var yuklenenToplam = (from s in db.VASurecleriBirimVerileris
                                      join bt in db.BelgeTurleris on s.BelgeTurID equals bt.BelgeTurID
                                      where s.VASurecleriBirim.BirimID == TvModel.BirimID && s.VASurecleriBirim.VASurecID == TvModel.VASurecID && s.AyID == TvModel.AyID
                                      group new { s } by new { s.BelgeTurID, s.PrimYuzdesi } into g1
                                      select new
                                      {
                                          g1.Key.BelgeTurID,
                                          g1.Key.PrimYuzdesi,
                                          AsgariGecimIndirimi = g1.Sum(p => p.s.AsgariGecimIndirimi ?? 0),
                                          GvMatrahi = g1.Sum(p => p.s.GvMatrahi ?? 0),
                                          GvKesinti = g1.Sum(p => p.s.GvKesinti ?? 0),
                                          PrimTutari = g1.Sum(p => (p.s.HakEdilenUcret + p.s.PrimIkramiyeIstihkak) * (g1.Key.PrimYuzdesi / 100))
                                      }).ToList();
                TvModel.Data.Add(new FrVASurecleriBirimVerileriAylikToplam
                {
                    AsgariGecimIndirimiToplam = yuklenenToplam.Sum(s => s.AsgariGecimIndirimi),
                    GvKesintiToplam = yuklenenToplam.Sum(s => s.GvKesinti),
                    GvMatrahiToplam = yuklenenToplam.Sum(s => s.GvMatrahi),
                    PrimTutarToplam = yuklenenToplam.Sum(s => s.PrimTutari),
                });
                #endregion

                model.ToplamsalVeriHtml = Management.RenderPartialView("VeriGirisi", "GetVASurecleriBirimVerileriAylikToplam", TvModel);
            }
            return View(model);


        }



        public ActionResult DetaySablon(FrSurecIslemleri model)
        {
            return View(model);
        }
        public ActionResult GetExcelYukle(int VASurecID, int AyID, int BirimID)
        {


            var model = new VeriGirisPopupExcelModel();
            model.SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);

            var VaSurecBirim = db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && p.BirimID == BirimID).First();
            model.BirimAdi = db.Vw_BirimlerTree.Where(p => p.BirimID == VaSurecBirim.BirimID).First().BirimTreeAdi;
            model.VASurecleriBirimID = VaSurecBirim.VASurecleriBirimID;
            model.VeriGirisTipAdi = db.VeriGirisTipleris.Where(p => p.VeriGirisTipID == VaSurecBirim.VeriGirisTipID).First().VeriGirisTipAdi;
            var page = Management.RenderPartialView("VeriGirisi", "ShowExcelYukle", model);

            return Json(new
            {
                page = page,
                IsAuthenticated = UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);

        }
        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult ShowExcelYukle(int VASurecID, int AyID, int BirimID)
        {
            var model = new VeriGirisPopupExcelModel();
            model.SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);

            var VaSurecBirim = db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && p.BirimID == BirimID).First();
            model.BirimAdi = db.Vw_BirimlerTree.Where(p => p.BirimID == VaSurecBirim.BirimID).First().BirimTreeAdi;
            model.VASurecleriBirimID = VaSurecBirim.VASurecleriBirimID;
            model.VeriGirisTipAdi = db.VeriGirisTipleris.Where(p => p.VeriGirisTipID == VaSurecBirim.VeriGirisTipID).First().VeriGirisTipAdi;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult ExcelYuklePost(int VASurecID, int AyID, int VASurecleriBirimID, HttpPostedFileBase DosyaEki)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Muhtasar ve SSK Bildirimi excel verisi yükleme işlemi";
            mMessage.MessageType = Msgtype.Warning;
            var SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);




            if (!SurecBilgi.IsAktif || !SurecBilgi.AktifSurec)
            {
                mMessage.Messages.Add(SurecBilgi.SurecYilAdi + " Aktfi değildir veri yükleme işlemi yapılamaz!");
            }
            else if (!SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir)
            {
                mMessage.Messages.Add(SurecBilgi.SecilenAyBilgi.AyAdi + " ayı veri girişi için aktif değildir veri yükleme işlemi yapılamaz!");
            }
            if (DosyaEki == null)
            {
                mMessage.Messages.Add("Excel veri dosyası ekleyiniz.");
            }
            else
            {
                string extension = Path.GetExtension(DosyaEki.FileName);
                if (extension != ".xls" && extension != ".xlsx")
                {
                    mMessage.Messages.Add(DosyaEki.FileName + " doyasının excel formatında olması gerekmektedir. Eki kaldırın ve Excel formatında tekrar ekleyiniz.");
                }
            }

            if (mMessage.Messages.Count == 0)
            {
                var model = DosyaEki.ToIsciIterateRows(VASurecID);




                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }

                if (mMessage.Messages.Count == 0)
                {
                    var BelgeMahiyetTipleris = db.BelgeMahiyetTipleris.Where(p => p.IsAktif).ToList();
                    var BelgeTurleris = db.BelgeTurleris.Where(p => p.IsAktif).ToList();
                    var VASurecleriBirims = db.VASurecleriBirims.Where(p => p.IsAktif && p.VASurecID == VASurecID).ToList();
                    var VeriGirisTipleris = db.VeriGirisTipleris.Where(p => p.IsAktif).ToList();
                    var IstenCikisNedenleris = db.IstenCikisNedenleris.Where(p => p.IsAktif).ToList();
                    var EksikGunNedenleris = db.EksikGunNedenleris.Where(p => p.IsAktif).ToList();
                    var MeslekTurleris = db.MeslekTurleris.Where(p => p.IsAktif).ToList();
                    var Birimler = db.Birimlers.ToList();
                    var VASurecleriBirimBildi = db.VASurecleriBirims.Where(p => p.VASurecleriBirimID == VASurecleriBirimID).First();

                    var qSilineceklerIDs = db.VASurecleriBirimVerileris.Where(p => p.VASurecleriBirimID == VASurecleriBirimID && p.AyID == AyID).Select(s => s.VASurecleriBirimVeriID).ToList();
                    var TumVeriler = db.VASurecleriBirimVerileris.Where(p => !qSilineceklerIDs.Contains(p.VASurecleriBirimVeriID)).OrderByDescending(o => o.VASurecleriBirim.VASurecleri.Yil).ThenByDescending(t => t.AyID).ToList();

                    var TumVerilerBirim = TumVeriler.Where(p => p.VASurecleriBirim.BirimID == VASurecleriBirimBildi.BirimID && !qSilineceklerIDs.Contains(p.VASurecleriBirimVeriID)).OrderByDescending(o => o.VASurecleriBirim.VASurecleri.Yil).ThenByDescending(t => t.AyID).ToList();


                    try
                    {
                        #region BildirimData 
                        model.Data = (from s in model.Data
                                      join bmt in BelgeMahiyetTipleris on s.BelgeMahiyetTipKodu equals bmt.BelgeMahiyetTipKodu into defbmt
                                      from Bmt in defbmt.DefaultIfEmpty()
                                      join bt in BelgeTurleris on s.BelgeTurKodu equals bt.BelgeTurKodu into defbt
                                      from Bt in defbt.DefaultIfEmpty()
                                      join b in VASurecleriBirims on new { VASurecleriBirimID, s.YeniUniteKodu, s.EskiUniteKodu, s.IsyeriSiraNumarasi, s.IlKodu, s.AltIsverenNumarasi, s.VeriGirisTipID } equals
                                                                     new { b.VASurecleriBirimID, b.YeniUniteKodu, b.EskiUniteKodu, b.IsyeriSiraNumarasi, b.IlKodu, b.AltIsverenNumarasi, b.VeriGirisTipID } into defb
                                      from B in defb.DefaultIfEmpty()
                                      join vgt in VeriGirisTipleris on s.VeriGirisTipID equals vgt.VeriGirisTipID into defvgt
                                      from Vgt in defvgt.DefaultIfEmpty()
                                      join icn in IstenCikisNedenleris on s.IstenCikisNedenKodu equals icn.IstenCikisNedenKodu into deficn
                                      from Icn in deficn.DefaultIfEmpty()
                                      join egn in EksikGunNedenleris on s.EksikGunNedenKodu equals egn.EksikGunNedenKodu into defegn
                                      from Egn in defegn.DefaultIfEmpty()
                                      join mtk in MeslekTurleris on s.MeslekTurKodu equals mtk.MeslekTurKodu into defmtk
                                      from Mtk in defmtk.DefaultIfEmpty()
                                      let _BirOncekiDurumVerisi = TumVerilerBirim.Where(p => p.TcKimlikNo == s.TcKimlikNo && (p.IseGirisAy.HasValue || p.IstenCikisAy.HasValue)).OrderBy(o => (o.IseGirisAy.HasValue || o.IstenCikisAy.HasValue) ? 1 : 2).FirstOrDefault()
                                      select new ExcelDataImportRow
                                      {
                                          SayfaNo = s.SayfaNo,
                                          SatirNo = s.SatirNo,
                                          HizmetDonemYil = s.HizmetDonemYil,
                                          HizmetDonemAy = s.HizmetDonemAy,
                                          BelgeMahiyetTipID = Bmt != null ? Bmt.BelgeMahiyetTipID : (int?)null,
                                          BelgeMahiyetTipKodu = s.BelgeMahiyetTipKodu,
                                          BelgeTurID = Bt != null ? Bt.BelgeTurID : (int?)null,
                                          PrimYuzdesi = Bt != null ? Bt.PrimYuzdesi : 0,
                                          BelgeTurKodu = s.BelgeTurKodu,
                                          DEsasKanunNo = s.DEsasKanunNo,
                                          UstBirimID = B != null ? B.UstBirimID : (int?)null,
                                          BirimID = B != null ? B.BirimID : (int?)null,
                                          VASurecleriBirimID = B != null ? B.VASurecleriBirimID : (int?)null,
                                          YeniUniteKodu = s.YeniUniteKodu,
                                          EskiUniteKodu = s.EskiUniteKodu,
                                          IsyeriSiraNumarasi = s.IsyeriSiraNumarasi,
                                          IlKodu = s.IlKodu,
                                          AltIsverenNumarasi = s.AltIsverenNumarasi,
                                          VeriGirisTipID = Vgt != null ? Vgt.VeriGirisTipID : (int?)null,
                                          IsAyAsiriHesaplama = B != null ? B.IsAyAsiriHesaplama : false,
                                          AyBaslangicGun = B != null ? B.AyBaslangicGun : null,
                                          GelecekAyBitisGun = B != null ? B.GelecekAyBitisGun : null,
                                          SSKSicilNo = s.SSKSicilNo,
                                          TcKimlikNo = s.TcKimlikNo,
                                          Ad = s.Ad,
                                          Soyad = s.Soyad,
                                          PrimOdemeGun = s.PrimOdemeGun,
                                          UzaktanCalismaGun = s.UzaktanCalismaGun,
                                          HakEdilenUcret = s.HakEdilenUcret,
                                          PrimIkramiyeIstihkak = s.PrimIkramiyeIstihkak,
                                          IseGirisGun = s.IseGirisGun,
                                          IseGirisAy = s.IseGirisAy,
                                          IseGirisAyStr = s.IseGirisAyStr,
                                          IseGirisGunStr = s.IseGirisGunStr,
                                          IstenCikisGun = s.IstenCikisGun,
                                          IstenCikisAy = s.IstenCikisAy,
                                          IstenCikisGunStr = s.IstenCikisGunStr,
                                          IstenCikisAyStr = s.IstenCikisAyStr,
                                          IstenCikisNedenID = Icn != null ? Icn.IstenCikisNedenID : (int?)null,
                                          IstenCikisNedenKodu = s.IstenCikisNedenKodu,
                                          EksikGunSayisi = s.EksikGunSayisi,
                                          EksikGunNedenID = Egn != null ? Egn.EksikGunNedenID : (int?)null,
                                          EksikGunNedenKodu = s.EksikGunNedenKodu,
                                          MeslekTurID = Mtk != null ? Mtk.MeslekTurID : (int?)null,
                                          MeslekTurKodu = s.MeslekTurKodu,
                                          IstirahatSurecindeCalismamistir = s.IstirahatSurecindeCalismamistir,
                                          TahakkukNedeni = s.TahakkukNedeni,
                                          GvDenMuafmi = s.GvDenMuafmi,
                                          AsgariGecimIndirimi = s.AsgariGecimIndirimi,
                                          GvMatrahi = s.GvMatrahi,
                                          GvEngellilikOrani = s.GvEngellilikOrani,
                                          GvKesinti = s.GvKesinti,
                                          AsgariGecimIstisnaDvTutar = s.AsgariGecimIstisnaDvTutar,
                                          AsgariGecimIstisnaGvTutar = s.AsgariGecimIstisnaGvTutar,
                                          HesaplananGv = s.HesaplananGv,
                                          DvKesintisi = s.DvKesintisi,

                                          IslemYapanID = UserIdentity.Current.Id,
                                          IslemYapanIP = UserIdentity.Ip,
                                          IslemTarihi = DateTime.Now,
                                          BirOncekiDurumVerisi = _BirOncekiDurumVerisi

                                      }).ToList();


                        var BirOncekiAy = new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, 1).AddMonths(-1);
                        var BirOncekiAyKayitlari = TumVerilerBirim.Where(p => p.VASurecleriBirim.VASurecleri.Yil == BirOncekiAy.Year && p.AyID == BirOncekiAy.Month).ToList();
                        var BildirimiYapilmamisKisiler = new List<VASurecleriBirimVerileri>();
                        foreach (var item in model.Data)
                        {
                            var GirilebilirAylar = new List<int>() { SurecBilgi.SecilenAyBilgi.AyID };
                            if (item.IsAyAsiriHesaplama) GirilebilirAylar.Add(new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, 1).AddMonths(1).Month);
                            var AyBaslangicGun = (item.IsAyAsiriHesaplama ? item.AyBaslangicGun.Value : 1);
                            var AyGunHesapSeciliAyGunSayisi = new List<bool>();
                            AyGunHesapSeciliAyGunSayisi.Add(item.PrimOdemeGun.HasValue && item.PrimOdemeGun < 30);
                            AyGunHesapSeciliAyGunSayisi.Add(item.IseGirisGun.HasValue && item.IseGirisGun != AyBaslangicGun);
                            AyGunHesapSeciliAyGunSayisi.Add(item.IstenCikisGun.HasValue);


                            item.AyDakiGunSayisi = AyGunHesapSeciliAyGunSayisi.Any(a => a == true) ? new DateTime(SurecBilgi.Yil, AyID, 1).AddMonths(1).AddDays(-1).Day : 30;
                            var AyBaslangicTarih = item.IsAyAsiriHesaplama ? new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, item.AyBaslangicGun.Value) : new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, 1);
                            var AyBitisTarih = item.IsAyAsiriHesaplama ? new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, item.GelecekAyBitisGun.Value).AddMonths(1) : new DateTime(SurecBilgi.Yil, SurecBilgi.SecilenAyBilgi.AyID, 1).AddMonths(1).AddDays(-1);


                            DateTime iseGirisTarih = item.IseGirisAy.HasValue ? new DateTime(SurecBilgi.Yil, 1, 1).AddMonths(item.IseGirisAy.Value - 1).AddDays(item.IseGirisGun.Value - 1) : DateTime.Now;
                            DateTime istenCikisTarih = item.IstenCikisAy.HasValue ? new DateTime(SurecBilgi.Yil, 1, 1).AddMonths(item.IstenCikisAy.Value - 1).AddDays(item.IstenCikisGun.Value - 1) : DateTime.Now;

                            if (item.IsAyAsiriHesaplama)
                            {
                                if (item.IseGirisAy.HasValue)
                                {
                                    var Yil = SurecBilgi.SecilenAyBilgi.AyID == 12 && item.IseGirisAy == 1 ? SurecBilgi.Yil + 1 : SurecBilgi.Yil;
                                    iseGirisTarih = new DateTime(Yil, 1, 1).AddMonths(item.IseGirisAy.Value - 1).AddDays(item.IseGirisGun.Value - 1);

                                }
                                if (item.IstenCikisAy.HasValue)
                                {
                                    var Yil = SurecBilgi.SecilenAyBilgi.AyID == 12 && item.IstenCikisAy == 1 ? SurecBilgi.Yil + 1 : SurecBilgi.Yil;
                                    istenCikisTarih = new DateTime(Yil, 1, 1).AddMonths(item.IstenCikisAy.Value - 1).AddDays(item.IstenCikisGun.Value - 1);
                                }
                            }
                            List<string> hataTipi = new List<string>();
                            if (item.BelgeMahiyetTipKodu.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Belge mahiyeti boş");
                                item.HataliHucreler.Add(0);
                            }
                            else if (!item.BelgeMahiyetTipID.HasValue)
                            {
                                hataTipi.Add("Belge mahiyeti kodu sistemde bulunamadı");
                                item.HataliHucreler.Add(0);
                            }
                            if (item.BelgeTurKodu.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Belge türü boş");
                                item.HataliHucreler.Add(1);
                            }
                            else if (!item.BelgeTurID.HasValue)
                            {
                                hataTipi.Add("Belge türü kodu sistemde bulunamadı");
                                item.HataliHucreler.Add(1);
                            }
                            if (item.DEsasKanunNo != "00000")
                            {
                                hataTipi.Add("Düzenlemeye esas kanun no: 00000 olmalıdır.");
                                item.HataliHucreler.Add(2);
                            }
                            if (!item.VASurecleriBirimID.HasValue)
                            {
                                hataTipi.Add("İşyeri kod bilgileri hiçbir birim ile eşleşmedi");
                                item.HataliHucreler.AddRange(new List<int> { 3, 4, 5, 6, 7, 8 });
                            }
                            if (!item.VeriGirisTipID.HasValue)
                            {
                                hataTipi.Add("Veri giriş tipi hatalı");
                                item.HataliHucreler.Add(8);
                            }
                            else if (item.VeriGirisTipID != VASurecleriBirimBildi.VeriGirisTipID)
                            {
                                hataTipi.Add("Veri giriş tipi seçilen birime ait değil");
                                item.HataliHucreler.Add(8);
                            }
                            if (item.TcKimlikNo.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Tc kimlik no boş");
                                item.HataliHucreler.Add(10);
                            }
                            else if (item.TcKimlikNo.Length != 11)
                            {
                                hataTipi.Add("Tc kimlik no 11 karakterden oluşmalıdır");
                                item.HataliHucreler.Add(10);
                            }
                            //else if (TumVeriler.Any(a => a.VASurecleriBirim.BirimID != item.BirimID && (a.VASurecleriBirim.UstBirimID == item.UstBirimID)))
                            //{
                            //    var EklenenBirim = TumVeriler.Where(a => a.VASurecleriBirim.BirimID != item.BirimID && (a.VASurecleriBirim.UstBirimID == item.UstBirimID)).First();
                            //    var UBirim = Birimler.Where(p => p.BirimID == EklenenBirim.VASurecleriBirim.UstBirimID).First();
                            //    hataTipi.Add("Tc kimlik no ile '" + UBirim.BirimAdi + " > " + EklenenBirim.VASurecleriBirim.Birimler.BirimAdi + "' iş yerine daha önceden eklendiği için tekrar ekleme işlemi yapılamaz!");
                            //    item.HataliHucreler.Add(11);
                            //}
                            if (item.Ad.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Ad boş");
                                item.HataliHucreler.Add(11);
                            }
                            if (item.Soyad.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Soyad boş");
                                item.HataliHucreler.Add(12);
                            }

                            bool EksikGunHesapYapilabilir = true;
                            if (!item.PrimOdemeGun.HasValue || (item.PrimOdemeGun >= 0 && item.PrimOdemeGun <= 30) == false)
                            {
                                hataTipi.Add("Prim Ödeme Gün 0 ile 30 arasında değer olmalıdır");
                                item.HataliHucreler.Add(13);
                                EksikGunHesapYapilabilir = false;
                            }
                            else
                            {
                                if (item.PrimOdemeGun.HasValue)
                                {
                                    if (item.PrimOdemeGun == 0 && item.HakEdilenUcret != 0)
                                    {
                                        hataTipi.Add("Prim ödeme gün 0 ise hak edilen ücret 0 olmalı");
                                        item.HataliHucreler.Add(15);
                                    }
                                    else if (item.PrimOdemeGun > 0 && item.HakEdilenUcret == 0)
                                    {
                                        hataTipi.Add("Prim ödeme gün 0 dan büyük ise  hak edilen ücret 0 dan büyük olmalı");
                                        item.HataliHucreler.Add(15);
                                    }
                                    //else if (item.PrimOdemeGun == 30)
                                    //{
                                    //    if(item.EksikGunSayisi!=0 && )
                                    //}
                                    if (item.PrimOdemeGun == 0 && item.HakEdilenUcret != 0)
                                    {
                                        hataTipi.Add("Prim ödeme gün 0 ise istikhak 0 olmalı");
                                        item.HataliHucreler.Add(16);
                                    }

                                }
                            }
                            if (!item.UzaktanCalismaGun.HasValue)
                            {
                                hataTipi.Add("Uzaktan çalışma gün bilgisi boş bırakılamaz. En az 0 değeri girilmelidir.");
                                item.HataliHucreler.Add(14);

                            }
                            var IseGirisLst = new List<int?> { item.IseGirisGun, item.IseGirisAy };
                            var CountIG = new List<int> { 0, 2 };

                            if (!CountIG.Contains(IseGirisLst.Count(p => p.HasValue || p > 0 || p <= 31)) || !CountIG.Contains(IseGirisLst.Count(p => !p.HasValue || p <= 0 || p > 31)))
                            {
                                EksikGunHesapYapilabilir = false;
                                if (!item.IseGirisGun.HasValue || item.IseGirisGun <= 0 || item.IseGirisGun > 31)
                                {
                                    hataTipi.Add("İşe giriş gün bilgisi hatalı");
                                    item.HataliHucreler.Add(17);

                                }
                                if (!item.IseGirisAy.HasValue || item.IseGirisAy <= 0 || item.IseGirisAy > 12)
                                {
                                    hataTipi.Add("İşe giriş ay bilgisi hatalı");
                                    item.HataliHucreler.Add(18);
                                }
                            }
                            else
                            {

                                if (item.IseGirisAy.HasValue && !GirilebilirAylar.Contains(item.IseGirisAy.Value))
                                {
                                    hataTipi.Add("İşe giriş ay bilgisi " + string.Join(" ya da ", GirilebilirAylar) + " olabilir");
                                    item.HataliHucreler.Add(18);
                                }

                                else
                                {
                                    if (item.IseGirisGun.HasValue)
                                    {

                                        if (AyBaslangicTarih > iseGirisTarih || AyBitisTarih < iseGirisTarih)
                                        {
                                            hataTipi.Add("İşe giriş bilgisi " + AyBaslangicTarih.ToString("dd-MM-yyyy") + " ile " + AyBitisTarih.ToString("dd-MM-yyyy") + " arasında olmalıdır");
                                            item.HataliHucreler.Add(17);
                                            item.HataliHucreler.Add(18);
                                            EksikGunHesapYapilabilir = false;
                                        }
                                        else if (item.BirOncekiDurumVerisi != null)
                                        {
                                            if (!item.BirOncekiDurumVerisi.IstenCikisAy.HasValue)
                                            {
                                                hataTipi.Add("Kişi " + item.BirOncekiDurumVerisi.HizmetDonemYil + " yılı " + item.BirOncekiDurumVerisi.IseGirisAy + ". ayda işe girişinden sonra çıkışı yapılmamıştır. Tekrar işe başlama yapılamaz.");
                                                item.HataliHucreler.AddRange(new List<int> { 17, 18 });
                                                EksikGunHesapYapilabilir = false;
                                            }
                                        }

                                    }
                                    else
                                    {

                                        if (item.BirOncekiDurumVerisi != null)
                                        {
                                            if (item.BirOncekiDurumVerisi.IstenCikisAy.HasValue)
                                            {
                                                hataTipi.Add("Kişi " + item.BirOncekiDurumVerisi.HizmetDonemYil + " yılı " + item.BirOncekiDurumVerisi.IstenCikisAy + ". ayda işten çıkarıldığı için tekrar işten çıkış yapılamaz.");
                                                item.HataliHucreler.AddRange(new List<int> { 17, 18 });

                                                EksikGunHesapYapilabilir = false;
                                            }
                                        }

                                    }

                                    // önceki dönemlerde  çıkış yapmış olması lazım ya da hiç kayıt olmaması lazım
                                }
                            }
                            var IstenCikisLst = new List<int?> { item.IstenCikisGun, item.IstenCikisAy };
                            var CountIC = new List<int> { 0, 2 };

                            if (!CountIC.Contains(IstenCikisLst.Count(p => p.HasValue || p > 0 || p <= 31)) || !CountIC.Contains(IstenCikisLst.Count(p => !p.HasValue || p <= 0 || p > 31)))
                            {
                                EksikGunHesapYapilabilir = false;
                                if (!item.IstenCikisGun.HasValue || item.IstenCikisGun <= 0 || item.IstenCikisGun > 31)
                                {
                                    hataTipi.Add("İşten çıkış gün bilgisi hatalı");
                                    item.HataliHucreler.Add(19);
                                }
                                if (!item.IstenCikisAy.HasValue || item.IstenCikisAy <= 0 || item.IstenCikisAy > 31)
                                {
                                    hataTipi.Add("İşten çıkış ay bilgisi hatalı");
                                    item.HataliHucreler.Add(20);
                                }
                            }
                            else
                            {

                                if (item.IsAyAsiriHesaplama && item.IstenCikisAy.HasValue && !GirilebilirAylar.Contains(item.IstenCikisAy.Value))
                                {
                                    hataTipi.Add("Ay aşırı hesaplamalarda işten çıkış ay bilgisi (" + string.Join(" ya da ", GirilebilirAylar) + ") olabilir");
                                    item.HataliHucreler.Add(20);
                                    EksikGunHesapYapilabilir = false;
                                }
                                else if (!item.IsAyAsiriHesaplama && item.IstenCikisAy.HasValue && !GirilebilirAylar.Contains(item.IstenCikisAy.Value))
                                {
                                    hataTipi.Add("İşten çıkış ay bilgisi " + string.Join(" ya da ", GirilebilirAylar) + " olabilir");
                                    item.HataliHucreler.Add(20);
                                    EksikGunHesapYapilabilir = false;
                                }
                                else
                                {
                                    if (item.IstenCikisGun.HasValue)
                                    {

                                        if (AyBaslangicTarih > istenCikisTarih || AyBitisTarih < istenCikisTarih)
                                        {
                                            hataTipi.Add("İşten çıkış bilgisi " + AyBaslangicTarih.ToString("dd-MM-yyyy") + " ile " + AyBitisTarih.ToString("dd-MM-yyyy") + " arasında olmalıdır");
                                            item.HataliHucreler.Add(19);
                                            item.HataliHucreler.Add(20);
                                            EksikGunHesapYapilabilir = false;
                                        }
                                        else if (!item.IstenCikisNedenKodu.HasValue)
                                        {
                                            hataTipi.Add("İşten çıkış neden kodu girilmedi");
                                            item.HataliHucreler.Add(21);
                                        }
                                        else if (!item.IstenCikisNedenID.HasValue)
                                        {
                                            hataTipi.Add("İşten çıkış neden bilgisi kodu sistemde bulunamadı");
                                            item.HataliHucreler.Add(21);
                                        }

                                    }
                                    else if (item.IstenCikisNedenKodu.HasValue)
                                    {
                                        hataTipi.Add("İşten çıkış neden kodu boş olmalı");
                                        item.HataliHucreler.Add(21);
                                    }
                                    if (item.BirOncekiDurumVerisi != null)
                                    {
                                        if (!item.IseGirisAy.HasValue && item.IstenCikisAy.HasValue)
                                        {
                                            if (item.BirOncekiDurumVerisi.IstenCikisAy.HasValue)
                                            {
                                                hataTipi.Add("Kişi " + item.BirOncekiDurumVerisi.HizmetDonemYil + " yılı " + item.BirOncekiDurumVerisi.IstenCikisAy + ". ayda işten çıkarıldığı için tekrar işten çıkış yapılamaz.");
                                                item.HataliHucreler.AddRange(new List<int> { 19, 20 });
                                                EksikGunHesapYapilabilir = false;
                                            }
                                        }

                                    }
                                    // önceki dönemlerde  çıkış yapmış olması lazım ya da hiç kayıt olmaması lazım
                                }
                            }
                            var Prim30_GirisCikisKontrol = new Dictionary<string, bool> { };
                            var totalDays = (AyBitisTarih - AyBaslangicTarih).TotalDays + 1;
                            var AySonGunGecerlilik = new List<int> { AyBitisTarih.Day };
                            if (totalDays > 30) AySonGunGecerlilik.Add(AyBitisTarih.Day - 1);
                            if (item.IseGirisGun.HasValue && item.IseGirisGun != AyBaslangicGun) Prim30_GirisCikisKontrol.Add("IseGiris", true);
                            if (item.IstenCikisGun.HasValue && !AySonGunGecerlilik.Contains(item.IstenCikisGun.Value)) Prim30_GirisCikisKontrol.Add("IstenCikis", true);

                            if (item.PrimOdemeGun == 30)
                            {
                                if (item.EksikGunSayisi.HasValue)
                                {
                                    hataTipi.Add("Prim ödeme gün bilgisi 30 iken eksik gün sayısı boş olmalıdır giriniz");
                                    item.HataliHucreler.Add(22);
                                    EksikGunHesapYapilabilir = false;
                                }
                                if (item.EksikGunNedenKodu.HasValue)
                                {
                                    hataTipi.Add("Prim ödeme gün bilgisi 30 iken eksik gün neden kodu boş olmalıdır giriniz");
                                    item.HataliHucreler.Add(23);
                                    EksikGunHesapYapilabilir = false;
                                }
                                if (Prim30_GirisCikisKontrol.Any(a => a.Value == true) && EksikGunHesapYapilabilir)
                                {
                                    if (Prim30_GirisCikisKontrol.Any(a => a.Key == "IseGiris" && a.Value == true))
                                    {
                                        hataTipi.Add("Prim ödeme gün bilgisi 30 iken işe giriş tarihi ay başlangıç terihine eşit olmalıdır");
                                        item.HataliHucreler.Add(18);
                                    }
                                    if (Prim30_GirisCikisKontrol.Any(a => a.Key == "IstenCikis" && a.Value == true))
                                    {
                                        hataTipi.Add("Prim ödeme gün bilgisi 30 iken işten çıkış tarihi ay bitiş tarihine eşit olmalıdır");
                                        item.HataliHucreler.Add(20);
                                    }
                                    EksikGunHesapYapilabilir = false;
                                }

                            }
                            if (EksikGunHesapYapilabilir)
                            {
                                int? EksikGunSayisi = 0;
                                int CalismaGunSayisi = 0;
                                if (item.IseGirisGun.HasValue && item.IstenCikisGun.HasValue) CalismaGunSayisi = ((istenCikisTarih - iseGirisTarih).Days + 1);
                                else if (item.IseGirisGun.HasValue) CalismaGunSayisi = ((AyBitisTarih - iseGirisTarih).Days + 1);
                                else if (item.IstenCikisGun.HasValue) CalismaGunSayisi = item.AyDakiGunSayisi - (AyBitisTarih - istenCikisTarih).Days;
                                else CalismaGunSayisi = item.AyDakiGunSayisi;

                                if (item.PrimOdemeGun == 30 && CalismaGunSayisi > 30) CalismaGunSayisi = 30;

                                EksikGunSayisi = CalismaGunSayisi - item.PrimOdemeGun;

                                if (EksikGunSayisi == 0) EksikGunSayisi = null;

                                if (Prim30_GirisCikisKontrol.Any(a => a.Value == true)) EksikGunSayisi = EksikGunSayisi > 30 ? 30 : EksikGunSayisi;

                                if (EksikGunSayisi != item.EksikGunSayisi)
                                {
                                    hataTipi.Add("Eksik gün sayısı (" + (EksikGunSayisi.HasValue ? EksikGunSayisi.Value.ToString() : "boş") + " olmalıdır)");
                                    item.HataliHucreler.Add(22);
                                }
                                bool EksikGunNedenOlmali = EksikGunSayisi.HasValue;
                                if (item.EksikGunNedenKodu.HasValue != EksikGunNedenOlmali)
                                {
                                    hataTipi.Add("Eksik gün neden kodu " + (EksikGunNedenOlmali ? " giriniz " : "boş olmalı"));
                                    item.HataliHucreler.Add(23);
                                }
                                else if (EksikGunNedenOlmali && !item.EksikGunNedenID.HasValue)
                                {
                                    hataTipi.Add("Eksik gün neden bilgisi kodu sistemde bulunamadı");
                                    item.HataliHucreler.Add(23);
                                }
                            }

                            if (item.MeslekTurKodu.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Meslek kodu boş");
                                item.HataliHucreler.Add(24);
                            }
                            else if (!item.MeslekTurID.HasValue)
                            {
                                hataTipi.Add("Meslek kodu sistemde bulunamadı");
                                item.HataliHucreler.Add(24);
                            }
                            if (!item.IstirahatSurecindeCalismamistir.HasValue)
                            {
                                hataTipi.Add("İstirahat sürelerinde çalışmadı bilgisi boş bırakılamaz");
                                item.HataliHucreler.Add(25);
                            }
                            else if (!new List<int> { 0, 2 }.Contains(item.IstirahatSurecindeCalismamistir.Value))
                            {
                                hataTipi.Add("İstirahat sürelerinde çalışmadı bilgisi 0 yada 2 olabilir");
                                item.HataliHucreler.Add(25);
                            }
                            if (item.TahakkukNedeni != "A")
                            {
                                hataTipi.Add("Tahakkun nedeni A olmalıdır.");
                                item.HataliHucreler.Add(26);
                            }
                            if (item.HizmetDonemAy != AyID.ToIntToStrAy_Gun())
                            {
                                hataTipi.Add("Hizmet dönem ay bilgisi " + AyID.ToIntToStrAy_Gun() + " olmalıdır");
                                item.HataliHucreler.Add(27);
                            }
                            if (item.HizmetDonemYil != SurecBilgi.Yil.ToString())
                            {
                                hataTipi.Add("Hizmet dönem yıl bilgisi " + SurecBilgi.Yil + " olmalıdır");
                                item.HataliHucreler.Add(28);
                            }
                            if (!new List<int?> { 1, 2 }.Contains(item.GvDenMuafmi))
                            {
                                hataTipi.Add("Gelir vergisinden muaf mı bilgisi 1 yada 2 olmalıdır");
                                item.HataliHucreler.Add(29);
                            }
                            if (item.GvMatrahi > 0)
                            {
                                if (((item.DvKesintisi + item.AsgariGecimIstisnaDvTutar) > 0) == false)
                                {
                                    item.HataliHucreler.AddRange(new List<int> { 35, 36, 30 });
                                    hataTipi.Add("Gelir Vergisi Matrahı 0'dan Büyük ise Damga Vergisi Kesintisi ile Asgari Geçim İstisna Damga Vergisi Tuturı Toplamı 0 an Büyük Olmalıdır.");
                                }
                            }
                            if (!item.HesaplananGv.HasValue)
                            {
                                hataTipi.Add("Hesaplanan Gelir Vergisi Bilgisi Boş Bırakılamaz.");
                                item.HataliHucreler.Add(32);
                            }
                            if (!item.AsgariGecimIstisnaGvTutar.HasValue)
                            {
                                hataTipi.Add("Asgari Ücret İstisna Gelir V. Tutarı Bilgisi Boş Bırakılamaz.");
                                item.HataliHucreler.Add(33);
                            }
                            if (!item.GvKesinti.HasValue)
                            {
                                hataTipi.Add("Gelir Vergisi Kesintisi Bilgisi Boş Bırakılamaz.");
                                item.HataliHucreler.Add(34);
                            }
                            else if (item.GvKesinti.HasValue && item.HesaplananGv.HasValue && item.AsgariGecimIstisnaGvTutar.HasValue)
                            {
                                if (item.GvKesinti != (item.HesaplananGv - item.AsgariGecimIstisnaGvTutar))
                                {
                                    hataTipi.Add("Gelir Vergisi = (Hesaplanan Gelir Vergisi - Asgari Geçim İstisna Gelir Vergisi Tutar) olmalıdır. ");
                                    item.HataliHucreler.AddRange(new List<int> { 34, 33, 32 });
                                }
                            }
                            if (!item.AsgariGecimIstisnaDvTutar.HasValue)
                            {
                                hataTipi.Add("Asgari Ücret İstisna Damga V. Tutarı Bilgisi Boş Bırakılamaz.");
                                item.HataliHucreler.Add(35);
                            }
                            if (!item.DvKesintisi.HasValue)
                            {
                                hataTipi.Add("Damga Vergisi Kesintisi Bilgisi Boş Bırakılamaz.");
                                item.HataliHucreler.Add(36);
                            }
                            if (hataTipi.Count > 0)
                            {
                                var Hatalar = string.Join(", ", hataTipi);
                                item.HataAciklamasi = Hatalar;
                            }

                        }
                        var EklenmesiGerekenVeriler = BirOncekiAyKayitlari.Where(p => !p.IstenCikisAy.HasValue && !model.Data.Any(a => a.TcKimlikNo == p.TcKimlikNo)).ToList();
                        if (!model.Data.Any(a => a.HataliHucreler.Any() && a.HataliHucreler.Count < 20) && !EklenmesiGerekenVeriler.Any())
                        {

                            model.Data = model.Data.Where(p => !p.HataliHucreler.Any()).ToList();
                            db.VASurecleriBirimVerileris.RemoveRange(db.VASurecleriBirimVerileris.Where(p => p.VASurecleriBirimID == VASurecleriBirimID && p.AyID == AyID));
                            foreach (var item in model.Data)
                            {
                                db.VASurecleriBirimVerileris.Add(new VASurecleriBirimVerileri
                                {
                                    VASurecleriBirimID = item.VASurecleriBirimID.Value,
                                    AyID = AyID,
                                    HizmetDonemAy = item.HizmetDonemAy,
                                    HizmetDonemYil = item.HizmetDonemYil,
                                    BelgeMahiyetTipID = item.BelgeMahiyetTipID.Value,
                                    BelgeMahiyetTipKodu = item.BelgeMahiyetTipKodu,
                                    BelgeTurID = item.BelgeTurID.Value,
                                    PrimYuzdesi = item.PrimYuzdesi,
                                    BelgeTurKodu = item.BelgeTurKodu,
                                    DEsasKanunNo = item.DEsasKanunNo,
                                    YeniUniteKodu = item.YeniUniteKodu,
                                    EskiUniteKodu = item.EskiUniteKodu,
                                    IsyeriSiraNumarasi = item.IsyeriSiraNumarasi,
                                    IlKodu = item.IlKodu,
                                    AltIsverenNumarasi = item.AltIsverenNumarasi,
                                    VeriGirisTipID = item.VeriGirisTipID.Value,
                                    SSKSicilNo = item.SSKSicilNo,
                                    TcKimlikNo = item.TcKimlikNo,
                                    Ad = item.Ad,
                                    Soyad = item.Soyad,
                                    PrimOdemeGun = item.PrimOdemeGun.Value,
                                    HakEdilenUcret = item.HakEdilenUcret.Value,
                                    PrimIkramiyeIstihkak = item.PrimIkramiyeIstihkak ?? 0,
                                    IseGirisGun = item.IseGirisGun,
                                    IseGirisGunStr = item.IseGirisGunStr,
                                    IseGirisAy = item.IseGirisAy,
                                    IseGirisAyStr = item.IseGirisAyStr,
                                    IstenCikisGun = item.IstenCikisGun,
                                    IstenCikisGunStr = item.IstenCikisGunStr,
                                    IstenCikisAy = item.IstenCikisAy,
                                    IstenCikisAyStr = item.IstenCikisAyStr,
                                    IstenCikisNedenID = item.IstenCikisNedenID,
                                    IstenCikisNedenKodu = item.IstenCikisNedenKodu,
                                    EksikGunSayisi = item.EksikGunSayisi,
                                    EksikGunNedenID = item.EksikGunNedenID,
                                    EksikGunNedenKodu = item.EksikGunNedenKodu,
                                    MeslekTurID = item.MeslekTurID.Value,
                                    MeslekTurKodu = item.MeslekTurKodu,
                                    IstirahatSurecindeCalismamistir = item.IstirahatSurecindeCalismamistir,
                                    TahakkukNedeni = item.TahakkukNedeni,
                                    GvDenMuafmi = item.GvDenMuafmi.Value,
                                    AsgariGecimIndirimi = item.AsgariGecimIndirimi,
                                    GvMatrahi = item.GvMatrahi,
                                    GvEngellilikOrani = item.GvEngellilikOrani,
                                    GvKesinti = item.GvKesinti,
                                    AsgariGecimIstisnaDvTutar = item.AsgariGecimIstisnaDvTutar,
                                    DvKesintisi = item.DvKesintisi,
                                    AsgariGecimIstisnaGvTutar = item.AsgariGecimIstisnaGvTutar,
                                    HesaplananGv = item.HesaplananGv,
                                    UzaktanCalismaGun = item.UzaktanCalismaGun,

                                    IslemTarihi = item.IslemTarihi,
                                    IslemYapanID = item.IslemYapanID,
                                    IslemYapanIP = item.IslemYapanIP
                                });
                            }
                            db.VASurecleriBirimVerileris.RemoveRange(db.VASurecleriBirimVerileris.Where(p => qSilineceklerIDs.Contains(p.VASurecleriBirimVeriID)));
                            db.SaveChanges();
                            mMessage.IsSuccess = true;
                            mMessage.Messages.Add("Muhtasar ve SSK Bildirimi verisi yükleme işlemi başarılı. Toplam: " + model.Data.Count + " Kalem bilgi sisteme işlendi.");
                            mMessage.MessageType = Msgtype.Success;


                        }
                        else
                        {

                            var excpt = model.IsciAktarilanExcelHataKontrolu(EklenmesiGerekenVeriler);



                            if (excpt == null)
                            {

                                mMessage.Messages.Add("<span style='color:red;'>Excel dosyasındaki bazı veriler düzgün girilmemiştir. Aşağıdaki dosyayı indirip kontrol ediniz lütfen.</span>");
                                if (EklenmesiGerekenVeriler.Any())
                                {
                                    mMessage.Messages.Add("<span style='color:red;'>Bazı bildirimlerin bu ay eksik giriş yapıldığı saptandı. Bu veriler aşağıdaki excel dosyasının son satırlarına işlendi.</span>");
                                }
                                mMessage.Messages.Add("<a style='color:red;' href='" + model.DosyaYolu + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> " + model.DosyaAdi + "</a>");
                            }

                            else
                            {
                                mMessage.Messages.Add("<span style='color:red;'>Excel dosyasındaki bazı veriler düzgün girilmemiştir.</span>");
                                var msg = string.Join("<br/>", model.Data.Select(s => (s.SatirNo + ". Satırda hata: " + s.HataAciklamasi)));
                                mMessage.Messages.Add(msg);

                                if (EklenmesiGerekenVeriler.Any())
                                {
                                    mMessage.Messages.Add("<span style='color:red;'>Bazı bildirimlerin bu ay eksik giriş yapıldığı saptandı. Lütfen eksik olan bildirimleri giriniz.</span>");
                                }
                                var msgEx = "Excel dosyası düzenlenirken bir hata oluştu! Hata:" + excpt.ToExceptionMessage();
                                Management.SistemBilgisiKaydet(msgEx, "VeriGirisi/ExcelYuklePost", BilgiTipi.Hata);
                            }


                            //var excpt = model.IsciAktarilanExcelHataKontrolu(EklenmesiGerekenVeriler);

                            //if (excpt == null)
                            //{

                            //    mMessage.Messages.Add("<span style='color:red;'>Excel dosyasındaki bazı veriler düzgün girilmemiştir. Aşağıdaki dosyayı indirip kontrol ediniz lütfen.</span>");
                            //    if (EklenmesiGerekenVeriler.Any())
                            //    {
                            //        mMessage.Messages.Add("<span style='color:red;'>Bazı bildirimlerin bu ay eksik giriş yapıldığı saptandı. Bu veriler aşağıdaki excel dosyasının son satırlarına işlendi.</span>");

                            //    }
                            //    mMessage.Messages.Add("<a style='color:red;' href='" + model.DosyaYolu + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> " + model.DosyaAdi + "</a>");

                            //}
                            //else
                            //{
                            //    var msg = "Excel dosyası düzenlenirken bir hata oluştu! Hata:" + excpt.ToExceptionMessage();
                            //    mMessage.Messages.Add(msg);
                            //    Management.SistemBilgisiKaydet(msg, "VeriGirisi/ExcelYuklePost", BilgiTipi.Hata);
                            //}
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        var msg = "Muhtasar ve SSK Bildirimi verisi yükleme işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        Management.SistemBilgisiKaydet(msg, "VeriGirisi/ExcelYuklePost", BilgiTipi.Hata);

                    }
                }


            }
            return mMessage.ToJsonResult();
        }



        public ActionResult GetVeriGirisi(int VASurecID, int AyID, int BirimID, int? VASurecleriBirimVerileriAylikToplamID)
        {


            var model = new VeriGirisPopupToplamModel();
            model.SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);

            var VaSurecBirim = db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && p.BirimID == BirimID).First();
            model.BirimAdi = db.Vw_BirimlerTree.Where(p => p.BirimID == VaSurecBirim.BirimID).First().BirimTreeAdi;
            model.BirimID = VaSurecBirim.BirimID;
            model.VeriGirisTipAdi = db.VeriGirisTipleris.Where(p => p.VeriGirisTipID == VaSurecBirim.VeriGirisTipID).First().VeriGirisTipAdi;
            model.VASurecleriBirimVerileriAylikToplamID = VASurecleriBirimVerileriAylikToplamID;

            if (VASurecleriBirimVerileriAylikToplamID.HasValue)
            {
                var BVA = db.VASurecleriBirimVerileriAylikToplams.Where(p => p.VASurecleriBirimVerileriAylikToplamID == VASurecleriBirimVerileriAylikToplamID).First();
                model.AsgariGecimIndirimiToplam = BVA.AsgariGecimIndirimiToplam;
                model.GvMatrahiToplam = BVA.GvMatrahiToplam;
                model.GvKesintiToplam = BVA.GvKesintiToplam;
                model.PrimTutarToplam = BVA.PrimTutarToplam;
            }
            var page = Management.RenderPartialView("VeriGirisi", "ShowVeriGirisi", model);

            return Json(new
            {
                page = page,
                IsAuthenticated = UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);

        }
        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult ShowVeriGirisi(int VASurecID, int AyID, int BirimID, int? VASurecleriBirimVerileriAylikToplamID)
        {
            var model = new VeriGirisPopupToplamModel();
            model.SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);

            var VaSurecBirim = db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && p.BirimID == BirimID).First();
            model.BirimAdi = db.Vw_BirimlerTree.Where(p => p.BirimID == VaSurecBirim.BirimID).First().BirimTreeAdi;
            model.BirimID = VaSurecBirim.BirimID;
            model.VeriGirisTipAdi = db.VeriGirisTipleris.Where(p => p.VeriGirisTipID == VaSurecBirim.VeriGirisTipID).First().VeriGirisTipAdi;
            model.VASurecleriBirimVerileriAylikToplamID = VASurecleriBirimVerileriAylikToplamID;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult VeriGirisiPost(int VASurecID, int AyID, int BirimID, int? VASurecleriBirimVerileriAylikToplamID, decimal? AsgariGecimIndirimiToplam, decimal? GvMatrahiToplam, decimal? GvKesintiToplam, decimal? PrimTutarToplam)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Muhtasar ve SSK Bildirimi Toplamsal Veri Girişi İşlemi";
            mMessage.MessageType = Msgtype.Warning;
            var SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);
            var BirimBilgi = db.VASurecleriBirims.Where(p => p.VASurecID == VASurecID && p.BirimID == BirimID).First();



            if (!SurecBilgi.IsAktif || !SurecBilgi.AktifSurec)
            {
                mMessage.Messages.Add(SurecBilgi.SurecYilAdi + " Aktfi değildir veri yükleme işlemi yapılamaz!");
            }
            else if (!SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir)
            {
                mMessage.Messages.Add(SurecBilgi.SecilenAyBilgi.AyAdi + " ayı veri girişi için aktif değildir veri yükleme işlemi yapılamaz!");
            }
            else if (!UserIdentity.Current.BirimYetkileri.Any(a => a == BirimID))
            {
                mMessage.Messages.Add(BirimBilgi.Birimler.BirimAdi + " Birimi için veri giriş işlemi yetkiniz bulunmamaktadır!");
            }
            else
            {
                //if (!AsgariGecimIndirimiToplam.HasValue)
                //{
                //    mMessage.Messages.Add("Agi Toplam Bilgisi Boş Bırakılamaz.");
                //    mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AsgariGecimIndirimiToplam" });
                //}
                //else mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AsgariGecimIndirimiToplam" });
                AsgariGecimIndirimiToplam = 0;
                if (!GvMatrahiToplam.HasValue)
                {
                    mMessage.Messages.Add("G.V Matrah Toplam Bilgisi Boş Bırakılamaz.");
                    mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GvMatrahiToplam" });
                }
                else mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GvMatrahiToplam" });
                if (!GvKesintiToplam.HasValue)
                {
                    mMessage.Messages.Add("G.V Kesinti Toplam Bilgisi Boş Bırakılamaz.");
                    mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GvKesintiToplam" });
                }
                else mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GvKesintiToplam" });
                if (!PrimTutarToplam.HasValue)
                {
                    mMessage.Messages.Add("Prim Tutar Toplam Bilgisi Boş Bırakılamaz.");
                    mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "PrimTutarToplam" });
                }
                else mMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "PrimTutarToplam" });
            }

            if (mMessage.Messages.Count == 0)
            {
                try
                {

                    if (VASurecleriBirimVerileriAylikToplamID.HasValue)
                    {
                        var data = db.VASurecleriBirimVerileriAylikToplams.Where(p => p.VASurecleriBirim.BirimID == BirimID && p.VASurecleriBirim.VASurecID == VASurecID && p.VASurecleriBirimVerileriAylikToplamID == VASurecleriBirimVerileriAylikToplamID.Value).First();
                        data.AsgariGecimIndirimiToplam = AsgariGecimIndirimiToplam.Value;
                        data.GvKesintiToplam = GvKesintiToplam.Value;
                        data.GvMatrahiToplam = GvMatrahiToplam.Value;
                        data.PrimTutarToplam = PrimTutarToplam.Value;
                        data.IslemTarihi = DateTime.Now;
                        data.IslemYapanID = UserIdentity.Current.Id;
                        data.IslemYapanIP = UserIdentity.Ip;
                        db.SaveChanges();

                    }
                    else
                    {
                        db.VASurecleriBirimVerileriAylikToplams.Add(new VASurecleriBirimVerileriAylikToplam
                        {
                            VASurecleriBirimID = BirimBilgi.VASurecleriBirimID,
                            AyID = AyID,
                            AsgariGecimIndirimiToplam = AsgariGecimIndirimiToplam.Value,
                            GvKesintiToplam = GvKesintiToplam.Value,
                            GvMatrahiToplam = GvMatrahiToplam.Value,
                            PrimTutarToplam = PrimTutarToplam.Value,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip
                        });
                        db.SaveChanges();
                    }
                    mMessage.IsSuccess = true;
                    mMessage.Messages.Add("Muhtasar ve SSK Bildirimi toplamsal veri girişi işlemi başarılı.");
                    mMessage.MessageType = Msgtype.Success;

                }
                catch (Exception ex)
                {

                    var msg = "Muhtasar ve SSK Bildirimi toplamsal veri girişi işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                    mMessage.Messages.Add(msg);
                    Management.SistemBilgisiKaydet(msg, "VeriGirisi/VeriGirisiPost", BilgiTipi.Hata);

                }

            }
            string page = "";
            if (mMessage.IsSuccess)
            {
                #region ToplamsalVerileri
                var model = new FmVASurecleriBirimVerileriAylikToplam { VASurecID = VASurecID, BirimID = BirimID, AyID = AyID };
                model.Data = db.VASurecleriBirimVerileriAylikToplams.Where(p => p.VASurecleriBirim.BirimID == BirimID && p.VASurecleriBirim.VASurecID == VASurecID && p.AyID == AyID).Select(s => new FrVASurecleriBirimVerileriAylikToplam
                {
                    IsGirilenOrHesaplananToplam = true,
                    VASurecleriBirimVerileriAylikToplamID = s.VASurecleriBirimVerileriAylikToplamID,
                    VASurecleriBirimID = s.VASurecleriBirimID,
                    AyID = s.AyID,
                    AsgariGecimIndirimiToplam = s.AsgariGecimIndirimiToplam,
                    GvKesintiToplam = s.GvKesintiToplam,
                    GvMatrahiToplam = s.GvMatrahiToplam,
                    PrimTutarToplam = s.PrimTutarToplam,
                    IslemTarihi = s.IslemTarihi,

                }).ToList();
                var yuklenenToplam = (from s in db.VASurecleriBirimVerileris
                                      join bt in db.BelgeTurleris on s.BelgeTurID equals bt.BelgeTurID
                                      where s.VASurecleriBirim.BirimID == BirimID && s.VASurecleriBirim.VASurecID == VASurecID && s.AyID == AyID
                                      group new { s } by new { s.BelgeTurID, s.PrimYuzdesi } into g1
                                      select new
                                      {
                                          g1.Key.BelgeTurID,
                                          g1.Key.PrimYuzdesi,
                                          AsgariGecimIndirimi = g1.Sum(p => p.s.AsgariGecimIndirimi ?? 0),
                                          GvMatrahi = g1.Sum(p => p.s.GvMatrahi ?? 0),
                                          GvKesinti = g1.Sum(p => p.s.GvKesinti ?? 0),
                                          PrimTutari = g1.Sum(p => (p.s.HakEdilenUcret + p.s.PrimIkramiyeIstihkak) * (g1.Key.PrimYuzdesi * (decimal)0.01))
                                      }).ToList();
                model.Data.Add(new FrVASurecleriBirimVerileriAylikToplam
                {
                    AsgariGecimIndirimiToplam = yuklenenToplam.Sum(s => s.AsgariGecimIndirimi),
                    GvKesintiToplam = yuklenenToplam.Sum(s => s.GvKesinti),
                    GvMatrahiToplam = yuklenenToplam.Sum(s => s.GvMatrahi),
                    PrimTutarToplam = yuklenenToplam.Sum(s => s.PrimTutari),
                });
                #endregion
                model.IsAktif = SurecBilgi.IsAktif && SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir;
                page = Management.RenderPartialView("VeriGirisi", "GetVASurecleriBirimVerileriAylikToplam", model);
            }
            return new { mMessage, page }.ToJsonResult();
        }

        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult VGSil(int VASurecID, int AyID, int BirimID, int VASurecleriBirimVerileriAylikToplamID)
        {

            string message = "";
            var SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);
            bool success = false;

            var BirimIDs = UserIdentity.Current.BirimYetkileri;

            if (!SurecBilgi.IsAktif || !SurecBilgi.AktifSurec)
            {
                message = SurecBilgi.SurecYilAdi + " Aktfi değildir veri yükleme işlemi yapılamaz!";

            }
            else if (!SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir)
            {
                message = SurecBilgi.SecilenAyBilgi.AyAdi + " ayı veri girişi için aktif değildir veri yükleme işlemi yapılamaz!";
            }
            else
            {
                var data = db.VASurecleriBirimVerileriAylikToplams.Where(p => BirimIDs.Contains(p.VASurecleriBirim.BirimID) && p.VASurecleriBirim.VASurecID == VASurecID && p.AyID == AyID && p.VASurecleriBirim.BirimID == BirimID && p.VASurecleriBirimVerileriAylikToplamID == VASurecleriBirimVerileriAylikToplamID).FirstOrDefault();
                if (data != null)
                {
                    try
                    {
                        message = "'" + data.IslemTarihi.ToFormatDateAndTime() + "' tarihli toplamsal veri girişi sistemden silindi!";
                        db.VASurecleriBirimVerileriAylikToplams.Remove(data);
                        db.SaveChanges();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        message = "'" + data.IslemTarihi.ToFormatDateAndTime() + "' tarihli toplamsal veri girişi silinirken bir hata oluştu! <br/> Bilgi:" + ex.ToExceptionMessage();
                        Management.SistemBilgisiKaydet(message, "VeriGirisi/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    }
                }
                else
                {
                    success = false;
                    message = "Seçilen toplamsal veri girişi bilgisi sistemde bulunamadı.!";
                }
            }



            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetVASurecleriBirimVerileriAylikToplam()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.VeriGirisiKayitYetkisi)]
        public ActionResult Sil(int VASurecID, int AyID, int BirimID)
        {

            string message = "";
            var SurecBilgi = Management.GetVASurecKontrol(VASurecID, AyID);
            bool success = false;

            var BirimIDs = UserIdentity.Current.BirimYetkileri;

            if (!SurecBilgi.IsAktif || !SurecBilgi.AktifSurec)
            {
                message = SurecBilgi.SurecYilAdi + " Aktfi değildir veri yükleme işlemi yapılamaz!";

            }
            else if (!SurecBilgi.SecilenAyBilgi.IsVeriGirilebilir)
            {
                message = SurecBilgi.SecilenAyBilgi.AyAdi + " ayı veri girişi için aktif değildir veri yükleme işlemi yapılamaz!";
            }
            else
            {
                var data = db.VASurecleriBirimVerileris.Where(p => BirimIDs.Contains(p.VASurecleriBirim.BirimID) && p.VASurecleriBirim.VASurecID == VASurecID && p.AyID == AyID && p.VASurecleriBirim.BirimID == BirimID).ToList();
                if (data.Any())
                {
                    try
                    {
                        message = "'" + data.Count + "' adet veri sistemden silindi!";
                        db.VASurecleriBirimVerileris.RemoveRange(data);
                        db.SaveChanges();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        message = "Seçilen Süreç , Ay ve Birime ait veriler silinirken bir hata oluştu! <br/> Bilgi:" + ex.ToExceptionMessage();
                        Management.SistemBilgisiKaydet(message, "VeriGirisi/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    }
                }
                else
                {
                    success = false;
                    message = "Seçilen Süreç , Ay ve Birime ait silinmek istenen herhangi bir veri bulunamadı.!";
                }
            }



            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }






    }
}