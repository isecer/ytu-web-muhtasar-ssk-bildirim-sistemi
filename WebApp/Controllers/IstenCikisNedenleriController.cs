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
    [Authorize(Roles = RoleNames.IstenCikisNedenleri)]
    public class IstenCikisNedenleriController : Controller
    {
        // GET: IstenCikisNedenleri
        private MusskDBEntities db = new MusskDBEntities();

        public ActionResult Index()
        {
            return Index(new FmIstenCikisNedenleri { });
        }
        [HttpPost]
        public ActionResult Index(FmIstenCikisNedenleri model)
        {

            var q = from s in db.IstenCikisNedenleris
                    select s;

            if (model.IstenCikisNedenKodu.HasValue) q = q.Where(p => p.IstenCikisNedenKodu == model.IstenCikisNedenKodu);
            if (!model.IstenCikisNedenAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.IstenCikisNedenAdi.Contains(model.IstenCikisNedenAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.IstenCikisNedenAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrIstenCikisNedenleri
            {
                IstenCikisNedenID = s.IstenCikisNedenID,
                IstenCikisNedenKodu = s.IstenCikisNedenKodu,
                IstenCikisNedenAdi = s.IstenCikisNedenAdi,
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
            var model = new IstenCikisNedenleri();
            if (id.HasValue && id > 0)
            {
                var data = db.IstenCikisNedenleris.Where(p => p.IstenCikisNedenID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(IstenCikisNedenleri kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.IstenCikisNedenKodu <= 0)
            {
                MmMessage.Messages.Add("İşten Çıkış Neden Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IstenCikisNedenKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IstenCikisNedenKodu" });
            if (kModel.IstenCikisNedenAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("İşten Çıkış Neden Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IstenCikisNedenAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IstenCikisNedenAdi" });
            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.IstenCikisNedenID <= 0)
                {
                    kModel.IsAktif = true;
                    db.IstenCikisNedenleris.Add(kModel);
                }
                else
                {
                    var data = db.IstenCikisNedenleris.Where(p => p.IstenCikisNedenID == kModel.IstenCikisNedenID).First();
                    data.IstenCikisNedenKodu = kModel.IstenCikisNedenKodu;
                    data.IstenCikisNedenAdi = kModel.IstenCikisNedenAdi;
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
            var kayit = db.IstenCikisNedenleris.Where(p => p.IstenCikisNedenID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.IstenCikisNedenAdi + "' İsimli İşten Çıkış Nedeni Silindi!";
                    db.IstenCikisNedenleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.IstenCikisNedenAdi + "' İsimli İşten Çıkış Nedeni Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "IstenCikisNedenleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz İşten Çıkış Nedeni sistemde bulunamadı!";
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