using DevExpress.Web.Mvc;
using BiskaUtil;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using Database;
using System.Web.Script.Serialization;
using DevExpress.XtraReports.UI;
using WebApp.Raporlar;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AjaxController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult GetThemeSetting()
        {
            var k = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).Select(s => new
            {
                s.FixedHeader,
                s.FixedSidebar,
                s.ScrollSidebar,
                s.RightSidebar,
                s.CustomNavigation,
                s.ToggledNavigation,
                s.BoxedOrFullWidth,
                s.ThemeName,
                s.BackgroundImage
            }).FirstOrDefault();
            return Json(k, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetThemeSetting(string columnName, string value)
        {
            var k = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).FirstOrDefault();
            if (columnName == "st_head_fixed") k.FixedHeader = value.ToBoolean().Value;
            if (columnName == "st_sb_fixed") k.FixedSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_scroll") k.ScrollSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_right") k.RightSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_custom") k.CustomNavigation = value.ToBoolean().Value;
            if (columnName == "st_sb_toggled") k.ToggledNavigation = value.ToBoolean().Value;
            if (columnName == "st_layout_boxed") k.BoxedOrFullWidth = value.ToBoolean().Value;
            if (columnName == "ThemeName") k.ThemeName = value;
            if (columnName == "BackgroundImage") k.BackgroundImage = value;
            db.SaveChanges();
            if (columnName == "st_head_fixed") UserIdentity.Current.Informations["FixedHeader"] = value.ToBoolean().Value;
            if (columnName == "st_sb_fixed") UserIdentity.Current.Informations["FixedSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_scroll") UserIdentity.Current.Informations["ScrollSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_right") UserIdentity.Current.Informations["RightSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_custom") UserIdentity.Current.Informations["CustomNavigation"] = value.ToBoolean().Value;
            if (columnName == "st_sb_toggled") UserIdentity.Current.Informations["ToggledNavigation"] = value.ToBoolean().Value;
            if (columnName == "st_layout_boxed") UserIdentity.Current.Informations["BoxedOrFullWidth"] = value.ToBoolean().Value;
            if (columnName == "ThemeName") UserIdentity.Current.Informations["ThemeName"] = value;
            if (columnName == "BackgroundImage") UserIdentity.Current.Informations["BackgroundImage"] = value;
            return Json("true", "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoginControl(string UserName, string Password, string captchaRequestCode, string CaptchaText, bool? RememberMe, string ReturnUrl, string dlgId)
        {

            var MmMessage = new AjaxLoginModel
            {
                ReturnUrl = ReturnUrl,
                UserName = UserName,
                Password = Password
            };
            RememberMe = RememberMe ?? false;

            string Hata = null;
            var xCaptchaxText = CaptchaImageRequests.GetCode(captchaRequestCode);
            var yCaptchaText = CaptchaImageRequests.GetCode(Session.SessionID);
            try
            {
                if (UserName.IsNullOrWhiteSpace())
                {
                    Hata = "Kullanıcı Adı Giriniz";
                }
                else if (Password.IsNullOrWhiteSpace())
                {
                    Hata = "Şifre Giriniz";
                }
                else if (CaptchaText.IsNullOrWhiteSpace())
                {
                    Hata = "Resimdeki Karakterleri Giriniz";
                }
                else if (!(xCaptchaxText == CaptchaText || yCaptchaText == CaptchaText))
                {
                    Hata = "Resimdeki Karakterleri Hatalı Girdiniz";
                }
                else
                {
                    string msg = "";
                    var user = Management.GetUser(UserName);
                    Kullanicilar loginUser = null;
                    if (user != null)
                    {
                        if (user.IsActiveDirectoryUser == false)
                        {
                            loginUser = Management.Login(UserName, Password);
                        }
                        else
                        {
                            LdapService.SecureSoapClient ld = new LdapService.SecureSoapClient();
                            var WsPwd = ConfigurationManager.AppSettings["ldapServicePassword"];
                            var IsSuccess = ld.Login(UserName, Password, WsPwd);
                            if (IsSuccess)
                            {
                                loginUser = user;
                            }
                            else
                            {
                                MmMessage.IsSuccess = false;
                                msg = "Active Directory Kontrolünden Geçilemedi!";
                                Management.SistemBilgisiKaydet("Active Directory Kontrolünden Geçilemedi! Kullanıcı Adı: " + UserName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                            }
                        }

                        if (loginUser != null && !loginUser.IsAktif)
                        {
                            Hata = "Kullanıcı Hesabı Pasif Durumda!";
                            MmMessage.IsSuccess = false;
                        }
                        else if (loginUser == null)
                        {
                            Hata = "Kullanıcı Adı veya Şifre Hatalı. " + msg;
                            MmMessage.IsSuccess = false;
                        }
                        else
                        {
                            MmMessage.IsSuccess = true;
                        }
                    }
                    else
                    {
                        MmMessage.IsSuccess = false;
                        Management.SistemBilgisiKaydet("Kullanıcı Sistemde Bulunamadı! Kullanıcı Adı: " + UserName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                        Hata = "Kullanıcı sistemde bulunamadı.";
                    }
                }

            }
            catch (Exception ex)
            {
                MmMessage.IsSuccess = false;
                Hata = "Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage();
            }
            MmMessage.Message = Hata;
            if (MmMessage.IsSuccess == false)
            {
                MmMessage.NewGuid = Guid.NewGuid().ToString().Replace("-", "");
                MmMessage.NewSrc = Url.Action("generatecaptcha", "Account", new { captchaRequestCode = MmMessage.NewGuid });

            }
            else
            {
                FormsAuthenticationUtil.SetAuthCookie(UserName, "", RememberMe.Value);
            }
            return MmMessage.ToJsonResult();
        }

        public ActionResult SignOut(string ReturnUrl)
        {
            var MmMessage = new AjaxLoginModel();

            if (UserIdentity.Current.IsAuthenticated)
            {
                var kulID = UserIdentity.Current.Id;
                var kul = db.Kullanicilars.Where(p => p.KullaniciID == kulID).First();
                kul.LastLogonDate = DateTime.Now;
                db.SaveChanges();
                FormsAuthenticationUtil.SignOut();
            }

            if (ReturnUrl.IsNullOrWhiteSpace()) MmMessage.ReturnUrl = Url.Action("Index", "Home");
            else MmMessage.ReturnUrl = ReturnUrl;
            MmMessage.IsSuccess = true;
            return MmMessage.ToJsonResult();
        }
        [Authorize]
        public ActionResult YetkiYenile(string ReturnUrl)
        {
            var MmMessage = new MmMessage();

            if (UserIdentity.Current.IsAuthenticated)
            {
                var userIdentity = Management.GetUserIdentity(UserIdentity.Current.Name);
                userIdentity.Impersonate();
                Session["UserIdentity"] = userIdentity;
                MmMessage.Messages.Add("Yetkileriniz yeniden yiklenmiştir.");
            }

            if (ReturnUrl.IsNullOrWhiteSpace()) MmMessage.ReturnUrl = Url.Action("Index", "Home");
            else MmMessage.ReturnUrl = ReturnUrl;
            MmMessage.IsSuccess = true;
            return MmMessage.ToJsonResult();
        }
        [HttpGet]
        public ActionResult GetKullaniciDetay(int KullaniciID)
        {

            if (!(RoleNames.Kullanicilar.InRole() == true)) KullaniciID = UserIdentity.Current.Id;
            var data = Management.GetUser(KullaniciID);
            ViewBag.ResimVar = data.ResimAdi.IsNullOrWhiteSpace() == false;
            ViewBag.KullaniciTipAdi = db.YetkiGruplaris.Where(p => p.YetkiGrupID == data.YetkiGrupID).First().YetkiGrupAdi;
            data.ResimAdi = data.ResimAdi.ToKullaniciResim();

            var userRoles = Management.GetUserRoles(KullaniciID);

            ViewBag.KRoller = userRoles;
            return View(data);
        }

        public ActionResult SifreResetle(string MailAddress)
        {

            var MmMessage = new MmMessage();

            if (MailAddress.IsNullOrWhiteSpace() || MailAddress.ToIsValidEmail())
            {
                MmMessage.IsSuccess = false;
                MmMessage.Title = "Girdiğiniz mail mail formatına uygun değildir. Lütfen kontrol ediniz.";
            }
            else
            {
                var kul = db.Kullanicilars.Where(p => p.EMail.Equals(MailAddress)).FirstOrDefault();
                if (kul == null)
                {
                    MmMessage.IsSuccess = false;
                    MmMessage.Title = "Girdiğiniz mail sistem üzerinde kayıtlı herhangi bir kullanıcı ile eşleşmemektedir.";
                }
                else
                {
                    if (kul.IsActiveDirectoryUser)
                    {
                        MmMessage.IsSuccess = false;
                        MmMessage.Title = "Girdiğiniz mail " + kul.KullaniciAdi + " kullanıcısı ile eşleşmiştir. Fakat eşleşen kullanıcı için şifre kabul türü Active Directory (EBSY, USIS Şifre kabul) sistemine entegre edilmiştir. Bu tarz eşleştirme yapılan kullanıcıların şifreleri 'Performans.yildiz.edu.tr' sistemi tarafından değiştirilemez.";
                    }
                    else
                    {
                        var ErisimAdresi = SistemAyar.getAyar(SistemAyar.AyarSistemErisimAdresi);
                        var mRowModel = new List<mailTableRow>();
                        DateTime gecerlilikTarihi = DateTime.Now.AddHours(2);
                        string guid = Guid.NewGuid().ToString().Substring(0, 20);
                        mRowModel.Add(new mailTableRow { Baslik = "Şifre Sıfırlama Linki", Aciklama = "<a target='_blank' href='" + ErisimAdresi + "/Account/ParolaSifirla?psKod=" + guid + "'> Şifrenizi sıfırlamak için tıklayınız </a>" });
                        mRowModel.Add(new mailTableRow { Baslik = "Link Geçerlilik Tarihi", Aciklama = "Yukarıdaki link '" + gecerlilikTarihi.ToFormatDateAndTime() + "' tarihine kadar geçerlidir." });

                        var mmmC = new mdlMailMainContent();
                        mmmC.UniversiteAdi = "Yıldız Teknik Üniversitesi";
                        var _ea = ErisimAdresi;
                        var WurlAddr = _ea.Split('/').ToList();
                        if (_ea.Contains("//"))
                            _ea = WurlAddr[0] + "//" + WurlAddr.Skip(2).Take(1).First();
                        else
                            _ea = "http://" + WurlAddr.First();
                        mmmC.LogoPath = _ea + "/Content/assets/images/ytu_logo_tr.png";
                        var mtc = new mailTableContent();
                        mtc.AciklamaBasligi = "Şifre Sıfırlama İşlemi";
                        mtc.AciklamaDetayi = "Şifrenizi sıfırlamak için aşağıda bulunan linke tıklayınız ve açılan sayfa da yeni şifrenizi tanımlayınız.";
                        mtc.Detaylar = mRowModel;
                        var tavleContent = Management.RenderPartialView("Ajax", "getMailTableContent", mtc);
                        mmmC.Content = tavleContent;

                        string htmlMail = Management.RenderPartialView("Ajax", "getMailContent", mmmC);
                        var lstMail = new List<string>();
                        lstMail.Add(kul.EMail);
                        var rtVal = MailManager.sendMailRetVal("Şifre Sıfırlama İşlemi", htmlMail, lstMail, new List<System.Net.Mail.Attachment> { });
                        if (rtVal == null)
                        {
                            MmMessage.IsSuccess = true;
                            MmMessage.Title = "Şifre sıfırlama linki '" + kul.EMail + "' adresine gönderilmiştir!";
                            kul.ParolaSifirlamaKodu = guid;
                            kul.ParolaSifirlamGecerlilikTarihi = gecerlilikTarihi;
                            db.SaveChanges();
                        }
                        else
                        {
                            MmMessage.IsSuccess = false;
                            Management.SistemBilgisiKaydet("Şifre sıfırlama! Hata: " + rtVal.ToExceptionMessage(), rtVal.ToExceptionStackTrace(), BilgiTipi.Hata, kul.KullaniciID, UserIdentity.Ip);
                            MmMessage.Title = "Şifre sıfırlama linki '" + kul.EMail + "' adresine gönderilemedi!";
                        }
                    }
                }

            }
            return MmMessage.ToJsonResult();
        }

        [Authorize]
        public ActionResult RotateImage(bool LeftOrRight, int KullaniciID)
        {
            if (RoleNames.KullaniciKayit.InRole() == false) KullaniciID = UserIdentity.Current.Id;
            var user = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).First();
            string folname = SistemAyar.getAyar(SistemAyar.KullaniciResimYolu);
            if (user.ResimAdi.IsNullOrWhiteSpace() == false)
            {
                var ImgPath = folname + "/" + user.ResimAdi;
                string pth = Server.MapPath(Management.GetRoot() + ImgPath);

                using (Image img = Image.FromFile(pth))
                {
                    img.RotateFlip(LeftOrRight ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone);
                    //  var format = (System.Drawing.Imaging.ImageFormat)img.RawFormat;
                    img.Save(pth, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

            }
            return new { ResimAdi = folname + "/" + user.ResimAdi }.ToJsonResult();
        }

        [Authorize]
        public ActionResult getImageUpload(int KullaniciID)
        {
            if (RoleNames.KullaniciKayit.InRole() == false) KullaniciID = UserIdentity.Current.Id;
            var kullanici = Management.GetUser(KullaniciID);
            return View(kullanici);
        }
        [Authorize]
        public ActionResult getImageUploadPost(int KullaniciID, HttpPostedFileBase KProfilResmi)
        {
            var mMessage = new MmMessage();
            string YeniResim = "";
            mMessage.Title = "Profil resmi yükleme işlemi başarısız";
            mMessage.IsSuccess = false;
            mMessage.MessageType = Msgtype.Warning;
            bool AnaResmiDegistir = false;
            if (KProfilResmi == null || KProfilResmi.ContentLength <= 0)
            {
                mMessage.Messages.Add("Lütfen Resim Seçiniz.");
                // MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "ProfilResmi", Message = msg });
            }
            else if (RoleNames.KullaniciKayit.InRole() == false && KullaniciID != UserIdentity.Current.Id)
            {
                mMessage.Messages.Add("Başka bir kullanıcı adına resim yüklemesi yapmaya yetkili değilsiniz.");
            }
            else
            {
                var contentlength = KProfilResmi.ContentLength;
                string uzanti = System.IO.Path.GetExtension(KProfilResmi.FileName);
                if ((uzanti == ".jpg" || uzanti == ".JPG" || uzanti == ".jpeg" || uzanti == ".JPEG" || uzanti == ".png" || uzanti == ".PNG" || uzanti == ".bmp" || uzanti == ".BMP") == false)

                {
                    mMessage.Messages.Add("Ekleyeceğiniz resim '.jpg, .JPG, .jpeg, .JPEG, .png, .PNG, .bmp, .BMP' formatlarından biri olmalıdır! ");
                }
                else if (contentlength > 2048000)
                {
                    mMessage.Messages.Add("Ekleyeceğiniz resim maksimum 2MB boyutunda olmalıdır! ");
                }
                else
                {
                    var kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).First();
                    var eskiResim = kul.ResimAdi;
                    var resimBilgi = Management.ResimKaydet(KProfilResmi);
                    kul.ResimAdi = YeniResim = resimBilgi.Caption;
                    kul.IslemYapanID = UserIdentity.Current.Id;
                    kul.IslemYapanIP = UserIdentity.Ip;
                    kul.IslemTarihi = DateTime.Now;
                    db.SaveChanges();
                    mMessage.Title = "Progil Resmi başarılı bir şekilde yüklenmiştir.";
                    mMessage.IsSuccess = true;
                    mMessage.MessageType = Msgtype.Success;
                    if (KullaniciID == UserIdentity.Current.Id)
                    {
                        AnaResmiDegistir = true;
                        var userIdentity = Management.GetUserIdentity(UserIdentity.Current.Name);
                        userIdentity.Impersonate();
                        Session["UserIdentity"] = userIdentity;
                    }
                    if (eskiResim.IsNullOrWhiteSpace() == false)
                    {
                        var rsmYol = SistemAyar.getAyar(SistemAyar.KullaniciResimYolu);
                        var rsm = Server.MapPath("~/" + rsmYol + "/" + eskiResim);
                        if (System.IO.File.Exists(rsm)) System.IO.File.Delete(rsm);
                    }
                }
            }
            return new { mMessage = mMessage, ResimAdi = YeniResim.ToKullaniciResim(), AnaResmiDegistir = AnaResmiDegistir }.ToJsonResult();
        }
        public ActionResult GetMessage(MmMessage model)
        {
            return View(model);
        }

        public ActionResult GetMailContent(mdlMailMainContent model)
        {
            return View(model);
        }

        public ActionResult GetMailTableContent(mailTableContent model)
        {
            return View(model);
        }




        [Authorize(Roles = RoleNames.MailIslemleri)]
        [ValidateInput(false)]
        public ActionResult MailGonder(List<string> KullaniciID, string SetKonu, string SetAciklama, int? id, string dlgid, bool Bsonuc = false, bool TopluMail = false, bool toOrBcc = false)
        {

            var model = new GonderilenMailler
            {
                MesajID = id,

            };

            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            if (TopluMail)
            {
                ViewBag.strAlicis = KullaniciID;
                KullaniciID = new List<string>();

            }


            var eList = new List<ComboModelString>();
            KullaniciID = KullaniciID ?? new List<string>();
            var Ids = KullaniciID.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
            var mails = KullaniciID.Where(p => p.IsNumber() == false).Select(s => s).ToList();
            db.Kullanicilars.Where(p => Ids.Contains(p.KullaniciID)).ToList().ForEach((k) => { eList.Add(new ComboModelString { Value = k.KullaniciID.ToString(), Caption = k.EMail }); });
            foreach (var item in mails.Where(p => p != ""))
            {
                eList.Add(new ComboModelString
                {
                    Value = item,
                    Caption = item
                });
            }
            ViewBag.EmailList = eList;
            if (SetKonu.IsNullOrWhiteSpace() == false) model.Konu = SetKonu;
            //if (SetAciklama.IsNullOrWhiteSpace() == false)
            //{
            //    var cevapA = "";
            //    if (id.HasValue)
            //    {
            //        var mesaj = db.Mesajlars.Where(p => p.MesajID == id.Value).First();
            //        var cevapAdresi = SistemAyar.getAyar(SistemAyar.AyarSistemErisimAdresi) + "/Home/Index?MesajGroupID=" + mesaj.GroupID;
            //        cevapA = "<a target='_blank' href='" + cevapAdresi + "' style='color:green;font-size:12pt;'> >> Bu maile sistem üzerinden cevap yazmak için lütfen tıklayınız << </a></br>";
            //    }
            //    var nAck = "</br><p>" + cevapA + "<span style='color:red'>Not: Cevaplama İşlemini Lütfen Sistem Üzerinden Yapınız. Bu mail sistem maili olduğundan yazılan cevaplar okunmamaktadır.</span></br><span style='color:red'>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------</span></p>"
            //                  + SetAciklama.Replace("<p>", "<p><span style='color:#A9A9A9;'>").Replace("</p>", "</span></p>");
            //    model.AciklamaHtml = nAck;
            //    SetAciklama = nAck;
            //}

            ViewBag.MailSablonlariID = new SelectList(Management.CmbMailSablonlari(true, false), "Value", "Caption");
            ViewBag.SetAciklama = SetAciklama ?? "";
            ViewBag.TopluMail = TopluMail;
            ViewBag.toOrBcc = toOrBcc;

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = RoleNames.MailIslemleri)]
        public ActionResult MailGonderPost(int? MesajID, string Alici, string Konu, string Aciklama, string AciklamaHtml, List<HttpPostedFileBase> DosyaEki, List<string> DosyaEkiAdi, List<string> EkYolu, bool TopluMail = false, string strAlicis = "", bool toOrBcc = true)
        {
            var mmMessage = new MmMessage
            {
                Title = "Mail gönderme işlemi"
            };

            DosyaEki = DosyaEki ?? new List<HttpPostedFileBase>();
            DosyaEkiAdi = DosyaEkiAdi ?? new List<string>();
            EkYolu = EkYolu ?? new List<string>();
            var secilenAlicilar = new List<string>();
            if (Alici.IsNullOrWhiteSpace() == false) Alici.Split(',').ToList().ForEach((itm) => { secilenAlicilar.Add(itm); });

            if (Aciklama.IsNullOrWhiteSpace() == false)
            {
                var cevapA = "";
                var geriDonusLink = "";
                if (MesajID.HasValue)
                {
                    var mesaj = db.Mesajlars.Where(p => p.MesajID == MesajID.Value).First();
                    if (mesaj.Mesajlar2 != null) mesaj = mesaj.Mesajlar2;
                    MesajID = mesaj.MesajID;
                    var cevapAdresi = SistemAyar.getAyar(SistemAyar.AyarSistemErisimAdresi) + "/Home/Index?MesajGroupID=" + mesaj.GroupID;
                    cevapA = "<div style='color:#A9A9A9;'>" + mesaj.AciklamaHtml + "</div>";
                    geriDonusLink = "<a target='_blank' href='" + cevapAdresi + "' style='color:green;font-size:12pt;'> >> Bu maile sistem üzerinden cevap yazmak için lütfen tıklayınız << </a>";
                }
                var nAck = "</br><p><span style='color:red'>Not: Cevaplama İşlemini Lütfen Sistem Üzerinden Yapınız. Bu mail sistem maili olduğundan yazılan cevaplar okunmamaktadır.</span></br><span style='color:red'>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------</span></p> " + cevapA;

                AciklamaHtml += geriDonusLink + nAck;
            }
            if (TopluMail)
            {
                secilenAlicilar.AddRange(strAlicis.Split(',').ToList());
            }
            var qDosyaEkAdi = DosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = DosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEkYolu = EkYolu.Select((s, inx) => new { s, inx }).ToList();

            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             join deY in qDosyaEkYolu on dek.inx equals deY.inx
                             select new
                             {
                                 dek.inx,
                                 DosyaEkAdi = dek.s,
                                 Dosya = de.s,
                                 mDosyaAdi = de.s != null ? (dek.s.Replace(".", "") + "." + de.s.FileName.Split('.').Last()) : (dek.s.Replace(".", "") + "." + deY.s.Split('.').Last()),
                                 DosyaYolu = de.s != null ? ("/MailDosyalari/" + dek.s + "_" + Guid.NewGuid().ToString().Substr(0, 4) + "." + de.s.FileName.Split('.').Last()) : (deY.s)
                             }).ToList();

            #region Kontrol 
            if (secilenAlicilar.Count == 0)
            {
                string msg = "Mail Gönderilecek Hiçbir Alıcı Belirlenemedi!";
                mmMessage.Messages.Add(msg);
            }

            if (Konu.IsNullOrWhiteSpace())
            {
                string msg = "Konu Giriniz.";
                mmMessage.Messages.Add(msg);
            }

            if (Aciklama.IsNullOrWhiteSpace() && AciklamaHtml.IsNullOrWhiteSpace())
            {
                string msg = "İçerik Giriniz.";
                mmMessage.Messages.Add(msg);
            }
            #endregion
            var kModel = new GonderilenMailler
            {
                Tarih = DateTime.Now
            };
            if (mmMessage.Messages.Count == 0)
            {
                kModel.MesajID = MesajID;
                kModel.IslemTarihi = DateTime.Now;
                kModel.Konu = Konu;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = Aciklama ?? "";
                kModel.AciklamaHtml = AciklamaHtml ?? "";

                var GonderilenMailEkleri = new List<GonderilenMailEkleri>();
                foreach (var item in qDosyalar)
                {
                    if (item.Dosya != null)
                        item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    GonderilenMailEkleri.Add(new GonderilenMailEkleri
                    {
                        EkAdi = item.mDosyaAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }
                var GonderilenMailKullanicilar = new List<GonderilenMailKullanicilar>();
                var tari = DateTime.Now;
                secilenAlicilar = secilenAlicilar.Distinct().ToList();
                if (secilenAlicilar.Count > 0)
                {
                    var qscIDs = secilenAlicilar.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
                    var qscMails = secilenAlicilar.Where(p => p.IsNumber() == false).ToList();
                    var dataqx = (from s in db.Kullanicilars
                                  where qscIDs.Contains(s.KullaniciID)
                                  select new
                                  {
                                      Email = s.EMail,
                                      KullaniciID = s.KullaniciID
                                  }).ToList();
                    foreach (var item in dataqx)
                    {
                        GonderilenMailKullanicilar.Add(new GonderilenMailKullanicilar
                        {

                            Email = item.Email,
                            KullaniciID = item.KullaniciID
                        });
                    }
                    foreach (var item in qscMails)
                    {
                        GonderilenMailKullanicilar.Add(new GonderilenMailKullanicilar
                        {

                            Email = item,
                            KullaniciID = null
                        });
                    }

                }
                kModel.GonderilenMailEkleris = GonderilenMailEkleri;
                kModel.GonderilenMailKullanicilars = GonderilenMailKullanicilar;
                var eklenen = db.GonderilenMaillers.Add(kModel);


                if (MesajID.HasValue)
                {
                    var mesaj = db.Mesajlars.Where(p => p.MesajID == MesajID.Value).FirstOrDefault();
                    if (mesaj != null)
                    {
                        mesaj.IsAktif = true;
                    }
                }




                GonderilenMailKullanicilar = db.GonderilenMailKullanicilars.AddRange(GonderilenMailKullanicilar.Distinct()).ToList();

                var attach = new List<Attachment>();
                foreach (var item in qDosyalar)
                {
                    var ekTamYol = Server.MapPath("~" + item.DosyaYolu);
                    if (System.IO.File.Exists(ekTamYol))
                        attach.Add(new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(ekTamYol)), item.mDosyaAdi, MediaTypeNames.Application.Octet));
                    else Management.SistemBilgisiKaydet("Mail gönderilirken eklenen dosya eki sistemde bulunamadı!<br/>Dosya Adı:" + item.mDosyaAdi + " <br/>Dosya Yolu:" + ekTamYol, "Ajax/MailGonderPost", BilgiTipi.Hata);
                }


                var gidecekler = GonderilenMailKullanicilar.Select(s => s.Email).ToList();
                Dictionary<int, List<string>> dct = new Dictionary<int, List<string>>();

                int inx = 0;
                while (gidecekler.Count > 800)
                {
                    dct.Add(inx, gidecekler.Take(800).ToList());
                    gidecekler = gidecekler.Skip(800).ToList();
                    inx++;
                }
                inx++;
                dct.Add(inx, gidecekler);
                toOrBcc = !TopluMail;
                foreach (var item in dct)
                {
                    var excpt = MailManager.sendMailRetVal(kModel.Konu, kModel.AciklamaHtml, item.Value, attach, toOrBcc);
                    if (excpt == null)
                    {
                        eklenen.Gonderildi = true;
                        db.SaveChanges();
                        mmMessage.Messages.Add("Mail gönderildi!");
                        mmMessage.IsSuccess = true;
                        mmMessage.MessageType = Msgtype.Success;
                    }
                    else
                    {
                        var msgerr = excpt.ToExceptionMessage().Replace("\r\n", "<br/>");
                        mmMessage.Messages.Add("Mail gönderilirken bir hata oluştu! </br>Hata:" + msgerr);
                        mmMessage.IsSuccess = false;
                        mmMessage.MessageType = Msgtype.Error;
                        try
                        {
                            db.GonderilenMaillers.Remove(eklenen);
                            foreach (var item2 in qDosyalar)
                            {
                                if (System.IO.File.Exists(Server.MapPath("~" + item2.DosyaYolu)))
                                    System.IO.File.Delete(Server.MapPath("~" + item2.DosyaYolu));
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Management.SistemBilgisiKaydet(ex.ToExceptionMessage(), "Ajax/MailGonderPost<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                        }
                    }
                }
            }
            else
            {
                mmMessage.IsSuccess = false;
                mmMessage.MessageType = Msgtype.Warning;
            }

            var strView = Management.RenderPartialView("Ajax", "getMessage", mmMessage);
            //return Content(strView, MediaTypeNames.Text.Html);
            return Json(new { success = mmMessage.IsSuccess, responseText = strView }, JsonRequestBehavior.AllowGet);
            //return new JsonResult { Data = new { IsSuccess = mmMessage.IsSuccess, Message = strView } };

        }


        [Authorize(Roles = RoleNames.MailIslemleri)]
        public ActionResult GetTumMailListesi(string term, string Ids)
        {
            var KullaniciIDs = new JavaScriptSerializer().Deserialize<List<string>>(Ids).Where(p => p.ToIntObj().HasValue).Select(s => s.ToInt().Value);
            var qKullanicilar = (from k in db.Kullanicilars
                                 orderby k.Ad, k.Soyad
                                 where k.EMail.Contains("@") && (k.EMail.StartsWith(term) || (k.Ad + " " + k.Soyad).Contains(term)) && !KullaniciIDs.Contains(k.KullaniciID)
                                 select new
                                 {
                                     id = k.KullaniciID,
                                     AdSoyad = k.Ad + " " + k.Soyad,
                                     text = k.EMail,
                                     Images = k.ResimAdi

                                 }).Take(25).ToList();
            var kul = qKullanicilar.Select(k => new
            {
                id = k.id.ToString(),
                AdSoyad = k.AdSoyad,
                text = k.text,
                Images = k.Images.ToKullaniciResim()

            }).ToList();
            return kul.ToJsonResult();
        }

        public ActionResult GetSablonlar(int MailSablonlariID)
        {
            var KulID = UserIdentity.Current.Id;
            var sbl = db.MailSablonlaris.Where(p => p.MailSablonlariID == MailSablonlariID).Select(s => new { s.SablonAdi, s.Sablon, s.SablonHtml, MailSablonlariEkleri = s.MailSablonlariEkleris.Select(s2 => new { s2.MailSablonlariEkiID, s2.EkAdi, s2.EkDosyaYolu }) }).First();
            return Json(new { sbl.SablonAdi, sbl.Sablon, sbl.SablonHtml, sbl.MailSablonlariEkleri }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMsjKategoris(string EnstituKod)
        {
            var KulID = UserIdentity.Current.Id;
            var Ots = Management.CmbMesajKategorileri(true, true);
            return Ots.Select(s => new { s.Value, s.Caption }).ToJsonResult();
        }
        public ActionResult GetKtNot(int MesajKategoriID)
        {
            string Not = "";
            var mkNot = db.MesajKategorileris.Where(p => p.MesajKategoriID == MesajKategoriID).FirstOrDefault();
            if (mkNot != null) Not = mkNot.KategoriAciklamasi;
            return Json(new { NotBilgisi = Not });
        }
        public ActionResult MesajKaydet(string dlgid, string GroupID)
        {
            var model = new Mesajlar();
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;

            if (GroupID.IsNullOrWhiteSpace() == false)
            {
                model = db.Mesajlars.Where(p => p.GroupID == GroupID).First();
                if (UserIdentity.Current.IsAuthenticated)
                {
                    if (model.KullaniciID != UserIdentity.Current.Id)
                    {
                        model.AdSoyad = UserIdentity.Current.AdSoyad;
                        model.Email = UserIdentity.Current.EMail;
                    }
                }

            }
            else if (UserIdentity.Current.IsAuthenticated)
            {
                model.AdSoyad = UserIdentity.Current.AdSoyad;
                model.Email = UserIdentity.Current.EMail;
            }

            ViewBag.MesajKategoriID = new SelectList(Management.CmbMesajKategorileri(true, true), "Value", "Caption", model.MesajKategoriID);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MesajKaydetPost(int MesajID, string GroupID, int MesajKategoriID, string Konu, string AdSoyad, string Email, string Aciklama, string AciklamaHtml, List<HttpPostedFileBase> DosyaEki, List<string> DosyaEkiAdi, string EKD)
        {
            var mmMessage = new MmMessage
            {
                Title = "Dilek/Öneri/Şikayet gönderme işlemi"
            };

            DosyaEki = DosyaEki ?? new List<HttpPostedFileBase>();
            DosyaEkiAdi = DosyaEkiAdi ?? new List<string>();

            var qDosyaEkAdi = DosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = DosyaEki.Select((s, inx) => new { s, inx }).ToList();

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

            #region Kontrol 

            if (MesajID <= 0)
            {
                if (Konu.IsNullOrWhiteSpace())
                {
                    string msg = "Konu Giriniz.";
                    mmMessage.Messages.Add(msg);
                }
            }
            else
            {
                var mesaj = db.Mesajlars.Where(p => p.MesajID == MesajID && p.GroupID == GroupID).First();
                mesaj.IsAktif = false;
                Konu = mesaj.Konu;
                MesajKategoriID = mesaj.MesajKategoriID;
                if (UserIdentity.Current.IsAuthenticated && mesaj.KullaniciID != UserIdentity.Current.Id)
                {
                    Email = UserIdentity.Current.EMail;
                    AdSoyad = UserIdentity.Current.AdSoyad;
                }
                else
                {
                    Email = mesaj.Email;
                    AdSoyad = mesaj.AdSoyad;

                }
            }
            if (Email.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("E Mail Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else if (Email.ToIsValidEmail())
            {
                mmMessage.Messages.Add("Lütfen EMail Formatını Doğru Giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            if (Aciklama.IsNullOrWhiteSpace() && AciklamaHtml.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("İçerik Giriniz.");
            }

            var kModel = new Mesajlar();
            #endregion
            if (mmMessage.Messages.Count == 0)
            {
                kModel.MesajKategoriID = MesajKategoriID;
                if (UserIdentity.Current.IsAuthenticated == false)
                {
                    kModel.AdSoyad = AdSoyad;
                    kModel.Email = Email;

                }
                else
                {
                    var kul = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).First();
                    kModel.AdSoyad = kul.Ad + " " + kul.Soyad;
                    kModel.Email = kul.EMail;
                    kModel.KullaniciID = UserIdentity.Current.Id;
                    kModel.IslemYapanID = UserIdentity.Current.Id;
                }
                kModel.UstMesajID = MesajID <= 0 ? (int?)null : MesajID;
                kModel.GroupID = Guid.NewGuid().ToString();
                kModel.Tarih = DateTime.Now;
                kModel.IslemTarihi = DateTime.Now;
                kModel.Konu = Konu;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = Aciklama ?? "";
                kModel.AciklamaHtml = AciklamaHtml ?? "";
                kModel.IsAktif = false;
                var mesajEkler = new List<MesajEkleri>();
                foreach (var item in qDosyalar)
                {
                    item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    mesajEkler.Add(new MesajEkleri
                    {
                        EkAdi = item.mDosyaAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }

                var eklenen = db.Mesajlars.Add(kModel);
                eklenen.MesajEkleris = mesajEkler;
                db.SaveChanges();
                mmMessage.IsSuccess = true;
            }
            else
            {
                mmMessage.IsSuccess = false;
            }

            //var strView = Management.RenderPartialView("Ajax", "getMessage", mmMessage);
            //return Content(strView, MediaTypeNames.Text.Html);
            return Json(new { success = mmMessage.IsSuccess, responseText = mmMessage.IsSuccess ? "Mesaj gönderme işlemi başarılı!" : "Mesaj gönderilirken bir hata oluştu! Hata: " + string.Join("</br>", mmMessage.Messages.ToList()) }, JsonRequestBehavior.AllowGet);
            //return new JsonResult { Data = new { IsSuccess = mmMessage.IsSuccess, Message = strView } };

        }

      

        [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PdfViewer()
        {
            return View();
        }

        public ActionResult GetChkList()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetDxReport(int? raporTipID)
        {
            XtraReport RprX = null;
            var RaporTipIDControl1 = new List<int>();
            if (raporTipID == RaporTipleri.BirimToplamsalRapor && RoleNames.BirimToplamsalRapor.InRole()) RaporTipIDControl1.Add(RaporTipleri.BirimToplamsalRapor);
            else if (raporTipID == RaporTipleri.BildirgeToplamsalRapor && RoleNames.BildirgeToplamsalRapor.InRole()) RaporTipIDControl1.Add(RaporTipleri.BildirgeToplamsalRapor);

            var RaporTipIDControl = RaporTipIDControl1.Contains(raporTipID ?? -1);
            if (RaporTipIDControl == false)
            {
                string BilgiMesaji = "Rapor almak için gereken kriterleri sağlamayan bir istek gönderildi!\r\n-----------------------";
                BilgiMesaji += "\r\nRaporTipD:" + (raporTipID.HasValue == false ? "NULL" : raporTipID.Value.ToString());
                BilgiMesaji += "\r\nOtantikasyon:" + (UserIdentity.Current.IsAuthenticated ? "Kullanıcı Ontantike (" + UserIdentity.Current.Name + ")" : "Kullanıcı Otantike Değil");
                BilgiMesaji += "\r\nIP Adresi:" + UserIdentity.Ip;
                Management.SistemBilgisiKaydet(BilgiMesaji, "Ajax/GetDxReport", BilgiTipi.Saldırı);

            }
            else
            {
                if (raporTipID == RaporTipleri.BirimToplamsalRapor)
                {
                    var VASurecID = Request["VASurecID"].ToInt().Value;
                    var BirimID = Request["BirimID"].Split(',').Select(s => s.ToInt()).ToList();
                    var AyID = Request["AyID"].ToInt().Value;

                    var DonemBilgi = Management.GetVASurecKontrol(VASurecID, AyID);
                    var Model = new RpModelToplamsalModel();
                    Model.DonemAdi = "Dönem: " + DonemBilgi.Yil + " " + DonemBilgi.SecilenAyBilgi.AyAdi;

                    //if (!UserIdentity.Current.IsAdmin)
                    //{
                    //    BirimID = BirimID.Where(p => UserIdentity.Current.BirimYetkileriRapor.Contains(p ?? 0)).ToList();
                    //}

                    var qdata = (from vb in db.VASurecleriBirims
                                 join b in db.Birimlers on vb.BirimID equals b.BirimID
                                 join ub in db.Birimlers on b.UstBirimID equals ub.BirimID into def1
                                 from Ub in def1.DefaultIfEmpty()
                                 where vb.VASurecID == VASurecID && UserIdentity.Current.BirimYetkileriRapor.Contains(vb.BirimID) && (BirimID.Contains(vb.BirimID) || BirimID.Contains(vb.UstBirimID ?? 0))
                                 group new { } by new
                                 {
                                     BirimID = (Ub != null ? Ub.BirimID : b.BirimID),
                                     BirimAdi = (Ub != null ? Ub.BirimAdi : b.BirimAdi),

                                 } into g1
                                 select new
                                 {
                                     BirimID = g1.Key.BirimID,
                                     BirimAdi = g1.Key.BirimAdi,
                                     Data = db.VASurecleriBirimVerileris.Where(p => p.VASurecleriBirim.VASurecID == VASurecID && (p.VASurecleriBirim.UstBirimID == g1.Key.BirimID || p.VASurecleriBirim.BirimID == g1.Key.BirimID) && p.AyID == AyID).ToList()

                                 }).OrderBy(o => o.BirimAdi).ToList().Select(s => new RpRowBirimToplamsal
                                 {
                                     BirimAdi = s.BirimAdi,
                                     PrimTutari = s.Data.Sum(s2 => (s2.HakEdilenUcret + s2.PrimIkramiyeIstihkak) * (s2.PrimYuzdesi / 100)),
                                     //Agi = s.Data.Sum(s2 => s2.AsgariGecimIndirimi ?? 0),
                                     DvKesinti = s.Data.Sum(s2 => s2.DvKesintisi ?? 0),
                                     GvMatrah = s.Data.Sum(s2 => s2.GvMatrahi ?? 0),
                                     GvKesinti = s.Data.Sum(s2 => s2.GvKesinti ?? 0),
                                     KisiSayisi = s.Data.Select(s2 => s2.TcKimlikNo).Distinct().Count(),

                                 }).ToList();

                    Model.Data = qdata;
                    var rpr = new RprBirimToplamsal();

                    rpr.DataSource = new List<RpModelToplamsalModel> { Model };
                    rpr.DisplayName = "MUSSK Bildirimi Birimlere Göre Toplamsal Bilgiler";

                    RprX = rpr;
                }
                else if (raporTipID == RaporTipleri.BildirgeToplamsalRapor)
                {
                    var VASurecID = Request["VASurecID"].ToInt().Value;
                    var BirimID = Request["BirimID"].Split(',').Select(s => s.ToInt()).ToList();
                    var AyID = Request["AyID"].ToInt().Value;

                    var DonemBilgi = Management.GetVASurecKontrol(VASurecID, AyID);
                    var Model = new List<RpModelToplamsalModel>();
                    var DonemAdi = "Dönem: " + DonemBilgi.Yil + " " + DonemBilgi.SecilenAyBilgi.AyAdi;

                    //if (!UserIdentity.Current.IsAdmin)
                    //{
                    //    BirimID = BirimID.Where(p => UserIdentity.Current.BirimYetkileriRapor.Contains(p ?? 0)).ToList();

                    //}


                    var yuklenenToplam = (from s in db.VASurecleriBirimVerileris.Where(p => p.VASurecleriBirim.VASurecID == VASurecID && UserIdentity.Current.BirimYetkileriRapor.Contains(p.VASurecleriBirim.BirimID) && (BirimID.Contains(p.VASurecleriBirim.BirimID) || BirimID.Contains(p.VASurecleriBirim.UstBirimID ?? 0)) && p.AyID == AyID)
                                          join bt in db.BelgeTurleris on s.BelgeTurID equals bt.BelgeTurID
                                          join b in db.Birimlers on s.VASurecleriBirim.UstBirimID equals b.BirimID
                                          group new { s } by new { s.BelgeTurID, s.BelgeTurKodu, bt.BelgeTurAdi, s.PrimYuzdesi, b.BirimID, b.BirimAdi } into g1
                                          select new RpRowBirimToplamsal
                                          {
                                              BirimID = g1.Key.BirimID,
                                              BirimAdi = g1.Key.BirimAdi,
                                              BildirgeNo = g1.Key.BelgeTurKodu,
                                              BildirgeAdi = g1.Key.BelgeTurAdi,
                                              PrimeEsasKazancTutar = g1.Sum(p => p.s.HakEdilenUcret + p.s.PrimIkramiyeIstihkak),
                                              PrimTutari = g1.Sum(p => (p.s.HakEdilenUcret + p.s.PrimIkramiyeIstihkak) * (g1.Key.PrimYuzdesi / 100)),
                                              KisiSayisi = g1.Select(s2 => s2.s.TcKimlikNo).Distinct().Count()

                                          }).OrderBy(o => o.BirimAdi).ThenBy(t => t.BildirgeAdi).ToList();

                    foreach (var item in yuklenenToplam.Select(s => new { s.BirimID, s.BirimAdi }).Distinct())
                    {
                        Model.Add(new RpModelToplamsalModel
                        {
                            BirimAdi = item.BirimAdi,
                            BirimID = item.BirimID,
                            Data = yuklenenToplam.Where(p => p.BirimID == item.BirimID).ToList()
                        });
                    }
                    var rpr = new RprBildirgeToplamsal(DonemAdi);

                    rpr.DataSource = Model;
                    rpr.DisplayName = "MUSSK Bildirimi Bildirgelere Göre Toplamsal Bilgiler";

                    RprX = rpr;
                }




            }
            return View(RprX);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}
