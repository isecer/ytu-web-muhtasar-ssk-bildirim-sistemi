using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.Yevmiyeler)]
    public class YevmiyelerController : Controller
    {
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            var fModel = new FmYevmiyeler { PageSize = 15 };
            var BirimID = UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler];
            var Yil = UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler];
            fModel.Expand = Yil.HasValue || BirimID.HasValue;
            fModel.YevmiyeHarcamaBirimID = BirimID;
            fModel.Yil = Yil;
            return Index(fModel);
        }

        [HttpPost]
        public ActionResult Index(FmYevmiyeler model, bool export = false)
        {

            var BirimIDs = UserIdentity.Current.BirimYetkileri;
            UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler] = model.YevmiyeHarcamaBirimID;
            UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler] = model.Yil;


            var q = (from s in db.Yevmiyelers
                     join b in db.YevmiyelerHarcamaBirimleris on s.YevmiyeHarcamaBirimID equals b.YevmiyeHarcamaBirimID
                     select new FrYevmiyeler
                     {
                         YevmiyeID = s.YevmiyeID,
                         YevmiyeTarih = s.YevmiyeTarih,
                         YevmiyeNo = s.YevmiyeNo,
                         YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID,
                         BirimAdi = b.BirimAdi,
                         HarcamaBirimAdi = s.HarcamaBirimAdi,
                         VergiKimlikNo = s.VergiKimlikNo,
                         HarcamaBirimKod = s.HarcamaBirimKod,
                         HesapKod = s.HesapKod,
                         HesapAdi = s.HesapAdi,
                         Borc = s.Borc,
                         Alacak = s.Alacak,
                         Aciklama = s.Aciklama,
                     }).AsQueryable();
            if (model.Yil.HasValue) q = q.Where(p => p.YevmiyeTarih.Year == model.Yil);
            if (model.YevmiyeNo.HasValue) q = q.Where(p => p.YevmiyeNo == model.YevmiyeNo);
            if (model.YevmiyeHarcamaBirimID.HasValue) q = q.Where(p => p.YevmiyeHarcamaBirimID == model.YevmiyeHarcamaBirimID);
            if (!model.HarcamaBirimKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HarcamaBirimKod == model.HarcamaBirimKod);
            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod == model.HesapKod || p.HesapAdi.Contains(model.HesapKod));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama == model.Aciklama);

            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderBy(o => o.YevmiyeTarih).ThenBy(t => t.BirimAdi);
            var PS = Management.SetStartRowInx(model.StartRowIndex, model.PageIndex, model.PageCount, model.RowCount, model.PageSize);
            model.PageIndex = PS.PageIndex;
            model.Data = q.Skip(PS.StartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi() { Toplam = 0, Pasif = 0, Aktif = 0 };
            ViewBag.IndexModel = IndexModel;
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.Yil);
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(model.Yil, true), "Value", "Caption", model.YevmiyeHarcamaBirimID);


            return View(model);
        }

        public ActionResult GetExcelYukle(int Yil)
        {


            var model = new YevmiyeVeriGirisPopupExcelModel();
            model.Yil = Yil;
            var page = Management.RenderPartialView("Yevmiyeler", "ShowExcelYukle", model);

            return Json(new
            {
                page = page,
                IsAuthenticated = UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);

        }
        [Authorize(Roles = RoleNames.YevmiyelerKayitYetkisi)]
        public ActionResult ShowExcelYukle(int Yil)
        {
            var model = new YevmiyeVeriGirisPopupExcelModel();
            model.Yil = Yil;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.YevmiyelerKayitYetkisi)]
        public ActionResult ExcelYuklePost(int Yil, HttpPostedFileBase DosyaEki)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Muhtasar ve SSK Bildirimi excel verisi yükleme işlemi";
            mMessage.MessageType = Msgtype.Warning;
            if (DosyaEki == null)
            {
                mMessage.Messages.Add("Excel veri dosyası ekleyiniz.");
            }
            else
            {
                string extension = Path.GetExtension(DosyaEki.FileName);
                if (extension != ".xls" && extension != ".xlsx")
                {
                    mMessage.Messages.Add(DosyaEki.FileName + " doyasının excel formatında olması gerekmektedir. Eki kaldırın ve Excel formatında tekrar ekleyiniz.");
                }
            }

            if (mMessage.Messages.Count == 0)
            {
                var model = DosyaEki.ToYevmiyeIterateRows(Yil);




                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }

                if (mMessage.Messages.Count == 0)
                {
                    var Birimler = db.YevmiyelerHarcamaBirimleris.ToList();

                    try
                    {
                        #region BildirimData 
                        model.Data = (from s in model.Data
                                      join b in Birimler on s.VergiKimlikNo equals b.VergiKimlikNo into defB
                                      from B in defB.DefaultIfEmpty()
                                      select new ExcelDataImportYevmiyeRow
                                      {
                                          SayfaNo = s.SayfaNo,
                                          SatirNo = s.SatirNo,
                                          YevmiyeTarih = s.YevmiyeTarih,
                                          YevmiyeNo = s.YevmiyeNo,
                                          VergiKimlikNo = s.VergiKimlikNo,
                                          YevmiyeHarcamaBirimID = (B != null ? (int?)B.YevmiyeHarcamaBirimID : null),
                                          HarcamaBirimAdi = s.HarcamaBirimAdi,
                                          HarcamaBirimKod = s.HarcamaBirimKod,
                                          HesapKod = s.HesapKod,
                                          HesapAdi = s.HesapAdi,
                                          Borc = s.Borc,
                                          Alacak = s.Alacak,
                                          Aciklama = s.Aciklama,
                                          IslemYapanID = UserIdentity.Current.Id,
                                          IslemYapanIP = UserIdentity.Ip,
                                          IslemTarihi = DateTime.Now,
                                      }).ToList();

                        foreach (var item in model.Data)
                        {
                            List<string> hataTipi = new List<string>();
                            if (!item.YevmiyeTarih.HasValue)
                            {
                                hataTipi.Add("Yevmiye tarihi boş");
                                item.HataliHucreler.Add(0);
                            }
                            else if (item.YevmiyeTarih.Value.Year != Yil)
                            {
                                hataTipi.Add("Yevmiye tarihi yüklenecek yıl ile uyuşmuyor");
                                item.HataliHucreler.Add(0);
                            }

                            if (!item.YevmiyeNo.HasValue)
                            {
                                hataTipi.Add("Yevmiye no boş");
                                item.HataliHucreler.Add(1);
                            }

                            if (item.VergiKimlikNo.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Vergi kimlik numarası boş");
                                item.HataliHucreler.Add(2);
                            }
                            else if (!item.YevmiyeHarcamaBirimID.HasValue || item.HarcamaBirimAdi.IsNullOrWhiteSpace())
                            {
                                var msg = "";
                                if (!item.YevmiyeHarcamaBirimID.HasValue) msg = "Harcama Birimi Vergi kimlik numarası sistemdeki hiçbir harcama Birim ile uyuşmuyor";
                                if (item.HarcamaBirimAdi.IsNullOrWhiteSpace()) msg += (msg.IsNullOrWhiteSpace() ? "" : ",") + "Harcama birim adı boş";
                                hataTipi.Add(msg);
                                item.HataliHucreler.Add(2);
                            }
                            if (item.HarcamaBirimKod.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Harcama Birim Kodu boş");
                                item.HataliHucreler.Add(3);
                            }
                            if (item.HesapKod.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Hesap Kodu boş");
                                item.HataliHucreler.Add(4);
                            }
                            if (item.HesapAdi.IsNullOrWhiteSpace())
                            {
                                hataTipi.Add("Hesap Adı boş");
                                item.HataliHucreler.Add(5);
                            }
                            if (!(item.Borc > 0) && !(item.Alacak > 0))
                            {
                                hataTipi.Add("Borç ve Alacak aynı anda boş veya 0 olamaz olamaz");
                                item.HataliHucreler.Add(6);
                                item.HataliHucreler.Add(7);
                            }
                            else if ((item.Borc > 0) && (item.Alacak > 0))
                            {
                                hataTipi.Add("Borç ve Alacak aynı anda dolu olamaz");
                                item.HataliHucreler.Add(6);
                                item.HataliHucreler.Add(7);
                            }

                            if (hataTipi.Count > 0)
                            {
                                var Hatalar = string.Join(", ", hataTipi);
                                item.HataAciklamasi = Hatalar;
                            }
                        }

                        if (!model.Data.Any(a => a.HataliHucreler.Any()))
                        {
                            model.Data = model.Data.Where(p => !p.HataliHucreler.Any()).ToList();

                            var addYevmiyes = model.Data.Select(s => new Yevmiyeler
                            {
                                YevmiyeTarih = s.YevmiyeTarih.Value,
                                YevmiyeNo = s.YevmiyeNo.Value,
                                VergiKimlikNo = s.VergiKimlikNo,
                                YevmiyeHarcamaBirimID = s.YevmiyeHarcamaBirimID.Value,
                                HarcamaBirimAdi = s.HarcamaBirimAdi,
                                HarcamaBirimKod = s.HarcamaBirimKod,
                                HesapKod = s.HesapKod,
                                HesapAdi = s.HesapAdi,
                                Borc = s.Borc.Value,
                                Alacak = s.Alacak.Value,
                                Aciklama = s.Aciklama,
                                IslemYapanID = s.IslemYapanID,
                                IslemYapanIP = s.IslemYapanIP,
                                IslemTarihi = s.IslemTarihi,
                            }).ToList();
                            db.Yevmiyelers.AddRange(addYevmiyes);
                            db.Yevmiyelers.RemoveRange(db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == Yil));
                            db.SaveChanges();
                            mMessage.IsSuccess = true;
                            mMessage.Messages.Add("Yevmiye verileri yükleme işlemi başarılı. Toplam: " + model.Data.Count + " Kalem bilgi sisteme işlendi.");
                            mMessage.MessageType = Msgtype.Success;
                        }
                        else
                        {
                            var excpt = model.YevmiyeAktarilanExcelHataKontrolu();

                            if (excpt == null)
                            {

                                mMessage.Messages.Add("<span style='color:red;'>Excel dosyasındaki bazı veriler düzgün girilmemiştir. Aşağıdaki dosyayı indirip kontrol ediniz lütfen.</span>");

                                mMessage.Messages.Add("<a style='color:red;' href='" + model.DosyaYolu + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> " + model.DosyaAdi + "</a>");

                            }
                            else
                            {
                                var msg = "Excel dosyası düzenlenirken bir hata oluştu! Hata:" + excpt.ToExceptionMessage();
                                mMessage.Messages.Add(msg);
                                Management.SistemBilgisiKaydet(msg, "DersIslemleri/FileDataDEOSave", BilgiTipi.Hata);
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        var msg = "Yevmiye verileri yükleme işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        Management.SistemBilgisiKaydet(msg, "Yevmiyeler/ExcelYuklePost", BilgiTipi.Hata);

                    }
                }


            }
            return mMessage.ToJsonResult();
        }

        public ActionResult GetDetail(int id)
        { 
            var mdl = db.Yevmiyelers.Where(p => p.YevmiyeID == id).Select(s => new YevmiyeDetayModel
            {

                YevmiyeID = s.YevmiyeID,
                YevmiyeTarih = s.YevmiyeTarih,
                YevmiyeNo = s.YevmiyeNo,
                VergiKimlikNo = s.VergiKimlikNo,
                HarcamaBirimKod = s.HarcamaBirimKod,
                HarcamaBirimAdi = s.HarcamaBirimAdi,
                HesapKod = s.HesapKod,
                HesapAdi = s.HesapAdi,
                Borc = s.Borc,
                Alacak = s.Alacak,
                Yevmiyeler1003BAyristirmalari = s.Yevmiyeler1003BAyristirmalari,
                YevmiyelerTasinirKontrolTifKaydis=s.YevmiyelerTasinirKontrolTifKaydis
            }).First();
            var GelirVergisiHesapKods = new List<string> { "360.01.01.01", "360.01.01.02" };
            mdl.YevmiyeNoToplamGv = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == mdl.YevmiyeTarih.Year && p.YevmiyeNo == mdl.YevmiyeNo && GelirVergisiHesapKods.Contains(p.HesapKod)).Sum(s => (decimal?)s.Alacak);
            var DamgaVergisiHesapKods = new List<string> { "360.03.01" };
            mdl.YevmiyeNoToplamDv = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == mdl.YevmiyeTarih.Year && p.YevmiyeNo == mdl.YevmiyeNo && DamgaVergisiHesapKods.Contains(p.HesapKod)).Sum(s => (decimal?)s.Alacak);
            return View(mdl);
        }

        public ActionResult YevmiyeAyristir(int YevmiyeID, int? Yevmiye1003BAyristirmaID = null)
        {
            var model = new Yevmiyeler1003BAyristirmalari();
            if (Yevmiye1003BAyristirmaID > 0)
            {
                model = db.Yevmiyeler1003BAyristirmalari.Where(p => p.YevmiyeID == YevmiyeID && p.Yevmiye1003BAyristirmaID == Yevmiye1003BAyristirmaID).First();
            }
            //var YilYevmiyeToplamAlacak=db.Yevmiyelers.Where(p=>p.)
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbBirimlerUniversiteIsYerleri(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);
            ViewBag.Yil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.Yil);
            ViewBag.AyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.AyID);
            ViewBag.YevmiyeBelgeKodID = new SelectList(Management.CmbYevmiyeBelgeKodlari(true), "Value", "Caption", model.YevmiyeBelgeKodID);

            return View(model);
        }

        [HttpPost]
        public ActionResult YevmiyeAyristir(Yevmiyeler1003BAyristirmalari kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003B Yevmiye Ayrıştırma İşlemi";
            MmMessage.MessageType = Msgtype.Warning; 
            if (kModel.YevmiyeHarcamaBirimID <= 0)
            {
                MmMessage.Messages.Add("İş Yeri Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeHarcamaBirimID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeHarcamaBirimID" });
            if (kModel.Yil <= 0)
            {
                MmMessage.Messages.Add("Yıl Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            if (kModel.AyID <= 0)
            {
                MmMessage.Messages.Add("Ay Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AyID" });
            if (kModel.YevmiyeBelgeKodID <= 0)
            {
                MmMessage.Messages.Add("Belge Kodu Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeBelgeKodID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeBelgeKodID" });

            if (kModel.Matrah <= 0)
            {
                MmMessage.Messages.Add("Matrah 0'Dan Büyük Olmalı.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Matrah" }); 
            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.Yevmiyeler1003BAyristirmalari.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.Yil == kModel.Yil && a.AyID == kModel.AyID && a.Yevmiye1003BAyristirmaID != kModel.Yevmiye1003BAyristirmaID);
                if (YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Seçilen Yıl ve Ay İçin Daha Önceden Ayrıştırma İşlemi Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyID" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "Yil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "AyID" });

                    var AyristirmaMatrahToplam = db.Yevmiyeler1003BAyristirmalari.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.Yevmiye1003BAyristirmaID != kModel.Yevmiye1003BAyristirmaID).Sum(s => (decimal?)s.Matrah);
                    var YevmiyeAlacakToplam = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First().Alacak;
                    AyristirmaMatrahToplam = AyristirmaMatrahToplam + kModel.Matrah;
                    if (AyristirmaMatrahToplam > YevmiyeAlacakToplam)
                    {
                        MmMessage.Messages.Add("Tüm Ayrıştırma İşlemlerinin Matrahları Toplamı Yevmiyenin Alacak Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
                    }
                } 
            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.Yevmiye1003BAyristirmaID <= 0)
                {
                    db.Yevmiyeler1003BAyristirmalari.Add(kModel);
                }
                else
                {
                    var data = db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yevmiye1003BAyristirmaID == kModel.Yevmiye1003BAyristirmaID).First();
                    data.YevmiyeHarcamaBirimID = kModel.YevmiyeHarcamaBirimID;
                    data.Yil = kModel.Yil;
                    data.AyID = kModel.AyID;
                    data.YevmiyeBelgeKodID = kModel.YevmiyeBelgeKodID;
                    data.Matrah = kModel.Matrah;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                MmMessage.Messages.Add("Yevmiye Ayrıştırma İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            } 
            return MmMessage.ToJsonResult();
        }

        public ActionResult YevmiyeTasinirkontrolTifKayit(int YevmiyeID, int? YevmiyelerTasinirKontrolTifKayitID = null)
        {
            var model = new YevmiyelerTasinirKontrolTifKaydi();
            if (YevmiyelerTasinirKontrolTifKayitID > 0)
            {
                model = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyeID == YevmiyeID && p.YevmiyelerTasinirKontrolTifKayitID == YevmiyelerTasinirKontrolTifKayitID).First();
            }
            //var YilYevmiyeToplamAlacak=db.Yevmiyelers.Where(p=>p.) 

            return View(model);
        } 
        [HttpPost]
        public ActionResult YevmiyeTasinirkontrolTifKayit(YevmiyelerTasinirKontrolTifKaydi kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003A Taşınır Kontrol Tif Kaydı";
            MmMessage.MessageType = Msgtype.Warning;

            if (kModel.TifNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Tif Numarası Giriniz");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TifNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "TifNo" });
            if (kModel.Tutar <= 0)
            {
                MmMessage.Messages.Add("Tutar 0'Dan Büyük Olmalıdır.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tutar" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tutar" });


            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.YevmiyelerTasinirKontrolTifKaydis.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.TifNo == kModel.TifNo && a.YevmiyelerTasinirKontrolTifKayitID != kModel.YevmiyelerTasinirKontrolTifKayitID);
                if (YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Girilen Tif Numarası İçin Daha Önceden Kayıt Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TifNo" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "TifNo" });

                    //var AyristirmaMatrahToplam = db.Yevmiyeler1003BAyristirmalari.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.YevmiyelerTasinirKontrolTifKayitID != kModel.YevmiyelerTasinirKontrolTifKayitID).Sum(s => (decimal?)s.Matrah);
                    //var YevmiyeBorcToplam = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First().Borc;
                    //AyristirmaMatrahToplam = AyristirmaMatrahToplam + kModel.Tutar;
                    //if (AyristirmaMatrahToplam > YevmiyeBorcToplam)
                    //{
                    //    MmMessage.Messages.Add("Tüm Tif Girişleri Tutar Toplamı Yevmiyenin Alacak Toplamını Geçemez!");
                    //    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
                    //}
                }

            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                if (kModel.YevmiyelerTasinirKontrolTifKayitID <= 0)
                {
                    db.YevmiyelerTasinirKontrolTifKaydis.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyelerTasinirKontrolTifKayitID == kModel.YevmiyelerTasinirKontrolTifKayitID).First();
                    data.TifNo = kModel.TifNo;
                    data.Tutar = kModel.Tutar;
                    data.Aciklama = kModel.Aciklama;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();
                MmMessage.Messages.Add("Tif Kaydı İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }

            return MmMessage.ToJsonResult();
        }
    }
}