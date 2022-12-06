using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using BiskaUtil;
using Database;
using Microsoft.Office.Interop.Excel;

namespace WebApp.Models
{




    public class ExcelDataImportRow : VASurecleriBirimVerileri
    {
        public int SatirNo { get; set; }
        public int SayfaNo { get; set; }
        public int? UstBirimID { get; set; }
        public int? BirimID { get; set; }
        public int? BelgeMahiyetTipID { get; set; }
        public string BelgeMahiyetTipKodu { get; set; }
        public int? BelgeTurID { get; set; }
        public string BelgeTurKodu { get; set; }
        public int? VeriGirisTipID { get; set; }
        public int? VASurecleriBirimID { get; set; }
        public int? PrimOdemeGun { get; set; }
        public decimal? HakEdilenUcret { get; set; }
        public decimal? PrimIkramiyeIstihkak { get; set; }
        public string MeslekTurKodu { get; set; }
        public int? MeslekTurID { get; set; }
        public int? IstirahatSurecindeCalismamistir { get; set; }
        public int? GvDenMuafmi { get; set; }

        public bool IsAyAsiriHesaplama { get; set; }
        public int? AyBaslangicGun { get; set; }
        public int? GelecekAyBitisGun { get; set; }
        public int AyDakiGunSayisi { get; set; }
        public List<int> HataliHucreler { get; set; }
        public string HataAciklamasi { get; set; }
        public VASurecleriBirimVerileri BirOncekiDurumVerisi { get; set; }
        public ExcelDataImportRow()
        {
            HataliHucreler = new List<int>();
        }
    }
    public class ExcelImportYevmiyeMaasModel
    {
        public List<string> ColumnValues { get; set; }
        public string DosyaAdi { get; set; }
        public string DosyaYolu { get; set; }
        public List<ExcelDataImportYevmiyeMaasRow> Data { get; set; }
        public ExcelImportYevmiyeMaasModel()
        {
            ColumnValues = new List<string>();
            Data = new List<ExcelDataImportYevmiyeMaasRow>();
        }
    }
    public class ExcelDataImportYevmiyeMaasRow
    {

        public List<string> ColumnValues { get; set; }
        public int SatirNo { get; set; }
        public int SayfaNo { get; set; }
        public List<int> HataliHucreler { get; set; }
        public string HataAciklamasi { get; set; }
        public ExcelDataImportYevmiyeMaasRow()
        {
            ColumnValues = new List<string>();
            HataliHucreler = new List<int>();
        }
    }
    public class ExcelImportYevmiyeDataModel
    {
        public string DosyaAdi { get; set; }
        public string DosyaYolu { get; set; }
        public List<ExcelDataImportYevmiyeRow> Data { get; set; }
        public ExcelImportYevmiyeDataModel()
        {
            Data = new List<ExcelDataImportYevmiyeRow>();
        }
    }
    public class ExcelDataImportYevmiyeRow : Yevmiyeler
    {

        public System.DateTime? YevmiyeTarih { get; set; }
        public int? YevmiyeNo { get; set; }
        public int? YevmiyeHarcamaBirimID { get; set; }
        public string HarcamaBirimAdi { get; set; }
        public decimal? Borc { get; set; }
        public decimal? Alacak { get; set; }

