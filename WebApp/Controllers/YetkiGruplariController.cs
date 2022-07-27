using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BiskaUtil;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.YetkiGruplari)]
    public class YetkiGruplariController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmYetkiGruplari());
        }
        [HttpPost]
        public ActionResult Index(FmYetkiGruplari model)
        {
            var q = from s in db.YetkiGruplaris select s;

            if (!model.YetkiGrupAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.YetkiGrupAdi.Contains(model.YetkiGrupAdi));
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(t => t.YetkiGrupAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).Select(s => new FrYetkiGruplari
            {
                YetkiGrupID = s.YetkiGrupID,
                YetkiGrupAdi = s.YetkiGrupAdi, 
                YetkiSayisi = s.YetkiGrupRolleris.Count
            }).ToArray();

            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new YetkiGruplari();
            if (id.HasValue && id > 0)
            {
                var data = db.YetkiGruplaris.Where(p => p.YetkiGrupID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            var roles = Management.GetAllRoles().ToList();
            var sRol = new List<Roller>();
            if (id.HasValue && id.Value > 0) sRol = Management.GetYetkiGrupRoles(id.Value);

            var dataR = roles.Select(s => new CheckObject<Roller>
            {
                Value = s,
                Checked = sRol.Any(p => p.RolID == s.RolID)
            });
            ViewBag.Roller = dataR;
            var kategr = roles.Select(s => s.Kategori).Distinct().ToArray();
            var menuK = db.Menulers.Where(a => a.BagliMenuID == 0 && kategr.Contains(a.MenuAdi)).ToList();
            var dct = new List<ComboModelInt>();
            foreach (var item in menuK)
            {
                dct.Add(new ComboModelInt { Value = item.SiraNo.Value, Caption = item.MenuAdi });
            }
            ViewBag.cats = dct; 
            ViewBag.MmMessage = MmMessage;
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(YetkiGruplari model, List<int> RolID)
        {
            RolID = RolID ?? new List<int>();
            var MmMessage = new MmMessage();
            if (model.YetkiGrupAdi.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Yetki Grup Adı Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YetkiGrupAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YetkiGrupAdi" });
            //if (RolID == null || RolID.Count == 0)
            //{
            //    string msg = "Yetki Grubuna Ait Rolleri Belirleyiniz!";
            //    MmMessage.Messages.Add(msg);
            //}
            if (MmMessage.Messages.Count == 0)
            {
                model.IslemYapanID = UserIdentity.Current.Id;
                model.IslemYapanIP = UserIdentity.Ip;
                model.IslemTarihi = DateTime.Now;
                if (model.YetkiGrupID == 0)
                {

                    db.YetkiGruplaris.Add(model);
                    db.SaveChanges();
                }
                else
                {
                    var yg = db.YetkiGruplaris.Where(p => p.YetkiGrupID == model.YetkiGrupID).First();
                    yg.IslemYapanID = UserIdentity.Current.Id;
                    yg.IslemYapanIP = UserIdentity.Ip;
                    yg.IslemTarihi = DateTime.Now;
                    yg.YetkiGrupAdi = model.YetkiGrupAdi;
                }
                var eskiROl = db.YetkiGrupRolleris.Where(p => p.YetkiGrupID == model.YetkiGrupID).ToList();
                db.YetkiGrupRolleris.RemoveRange(eskiROl);
                foreach (var item in RolID)
                {
                    db.YetkiGrupRolleris.Add(new YetkiGrupRolleri { YetkiGrupID = model.YetkiGrupID, RolID = item });
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            var roles = Management.GetAllRoles().ToList();
            var sRol = new List<int>();
            if (RolID != null && RolID.Count > 0) sRol = RolID;

            var dataR = roles.Select(s => new CheckObject<Roller>
            {
                Value = s,
                Checked = sRol.Any(p => p == s.RolID)
            });
            var kategr = roles.Select(s => s.Kategori).Distinct().ToArray();
            var menuK = db.Menulers.Where(a => a.BagliMenuID == 0 && kategr.Contains(a.MenuAdi)).ToList();
            var dct = new List<ComboModelInt>();
            foreach (var item in menuK)
            {
                dct.Add(new ComboModelInt { Value = item.SiraNo.Value, Caption = item.MenuAdi });
            }
            ViewBag.cats = dct;
            ViewBag.Roller = dataR;
            ViewBag.MmMessage = MmMessage;
            return View(model);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.YetkiGruplaris.Where(p => p.YetkiGrupID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.YetkiGrupAdi + "' Yetki Grubu Silindi!";
                    db.YetkiGruplaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.YetkiGrupAdi + "' Yetki Grubu Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage(); 
                    Management.SistemBilgisiKaydet(message, "YetkiGruplari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Yetki Grubu sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}
