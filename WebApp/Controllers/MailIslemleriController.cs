using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using Database;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.MailIslemleri)]
    public class MailIslemleriController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmMailGonderme() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmMailGonderme model)
        {
            var q = from s in db.GonderilenMaillers
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    where s.Silindi == false  
                    select new
                    {
                        s.GonderilenMailID,
                        s.Tarih, 
                        s.Konu,
                        s.Aciklama,
                        s.AciklamaHtml,
                        MailGonderen = k.Ad + " " + k.Soyad,
                        EkSayisi = s.GonderilenMailEkleris.Count,
                        KisiSayisi = s.GonderilenMailKullanicilars.Count,
                        s.Gonderildi,
                        s.HataMesaji
                    };

            if (!model.Konu.IsNullOrWhiteSpace()) q = q.Where(p => p.Konu.Contains(model.Konu));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));
            if (!model.MailGonderen.IsNullOrWhiteSpace()) q = q.Where(p => p.MailGonderen.Contains(model.MailGonderen));
            if (model.Tarih.HasValue)
            {
                var trih = model.Tarih.Value.TodateToShortDate();
                q = q.Where(p => p.Tarih == trih);

            }
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(o => o.Tarih);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).Select(s => new FrMailGonderme
            {
                GonderilenMailID = s.GonderilenMailID,
                Tarih = s.Tarih,
                Konu = s.Konu,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                MailGonderen = s.MailGonderen,
                KisiSayisi = s.KisiSayisi,
                EkSayisi = s.EkSayisi,
                Gonderildi = s.Gonderildi,
                HataMesaji = s.HataMesaji

            }).ToList();
            return View(model);
        }
        public ActionResult MailDetay(int GonderilenMailID)
        {

            var data = (from s in db.GonderilenMaillers
                        join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                        where s.GonderilenMailID == GonderilenMailID
                        select new FrMailGonderme
                        {
                            GonderilenMailID = s.GonderilenMailID,
                            Tarih = s.Tarih,
                            Konu = s.Konu,
                            Aciklama = s.Aciklama,
                            AciklamaHtml = s.AciklamaHtml,
                            MailGonderen = k.Ad + " " + k.Soyad,
                            IslemYapanIP = s.IslemYapanIP,
                            EkSayisi = s.GonderilenMailEkleris.Count,
                            KisiSayisi = s.GonderilenMailKullanicilars.Count,
                            GonderilenMailEkleris = s.GonderilenMailEkleris.ToList()

                        }).First();
            var dataK = (from s in db.GonderilenMailKullanicilars
                         orderby s.Kullanicilar.Ad, s.Kullanicilar.Soyad
                         where s.GonderilenMailID == GonderilenMailID
                         select new MailKullaniciBilgi
                         {
                             AdSoyad = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                             Email = s.Email
                         }).ToList();
            ViewBag.DataK = dataK;
            return View(data);
        }

        public ActionResult Gonder(int? id, List<int> KullaniciID, string EKD, string dlgid)
        {
            var model = new GonderilenMailler();
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;

            var dataK = (from s in db.GonderilenMailKullanicilars
                         orderby s.Kullanicilar.Ad, s.Kullanicilar.Soyad
                         select new MailKullaniciBilgi
                         {
                             AdSoyad = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                             Email = s.Email
                         }).ToList();



            ViewBag.Kullanicilar = dataK;
            ViewBag.SelectedTab = 1;
            ViewBag.Alici = "";
            var eList = new List<ComboModelInt>();
            KullaniciID = KullaniciID ?? new List<int>();
            db.Kullanicilars.Where(p => KullaniciID.Contains(p.KullaniciID)).ToList().ForEach((k) => { eList.Add(new ComboModelInt { Value = k.KullaniciID, Caption = k.EMail }); });

            ViewBag.MailSablonlariID = new SelectList(Management.CmbMailSablonlari(true, false), "Value", "Caption");
            ViewBag.EmailList = eList;
            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Gonder(GonderilenMailler kModel, List<string> DosyaEkiAdi, List<HttpPostedFileBase> DosyaEki, List<int?> DuyuruDosyaEkID, List<int> KullaniciIDs, string EKD, string Alici = "", string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            DuyuruDosyaEkID = DuyuruDosyaEkID == null ? new List<int?>() : DuyuruDosyaEkID;
            DosyaEki = DosyaEki == null ? new List<HttpPostedFileBase>() : DosyaEki;
            DosyaEkiAdi = DosyaEkiAdi == null ? new List<string>() : DosyaEkiAdi;
            KullaniciIDs = KullaniciIDs == null ? new List<int>() : KullaniciIDs;
            var secilenAlicilar = new List<string>();
            if (Alici.IsNullOrWhiteSpace() == false) Alici.Split(',').ToList().ForEach((itm) => { secilenAlicilar.Add(itm); });
            var qDosyaEkAdi = DosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = DosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDuyuruDosyaEkID = DuyuruDosyaEkID.Select((s, inx) => new { s, inx }).ToList();
            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             select new
                             {
                                 dek.inx,
                                 DosyaEkAdi = dek.s,
                                 Dosya = de.s,
                                 mDosyaAdi = dek.s.Replace(".", "") + "." + de.s.FileName.Split('.').Last(),
                                 DosyaYolu = "/MailDosyalari/" + dek.s + "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + de.s.FileName.Split('.').Last()
                             }).ToList();

            var qVarolanlar = (from s in qDosyaEkAdi
                               join sid in qDuyuruDosyaEkID on s.inx equals sid.inx
                               select new { s.inx, DosyaEkAdi = s.s, DuyuruDosyaEkID = sid.s });
            #region Kontrol
            kModel.Tarih = DateTime.Now;

            if (kModel.Konu.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Konu Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Konu" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Konu" });

            if (kModel.Aciklama.IsNullOrWhiteSpace() && kModel.AciklamaHtml.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("İçerik Giriniz.");
            }


            #endregion
            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now; 
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = kModel.Aciklama ?? "";
                var eklenen = db.GonderilenMaillers.Add(kModel);

                foreach (var item in qDosyalar)
                {
                    item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    db.GonderilenMailEkleris.Add(new GonderilenMailEkleri
                    {

                        GonderilenMailID = eklenen.GonderilenMailID,
                        EkAdi = item.DosyaEkAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }
                var mailList = new List<GonderilenMailKullanicilar>();
                var tari = DateTime.Now;

                if (secilenAlicilar.Count > 0)
                {
                    var qscIDs = secilenAlicilar.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
                    var qscMails = secilenAlicilar.Where(p => p.IsNumber() == false).ToList();
                    var dataqx = (from s in db.Kullanicilars
                                  where qscIDs.Contains(s.KullaniciID)
                                  select new
                                  {
                                      Email = s.EMail,
                                      GonderilenMailID = eklenen.GonderilenMailID,
                                      KullaniciID = s.KullaniciID
                                  }).ToList();
                    foreach (var item in dataqx)
                    {
                        mailList.Add(new GonderilenMailKullanicilar
                        {
                            Email = item.Email,
                            GonderilenMailID = item.GonderilenMailID,
                            KullaniciID = item.KullaniciID
                        });
                    }
                    foreach (var item in qscMails)
                    {
                        mailList.Add(new GonderilenMailKullanicilar
                        {
                            Email = item,
                            GonderilenMailID = eklenen.GonderilenMailID,
                            KullaniciID = null
                        });
                    }
                }
                mailList = db.GonderilenMailKullanicilars.AddRange(mailList).ToList();
                db.SaveChanges();
                var attach = new List<Attachment>();

                foreach (var item in qDosyalar)
                {

                    attach.Add(new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~" + item.DosyaYolu))), item.mDosyaAdi, MediaTypeNames.Application.Octet));
                }
                MailManager.sendMail(eklenen.GonderilenMailID, kModel.Konu, kModel.AciklamaHtml, mailList.Select(s => s.Email).ToList(), attach);
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;


            var qKullanicilar = from k in db.Kullanicilars
                                join bi in db.Birimlers on k.BirimID equals bi.BirimID
                                where k.EMail.Contains("@")
                                orderby k.Ad, k.Soyad
                                select new MailKullaniciBilgi
                                {
                                    KullaniciID = k.KullaniciID,
                                    AdSoyad = k.Ad + " " + k.Soyad,
                                    BirimAdi = bi.BirimAdi,
                                    Email = k.EMail

                                };
            var kul = qKullanicilar.ToList();
            foreach (var item in kul)
            {
                if (KullaniciIDs.Contains(item.KullaniciID)) item.Checked = true;
            }

            ViewBag.Kullanicilar = kul;
            ViewBag.Alici = Alici;
            ViewBag.MailSablonlariID = new SelectList(Management.CmbMailSablonlari(true, false), "Value", "Caption");
            return View(kModel);
        }

         
        public ActionResult TekrarGonder(int id)
        {
            var gm = db.GonderilenMaillers.Where(p => p.GonderilenMailID == id).FirstOrDefault();
            var gP = gm.GonderilenMailKullanicilars.ToList();
            var gEk = gm.GonderilenMailEkleris.ToList();
            var attach = new List<Attachment>();
            foreach (var item in gEk)
            {
                attach.Add(new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~" + item.EkDosyaYolu))), item.EkAdi, MediaTypeNames.Application.Octet));

            }
            MailManager.sendMail(gm.GonderilenMailID, gm.Konu, gm.AciklamaHtml, gP.Select(s => s.Email).ToList(), attach);
            return true.ToJsonResult();
        }



        public ActionResult Sil(int id)
        {
            var kayit = db.GonderilenMaillers.Where(p => p.GonderilenMailID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    //var dosyalar = kayit.GonderilenMailEkleris.ToList();
                    //foreach (var item in dosyalar)
                    //{
                    //    if (success == true)
                    //    {
                    //        try
                    //        {
                    //            System.IO.File.Delete(Server.MapPath("~" + item.EkDosyaYolu));
                    //        }
                    //        catch (Exception exM)
                    //        {
                    //            message = exM.ToExceptionMessage().Replace("\r\n", "<br/>");
                    //            success = false;
                    //        }
                    //    }
                    //}
                    if (message == "")
                    {
                        kayit.Silindi = true;

                        //db.GonderilenMaillers.Remove(kayit);
                        db.SaveChanges();
                        message = "'" + kayit.Konu + "' konulu email Silindi!";
                    }

                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Konu + "' Konulu Mail Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "MailGonder/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz mail bilgisi sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

    }
}
