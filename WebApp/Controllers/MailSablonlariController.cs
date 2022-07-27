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
    [Authorize(Roles = RoleNames.MailSablonlari)]
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 0, VaryByParam = "*")]
    public class MailSablonlariController : Controller
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
                    where s.MailSablonTipleri.SistemMaili == false
                    select new FrMailSablonlari
                    {
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
            else q = q.OrderByDescending(o => o.IslemTarihi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
            ViewBag.IndexModel = IndexModel;
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
                var data = db.MailSablonlaris.Where(p => p.MailSablonlariID == id && p.MailSablonTipleri.SistemMaili == false).FirstOrDefault();
                if (data != null) model = data;
            }

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

            if (kModel.SablonAdi.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Şablon Adı Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "SablonAdi"  });
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
                    kModel.MailSablonTipID = 1;
                    kModel.IsAktif = true;
                    var eklenen = db.MailSablonlaris.Add(kModel);

                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        eklenen.MailSablonlariEkleris.Add(new MailSablonlariEkleri
                        { 
                            EkAdi = item.DosyaEkAdi,
                            EkDosyaYolu = DosyaYolu
                        });
                    }
                }
                else
                {
                    var data = db.MailSablonlaris.Where(p => p.MailSablonlariID == kModel.MailSablonlariID && p.MailSablonTipleri.SistemMaili == false).First();
                    data.SablonAdi = kModel.SablonAdi;
                    data.Sablon = kModel.Sablon;
                    data.SablonHtml = kModel.SablonHtml;
                    data.IsAktif = kModel.IsAktif;
                    data.IslemTarihi = DateTime.Now;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;

                    var SilinenMSEkleri = data.MailSablonlariEkleris.Where(p => MailSablonlariEkiID.Contains(p.MailSablonlariEkiID) == false).ToList();
                    var VarolanMSEkleri = data.MailSablonlariEkleris.Where(p => MailSablonlariEkiID.Contains(p.MailSablonlariEkiID)).ToList();
                    foreach (var item in VarolanMSEkleri)
                    {
                        var qd = qVarolanlar.Where(p => p.MailSablonlariEkiID == item.MailSablonlariEkiID).FirstOrDefault();
                        if (qd != null)
                        {
                            item.EkAdi = qd.DosyaEkAdi;
                        }
                    }
                    db.MailSablonlariEkleris.RemoveRange(SilinenMSEkleri);
                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        data.MailSablonlariEkleris.Add(new MailSablonlariEkleri
                        { 
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
            ViewBag.MmMessage = MmMessage;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.MailSablonlaris.Where(p => p.MailSablonlariID == id && p.MailSablonTipleri.SistemMaili == false).FirstOrDefault();
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