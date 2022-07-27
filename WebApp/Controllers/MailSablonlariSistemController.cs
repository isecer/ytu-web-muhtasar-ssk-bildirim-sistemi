using BiskaUtil;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.MailSablonlariSistem)]
    public class MailSablonlariSistemController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmMailSablonlari() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmMailSablonlari model)
        {
            var q = from s in db.MailSablonlaris
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    where s.MailSablonTipleri.SistemMaili
                    select new FrMailSablonlari
                    {
                        MailSablonTipID = s.MailSablonTipID,
                        SablonTipAdi = s.MailSablonTipleri.SablonTipAdi,
                        Parametreler = s.MailSablonTipleri.Parametreler,
                        MailSablonlariID = s.MailSablonlariID,
                        SablonAdi = s.SablonAdi,
                        EkSayisi = s.MailSablonlariEkleris.Count,
                        MailSablonlariEkleris = s.MailSablonlariEkleris.ToList(),
                        Sablon = s.Sablon,
                        SablonHtml = s.SablonHtml,
                        IsAktif = s.IsAktif,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanID = s.IslemYapanID,
                        IslemYapanIP = s.IslemYapanIP,
                        IslemYapan = k.Ad + " " + k.Soyad,
                    };

            if (!model.SablonAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.SablonAdi.Contains(model.SablonAdi) || p.Sablon.Contains(model.SablonAdi));
            if (model.MailSablonTipID.HasValue) q = q.Where(p => p.MailSablonTipID == model.MailSablonTipID);
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);

            model.RowCount = q.Count();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(t => t.SablonTipAdi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
            ViewBag.IndexModel = IndexModel;
            ViewBag.MailSablonTipID = new SelectList(Management.CmbMailSablonTipleri(true), "Value", "Caption", model.MailSablonTipID);
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id, string dlgid)
        {

            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new MailSablonlari();
            if (id.HasValue && id > 0)
            {
                var data = db.MailSablonlaris.Where(p => p.MailSablonlariID == id && p.MailSablonTipleri.SistemMaili).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.SablonTipi = db.MailSablonTipleris.Where(p => p.MailSablonTipID == model.MailSablonTipID).FirstOrDefault();
            ViewBag.MailSablonTipID = new SelectList(Management.CmbMailSablonTipleri(true), "Value", "Caption", model.MailSablonTipID);
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Kayit(MailSablonlari kModel, List<string> EkAdi, List<HttpPostedFileBase> DosyaEki, List<int?> MailSablonlariEkiID, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            MailSablonlariEkiID = MailSablonlariEkiID == null ? new List<int?>() : MailSablonlariEkiID;
            EkAdi = EkAdi == null ? new List<string>() : EkAdi;
            DosyaEki = DosyaEki == null ? new List<HttpPostedFileBase>() : DosyaEki;
            var qDosyaEkAdi = EkAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = DosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDuyuruDosyaEkID = MailSablonlariEkiID.Select((s, inx) => new { s, inx }).ToList();
            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             select new { dek.inx, DosyaEkAdi = dek.s, Dosya = de.s }).ToList();

            var qVarolanlar = (from s in qDosyaEkAdi
                               join sid in qDuyuruDosyaEkID on s.inx equals sid.inx
                               select new { s.inx, DosyaEkAdi = s.s, MailSablonlariEkiID = sid.s });
            #region Kontrol

            if (kModel.MailSablonTipID <= 0)
            { 
                MmMessage.Messages.Add("Şablon Tipini Seçiniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MailSablonTipID"  });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MailSablonTipID" });
            if (kModel.SablonAdi.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Mail Konusu Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "SablonAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "SablonAdi" });

            if (kModel.Sablon.IsNullOrWhiteSpace() && kModel.SablonHtml.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Sablon Açıklaması Giriniz.");
            }
            #endregion


            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Sablon = kModel.Sablon ?? "";

                if (kModel.MailSablonlariID <= 0)
                {
                    kModel.IsAktif = true;
                    var eklenen = db.MailSablonlaris.Add(kModel);

                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        db.MailSablonlariEkleris.Add(new MailSablonlariEkleri
                        {
                            MailSablonlariID = eklenen.MailSablonlariID,
                            EkAdi = item.DosyaEkAdi,
                            EkDosyaYolu = DosyaYolu
                        });
                    }
                }
                else
                {
                    var data = db.MailSablonlaris.Where(p => p.MailSablonlariID == kModel.MailSablonlariID && p.MailSablonTipleri.SistemMaili).First();
                    data.MailSablonTipID = kModel.MailSablonTipID;
                    data.SablonAdi = kModel.SablonAdi;
                    data.Sablon = kModel.Sablon;
                    data.SablonHtml = kModel.SablonHtml;
                    data.MailSablonTipID = kModel.MailSablonTipID;
                    data.IsAktif = kModel.IsAktif;
                    data.IslemTarihi = DateTime.Now;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;

                    var SilinenDuyuruEkleri = db.MailSablonlariEkleris.Where(p => MailSablonlariEkiID.Contains(p.MailSablonlariEkiID) == false && p.MailSablonlariID == data.MailSablonlariID).ToList();
                    var VarolanDuyuruEkleri = db.MailSablonlariEkleris.Where(p => MailSablonlariEkiID.Contains(p.MailSablonlariEkiID) && p.MailSablonlariID == data.MailSablonlariID).ToList();
                    foreach (var item in VarolanDuyuruEkleri)
                    {
                        var qd = qVarolanlar.Where(p => p.MailSablonlariEkiID == item.MailSablonlariEkiID).FirstOrDefault();
                        if (qd != null)
                        {
                            item.EkAdi = qd.DosyaEkAdi;
                        }
                    }
                    db.MailSablonlariEkleris.RemoveRange(SilinenDuyuruEkleri);
                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        db.MailSablonlariEkleris.Add(new MailSablonlariEkleri
                        {
                            MailSablonlariID = data.MailSablonlariID,
                            EkAdi = item.DosyaEkAdi,
                            EkDosyaYolu = DosyaYolu
                        });
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.SablonTipi = db.MailSablonTipleris.Where(p => p.MailSablonTipID == kModel.MailSablonTipID).FirstOrDefault();
            ViewBag.MailSablonTipID = new SelectList(Management.CmbMailSablonTipleri(true), "Value", "Caption", kModel.MailSablonTipID);
            ViewBag.MmMessage = MmMessage;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            return View(kModel);
        }
        public ActionResult getSablonTipParametre(int MailSablonTipID)
        {
            var Stip = db.MailSablonTipleris.Where(p => p.MailSablonTipID == MailSablonTipID).First();
            return Json(Stip.Parametreler, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.MailSablonlaris.Where(p => p.MailSablonlariID == id && p.MailSablonTipleri.SistemMaili).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.SablonAdi + "' Şablon Şablon Silindi!";
                    var dosyalar = kayit.MailSablonlariEkleris.ToList();

                    db.MailSablonlaris.Remove(kayit);
                    db.SaveChanges();
                    foreach (var item in dosyalar)
                    {
                        System.IO.File.Delete(Server.MapPath("~" + item.EkDosyaYolu));
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.SablonAdi + "' Başlıklı Şablon! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "MailSablonlari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Şablon sistemde bulunamadı!";
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