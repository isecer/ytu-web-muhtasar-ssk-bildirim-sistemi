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
    [Authorize(Roles = RoleNames.Birimler)]
    public class BirimlerController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            return Index(new FmBirimler { PageSize = 20, VeriGirisTipID = 0 });
        }
        [HttpPost]
        public ActionResult Index(FmBirimler model)
        {

            var brmTree = db.sp_BirimAgaci().ToList();
            var brms = db.Birimlers.ToList();
            var q = (from s in brms
                     join bt in brmTree on s.BirimID equals bt.BirimID
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrBirimler
                     {
                         BirimID = s.BirimID,
                         UstBirimID = s.UstBirimID,
                         BirimAdi = s.BirimAdi,
                         BirimTreeAdi = bt.BirimTreeAdi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         IsVeriGirisiYapilabilir = s.IsVeriGirisiYapilabilir,
                         VeriGirisTipID = s.VeriGirisTipID,
                         VeriGirisTipleri = s.VeriGirisTipleri,
                         YeniUniteKodu = s.YeniUniteKodu,
                         EskiUniteKodu = s.EskiUniteKodu,
                         IsyeriSiraNumarasi = s.IsyeriSiraNumarasi,
                         IlKodu = s.IlKodu,
                         AltIsverenNumarasi = s.AltIsverenNumarasi,
                         IsAyAsiriHesaplama = s.IsAyAsiriHesaplama,
                         AyBaslangicGun = s.AyBaslangicGun,
                         GelecekAyBitisGun = s.GelecekAyBitisGun,
                         IsYevmiyeVeriGirisiYapilabilir = s.IsYevmiyeVeriGirisiYapilabilir,
                         VergiKimlikNo = s.VergiKimlikNo,

                     }).AsQueryable();
            if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.AltIsverenNumarasi == model.Aranan ||
                                                                     p.IsyeriSiraNumarasi == model.Aranan ||
                                                                     p.YeniUniteKodu == model.Aranan ||
                                                                     p.EskiUniteKodu == model.Aranan ||
                                                                     p.IlKodu == model.Aranan ||
                                                                     p.VergiKimlikNo == model.Aranan ||
                                                                     p.BirimTreeAdi.ToLower().Contains(model.Aranan.ToLower())
                                                                 );

            if (model.IsVeriGirisiYapilabilir.HasValue) q = q.Where(p => p.IsVeriGirisiYapilabilir == model.IsVeriGirisiYapilabilir.Value);
            if (model.IsYevmiyeVeriGirisiYapilabilir.HasValue) q = q.Where(p => p.IsYevmiyeVeriGirisiYapilabilir == model.IsYevmiyeVeriGirisiYapilabilir.Value);
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            if (model.VeriGirisTipID > 0 || !model.VeriGirisTipID.HasValue) q = q.Where(p => p.VeriGirisTipID == model.VeriGirisTipID);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.UstBirimID.HasValue).ThenBy(o => o.BirimTreeAdi);
            var IndexModel = new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Where(p => !p.IsAktif).Count() };
            IndexModel.Aktif = IndexModel.Toplam - IndexModel.Pasif;
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).ToList();
            ViewBag.IndexModel = IndexModel;
            ViewBag.IsAyAsiriHesaplama = new SelectList(Management.CmbVarYokData(), "Value", "Caption", model.IsAyAsiriHesaplama);
            ViewBag.IsAktif = new SelectList(Management.CmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            var IsVgT = Management.CmbVeriGirisTipleri();
            IsVgT.Insert(0, new ComboModelInt { Value = 0, Caption = "" });
            IsVgT[1].Caption = "Veri Girişi Olmayanlar";
            ViewBag.VeriGirisTipID = new SelectList(IsVgT, "Value", "Caption", model.VeriGirisTipID);

            return View(model);
        }
        public ActionResult Kayit(int? id, string dlgid)
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            ViewBag.MmMessage = MmMessage;
            var model = new Birimler();
            model.IsAktif = true;
            if (id.HasValue)
            {
                var data = db.Birimlers.Where(p => p.BirimID == id).FirstOrDefault();
                if (data != null)
                {
                    model = data;
                }
            }

            ViewBag.UstBirimID = new SelectList(Management.CmbUstBirimlerTree(true, model.BirimID), "Value", "Caption", model.UstBirimID);
            ViewBag.VeriGirisTipID = new SelectList(Management.CmbVeriGirisTipleri(), "Value", "Caption", model.VeriGirisTipID);
            ViewBag.Secilenler = null;
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(Birimler kModel, string dlgid = "")
        {
            var MmMessage = new MmMessage() { IsDialog = !dlgid.IsNullOrWhiteSpace(), DialogID = dlgid };
            #region Kontrol

            if (kModel.BirimAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Birim adı boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimAdi" });


            if (kModel.IsVeriGirisiYapilabilir)
            {
                if (!kModel.VeriGirisTipID.HasValue)
                {
                    MmMessage.Messages.Add("Veri giriş tipini seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VeriGirisTipID" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VeriGirisTipID" });

                if (kModel.YeniUniteKodu.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Yeni ünite kodu boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniUniteKodu" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniUniteKodu" });
                if (kModel.EskiUniteKodu.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Eski ünite kodu boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiUniteKodu" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EskiUniteKodu" });
                if (kModel.IsyeriSiraNumarasi.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("İşyeri sıra numarası boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IsyeriSiraNumarasi" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IsyeriSiraNumarasi" });
                if (kModel.IlKodu.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("İl kodu boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IlKodu" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IlKodu" });
                if (kModel.AltIsverenNumarasi.IsNullOrWhiteSpace())
                {
                    MmMessage.Messages.Add("Alt işveren numarası boş bırakılamaz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AltIsverenNumarasi" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AltIsverenNumarasi" });

                if (kModel.IsAyAsiriHesaplama)
                {
                    if (!kModel.AyBaslangicGun.HasValue)
                    {
                        MmMessage.Messages.Add("Ay Aşırı hesaplama için Ay Başlangıç Gün bilgisini giriniz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyBaslangicGun" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AyBaslangicGun" });
                    if (!kModel.GelecekAyBitisGun.HasValue)
                    {
                        MmMessage.Messages.Add("Ay Aşırı hesaplama için Gelecek Ay Bitiş Gün bilgisini giriniz.");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GelecekAyBitisGun" });
                    }
                    else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GelecekAyBitisGun" });
                }
            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {

                if (!kModel.IsVeriGirisiYapilabilir)
                {
                    kModel.VeriGirisTipID = null;
                    kModel.YeniUniteKodu = null;
                    kModel.EskiUniteKodu = null;
                    kModel.IsyeriSiraNumarasi = null;
                    kModel.IlKodu = null;
                    kModel.AltIsverenNumarasi = null;
                    kModel.IsAyAsiriHesaplama = false;
                    kModel.AyBaslangicGun = null;
                    kModel.GelecekAyBitisGun = null;
                }
                if (!kModel.IsYevmiyeVeriGirisiYapilabilir)
                {
                    kModel.VergiKimlikNo = null;
                }


                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var Table = new Birimler();
                if (kModel.BirimID <= 0)
                {
                    kModel.IsAktif = true;
                    Table = db.Birimlers.Add(kModel);
                }
                else
                {
                    Table = db.Birimlers.Where(p => p.BirimID == kModel.BirimID).First();
                    Table.BirimAdi = kModel.BirimAdi.Trim();
                    Table.UstBirimID = kModel.UstBirimID;
                    Table.IsVeriGirisiYapilabilir = kModel.IsVeriGirisiYapilabilir;
                    Table.VeriGirisTipID = kModel.VeriGirisTipID;
                    Table.YeniUniteKodu = kModel.YeniUniteKodu;
                    Table.EskiUniteKodu = kModel.EskiUniteKodu;
                    Table.IsyeriSiraNumarasi = kModel.IsyeriSiraNumarasi;
                    Table.IlKodu = kModel.IlKodu;
                    Table.AltIsverenNumarasi = kModel.AltIsverenNumarasi;
                    Table.IsAyAsiriHesaplama = kModel.IsAyAsiriHesaplama;
                    Table.AyBaslangicGun = kModel.AyBaslangicGun;
                    Table.GelecekAyBitisGun = kModel.GelecekAyBitisGun;
                    Table.IsYevmiyeVeriGirisiYapilabilir = kModel.IsYevmiyeVeriGirisiYapilabilir;
                    Table.VergiKimlikNo = kModel.VergiKimlikNo;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = kModel.IslemTarihi;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.UstBirimID = new SelectList(Management.CmbUstBirimlerTree(), "Value", "Caption", kModel.UstBirimID);
            ViewBag.VeriGirisTipID = new SelectList(Management.CmbVeriGirisTipleri(), "Value", "Caption", kModel.VeriGirisTipID);
            ViewBag.MmMessage = MmMessage;
            return View(kModel);
        }





        public ActionResult Sil(int id)
        {
            var kayit = db.Birimlers.Where(p => p.BirimID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.BirimAdi + "' İsimli Birim Silindi!";
                    db.Birimlers.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.BirimAdi + "' İsimli Birim Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "Birimler/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Birim sistemde bulunamadı!";
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
