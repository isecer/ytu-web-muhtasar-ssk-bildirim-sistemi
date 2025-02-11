using BiskaUtil;
using Database;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.YevmiyeAlimKanunTurleri)]
    public class YevmiyeAlimKanunTurleriController : Controller
    {
        private readonly MusskDBEntities db = new MusskDBEntities();
        // GET: YevmiyeBelgeKodlari
        public ActionResult Index()
        {
            return Index(new FmYevmiyeAlimKanunTurleri { });
        }
        [HttpPost]
        public ActionResult Index(FmYevmiyeAlimKanunTurleri model)
        {

            var q = from s in db.YevmiyelerAlimKanunTurleris
                join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                select new FrYevmiyeAlimKanunTurleri
                {
                    YevmiyeAlimKanunTurID = s.YevmiyeAlimKanunTurID,
                    AlimKanunTurAdi = s.AlimKanunTurAdi,
                    IslemTarihi = s.IslemTarihi,
                    IslemYapan = kul.Ad + " " + kul.Soyad,
                    IslemYapanID = s.IslemYapanID,
                    IslemYapanIP = s.IslemYapanIP
                };
                     

            if (!model.AlimKanunTurAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.AlimKanunTurAdi == model.AlimKanunTurAdi);
              model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.AlimKanunTurAdi);
            var ps = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = ps.PageIndex;
            model.Data = q.Skip(ps.StartRowIndex).Take(model.PageSize).ToArray();
            var indexModel = new MIndexBilgi
            {
                Toplam = model.RowCount
            };
            ViewBag.IndexModel = indexModel;
            return View(model);
        }

        public ActionResult Kayit(int? id, string dlgid)
        {
            var mmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = mmMessage;
            var model = new YevmiyelerAlimKanunTurleri();
            if (id.HasValue && id > 0)
            {
                var data = db.YevmiyelerAlimKanunTurleris.FirstOrDefault(p => p.YevmiyeAlimKanunTurID == id);
                if (data != null) model = data;
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YevmiyelerAlimKanunTurleri kModel, string dlgid = "")
        {
            var mmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
           
            if (kModel.AlimKanunTurAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Alım Kanun Tür Adı Boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AlimKanunTurAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AlimKanunTurAdi" });
          
             
            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyeAlimKanunTurID <= 0)
                {
                    db.YevmiyelerAlimKanunTurleris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerAlimKanunTurleris.First(p => p.YevmiyeAlimKanunTurID == kModel.YevmiyeAlimKanunTurID);
                    data.AlimKanunTurAdi = kModel.AlimKanunTurAdi; 
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            }

            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.YevmiyelerAlimKanunTurleris.FirstOrDefault(p => p.YevmiyeAlimKanunTurID == id);
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.AlimKanunTurAdi + "' İsimli Kayıt Silindi!";
                    db.YevmiyelerAlimKanunTurleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.AlimKanunTurAdi + "' İsimli Kayıt Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeAlimKanunTurleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kayıt sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}