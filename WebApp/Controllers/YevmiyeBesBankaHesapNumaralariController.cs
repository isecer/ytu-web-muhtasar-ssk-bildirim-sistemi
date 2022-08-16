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
    public class YevmiyeBesBankaHesapNumaralariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyelerBesBankaHesapNumaralari { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerBesBankaHesapNumaralari model)
        {

            var q = from s in db.YevmiyelerBesBankaHesapNumaralaris
                    select s;

            if (model.IsYevmiyeDokumuAyriOlabilir.HasValue) q = q.Where(p => p.IsYevmiyeDokumuAyriOlabilir == model.IsYevmiyeDokumuAyriOlabilir);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod);
            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.FirmaAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.FirmaAdi.Contains(model.FirmaAdi));
            if (!model.IBanNo.IsNullOrWhiteSpace()) q = q.Where(p => p.IBanNo == model.IBanNo);
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama == model.Aciklama);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.FirmaAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerBesBankaHesapNumaralari
            {
                YevmiyeBesBankaHesapNumaraID = s.YevmiyeBesBankaHesapNumaraID,
                IsYevmiyeDokumuAyriOlabilir = s.IsYevmiyeDokumuAyriOlabilir,
                HesapKod = s.HesapKod,
                VergiKimlikNo = s.VergiKimlikNo,
                FirmaAdi = s.FirmaAdi,
                IBanNo = s.IBanNo,
                Aciklama = s.Aciklama,
                IslemTarihi = s.IslemTarihi
            }).Skip(PS.StartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            ViewBag.IndexModel = IndexModel;
            return View(model);
        }

        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new YevmiyelerBesBankaHesapNumaralari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerBesBankaHesapNumaralaris.Where(p => p.YevmiyeBesBankaHesapNumaraID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerBesBankaHesapNumaralari kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.HesapKod.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Kodu Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });
            if (kModel.IsYevmiyeDokumuAyriOlabilir != true)
            {
                if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Vergi Kimlik Numarası Boş Bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            }
            if (kModel.FirmaAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Firma Adı Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FirmaAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FirmaAdi" });
            if (kModel.IBanNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("IBan No Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IBanNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IBanNo" });

            if (kModel.Aciklama.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Açıklama Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Aciklama" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerBesBankaHesapNumaralaris.Any(a => (a.VergiKimlikNo == kModel.VergiKimlikNo && a.HesapKod == kModel.HesapKod) && a.YevmiyeBesBankaHesapNumaraID != kModel.YevmiyeBesBankaHesapNumaraID))
                {
                    MmMessage.Messages.Add("Vergi Kimlik ve Hesap Numarası daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.IsYevmiyeDokumuAyriOlabilir != true) kModel.VergiKimlikNo = null;

                if (kModel.YevmiyeBesBankaHesapNumaraID <= 0)
                {
                    db.YevmiyelerBesBankaHesapNumaralaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerBesBankaHesapNumaralaris.Where(p => p.YevmiyeBesBankaHesapNumaraID == kModel.YevmiyeBesBankaHesapNumaraID).First();
                    data.IsYevmiyeDokumuAyriOlabilir = kModel.IsYevmiyeDokumuAyriOlabilir;
                    data.HesapKod = kModel.HesapKod;
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.FirmaAdi = kModel.FirmaAdi;
                    data.IBanNo = kModel.IBanNo;
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
            var kayit = db.YevmiyelerBesBankaHesapNumaralaris.Where(p => p.YevmiyeBesBankaHesapNumaraID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.HesapKod + " Hesap Kodlu' ve " + kayit.VergiKimlikNo + " Vergi kimlik numaralı Banka Hesap Numarası Silindi!";
                    db.YevmiyelerBesBankaHesapNumaralaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.HesapKod + " Hesap Kodlu' ve " + kayit.VergiKimlikNo + " Vergi kimlik numaralı Banka Hesap Numarası Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeBesHesapKoduEslestirmeController/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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