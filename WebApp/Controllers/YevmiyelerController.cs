using BiskaUtil;
using Database;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            var HesapKodTurYetkis = UserIdentity.Current.YevmiyeHesapKodTurYetkileri;
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
            var IsBankaYetkiVar = HesapKodTurYetkis.Any(a => a == HesapKoduTuru.BankaIslemleriHesapKodlari);
            if (!IsBankaYetkiVar)
            {
                var IsSendikaYetkiVar = HesapKodTurYetkis.Any(a => a == HesapKoduTuru.SendikaIslemleriHesapKodlari);

                var HesapKods = new List<string>();
                if (IsSendikaYetkiVar) HesapKods.AddRange(db.YevmiyelerSendikaBilgileris.Select(s => s.HesapKod).ToList());
                var EslesenHesapkodlaris = db.YevmiyelerHesapKodlaris.Where(p => HesapKodTurYetkis.Contains(p.YevmiyeHesapKodTurID)).Select(s => new { s.YevmiyeHesapKodTurID, s.HesapKod, s.IsGelirKaydindaKullaniclacak }).ToList();
                HesapKods.AddRange(EslesenHesapkodlaris.Select(s => s.HesapKod));
                q = q.Where(p => HesapKods.Contains(p.HesapKod));
            }

            if (model.Yil.HasValue) q = q.Where(p => p.YevmiyeTarih.Year == model.Yil);
            if (model.YevmiyeHarcamaBirimID.HasValue) q = q.Where(p => p.YevmiyeHarcamaBirimID == model.YevmiyeHarcamaBirimID);
            if (model.YevmiyeHesapKodTurID.HasValue)
            {
                var HesapTurKods = db.YevmiyelerHesapKodlaris.Where(p => HesapKodTurYetkis.Contains(p.YevmiyeHesapKodTurID)).Select(s => new { s.YevmiyeHesapKodTurID, s.HesapKod, s.IsGelirKaydindaKullaniclacak }).ToList();

                if (model.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod) && p.Alacak > 0);
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID && p.IsGelirKaydindaKullaniclacak == (model.GelirKaydiOlacak ?? p.IsGelirKaydindaKullaniclacak)).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod) && p.Alacak > 0);
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.SendikaIslemleriHesapKodlari)
                {
                    var HesapKods = db.YevmiyelerSendikaBilgileris.Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
                else if (model.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari)
                {
                    var HesapKods = HesapTurKods.Where(p => p.YevmiyeHesapKodTurID == model.YevmiyeHesapKodTurID).Select(s => s.HesapKod).ToList();
                    q = q.Where(p => HesapKods.Contains(p.HesapKod));
                }
            }
            if (model.YevmiyeNo.HasValue) q = q.Where(p => p.YevmiyeNo == model.YevmiyeNo);
            if (!model.HarcamaBirimKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HarcamaBirimKod == model.HarcamaBirimKod);

            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod.StartsWith(model.HesapKod) || p.HesapAdi.Contains(model.HesapKod));
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
            ViewBag.YevmiyeHarcamaBirimID = new SelectList(Management.CmbYevmiyelerBirim(true), "Value", "Caption", model.YevmiyeHarcamaBirimID);
            ViewBag.YevmiyeHesapKodTurID = new SelectList(Management.CmbYevmiyeHesapKodTurleri(true, HesapKodTurYetkis), "Value", "Caption", model.YevmiyeHesapKodTurID);


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
            if (RoleNames.YevmiyelerExcelYukleme.InRole())
            {
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
            }
            else
            {
                mMessage.Messages.Add("Excel Yüklemek İçin Yetkili Değilsiniz.");
            }

            if (mMessage.Messages.Count == 0)
            {
                var model = DosyaEki.ToYevmiyeIterateRows(Yil);
                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }
                else
                {
                    //var DBMaxYevmiyeNo = db.Yevmiyelers.Where(p => p.IslemTarihi.Year == Yil).Max(m => (int?)m.YevmiyeNo);
                    //if (DBMaxYevmiyeNo.HasValue)
                    //{
                    //    var ExcelMaxYevmiyeNo = model.Data.Select(s => s.YevmiyeNo).Min();
                    //    if (ExcelMaxYevmiyeNo <= DBMaxYevmiyeNo)
                    //    {
                    //        mMessage.Messages.Add("Yükleyeceğiniz yevmiyelerin yevmiye numarası daha önce yüklenen en büyük yevmiye numarasından ("+ DBMaxYevmiyeNo + ") büyük olmalı.");
                    //    }
                    //}
                }


                if (mMessage.Messages.Count == 0)
                {
                    var Birimler = db.YevmiyelerHarcamaBirimleris.ToList();
                    var Sendikalar = db.YevmiyelerSendikaBilgileris.ToList();
                    var EmekliKesenekHesapKods = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari).ToList();

                    try
                    {
                        #region BildirimData 
                        model.Data = (from s in model.Data
                                      join Br in Birimler on s.VergiKimlikNo equals Br.VergiKimlikNo into defB
                                      from Br in defB.DefaultIfEmpty()
                                      join Sn in Sendikalar on s.YevmiyeSendikaBilgiID equals Sn.YevmiyeSendikaBilgiID into defSn
                                      from Sn in defSn.DefaultIfEmpty()
                                      select new ExcelDataImportYevmiyeRow
                                      {
                                          SayfaNo = s.SayfaNo,
                                          SatirNo = s.SatirNo,
                                          YevmiyeTarih = s.YevmiyeTarih,
                                          YevmiyeNo = s.YevmiyeNo,
                                          VergiKimlikNo = s.VergiKimlikNo,
                                          YevmiyeHarcamaBirimID = (Br != null ? (int?)Br.YevmiyeHarcamaBirimID : null),
                                          YevmiyeSendikaBilgiID = (Br != null ? (int?)Sn.YevmiyeSendikaBilgiID : null),
                                          HarcamaBirimAdi = s.HarcamaBirimAdi,
                                          HarcamaBirimKod = s.HarcamaBirimKod,
                                          EKYevmiyeHarcamaBirimID = EmekliKesenekHesapKods.Any(a => a.HesapKod == s.HesapKod) ? s.YevmiyeHarcamaBirimID : null,
                                          BESYevmiyeHesapKodID = EmekliKesenekHesapKods.Any(a => a.HesapKod == s.HesapKod) ? s.YevmiyeHarcamaBirimID : null,
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
                                BESYevmiyeHesapKodID = s.BESYevmiyeHesapKodID,
                                EKYevmiyeHarcamaBirimID = s.EKYevmiyeHarcamaBirimID,
                                IslemYapanID = s.IslemYapanID,
                                IslemYapanIP = s.IslemYapanIP,
                                IslemTarihi = s.IslemTarihi,
                            }).ToList();
                            db.Yevmiyelers.AddRange(addYevmiyes);
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

        public ActionResult MaasExcelYukle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MaasExcelYuklePost(HttpPostedFileBase DosyaEki)
        {

            var mMessage = new MmMessage();
            mMessage.IsSuccess = false;
            mMessage.Title = "Maaş Excel Dosyasını Dönüştürme işlemi";
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
                var model = DosyaEki.ToMaasDonusturIterateRows();
                if (model.Data.Count == 0)
                {
                    mMessage.Messages.Add(DosyaEki.FileName + "  isimli excel dosyasında hiçbir veriye rastlanmadı!");
                }



                if (mMessage.Messages.Count == 0)
                {


                    try
                    {

                        model.Data = model.Data.Where(p => p.ColumnValues.Where(p2 => p2.Trim() != "").Count() >= 9).ToList();
                        var IlkSiraNoInx = model.Data.FindIndex(p => p.ColumnValues[0].ToInt() == 1);
                        model.ColumnValues = model.Data.Where((p, inx) => inx < IlkSiraNoInx).SelectMany(s => s.ColumnValues).ToList();
                        var AllRows = new List<ExcelDataImportYevmiyeMaasRow>();
                        var TempRows = new List<ExcelDataImportYevmiyeMaasRow>();
                        int TempSiraNo = 1;
                        var SiraNo = 0;
                        foreach (var item in model.Data.Where((p, inx) => inx >= IlkSiraNoInx && p.ColumnValues[1].Trim()!="" && p.ColumnValues[2].Trim()!=""))
                        {
                            var _SiraNo = item.ColumnValues[0].ToInt();
                            if (_SiraNo.HasValue) SiraNo = _SiraNo.Value; 
                            if (SiraNo < TempSiraNo + 1)
                            {
                                TempRows.Add(item);
                            }
                            else
                            {
                                AllRows.Add(new ExcelDataImportYevmiyeMaasRow { SatirNo = TempSiraNo - 1, ColumnValues = TempRows.SelectMany(s => s.ColumnValues).ToList() });
                                
                                TempRows.Clear();
                                TempRows.Add(item);
                                TempSiraNo++;
                            }
                        }
                        if(TempRows.Any()) AllRows.Add(new ExcelDataImportYevmiyeMaasRow { SatirNo = TempSiraNo - 1, ColumnValues = TempRows.SelectMany(s => s.ColumnValues).ToList() });
                        model.Data.Clear();
                        model.Data.AddRange(AllRows);

                        int ColumnNum = 0;
                        var dataTable = new System.Data.DataTable("MaasTable");
                        foreach (var item in model.ColumnValues.Where(p => p.Trim() != ""))
                        {
                            var ColumnName = string.Join("_", item.Split(' ').ToList().Where(p => p != "").Select(s => s).ToList());

                            ColumnNum++;
                            ColumnName += "_" + ColumnNum;
                            dataTable.Columns.Add(ColumnName);
                        }
                        foreach (var itemRow in model.Data)
                        {
                            try
                            {


                                var dr = dataTable.NewRow();
                                int Ic = 0;
                                foreach (var itemC in itemRow.ColumnValues.Where(p => p.Trim() != ""))
                                {
                                    dr[Ic] = itemC;
                                    Ic++;
                                }
                                dataTable.Rows.Add(dr);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        var gv = new GridView();
                        gv.DataSource = dataTable;
                        gv.DataBind();
                        Response.ContentType = "application/ms-excel";
                        Response.ContentEncoding = System.Text.Encoding.UTF8;
                        Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                        StringWriter sw = new StringWriter();
                        HtmlTextWriter htw = new HtmlTextWriter(sw);
                        gv.RenderControl(htw);

                        var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
                        var path = "/TempDocumentFolder/" + unqCode.ReplaceSpecialCharacter() + "_MaasExcel.xls";
                        var SavePath = System.Web.HttpContext.Current.Server.MapPath("~" + path);
                        string renderedGridView = sw.ToString();
                        System.IO.File.WriteAllText(SavePath, renderedGridView);

                        mMessage.Messages.Add("<a style='color:red;' href='" + path + "' target='_blank;'><img src='/Content/img/Excel-Icon.png' width='18' height='17'> Dönüştürülen Dosyayı indir.</a>");

                    }
                    catch (Exception ex)
                    {


                    }
                }


            }
            return mMessage.ToJsonResult();
        }
        public ActionResult GetDetail(int id)
        {
            var YetkiYevmiyeHesapKodTurID = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).SelectMany(s => s.KullaniciYevmiyeHesapKodTurYetkileris).Select(s => s.YevmiyeHesapKodTurID).ToList();
            var mdl = (from s in db.Yevmiyelers.Where(p => p.YevmiyeID == id)
                       join BHk in db.YevmiyelerHesapKodlaris on s.BESYevmiyeHesapKodID equals BHk.YevmiyeHesapKodID into defBkh
                       from BHk in defBkh.DefaultIfEmpty()
                       join Ekh in db.YevmiyelerHarcamaBirimleris on s.EKYevmiyeHarcamaBirimID equals Ekh.YevmiyeHarcamaBirimID into defEkh
                       from Ekh in defEkh.DefaultIfEmpty()
                       select new YevmiyeDetayModel
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
                           Yevmiyeler1003AGelirKayit = s.Yevmiyeler1003AGelirKayit,
                           YevmiyelerKdvTevkifatKayitlaris = s.YevmiyelerKdvTevkifatKayitlaris,
                           YevmiyelerTasinirKontrolTifKaydis = s.YevmiyelerTasinirKontrolTifKaydis,
                           ProjeBankaHesapNoID = s.ProjeBankaHesapNoID,
                           YevmiyelerProjeBankaHesapNumaralari = s.YevmiyelerProjeBankaHesapNumaralari,
                           YevmiyeSendikaBilgiID = s.YevmiyeSendikaBilgiID,
                           YevmiyelerSendikaBilgileri = s.YevmiyelerSendikaBilgileri,
                           BESYevmiyeHesapKodID = s.BESYevmiyeHesapKodID,
                           BesHesapKod = BHk.HesapKod,
                           BesHesapKodAdi = BHk.HesapAdi,
                           Is1003BYevmiyeParcalamaOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.SSKPrimHesapKodlari1003B) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.SSKPrimHesapKodlari1003B && a.HesapKod == s.HesapKod && s.Alacak > 0),
                           Is1003AGelirKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.VergiTevkifatHesapKodlari1003A) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && a.HesapKod == s.HesapKod),
                           Is1003A10_24GelirKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.VergiTevkifatHesapKodlari1003A) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && a.IsGelirKaydindaKullaniclacak == true && a.HesapKod == s.HesapKod),
                           IsKdvVergiKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.KDVTevkifatHesapKodlari) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.KDVTevkifatHesapKodlari && a.HesapKod == s.HesapKod && s.Alacak > 0),
                           IsTasinirKontrolKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.TasinirKontrolHesapKodlari) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.TasinirKontrolHesapKodlari && a.HesapKod == s.HesapKod),
                           IsEmekliKesenekKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.EmekliKesintiHesapKodlari) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.EmekliKesintiHesapKodlari && a.HesapKod == s.HesapKod),
                           IsSendikaKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.SendikaIslemleriHesapKodlari) && db.YevmiyelerSendikaBilgileris.Any(a => a.HesapKod == s.HesapKod),
                           IsBireyselEmeklikKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.BireyselEmeklilikHesapKodlari) && db.YevmiyelerHesapKodlaris.Any(a => a.YevmiyeHesapKodTurID == HesapKoduTuru.BireyselEmeklilikHesapKodlari && a.HesapKod == s.HesapKod),
                           IsBankaIslemleriKaydiOlacak = YetkiYevmiyeHesapKodTurID.Any(a => a == HesapKoduTuru.BankaIslemleriHesapKodlari),
                           EKYevmiyeHarcamaBirimID = s.EKYevmiyeHarcamaBirimID,
                           EKHarcamaBirimAdi = Ekh.BirimAdi,

                       }).First();
            if (mdl.Yevmiyeler1003AGelirKayit.Count == 0) mdl.Yevmiyeler1003AGelirKayit.Add(new Yevmiyeler1003AGelirKayit() { YevmiyelerHesapKodlari = db.YevmiyelerHesapKodlaris.Where(p => p.HesapKod == mdl.HesapKod).FirstOrDefault() });


            var GelirVergisiHesapKods = new List<string> { "360.01.01.01", "360.01.01.02" };
            var DamgaVergisiHesapKods = new List<string> { "360.03.01" };

            mdl.YevmiyeNoToplamGv = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == mdl.YevmiyeTarih.Year && p.YevmiyeNo == mdl.YevmiyeNo && GelirVergisiHesapKods.Contains(p.HesapKod)).Sum(s => (decimal?)s.Alacak);
            mdl.YevmiyeNoToplamDv = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == mdl.YevmiyeTarih.Year && p.YevmiyeNo == mdl.YevmiyeNo && DamgaVergisiHesapKods.Contains(p.HesapKod)).Sum(s => (decimal?)s.Alacak);

            var Yevmiyeler1003AGelirK = mdl.Yevmiyeler1003AGelirKayit.First();
            mdl.SHesapKodlari1003A = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.VergiTevkifatHesapKodlari1003A), "Value", "Caption", Yevmiyeler1003AGelirK.YeniYevmiyeHesapKodID);
            mdl.SBesHesapKod = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.BireyselEmeklilikHesapKodlari), "Value", "Caption", mdl.BESYevmiyeHesapKodID);
            mdl.EKHarcamaBirim = new SelectList(Management.CmbYevmiyelerBirim(), "Value", "Caption", mdl.EKYevmiyeHarcamaBirimID);

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
        public ActionResult YevmiyeAyristirSil(int id)
        {
            var kayit = db.Yevmiyeler1003BAyristirmalari.Where(p => p.Yevmiye1003BAyristirmaID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.YevmiyelerHarcamaBirimleri.BirimAdi + "' İsimli Harcama Birimine Ait Yevmiye Ayrıştırma Kaydı Silindi!";
                    db.Yevmiyeler1003BAyristirmalari.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.YevmiyelerHarcamaBirimleri.BirimAdi + "' İsimli Harcama Birimine Ait Yevmiye Ayrıştırma kaydıSilinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeAyristirSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Harcama Birimine Ait Yevmiye Ayrıştırma Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GelirKayit(Yevmiyeler1003AGelirKayit kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003A Gelir Kaydı";
            MmMessage.MessageType = Msgtype.Warning;

            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == kModel.YevmiyeID).First();
            var YevmiyeHesap = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodTurID == HesapKoduTuru.VergiTevkifatHesapKodlari1003A && p.HesapKod == Yevmiye.HesapKod).FirstOrDefault();

            if (YevmiyeHesap == null)
            {
                MmMessage.Messages.Add("Bu yevmiye için Gelir Kaydı işlemi yapılamaz");
            }
            else
            {
                if (!kModel.IsHesaplamayaGirecek.HasValue)
                {
                    MmMessage.Messages.Add("Bu yevmiye kaydının hesaplamaya girip girmeyeceğini seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "IsHesaplamayaGirecek" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "IsHesaplamayaGirecek" });
                if (YevmiyeHesap.IsGelirKaydindaKullaniclacak == true)
                {
                    if (!kModel.VergiKimlikNo.IsNullOrWhiteSpace() || !kModel.AdSoyad.IsNullOrWhiteSpace() || !kModel.Adres.IsNullOrWhiteSpace() || kModel.Matrah.HasValue || !kModel.BelgeninMahiyeti.IsNullOrWhiteSpace() || !kModel.FaturaTarihi.HasValue || !kModel.FaturaNo.IsNullOrWhiteSpace())
                    {
                        if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Vergi Kimlik Numarası seçiniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
                        if (kModel.AdSoyad.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Ad Soyad Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });
                        if (kModel.Adres.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Adres Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Adres" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Adres" });
                        if (!(kModel.Matrah > 0))
                        {
                            MmMessage.Messages.Add("Matrah bilgisi 0'Dan büyük olmalıdır.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Matrah" });
                        if (kModel.BelgeninMahiyeti.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Belge Mahiyeti Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BelgeninMahiyeti" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BelgeninMahiyeti" });
                        if (!kModel.FaturaTarihi.HasValue)
                        {
                            MmMessage.Messages.Add("Fatura Tarihi Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaTarihi" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaTarihi" });
                        if (kModel.FaturaNo.IsNullOrWhiteSpace())
                        {
                            MmMessage.Messages.Add("Fatura Numarası Giriniz.");
                            MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaNo" });
                        }
                        else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaNo" });

                    }
                }
                if (!MmMessage.Messages.Any())
                {
                    if (kModel.YeniYevmiyeHesapKodID.HasValue)
                    {
                        var YeniHesapKodu = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == kModel.YeniYevmiyeHesapKodID).First();
                        kModel.VergiKodu = YeniHesapKodu.VergiKodu;
                    }
                    else kModel.VergiKodu = null;

                    kModel.IslemTarihi = DateTime.Now;
                    kModel.IslemYapanID = UserIdentity.Current.Id;
                    kModel.IslemYapanIP = UserIdentity.Ip;
                    if (!Yevmiye.Yevmiyeler1003AGelirKayit.Any())
                    {
                        db.Yevmiyeler1003AGelirKayit.Add(kModel);
                    }
                    else
                    {
                        var data = Yevmiye.Yevmiyeler1003AGelirKayit.First();
                        data.YeniYevmiyeHesapKodID = kModel.YeniYevmiyeHesapKodID;
                        data.VergiKodu = kModel.VergiKodu;
                        data.IsHesaplamayaGirecek = kModel.IsHesaplamayaGirecek;
                        data.VergiKimlikNo = kModel.VergiKimlikNo;
                        data.AdSoyad = kModel.AdSoyad;
                        data.Adres = kModel.Adres;
                        data.Matrah = kModel.Matrah;
                        data.BelgeninMahiyeti = kModel.BelgeninMahiyeti;
                        data.FaturaTarihi = kModel.FaturaTarihi;
                        data.FaturaNo = kModel.FaturaNo;
                        data.IslemTarihi = kModel.IslemTarihi;
                        data.IslemYapanID = kModel.IslemYapanID;
                        data.IslemYapanIP = kModel.IslemYapanIP;
                    }
                    if (!kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                    {
                        var YevmiyeVKN = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo == kModel.VergiKimlikNo).FirstOrDefault();
                        if (YevmiyeVKN == null)
                        {
                            db.YevmiyelerVergiKimlikNumaralaris.Add(new YevmiyelerVergiKimlikNumaralari
                            {
                                VergiKimlikNo = kModel.VergiKimlikNo,
                                AdSoyad = kModel.AdSoyad,
                                Adres = kModel.Adres
                            });
                        }
                    }
                    db.SaveChanges();

                    MmMessage.Messages.Add("Gelir Kaydı İşlemi Yapıldı.");
                    MmMessage.IsSuccess = true;
                    MmMessage.MessageType = Msgtype.Success;

                }
            }

            return MmMessage.ToJsonResult();
        }
        public ActionResult YevmiyeKdvTevkifatKayit(int YevmiyeID, int? YevmiyeKdvTevkifatKayitID = null)
        {
            var model = new YevmiyelerKdvTevkifatKayitlari();
            if (YevmiyeKdvTevkifatKayitID > 0)
            {
                model = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeID == YevmiyeID && p.YevmiyeKdvTevkifatKayitID == YevmiyeKdvTevkifatKayitID).First();
            }
            ViewBag.YeniYevmiyeHesapKodID = new SelectList(Management.CmbYevmiyelerHesapKodlari(HesapKoduTuru.KDVTevkifatHesapKodlari, true), "Value", "Caption", model.YeniYevmiyeHesapKodID);
            ViewBag.FaturaYil = new SelectList(Management.CmbYevmiylerYil(true), "Value", "Caption", model.FaturaYil);
            ViewBag.FaturaAyID = new SelectList(Management.CmbAylar(true), "Value", "Caption", model.FaturaAyID);
            ViewBag.YevmiyeKdvKodID = new SelectList(Management.CmbYevmiyeKdvKodlari(true), "Value", "Caption", model.YevmiyeKdvKodID);
            ViewBag.KdvOrani = new SelectList(Management.CmbKdvOranlari(true), "Value", "Caption", model.KdvOrani);

            return View(model);
        }

        [HttpPost]
        public ActionResult YevmiyeKdvTevkifatKayit(YevmiyelerKdvTevkifatKayitlari kModel)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "1003B Kdv Tevkifat Kayıt İşlemi";
            MmMessage.MessageType = Msgtype.Warning;


            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik Numarası Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.AdSoyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Soyad Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });
            if (kModel.YevmiyeKdvKodID <= 0)
            {
                MmMessage.Messages.Add("Kdv Kodu Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YevmiyeKdvKodID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YevmiyeKdvKodID" });
            if (kModel.FaturaYil <= 0)
            {
                MmMessage.Messages.Add("Fatura Yılı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaYil" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaYil" });
            if (kModel.FaturaAyID <= 0)
            {
                MmMessage.Messages.Add("Fatura Ayı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaAyID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaturaAyID" });


            if (kModel.Matrah <= 0)
            {
                MmMessage.Messages.Add("Matrah Bilgisi Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Matrah" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Matrah" });
            if (kModel.KdvOrani <= 0)
            {
                MmMessage.Messages.Add("Kdv oranı Seçiniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KdvOrani" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KdvOrani" });
            if (!MmMessage.Messages.Any())
            {
                var YevmiyeAyniYilGirisi = db.YevmiyelerKdvTevkifatKayitlaris.Any(a => a.YevmiyeID == kModel.YevmiyeID && a.VergiKimlikNo == kModel.VergiKimlikNo && a.FaturaYil == kModel.FaturaYil && a.FaturaAyID == kModel.FaturaAyID && a.YevmiyeKdvTevkifatKayitID != kModel.YevmiyeKdvTevkifatKayitID);
                if (false && YevmiyeAyniYilGirisi)
                {
                    MmMessage.Messages.Add("Seçilen Vergi Kimlik Numarası için Yıl ve Ay İçin Daha Önceden Ayrıştırma İşlemi Yapıldı!");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaYil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaturaAyID" });
                }
                else
                {
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "VergiKimlikNo" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "FaturaYil" });
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Nothing, PropertyName = "FaturaAyID" });
                    var Yevmiye = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First();
                    var KayitliTevkifatToplam = db.YevmiyelerKdvTevkifatKayitlaris.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.YevmiyeKdvTevkifatKayitID != kModel.YevmiyeKdvTevkifatKayitID).Sum(s => (decimal?)s.TevkifatTutari);
                    var YevmiyeAlacakToplam = Yevmiye.Alacak;

                    YevmiyelerHesapKodlari YevmiyeHesapKodlari;
                    if (kModel.YeniYevmiyeHesapKodID.HasValue)
                    {
                        YevmiyeHesapKodlari = db.YevmiyelerHesapKodlaris.Where(p => p.YevmiyeHesapKodID == kModel.YeniYevmiyeHesapKodID).First();
                    }
                    else YevmiyeHesapKodlari = db.YevmiyelerHesapKodlaris.Where(p => p.HesapKod == Yevmiye.HesapKod).First();

                    kModel.KdvTutari = (kModel.Matrah * kModel.KdvOrani) / 100;
                    kModel.TevkifatTutari = kModel.KdvTutari * ((kModel.KdvOrani / (((decimal)YevmiyeHesapKodlari.TevkifatOranBolunen.Value / (decimal)YevmiyeHesapKodlari.TevkifatOranBolen.Value))) / 100);

                    KayitliTevkifatToplam = KayitliTevkifatToplam + kModel.TevkifatTutari;

                    if (KayitliTevkifatToplam > YevmiyeAlacakToplam)
                    {
                        MmMessage.Messages.Add("Tüm Kdv Tevkifat Kayıtları Tevkifat Tutarı Toplamı Yevmiyenin Alacak Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "TevkifatTutari" });
                    }


                }
            }
            if (!MmMessage.Messages.Any())
            {

                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;

                var KdvKodlus = db.YevmiyelerKdvKodlaris.Where(p => p.YevmiyeKdvKodID == kModel.YevmiyeKdvKodID).First();
                kModel.KdvKodu = KdvKodlus.KdvKodu;
                kModel.KdvKodOrani = KdvKodlus.KdvOrani;

                if (kModel.YevmiyeKdvTevkifatKayitID <= 0)
                {
                    db.YevmiyelerKdvTevkifatKayitlaris.Add(kModel);
                }
                else
                {
                    var data = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeKdvTevkifatKayitID == kModel.YevmiyeKdvTevkifatKayitID).First();
                    data.YeniYevmiyeHesapKodID = kModel.YeniYevmiyeHesapKodID;
                    data.VergiKimlikNo = kModel.VergiKimlikNo;
                    data.AdSoyad = kModel.AdSoyad;
                    data.Matrah = kModel.Matrah;
                    data.KdvOrani = kModel.KdvOrani;
                    data.KdvTutari = kModel.KdvTutari;
                    data.TevkifatTutari = kModel.TevkifatTutari;
                    data.YevmiyeKdvKodID = kModel.YevmiyeKdvKodID;
                    data.KdvKodu = kModel.KdvKodu;
                    data.KdvKodOrani = kModel.KdvKodOrani;
                    data.FaturaYil = kModel.FaturaYil;
                    data.FaturaAyID = kModel.FaturaAyID;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                }
                if (!kModel.VergiKimlikNo.IsNullOrWhiteSpace())
                {
                    var YevmiyeVKN = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo == kModel.VergiKimlikNo).FirstOrDefault();
                    if (YevmiyeVKN == null)
                    {
                        db.YevmiyelerVergiKimlikNumaralaris.Add(new YevmiyelerVergiKimlikNumaralari
                        {
                            VergiKimlikNo = kModel.VergiKimlikNo,
                            AdSoyad = kModel.AdSoyad
                        });
                    }

                }
                db.SaveChanges();
                MmMessage.Messages.Add("Tevkifat Kayıt İşlemi Yapıldı.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }

        public ActionResult YevmiyeKdvTevkifatKayitSil(int id)
        {
            var kayit = db.YevmiyelerKdvTevkifatKayitlaris.Where(p => p.YevmiyeKdvTevkifatKayitID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.VergiKimlikNo + "' Vergi Kimlik Numarasına Ait Kdv Tefkifatı Kaydı Silindi!";
                    db.YevmiyelerKdvTevkifatKayitlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.VergiKimlikNo + "' Vergi Kimlik Numarasına Ait Kdv Tefkifatı Kaydı Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeKdvTevkifatKayitSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kdv Tefkifatı Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult YevmiyeEKHarcamaBirimKayit(int YevmiyeID, int? EKYevmiyeHarcamaBirimID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Emekli Kesenek Harcama Birimi Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            if (!MmMessage.Messages.Any())
            {
                Yevmiye.EKYevmiyeHarcamaBirimID = EKYevmiyeHarcamaBirimID;
                db.SaveChanges();
                MmMessage.Messages.Add("Harcama Birimi Güncellendi.");
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
            MmMessage.Title = "Taşınır Kontrol Kaydı";
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

                    var AyristirmaMatrahToplam = db.YevmiyelerTasinirKontrolTifKaydis.Where(a => a.YevmiyeID == kModel.YevmiyeID && a.YevmiyelerTasinirKontrolTifKayitID != kModel.YevmiyelerTasinirKontrolTifKayitID).Sum(s => (decimal?)s.Tutar) ?? 0;
                    var YevmiyeBorcToplam = db.Yevmiyelers.Where(a => a.YevmiyeID == kModel.YevmiyeID).First().Borc;
                    AyristirmaMatrahToplam = AyristirmaMatrahToplam + kModel.Tutar;
                    if (AyristirmaMatrahToplam > YevmiyeBorcToplam)
                    {
                        MmMessage.Messages.Add("Tüm Tif Girişleri Tutar Toplamı Yevmiyenin Borç Toplamını Geçemez!");
                        MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tutar" });
                    }
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
        public ActionResult YevmiyeTasinirkontrolTifKayitSil(int id)
        {
            var kayit = db.YevmiyelerTasinirKontrolTifKaydis.Where(p => p.YevmiyelerTasinirKontrolTifKayitID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.TifNo + "' Tif Kaydı Silindi!";
                    db.YevmiyelerTasinirKontrolTifKaydis.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.TifNo + "' Tif Kaydı Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    Management.SistemBilgisiKaydet(message, "YevmiyeTasinirkontrolTifKayitSil/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Tif Kaydı Sistemde Bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult YevmiyeBankaHesapNoKayit(int YevmiyeID, int? ProjeBankaHesapNoID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Hesap Numarası Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            var TumYevmiyeler = db.Yevmiyelers.Where(p => p.YevmiyeTarih.Year == Yevmiye.YevmiyeTarih.Year && p.YevmiyeNo == Yevmiye.YevmiyeNo).ToList();
            foreach (var item in TumYevmiyeler)
            {
                item.ProjeBankaHesapNoID = ProjeBankaHesapNoID;
            }
            db.SaveChanges();
            MmMessage.Messages.Add("Yevmiye Banka Hesap Numarası Güncellendi.");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;

            return MmMessage.ToJsonResult();
        }

        [HttpPost]
        public ActionResult YevmiyeSendikaBilgisiKayit(int YevmiyeID, int? YevmiyeSendikaBilgiID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Sendika Bilgisi Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();
            Yevmiye.YevmiyeSendikaBilgiID = YevmiyeSendikaBilgiID;

            db.SaveChanges();
            MmMessage.Messages.Add("Yevmiye Sendika Bilgisi Güncellendi.");
            MmMessage.IsSuccess = true;
            MmMessage.MessageType = Msgtype.Success;

            return MmMessage.ToJsonResult();
        }
        [HttpPost]
        public ActionResult BESHesapKodKayit(int YevmiyeID, int? BESYevmiyeHesapKodID)
        {
            var MmMessage = new MmMessage();
            MmMessage.IsSuccess = false;
            MmMessage.Title = "Bireysel Emeklilik Hesap Kodu Değişikliği";
            MmMessage.MessageType = Msgtype.Warning;
            var Yevmiye = db.Yevmiyelers.Where(p => p.YevmiyeID == YevmiyeID).First();

            if (!MmMessage.Messages.Any())
            {
                Yevmiye.BESYevmiyeHesapKodID = BESYevmiyeHesapKodID;
                db.SaveChanges();
                MmMessage.Messages.Add("Hesap Kodu Güncellendi.");
                MmMessage.IsSuccess = true;
                MmMessage.MessageType = Msgtype.Success;

            }
            return MmMessage.ToJsonResult();
        }


        public ActionResult GetVergiKimlikNo(string term)
        {
            var VergiKimlikNos = db.YevmiyelerVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo.Contains(term) || p.AdSoyad.Contains(term)).Select(s => new
            {
                id = s.VergiKimlikNo,
                text = s.VergiKimlikNo,
                s.AdSoyad,
                s.VergiKimlikNo,
                s.Adres
            }).OrderBy(o => o.AdSoyad).Take(25).ToList();

            return VergiKimlikNos.ToJsonResult();


        }
        public ActionResult GetBankaHesapNumaralari(string term)
        {
            var HesapNumaralari = db.YevmiyelerProjeBankaHesapNumaralaris.Where(p => p.HesapNo == term || p.HesapAdi.Contains(term) || p.ProjeNo.Contains(term) || p.ProjeAdi.Contains(term)).Select(s => new
            {
                id = s.ProjeBankaHesapNoID,
                text = s.HesapNo + " " + s.HesapAdi + " (" + s.ProjeNo + "- " + s.ProjeAdi + ")",
                s.HesapNo,
                s.HesapAdi,
                s.ProjeNo,
                s.ProjeAdi,
            }).OrderBy(o => o.HesapAdi).Take(25).ToList();

            return HesapNumaralari.ToJsonResult();


        }
        public ActionResult GetSendikaBilgileri(string term)
        {
            var Sendikalar = db.YevmiyelerSendikaBilgileris.Where(p => p.HesapKod.StartsWith(term) || p.VergiKimlikNo.StartsWith(term) || p.AdSoyad.Contains(term) || p.KisaAdi.Contains(term)).Select(s => new
            {
                id = s.YevmiyeSendikaBilgiID,
                text = s.HesapKod + " " + s.AdSoyad,
                s.HesapKod,
                s.VergiKimlikNo,
                s.AdSoyad,
                s.IBanNo,
                s.KisaAdi,
                s.Aciklama
            }).OrderBy(o => o.AdSoyad).Take(25).ToList();

            return Sendikalar.ToJsonResult();


        }
    }
}