        public int SatirNo { get; set; }
        public int SayfaNo { get; set; }
        public List<int> HataliHucreler { get; set; }
        public string HataAciklamasi { get; set; }
        public ExcelDataImportYevmiyeRow()
        {
            HataliHucreler = new List<int>();
        }
    }
    public class ExcelImportDataModel
    {
        public string DosyaAdi { get; set; }
        public string DosyaYolu { get; set; }
        public List<ExcelDataImportRow> Data { get; set; }
        public ExcelImportDataModel()
        {
            Data = new List<ExcelDataImportRow>();
        }
    }
    public static class ExcelImportModel
    {
        public static string ToConnectionStringXls(this string Path)
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
        }
        public static string ToConnectionStringXlsx(this string Path)
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
        }



        public static ExcelImportDataModel ToIsciIterateRows(this HttpPostedFileBase item, int VASurecID)
        {

            var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
            string extension = Path.GetExtension(item.FileName);

            string fileName = unqCode.ReplaceSpecialCharacter() + "_" + VASurecID + item.FileName.Replace(extension, "").ReplaceSpecialCharacter() + extension;
            var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/TempDocumentFolder"), fileName);

            item.SaveAs(path);
            FileInfo file = new FileInfo(path);

            var Model = new ExcelImportDataModel();
            Model.DosyaAdi = file.Name;
            Model.DosyaYolu = "/TempDocumentFolder/" + fileName;
            DataSet ds = new DataSet();
            string fileExtension = System.IO.Path.GetExtension(item.FileName);
            string ConnectionString = string.Empty;
            if (fileExtension == ".xls") ConnectionString = path.ToConnectionStringXls();
            else if (fileExtension == ".xlsx") ConnectionString = path.ToConnectionStringXlsx();
            OleDbConnection excelConnection = new OleDbConnection(ConnectionString);
            excelConnection.Open();
            var dt = new System.Data.DataTable();

            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (dt == null)
            {
                return null;
            }
            String[] excelSheets = new String[dt.Rows.Count];
            int t = 0;
            //excel data saves in temp file here.
            foreach (DataRow row in dt.Rows)
            {
                excelSheets[t] = row["TABLE_NAME"].ToString();
                t++;
            }
            OleDbConnection excelConnection1 = new OleDbConnection(ConnectionString);

            for (int iSht = 0; iSht < excelSheets.Length; iSht++)
            {
                string query = string.Format("Select * from [{0}]", excelSheets[iSht]);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    if (ds.Tables.Count > 0) ds.Tables[0].Rows.Clear();
                    dataAdapter.Fill(ds);
                }
                var tables = ds.Tables[0];
                for (int i = 0; i < tables.Rows.Count; i++)
                {

                    var _BelgeMahiyetTipKodu = tables.Rows[i][0].ToStrObjEmptString().Trim();
                    var _BelgeTurKodu = tables.Rows[i][1].ToStrObjEmptString().Trim();
                    var _DEsasKanunNo = tables.Rows[i][2].ToStrObjEmptString().Trim();
                    var _YeniUniteKodu = tables.Rows[i][3].ToStrObjEmptString().Trim();
                    var _EskiUniteKodu = tables.Rows[i][4].ToStrObjEmptString().Trim();
                    var _IsyeriSiraNumarasi = tables.Rows[i][5].ToStrObjEmptString().Trim();
                    var _IlKodu = tables.Rows[i][6].ToStrObjEmptString().Trim();
                    var _AltIsverenNumarasi = tables.Rows[i][7].ToStrObjEmptString().Trim();
                    var _VeriGirisTipID = tables.Rows[i][8].ToIntObj();
                    var _TcKimlikNo = tables.Rows[i][10].ToStrObjEmptString().Trim();
                    var _Ad = tables.Rows[i][11].ToStrObjEmptString().Trim();
                    var _Soyad = tables.Rows[i][12].ToStrObjEmptString().Trim();
                    var _PrimOdemeGun = tables.Rows[i][13].ToIntObj();
                    var _UzaktanCalismaGun = tables.Rows[i][14].ToIntObj();
                    var _HakEdilenUcret = tables.Rows[i][15].ToDecimalObj();
                    var _PrimIkramiyeIstihkak = tables.Rows[i][16].ToDecimalObj();
                    var _IseGirisGun = tables.Rows[i][17].ToIntObj();
                    var _IseGirisAy = tables.Rows[i][18].ToIntObj();
                    var _IstenCikisGun = tables.Rows[i][19].ToIntObj();
                    var _IstenCikisAy = tables.Rows[i][20].ToIntObj();
                    var _IseGirisGunStr = _IseGirisGun.HasValue ? _IseGirisGun.Value.ToIntToStrAy_Gun() : "";
                    var _IseGirisAyStr = _IseGirisAy.HasValue ? _IseGirisAy.Value.ToIntToStrAy_Gun() : "";
                    var _IstenCikisGunStr = _IstenCikisGun.HasValue ? _IstenCikisGun.Value.ToIntToStrAy_Gun() : "";
                    var _IstenCikisAyStr = _IstenCikisAy.HasValue ? _IstenCikisAy.Value.ToIntToStrAy_Gun() : "";
                    var _IstenCikisNedenKodu = tables.Rows[i][21].ToIntObj();
                    var _EksikGunSayisi = tables.Rows[i][22].ToIntObj();
                    var _EksikGunNedenKodu = tables.Rows[i][23].ToIntObj();
                    var _MeslekTurKodu = tables.Rows[i][24].ToStrObjEmptString().Trim().Replace(",", ".");
                    var _IstirahatSurecindeCalismamistir = tables.Rows[i][25].ToIntObj();
                    var _TahakkukNedeni = tables.Rows[i][26].ToStrObjEmptString().Trim();
                    var _HizmetDonemAy = tables.Rows[i][27].ToStrObjEmptString().Trim();
                    var _HizmetDonemYil = tables.Rows[i][28].ToStrObjEmptString().Trim();
                    var _GvDenMuafmi = tables.Rows[i][29].ToIntObj();
                    // var _AsgariGecimIndirimi = tables.Rows[i][29].ToDecimalObj();
                    var _GvMatrahi = tables.Rows[i][30].ToDecimalObj();
                    var _GvEngellilikOrani = tables.Rows[i][31].ToIntObj();
                    var _HesaplananGv = tables.Rows[i][32].ToDecimalObj();
                    var _AsgariGecimIstisnaGvTutar = tables.Rows[i][33].ToDecimalObj();
                    var _GvKesinti = tables.Rows[i][34].ToDecimalObj();
                    var _AsgariGecimIstisnaDvTutar = tables.Rows[i][35].ToDecimalObj();
                    var _DvKesintisi = tables.Rows[i][36].ToDecimalObj();
                    Model.Data.Add(new ExcelDataImportRow
                    {
                        SayfaNo = iSht + 1,
                        SatirNo = i + 2,
                        HizmetDonemYil = _HizmetDonemYil,
                        HizmetDonemAy = _HizmetDonemAy,
                        BelgeMahiyetTipKodu = _BelgeMahiyetTipKodu,
                        BelgeTurKodu = _BelgeTurKodu,
                        DEsasKanunNo = _DEsasKanunNo,
                        YeniUniteKodu = _YeniUniteKodu,
                        EskiUniteKodu = _EskiUniteKodu,
                        IsyeriSiraNumarasi = _IsyeriSiraNumarasi,
                        IlKodu = _IlKodu,
                        AltIsverenNumarasi = _AltIsverenNumarasi,
                        VeriGirisTipID = _VeriGirisTipID,
                        TcKimlikNo = _TcKimlikNo,
                        Ad = _Ad,
                        Soyad = _Soyad,
                        PrimOdemeGun = _PrimOdemeGun,
                        UzaktanCalismaGun = _UzaktanCalismaGun,
                        HakEdilenUcret = _HakEdilenUcret,
                        PrimIkramiyeIstihkak = _PrimIkramiyeIstihkak,
                        IseGirisGun = _IseGirisGun,
                        IseGirisGunStr = _IseGirisGunStr,
                        IseGirisAy = _IseGirisAy,
                        IseGirisAyStr = _IseGirisAyStr,
                        IstenCikisGun = _IstenCikisGun,
                        IstenCikisGunStr = _IstenCikisGunStr,
                        IstenCikisAy = _IstenCikisAy,
                        IstenCikisAyStr = _IstenCikisAyStr,
                        IstenCikisNedenKodu = _IstenCikisNedenKodu,
                        EksikGunSayisi = _EksikGunSayisi,
                        EksikGunNedenKodu = _EksikGunNedenKodu,
                        MeslekTurKodu = _MeslekTurKodu,
                        IstirahatSurecindeCalismamistir = _IstirahatSurecindeCalismamistir,
                        TahakkukNedeni = _TahakkukNedeni,
                        GvDenMuafmi = _GvDenMuafmi,
                        // AsgariGecimIndirimi = _AsgariGecimIndirimi,
                        GvMatrahi = _GvMatrahi,
                        GvEngellilikOrani = _GvEngellilikOrani,
                        HesaplananGv = _HesaplananGv,
                        AsgariGecimIstisnaGvTutar = _AsgariGecimIstisnaGvTutar,
                        GvKesinti = _GvKesinti,
                        AsgariGecimIstisnaDvTutar = _AsgariGecimIstisnaDvTutar,
                        DvKesintisi = _DvKesintisi,


                    });
                }
            }



            try
            {
                excelConnection.Close();
                excelConnection1.Close();

            }
            catch (Exception ex)
            {
            }


            return Model;
        }
        public static Exception IsciAktarilanExcelHataKontrolu(this ExcelImportDataModel model, List<VASurecleriBirimVerileri> EklenmesiGerekenVeriler)
        {
            Exception excpt = null;
            try
            {

                int BilgiYazimHucreNo = 38;
                Application excel = new Application();
                var path = System.Web.HttpContext.Current.Server.MapPath("~" + model.DosyaYolu);
                Workbook workbook = excel.Workbooks.Open(path, ReadOnly: false, Editable: true);

                var MaxRowNum = model.Data.Max(s => s.SatirNo);
                int SNo = 0;
                foreach (var itemRow in model.Data)
                {
                    Worksheet worksheet = workbook.Worksheets.Item[itemRow.SayfaNo] as Worksheet;

                    if (itemRow.SayfaNo != SNo)
                    {
                        SNo = itemRow.SayfaNo;
                        Range rng = worksheet.Range["A2:AL" + (MaxRowNum + 1)];
                        rng.Interior.Color = Color.Transparent;
                        rng.Font.Color = Color.Black;

                        rng.Borders.Color = Color.Black;
                        Range rng2 = worksheet.Range["AL2:AL" + (MaxRowNum + 1)];
                        rng2.Value = "";


                    }

                    Range AckCell = worksheet.Cells[itemRow.SatirNo, BilgiYazimHucreNo];
                    AckCell.Value = "";

                    if (itemRow.HataliHucreler.Count < 20)
                    {
                        foreach (var item in itemRow.HataliHucreler)
                        {
                            Range Satir = worksheet.Cells[itemRow.SatirNo, item + 1];
                            Satir.Interior.Color = Color.OrangeRed;
                        }
                        if (itemRow.HataliHucreler.Any())
                        {
                            Range HcAck = worksheet.Cells[itemRow.SatirNo, BilgiYazimHucreNo];
                            HcAck.Value = "Hatalı Satır: " + itemRow.HataAciklamasi;
                            HcAck.Font.Color = Color.Red;
                            HcAck.Borders.Color = Color.Black;
                            HcAck.Merge();
                        }
                    }

                }
                Worksheet worksheet1 = workbook.Worksheets.Item[1] as Worksheet;

                foreach (var item in EklenmesiGerekenVeriler)
                {

                    MaxRowNum = MaxRowNum + 1;
                    Range HcAck = worksheet1.Cells[MaxRowNum, 1];

                    HcAck = worksheet1.Cells[MaxRowNum, 11];
                    HcAck.Value = item.TcKimlikNo;
                    HcAck.Font.Color = Color.Red;
                    HcAck.Borders.Color = Color.Black;
                    HcAck.Merge();

                    HcAck = worksheet1.Cells[MaxRowNum, 12];
                    HcAck.Value = item.Ad;
                    HcAck.Font.Color = Color.Red;
                    HcAck.Borders.Color = Color.Black;
                    HcAck.Merge();

                    HcAck = worksheet1.Cells[MaxRowNum, 13];
                    HcAck.Value = item.Soyad;
                    HcAck.Font.Color = Color.Red;
                    HcAck.Borders.Color = Color.Black;
                    HcAck.Merge();

                    HcAck = worksheet1.Cells[MaxRowNum, BilgiYazimHucreNo];
                    HcAck.Value = "Hatalı Satır: " + "Kişi bilgisinin eklenmesi gerekmektedir.";
                    HcAck.Font.Color = Color.Red;
                    HcAck.Borders.Color = Color.Black;
                    HcAck.Merge();
                }
                worksheet1.Columns.AutoFit();
                excel.Application.ActiveWorkbook.Save();
                try
                {
                    workbook.Close();
                    excel.Application.ActiveWorkbook.Close();
                }
                catch (Exception)
                {
                }

                excel.Application.Quit();
                excel.Quit();
                //if (File.Exists(path))
                //{
                //    try
                //    {
                //        File.Delete(path);
                //    }
                //    catch (Exception ex)
                //    {
                //        Management.SistemBilgisiKaydet("Kontrol edilen excel dosyası silinemedi! Hata:" + ex.ToExceptionMessage(), "ExcelImportModel/AktarilanExcelHataKontrolu", BilgiTipi.OnemsizHata, UserIdentity.Current.Id, UserIdentity.Ip);
                //    }

                //}

            }
            catch (Exception e)
            {
                excpt = e;
            }
            return excpt;
        }

        public static ExcelImportYevmiyeMaasModel ToMaasDonusturIterateRows(this HttpPostedFileBase item)
        {

            var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
            string extension = Path.GetExtension(item.FileName);

            string fileName = unqCode.ReplaceSpecialCharacter() + "_" + item.FileName.Replace(extension, "").ReplaceSpecialCharacter() + extension;
            var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/TempDocumentFolder"), fileName);

            item.SaveAs(path);
            FileInfo file = new FileInfo(path);

            var Model = new ExcelImportYevmiyeMaasModel();
            Model.DosyaAdi = file.Name;
            Model.DosyaYolu = "/TempDocumentFolder/" + fileName;
            DataSet ds = new DataSet();
            string fileExtension = System.IO.Path.GetExtension(item.FileName);
            string ConnectionString = string.Empty;
            if (fileExtension == ".xls") ConnectionString = path.ToConnectionStringXls();
            else if (fileExtension == ".xlsx") ConnectionString = path.ToConnectionStringXlsx();
            OleDbConnection excelConnection = new OleDbConnection(ConnectionString);
            excelConnection.Open();
            var dt = new System.Data.DataTable();

            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (dt == null)
            {
                return null;
            }
            String[] excelSheets = new String[dt.Rows.Count];
            int t = 0;
            //excel data saves in temp file here.
            foreach (DataRow row in dt.Rows)
            {
                excelSheets[t] = row["TABLE_NAME"].ToString();
                t++;
            }
            OleDbConnection excelConnection1 = new OleDbConnection(ConnectionString);

            for (int iSht = 0; iSht < excelSheets.Length; iSht++)
            {
                string query = string.Format("Select * from [{0}]", excelSheets[iSht]);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    if (ds.Tables.Count > 0) ds.Tables[0].Rows.Clear();
                    dataAdapter.Fill(ds);
                }
                var tables = ds.Tables[0];
                for (int i = 0; i < tables.Rows.Count; i++)
                {
                    var Row = new ExcelDataImportYevmiyeMaasRow();
                    for (int ci = 0; ci < 12; ci++)
                    {
                        Row.ColumnValues.Add(tables.Rows[i][ci].ToStrObjEmptString());
                    }

                    Model.Data.Add(Row);
                }
            }



            try
            {
                excelConnection.Close();
                excelConnection1.Close();

            }
            catch (Exception ex)
            {
            }


            return Model;
        }

        public static ExcelImportYevmiyeDataModel ToYevmiyeIterateRows(this HttpPostedFileBase item, int Yil)
        {

            var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
            string extension = Path.GetExtension(item.FileName);

            string fileName = unqCode.ReplaceSpecialCharacter() + "_" + Yil + item.FileName.Replace(extension, "").ReplaceSpecialCharacter() + extension;
            var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/TempDocumentFolder"), fileName);

            item.SaveAs(path);
            FileInfo file = new FileInfo(path);

            var Model = new ExcelImportYevmiyeDataModel();
            Model.DosyaAdi = file.Name;
            Model.DosyaYolu = "/TempDocumentFolder/" + fileName;
            DataSet ds = new DataSet();
            string fileExtension = System.IO.Path.GetExtension(item.FileName);
            string ConnectionString = string.Empty;
            if (fileExtension == ".xls") ConnectionString = path.ToConnectionStringXls();
            else if (fileExtension == ".xlsx") ConnectionString = path.ToConnectionStringXlsx();
            OleDbConnection excelConnection = new OleDbConnection(ConnectionString);
            excelConnection.Open();
            var dt = new System.Data.DataTable();

            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (dt == null)
            {
                return null;
            }
            String[] excelSheets = new String[dt.Rows.Count];
            int t = 0;
            //excel data saves in temp file here.
            foreach (DataRow row in dt.Rows)
            {
                excelSheets[t] = row["TABLE_NAME"].ToString();
                t++;
            }
            OleDbConnection excelConnection1 = new OleDbConnection(ConnectionString);

            for (int iSht = 0; iSht < excelSheets.Length; iSht++)
            {
                string query = string.Format("Select * from [{0}]", excelSheets[iSht]);
                using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                {
                    if (ds.Tables.Count > 0) ds.Tables[0].Rows.Clear();
                    dataAdapter.Fill(ds);
                }
                var tables = ds.Tables[0];
                for (int i = 0; i < tables.Rows.Count; i++)
                {

                    var _YevmiyeTarih = tables.Rows[i][0].ToDatetimeObj();
                    var _YevmiyeNo = tables.Rows[i][1].ToIntObj();
                    var _HarcamaBirim = tables.Rows[i][2].ToStrObjEmptString().Trim();
                    var _VergiKimlikNo = "";
                    var _HarcamaBirimAdi = "";
                    var HarcamaBirimSpl = _HarcamaBirim.Split('-').ToList();
                    if (HarcamaBirimSpl.Count > 1)
                    {
                        _VergiKimlikNo = HarcamaBirimSpl[0].ToStrObjEmptString();
                        _HarcamaBirimAdi = HarcamaBirimSpl[1];

                    }
                    else
                    {
                        _VergiKimlikNo = HarcamaBirimSpl[0];
                    }

                    var _HarcamaBirimKod = tables.Rows[i][3].ToStrObjEmptString().Trim();
                    var _HesapKod = tables.Rows[i][4].ToStrObjEmptString().Trim();
                    var _HesapAdi = tables.Rows[i][5].ToStrObjEmptString().Trim();
                    var _Borc = tables.Rows[i][6].ToDecimalObj();
                    var _Alacak = tables.Rows[i][7].ToDecimalObj();  
                    var _Aciklama = tables.Rows[i][8].ToStrObjEmptString().Trim();


                    Model.Data.Add(new ExcelDataImportYevmiyeRow
                    {
                        SayfaNo = iSht + 1,
                        SatirNo = i + 2,
                        YevmiyeTarih = _YevmiyeTarih,
                        YevmiyeNo = _YevmiyeNo,
                        VergiKimlikNo = _VergiKimlikNo,
                        HarcamaBirimAdi = _HarcamaBirimAdi,
                        HarcamaBirimKod = _HarcamaBirimKod,
                        HesapKod = _HesapKod,
                        HesapAdi = _HesapAdi,
                        Borc = _Borc,
                        Alacak = _Alacak,
                        Aciklama = _Aciklama,
                    });
                }
            }



            try
            {
                excelConnection.Close();
                excelConnection1.Close();

            }
            catch (Exception ex)
            {
            }


            return Model;
        }

        public static Exception YevmiyeAktarilanExcelHataKontrolu(this ExcelImportYevmiyeDataModel model)
        {
            Exception excpt = null;
            try
            {

                int BilgiYazimHucreNo = 10;
                Application excel = new Application();
                var path = System.Web.HttpContext.Current.Server.MapPath("~" + model.DosyaYolu);
                Workbook workbook = excel.Workbooks.Open(path, ReadOnly: false, Editable: true);
                foreach (var itemRow in model.Data.Where(p => p.HataliHucreler.Any() || !p.HataAciklamasi.IsNullOrWhiteSpace()))
                {
                    Worksheet worksheet = workbook.Worksheets.Item[itemRow.SayfaNo] as Worksheet;


                    Range AckCell = worksheet.Cells[itemRow.SatirNo, BilgiYazimHucreNo];
                    AckCell.Value = "";

                    if (itemRow.HataliHucreler.Count < 20)
                    {
                        foreach (var item in itemRow.HataliHucreler)
                        {
                            Range Satir = worksheet.Cells[itemRow.SatirNo, item + 1];
                            Satir.Interior.Color = Color.OrangeRed;
                        }
                        if (itemRow.HataliHucreler.Any())
                        {
                            Range HcAck = worksheet.Cells[itemRow.SatirNo, BilgiYazimHucreNo];
                            HcAck.Value = "Hatalı Satır: " + itemRow.HataAciklamasi;
                            HcAck.Font.Color = Color.Red;
                            HcAck.Borders.Color = Color.Black;
                            HcAck.Merge();
                        }
                    }
                }
                excel.Application.ActiveWorkbook.Save();
                excel.Application.Quit();
                excel.Quit();
                //if (File.Exists(path))
                //{
                //    try
                //    {
                //        File.Delete(path);
                //    }
                //    catch (Exception ex)
                //    {
                //        Management.SistemBilgisiKaydet("Kontrol edilen excel dosyası silinemedi! Hata:" + ex.ToExceptionMessage(), "ExcelImportModel/AktarilanExcelHataKontrolu", BilgiTipi.OnemsizHata, UserIdentity.Current.Id, UserIdentity.Ip);
                //    }

                //}

            }
            catch (Exception e)
            {
                excpt = e;
            }
            return excpt;
        }


    }



}