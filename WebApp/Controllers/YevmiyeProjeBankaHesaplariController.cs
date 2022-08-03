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
    [Authorize(Roles = RoleNames.YevmiyeProjeBankaHesaplari)]
    public class YevmiyeProjeBankaHesaplariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyelerProjeBankaHesapNumaralari { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyelerProjeBankaHesapNumaralari model)
        {

            var q = from s in db.YevmiyelerProjeBankaHesapNumaralaris
                    select s;

            if (!model.HesapNo.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapNo == model.HesapNo);
            if (!model.HesapAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapAdi.Contains(model.HesapAdi));
            if (!model.ProjeNo.IsNullOrWhiteSpace()) q = q.Where(p => p.ProjeNo == model.ProjeNo);
            if (!model.ProjeAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.ProjeAdi.Contains(model.ProjeAdi));
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.HesapAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerProjeBankaHesapNumaralari
            {
                ProjeBankaHesapNoID = s.ProjeBankaHesapNoID,
                HesapNo = s.HesapNo,
                HesapAdi = s.HesapAdi,
                ProjeNo = s.ProjeNo,
                ProjeAdi = s.ProjeAdi,
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
            var model = new YevmiyelerProjeBankaHesapNumaralari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerProjeBankaHesapNumaralaris.Where(p => p.ProjeBankaHesapNoID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerProjeBankaHesapNumaralari kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.HesapNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Numarası Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapNo" });
            if (kModel.HesapAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapAdi" });
            if (kModel.ProjeNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Proje Numarası Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "ProjeNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "ProjeNo" });
            if (kModel.ProjeAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Proje Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "ProjeAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "ProjeAdi" });
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerProjeBankaHesapNumaralaris.Any(a => a.HesapNo == kModel.HesapNo && a.ProjeBankaHesapNoID != kModel.ProjeBankaHesapNoID))
                {
                    MmMessage.Messages.Add("Hesap Numarası daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapNo" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.ProjeBankaHesapNoID <= 0)
                {
                    db.YevmiyelerProjeBankaHesapNumaralaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerProjeBankaHesapNumaralaris.Where(p => p.ProjeBankaHesapNoID == kModel.ProjeBankaHesapNoID).First();
                    data.HesapNo = kModel.HesapNo;
                    data.HesapAdi = kModel.HesapAdi;
                    data.ProjeNo = kModel.ProjeNo;
                    data.ProjeAdi = kModel.ProjeAdi;
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
            var kayit = db.YevmiyelerProjeBankaHesapNumaralaris.Where(p => p.ProjeBankaHesapNoID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.HesapNo + " - " + kayit.HesapAdi + "' İsimli Hesap No Silindi!";
                    db.YevmiyelerProjeBankaHesapNumaralaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.HesapNo + " - " + kayit.HesapAdi + "' İsimli Hesap No Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeProjeBankaHesaplari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Hesap No sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}