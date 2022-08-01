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
        // GET: YevmiyeGiris
        private MusskDBEntities db = new MusskDBEntities();
        public ActionResult Index()
        {
            var fModel = new FmYevmiyeGiris { PageSize = 15 };
            var BirimID = UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler];
            var Yil = UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler];
            fModel.Expand = Yil.HasValue || BirimID.HasValue;
            fModel.BirimID = BirimID;
            fModel.Yil = Yil;
            return Index(fModel);
        }

        [HttpPost]
        public ActionResult Index(FmYevmiyeGiris model, bool export = false)
        {

            var BirimIDs = UserIdentity.Current.BirimYetkileri;
            UserIdentity.Current.SeciliBirimID[RoleNames.Yevmiyeler] = model.BirimID;
            UserIdentity.Current.SeciliYil[RoleNames.Yevmiyeler] = model.Yil;


            var q = (from s in db.Yevmiyelers
                     join b in db.Vw_BirimlerTree on s.BirimID equals b.BirimID
                     select new FrYevmiyeGirisi
                     {
                         YevmiyeID = s.YevmiyeID,
                         YevmiyeTarih = s.YevmiyeTarih,
                         YevmiyeNo = s.YevmiyeNo,
                         BirimID = s.BirimID,
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
            if (model.BirimID.HasValue) q = q.Where(p => p.BirimID == model.BirimID);
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
            ViewBag.Yil = new SelectList(Management.CmbYevmiyeGirisYil(true), "Value", "Caption", model.Yil);
            ViewBag.BirimID = new SelectList(Management.CmbYevmiyeGirisBirim(model.Yil, true), "Value", "Caption", model.BirimID);


            return View(model);
        }

        public ActionResult GetExcelYukle(int Yil)
        {


            var model = new YevmiyeVeriGirisPopupExcelModel();
            model.Yil = Yil;
            var page = Management.RenderPartialView("YevmiyeGiris", "ShowExcelYukle", model);

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
                    var Birimler = db.Birimlers.Where(p => p.IsYevmiyeVeriGirisiYapilabilir).ToList();

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
                                          BirimID = (B != null ? (int?)B.BirimID : null),
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
                            else if (!item.BirimID.HasValue || item.HarcamaBirimAdi.IsNullOrWhiteSpace())
                            {
                                var msg = "";
                                if (!item.BirimID.HasValue) msg = "Harcama Birimi Vergi kimlik numarası sistemdeki hiçbir Birim ile uyuşmuyor";
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
                                BirimID = s.BirimID.Value,
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
                        Management.SistemBilgisiKaydet(msg, "YevmiyeGiris/ExcelYuklePost", BilgiTipi.Hata);

                    }
                }


            }
            return mMessage.ToJsonResult();
        }

    }
}