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
    [Authorize(Roles = RoleNames.YevmiyeKDVKodlari)]
    public class YevmiyeKdvKodlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyeKdvKodlari { PageSize = 50 });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeKdvKodlari model)
        {

            var q = from s in db.YevmiyelerKdvKodlaris
                    select s;

            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod);
            if (!model.KdvKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.KdvKodu == model.KdvKodu);
            if (!model.KdvAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.KdvAdi.Contains(model.KdvAdi));
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.KdvAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerKdvKodlari
            {
                YevmiyeKdvKodID = s.YevmiyeKdvKodID,
                HesapKod = s.HesapKod,
                IsDigerKdvler = s.IsDigerKdvler,
                KdvKodu = s.KdvKodu,
                KdvAdi = s.KdvAdi,
                KdvOrani = s.KdvOrani,
                TevkifatOranBolen = s.TevkifatOranBolen,
                TevkifatOranBolunen = s.TevkifatOranBolunen,
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
            var model = new YevmiyelerKdvKodlari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerKdvKodlari kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (!kModel.IsDigerKdvler)
            {
                if (kModel.HesapKod.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Hesap Kodu Boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });
            }
            if (kModel.KdvKodu.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Kdv Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KdvKodu" });
            if (kModel.KdvAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Kdv Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KdvAdi" });
            if (kModel.KdvOrani.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Kdv Oranı Giriniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvOrani" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KdvOrani" });
            if (!kModel.IsDigerKdvler)
            {
                if (kModel.TevkifatOranBolunen <= 0)
                {
                    MmMessage.Messages.Add("Oran Bölünen 0 Dan  Büyük Olmalı.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOranBolunen" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOranBolunen" });
                if (kModel.TevkifatOranBolen <= 0)
                {
                    MmMessage.Messages.Add("Oran Bölen 0 Dan  Büyük Olmalı.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatOranBolen" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TevkifatOranBolen" });
            }
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerKdvKodlaris.Any(a => a.KdvKodu == kModel.KdvKodu && a.YevmiyeKdvKodID != kModel.YevmiyeKdvKodID))
                {
                    MmMessage.Messages.Add("Kdv Kodu daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvKodu" });
                }
            }
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.IsDigerKdvler) { kModel.TevkifatOranBolen = null; kModel.TevkifatOranBolunen = null; }
                if (kModel.YevmiyeKdvKodID <= 0)
                {
                    db.YevmiyelerKdvKodlaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == kModel.YevmiyeKdvKodID).First();
                    data.HesapKod = kModel.HesapKod;
                    data.KdvKodu = kModel.KdvKodu;
                    data.KdvAdi = kModel.KdvAdi;
                    data.KdvOrani = kModel.KdvOrani;
                    data.IsDigerKdvler = kModel.IsDigerKdvler;
                    data.TevkifatOranBolunen = kModel.TevkifatOranBolunen;
                    data.TevkifatOranBolen = kModel.TevkifatOranBolen;
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
            var kayit = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.KdvAdi + "' İsimli Kdv Silindi!";
                    db.YevmiyelerKdvKodlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.KdvAdi + "' İsimli Kdv Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeKdvKodlari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kdv bilgisi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}