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
    [Authorize(Roles = RoleNames.YevmiyeHarcamaBirimleri)]
    public class YevmiyeHarcamaBirimleriController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyelerHarcamaBirimleri { PageSize = 50 });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerHarcamaBirimleri model)
        {

            var q = from s in db.YevmiyelerHarcamaBirimleris
                    select s;

            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.BirimAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.BirimAdi.Contains(model.BirimAdi));
            if (model.IsUniversiteIsyeri.HasValue) q = q.Where(p => p.IsUniversiteIsyeri == model.IsUniversiteIsyeri);
            if (model.IsAltBirim.HasValue) q = q.Where(p => p.IsAltBirim == model.IsAltBirim);
            if (!model.IsyeriKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.IsyeriKodu == model.IsyeriKodu);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.IsyeriKodu);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerHarcamaBirimleri
            {
                YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                VergiKimlikNo = s.VergiKimlikNo,
                BirimAdi = s.BirimAdi,
                IsUniversiteIsyeri = s.IsUniversiteIsyeri,
                IsAltBirim = s.IsAltBirim,
                IsyeriKodu = s.IsyeriKodu,
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
            var model = new YevmiyelerHarcamaBirimleri();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerHarcamaBirimleris.Where(p => p.YevmiyeHarcamaBirimID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerHarcamaBirimleri kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik Numarası Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.BirimAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Birim Adı Boş Bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimAdi" });
            if (kModel.IsUniversiteIsyeri == true)
            {
                if (kModel.IsyeriKodu.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("İş Yeri Kodu Boş Bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IsyeriKodu" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IsyeriKodu" });
            }
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerHarcamaBirimleris.Any(a => a.VergiKimlikNo == kModel.VergiKimlikNo && a.IsyeriKodu == kModel.IsyeriKodu && a.YevmiyeHarcamaBirimID != kModel.YevmiyeHarcamaBirimID))
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
                if (!kModel.IsUniversiteIsyeri) { kModel.IsyeriKodu = null; kModel.IsAltBirim = false; }
                if (kModel.YevmiyeHarcamaBirimID <= 0)
                {
                    db.YevmiyelerHarcamaBirimleris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerHarcamaBirimleris.Where(p => p.YevmiyeHarcamaBirimID == kModel.YevmiyeHarcamaBirimID).First();
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.BirimAdi = kModel.BirimAdi;
                    data.IsUniversiteIsyeri = kModel.IsUniversiteIsyeri;
                    data.IsAltBirim = kModel.IsAltBirim;
                    data.IsyeriKodu = kModel.IsyeriKodu;
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
            var kayit = db.YevmiyelerHarcamaBirimleris.Where(p => p.YevmiyeHarcamaBirimID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.VergiKimlikNo + "' vergi kimlik numaralı Harcama Birimi Silindi!";
                    db.YevmiyelerHarcamaBirimleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.VergiKimlikNo + "' vergi kimlik numaralı Harcama Birimi Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeHarcamaBirimleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Harcama Birimi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}