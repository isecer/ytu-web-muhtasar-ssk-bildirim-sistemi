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
    [Authorize(Roles = RoleNames.EksikGunNedenleri)]
    public class EksikGunNedenleriController : Controller
    {
        // GET: EksikGunNedenleri
        private MusskDBEntities db = new MusskDBEntities();

        public ActionResult Index()
        {
            return Index(new FmEksikGunNedenleri { });
        }
        [HttpPost]
        public ActionResult Index(FmEksikGunNedenleri model)
        {

            var q = from s in db.EksikGunNedenleris
                    select s;

            if (model.EksikGunNedenKodu.HasValue) q = q.Where(p => p.EksikGunNedenKodu==model.EksikGunNedenKodu);
            if (!model.EksikGunNedenAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.EksikGunNedenAdi.Contains(model.EksikGunNedenAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.EksikGunNedenAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrEksikGunNedenleri
            {
                EksikGunNedenID = s.EksikGunNedenID,
                EksikGunNedenKodu = s.EksikGunNedenKodu,
                EksikGunNedenAdi = s.EksikGunNedenAdi,
                IsAktif = s.IsAktif,
                IslemTarihi = s.IslemTarihi,
                IslemYapan = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                IslemYapanID = s.IslemYapanID,
                IslemYapanIP = s.IslemYapanIP
            }).Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
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
            var model = new EksikGunNedenleri();
            if (id.HasValue && id > 0)
            {
                var data = db.EksikGunNedenleris.Where(p => p.EksikGunNedenID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(EksikGunNedenleri kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.EksikGunNedenKodu<=0)
            {
                MmMessage.Messages.Add("Eksik Gün Neden Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EksikGunNedenKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EksikGunNedenKodu" });
            if (kModel.EksikGunNedenAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Eksik Gün Neden Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EksikGunNedenAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EksikGunNedenAdi" });
            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.EksikGunNedenID <= 0)
                {
                    kModel.IsAktif = true;
                    db.EksikGunNedenleris.Add(kModel);
                }
                else
                {
                    var data = db.EksikGunNedenleris.Where(p => p.EksikGunNedenID == kModel.EksikGunNedenID).First();
                    data.EksikGunNedenKodu = kModel.EksikGunNedenKodu;
                    data.EksikGunNedenAdi = kModel.EksikGunNedenAdi;
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
            var kayit = db.EksikGunNedenleris.Where(p => p.EksikGunNedenID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.EksikGunNedenAdi + "' İsimli Eksik Gün Nedeni Silindi!";
                    db.EksikGunNedenleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.EksikGunNedenAdi + "' İsimli Eksik Gün Nedeni Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "EksikGunNedenleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Eksik Gün Nedeni sistemde bulunamadı!";
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