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
    [Authorize(Roles = RoleNames.Mesajlar)]
    public class MesajlarController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmMesajlar() { PageSize = 15, Expand = true });
        }
        [HttpPost]
        public ActionResult Index(FmMesajlar model)
        {
            var q = from s in db.Mesajlars.Where(p => p.UstMesajID.HasValue == false)
                    join mk in db.MesajKategorileris on s.MesajKategoriID equals mk.MesajKategoriID
                    join k in db.Kullanicilars on s.KullaniciID equals k.KullaniciID into defK
                    from kul in defK.DefaultIfEmpty()
                    where s.Silindi == false
                    select new
                    {
                        s.MesajKategoriID,
                        mk.KategoriAdi,
                        s.MesajID,
                        Tarih = s.Mesajlar1.Any() ? s.Mesajlar1.OrderByDescending(s2 => s2.Tarih).FirstOrDefault().Tarih : s.Tarih,
                        s.Konu,
                        s.Email,
                        s.Aciklama,
                        s.AciklamaHtml,
                        s.AdSoyad,
                        s.IslemYapanIP,
                        Kullanici = kul,
                        EkSayisi = s.MesajEkleris.Count,
                        Ekler = s.MesajEkleris,
                        IsAktif = s.IsAktif,
                        s.KullaniciID,
                        s.Mesajlar1
                    };

            if (model.MesajKategoriID.HasValue) q = q.Where(p => p.MesajKategoriID == model.MesajKategoriID.Value);
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.Konu.Contains(model.Konu) || p.Aciklama.Contains(model.Konu) || p.AdSoyad.Contains(model.AdSoyad) || p.Email.Contains(model.AdSoyad));
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
            IndexModel.Pasif = IndexModel.Toplam - IndexModel.Aktif;
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.IsAktif).ThenByDescending(t => t.Tarih);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).Select(s => new FrMesajlar
            {
                MesajKategoriID = s.MesajKategoriID,
                KategoriAdi = s.KategoriAdi,
                MesajID = s.MesajID,
                Konu = s.Konu,
                Email = s.Email,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                Tarih = s.Tarih,
                AdSoyad = s.AdSoyad,
                KullaniciID = s.KullaniciID ?? 0,
                Kullanici = s.Kullanici,
                IslemYapanIP = s.IslemYapanIP,
                IsAktif = s.IsAktif

            }).ToList();
            foreach (var item in model.Data)
            {
                if (item.KullaniciID > 0)
                {
                    var kul = item.Kullanici;
                    item.BirimAdi = db.sp_BirimAgaciGetBr(kul.BirimID).FirstOrDefault().BirimTreeAdi;
                }
            }
            ViewBag.MesajKategoriID = new SelectList(Management.CmbMesajKategorileri(true), "Value", "Caption", model.MesajKategoriID);
            ViewBag.IndexModel = IndexModel;
            ViewBag.IsAktif = new SelectList(Management.CmbAcikKapaliData(true), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult DurumKayit(int id, bool IsAktif, bool? MainFilter)
        {
            var kayit = db.Mesajlars.Where(p => p.MesajID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            try
            {
                message = "'" + kayit.Konu + "' Konulu Mesaj durumu " + (!IsAktif ? "Açık" : "Kapalı") + " olarak İşaretlendi";
                kayit.IsAktif = IsAktif;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                success = false;
                message = "'" + kayit.Konu + "'Konulu Mesaj Durumu Güncellenemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                Management.SistemBilgisiKaydet(message, "Mesajlar/DurumKayit<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAcikMsjCount()
        {
            var model = Management.GetCevaplanmamisMesajCount();
            return new { mCount = model.Value.Value, HtmlContent = model.Caption }.ToJsonResult();
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.Mesajlars.Where(p => p.MesajID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Konu + "' Konulu Mesaj Silindi!";
                    // var dosyalar = kayit.MesajEkleris.ToList();
                    kayit.Silindi = true;
                    var mails = db.GonderilenMaillers.Where(p => p.MesajID == kayit.MesajID).ToList();
                    foreach (var item in mails)
                    {
                        item.Silindi = true;
                    }
                    // db.Mesajlars.Remove(kayit);
                    db.SaveChanges();
                    //foreach (var item in dosyalar)
                    //{
                    //    System.IO.File.Delete(Server.MapPath("~" + item.EkDosyaYolu));
                    //}
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Konu + "'Konulu Mesaj Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "Mesajlar/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Mesaj sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult getMesajDetay(int MesajID)
        {
            var mesaj = db.Mesajlars.Where(p => p.UstMesajID.HasValue == false && p.MesajID == MesajID).Select(s => new FrMesajlar
            {
                MesajKategoriID = s.MesajKategoriID,
                MesajID = s.MesajID,
                Konu = s.Konu,
                KullaniciID = s.KullaniciID,
                Email = s.Email,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                Tarih = s.Tarih,
                AdSoyad = s.AdSoyad,
                Kullanici = s.Kullanicilar,
                IslemYapanIP = s.IslemYapanIP,
                MesajEkleris = s.MesajEkleris.ToList()
            }).First();

            var groupMesajs = db.Mesajlars.Where(p => p.UstMesajID == mesaj.MesajID).ToList().Select(s => new SubMessages
            {
                MesajID = s.MesajID,
                KullaniciID = s.KullaniciID ?? 0,
                EMail = s.Email,
                AdSoyad = s.AdSoyad,
                Tarih = s.Tarih,
                Icerik = s.AciklamaHtml,
                ResimYolu = s.KullaniciID.HasValue ? s.Kullanicilar.ResimAdi : "",
                IslemYapanIP = s.IslemYapanIP,
                Gonderilenler = new List<GonderilenMailKullanicilar>(),
                Ekler = s.MesajEkleris.ToList()


            }).ToList();
            groupMesajs.Add(new SubMessages
            {
                MesajID = mesaj.MesajID,
                KullaniciID = mesaj.KullaniciID ?? 0,
                EMail = mesaj.Email,
                AdSoyad = mesaj.AdSoyad,
                Tarih = mesaj.Tarih,
                Icerik = mesaj.AciklamaHtml,
                ResimYolu = mesaj.Kullanici != null ? mesaj.Kullanici.ResimAdi : "",
                IslemYapanIP = mesaj.IslemYapanIP,
                Gonderilenler = new List<GonderilenMailKullanicilar>(),
                Ekler = mesaj.MesajEkleris.Select(s => new MesajEkleri { EkAdi = s.EkAdi, EkDosyaYolu = s.EkDosyaYolu }).ToList(),
            });
            var gMesajs = db.GonderilenMaillers.Where(p => p.MesajID == mesaj.MesajID).ToList();
            foreach (var item in gMesajs)
            {
                var kul = item.Kullanicilar;
                groupMesajs.Add(new SubMessages
                {
                    MesajID = item.MesajID.Value,
                    KullaniciID = kul.KullaniciID,
                    EMail = kul.EMail,
                    AdSoyad = kul.Ad + " " + kul.Soyad,
                    Tarih = item.Tarih,
                    Ekler = item.GonderilenMailEkleris.Select(s2 => new MesajEkleri { MesajEkiID = 1, EkAdi = s2.EkAdi, EkDosyaYolu = s2.EkDosyaYolu }).ToList(),
                    Icerik = item.AciklamaHtml,
                    ResimYolu = kul.ResimAdi,
                    Gonderilenler = item.GonderilenMailKullanicilars.ToList(),
                    IslemYapanIP = item.IslemYapanIP,

                });
            }
            mesaj.SubMesajList = groupMesajs.OrderByDescending(o => o.Tarih).ToList();
            return View(mesaj);
        }

    }
}