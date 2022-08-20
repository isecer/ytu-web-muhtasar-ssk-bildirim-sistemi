using BiskaUtil;
using Database;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.Yevmiyeler)]
    public class YevmiyelerController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            var fModel = new FmYevmiyeler { PageSize = 50 };
            var BirimID = UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler];
            var Yil = UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler];
            fModel.Expand = Yil.HasValue || BirimID.HasValue;
            fModel.YevmiyeHarcamaBirimID = BirimID;
            fModel.Yil = Yil;
            fModel.BitisTarihi = DateTime.Now.Date;
            return Index(fModel);
        }

        [HttpPost]
        public ActionResult Index(FmYevmiyeler model, bool export = false)
        {

            var BirimIDs = UserIdentity.Current.BirimYetkileri;
            var HesapKodTurYetkis = UserIdentity.Current.YevmiyeHesapKodTurYetkileri;
            UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler] = model.YevmiyeHarcamaBirimID;
            UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler] = model.Yil;

            var q = (from s in db.Yevmiyelers
                     join b in db.YevmiyelerHarcamaBirimleris on s.YevmiyeHarcamaBirimID equals b.YevmiyeHarcamaBirimID
                     join hk in db.YevmiyelerHesapKodlaris on s.HesapKod equals hk.HesapKod into defhk
                     from hk in defhk.DefaultIfEmpty()
                     join sn in db.YevmiyelerSendikaBilgileris on s.HesapKod equals sn.HesapKod into defsn
                     from sn in defsn.DefaultIfEmpty()
                     select new FrYevmiyeler
                     {
                         YevmiyeID = s.YevmiyeID,
                         YevmiyeTarih = s.YevmiyeTarih,
                         YevmiyeNo = s.YevmiyeNo,
                         YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                         BirimAdi = b.BirimAdi,
                         HarcamaBirimAdi = s.HarcamaBirimAdi,
                         VergiKimlikNo = s.VergiKimlikNo,
                         HarcamaBirimKod = s.HarcamaBirimKod,
                         HesapKod = s.HesapKod,
                         HesapAdi = s.HesapAdi,
                         Borc = s.Borc,
                         Alacak = s.Alacak,
                         Aciklama = s.Aciklama,
                         Y1003BYilAyModels = s.Yevmiyeler1003BAyristirmalari.Select(sy => new FrYilAyModel { Yil = sy.Yil, AyID = sy.AyID }).ToList(),
                         KdvTevkifatYilAyModels = s.YevmiyelerKdvTevkifatKayitlaris.Select(sy => new FrYilAyModel { Yil = sy.FaturaYil, AyID = sy.FaturaAyID }).ToList(),
                         IsY1003BVeriGirisiTamamlandi = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B ? s.Yevmiyeler1003BAyristirmalari.Sum(sm => sm.SskPrimTutar) == s.Alacak : (bool?)null,
                         Is1003AHesaplamayaGirecek = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A ? s.Y1003AIsHesaplamayaGirecek : (bool?)null,
                         Is1003AGelirKaydiYapildi = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && hk.IsGelirKaydindaKullanilacak == true ? s.Y1003AVergiKimlikNo != "" : (bool?)null,
                         IsKdvTevkifatVeriGirisiTamamlandi = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari ? s.YevmiyelerKdvTevkifatKayitlaris.Sum(sm => sm.TevkifatTutari) == s.Alacak : (bool?)null,
                         IsEKHarcamaBirimiDegisti = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari ? s.EKYevmiyeHarcamaBirimID.HasValue && s.YevmiyeHarcamaBirimID != s.EKYevmiyeHarcamaBirimID : (bool?)null,
                         IsTifVeriGirisiTamamlandi = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari ? s.YevmiyelerTasinirKontrolTifKaydis.Sum(sm => sm.Tutar) == s.Borc : (bool?)null,
                         IsSendikaBilgisiDegisti = sn != null ? s.YevmiyeSendikaBilgiID.HasValue && s.YevmiyelerSendikaBilgileri.HesapKod != s.HesapKod : (bool?)null,
                         IsBesBilgisiDegisti = hk != null && hk.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari ? s.BESYevmiyeHesapKodID.HasValue && s.BESHesapKod != s.HesapKod : (bool?)null,
                         IsBankaHesapNumarasiGirildi = s.ProjeBankaHesapNoID.HasValue
                     }).AsQueryable();
            if (HesapKodTurYetkis.Count != db.YevmiyelerHesapKodTurleris.Count())
            {
                var IsBankaYetkiVar = HesapKodTurYetkis.Any(a => a == HesapKoduTuru.BankaIslemleriHesapKodlari);
                if (!IsBankaYetkiVar)
                {
                    var IsSendikaYetkiVar = HesapKodTurYetkis.Any(a => a == HesapKoduTuru.SendikaIslemleriHesapKodlari);

                    var HesapKods = new List<string>();
                    if (IsSendikaYetkiVar) HesapKods.AddRange(db.YevmiyelerSendikaBilgileris.Select(s => s.HesapKod).ToList());
                    var EslesenHesapkodlaris = db.YevmiyelerHesapKodlaris.Where(p => HesapKodTurYetkis.Contains(p.YevmiyeHesapKodTurID)).Select(s => new { s.YevmiyeHesapKodTurID, s.HesapKod, s.IsGelirKaydindaKullanilacak }).ToList();
                    HesapKods.AddRange(EslesenHesapkodlaris.Select(s => s.HesapKod));
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
            }

            if (model.Yil.HasValue) q = q.Where(p => p.YevmiyeTarih.Year == model.Yil);
            if (model.BaslangicTarihi.HasValue && model.BitisTarihi.HasValue) q = q.Where(p => p.YevmiyeTarih >= model.BaslangicTarihi && p.YevmiyeTarih <= model.BitisTarihi);
            if (model.YevmiyeHarcamaBirimID.HasValue) q = q.Where(p => p.YevmiyeHarcamaBirimID == model.YevmiyeHarcamaBirimID);
            var SelectName = "";
            if (model.YevmiyeHesapKodTurID.HasValue)
            {
                var HesapTurKods = db.YevmiyelerHesapKodlaris.Where(p => HesapKodTurYetkis.Contains(p.YevmiyeHesapKodTurID)).Select(s => new { s.YevmiyeHesapKodTurID, s.HesapKod, s.IsGelirKaydindaKullanilacak }).ToList();

                if (model.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod) && p.Alacak > 0);
                    if (model.IsY1003BVeriGirisiTamamlandi.HasValue) q = q.Where(p => p.IsY1003BVeriGirisiTamamlandi == model.IsY1003BVeriGirisiTamamlandi);
                    ViewBag.IsY1003BVeriGirisiTamamlandi = new SelectList(Management.CmbVeriGirisiTamamlandiData(true), "Value", "Caption", model.IsY1003BVeriGirisiTamamlandi);
                    SelectName = "IsY1003BVeriGirisiTamamlandi";

                    ViewBag.FYil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.FYil);
                    ViewBag.FAyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.FAyID);

                    if (model.FYil.HasValue) q = q.Where(p => p.Y1003BYilAyModels.Any(a => a.Yil == model.FYil));
                    if (model.FAyID.HasValue) q = q.Where(p => p.Y1003BYilAyModels.Any(a => a.AyID == model.FAyID));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID && p.IsGelirKaydindaKullanilacak == (model.Is1003AGelirKaydiYapildi ?? p.IsGelirKaydindaKullanilacak)).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.Is1003AHesaplamayaGirecek.HasValue) q = q.Where(p => p.Is1003AHesaplamayaGirecek == model.Is1003AHesaplamayaGirecek);
                    ViewBag.Is1003AHesaplamayaGirecek = new SelectList(Management.CmbHesaplamayaGirmeDurumData(true), "Value", "Caption", model.Is1003AHesaplamayaGirecek);
                    SelectName = "Is1003AHesaplamayaGirecek";
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.IsKdvTevkifatVeriGirisiTamamlandi.HasValue) q = q.Where(p => p.IsKdvTevkifatVeriGirisiTamamlandi == model.IsKdvTevkifatVeriGirisiTamamlandi);
                    ViewBag.IsKdvTevkifatVeriGirisiTamamlandi = new SelectList(Management.CmbVeriGirisiTamamlandiData(true), "Value", "Caption", model.IsKdvTevkifatVeriGirisiTamamlandi);
                    SelectName = "IsKdvTevkifatVeriGirisiTamamlandi";

                    ViewBag.FYil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.FYil);
                    ViewBag.FAyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.FAyID);

                    if (model.FYil.HasValue) q = q.Where(p => p.KdvTevkifatYilAyModels.Any(a => a.Yil == model.FYil));
                    if (model.FAyID.HasValue) q = q.Where(p => p.KdvTevkifatYilAyModels.Any(a => a.AyID == model.FAyID));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.IsEKHarcamaBirimiDegisti.HasValue) q = q.Where(p => p.IsEKHarcamaBirimiDegisti == model.IsEKHarcamaBirimiDegisti);
                    ViewBag.IsEKHarcamaBirimiDegisti = new SelectList(Management.CmbHarcamaBirimiDegistiData(true), "Value", "Caption", model.IsEKHarcamaBirimiDegisti);
                    SelectName = "IsEKHarcamaBirimiDegisti";
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.IsTifVeriGirisiTamamlandi.HasValue) q = q.Where(p => p.IsTifVeriGirisiTamamlandi == model.IsTifVeriGirisiTamamlandi);
                    ViewBag.IsTifVeriGirisiTamamlandi = new SelectList(Management.CmbVeriGirisiTamamlandiData(true), "Value", "Caption", model.IsTifVeriGirisiTamamlandi);
                    SelectName = "IsTifVeriGirisiTamamlandi";
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.SendikaIslemleriHesapKodlari)
                {
                    var HesapKods = db.YevmiyelerSendikaBilgileris.Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.IsSendikaBilgisiDegisti.HasValue) q = q.Where(p => p.IsSendikaBilgisiDegisti == model.IsSendikaBilgisiDegisti);
                    ViewBag.IsSendikaBilgisiDegisti = new SelectList(Management.CmbSendikaBilgisiDegistiData(true), "Value", "Caption", model.IsSendikaBilgisiDegisti);
                    SelectName = "IsSendikaBilgisiDegisti";
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                    if (model.IsBesBilgisiDegisti.HasValue) q = q.Where(p => p.IsBesBilgisiDegisti == model.IsBesBilgisiDegisti);
                    ViewBag.IsBesBilgisiDegisti = new SelectList(Management.CmbBesBilgisiDegistiData(true), "Value", "Caption", model.IsBesBilgisiDegisti);
                    SelectName = "IsBesBilgisiDegisti";
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.BankaIslemleriHesapKodlari)
                {
                    if (model.IsBankaHesapNumarasiGirildi.HasValue) q = q.Where(p => p.IsBankaHesapNumarasiGirildi == model.IsBankaHesapNumarasiGirildi);
                    ViewBag.IsBankaHesapNumarasiGirildi = new SelectList(Management.CmbBankaVeriGirisDurumData(true), "Value", "Caption", model.IsBankaHesapNumarasiGirildi);
                    SelectName = "IsBankaHesapNumarasiGirildi";
                }

            }

            if (model.YevmiyeNo.HasValue) q = q.Where(p => p.YevmiyeNo == model.YevmiyeNo);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod.StartsWith(model.HesapKod) || p.HesapAdi.Contains(model.HesapKod));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));

            model.RowCount = q.Count();
            model.BorcToplam = q.Sum(s => (decimal?)s.Borc) ?? 0;
            model.AlacakToplam = q.Sum(s => (decimal?)s.Alacak) ?? 0;
            model.KalanToplam = model.BorcToplam - model.AlacakToplam;
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.YevmiyeNo).ThenBy(t => t.YevmiyeTarih).ThenBy(t => t.BirimAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi() { Toplam = 0, Pasif = 0, Aktif = 0 };
            ViewBag.IndexModel = IndexModel;
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.Yil);
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);
            ViewBag.YevmiyeHesapKodTurID = new SelectList(Management.CmbYevmiyeHesapKodTurleri(true, HesapKodTurYetkis), "Value", "Caption", model.YevmiyeHesapKodTurID);
            ViewBag.SelectName = SelectName;
            return View(model);
        }

        public ActionResult GetExcelYukle(int Yil)
        {


            var model = new YevmiyeVeriGirisPopupExcelModel();
            model.Yil = Yil;
            var page = Management.RenderPartialView("Yevmiyeler", "ShowExcelYukle", model);

            return Json(new
            {
                page = page,
                IsAuthenticated = UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);

        }
        [Authorize(Roles = RoleNames.YevmiyelerKayitYetkisi)]
        public ActionResult ShowExcelYukle(int Yil)
        {
            var model = new YevmiyeVeriGirisPopupExcelModel();
            model.Yil = Yil;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.YevmiyelerKayitYetkisi)]
        public ActionResult ExcelYuklePost(int Yil, HttpPostedFileBase DosyaEki)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Muhtasar ve SSK Bildirimi excel verisi yükleme işlemi";
            mMessage.MessageType = Msgtype.Warning;
            if (RoleNames.YevmiyelerExcelYukleme.InRole())
            {
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
            }
            else
            {
                mMessage.Messages.Add("Excel Yüklemek İçin Yetkili Değilsiniz.");
            }
            if (mMessage.Messages.Count == 0)
            {
                var model = DosyaEki.ToYevmiyeIterateRows(Yil);
                model.Data = model.Data.Where(p => p.YevmiyeNo.HasValue && p.YevmiyeTarih.HasValue).ToList();
                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }
                else
                {
                    var DBMaxYevmiyeNo = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == Yil).Max(m => (int?)m.YevmiyeNo);
                    var DBMinYevmiyeNo = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == Yil).Min(m => (int?)m.YevmiyeNo);
                    var ExcelMinYevmiyeNo = model.Data.Select(s => s.YevmiyeNo).Min();
                    var ExcelMaxYevmiyeNo = model.Data.Select(s => s.YevmiyeNo).Max();
                    if (!DBMinYevmiyeNo.HasValue)
                    {
                        if (ExcelMinYevmiyeNo != 4 && ExcelMinYevmiyeNo != 1)
                        {
                            mMessage.Messages.Add("Yükleyeceğiniz yevmiyelerin yevmiye numarası dört yada bir ile başlamalı.");
                        }
                    }
                    else
                    {
                        if (ExcelMinYevmiyeNo >= 4)
                        {
                            if (ExcelMinYevmiyeNo <= DBMaxYevmiyeNo)
                            {
                                mMessage.Messages.Add("Yükleyeceğiniz yevmiyelerin yevmiye numarası daha önce yüklenen en büyük yevmiye numarasından (" + DBMaxYevmiyeNo + ") büyük olmalı.");
                            }
                            else
                            {
                                var ExcelYevmiyeNos = model.Data.Select(s => s.YevmiyeNo).Distinct().ToList();
                                if (DBMinYevmiyeNo < 4)
                                {
                                    mMessage.Messages.Add("Yüklemek istediğiniz 1,2,3 yevmiye numaraları daha önce yüklenmiştir.");
                                }
                                else
                                {
                                    if (ExcelYevmiyeNos.Where(a => new List<int> { 1, 2, 3 }.Contains(a.Value)).Count() != ExcelYevmiyeNos.Count)
                                    {
                                        mMessage.Messages.Add("Yükleyeceğiniz yevmiye numarası 4 ten daha küçük olacaksa sadece 1,2,3 yevmiye numaraları olması gerekmetkedir.");
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (DBMinYevmiyeNo < 4)
                            {
                                mMessage.Messages.Add("Yüklemek istediğiniz 1,2,3 yevmiye numaraları daha önce yüklenmiştir.");
                            }

                        }
                    }
                    if (((ExcelMaxYevmiyeNo - ExcelMinYevmiyeNo) + 1) != model.Data.Select(s => s.YevmiyeNo).Distinct().Count())
                    {
                        mMessage.Messages.Add("Excel de yevmiye numarası atlaması bulunuyor. Yevmiye numaraları sıra ile gitmeli ve eksik olmamalı.");
                    }

                }

                if (mMessage.Messages.Count == 0)
                {
                    var Birimler = db.YevmiyelerHarcamaBirimleris.Where(p => !p.IsAltBirim).ToList();
                    var Sendikalar = db.YevmiyelerSendikaBilgileris.ToList();
                    var VergiTevkifatHesapKodlari1003As = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A).ToList();
                    var EmekliKesenekHesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari).ToList();
                    var BesHesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari).ToList();

                    try
                    {

                        #region BildirimData 
                        model.Data = (from s in model.Data
                                      join Br in Birimler on s.VergiKimlikNo equals Br.VergiKimlikNo into defB
                                      from Br in defB.DefaultIfEmpty()
                                      join Vh in VergiTevkifatHesapKodlari1003As on s.HesapKod equals Vh.HesapKod into defVh
                                      from Vh in defVh.DefaultIfEmpty()
                                      join Sn in Sendikalar on s.YevmiyeSendikaBilgiID equals Sn.YevmiyeSendikaBilgiID into defSn
                                      from Sn in defSn.DefaultIfEmpty()
                                      join ek in EmekliKesenekHesapKods on s.HesapKod equals ek.HesapKod into defEk
                                      from ek in defEk.DefaultIfEmpty()
                                      join bs in EmekliKesenekHesapKods on s.HesapKod equals bs.HesapKod into defbs
                                      from bs in defbs.DefaultIfEmpty()
                                      select new ExcelDataImportYevmiyeRow
                                      {
                                          SayfaNo = s.SayfaNo,
                                          SatirNo = s.SatirNo,
                                          YevmiyeTarih = s.YevmiyeTarih,
                                          YevmiyeNo = s.YevmiyeNo,
                                          VergiKimlikNo = s.VergiKimlikNo,
                                          YevmiyeHarcamaBirimID = (Br != null ? (int?)Br.YevmiyeHarcamaBirimID : null),
                                          HarcamaBirimAdi = s.HarcamaBirimAdi,
                                          HarcamaBirimKod = s.HarcamaBirimKod,
                                          HesapKod = s.HesapKod,
                                          HesapAdi = s.HesapAdi,
                                          Y1003AHesapKodID = Vh != null ? Vh.YevmiyeHesapKodID : (int?)null,
                                          Y1003AIsHesaplamayaGirecek = Vh != null ? (s.Borc > 0 ? false : true) : false,
                                          EKYevmiyeHarcamaBirimID = ek != null ? s.YevmiyeHarcamaBirimID : (int?)null,
                                          YevmiyeSendikaBilgiID = (Sn != null ? (int?)Sn.YevmiyeSendikaBilgiID : null),
                                          BESYevmiyeHesapKodID = bs != null ? bs.YevmiyeHesapKodID : (int?)null,
                                          Borc = s.Borc,
                                          Alacak = s.Alacak,
                                          Aciklama = s.Aciklama,
                                          IslemYapanID = UserIdentity.Current.Id,
                                          IslemYapanIP = UserIdentity.Ip,
                                          IslemTarihi = DateTime.Now,
                                      }).ToList();
                        var qGrup = model.Data.Where(p=>p.YevmiyeTarih.HasValue && p.YevmiyeNo.HasValue && p.YevmiyeHarcamaBirimID.HasValue).GroupBy(g => new { g.YevmiyeTarih, g.YevmiyeNo, g.VergiKimlikNo, g.YevmiyeHarcamaBirimID, g.HarcamaBirimAdi, g.HarcamaBirimKod, g.HesapKod, g.HesapAdi, g.Borc, g.Alacak, g.Aciklama }).Select(s => new
                        {
                            s.Key.YevmiyeNo,
                            Count = s.Count(),
                            Satirlar = s.Select(s2 => s2.SatirNo).ToList()
                        }).ToList();
                        var GrupMukerrer = qGrup.Where(p => p.Count > 1).ToList();
                        foreach (var item in GrupMukerrer)
                        {
                            mMessage.Messages.Add("Excel Verilerinde Mukerrer Kayıt.Yevmiye No: " + item.YevmiyeNo + " Satırlar:" + string.Join(",", item.Satirlar));
                        }
                        if (!mMessage.Messages.Any())
                        {
                            foreach (var item in model.Data)
                            {
                                List<string> hataTipi = new List<string>();

                                if (!item.YevmiyeTarih.HasValue)
                                {
                                    hataTipi.Add("Yevmiye tarihi boş");
                                    item.HataliHucreler.Add(0);
                                }
                                else if (item.YevmiyeTarih.Value.Year != Yil)
                                {
                                    hataTipi.Add("Yevmiye tarihi yüklenecek yıl ile uyuşmuyor");
                                    item.HataliHucreler.Add(0);
                                }

                                if (!item.YevmiyeNo.HasValue)
                                {
                                    hataTipi.Add("Yevmiye no boş");
                                    item.HataliHucreler.Add(1);
                                }

                                if (item.VergiKimlikNo.IsNullOrWhiteSpace())
                                {
                                    hataTipi.Add("Vergi kimlik numarası boş");
                                    item.HataliHucreler.Add(2);
                                }
                                else if (!item.YevmiyeHarcamaBirimID.HasValue || item.HarcamaBirimAdi.IsNullOrWhiteSpace())
                                {
                                    var msg = "";
                                    if (!item.YevmiyeHarcamaBirimID.HasValue) msg = "Harcama Birimi Vergi kimlik numarası sistemdeki hiçbir harcama Birim ile uyuşmuyor";
                                    if (item.HarcamaBirimAdi.IsNullOrWhiteSpace()) msg += (msg.IsNullOrWhiteSpace() ? "" : ",") + "Harcama birim adı boş";
                                    hataTipi.Add(msg);
                                    item.HataliHucreler.Add(2);
                                }
                                if (item.HarcamaBirimKod.IsNullOrWhiteSpace())
                                {
                                    hataTipi.Add("Harcama Birim Kodu boş");
                                    item.HataliHucreler.Add(3);
                                }
                                if (item.HesapKod.IsNullOrWhiteSpace())
                                {
                                    hataTipi.Add("Hesap Kodu boş");
                                    item.HataliHucreler.Add(4);
                                }
                                if (item.HesapAdi.IsNullOrWhiteSpace())
                                {
                                    hataTipi.Add("Hesap Adı boş");
                                    item.HataliHucreler.Add(5);
                                }
                                if (!(item.Borc > 0) && !(item.Alacak > 0))
                                {
                                    hataTipi.Add("Borç ve Alacak aynı anda boş veya 0 olamaz olamaz");
                                    item.HataliHucreler.Add(6);
                                    item.HataliHucreler.Add(7);
                                }
                                else if ((item.Borc > 0) && (item.Alacak > 0))
                                {
                                    hataTipi.Add("Borç ve Alacak aynı anda dolu olamaz");
                                    item.HataliHucreler.Add(6);
                                    item.HataliHucreler.Add(7);
                                }

                                if (hataTipi.Count > 0)
                                {
                                    var Hatalar = string.Join(", ", hataTipi);
                                    item.HataAciklamasi = Hatalar;
                                }
                            }

                            if (!model.Data.Any(a => a.HataliHucreler.Any()))
                            {
                                model.Data = model.Data.Where(p => !p.HataliHucreler.Any()).ToList();
                                var addYevmiyes = model.Data.Select(s => new Yevmiyeler
                                {
                                    YevmiyeTarih = s.YevmiyeTarih.Value,
                                    YevmiyeNo = s.YevmiyeNo.Value,
                                    VergiKimlikNo = s.VergiKimlikNo,
                                    YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID.Value,
                                    HarcamaBirimAdi = s.HarcamaBirimAdi,
                                    HarcamaBirimKod = s.HarcamaBirimKod,
                                    HesapKod = s.HesapKod,
                                    HesapAdi = s.HesapAdi,
                                    Y1003AHesapKodID = s.Y1003AHesapKodID,
                                    Y1003AIsHesaplamayaGirecek = s.Y1003AIsHesaplamayaGirecek,
                                    EKYevmiyeHarcamaBirimID = s.EKYevmiyeHarcamaBirimID,
                                    YevmiyeSendikaBilgiID = s.YevmiyeSendikaBilgiID,
                                    BESYevmiyeHesapKodID = s.BESYevmiyeHesapKodID,
                                    Borc = s.Borc.Value,
                                    Alacak = s.Alacak.Value,
                                    Aciklama = s.Aciklama,
                                    IslemYapanID = s.IslemYapanID,
                                    IslemYapanIP = s.IslemYapanIP,
                                    IslemTarihi = s.IslemTarihi,
                                }).ToList();
                                db.Yevmiyelers.AddRange(addYevmiyes);
                                db.SaveChanges();
                                mMessage.IsSuccess = true;
                                mMessage.Messages.Add("Yevmiye verileri yükleme işlemi başarılı. Toplam: " + model.Data.Count + " Kalem bilgi sisteme işlendi.");
                                mMessage.MessageType = Msgtype.Success;
                            }
                            else
                            {

                                var excpt = model.YevmiyeAktarilanExcelHataKontrolu();

                                if (excpt == null)
                                {

                                    mMessage.Messages.Add("<span style='color:red;'>Excel dosyasındaki bazı veriler düzgün girilmemiştir. Aşağıdaki dosyayı indirip kontrol ediniz lütfen.</span>");

                                    mMessage.Messages.Add("<a style='color:red;' href='" + model.DosyaYolu + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> " + model.DosyaAdi + "</a>");

                                }
                                else
                                {
                                    var msg = "Excel dosyası düzenlenirken bir hata oluştu! Hata:" + excpt.ToExceptionMessage();
                                    mMessage.Messages.Add(msg);
                                    Management.SistemBilgisiKaydet(msg, "DersIslemleri/FileDataDEOSave", BilgiTipi.Hata);
                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        var msg = "Yevmiye verileri yükleme işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        Management.SistemBilgisiKaydet(msg, "Yevmiyeler/ExcelYuklePost", BilgiTipi.Hata);

                    }
                }


            }
            return mMessage.ToJsonResult();
        }

        public ActionResult MaasExcelYukle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MaasExcelYuklePost(HttpPostedFileBase DosyaEki)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Maaş Excel Dosyasını Dönüştürme işlemi";
            mMessage.MessageType = Msgtype.Warning;

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
                var model = DosyaEki.ToMaasDonusturIterateRows();
                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }



                if (mMessage.Messages.Count == 0)
                {


                    try
                    {

                        model.Data = model.Data.Where(p => p.ColumnValues.Where(p2 => p2.Trim() != "").Count() >= 9).ToList();
                        var IlkSiraNoInx = model.Data.FindIndex(p => p.ColumnValues[0].ToInt() == 1);
                        model.ColumnValues = model.Data.Where((p, inx) => inx < IlkSiraNoInx).SelectMany(s => s.ColumnValues).ToList();
                        var AllRows = new List<ExcelDataImportYevmiyeMaasRow>();
                        var TempRows = new List<ExcelDataImportYevmiyeMaasRow>();
                        int TempSiraNo = 1;
                        var SiraNo = 0;
                        foreach (var item in model.Data.Where((p, inx) => inx >= IlkSiraNoInx && p.ColumnValues[1].Trim() != "" && p.ColumnValues[2].Trim() != ""))
                        {
                            var _SiraNo = item.ColumnValues[0].ToInt();
                            if (_SiraNo.HasValue) SiraNo = _SiraNo.Value;
                            if (SiraNo < TempSiraNo + 1)
                            {
                                TempRows.Add(item);
                            }
                            else
                            {
                                AllRows.Add(new ExcelDataImportYevmiyeMaasRow { SatirNo = TempSiraNo - 1, ColumnValues = TempRows.SelectMany(s => s.ColumnValues).ToList() });

                                TempRows.Clear();
                                TempRows.Add(item);
                                TempSiraNo++;
                            }
                        }
                        if (TempRows.Any()) AllRows.Add(new ExcelDataImportYevmiyeMaasRow { SatirNo = TempSiraNo - 1, ColumnValues = TempRows.SelectMany(s => s.ColumnValues).ToList() });
                        model.Data.Clear();
                        model.Data.AddRange(AllRows);

                        int ColumnNum = 0;
                        var dataTable = new System.Data.DataTable("MaasTable");
                        foreach (var item in model.ColumnValues.Where(p => p.Trim() != ""))
                        {
                            var ColumnName = string.Join("_", item.Split(' ').ToList().Where(p => p != "").Select(s => s).ToList());

                            ColumnNum++;
                            ColumnName += "_" + ColumnNum;
                            dataTable.Columns.Add(ColumnName);
                        }
                        foreach (var itemRow in model.Data)
                        {
                            try
                            {


                                var dr = dataTable.NewRow();
                                int Ic = 0;
                                foreach (var itemC in itemRow.ColumnValues.Where(p => p.Trim() != ""))
                                {
                                    dr[Ic] = itemC;
                                    Ic++;
                                }
                                dataTable.Rows.Add(dr);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        var gv = new GridView();
                        gv.DataSource = dataTable;
                        gv.DataBind();
                        Response.ContentType = "application/ms-excel";
                        Response.ContentEncoding = System.Text.Encoding.UTF8;
                        Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                        StringWriter sw = new StringWriter();
                        HtmlTextWriter htw = new HtmlTextWriter(sw);
                        gv.RenderControl(htw);

                        var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
                        var path = "/TempDocumentFolder/" + unqCode.ReplaceSpecialCharacter() + "_MaasExcel.xls";
                        var SavePath = System.Web.HttpContext.Current.Server.MapPath("~" + path);
                        string renderedGridView = sw.ToString();
                        System.IO.File.WriteAllText(SavePath, renderedGridView);

                        mMessage.Messages.Add("<a style='color:red;' href='" + path + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> Dönüştürülen Dosyayı indir.</a>");

                    }
                    catch (Exception ex)
                    {


                    }
                }


            }
            return mMessage.ToJsonResult();
        }
        public ActionResult GetDetail(int id)
        {
            var YevmiyeKodlaris = db.YevmiyelerHesapKodlaris.ToList();
            var YetkiYevmiyeHesapKodTurID = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).SelectMany(s => s.KullaniciYevmiyeHesapKodTurYetkileris).Select(s => s.YevmiyeHesapKodTurID).ToList();
            var mdl = (from s in db.Yevmiyelers.Where(p => p.YevmiyeID == id)
                       join BHk in db.YevmiyelerHesapKodlaris on s.BESYevmiyeHesapKodID equals BHk.YevmiyeHesapKodID into defBkh
                       from BHk in defBkh.DefaultIfEmpty()
                       join Ekh in db.YevmiyelerHarcamaBirimleris on s.EKYevmiyeHarcamaBirimID equals Ekh.YevmiyeHarcamaBirimID into defEkh
                       from Ekh in defEkh.DefaultIfEmpty()
                       select new YevmiyeDetayModel
                       {
                           YevmiyeID = s.YevmiyeID,
                           YevmiyeTarih = s.YevmiyeTarih,
                           YevmiyeNo = s.YevmiyeNo,
                           VergiKimlikNo = s.VergiKimlikNo,
                           HarcamaBirimKod = s.HarcamaBirimKod,
                           HarcamaBirimAdi = s.HarcamaBirimAdi,
                           HesapKod = s.HesapKod,
                           HesapAdi = s.HesapAdi,
                           Borc = s.Borc,
                           Alacak = s.Alacak,
                           Y1003AHesapKodID = s.Y1003AHesapKodID,
                           Y1003AVergiKodu = s.Y1003AVergiKodu,
                           Y1003AIsHesaplamayaGirecek = s.Y1003AIsHesaplamayaGirecek,
                           Y1003AVergiKimlikNo = s.Y1003AVergiKimlikNo,
                           Y1003AAdSoyad = s.Y1003AAdSoyad,
                           Y1003AAdres = s.Y1003AAdres,
                           Y1003AMatrah = s.Y1003AMatrah,
                           Y1003ABelgeninMahiyeti = s.Y1003ABelgeninMahiyeti,
                           Y1003AFaturaTarihi = s.Y1003AFaturaTarihi,
                           Y1003AFaturaNo = s.Y1003AFaturaNo,
                           Yevmiyeler1003BAyristirmalari = s.Yevmiyeler1003BAyristirmalari,
                           YevmiyelerKdvTevkifatKayitlaris = s.YevmiyelerKdvTevkifatKayitlaris,
                           YevmiyelerTasinirKontrolTifKaydis = s.YevmiyelerTasinirKontrolTifKaydis,
                           ProjeBankaHesapNoID = s.ProjeBankaHesapNoID,
                           YevmiyelerProjeBankaHesapNumaralari = s.YevmiyelerProjeBankaHesapNumaralari,
                           YevmiyeSendikaBilgiID = s.YevmiyeSendikaBilgiID,
                           YevmiyelerSendikaBilgileri = s.YevmiyelerSendikaBilgileri,
                           BESYevmiyeHesapKodID = s.BESYevmiyeHesapKodID,
                           BesHesapKod = BHk.HesapKod,
                           BesHesapKodAdi = BHk.HesapAdi,
                           BESIsYevmiyeDokumuAyri = s.BESIsYevmiyeDokumuAyri,
                           BESIsYevmiyeOdendi = s.BESIsYevmiyeOdendi,
                           EKYevmiyeHarcamaBirimID = s.EKYevmiyeHarcamaBirimID,
                           EKHarcamaBirimAdi = Ekh.BirimAdi,

                       }).First();
            mdl.Is1003BYevmiyeParcalamaOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.SSKPrimHesapKodlari1003B) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B && a.HesapKod == mdl.HesapKod && mdl.Alacak > 0);
            mdl.Is1003AGelirKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.VergiTevkifatHesapKodlari1003A) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && a.HesapKod == mdl.HesapKod);
            mdl.Is1003A10_24GelirKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.VergiTevkifatHesapKodlari1003A) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && a.IsGelirKaydindaKullanilacak == true && a.HesapKod == mdl.HesapKod);
            mdl.IsKdvVergiKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.KDVTevkifatHesapKodlari) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari && a.HesapKod == mdl.HesapKod && mdl.Alacak > 0);
            mdl.IsTasinirKontrolKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.TasinirKontrolHesapKodlari) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari && a.HesapKod == mdl.HesapKod);
            mdl.IsEmekliKesenekKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.EmekliKesintiHesapKodlari) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari && a.HesapKod == mdl.HesapKod);
            mdl.IsSendikaKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.SendikaIslemleriHesapKodlari) && db.YevmiyelerSendikaBilgileris.Any(a => a.HesapKod == mdl.HesapKod);
            mdl.IsBireyselEmeklikKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.BireyselEmeklilikHesapKodlari) && YevmiyeKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari && a.HesapKod == mdl.HesapKod);
            mdl.IsBankaIslemleriKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.BankaIslemleriHesapKodlari);



            mdl.SHesapKodlari1003A = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.VergiTevkifatHesapKodlari1003A), "Value", "Caption", mdl.Y1003AHesapKodID);
            mdl.SBesHesapKod = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.BireyselEmeklilikHesapKodlari), "Value", "Caption", mdl.BESYevmiyeHesapKodID);
            mdl.EKHarcamaBirim = new SelectList(Management.CmbYevmiyelerBirim(), "Value", "Caption", mdl.EKYevmiyeHarcamaBirimID);

            return View(mdl);
        }
        public ActionResult YevmiyeAyristir(int YevmiyeID, int? Yevmiye1003BAyristirmaID = null)
        {
            var model = new Yevmiyeler1003BAyristirmalari();
            if (Yevmiye1003BAyristirmaID > 0)
            {
                model = db.Yevmiyeler1003BAyristirmalari.Where(p => p.YevmiyeID == YevmiyeID && p.Yevmiye1003BAyristirmaID == Yevmiye1003BAyristirmaID).First();
            }
            //var YilYevmiyeToplamAlacak=db.Yevmiyelers.Where(p=>p.)
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbBirimlerUniversiteIsYerleri(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.AyID);
            ViewBag.YevmiyeBelgeKodID = new SelectList(Management.CmbYevmiyeBelgeKodlari(true), "Value", "Caption", model.YevmiyeBelgeKodID);

            return View(model);
        }

        [HttpPost]
        public ActionResult YevmiyeAyristir(Yevmiyeler1003BAyristirmalari kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003B Yevmiye Ayrıştırma İşlemi";
            MmMessage.MessageType = Msgtype.Warning;
            if (kModel.YevmiyeHarcamaBirimID <= 0)
            {
                MmMessage.Messages.Add("İş Yeri Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHarcamaBirimID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeHarcamaBirimID" });
            if (kModel.Yil <= 0)
            {
                MmMessage.Messages.Add("Yıl Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            if (kModel.AyID <= 0)
            {
                MmMessage.Messages.Add("Ay Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AyID" });
            if (kModel.YevmiyeBelgeKodID <= 0)
            {
                MmMessage.Messages.Add("Belge Kodu Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeBelgeKodID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeBelgeKodID" });
            if (kModel.SskPrimTutar <= 0)
            {
                MmMessage.Messages.Add("SSK Prim Tutarı 0'Dan Büyük Olmalı.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "SskPrimTutar" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "SskPrimTutar" });
            if (kModel.Matrah.HasValue)
            {
                if (kModel.Matrah < 0)
                {
                    MmMessage.Messages.Add("Matrah 0'Dan Büyük Olmalı.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Matrah" });
            }

            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.Yevmiyeler1003BAyristirmalari.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.Yil == kModel.Yil && a.AyID == kModel.AyID && a.Yevmiye1003BAyristirmaID != kModel.Yevmiye1003BAyristirmaID);
                if (YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Seçilen Yıl ve Ay İçin Daha Önceden Ayrıştırma İşlemi Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyID" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "Yil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "AyID" });

                    var AyristirmaMatrahToplam = db.Yevmiyeler1003BAyristirmalari.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.Yevmiye1003BAyristirmaID != kModel.Yevmiye1003BAyristirmaID).Sum(s => (decimal?)s.SskPrimTutar);
                    var YevmiyeAlacakToplam = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First().Alacak;
                    AyristirmaMatrahToplam = AyristirmaMatrahToplam + kModel.SskPrimTutar;
                    if (AyristirmaMatrahToplam > YevmiyeAlacakToplam)
                    {
                        MmMessage.Messages.Add("Tüm Ayrıştırma İşlemlerinin SSK Prim Toplamı Yevmiyenin Alacak Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "SskPrimTutar" });
                    }
                }
            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.Yevmiye1003BAyristirmaID <= 0)
                {
                    db.Yevmiyeler1003BAyristirmalari.Add(kModel);
                }
                else
                {
                    var data = db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yevmiye1003BAyristirmaID == kModel.Yevmiye1003BAyristirmaID).First();
                    data.YevmiyeHarcamaBirimID = kModel.YevmiyeHarcamaBirimID;
                    data.Yil = kModel.Yil;
                    data.AyID = kModel.AyID;
                    data.YevmiyeBelgeKodID = kModel.YevmiyeBelgeKodID;
                    data.SskPrimTutar = kModel.SskPrimTutar;
                    data.Matrah = kModel.Matrah;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                MmMessage.Messages.Add("Yevmiye Ayrıştırma İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }
        public ActionResult YevmiyeAyristirSil(int id)
        {
            var kayit = db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yevmiye1003BAyristirmaID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.YevmiyelerHarcamaBirimleri.BirimAdi + "' İsimli Harcama Birimine Ait Yevmiye Ayrıştırma Kaydı Silindi!";
                    db.Yevmiyeler1003BAyristirmalari.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.YevmiyelerHarcamaBirimleri.BirimAdi + "' İsimli Harcama Birimine Ait Yevmiye Ayrıştırma kaydıSilinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeAyristirSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Harcama Birimine Ait Yevmiye Ayrıştırma Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GelirKayit(Yevmiyeler kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003A Gelir Kaydı";
            MmMessage.MessageType = Msgtype.Warning;

            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == kModel.YevmiyeID).First();
            var YevmiyeHesap = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && p.HesapKod == Yevmiye.HesapKod).FirstOrDefault();

            if (YevmiyeHesap == null)
            {
                MmMessage.Messages.Add("Bu yevmiye için Gelir Kaydı işlemi yapılamaz");
            }
            else
            {

                if (YevmiyeHesap.IsGelirKaydindaKullanilacak == true)
                {
                    if (!kModel.Y1003AVergiKimlikNo.IsNullOrWhiteSpace() || !kModel.Y1003AAdSoyad.IsNullOrWhiteSpace() || !kModel.Y1003AAdres.IsNullOrWhiteSpace() || kModel.Y1003AMatrah.HasValue || !kModel.Y1003ABelgeninMahiyeti.IsNullOrWhiteSpace() || !kModel.Y1003AFaturaTarihi.HasValue || !kModel.Y1003AFaturaNo.IsNullOrWhiteSpace())
                    {
                        if (kModel.Y1003AVergiKimlikNo.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Vergi Kimlik Numarası seçiniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AVergiKimlikNo" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AVergiKimlikNo" });
                        if (kModel.Y1003AAdSoyad.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Ad Soyad Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AAdSoyad" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AAdSoyad" });
                        if (kModel.Y1003AAdres.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Adres Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AAdres" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AAdres" });
                        if (!(kModel.Y1003AMatrah > 0))
                        {
                            MmMessage.Messages.Add("Matrah bilgisi 0'Dan büyük olmalıdır.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AMatrah" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AMatrah" });
                        if (kModel.Y1003ABelgeninMahiyeti.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Belge Mahiyeti Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003ABelgeninMahiyeti" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003ABelgeninMahiyeti" });
                        if (!kModel.Y1003AFaturaTarihi.HasValue)
                        {
                            MmMessage.Messages.Add("Fatura Tarihi Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AFaturaTarihi" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AFaturaTarihi" });
                        if (kModel.Y1003AFaturaNo.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Fatura Numarası Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Y1003AFaturaNo" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Y1003AFaturaNo" });

                    }
                }
                if (!MmMessage.Messages.Any())
                {
                    Yevmiye.Y1003AHesapKodID = kModel.Y1003AHesapKodID;
                    Yevmiye.Y1003AIsHesaplamayaGirecek = kModel.Y1003AIsHesaplamayaGirecek;
                    Yevmiye.Y1003AVergiKodu = kModel.Y1003AVergiKodu;
                    Yevmiye.Y1003AVergiKimlikNo = kModel.Y1003AVergiKimlikNo;
                    Yevmiye.Y1003AAdSoyad = kModel.Y1003AAdSoyad;
                    Yevmiye.Y1003AAdres = kModel.Y1003AAdres;
                    Yevmiye.Y1003AMatrah = kModel.Y1003AMatrah;
                    Yevmiye.Y1003ABelgeninMahiyeti = kModel.Y1003ABelgeninMahiyeti;
                    Yevmiye.Y1003AFaturaTarihi = kModel.Y1003AFaturaTarihi;
                    Yevmiye.Y1003AFaturaNo = kModel.Y1003AFaturaNo;
                    Yevmiye.IslemTarihi = DateTime.Now; ;
                    Yevmiye.IslemYapanID = UserIdentity.Current.Id;
                    Yevmiye.IslemYapanIP = UserIdentity.Ip;


                    db.SaveChanges();

                    MmMessage.Messages.Add("Gelir Kaydı İşlemi Yapıldı.");
                    MmMessage.IsSuccess = true;
                    MmMessage.MessageType = Msgtype.Success;

                }
            }

            return MmMessage.ToJsonResult();
        }
        public ActionResult YevmiyeKdvTevkifatKayit(int YevmiyeID, int? YevmiyeKdvTevkifatKayitID = null)
        {
            var model = new YevmiyelerKdvTevkifatKayitlari();
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();
            if (YevmiyeKdvTevkifatKayitID > 0)
            {
                model = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeID == YevmiyeID && p.YevmiyeKdvTevkifatKayitID == YevmiyeKdvTevkifatKayitID).First();
            }
            ViewBag.YeniYevmiyeHesapKodID = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.KDVTevkifatHesapKodlari, true), "Value", "Caption", model.YeniYevmiyeHesapKodID);
            ViewBag.FaturaYil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.FaturaYil);
            ViewBag.FaturaAyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.FaturaAyID);
            ViewBag.YevmiyeKdvKodID = new SelectList(Management.CmbYevmiyeKdvKodlari(Yevmiye.HesapKod, true), "Value", "Caption", model.YevmiyeKdvKodID);
            ViewBag.KdvOrani = new SelectList(Management.CmbKdvOranlari(true), "Value", "Caption", model.KdvOrani);
            ViewBag.DigerYevmiyeKdvKodu = new SelectList(Management.CmbYevmiyeDigerKdvKodlari(true), "Value", "Caption", (model.TevkifatOranBolunen + "/" + model.TevkifatOranBolen));
            ViewBag.DigerKdvKodRwShow = YevmiyeKdvTevkifatKayitID > 0 ? model.YevmiyelerKdvKodlari.IsDigerKdvler : false;
            return View(model);
        }
        public ActionResult YevmiyeKdvKodKontrol(int id)
        {
            var KdvKod = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == id).Select(s => new { s.YevmiyeKdvKodID, s.HesapKod, s.KdvAdi, s.KdvOrani, s.IsDigerKdvler, s.TevkifatOranBolen, s.TevkifatOranBolunen }).First();
            return KdvKod.ToJsonResult();
        }
        public ActionResult YevmiyeKdvHesapKod(int id)
        {
            var YvHk = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == id).First();
            var KdvKodList = Management.CmbYevmiyeKdvKodlari(YvHk.HesapKod, false);
            return KdvKodList.ToJsonResult();
        }
        [HttpPost]
        public ActionResult YevmiyeKdvTevkifatKayit(YevmiyelerKdvTevkifatKayitlari kModel, string DigerYevmiyeKdvKodu = null)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Kdv Tevkifat Kayıt İşlemi";
            MmMessage.MessageType = Msgtype.Warning;


            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik Numarası Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.AdSoyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Soyad Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });

            if (kModel.FaturaYil <= 0)
            {
                MmMessage.Messages.Add("Fatura Yılı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaYil" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaYil" });
            if (kModel.FaturaAyID <= 0)
            {
                MmMessage.Messages.Add("Fatura Ayı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaAyID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaAyID" });


            if (kModel.Matrah <= 0)
            {
                MmMessage.Messages.Add("Matrah Bilgisi Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Matrah" });
            if (kModel.KdvOrani <= 0)
            {
                MmMessage.Messages.Add("Kdv oranı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvOrani" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KdvOrani" });
            if (kModel.YevmiyeKdvKodID <= 0)
            {
                MmMessage.Messages.Add("Kdv Kodu Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeKdvKodID" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeKdvKodID" });
                var KdvKod = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == kModel.YevmiyeKdvKodID).First();
                if (KdvKod.IsDigerKdvler)
                {
                    if (DigerYevmiyeKdvKodu.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Diğer Kdv Oranı Seçiniz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "DigerYevmiyeKdvKodu" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "DigerYevmiyeKdvKodu" });
                }
            }
            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.YevmiyelerKdvTevkifatKayitlaris.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.VergiKimlikNo == kModel.VergiKimlikNo && a.FaturaYil == kModel.FaturaYil && a.FaturaAyID == kModel.FaturaAyID && a.YevmiyeKdvTevkifatKayitID != kModel.YevmiyeKdvTevkifatKayitID);
                if (false && YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Seçilen Vergi Kimlik Numarası için Yıl ve Ay İçin Daha Önceden Ayrıştırma İşlemi Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaYil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaAyID" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "VergiKimlikNo" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "FaturaYil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "FaturaAyID" });
                    var Yevmiye = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First();
                    var KayitliTevkifatToplam = db.YevmiyelerKdvTevkifatKayitlaris.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.YevmiyeKdvTevkifatKayitID != kModel.YevmiyeKdvTevkifatKayitID).Sum(s => (decimal?)s.TevkifatTutari) ?? 0;
                    var YevmiyeAlacakToplam = Yevmiye.Alacak;

                    var YevmiyeKdvKodlari = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == kModel.YevmiyeKdvKodID).First();
                    if (YevmiyeKdvKodlari.IsDigerKdvler)
                    {
                        kModel.TevkifatOranBolen = DigerYevmiyeKdvKodu.Split('/')[1].ToInt().Value;
                        kModel.TevkifatOranBolunen = DigerYevmiyeKdvKodu.Split('/')[0].ToInt().Value;
                    }
                    else
                    {
                        kModel.TevkifatOranBolen = YevmiyeKdvKodlari.TevkifatOranBolen.Value;
                        kModel.TevkifatOranBolunen = YevmiyeKdvKodlari.TevkifatOranBolunen.Value;

                    }

                    kModel.KdvTutari = (kModel.Matrah * kModel.KdvOrani) / 100;
                    kModel.TevkifatTutari = kModel.KdvTutari * ((decimal)kModel.TevkifatOranBolunen / (decimal)kModel.TevkifatOranBolen);

                    KayitliTevkifatToplam = (KayitliTevkifatToplam + kModel.TevkifatTutari).ToString("n2").ToDecimal().Value;

                    if (KayitliTevkifatToplam > YevmiyeAlacakToplam)
                    {
                        MmMessage.Messages.Add("Tüm Kdv Tevkifat Kayıtları (" + KayitliTevkifatToplam + ") Tevkifat Tutarı Toplamı  Yevmiyenin Alacak Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatTutari" });
                    }


                }
            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;

                var KdvKodus = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == kModel.YevmiyeKdvKodID).First();
                kModel.KdvKodu = KdvKodus.KdvKodu;
                kModel.KdvKodOrani = KdvKodus.KdvOrani;
                if (kModel.YevmiyeKdvTevkifatKayitID <= 0)
                {
                    db.YevmiyelerKdvTevkifatKayitlaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeKdvTevkifatKayitID == kModel.YevmiyeKdvTevkifatKayitID).First();
                    data.YeniYevmiyeHesapKodID = kModel.YeniYevmiyeHesapKodID;
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.FaturaYil = kModel.FaturaYil;
                    data.FaturaAyID = kModel.FaturaAyID;
                    data.AdSoyad = kModel.AdSoyad;
                    data.Matrah = kModel.Matrah;
                    data.KdvOrani = kModel.KdvOrani;
                    data.KdvTutari = kModel.KdvTutari;
                    data.YevmiyeKdvKodID = kModel.YevmiyeKdvKodID;
                    data.KdvKodOrani = kModel.KdvKodOrani;
                    data.KdvKodu = kModel.KdvKodu;
                    data.TevkifatOranBolen = kModel.TevkifatOranBolen;
                    data.TevkifatOranBolunen = kModel.TevkifatOranBolunen;
                    data.TevkifatTutari = kModel.TevkifatTutari;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                if (!kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                {
                    var YevmiyeVKN = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo == kModel.VergiKimlikNo).FirstOrDefault();
                    if (YevmiyeVKN == null)
                    {
                        db.YevmiyelerVergiKimlikNumaralaris.Add(new YevmiyelerVergiKimlikNumaralari
                        {
                            VergiKimlikNo = kModel.VergiKimlikNo,
                            AdSoyad = kModel.AdSoyad
                        });
                    }

                }
                db.SaveChanges();
                MmMessage.Messages.Add("Tevkifat Kayıt İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }

        public ActionResult YevmiyeKdvTevkifatKayitSil(int id)
        {
            var kayit = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeKdvTevkifatKayitID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.VergiKimlikNo + "' Vergi Kimlik Numarasına Ait Kdv Tefkifatı Kaydı Silindi!";
                    db.YevmiyelerKdvTevkifatKayitlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.VergiKimlikNo + "' Vergi Kimlik Numarasına Ait Kdv Tefkifatı Kaydı Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeKdvTevkifatKayitSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kdv Tefkifatı Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult YevmiyeEKHarcamaBirimKayit(int YevmiyeID, int? EKYevmiyeHarcamaBirimID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Emekli Kesenek Harcama Birimi Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            if (!MmMessage.Messages.Any())
            {
                Yevmiye.EKYevmiyeHarcamaBirimID = EKYevmiyeHarcamaBirimID;
                db.SaveChanges();
                MmMessage.Messages.Add("Harcama Birimi Güncellendi.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }
        public ActionResult YevmiyeTasinirkontrolTifKayit(int YevmiyeID, int? YevmiyelerTasinirKontrolTifKayitID = null)
        {
            var model = new YevmiyelerTasinirKontrolTifKaydi();
            if (YevmiyelerTasinirKontrolTifKayitID > 0)
            {
                model = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyeID == YevmiyeID && p.YevmiyelerTasinirKontrolTifKayitID == YevmiyelerTasinirKontrolTifKayitID).First();
            }
            //var YilYevmiyeToplamAlacak=db.Yevmiyelers.Where(p=>p.) 

            return View(model);
        }
        [HttpPost]
        public ActionResult YevmiyeTasinirkontrolTifKayit(YevmiyelerTasinirKontrolTifKaydi kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Taşınır Kontrol Kaydı";
            MmMessage.MessageType = Msgtype.Warning;

            if (kModel.TifNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Tif Numarası Giriniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TifNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TifNo" });
            if (kModel.Tutar <= 0)
            {
                MmMessage.Messages.Add("Tutar 0'Dan Büyük Olmalıdır.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tutar" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tutar" });


            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.YevmiyelerTasinirKontrolTifKaydis.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.TifNo == kModel.TifNo && a.YevmiyelerTasinirKontrolTifKayitID != kModel.YevmiyelerTasinirKontrolTifKayitID);
                if (YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Girilen Tif Numarası İçin Daha Önceden Kayıt Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TifNo" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "TifNo" });

                    var AyristirmaMatrahToplam = db.YevmiyelerTasinirKontrolTifKaydis.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.YevmiyelerTasinirKontrolTifKayitID != kModel.YevmiyelerTasinirKontrolTifKayitID).Sum(s => (decimal?)s.Tutar) ?? 0;
                    var YevmiyeBorcToplam = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First().Borc;
                    AyristirmaMatrahToplam = AyristirmaMatrahToplam + kModel.Tutar;
                    if (AyristirmaMatrahToplam > YevmiyeBorcToplam)
                    {
                        MmMessage.Messages.Add("Tüm Tif Girişleri Tutar Toplamı Yevmiyenin Borç Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tutar" });
                    }
                }

            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyelerTasinirKontrolTifKayitID <= 0)
                {
                    db.YevmiyelerTasinirKontrolTifKaydis.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyelerTasinirKontrolTifKayitID == kModel.YevmiyelerTasinirKontrolTifKayitID).First();
                    data.TifNo = kModel.TifNo;
                    data.Tutar = kModel.Tutar;
                    data.Aciklama = kModel.Aciklama;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                MmMessage.Messages.Add("Tif Kaydı İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }

            return MmMessage.ToJsonResult();
        }
        public ActionResult YevmiyeTasinirkontrolTifKayitSil(int id)
        {
            var kayit = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyelerTasinirKontrolTifKayitID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.TifNo + "' Tif Kaydı Silindi!";
                    db.YevmiyelerTasinirKontrolTifKaydis.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.TifNo + "' Tif Kaydı Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeTasinirkontrolTifKayitSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Tif Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult YevmiyeBankaHesapNoKayit(int YevmiyeID, int? ProjeBankaHesapNoID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Hesap Numarası Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            var TumYevmiyeler = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == Yevmiye.YevmiyeTarih.Year && p.YevmiyeNo == Yevmiye.YevmiyeNo).ToList();
            foreach (var item in TumYevmiyeler)
            {
                item.ProjeBankaHesapNoID = ProjeBankaHesapNoID;
            }
            db.SaveChanges();
            MmMessage.Messages.Add("Yevmiye Banka Hesap Numarası Güncellendi.");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;

            return MmMessage.ToJsonResult();
        }

        [HttpPost]
        public ActionResult YevmiyeSendikaBilgisiKayit(int YevmiyeID, int? YevmiyeSendikaBilgiID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Sendika Bilgisi Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            Yevmiye.YevmiyeSendikaBilgiID = YevmiyeSendikaBilgiID;

            db.SaveChanges();
            MmMessage.Messages.Add("Yevmiye Sendika Bilgisi Güncellendi.");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;

            return MmMessage.ToJsonResult();
        }
        [HttpPost]
        public ActionResult BESHesapKodKayit(int YevmiyeID, int? BESYevmiyeHesapKodID, bool? BESIsYevmiyeDokumuAyri, bool? BESIsYevmiyeOdendi)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Bireysel Emeklilik Hesap Kodu Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            if (!MmMessage.Messages.Any())
            {
                Yevmiye.BESYevmiyeHesapKodID = BESYevmiyeHesapKodID;
                Yevmiye.BESIsYevmiyeDokumuAyri = BESIsYevmiyeDokumuAyri;
                Yevmiye.BESIsYevmiyeOdendi = BESIsYevmiyeOdendi;
                db.SaveChanges();
                MmMessage.Messages.Add("Hesap Kodu Güncellendi.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }


        public ActionResult GetVergiKimlikNo(string term)
        {
            var VergiKimlikNos = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo.Contains(term) || p.AdSoyad.Contains(term)).Select(s => new
            {
                id = s.VergiKimlikNo,
                text = s.VergiKimlikNo,
                s.AdSoyad,
                s.VergiKimlikNo,
                s.Adres
            }).OrderBy(o => o.AdSoyad).Take(25).ToList();

            return VergiKimlikNos.ToJsonResult();


        }
        public ActionResult GetBankaHesapNumaralari(string term)
        {
            var HesapNumaralari = db.YevmiyelerProjeBankaHesapNumaralaris.Where(p => p.HesapNo == term || p.HesapAdi.Contains(term) || p.ProjeNo.Contains(term) || p.ProjeAdi.Contains(term)).Select(s => new
            {
                id = s.ProjeBankaHesapNoID,
                text = s.HesapNo + " " + s.HesapAdi + " (" + s.ProjeNo + "- " + s.ProjeAdi + ")",
                s.HesapNo,
                s.HesapAdi,
                s.ProjeNo,
                s.ProjeAdi,
            }).OrderBy(o => o.HesapAdi).Take(25).ToList();

            return HesapNumaralari.ToJsonResult();


        }
        public ActionResult GetSendikaBilgileri(string term)
        {
            var Sendikalar = db.YevmiyelerSendikaBilgileris.Where(p => p.HesapKod.StartsWith(term) || p.VergiKimlikNo.StartsWith(term) || p.AdSoyad.Contains(term) || p.KisaAdi.Contains(term)).Select(s => new
            {
                id = s.YevmiyeSendikaBilgiID,
                text = s.HesapKod + " " + s.AdSoyad,
                s.HesapKod,
                s.VergiKimlikNo,
                s.AdSoyad,
                s.IBanNo,
                s.KisaAdi,
                s.Aciklama
            }).OrderBy(o => o.AdSoyad).Take(25).ToList();

            return Sendikalar.ToJsonResult();


        }
    }
}