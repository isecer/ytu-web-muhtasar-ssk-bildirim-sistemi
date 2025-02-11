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
    public class YevmiyeBelgeKodlariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyeBelgeKodlari { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeBelgeKodlari model)
        {

            var q = from s in db.YevmiyelerBelgeKodlaris
                    let belgeTur = db.BelgeTurleris.FirstOrDefault(p => p.BelgeTurKodu == s.BelgeKodu)
                    select new
                    {
                        s.YevmiyeBelgeKodID,
                        s.BelgeKodu,
                        s.BelgeAdi,
                        s.IslemTarihi,
                        s.Kullanicilar.Ad,
                        s.Kullanicilar.Soyad,
                        s.IslemYapanID,
                        s.IslemYapanIP,
                        belgeTur
                    };

            if (!model.BelgeKodu.IsNullOrWhiteSpace()) q = q.Where(p => p.BelgeKodu == model.BelgeKodu);
            if (!model.BelgeAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.BelgeAdi.Contains(model.BelgeAdi));
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.BelgeAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Select(s => new FrYevmiyelerBelgeKodlari
            {
                YevmiyeBelgeKodID = s.YevmiyeBelgeKodID,
                BelgeKodu = s.BelgeKodu,
                BelgeAdi = s.BelgeAdi,
                YuzdeOran = s.belgeTur != null ? s.belgeTur.PrimYuzdesi : 0,
                IsBelgeTurTablosundaBelgeKoduVar = s.belgeTur != null,
                BelgeKodunaDenkGelenBelgeTurAdi = s.belgeTur != null ? s.belgeTur.BelgeTurAdi : "",
                IslemTarihi = s.IslemTarihi,
                IslemYapan = s.Ad + " " + s.Soyad,
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
            var model = new YevmiyelerBelgeKodlari();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerBelgeKodlaris.Where(p => p.YevmiyeBelgeKodID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerBelgeKodlari kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol  
            if (kModel.BelgeKodu.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Belge Kodu Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeKodu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BelgeKodu" });
            if (kModel.BelgeAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Belge Adı Boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BelgeAdi" });
         
            #endregion
            if (!MmMessage.Messages.Any())
            {
                if (db.YevmiyelerBelgeKodlaris.Any(a => a.BelgeKodu == kModel.BelgeKodu && a.YevmiyeBelgeKodID != kModel.YevmiyeBelgeKodID))
                {
                    MmMessage.Messages.Add("Belge kodu daha önce tanımlanmıştır. Tekrar tanımlanamaz!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeKodu" });
                }
            }
            if (!MmMessage.Messages.Any())
            {
                var belgeTur = db.BelgeTurleris.FirstOrDefault(a => a.BelgeTurKodu == kModel.BelgeKodu);
                if (belgeTur==null)
                {
                    MmMessage.Messages.Add($"{kModel.BelgeKodu} nolu Bu belge kodunun tanımlanabilmesi için İşçi işlemleri > Belge Türlerinde bulunan kayıtlarda aynı belge kodu ile kayıt açılması gerekmektedir!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeKodu" });
                }
                else
                {
                    kModel.YuzdeOran = belgeTur.PrimYuzdesi;
                }
            } 
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyeBelgeKodID <= 0)
                {
                    db.YevmiyelerBelgeKodlaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerBelgeKodlaris.Where(p => p.YevmiyeBelgeKodID == kModel.YevmiyeBelgeKodID).First();
                    data.BelgeKodu = kModel.BelgeKodu;
                    data.BelgeAdi = kModel.BelgeAdi;
                    data.YuzdeOran = kModel.YuzdeOran;
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
            var kayit = db.YevmiyelerBelgeKodlaris.Where(p => p.YevmiyeBelgeKodID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.BelgeAdi + "' İsimli Belge Kodu Silindi!";
                    db.YevmiyelerBelgeKodlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.BelgeAdi + "' İsimli Belge Kodu Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeBelgeKodlari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Belge Kodu sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}