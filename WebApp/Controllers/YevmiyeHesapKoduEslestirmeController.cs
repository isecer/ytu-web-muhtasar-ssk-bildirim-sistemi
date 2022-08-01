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
            return Index(new FmYevmiyelerEslestirme { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerEslestirme model)
        { 
            var q = from s in db.YevmiyelerEslestirmes
                    select s;

            if (model.YevmiyeEslestirmeID.HasValue) q = q.Where(p => p.YevmiyeEslestirmeID == model.YevmiyeEslestirmeID);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod);
            if (!model.HesapAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapAdi.Contains(model.HesapAdi));
            if (!model.VergiKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKodu == model.VergiKodu);
            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.AdSoyad == model.AdSoyad);
            if (!model.IBanNo.IsNullOrWhiteSpace()) q = q.Where(p => p.IBanNo == model.IBanNo);
            if (!model.KisaAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.KisaAdi.Contains(model.KisaAdi));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));

            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.HesapAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerEslestirme
            {
                YevmiyeEslestirmeID = s.YevmiyeEslestirmeID,
                EslestirmeTurAdi = s.YevmiyelerEslestirmeTurleri.EslestirmeTurAdi,
                HesapKod = s.HesapKod,
                HesapAdi = s.HesapAdi,
                VergiKodu = s.VergiKodu,
                VergiKimlikNo = s.VergiKimlikNo,
                AdSoyad = s.AdSoyad,
                IBanNo = s.IBanNo,
                KisaAdi = s.KisaAdi,
                Aciklama = s.Aciklama,
                IslemTarihi = s.IslemTarihi,
                IslemYapan = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                IslemYapanID = s.IslemYapanID,
                IslemYapanIP = s.IslemYapanIP
            }).Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            ViewBag.IndexModel = IndexModel;
            return View(model);
        }

        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new YevmiyelerEslestirme();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerEslestirmes.Where(p => p.YevmiyeEslestirmeID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerEslestirme kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  

            if (kModel.YevmiyeEslestirmeTurID <= 0)
            {
                MmMessage.Messages.Add("Eşleştirme Türü Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeEslestirmeTurID" });
            }
            else
            {

                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeEslestirmeTurID" });
                if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.Ssk1003B)
                {
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
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.Vergi1003A)
                {
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
                    if (kModel.VergiKodu.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Vergi Kodu Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKodu" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKodu" });
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.KdvVergi)
                {
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
                    if (!kModel.TevkifatOraniBolunen.HasValue || kModel.TevkifatOraniBolunen <= 0)
                    {
                        MmMessage.Messages.Add("Bölünen Kısmı 0 dan büyük bir değer olmalıdır.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOraniBolunen" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOraniBolunen" });
                    if (!kModel.TevkifatOraniBolen.HasValue || kModel.TevkifatOraniBolen <= 0)
                    {
                        MmMessage.Messages.Add("Bölen Kısmı 0 dan büyük bir değer olmalıdır.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOraniBolen" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOraniBolen" });
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.EmekliKesenek)
                {
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
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.TasinirKontrol)
                {
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
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.SendikaIslemleri)
                {

                    if (kModel.HesapKod.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Hesap Kodu Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });
                    if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Vergi Kimlik Numrası Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });

                    if (kModel.AdSoyad.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Ad Soyad Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });

                    if (kModel.IBanNo.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("IBan Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IBanNo" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IBanNo" });
                    if (kModel.KisaAdi.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("IBan Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KisaAdi" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KisaAdi" });
                    if (kModel.Aciklama.IsNullOrWhiteSpace())
                    {
                        MmMessage.Messages.Add("Açıklama Boş bırakılamaz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Aciklama" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });
                }
            }
            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.Ssk1003B)
                {
                    kModel.VergiKodu = null;
                    kModel.TevkifatOraniBolen = null;
                    kModel.TevkifatOraniBolunen = null;
                    kModel.VergiKimlikNo = null;
                    kModel.AdSoyad = null;
                    kModel.IBanNo = null;
                    kModel.KisaAdi = null;
                    kModel.Aciklama = null;
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.Vergi1003A)
                {
                    kModel.TevkifatOraniBolen = null;
                    kModel.TevkifatOraniBolunen = null;
                    kModel.VergiKimlikNo = null;
                    kModel.AdSoyad = null;
                    kModel.IBanNo = null;
                    kModel.KisaAdi = null;
                    kModel.Aciklama = null;
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.KdvVergi)
                {
                    kModel.VergiKodu = null;
                    kModel.VergiKimlikNo = null;
                    kModel.AdSoyad = null;
                    kModel.IBanNo = null;
                    kModel.KisaAdi = null;
                    kModel.Aciklama = null;

                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.EmekliKesenek)
                {
                    kModel.VergiKodu = null;
                    kModel.TevkifatOraniBolen = null;
                    kModel.TevkifatOraniBolunen = null;
                    kModel.VergiKimlikNo = null;
                    kModel.AdSoyad = null;
                    kModel.IBanNo = null;
                    kModel.KisaAdi = null;
                    kModel.Aciklama = null;

                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.TasinirKontrol)
                {
                    kModel.VergiKodu = null;
                    kModel.TevkifatOraniBolen = null;
                    kModel.TevkifatOraniBolunen = null;
                    kModel.VergiKimlikNo = null;
                    kModel.AdSoyad = null;
                    kModel.IBanNo = null;
                    kModel.KisaAdi = null;
                    kModel.Aciklama = null;
                }
                else if (kModel.YevmiyeEslestirmeTurID == YevmiyeEslestirmeTuru.SendikaIslemleri)
                {
                    kModel.HesapAdi = null;
                    kModel.TevkifatOraniBolen = null;
                    kModel.TevkifatOraniBolunen = null;

                }

                if (kModel.YevmiyeEslestirmeID <= 0)
                {
                    db.YevmiyelerEslestirmes.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerEslestirmes.Where(p => p.YevmiyeEslestirmeID == kModel.YevmiyeEslestirmeID).First();
                    data.HesapKod = kModel.HesapKod;
                    data.HesapAdi = kModel.HesapAdi;
                    data.VergiKodu = kModel.VergiKodu;
                    data.TevkifatOraniBolunen = kModel.TevkifatOraniBolunen;
                    data.TevkifatOraniBolen = kModel.TevkifatOraniBolen;
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.AdSoyad = kModel.AdSoyad;
                    data.IBanNo = kModel.IBanNo;
                    data.KisaAdi = kModel.KisaAdi;
                    data.Aciklama = kModel.Aciklama;
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

            ViewBag.MmMessage = MmMessage;
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.YevmiyelerEslestirmes.Where(p => p.YevmiyeEslestirmeID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.YevmiyelerEslestirmeTurleri.EslestirmeTurAdi + "' eşleştirme türüne ait '" + kayit.HesapKod + "' Kodlu Yevmiye eşleştirmesi Silindi!";
                    db.YevmiyelerEslestirmes.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.YevmiyelerEslestirmeTurleri.EslestirmeTurAdi + "' eşleştirme türüne ait '" + kayit.HesapKod + "' Kodlu Yevmiye eşleştirmesi Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeProjeBankaHesaplari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek Yevmiye eşleştirmesi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}