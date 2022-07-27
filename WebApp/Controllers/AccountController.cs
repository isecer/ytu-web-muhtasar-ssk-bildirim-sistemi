using BiskaUtil;
using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database; 

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AccountController : Controller
    { 
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Login(bool? logout, string dlgId, string ReturnUrl)
        {
            
            if (logout == true)
            {
                FormsAuthenticationUtil.SignOut();
                return RedirectToAction("Index", "Home");
            }
            else if (UserIdentity.Current.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewBag.UserName = "";
            var MmMessage = new MmMessage() { IsDialog = !dlgId.IsNullOrWhiteSpace(), DialogID = dlgId, ReturnUrl = ReturnUrl };
            ViewBag.MmMessage = MmMessage;
            return PartialView();
        }
        [HttpPost]
        public ActionResult Login(string UserName, string Password, string captchaRequestCode, string CaptchaText, bool? RememberMe, string ReturnUrl, string dlgId)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgId.IsNullOrWhiteSpace(), DialogID = dlgId, ReturnUrl = ReturnUrl };
            ViewBag.UserName = UserName;
            ViewBag.Password = Password;
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
                                msg = "Active Directory Kontrolünden Geçilemedi!";
                                Management.SistemBilgisiKaydet("Active Directory Kontrolünden Geçilemedi! Kullanıcı Adı: " + UserName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                                //log.Info("Login Yapılamadı", "");
                            }
                        }

                        if (loginUser != null && loginUser.IsAktif == true)
                        {
                            RememberMe = RememberMe ?? false;
                            FormsAuthenticationUtil.SetAuthCookie(UserName, "", RememberMe.Value);
                            MmMessage.IsCloseDialog = true;
                            if (MmMessage.IsDialog)
                            {
                                if (ReturnUrl.IsNullOrWhiteSpace()) MmMessage.ReturnUrl = Url.Action("Index", "Home");
                            }
                            else
                            {
                                Management.SetLastLogon();
                                if (ReturnUrl.IsNullOrWhiteSpace()) return RedirectToAction("Index", "Home");
                                else return Redirect(ReturnUrl);
                            }

                        }
                        else
                        {
                            #region default user
                            if (loginUser == null && UserName == "admin")
                            {
                                Management.CreateAdmin();
                            }
                            #endregion
                            if (loginUser != null && !loginUser.IsAktif) Hata = "Kullanıcı Hesabı Pasif Durumda!";
                            else Hata = "Kullanıcı Adı veya Şifre Hatalı. " + msg;
                        }
                    }
                    else
                    {
                        Management.SistemBilgisiKaydet("Kullanıcı Sistemde Bulunamadı! Kullanıcı Adı: " + UserName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                        Hata = "Kullanıcı sistemde bulunamadı.";
                    }
                }

            }
            catch (Exception ex)
            {
                MmMessage.IsSuccess = false;
                MmMessage.Messages.Add("Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage());
                Hata = "Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage();
            }
            ViewBag.Hata = Hata;
            ViewBag.MmMessage = MmMessage;
            return PartialView();

        }
       
        public ActionResult OnlineUserCnt()
        {

            var users = OnlineUsers.users;
            int Count = users.Count();
            var q = Count.ToJsonResult();
            return q;

        }

        public ActionResult getOnlineUserList()
        {
            var users = OnlineUsers.users;
            return View(users);
        }
        public ActionResult generatecaptcha(string captchaRequestCode)
        {
            CaptchaImage ci = new CaptchaImage(string.Empty, 180, 50, 12);
            var text = ci.Text;
            CaptchaImageRequests.AddCode(captchaRequestCode, text);
            CaptchaImageRequests.AddCode(Session.SessionID, text);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ci.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            FileContentResult fcr = new FileContentResult(ms.ToArray(), "image/jpeg");
            fcr.FileDownloadName = "captcha.jpeg";
            return fcr;
        }



        public ActionResult ParolaSifirla(string psKod, int? KullaniciID = null, string dlgId = "")
        {

            MmMessage msg = new MmMessage();
            msg.ReturnUrlTimeOut = 4000;
            msg.IsDialog = !dlgId.IsNullOrWhiteSpace();
            msg.DialogID = dlgId;
            if (psKod.IsNullOrWhiteSpace() && KullaniciID.HasValue == false) return RedirectToAction("Index", "Home");



            var kul = new Kullanicilar();
            if (KullaniciID.HasValue == false)
            {
                kul = db.Kullanicilars.Where(p => p.ParolaSifirlamaKodu == psKod).FirstOrDefault();

                if (kul != null)
                {
                    kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
                    if (kul.ParolaSifirlamGecerlilikTarihi.HasValue && kul.ParolaSifirlamGecerlilikTarihi.Value < DateTime.Now)
                    {
                        msg.IsSuccess = false;
                        msg.Messages.Add("Parola Sıfırlama linkinin geçerlilik süresi dolmuştur");
                        msg.ReturnUrl = Url.Action("Index", "Home");
                    }
                }
                else
                {
                    msg.IsSuccess = false;
                    msg.Messages.Add("Şifre sıfırlama linki herhangi bir kullanıcıya eşleştirilemedi");
                    msg.ReturnUrl = Url.Action("Index", "Home");


                }
            }
            else
            {
                if (UserIdentity.Current.IsAuthenticated)
                {
                    KullaniciID = UserIdentity.Current.Id;
                    kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
                    if (kul != null)
                    {
                        kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
                    }
                }
                else
                {
                    msg.IsSuccess = false;
                    msg.IsCloseDialog = true;
                    msg.Messages.Add("Lütfen Giriş Yapın");
                    msg.ReturnUrl = Url.Action("Index", "Home");

                }
            }
            Session["ShwMesaj"] = msg;
            ViewBag.MmMessage = msg;
            ViewBag.KullaniciID = KullaniciID;
            ViewBag.EskiSifre = "";
            ViewBag.YeniSifre = "";
            ViewBag.YeniSifreTekrar = "";
            return View(kul);
        }
        [HttpPost]
        public ActionResult ParolaSifirla(string psKod, string EskiSifre, string YeniSifre, string YeniSifreTekrar, int? KullaniciID = null, string dlgId = "")
        {
            MmMessage MmMessage = new MmMessage();
            MmMessage.IsDialog = !dlgId.IsNullOrWhiteSpace();
            MmMessage.DialogID = dlgId;
            MmMessage.ReturnUrlTimeOut = 4000;
            if (psKod.IsNullOrWhiteSpace() == true)
            {
                MmMessage.MessageType = Msgtype.Error;
                MmMessage.Title = "Şifre değiştirme işlemi başarısız";
                MmMessage.ReturnUrl = Url.Action("Index", "Home");
            }
            var kul = new Kullanicilar();
            if (KullaniciID.HasValue) kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
            else kul = db.Kullanicilars.Where(p => p.ParolaSifirlamaKodu == psKod).FirstOrDefault();
            if (kul != null)
            {
                if (KullaniciID.HasValue == false)
                    if (kul.ParolaSifirlamGecerlilikTarihi.HasValue && kul.ParolaSifirlamGecerlilikTarihi.Value < DateTime.Now)
                    {
                        MmMessage.MessageType = Msgtype.Error;
                        MmMessage.Messages.Add("Parola Sıfırlama linkinin geçerlilik süresi dolmuştur");
                        MmMessage.ReturnUrl = Url.Action("Index", "Home");
                    }
                if (KullaniciID.HasValue)
                {
                    if (EskiSifre.IsNullOrWhiteSpace())
                    { 
                        MmMessage.Messages.Add("Varolan şifrenizi giriniz");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre" });
                    }
                    else if (!(kul.Sifre == EskiSifre.ComputeHash(Management.Tuz)))
                    { 
                        MmMessage.Messages.Add("Varolan şifrenizi yanlış girdiniz");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre"  });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EskiSifre" });
                }
                if (MmMessage.Messages.Count == 0)
                {

                    if (YeniSifre.Length < 4)
                    { 
                        MmMessage.Messages.Add("Yeni şifreniz en az 4 haneli olmalıdır");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre"  });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifre" });
                    if (YeniSifreTekrar.Length < 4)
                    { 
                        MmMessage.Messages.Add("Yeni şifre tekrar en az 4 haneli olmalıdır");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
                    if (MmMessage.Messages.Count == 0)
                    {
                        if (YeniSifreTekrar != YeniSifre)
                        { 
                            MmMessage.Messages.Add("Yeni şifre ile yeni şifre tekrar birbiriyle uyuşmuyor");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar"  });
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre"  });
                        }
                        else
                        {
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifre" });
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifreTekrar" });
                        }
                    }
                }

                if (MmMessage.Messages.Count == 0)
                {
                    kul.Sifre = YeniSifreTekrar.ComputeHash(Management.Tuz);
                    kul.ParolaSifirlamGecerlilikTarihi = DateTime.Now;
                    db.SaveChanges();
                    MmMessage.MessageType = Msgtype.Success;
                    MmMessage.Title = "Şifre değiştirme işlemi";
                    if (KullaniciID.HasValue == false)
                    { 
                        MmMessage.Messages.Add("Şifreniz değiştirildi! Giriş sayfasına yönlendiriliyorsunuz...");
                        MmMessage.ReturnUrl = Url.Action("Login", "Account");
                    }
                    else
                    { 
                        MmMessage.IsCloseDialog = true;
                        MmMessage.Messages.Add("Şifreniz değiştirildi!");
                    }
                }
                else
                {
                    MmMessage.MessageType = Msgtype.Error;
                    MmMessage.Title = "Şifre değiştirme işlemi başarısız!";
                    Management.SistemBilgisiKaydet("Şifre değiştirme işlemi başarısız! Hata:" + string.Join("\r\n", MmMessage.Messages) + "\r\n KullanıcıAdı:" + kul.KullaniciAdi, "Account/ParolaSifirla", BilgiTipi.Bilgi);
                }
                kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
            }
            else
            {
                MmMessage.MessageType = Msgtype.Error;
                MmMessage.Title = "Şifre değiştirme işlemi başarısız!";
            }
            if (MmMessage.Messages.Count > 0)
            {
                if (UserIdentity.Current.IsAuthenticated)
                {
                    MessageBox.Show(MmMessage.Title, MmMessage.MessageType == Msgtype.Success ? MessageBox.MessageType.Success : MessageBox.MessageType.Error, MmMessage.Messages.ToArray());
                }
                else
                {
                    Session["ShwMesaj"] = MmMessage;
                }
            }


            ViewBag.MmMessage = MmMessage;
            ViewBag.KullaniciID = KullaniciID;
            ViewBag.EskiSifre = EskiSifre;
            ViewBag.YeniSifre = YeniSifre;
            ViewBag.YeniSifreTekrar = YeniSifreTekrar;
            return View(kul);
        }
 
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
