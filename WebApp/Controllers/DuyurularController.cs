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
    [Authorize(Roles = RoleNames.Duyurular)]
    public class DuyurularController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmDuyurular() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmDuyurular model)
        {
            
            var q = from s in db.Duyurulars
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID 
                    select new
                    {
                         
                        s.DuyuruID,
                        s.Tarih,
                        s.Baslik,
                        s.Aciklama,
                        s.AciklamaHtml,
                        DuyuruYapan = k.Ad + " " + k.Soyad,
                        s.IslemYapanIP,
                        EkSayisi = s.DuyuruEkleris.Count,
                        Ekler = s.DuyuruEkleris,
                        IsAktif = s.IsAktif,
                        AnaSayfadaGozuksun = s.AnaSayfadaGozuksun,
                        AnaSayfaPopupAc = s.AnaSayfaPopupAc, 
                        YayinSonTarih = s.YayinSonTarih,
                    };

            if (!model.Baslik.IsNullOrWhiteSpace()) q = q.Where(p => p.Baslik.Contains(model.Baslik));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);
            if (model.Tarih.HasValue)
            {
                var trih = model.Tarih.Value.TodateToShortDate();
                q = q.Where(p => p.Tarih == trih);

            }
            model.RowCount = q.Count();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            IndexModel.Aktif = q.Where(p => p.IsAktif).Count();
            IndexModel.Pasif = model.RowCount - IndexModel.Aktif;
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(o => o.Tarih);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).Select(s => new FrDuyurular
            { 
                DuyuruID = s.DuyuruID,
                Baslik = s.Baslik,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                Tarih = s.Tarih,
                DuyuruYapan = s.DuyuruYapan,
                IslemYapanIP = s.IslemYapanIP,
                EkSayisi = s.EkSayisi,
                DuyuruEkleris = s.Ekler,
                IsAktif = s.IsAktif,
                AnaSayfadaGozuksun = s.AnaSayfadaGozuksun,
                AnaSayfaPopupAc = s.AnaSayfaPopupAc, 
                YayinSonTarih = s.YayinSonTarih
            }).ToList();
            ViewBag.IndexModel = IndexModel;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new Duyurular();
            if (id.HasValue && id > 0)
            {
                var data = db.Duyurulars.Where(p => p.DuyuruID == id).FirstOrDefault();
                if (data != null) model = data;
            }

            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Kayit(Duyurular kModel, List<string> DosyaEkiAdi, List<HttpPostedFileBase> DosyaEki, List<int?> DuyuruDosyaEkID, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            DuyuruDosyaEkID = DuyuruDosyaEkID == null ? new List<int?>() : DuyuruDosyaEkID;
            DosyaEkiAdi = DosyaEkiAdi == null ? new List<string>() : DosyaEkiAdi;
            DosyaEki = DosyaEki == null ? new List<HttpPostedFileBase>() : DosyaEki;
            var qDosyaEkAdi = DosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = DosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDuyuruDosyaEkID = DuyuruDosyaEkID.Select((s, inx) => new { s, inx }).ToList();
            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             select new { dek.inx, DosyaEkAdi = dek.s, Dosya = de.s }).ToList();

            var qVarolanlar = (from s in qDosyaEkAdi
                               join sid in qDuyuruDosyaEkID on s.inx equals sid.inx
                               select new { s.inx, DosyaEkAdi = s.s, DuyuruDosyaEkID = sid.s });
            #region Kontrol
            
            if (kModel.Tarih == DateTime.MinValue)
            { 
                MmMessage.Messages.Add("Geçerli Bir Tarih Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tarih" });
            }
            else
            {
                if (kModel.YayinSonTarih.HasValue)
                {
                    if (kModel.YayinSonTarih.Value <= kModel.Tarih)
                    { 
                        MmMessage.Messages.Add("Duyurunun yayınlanacağı son tarih Duyuru tarihinden tarihten küçük yada eşit olamaz! ");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tarih"  });
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YayinSonTarih"  });
                    }
                }
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tarih" });
            }
            if (kModel.Baslik.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Başlık Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Baslik" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Baslik" });

            if (kModel.Aciklama.IsNullOrWhiteSpace() && kModel.AciklamaHtml.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Aciklama Giriniz.");
            } 
            #endregion
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = kModel.Aciklama ?? "";
                if (kModel.DuyuruID <= 0)
                {
                    kModel.IsAktif = true;
                    var eklenen = db.Duyurulars.Add(kModel);

                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        db.DuyuruEkleris.Add(new DuyuruEkleri
                        {
                            DuyuruID = eklenen.DuyuruID,
                            DosyaEkAdi = item.DosyaEkAdi,
                            DosyaYolu = DosyaYolu
                        });
                    }
                }
                else
                {
                    var data = db.Duyurulars.Where(p => p.DuyuruID == kModel.DuyuruID).First();
                    data.Baslik = kModel.Baslik;
                    data.Aciklama = kModel.Aciklama;
                    data.AciklamaHtml = kModel.AciklamaHtml;
                    data.Tarih = kModel.Tarih;
                    data.YayinSonTarih = kModel.YayinSonTarih;
                    data.AnaSayfadaGozuksun = kModel.AnaSayfadaGozuksun;
                    data.AnaSayfaPopupAc = kModel.AnaSayfaPopupAc; 
                    data.IsAktif = kModel.IsAktif;
                    data.IslemTarihi = DateTime.Now;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;

                    var SilinenDuyuruEkleri = db.DuyuruEkleris.Where(p => DuyuruDosyaEkID.Contains(p.DuyuruDosyaEkID) == false && p.DuyuruID == data.DuyuruID).ToList();
                    var VarolanDuyuruEkleri = db.DuyuruEkleris.Where(p => DuyuruDosyaEkID.Contains(p.DuyuruDosyaEkID) && p.DuyuruID == data.DuyuruID).ToList();
                    foreach (var item in VarolanDuyuruEkleri)
                    {
                        var qd = qVarolanlar.Where(p => p.DuyuruDosyaEkID == item.DuyuruDosyaEkID).FirstOrDefault();
                        if (qd != null)
                        {
                            item.DosyaEkAdi = qd.DosyaEkAdi;
                        }
                    }
                    db.DuyuruEkleris.RemoveRange(SilinenDuyuruEkleri);
                    foreach (var item in qDosyalar)
                    {
                        var dosyaTipi = item.Dosya.FileName.Split('.').Last();
                        var DosyaAdi = item.Dosya.FileName.Replace('.' + dosyaTipi, "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + dosyaTipi);
                        string DosyaYolu = "/DuyuruDosyalari/" + DosyaAdi;
                        item.Dosya.SaveAs(Server.MapPath("~" + DosyaYolu));

                        db.DuyuruEkleris.Add(new DuyuruEkleri
                        {
                            DuyuruID = data.DuyuruID,
                            DosyaEkAdi = item.DosyaEkAdi,
                            DosyaYolu = DosyaYolu
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
            var kayit = db.Duyurulars.Where(p => p.DuyuruID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Baslik + "' Başlıklı Duyuru Silindi!";
                    var dosyalar = kayit.DuyuruEkleris.ToList();

                    db.Duyurulars.Remove(kayit);
                    db.SaveChanges();
                    foreach (var item in dosyalar)
                    {
                        System.IO.File.Delete(Server.MapPath("~" + item.DosyaYolu));
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Baslik + "' Başlıklı Duyuru! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "Duyurular/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Duyuru sistemde bulunamadı!";
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
