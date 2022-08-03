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
    [Authorize(Roles = RoleNames.YevmiyeHesapKoduEslestirme)]
    public class YevmiyeHesapKoduEslestirmeController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyelerHesapKodlari { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerHesapKodlari model)
        {
            var q = from s in db.YevmiyelerHesapKodlaris
                    select s;

            if (model.YevmiyeHesapKodTurID.HasValue) q = q.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod);
            if (!model.HesapAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapAdi.Contains(model.HesapAdi));
            if (model.IsGelirKaydindaKullaniclacak.HasValue) q = q.Where(p => p.IsGelirKaydindaKullaniclacak == model.IsGelirKaydindaKullaniclacak);
            if (!model.VergiKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKodu == model.VergiKodu);


            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.HesapAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerHesapKodlari
            {
                YevmiyeHesapKodID = s.YevmiyeHesapKodID,
                HesapKodTurAdi = s.YevmiyelerHesapKodTurleri.HesapKodTurAdi,
                HesapKod = s.HesapKod,
                HesapAdi = s.HesapAdi,
                VergiKodu = s.VergiKodu,
                IsGelirKaydindaKullaniclacak=s.IsGelirKaydindaKullaniclacak,
                TevkifatOranBolunen=s.TevkifatOranBolunen,
                TevkifatOranBolen=s.TevkifatOranBolen,
                IslemTarihi = s.IslemTarihi,
                IslemYapan = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                IslemYapanID = s.IslemYapanID,
                IslemYapanIP = s.IslemYapanIP
            }).Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            ViewBag.IndexModel = IndexModel;
            ViewBag.YevmiyeHesapKodTurID = new SelectList(Management.CmbYevmiyeHesapKodTurleri(true), "Value", "Caption", model.YevmiyeHesapKodTurID);
            return View(model);
        }

        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new YevmiyelerHesapKodlari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.YevmiyeHesapKodTurID = new SelectList(Management.CmbYevmiyeHesapKodTurleri(true), "Value", "Caption", model.YevmiyeHesapKodTurID);
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerHesapKodlari kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  

            if (kModel.YevmiyeHesapKodTurID <= 0)
            {
                MmMessage.Messages.Add("Hesap Kod Türü Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHesapKodTurID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeHesapKodTurID" });



            if (kModel.HesapKod.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });
            if (kModel.HesapAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapAdi" });

            if (kModel.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A)
            {
                if (!kModel.IsGelirKaydindaKullaniclacak.HasValue)
                {
                    MmMessage.Messages.Add("Gelir Kaydı Yapılacak Mı Sorusunu Cevaplayınız.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IsGelirKaydindaKullaniclacak" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IsGelirKaydindaKullaniclacak" });
                if (kModel.VergiKodu.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Vergi Kodu Boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKodu" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKodu" });

            }
            if (kModel.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari)
            {
                if (!(kModel.TevkifatOranBolunen > 0))
                {
                    MmMessage.Messages.Add("Tevkifat Oranı Bölünen Kısmı Giriniz");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOranBolunen" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOranBolunen" });
                if (!(kModel.TevkifatOranBolen > 0))
                {
                    MmMessage.Messages.Add("Tevkifat Oranı Bölen Kısmı Giriniz");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOranBolen" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOranBolen" });
            }

            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerHesapKodlaris.Any(a => a.HesapKod == kModel.HesapKod && a.YevmiyeHesapKodID != kModel.YevmiyeHesapKodID))
                {
                    MmMessage.Messages.Add("Hesap Kodu daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyeHesapKodTurID != HesapKoduTuru.VergiTevkifatHesapKodlari1003A)
                {
                    kModel.VergiKodu = null;
                    kModel.IsGelirKaydindaKullaniclacak = null;
                }
                if (kModel.YevmiyeHesapKodTurID != HesapKoduTuru.KDVTevkifatHesapKodlari)
                {
                    kModel.TevkifatOranBolen = null;
                    kModel.TevkifatOranBolunen = null;
                }
                if (kModel.YevmiyeHesapKodID <= 0)
                {
                    db.YevmiyelerHesapKodlaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == kModel.YevmiyeHesapKodID).First();
                    data.YevmiyeHesapKodTurID = kModel.YevmiyeHesapKodTurID;
                    data.HesapKod = kModel.HesapKod;
                    data.HesapAdi = kModel.HesapAdi;
                    data.IsGelirKaydindaKullaniclacak = kModel.IsGelirKaydindaKullaniclacak;
                    data.VergiKodu = kModel.VergiKodu;
                    data.TevkifatOranBolen = kModel.TevkifatOranBolen;
                    data.TevkifatOranBolunen = kModel.TevkifatOranBolunen;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }

            ViewBag.YevmiyeHesapKodTurID = new SelectList(Management.CmbYevmiyeHesapKodTurleri(true), "Value", "Caption", kModel.YevmiyeHesapKodTurID);
            ViewBag.MmMessage = MmMessage;
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = kayit.HesapKod + "' Kodlu  Hesap Kodu Eşleştirmesi Silindi!";
                    db.YevmiyelerHesapKodlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = kayit.HesapKod + "' Kodlu  Hesap Kodu Eşleştirmesi Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeHesapKoduEslestirme/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek itenen Hesap Kodu Eşleştirmesi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}