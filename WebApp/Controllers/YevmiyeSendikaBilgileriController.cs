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
    [Authorize(Roles = RoleNames.YevmiyeSendikaBilgileri)]
    public class YevmiyeSendikaBilgileriController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyeSendikaBilgileri { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeSendikaBilgileri model)
        {

            var q = from s in db.YevmiyelerSendikaBilgileris
                    select s;

            if (!model.HesapKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKodu == model.HesapKodu);
            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.AdSoyad.Contains(model.AdSoyad));
            if (!model.IBanNo.IsNullOrWhiteSpace()) q = q.Where(p => p.IBanNo == model.IBanNo);
            if (!model.KisaAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.KisaAdi == model.KisaAdi);
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama == model.Aciklama);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.AdSoyad);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerSendikaBilgileri
            {
                YevmiyeSendikaBilgiID = s.YevmiyeSendikaBilgiID,
                HesapKodu = s.HesapKodu,
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
            var model = new YevmiyelerSendikaBilgileri();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerSendikaBilgileris.Where(p => p.YevmiyeSendikaBilgiID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerSendikaBilgileri kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.HesapKodu.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Kodu Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKodu" });
            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik Numarası Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.AdSoyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Soyad Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });
            if (kModel.IBanNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("IBan No Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IBanNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IBanNo" });
            if (kModel.KisaAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Kısa Adı Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KisaAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KisaAdi" });
            if (kModel.Aciklama.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Açıklama Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Aciklama" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerSendikaBilgileris.Any(a => a.VergiKimlikNo == kModel.VergiKimlikNo && a.YevmiyeSendikaBilgiID != kModel.YevmiyeSendikaBilgiID))
                {
                    MmMessage.Messages.Add("Vergi Kimlik Numarası daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip; 
                if (kModel.YevmiyeSendikaBilgiID <= 0)
                {
                    db.YevmiyelerSendikaBilgileris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerSendikaBilgileris.Where(p => p.YevmiyeSendikaBilgiID == kModel.YevmiyeSendikaBilgiID).First();
                    data.HesapKodu = kModel.HesapKodu;
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
            var kayit = db.YevmiyelerSendikaBilgileris.Where(p => p.YevmiyeSendikaBilgiID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.VergiKimlikNo + "' vergi kimlik numaralı Sendika Silindi!";
                    db.YevmiyelerSendikaBilgileris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.VergiKimlikNo + "' vergi kimlik numaralı Sendika Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeSendikaBilgileri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Sendika sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}