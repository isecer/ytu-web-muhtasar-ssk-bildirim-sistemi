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
    [Authorize(Roles = RoleNames.YevmiyeBegleKodlari)]
    public class YevmiyeVergiKimlikNumaralariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyelerVergiKimlikNumaralari { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerVergiKimlikNumaralari model)
        {

            var q = from s in db.YevmiyelerVergiKimlikNumaralaris
                    select s;

            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.AdSoyad.Contains(model.AdSoyad));
            if (!model.Adres.IsNullOrWhiteSpace()) q = q.Where(p => p.Adres.Contains(model.Adres));
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.AdSoyad);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerVergiKimlikNumaralari
            {
                YevmiyeVergiKimlikNoID = s.YevmiyeVergiKimlikNoID,
                VergiKimlikNo = s.VergiKimlikNo,
                AdSoyad = s.AdSoyad,
                Adres = s.Adres,
            }).Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            ViewBag.IndexModel = IndexModel;
            return View(model);
        }

        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new YevmiyelerVergiKimlikNumaralari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.YevmiyeVergiKimlikNoID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerVergiKimlikNumaralari kModel)
        {
            var MmMessage = new MmMessage();
            #region Kontrol  
            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik No Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.AdSoyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Soyad Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });
            if (kModel.Adres.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Adres Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Adres" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Adres" });
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerVergiKimlikNumaralaris.Any(a => a.VergiKimlikNo == kModel.VergiKimlikNo && a.YevmiyeVergiKimlikNoID != kModel.YevmiyeVergiKimlikNoID))
                {
                    MmMessage.Messages.Add("Vergi Kimlik No daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {

                if (kModel.YevmiyeVergiKimlikNoID <= 0)
                {
                    db.YevmiyelerVergiKimlikNumaralaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.YevmiyeVergiKimlikNoID == kModel.YevmiyeVergiKimlikNoID).First();
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.AdSoyad = kModel.AdSoyad;
                    data.Adres = kModel.Adres;
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
            var kayit = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.YevmiyeVergiKimlikNoID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.VergiKimlikNo + " - " + kayit.AdSoyad + "' İsimli Vergi Kimlik No Silindi!";
                    db.YevmiyelerVergiKimlikNumaralaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.VergiKimlikNo + " - " + kayit.AdSoyad + "' Vergi Kimlik No Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeVergiKimlikNumaralari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Vergi Kimlik No sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}