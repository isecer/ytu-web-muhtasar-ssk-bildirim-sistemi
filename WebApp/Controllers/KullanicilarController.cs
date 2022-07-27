using WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BiskaUtil;
using System.Net.Mail;
using System.IO;
using System.Drawing;
using System.Net.Mime;
using Database;

namespace WebApp.Controllers
{
    [Authorize]
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 4, VaryByParam = "*")]
    public class KullanicilarController : Controller
    {   //iRfaa
        private MusskDBEntities db = new MusskDBEntities();
        [Authorize(Roles = RoleNames.Kullanicilar)]
        public ActionResult Index()
        {
            return Index(new FmKullanicilar() { PageSize = 15, Expand = false });
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Kullanicilar)]
        public ActionResult Index(FmKullanicilar model, List<int> RollID = null)
        {

            RollID = RollID ?? new List<int>();
            var brms = db.sp_BirimAgaci().ToList();
            var Kyetkisi = RoleNames.KullaniciKayit.InRole();
            var q = from s in db.Kullanicilars
                    join ktl in db.YetkiGruplaris on s.YetkiGrupID equals ktl.YetkiGrupID
                    select new FrKullanicilar
                    {
                        KullaniciID = s.KullaniciID,
                        YetkiGrupID = s.YetkiGrupID,
                        YetkiGrupAdi = ktl.YetkiGrupAdi,
                        UnvanID = s.UnvanID,
                        UnvanAdi = s.Unvanlar.UnvanAdi,
                        BirimID = s.BirimID,
                        BirimAdi = s.Birimler.BirimAdi,
                        Ad = s.Ad,
                        Soyad = s.Soyad,
                        Tel = s.Tel,
                        EMail = s.EMail,
                        ResimAdi = s.ResimAdi,
                        KullaniciAdi = s.KullaniciAdi,
                        Sifre = s.Sifre,
                        FixedHeader = s.FixedHeader,
                        FixedSidebar = s.FixedSidebar,
                        ScrollSidebar = s.ScrollSidebar,
                        RightSidebar = s.RightSidebar,
                        CustomNavigation = s.CustomNavigation,
                        ToggledNavigation = s.ToggledNavigation,
                        BoxedOrFullWidth = s.BoxedOrFullWidth,
                        ThemeName = s.ThemeName,
                        BackgroundImage = s.BackgroundImage,
                        SifresiniDegistirsin = s.SifresiniDegistirsin,
                        IsAktif = s.IsAktif,
                        IsActiveDirectoryUser = s.IsActiveDirectoryUser,
                        IsAdmin = s.IsAdmin,
                        Aciklama = s.Aciklama,
                        ParolaSifirlamaKodu = s.ParolaSifirlamaKodu,
                        ParolaSifirlamGecerlilikTarihi = s.ParolaSifirlamGecerlilikTarihi,
                        OlusturmaTarihi = s.OlusturmaTarihi,
                        LastLogonDate = s.LastLogonDate,
                        LastLogonIP = s.LastLogonIP,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanIP = s.IslemYapanIP
                    };

            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => (p.Ad + " " + p.Soyad).Contains(model.AdSoyad) || p.EMail.Contains(model.AdSoyad) || p.Tel.Contains(model.AdSoyad) || p.KullaniciAdi.Contains(model.AdSoyad));

            if (model.YetkiGrupID.HasValue) q = q.Where(p => p.YetkiGrupID == model.YetkiGrupID.Value);
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            if (model.IsActiveDirectoryUser.HasValue) q = q.Where(p => p.IsActiveDirectoryUser == model.IsActiveDirectoryUser.Value);

            if (model.BirimID.HasValue)
            {
                var sbKods = Management.GetSubBirimIDs(model.BirimID.Value);
                q = q.Where(p => sbKods.Contains(p.BirimID));
            }

            if (model.IsAdmin.HasValue) q = q.Where(p => p.IsAdmin == model.IsAdmin);

            model.RowCount = q.Count();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            IndexModel.Pasif = q.Where(p => p.IsAktif == false).Count();
            IndexModel.Aktif = IndexModel.Toplam - IndexModel.Pasif;
            if (!model.Sort.IsNullOrWhiteSpace())
                if (model.Sort == "AdSoyad") q = q.OrderBy(o => o.Ad).ThenBy(o => o.Soyad);
                else if (model.Sort.Contains("AdSoyad") && model.Sort.Contains("DESC")) q = q.OrderByDescending(o => o.Ad).ThenByDescending(o => o.Soyad);
                else q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.Ad).ThenBy(t => t.Soyad);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).ToArray();
            foreach (var item in model.Data)
            {
                var secilenB = brms.Where(p => p.BirimID == item.BirimID).FirstOrDefault();
                item.BirimAdi = secilenB.BirimTreeAdi;
            }
            ViewBag.YetkiGrupID = new SelectList(Management.CmbYetkiGruplari(), "Value", "Caption", model.YetkiGrupID);
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.IsAdmin = new SelectList(Management.CmbVarYokData(), "Value", "Caption", model.IsAdmin);
            ViewBag.RollID = new SelectList(Management.GetAllRoles(), "RolID", "GorunurAdi");
            ViewBag.BirimID = new SelectList(Management.CmbKullaniciBirimlerTree(), "Value", "Caption", model.BirimID);
            ViewBag.UnvanID = new SelectList(Management.CmbUnvanlar(), "Value", "Caption", model.UnvanID);
            ViewBag.IsActiveDirectoryUser = new SelectList(Management.CmbIsActiveDirectoryUserData(), "Value", "Caption", model.IsActiveDirectoryUser);
            ViewBag.SelectedRolls = RollID;
            ViewBag.IndexModel = IndexModel;
            ViewBag.kIds = q.Select(s => s.KullaniciID).ToList();
            return View(model);
        }
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new Kullanicilar();
            model.IsAktif = true;
            bool ResimVar = false;
            if (id.HasValue && id > 0)
            {
                var data = db.Kullanicilars.Where(p => p.KullaniciID == id).FirstOrDefault();
                if (data != null)
                {
                    ResimVar = data.ResimAdi.IsNullOrWhiteSpace() == false;
                    data.ResimAdi = data.ResimAdi.ToKullaniciResim();
                    model = data;
                }
                model.Sifre = "";
            }

            ViewBag.ResimVar = ResimVar;
            ViewBag.YetkiGrupID = new SelectList(Management.CmbYetkiGruplari(), "Value", "Caption", model.YetkiGrupID);
            //ViewBag.BirimID = new SelectList(Management.GetBirimler().ToOrderedList("BirimID", "UstBirimID", "BirimAdi"), "BirimID", "BirimAdi", model.BirimID);
            ViewBag.BirimID = new SelectList(Management.CmbKullaniciBirimlerTree(), "Value", "Caption", model.BirimID);
            ViewBag.UnvanID = new SelectList(Management.CmbUnvanlar(), "Value", "Caption", model.UnvanID);
            ViewBag.Kullanici = Management.GetUser(model.KullaniciID);
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Kayit(Kullanicilar kModel, int? EO_DonemID, HttpPostedFileBase ProfilResmi, bool YetkilendirmeyeGit = false, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            var resimBilgi = new ComboModelString { Caption = "", Value = "" };
            bool ResimVar = false;
            if (kModel.KullaniciID > 0)
            {
                var kul = db.Kullanicilars.Where(p => p.KullaniciID == kModel.KullaniciID).First();
                ResimVar = kul.ResimAdi.IsNullOrWhiteSpace() == false;
                kModel.ResimAdi = kul.ResimAdi.ToKullaniciResim();
            }
            #region Kontrol
            kModel.KullaniciAdi = kModel.KullaniciAdi != null ? kModel.KullaniciAdi.Trim() : "";

            if (kModel.Ad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Ad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Ad" });
            if (kModel.Soyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Soyad Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Soyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Soyad" });


            if (kModel.BirimID <= 0)
            {
                MmMessage.Messages.Add("Birim Seçiniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimID" });
            if (kModel.UnvanID <= 0)
            {
                MmMessage.Messages.Add("Ünvan Seçiniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "UnvanID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "UnvanID" });
            if (kModel.Tel.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Cep telefonu bilgisini giriniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tel" });
            }
            else
            {
                if (kModel.Tel.IsNullOrWhiteSpace() == false) MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tel" });
            }

            if (kModel.EMail.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("E Mail Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else if (kModel.EMail.ToIsValidEmail())
            {
                MmMessage.Messages.Add("Lütfen EMail Formatını Doğru Giriniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EMail" });
            }
            if (kModel.KullaniciAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Kullanıcı Adı Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KullaniciAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KullaniciAdi" });


            if (kModel.KullaniciID <= 0)
            {
                if (kModel.Sifre.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Şifre Giriniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else if (kModel.Sifre.Length < 4)
                {
                    MmMessage.Messages.Add("Şifre en az 4 haneli olmalıdır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Sifre" });
            }
            else if (!kModel.Sifre.IsNullOrWhiteSpace())
            {
                if (kModel.Sifre.Length < 4 && kModel.KullaniciID > 0)
                {
                    MmMessage.Messages.Add("Şifre en az 4 haneli olmalıdır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else if (kModel.Sifre.Length >= 4 && kModel.KullaniciID > 0) MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Sifre" });
            }
            if (kModel.YetkiGrupID <= 0)
            {
                MmMessage.Messages.Add("Kullanıcı hesabı için yetki grubu seçiniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YetkiGrupID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YetkiGrupID" });
            #endregion



            if (MmMessage.Messages.Count == 0)
            {
                kModel.Ad = kModel.Ad.Trim();
                kModel.Soyad = kModel.Soyad.Trim();
                kModel.EMail = kModel.EMail.Trim();
                kModel.Tel = kModel.Tel.Trim();
                kModel.KullaniciAdi = kModel.KullaniciAdi.Trim();
                var qKullanici = db.Kullanicilars.AsQueryable();

                var cUserName = qKullanici.Where(p => p.IsAktif && p.KullaniciID != kModel.KullaniciID && p.KullaniciAdi == kModel.KullaniciAdi).Count();
                if (cUserName > 0)
                {
                    MmMessage.Messages.Add("Tanımlamak istediğiniz kullanıcı adı sistemde zaten mevcut!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KullaniciAdi" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KullaniciAdi" });
                //var cEmail = qKullanici.Where(p => p.IsAktif && p.KullaniciID != kModel.KullaniciID && p.EMail == kModel.EMail).Count();
                //if (cEmail > 0)
                //{
                //    string msg = "Tanımlamak istediğiniz Email sistemde zaten mevcut!";
                //    MmMessage.Messages.Add(msg);
                //    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail", Message = msg });
                //}
                //else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EMail" });

            }


            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var YeniKullanici = kModel.KullaniciID <= 0;
                if (YeniKullanici)
                {
                    var sfr = kModel.Sifre;
                    kModel.OlusturmaTarihi = DateTime.Now;
                    kModel.Sifre = kModel.Sifre.ComputeHash(Management.Tuz);
                    kModel.IsAktif = true;
                    kModel.FixedHeader = false;
                    kModel.FixedSidebar = false;
                    kModel.ScrollSidebar = false;
                    kModel.RightSidebar = false;
                    kModel.CustomNavigation = true;
                    kModel.ToggledNavigation = false;
                    kModel.BoxedOrFullWidth = true;
                    kModel.ThemeName = "/Content/css/theme-forest.css";
                    kModel.BackgroundImage = "wall_2";

                    if (ProfilResmi != null)
                    {
                        resimBilgi = Management.ResimKaydet(ProfilResmi);
                        kModel.ResimAdi = resimBilgi.Caption;

                    }

                    kModel = db.Kullanicilars.Add(kModel);
                    db.SaveChanges();

                    var excpt = MailManager.YeniHesapMailGonder(kModel, sfr);
                    if (excpt != null)
                    {
                        MmMessage.Messages.Add(kModel.KullaniciAdi + " kullanıcı hesabı oluşturuldu fakat kullanıcıya bilgi maili atılırken bir hata oluştu! Hata:" + excpt.ToExceptionMessage());
                        MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
                    }

                }
                else
                {
                    var data = db.Kullanicilars.Where(p => p.KullaniciID == kModel.KullaniciID).First();
                    data.YetkiGrupID = kModel.YetkiGrupID;
                    data.Ad = kModel.Ad;
                    data.Soyad = kModel.Soyad;
                    data.BirimID = kModel.BirimID;
                    data.UnvanID = kModel.UnvanID;
                    data.IsAktif = kModel.IsAktif;
                    data.Tel = kModel.Tel;
                    data.EMail = kModel.EMail;
                    data.KullaniciAdi = kModel.KullaniciAdi;
                    if (!kModel.Sifre.IsNullOrWhiteSpace())
                        data.Sifre = kModel.Sifre.ComputeHash(Management.Tuz);
                    data.SifresiniDegistirsin = kModel.SifresiniDegistirsin;
                    data.Aciklama = kModel.Aciklama;
                    data.IsActiveDirectoryUser = kModel.IsActiveDirectoryUser;
                    data.IsAdmin = kModel.IsAdmin;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                    if (ProfilResmi != null)
                    {
                        if (data.ResimAdi.IsNullOrWhiteSpace() == false)
                        {
                            var rsmYol = SistemAyar.getAyar(SistemAyar.KullaniciResimYolu);
                            var rsm = Server.MapPath("~/" + rsmYol + "/" + data.ResimAdi);
                            if (System.IO.File.Exists(rsm)) System.IO.File.Delete(rsm);
                        }
                        resimBilgi = Management.ResimKaydet(ProfilResmi);
                        data.ResimAdi = resimBilgi.Caption;
                    }
                    db.SaveChanges();
                    if (data.KullaniciID == UserIdentity.Current.Id) { UserIdentity.Current.ImagePath = data.ResimAdi.ToKullaniciResim(); }

                }


                if (YetkilendirmeyeGit) return RedirectToAction("KullaniciBirimYetkileri", new { id = kModel.KullaniciID });
                else return RedirectToAction("Index");

            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.ResimVar = ResimVar;
            ViewBag.YetkiGrupID = new SelectList(Management.CmbYetkiGruplari(), "Value", "Caption", kModel.YetkiGrupID);
            //ViewBag.BirimID = new SelectList(Management.GetBirimler().ToOrderedList("BirimID", "UstBirimID", "BirimAdi"), "BirimID", "BirimAdi", kModel.BirimID);

            ViewBag.BirimID = new SelectList(Management.CmbKullaniciBirimlerTree(), "Value", "Caption", kModel.BirimID);
            ViewBag.UnvanID = new SelectList(Management.CmbUnvanlar(), "Value", "Caption", kModel.UnvanID);


            ViewBag.MmMessage = MmMessage;
            ViewBag.Kullanici = Management.GetUser(kModel.KullaniciID);
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Yetkilendirme(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var kid = id;
            var roles = Management.GetAllRoles().ToList();
            var userRoles = Management.GetUserRoles(kid.Value);
            var Kullanici = Management.GetUser(kid.Value);
            ViewBag.Kullanici = Kullanici;
            var data = roles.Select(s => new CheckObject<Roller>
            {
                Value = s,
                Disabled = userRoles.YetkiGrupRolleri.Any(a => a.RolID == s.RolID),
                Checked = userRoles.TumRoller.Any(p => p.RolID == s.RolID)
            });
            ViewBag.Roller = data;
            var kategr = roles.Select(s => s.Kategori).Distinct().ToArray();
            var menuK = db.Menulers.Where(a => a.BagliMenuID == 0 && kategr.Contains(a.MenuAdi)).ToList();
            var dct = new List<ComboModelInt>();
            foreach (var item in menuK)
            {
                dct.Add(new ComboModelInt { Value = item.SiraNo.Value, Caption = item.MenuAdi });
            }
            ViewBag.cats = dct;
            ViewBag.YetkiGrupID = new SelectList(Management.CmbYetkiGruplari(), "Value", "Caption", Kullanici.YetkiGrupID);
            return View();
        }
        [HttpPost, ActionName("Yetkilendirme")]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Yetkilendirme(List<int> RolID, int KullaniciID, int YetkiGrupID)
        {
            RolID = RolID ?? new List<int>();
            Management.SetUserRoles(KullaniciID, RolID, YetkiGrupID);
            OnlineUsers.YetkiYenile(KullaniciID);
            MessageBox.Show("Yetkiler Kaydedildi", MessageBox.MessageType.Success);
            return RedirectToAction("Index");
        }
        public ActionResult getYetkiGrubuRolIDs(int id)
        {
            var rolIDs = db.YetkiGrupRolleris.Where(p => p.YetkiGrupID == id).Select(s => new { s.RolID, s.Roller.GorunurAdi }).ToList();
            return Json(rolIDs, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciBirimYetkileri(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var Birimlers = db.Birimlers.Where(p=>db.Birimlers.Any(a=>a.UstBirimID==p.BirimID) || p.IsVeriGirisiYapilabilir).OrderBy(o=>o.BirimAdi).ToList();
            var tBirimlers = Birimlers.ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
            //var mAgacs = db.sp_MaddeAgaci().ToList();
            //foreach (var item in tMaddelers)
            //{
            //    item.MaddeAdi = mAgacs.Where(p => p.MaddeID == item.MaddeID).FirstOrDefault().MaddeTreeAdi;
            //}
            var Kullanici = Management.GetUser(id.Value);
            ViewBag.YetkiliBirimleri = db.KullaniciBirimleris.Where(p => p.KullaniciID == id.Value).ToList();
            ViewBag.Kullanici = Kullanici;
            return View(tBirimlers);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciBirimYetkileri(List<int> BirimID, int KullaniciID, bool RaporBirimYetkilendirmeyeGit = false)
        {
            if (KullaniciID <= 0)
            {
                return RedirectToAction("Index");
            }
            BirimID = BirimID ?? new List<int>();
            var kMadde = db.KullaniciBirimleris.Where(p => p.KullaniciID == KullaniciID).ToList();
            db.KullaniciBirimleris.RemoveRange(kMadde);
            var kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).First();
            kul.KullaniciBirimleris = BirimID.Select(s => new KullaniciBirimleri { BirimID = s }).ToList();
            db.SaveChanges();
            OnlineUsers.YetkiYenile(kul.KullaniciID);
            if (RaporBirimYetkilendirmeyeGit)
            {
                return RedirectToAction("KullaniciRaporBirimYetkileri", new { id = kul.KullaniciID });
            }
            else
            {
                MessageBox.Show("Birim Yetkileri Kaydedildi", MessageBox.MessageType.Success);
                return RedirectToAction("Index");
            }

        }

        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciRaporBirimYetkileri(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var Birimlers = db.Birimlers.Where(p => db.Birimlers.Any(a => a.UstBirimID == p.BirimID) || p.IsVeriGirisiYapilabilir).OrderBy(o => o.BirimAdi).ToList();
            var tBirimlers = Birimlers.ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
            //var mAgacs = db.sp_MaddeAgaci().ToList();
            //foreach (var item in tMaddelers)
            //{
            //    item.MaddeAdi = mAgacs.Where(p => p.MaddeID == item.MaddeID).FirstOrDefault().MaddeTreeAdi;
            //}
            var Kullanici = Management.GetUser(id.Value);
            ViewBag.YetkiliBirimleri = db.KullaniciBirimleriRapors.Where(p => p.KullaniciID == id.Value).ToList();
            ViewBag.Kullanici = Kullanici;
            return View(tBirimlers);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciRaporBirimYetkileri(List<int> BirimID, int KullaniciID, bool YetkilendirmeyeGit = false)
        {
            if (KullaniciID <= 0)
            {
                return RedirectToAction("Index");
            }
            BirimID = BirimID ?? new List<int>();
            var kMadde = db.KullaniciBirimleriRapors.Where(p => p.KullaniciID == KullaniciID).ToList();
            db.KullaniciBirimleriRapors.RemoveRange(kMadde);
            var kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).First();
            kul.KullaniciBirimleriRapors = BirimID.Select(s => new KullaniciBirimleriRapor { BirimID = s }).ToList();
            db.SaveChanges();
            OnlineUsers.YetkiYenile(kul.KullaniciID);
            if (YetkilendirmeyeGit)
            {
                return RedirectToAction("Yetkilendirme", new { id = kul.KullaniciID });
            }
            else
            {
                MessageBox.Show("Rapor Birim Yetkileri Kaydedildi", MessageBox.MessageType.Success);
                return RedirectToAction("Index");
            }

        }


        public ActionResult ProfilBilgi(int KullaniciID, string dlgid)
        {
            var kulYet = RoleNames.Kullanicilar.InRole();
            var closePopup = false;
            if ((!kulYet && KullaniciID != UserIdentity.Current.Id) || KullaniciID == 0)
            {
                KullaniciID = UserIdentity.Current.Id;
            }
            var kul = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
            kul.ResimAdi = SistemAyar.getAyar(SistemAyar.KullaniciResimYolu) + "/" + kul.ResimAdi;
            var dataLog = db.KullanicilarLogs.Where(p => p.KullaniciID == kul.KullaniciID).OrderByDescending(o => o.LastLogonDate).FirstOrDefault();
            if (dataLog != null)
            {

                kul.LastLogonDate = dataLog.LastLogonDate;
                kul.LastLogonIP = dataLog.LastLogonIP;
            }

            ViewBag.closePopup = closePopup;

            ViewBag.BirimID = new SelectList(Management.CmbBirimler(), "Value", "Caption", kul.BirimID);

            return View(kul);
        }



        public ActionResult getPageData(int KullaniciID)
        {
            var data = db.Kullanicilars.Where(p => p.KullaniciID == KullaniciID).FirstOrDefault();
            return View(data);
        }
        public ActionResult SifreDegistir(string dlgid = "")
        {
            var mmsg = new MmMessage();
            mmsg.DialogID = dlgid;
            ViewBag.MmMessage = mmsg;

            ViewBag.EskiSifre = "";
            ViewBag.YeniSifre = "";
            ViewBag.YeniSifreTekrar = "";
            return View();
        }
        [HttpPost]
        public ActionResult SifreDegistir(string EskiSifre, string YeniSifre, string YeniSifreTekrar, string dlgid = "")
        {
            var mmsg = new MmMessage();
            mmsg.IsDialog = dlgid != "";
            mmsg.DialogID = dlgid;

            if (EskiSifre.IsNullOrWhiteSpace())
            {
                string msg = "Kullanmakta Olduğunuz Şifreyi Giriniz";
                mmsg.Messages.Add(msg);
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre" });
            }
            else mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EskiSifre" });
            if (YeniSifre.IsNullOrWhiteSpace())
            {
                string msg = "Yani Şifrenizi Giriniz";
                mmsg.Messages.Add(msg);
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre" });
            }
            else mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifre" });

            if (YeniSifreTekrar.IsNullOrWhiteSpace())
            {
                string msg = "Yeni Şifrenizi Tekrar Giriniz";
                mmsg.Messages.Add(msg);
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
            }
            else mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifreTekrar" });

            if (mmsg.Messages.Count == 0 && YeniSifre != YeniSifreTekrar)
            {
                string msg = "Yeni Şifre İle Yeni Şifre Tekrar Birbiriyle Uyuşmuyor";
                mmsg.Messages.Add(msg);
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre" });
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
            }
            var kullanici = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).First();
            if (mmsg.Messages.Count == 0 && kullanici.Sifre != EskiSifre.ComputeHash(Management.Tuz))
            {
                string msg = "Kullanmakta Olduğunuz Şifreyi Hatalı Girdiniz.";
                mmsg.Messages.Add(msg);
                mmsg.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre" });
            }
            if (mmsg.Messages.Count == 0)
            {

                kullanici.Sifre = YeniSifre.ComputeHash(Management.Tuz);
                db.SaveChanges();
                mmsg.IsSuccess = true;
                mmsg.IsCloseDialog = true;
                MessageBox.Show("Şifre Değitrime İşlemi", MessageBox.MessageType.Success, "Şifre Değiştirme İşlemi Başarılı");
            }
            else
            {
                MessageBox.Show("Hatalı İşlem", MessageBox.MessageType.Error, mmsg.Messages.ToArray());
            }




            ViewBag.MmMessage = mmsg;
            ViewBag.EskiSifre = EskiSifre;
            ViewBag.YeniSifre = YeniSifre;
            ViewBag.YeniSifreTekrar = YeniSifreTekrar;
            return View();
        }

        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.Kullanicilars.Where(p => p.KullaniciID == id).FirstOrDefault();

            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Ad + " " + kayit.Soyad + "' Kullanıcısı Silindi!";
                    db.Kullanicilars.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Ad + " " + kayit.Soyad + "' Kullanıcısı  Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "Kullanicilar/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kullanıcı sistemde bulunamadı!";
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
