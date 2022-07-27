using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BelgeMahiyetTipleri)]
    public class BelgeMahiyetTipleriController : Controller
    {
        // GET: BelgeMahiyetTipleri
        private MusskDBEntities db = new MusskDBEntities();
      
        public ActionResult Index()
        {
            return Index(new FmBelgeMahiyetTipleri { });
        }
        [HttpPost]
        public ActionResult Index(FmBelgeMahiyetTipleri model)
        {

            var q = from s in db.BelgeMahiyetTipleris
                    select s;

            if (!model.BelgeMahiyetTipKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.BelgeMahiyetTipKodu.Contains(model.BelgeMahiyetTipKodu));
            if (!model.BelgeMahiyetTipAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.BelgeMahiyetTipAdi.Contains(model.BelgeMahiyetTipAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.BelgeMahiyetTipAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrBelgeMahiyetTipleri
            {
                BelgeMahiyetTipID = s.BelgeMahiyetTipID,
                BelgeMahiyetTipAdi = s.BelgeMahiyetTipAdi,
                BelgeMahiyetTipKodu = s.BelgeMahiyetTipKodu,
                IsAktif = s.IsAktif,
                IslemTarihi = s.IslemTarihi,
                IslemYapan = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                IslemYapanID = s.IslemYapanID,
                IslemYapanIP = s.IslemYapanIP }).Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            IndexModel.Aktif = q.Where(p => p.IsAktif).Count();
            IndexModel.Pasif = q.Where(p => !p.IsAktif).Count();
            ViewBag.IndexModel = IndexModel;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new BelgeMahiyetTipleri();
            if (id.HasValue && id > 0)
            {
                var data = db.BelgeMahiyetTipleris.Where(p => p.BelgeMahiyetTipID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(BelgeMahiyetTipleri kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.BelgeMahiyetTipKodu.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Belge Mahiyet Tip Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeMahiyetTipKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BelgeMahiyetTipKodu" });
            if (kModel.BelgeMahiyetTipAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Belge Mahiyet Tip Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeMahiyetTipAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BelgeMahiyetTipAdi" });
            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.BelgeMahiyetTipID <= 0)
                {
                    kModel.IsAktif = true;
                    db.BelgeMahiyetTipleris.Add(kModel);
                }
                else
                {
                    var data = db.BelgeMahiyetTipleris.Where(p => p.BelgeMahiyetTipID == kModel.BelgeMahiyetTipID).First();
                    data.BelgeMahiyetTipKodu = kModel.BelgeMahiyetTipKodu;
                    data.BelgeMahiyetTipAdi = kModel.BelgeMahiyetTipAdi;
                    data.IsAktif = kModel.IsAktif;
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
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.BelgeMahiyetTipleris.Where(p => p.BelgeMahiyetTipID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.BelgeMahiyetTipAdi + "' İsimli Belge Mahiyet Tipi Silindi!";
                    db.BelgeMahiyetTipleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.BelgeMahiyetTipAdi + "' İsimli Belge Mahiyet Tipi Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "BelgeMahiyetTipleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Belge Mahiyet Tipi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}