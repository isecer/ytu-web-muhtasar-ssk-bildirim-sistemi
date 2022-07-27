using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using Database;
using BiskaUtil;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SurecIslemleri)]
    public class SurecIslemleriController : Controller
    {

        // GET: SurecIslemleri 

        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmSurecIslemleri() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmSurecIslemleri model)
        {
            var nowDate = DateTime.Now.Date;
            var q = from s in db.VASurecleris
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    select new FrSurecIslemleri
                    {
                        VASurecID = s.VASurecID,
                        SurecYilAdi = s.Yil + " Yılı Süreci",
                        Yil = s.Yil,
                        BaslangicTarihi = s.BaslangicTarihi,
                        BitisTarihi = s.BitisTarihi,
                        IsAktif = s.IsAktif,
                        IslemYapanID = s.IslemYapanID,
                        IslemYapan = k.KullaniciAdi,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanIP = s.IslemYapanIP,
                        AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                    };
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(t => t.Yil).ThenByDescending(t => t.BaslangicTarihi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Where(p => !p.IsAktif).Count() };
            IndexModel.Aktif = IndexModel.Toplam - IndexModel.Pasif;
            ViewBag.IndexModel = IndexModel;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new KmSurecIslemleri();
            model.IsAktif = true;
            if (id.HasValue && id > 0)
            {
                var data = db.VASurecleris.Where(p => p.VASurecID == id).FirstOrDefault();

                if (data != null)
                {
                    model.VASurecID = data.VASurecID;
                    model.Yil = data.Yil;
                    model.BaslangicTarihi = data.BaslangicTarihi;
                    model.BitisTarihi = data.BitisTarihi;
                    model.IsAktif = data.IsAktif;
                    model.IslemTarihi = DateTime.Now;
                    model.IslemYapanID = data.IslemYapanID;
                    model.IslemYapanIP = data.IslemYapanIP;
                }

            }
            else
            {
                model.Yil = DateTime.Now.Year;
            }

            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.Yil = new SelectList(Management.CmbSurecKayitYillari(), "Value", "Caption", model.Yil);

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult Kayit(KmSurecIslemleri kModel, bool IsSinavVar = false, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };

            #region Kontrol  
            if (kModel.Yil <= 0)
            {
                MmMessage.Messages.Add("Süreç Yılını Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });

            }
            else if (db.VASurecleris.Any(p => p.VASurecID != kModel.VASurecID && p.Yil == kModel.Yil))
            {
                MmMessage.Messages.Add("Seçtiğiniz Süreç Yılı Daha Önceden Kayıt Edilmiştir. Tekrar Kayıt Edilemez.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            }
            if (!kModel.BaslangicTarihi.HasValue || !kModel.BitisTarihi.HasValue)
            {
                if (!kModel.BaslangicTarihi.HasValue)
                {
                    MmMessage.Messages.Add("Başlangıç Tarihi Seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });

                }
                if (!kModel.BitisTarihi.HasValue)
                {
                    MmMessage.Messages.Add("Bitiş Tarihi Seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });

                }

            }
            else if (kModel.BaslangicTarihi >= kModel.BitisTarihi)
            {
                MmMessage.Messages.Add("Başlangıç Tarihi Bitiş Tarihinden Büyük Yada Eşit Olamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BaslangicTarihi" });
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BitisTarihi" });
            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Yil = kModel.Yil;
                var Table = new VASurecleri();
                if (kModel.VASurecID <= 0)
                {
                    Table = db.VASurecleris.Add(new VASurecleri
                    {

                        Yil = kModel.Yil,
                        BaslangicTarihi = kModel.BaslangicTarihi.Value,
                        BitisTarihi = kModel.BitisTarihi.Value,
                        IsAktif = kModel.IsAktif,
                        IslemTarihi = kModel.IslemTarihi,
                        IslemYapanID = kModel.IslemYapanID,
                        IslemYapanIP = kModel.IslemYapanIP,
                        VASurecleriAylars = db.Aylars.ToList().Select(s => new VASurecleriAylar
                        {
                            AyID = s.AyID,
                            AyDurumID = 3,
                            IslemTarihi = kModel.IslemTarihi,
                            IslemYapanID = kModel.IslemYapanID,
                            IslemYapanIP = kModel.IslemYapanIP

                        }).ToList()

                    });
                    kModel.BirimleriKopyala = true;
                }
                else
                {
                    Table = db.VASurecleris.Where(p => p.VASurecID == kModel.VASurecID).First();
                    Table.Yil = kModel.Yil;
                    Table.BaslangicTarihi = kModel.BaslangicTarihi.Value;
                    Table.BitisTarihi = kModel.BitisTarihi.Value;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = DateTime.Now;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;
                }

                
                db.SaveChanges();
                if (kModel.BirimleriKopyala) BirimleriKopyala(Table.VASurecID);

                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(Management.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }




        public void BirimleriKopyala(int VASurecID)
        {


            var EODonem = db.VASurecleris.Where(p => p.VASurecID == VASurecID).First();
            var VASurecleriBirimler = EODonem.VASurecleriBirims.Where(p => p.VASurecID == VASurecID).ToList();
            var Birimlers = db.Birimlers.Where(p => p.IsAktif && p.IsVeriGirisiYapilabilir).ToList();
            var EklenecekBirimler = Birimlers.Where(p => !EODonem.VASurecleriBirims.Any(a => a.BirimID == p.BirimID)).ToList();
            var VarolanBirimlers = VASurecleriBirimler.Where(p => Birimlers.Any(a => a.BirimID == p.BirimID)).ToList();
            var SilinecekBirimler = VASurecleriBirimler.Where(p => !Birimlers.Any(a => a.BirimID == p.BirimID)).ToList();
            if (EklenecekBirimler.Any())
                db.VASurecleriBirims.AddRange(EklenecekBirimler.Select(item => new VASurecleriBirim
                {
                    VASurecID = VASurecID,
                    BirimID = item.BirimID,
                    UstBirimID = item.UstBirimID,
                    IsVeriGirisiYapilabilir = item.IsVeriGirisiYapilabilir,
                    VeriGirisTipID = item.VeriGirisTipID,
                    YeniUniteKodu = item.YeniUniteKodu,
                    EskiUniteKodu = item.EskiUniteKodu,
                    IsyeriSiraNumarasi = item.IsyeriSiraNumarasi,
                    IlKodu = item.IlKodu,
                    AltIsverenNumarasi = item.AltIsverenNumarasi,
                    IsAyAsiriHesaplama=item.IsAyAsiriHesaplama,
                    AyBaslangicGun=item.AyBaslangicGun,
                    GelecekAyBitisGun=item.GelecekAyBitisGun,
                    IsAktif = true,
                    IslemTarihi = DateTime.Now,
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                }));
            foreach (var item in VarolanBirimlers)
            {
                var Birim = Birimlers.Where(p => p.BirimID == item.BirimID).First();

                item.VASurecID = VASurecID;
                item.BirimID = Birim.BirimID;
                item.UstBirimID = Birim.UstBirimID;
                item.IsVeriGirisiYapilabilir = Birim.IsVeriGirisiYapilabilir;
                item.VeriGirisTipID = Birim.VeriGirisTipID;
                item.YeniUniteKodu = Birim.YeniUniteKodu;
                item.EskiUniteKodu = Birim.EskiUniteKodu;
                item.IsyeriSiraNumarasi = Birim.IsyeriSiraNumarasi;
                item.IlKodu = Birim.IlKodu;
                item.AltIsverenNumarasi = Birim.AltIsverenNumarasi;
                item.IsAyAsiriHesaplama = Birim.IsAyAsiriHesaplama;
                item.AyBaslangicGun = Birim.AyBaslangicGun;
                item.GelecekAyBitisGun = Birim.GelecekAyBitisGun;
                item.IslemTarihi = DateTime.Now;
                item.IslemYapanID = UserIdentity.Current.Id;
                item.IslemYapanIP = UserIdentity.Ip;


            }
            foreach (var item in SilinecekBirimler)
                item.IsAktif = false;
            db.SaveChanges();



        }

        public ActionResult GetDetail(int id, int tbInx)
        {
            var mdl = (from s in db.VASurecleris.Where(p => p.VASurecID == id)
                       join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                       select new FrSurecIslemleri
                       {
                           VASurecID = s.VASurecID,
                           SurecYilAdi = s.Yil + " Yılı Süreci",
                           Yil = s.Yil,
                           BaslangicTarihi = s.BaslangicTarihi,
                           BitisTarihi = s.BitisTarihi,
                           IsAktif = s.IsAktif,
                           IslemYapanID = s.IslemYapanID,
                           IslemYapan = k.KullaniciAdi,
                           IslemTarihi = s.IslemTarihi,
                           IslemYapanIP = s.IslemYapanIP,

                       }).First();
            mdl.SelectedTabIndex = tbInx;
            var page = Management.RenderPartialView("SurecIslemleri", "DetaySablon", mdl);
            return Json(new { page = page, IsAuthenticated = UserIdentity.Current.IsAuthenticated }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult DetaySablon(FrSurecIslemleri model)
        {
            return View(model);
        }
        public ActionResult GetSubData(int id, int? SelectDurumID, int? SelectAyID, int tbInx)
        {
            var nowDate = DateTime.Now.Date;
            string page = "";
            var mdl = (from s in db.VASurecleris.Where(p => p.VASurecID == id)
                       join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                       select new FrSurecIslemleri
                       {
                           VASurecID = s.VASurecID,
                           SurecYilAdi = s.Yil + " Yılı Süreci",
                           Yil = s.Yil,
                           BaslangicTarihi = s.BaslangicTarihi,
                           BitisTarihi = s.BitisTarihi,
                           IsAktif = s.IsAktif,
                           IslemYapanID = s.IslemYapanID,
                           IslemYapan = k.KullaniciAdi,
                           IslemTarihi = s.IslemTarihi,
                           IslemYapanIP = s.IslemYapanIP,
                           AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                       }).First();
            if (tbInx == 1)
            {
                #region AyBilgileri 
                mdl.AyData = db.VASurecleriAylars.Include("Aylar").Where(p => p.VASurecID == id).ToList();
                mdl.SListAyDurum = new SelectList(Management.CmbAyDurumlari(false), "Value", "Caption");
                #endregion
                page = Management.RenderPartialView("SurecIslemleri", "GetAyBilgileri", mdl);

            }
            else if (tbInx == 2)
            {


                #region BirimBilgileri 
                mdl.SBirimDurumID = SelectDurumID;
                mdl.SAyID = SelectAyID ?? DateTime.Now.Month;
                var mBirimIDs = db.VASurecleriBirims.Select(s => s.BirimID).Distinct().ToList();
                mdl.BirimData = (from s in db.VASurecleriBirims.Where(p => p.VASurecID == id && mBirimIDs.Contains(p.BirimID))
                                 join bt in db.Vw_BirimlerTree on s.BirimID equals bt.BirimID
                                 select new FrBirimler
                                 {
                                     BirimID = s.BirimID,
                                     BirimAdi = bt.BirimTreeAdi,
                                     IslemTarihi = s.IslemTarihi,
                                     VeriGirisTipAdi = bt.VeriGirisTipAdi,
                                     VeriGirisVar = s.VASurecleriBirimVerileris.Any(a => a.AyID == mdl.SAyID)

                                 }).Where(p => p.VeriGirisVar == (mdl.SBirimDurumID.HasValue ? (mdl.SBirimDurumID == 1 ? true : false) : p.VeriGirisVar)).OrderBy(o => o.BirimAdi).ToList();
                mdl.SListBirimDurum = new SelectList(Management.CmbSurecBirimDurum(), "Value", "Caption", mdl.SBirimDurumID);

                mdl.SListAy = new SelectList(Management.CmbAylar(false), "Value", "Caption", mdl.SAyID);

                #endregion
                page = Management.RenderPartialView("SurecIslemleri", "GetBirimBilgileri", mdl);
            }
            return
                new
                {
                    page = page,
                    IsAuthenticated = UserIdentity.Current.IsAuthenticated
                }.ToJsonResult();
        }


        public ActionResult GetBirimBilgileri(FrSurecIslemleri model)
        {
            return View(model);
        }

        public ActionResult GetMaddeBilgileri(FrSurecIslemleri model)
        {
            return View(model);
        }
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult AyDurumuGuncelle(int id, int AyDurumID)
        {
            var kayit = db.VASurecleriAylars.Where(p => p.VASurecleriAyID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    kayit.AyDurumID = AyDurumID;
                    db.SaveChanges(); 
                    message = "'" + kayit.Aylar.AyAdi + "' ayı için durum bilgisi '" + kayit.AyDurumlari.DurumAdi + "' şeklinde güncellendi.";
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Aylar.AyAdi + "' ayı için durum güncelleme işlemi yapılamadı! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "SurecIslemleri/AyDurumGuncelle<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
            else
            {
                success = false;
                message = "Güncellemek istediğiniz Ay sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var mmMessage = new MmMessage();

            var kayit = db.VASurecleris.Where(p => p.VASurecID == id).FirstOrDefault();

            string message = "";
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Yil + "' Yılı Süreci Silindi!";
                    db.VASurecleris.Remove(kayit);
                    db.SaveChanges();
                    mmMessage.Title = "Uyarı";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    message = "'" + kayit.Yil + "' Yılı Süreci Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "SürecIslemleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    mmMessage.Title = "Hata";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Error;
                    mmMessage.IsSuccess = false;
                }
            }
            else
            {
                message = "Silmek İstediğiniz Süreç Sistemde Bulunamadı!";
                mmMessage.Title = "Hata";
                mmMessage.Messages.Add(message);
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.IsSuccess = true;
            }
            var strView = Management.RenderPartialView("Ajax", "getMessage", mmMessage);
            return Json(new { IsSuccess = mmMessage.IsSuccess, Messages = strView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}