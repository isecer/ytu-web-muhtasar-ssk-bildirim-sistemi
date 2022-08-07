using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Models
{

    public class URoles
    {

        public int? YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public List<Roller> EklenenRoller { get; set; }
        public List<Roller> YetkiGrupRolleri { get; set; }
        public List<Roller> TumRoller { get; set; }
        public URoles()
        {
            EklenenRoller = new List<Roller>();
            YetkiGrupRolleri = new List<Roller>();
            TumRoller = new List<Roller>();
        }
    }

    public class FmSurecIslemleri : PagerOption
    {
        public int? Yıl { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrSurecIslemleri> Data { get; set; }
    }

    public class FrSurecIslemleri : VASurecleri
    {
        public string SurecYilAdi { get; set; }
        public string IslemYapan { get; set; }
        public bool AktifSurec { get; set; }
        public int SelectedTabIndex { get; set; }

        public List<FrBirimler> BirimData = new List<FrBirimler>();
        public List<VASurecleriAylar> AyData = new List<VASurecleriAylar>();
        public SelectList SListBirimDurum { get; set; }
        public SelectList SListAy { get; set; }
        public SelectList SListAyDurum { get; set; }
        public int? SBirimDurumID { get; set; }
        public int? SAyID { get; set; }

        public FrSecilenAyBilgi SecilenAyBilgi { get; set; }


    }
    public class FrSecilenAyBilgi : VASurecleriAylar
    {
        public bool IsVeriGirilebilir { get; set; }
        public string AyAdi { get; set; }
        public string AyDurumAdi { get; set; }
    }


    public class FmVeriGiris : PagerOption
    {
        public bool IsAktif { get; set; }
        public int? VASurecID { get; set; }
        public int? BirimID { get; set; }
        public int? AyID { get; set; }
        public string Aranan { get; set; }
        public int? VeriGirisTipID { get; set; }
        public Birimler BirimBilgi { get; set; }
        public string ToplamsalVeriHtml { get; set; }

        public List<FrVeriGirisi> Data = new List<FrVeriGirisi>();
    }

    public class FrVeriGirisi : VASurecleriBirimVerileri
    {
        public int VASurecID { get; set; }
        public string AyAdi { get; set; }
        public string BirimAdi { get; set; }
        public string BelgeMahiyetTipAdi { get; set; }
        public string BelgeTurAdi { get; set; }
        public string VeriGirisTipAdi { get; set; }
        public string IstenCikisNedenAdi { get; set; }
        public string EksikGunNedenAdi { get; set; }
        public string MeslekTurAdi { get; set; }
    }






    public class KmSurecIslemleri : VASurecleri
    {
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public bool BirimleriKopyala { get; set; }
    }



    public class FmKullanicilar : PagerOption
    {

        public int KullaniciID { get; set; }

        public int? BirimID { get; set; }
        public int? UnvanID { get; set; }
        public bool? IsAktif { get; set; }
        public int? YetkiGrupID { get; set; }
        public int? Cinsiyet { get; set; }
        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }
        public string EMail { get; set; }
        public string Telefon { get; set; }
        public bool? IsActiveDirectoryUser { get; set; }
        public bool? IsAdmin { get; set; }
        public string Aciklama { get; set; }
        public IEnumerable<FrKullanicilar> Data { get; set; }
        public FmKullanicilar()
        {
            Data = new FrKullanicilar[0];
        }
    }
    public class FrKullanicilar : Kullanicilar
    {
        public string YetkiGrupAdi { get; set; }
        public string EgitmenTipAdi { get; set; }
        public string AlternatifKadroTipAdi { get; set; }
        public string UnvanAdi { get; set; }
        public string BirimAdi { get; set; }
        public string BirimTreeAdi { get; set; }
        public List<Birimler> BirimYetkileri { get; set; }

        public Dictionary<string, int?> SeciliBirimID { get; set; }
        public Dictionary<string, int?> SeciliVASurecID { get; set; }
        public Dictionary<string, int?> SeciliAyID { get; set; }
        public Dictionary<string, int?> SeciliYil { get; set; }
        public FrKullanicilar()
        {
            BirimYetkileri = new List<Birimler>();
            SeciliBirimID = new Dictionary<string, int?>();
            SeciliVASurecID = new Dictionary<string, int?>();
            SeciliAyID = new Dictionary<string, int?>();
            SeciliYil = new Dictionary<string, int?>();
        }

    }


    public class FmYetkiGruplari : PagerOption
    {
        public int YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public IEnumerable<FrYetkiGruplari> Data { get; set; }
    }
    public class FrYetkiGruplari
    {
        public int YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public string EnstituKod { get; set; }
        public string EnstituAdi { get; set; }
        public int YetkiSayisi { get; set; }
    }

    public class FmUnvanlar : PagerOption
    {
        public string UnvanAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<Unvanlar> Data { get; set; }

    }

    public class FmBirimler : PagerOption
    {
        public bool? IsVeriGirisiYapilabilir { get; set; }
        public int? VeriGirisTipID { get; set; }
        public bool? IsAyAsiriHesaplama { get; set; }
        public bool? IsYevmiyeVeriGirisiYapilabilir { get; set; }
        public bool? IsUniversiteIsyeri { get; set; }
        public string Aranan { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrBirimler> Data { get; set; }

    }
    public class FrBirimler : Birimler
    {
        public string IslemYapan { get; set; }
        public string BirimTreeAdi { get; set; }
        public string VeriGirisTipAdi { get; set; }
        public bool VeriGirisVar { get; set; }
        public FrBirimler()
        {
        }
    }
    public class FmBelgeMahiyetTipleri : PagerOption
    {
        public string BelgeMahiyetTipKodu { get; set; }
        public string BelgeMahiyetTipAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrBelgeMahiyetTipleri> Data { get; set; }

    }
    public class FrBelgeMahiyetTipleri : BelgeMahiyetTipleri
    {
        public string IslemYapan { get; set; }
        public FrBelgeMahiyetTipleri()
        {
        }
    }
    public class FmBelgeTurleri : PagerOption
    {
        public string BelgeTurKodu { get; set; }
        public string BelgeTurAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrBelgeTurleri> Data { get; set; }

    }
    public class FrBelgeTurleri : BelgeTurleri
    {
        public string IslemYapan { get; set; }
        public FrBelgeTurleri()
        {
        }
    }
    public class FmIstenCikisNedenleri : PagerOption
    {
        public int? IstenCikisNedenKodu { get; set; }
        public string IstenCikisNedenAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrIstenCikisNedenleri> Data { get; set; }

    }
    public class FrIstenCikisNedenleri : IstenCikisNedenleri
    {
        public string IslemYapan { get; set; }
        public FrIstenCikisNedenleri()
        {
        }
    }
    public class FmEksikGunNedenleri : PagerOption
    {
        public int? EksikGunNedenKodu { get; set; }
        public string EksikGunNedenAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrEksikGunNedenleri> Data { get; set; }

    }
    public class FrEksikGunNedenleri : EksikGunNedenleri
    {
        public string IslemYapan { get; set; }
        public FrEksikGunNedenleri()
        {
        }
    }
    public class FmMeslekTurleri : PagerOption
    {
        public string MeslekTurKodu { get; set; }
        public string MeslekTurAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrMeslekTurleri> Data { get; set; }

    }
    public class FrMeslekTurleri : MeslekTurleri
    {
        public string IslemYapan { get; set; }
        public FrMeslekTurleri()
        {
        }
    }
    public class FmSistemBilgilendirme : PagerOption
    {
        public Nullable<byte> BilgiTipi { get; set; }
        public string Kategori { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Nullable<System.DateTime> IslemZamani { get; set; }
        public string IpAdresi { get; set; }
        public string AdSoyad { get; set; }

        public IEnumerable<FrSistemBilgilendirme> Data { get; set; }

    }
    public class FrSistemBilgilendirme
    {
        public int SistemBilgiID { get; set; }
        public Nullable<byte> BilgiTipi { get; set; }
        public string Kategori { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Nullable<System.DateTime> IslemZamani { get; set; }
        public string IpAdresi { get; set; }
        public string AdSoyad { get; set; }
        public string KullaniciAdi { get; set; }
    }
    public class FmDuyurular : PagerOption
    {
        public string Baslik { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public bool? IsAktif { get; set; }
        public string DuyuruYapan { get; set; }
        public IEnumerable<FrDuyurular> Data { get; set; }
    }

    public class FrDuyurular : Duyurular
    {

        public string DuyuruYapan { get; set; }
        public int EkSayisi { get; set; }
        public List<DuyuruEkleri> Ekler { get; set; }
    }
    public class FmMailSablonlari : PagerOption
    {
        public int? MailSablonTipID { get; set; }
        public string SablonAdi { get; set; }
        public DateTime? Tarih { get; set; }
        public string Sablon { get; set; }
        public bool? IsAktif { get; set; }
        public string DuyuruYapan { get; set; }
        public IEnumerable<FrMailSablonlari> Data { get; set; }
    }

    public class FrMailSablonlari : MailSablonlari
    {
        public string SablonTipAdi { get; set; }
        public string Parametreler { get; set; }
        public string IslemYapan { get; set; }
        public int EkSayisi { get; set; }
    }
    public class FmMesajKategorileri : PagerOption
    {
        public string KategoriAdi { get; set; }
        public string KategoriAciklamasi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<MesajKategorileri> Data { get; set; }
    }
    public class FrMesajKategorileri : MesajKategorileri
    {
        public string IslemYapan { get; set; }
    }

    public class FmMesajlar : PagerOption
    {
        public int? MesajKategoriID { get; set; }
        public string Konu { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public bool? IsAktif { get; set; }
        public string AdSoyad { get; set; }
        public IEnumerable<FrMesajlar> Data { get; set; }
    }


    public class FrMesajlar : Mesajlar
    {
        public string KategoriAdi { get; set; }
        public string BirimAdi { get; set; }
        public Kullanicilar Kullanici { get; set; }
        public int EkSayisi { get; set; }
        public List<SubMessages> SubMesajList { get; set; }


    }

    public class SubMessages
    {
        public int KullaniciID { get; set; }
        public string EMail { get; set; }
        public DateTime Tarih { get; set; }
        public string ResimYolu { get; set; }
        public string AdSoyad { get; set; }
        public int MesajID { get; set; }
        public string Icerik { get; set; }
        public string IslemYapanIP { get; set; }
        public List<MesajEkleri> Ekler { get; set; }
        public List<GonderilenMailKullanicilar> Gonderilenler { get; set; }

    }

    public class FmMailGonderme : PagerOption
    {
        public string Konu { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public string MailGonderen { get; set; }
        public IEnumerable<FrMailGonderme> Data { get; set; }
    }
    public class FrMailGonderme : GonderilenMailler
    {
        public string MailGonderen { get; set; }
        public int EkSayisi { get; set; }
        public int KisiSayisi { get; set; }

    }

    public class MailKullaniciBilgi
    {

        public bool Checked { get; set; }
        public int KullaniciID { get; set; }
        public string AdSoyad { get; set; }
        public string BirimAdi { get; set; }
        public string Email { get; set; }

    }

    public class UrlInfoModel
    {
        public string Root { get; set; }
        public string AbsolutePath { get; set; }
        public string LastPath { get; set; }
        public string Query { get; set; }
    }

    public class ChkListModel
    {
        public string PanelTitle { get; set; }
        public string TableID { get; set; }
        public string InputName { get; set; }
        public IEnumerable<CheckObject<ChkListDataModel>> Data { get; set; }
        public bool AllDataChecked
        {
            get
            {

                return Data.Any() && Data.Select(s => s.Value).Count() == Data.Where(p => p.Checked == true).Select(s => s.Value).Count();

            }
        }
        public ChkListModel(string InputName = "")
        {
            this.InputName = InputName;
            var ID = Guid.NewGuid().ToString().Substr(0, 4);
            TableID = ID;
        }
    }
    public class ChkListDataModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Caption { get; set; }
        public string Detail { get; set; }
    }
    public class RaporSendPdfModel
    {
        public MemoryStream RaporMemoryStream { get; set; }
        public string DisplayName { get; set; }

    }

    #region ComboModels 

    public class ComboModelBool
    {
        public bool? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelBoolDatetime
    {
        public int GroupID { get; set; }
        public bool? Value { get; set; }
        public DateTime? Caption { get; set; }
    }
    public class ComboModelInt
    {
        public int? Value { get; set; }
        public string Caption { get; set; }
    }


    public class ComboModelIntChecked
    {
        public bool Checked { get; set; }
        public int? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelLong
    {
        public long? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelInt_multi
    {
        public int Inx { get; set; }
        public int Key { get; set; }
        public int Value { get; set; }
        public string ValueS { get; set; }
        public string ValueS2 { get; set; }
        public string ValueS3 { get; set; }
        public bool ValueB { get; set; }
        public bool ValueB2 { get; set; }
        public double ValueDouble { get; set; }
        public double ValueDouble2 { get; set; }
        public DateTime? DateTime1 { get; set; }
        public DateTime? DateTime2 { get; set; }
    }
    public class ComboModelIntInt
    {
        public int Value { get; set; }
        public int Caption { get; set; }
    }
    public class ComboModelPageIndex
    {
        public int StartRowIndex { get; set; }
        public int PageIndex { get; set; }
    }

    public class ComboModelString
    {
        public string Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelDatetime
    {
        public int Value { get; set; }
        public DateTime? Caption { get; set; }
    }
    public class ComboModelIntBool
    {
        public int Value { get; set; }
        public bool Caption { get; set; }
    }
    #endregion

    #region SabitTanimlar


    public static class RaporTipleri
    {

        public static int BirimToplamsalRapor { get; set; } = 1;
        public static int BildirgeToplamsalRapor { get; set; } = 2;
    }

    public static class Cinsiyet
    {
        public const byte Erkek = 1;
        public const byte Kadın = 2;

    }
    public static class MedeniHal
    {
        public const byte Bekar = 1;
        public const byte Evli = 2;

    }

    public static class ModalSizeClass
    {

        public const string Small = "modal-dialog modal-sm";
        public const string Basic = "modal-dialog";
        public const string Large = "modal-dialog modal-lg";
    }

    public enum HaftaGunleriEnum
    {
        Pazar = 0,
        Pazartesi = 1,
        Salı = 2,
        Çarşamba = 3,
        Perşembe = 4,
        Cuma = 5,
        Cumartesi = 6
    }
    public static class HaftaGunleri
    {
        public static int Pazartesi { get; set; } = 1;
        public static int Salı { get; set; } = 2;
        public static int Çarşamba { get; set; } = 3;
        public static int Perşembe { get; set; } = 4;
        public static int Cuma { get; set; } = 5;
        public static int Cumartesi { get; set; } = 6;
        public static int Pazar { get; set; } = 0;
    }
    public enum Msgtype
    {
        Success, Error, Warning, Information, Nothing
    }
    public static class BilgiTipi
    {
        public const byte Hata = 1;
        public const byte Uyarı = 2;
        public const byte Kritik = 3;
        public const byte OnemsizHata = 4;
        public const byte Saldırı = 5;
        public const byte LoginHatalari = 6;
        public const byte Bilgi = 7;
    }


    public static class ZamanTipi
    {
        public const byte Yil = 1;
        public const byte Ay = 2;
        public const byte Gun = 3;
        public const byte Saat = 4;
        public const byte Dakika = 5;
    }
    public static class DenklikTipi
    {
        public const byte Kucuk = 1;
        public const byte Buyuk = 2;
        public const byte Esit = 3;
    }
    public static class RenkTiplier
    {
        public static string Primary = "#33414e";
        public static string Info = "#3fbae4";
        public static string Warning = "#fea223";
        public static string Success = "#95b75d";
        public static string Danger = "#b64645";
    }
    public static class HttpDurumKod
    {
        public const int Continue = 100;
        public const int SwitchingProtocols = 101;
        public const int Processing = 102;
        public const int OK = 200;
        public const int Created = 201;
        public const int Accepted = 202;
        public const int NonAuthoritativeInformation = 203;
        public const int NoContent = 204;
        public const int ResetContent = 205;
        public const int PartialContent = 206;
        public const int MultiStatus = 207;
        public const int ContentDifferent = 210;
        public const int MultipleChoices = 300;
        public const int MovedPermanently = 301;
        public const int MovedTemporarily = 302;
        public const int SeeOther = 303;
        public const int NotModified = 304;
        public const int UseProxy = 305;
        public const int TemporaryRedirect = 307;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int PaymentRequired = 402;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int NotAccessMethod = 405;
        public const int NotAcceptable = 406;
        public const int UnLoginToProxyServer = 407;
        public const int RequestTimeOut = 408;
        public const int Conflict = 409;
        public const int Gone = 410;
        public const int LengthRequired = 411;
        public const int PreconditionAiled = 412;
        public const int RequestEntityTooLarge = 413;
        public const int RequestURITooLong = 414;
        public const int UnsupportedMediaType = 415;
        public const int RequestedrangeUnsatifiable = 416;
        public const int Expectationfailed = 417;
        public const int Unprocessableentity = 422;
        public const int Locked = 423;
        public const int Methodfailure = 424;
        public const int InternalServerError = 500;
        public const int Uygulanmamış = 501;
        public const int GeçersizAğGeçidi = 502;
        public const int HizmetYok = 503;
        public const int GatewayTimeout = 504;
        public const int HTTPVersionNotSupported = 505;
        public const int InsufficientStorage = 507;

    }


    public class BilgiTipleri
    {
        public List<BilgiRow> BilgiTip { get; set; }
        public BilgiTipleri()
        {

            var dct = new List<BilgiRow>();
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Hata, BilgiTipAdi = "Hata", BilgiTipCls = "primary" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Uyarı, BilgiTipAdi = "Uyarı", BilgiTipCls = "warning" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Kritik, BilgiTipAdi = "Kritik Durum", BilgiTipCls = "danger" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.OnemsizHata, BilgiTipAdi = "Önemsiz Hata", BilgiTipCls = "default" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Saldırı, BilgiTipAdi = "Saldırı", BilgiTipCls = "danger" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.LoginHatalari, BilgiTipAdi = "loginHatalari", BilgiTipCls = "info" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Bilgi, BilgiTipAdi = "Bilgi", BilgiTipCls = "success" });
            BilgiTip = dct;



        }


    }



    [Serializable()]
    public static class TreeExt
    {
        [Serializable()]
        public class TreeExtRow<T> where T : class
        {
            public string ParentLevel { get; set; }
            public int Level { get; set; }
            public bool HasChild { get; set; }
            public T Value { get; set; }
        }
        [Serializable()]
        public class TreeExtNode<T> where T : class
        {
            public string ParentLevel { get; set; }
            public int Level { get; set; }
            public T Value { get; set; }
            public bool Checked { get; set; }
            public TreeExtNode<T> Parent { get; set; }
            public List<TreeExtNode<T>> Children { get; set; }
        }

        public static IEnumerable<TreeExtRow<T>> CastToTree<T>(this IEnumerable<T> source, string RootPropertyField, string ParentPropertyField) where T : class
        {
            List<TreeExtRow<T>> resultList = new List<TreeExtRow<T>>();

            if (source == null || source.Count() == 0) return resultList.AsEnumerable();
            var type = typeof(T);
            var ids = source.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).Distinct().ToArray();

            //List<T> roots = new List<T>(); 
            //foreach (var l in source) 
            //{ 
            //    if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l); 
            //    else 
            //    { 
            //        var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString(); 
            //        if (ids.Contains(bid) == false) roots.Add(l); 
            //    } 
            //} 
            var roots = source.Where(p => type.GetProperty(ParentPropertyField).GetValue(p, null) == null ||
                                      ids.Contains(type.GetProperty(ParentPropertyField).GetValue(p, null).ToString()) == false);
            Func<TreeExtRow<T>, int> fxDetail = null;
            fxDetail = new Func<TreeExtRow<T>, int>
                (
                   (parent) =>
                   {
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent.Value, null);
                       var details = source.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root 
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();
                       parent.HasChild = details.Length > 0;
                       foreach (var detail in details)
                       {
                           TreeExtRow<T> row = new TreeExtRow<T>();
                           row.Value = detail;
                           row.Level = parent.Level + 1;
                           row.ParentLevel = parent.ParentLevel + "_" + type.GetProperty(RootPropertyField).GetValue(detail, null).ToString();
                           resultList.Add(row);
                           fxDetail(row);
                       }
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                TreeExtRow<T> row = new TreeExtRow<T>();
                row.ParentLevel = type.GetProperty(RootPropertyField).GetValue(root, null).ToString();
                row.Level = 0;
                row.Value = root;
                row.HasChild = false;
                resultList.Add(row);
                fxDetail(row);
            }
            return resultList.AsEnumerable();
        }


        public static TreeExtNode<T> HasChildNode<T>(this TreeExtNode<T> Parent, object ID, string IDField) where T : class
        {
            var type = typeof(T);
            TreeExtNode<T> result = null;
            Func<TreeExtNode<T>, bool> fxDetail = null;
            fxDetail = new Func<TreeExtNode<T>, bool>((parent) =>
            {
                if (parent != null && parent.Children != null && parent.Children.Count > 0)
                {
                    foreach (var child in parent.Children)
                    {
                        object idValue = type.GetProperty(IDField).GetValue(child.Value, null);
                        if ((idValue == null && ID == null) || (idValue.ToString() == ID.ToString()))
                        {
                            result = child;
                            break;
                        }
                        else
                        {
                            fxDetail(child);
                        }
                    }
                }
                return false;
            });
            fxDetail(Parent);
            return result;
        }

        public static bool InChildNode<T>(this TreeExtNode<T> Parent, object ID, string IDField, string ParentIDField) where T : class
        {
            var type = typeof(T);

            bool result = false;
            Func<TreeExtNode<T>, bool> InSub = null;

            TreeExtNode<T> toNode = null;
            object ParentID = type.GetProperty(IDField).GetValue(Parent.Value, null);
            #region insub
            InSub = (TreeExtNode<T> parent) =>
            {
                foreach (var item in parent.Children)
                {
                    object id = type.GetProperty(IDField).GetValue(item.Value, null);
                    if (id == ID)
                    {
                        toNode = item;
                        result = InChildNode(Parent, toNode, IDField, ParentIDField);
                        #region  aa
                        /*
                        #region go to root
                        var toNodeParent = toNode;
                        while( (toNodeParent=toNode.Parent) !=null)
                        {
                            var toNodeParentID = type.GetProperty(ParentIDField).GetValue(toNodeParent.Value, null);
                            if (toNodeParentID == ParentID) 
                            {
                                result = true; break;
                            }
                        }
                        #endregion
                        */
                        #endregion
                        break;
                    }
                    else
                    {
                        InSub(item);
                    }
                }
                return false;
            };
            InSub(Parent);
            #endregion
            return result;
        }
        public static bool InChildNode<T>(this TreeExtNode<T> thisNode, TreeExtNode<T> toNode, string IDField, string ParentIDField) where T : class
        {
            var type = typeof(T);
            bool result = false;
            object ParentID = type.GetProperty(IDField).GetValue(thisNode.Value, null);
            #region go to root
            var toNodeParent = toNode;
            while ((toNodeParent = toNode.Parent) != null)
            {
                var toNodeParentID = type.GetProperty(ParentIDField).GetValue(toNodeParent.Value, null);
                if (toNodeParentID == ParentID)
                {
                    result = true; break;
                }
            }
            #endregion
            return result;
        }



        public static IEnumerable<TreeExtNode<T>> CastToTreeNode<T>(this IEnumerable<T> source, string RootPropertyField, string ParentPropertyField, params int[] notThisNodeChildNodes) where T : class
        {
            List<TreeExtNode<T>> resultList = new List<TreeExtNode<T>>();

            if (source == null || source.Count() == 0) return resultList.AsEnumerable();
            var type = typeof(T);
            var ids = source.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).Distinct().ToArray();

            var roots = source.Where(p => type.GetProperty(ParentPropertyField).GetValue(p, null) == null ||
                                      ids.Contains(type.GetProperty(ParentPropertyField).GetValue(p, null).ToString()) == false);
            Func<TreeExtNode<T>, int> fxDetail = null;

            fxDetail = new Func<TreeExtNode<T>, int>
                (
                   (parent) =>
                   {

                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent.Value, null);
                       var details = source.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root 
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();

                       foreach (var detail in details)
                       {

                           #region Not This Child Nodes
                           if (notThisNodeChildNodes != null && notThisNodeChildNodes.Length > 0)
                           {
                               object detailParentID = type.GetProperty(ParentPropertyField).GetValue(detail, null);
                               if (detailParentID != null)
                               {
                                   if (notThisNodeChildNodes.Contains((int)detailParentID))
                                   {
                                       continue;
                                   }
                               }
                           }

                           #endregion
                           TreeExtNode<T> node = new TreeExtNode<T>();
                           node.Value = detail;
                           node.Parent = parent;
                           if (parent.Children == null) parent.Children = new List<TreeExtNode<T>>();
                           parent.Children.Add(node);

                           node.Value = detail;
                           node.Level = parent.Level + 1;
                           node.ParentLevel = parent.ParentLevel + "_" + type.GetProperty(RootPropertyField).GetValue(detail, null).ToString();

                           resultList.Add(node);
                           fxDetail(node);
                       }
                       return 0;
                   }
                );
            foreach (var root in roots)
            {
                TreeExtNode<T> node = new TreeExtNode<T>();
                node.Value = root;
                node.Parent = null;
                node.Children = new List<TreeExtNode<T>>();
                node.ParentLevel = type.GetProperty(RootPropertyField).GetValue(root, null).ToString();
                node.Level = 0;
                resultList.Add(node);
                fxDetail(node);
            }
            return resultList.AsEnumerable();
        }

    }
    public class MrMessage
    {

        public string DialogID { get; set; }
        public bool IsSucces { get; set; }

        public string PropertyName { get; set; }
        public bool AddIcon { get; set; }
        public string HtmlData { get; set; }
        public List<int> ReturnIds { get; set; }
        public Msgtype MessageType { get; set; }
        public MrMessage()
        {
            AddIcon = true;
            MessageType = Msgtype.Nothing;
        }
    }
    public class MmMessage
    {

        public bool IsDialog { get; set; }
        public string DialogID { get; set; }
        public bool IsCloseDialog { get; set; }
        public bool IsSuccess { get; set; }
        public Msgtype MessageType { get; set; }

        public string Title { get; set; }
        public string ReturnUrl { get; set; }
        public string ReturnHtml { get; set; }
        public int ReturnUrlTimeOut { get; set; }
        public List<string> Messages { get; set; }
        public List<MrMessage> MessagesDialog { get; set; }
        public MmMessage()
        {
            MessageType = Msgtype.Nothing;
            Messages = new List<string>();
            MessagesDialog = new List<MrMessage>();
            ReturnUrlTimeOut = 400;
        }

    }
    public class MrMesajBilgi
    {

        public int MesajlarID { get; set; }
        public int KullaniciID { get; set; }
        public DateTime Tarih { get; set; }
        public string Konu { get; set; }
        public string Aciklama { get; set; }

    }
    public class MesajBilgi
    {
        public int Count { get; set; }
        public List<MrMesajBilgi> Mesajlar { get; set; }
    }

    public class BilgiRow
    {
        public int BilgiTipID { get; set; }
        public string BilgiTipAdi { get; set; }
        public string BilgiTipCls { get; set; }
    }

    public static class DuyuruPopupTipleri
    {
        public const byte AnaSayfa = 1;

    }
    public class AjaxLoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string ReturnUrl { get; set; }
        public string NewGuid { get; set; }
        public string NewSrc { get; set; }
    }
    public class MIndexBilgi
    {
        public int Toplam { get; set; }
        public int Aktif { get; set; }
        public int Pasif { get; set; }
        public List<mxRowModel> ListB { get; set; }
        public MIndexBilgi()
        {
            ListB = new List<mxRowModel>();
        }
    }
    public class mxRowModel
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string ClassName { get; set; }
        public string Color { get; set; }
        public int Toplam { get; set; }

    }


    public class EkAciklamaContent
    {
        public string Baslik { get; set; }
        public List<ComboModelString> Detay { get; set; }
        public EkAciklamaContent()
        {
            Detay = new List<ComboModelString>();
        }
    }

    public class VeriGirisPopupExcelModel
    {
        public FrSurecIslemleri SurecBilgi { get; set; }
        public int VASurecleriBirimID { get; set; }
        public string BirimAdi { get; set; }
        public string VeriGirisTipAdi { get; set; }
    }
    public class YevmiyeVeriGirisPopupExcelModel
    {
        public int Yil { get; set; }
    }
    public class VeriGirisPopupToplamModel
    {
        public FrSurecIslemleri SurecBilgi { get; set; }
        public int BirimID { get; set; }
        public string BirimAdi { get; set; }
        public string VeriGirisTipAdi { get; set; }


        public int? VASurecleriBirimVerileriAylikToplamID { get; set; }
        public decimal? AsgariGecimIndirimiToplam { get; set; }
        public decimal? GvMatrahiToplam { get; set; }
        public decimal? GvKesintiToplam { get; set; }
        public decimal? PrimTutarToplam { get; set; }
    }
    public class FmYevmiyelerHarcamaBirimleri : PagerOption
    {
        public string VergiKimlikNo { get; set; }
        public string BirimAdi { get; set; }
        public bool? IsUniversiteIsyeri { get; set; }
        public string IsyeriKodu { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyelerHarcamaBirimleri> Data { get; set; }

    }
    public class FrYevmiyelerHarcamaBirimleri : YevmiyelerHarcamaBirimleri
    {
        public string IslemYapan { get; set; }
        public FrYevmiyelerHarcamaBirimleri()
        {
        }
    }
    public class FmYevmiyelerHesapKodlari : PagerOption
    {
        public int? YevmiyeHesapKodTurID { get; set; }
        public string HesapKod { get; set; }
        public string HesapAdi { get; set; }
        public bool? IsGelirKaydindaKullaniclacak { get; set; }
        public string VergiKodu { get; set; }
        public IEnumerable<FrYevmiyelerHesapKodlari> Data { get; set; }

    }
    public class FrYevmiyelerHesapKodlari : YevmiyelerHesapKodlari
    {
        public string HesapKodTurAdi { get; set; }
        public string IslemYapan { get; set; }
        public FrYevmiyelerHesapKodlari()
        {
        }
    }
    public class FmYevmiyeEkHbToplamlari : PagerOption
    {
        public int? Yil { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyeEkHbToplamlari> Data { get; set; }

    }
    public class FrYevmiyeEkHbToplamlari : YevmiyelerHarcamaBirimleri
    {
        public decimal Alacak { get; set; }
        public decimal Borc { get; set; }
        public decimal Kalan { get; set; }
        public decimal KayitEdilen { get; set; }
        
    }
    public class FmYevmiyeSendikaToplamlari : PagerOption
    {
        public int? Yil { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyeSendikaToplamlari> Data { get; set; }

    }
    public class FrYevmiyeSendikaToplamlari : YevmiyelerSendikaBilgileri
    {
        public decimal Alacak { get; set; }
        public decimal Borc { get; set; }
        public decimal Kalan { get; set; } 

    }
    public class FmYevmiyeBelgeKodlari : PagerOption
    {
        public string BelgeKodu { get; set; }
        public string BelgeAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyelerBelgeKodlari> Data { get; set; }

    }
    public class FrYevmiyelerBelgeKodlari : YevmiyelerBelgeKodlari
    {
        public string IslemYapan { get; set; }
        public FrYevmiyelerBelgeKodlari()
        {
        }
    }


    public class FmYevmiyeKdvKodlari : PagerOption
    {
        public string KdvKodu { get; set; }
        public string KdvAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyelerKdvKodlari> Data { get; set; }

    }
    public class FrYevmiyelerKdvKodlari : YevmiyelerKdvKodlari
    {
        public string IslemYapan { get; set; }
        public FrYevmiyelerKdvKodlari()
        {
        }
    }
    public class FmYevmiyelerProjeBankaHesapNumaralari : PagerOption
    {
        public string HesapNo { get; set; }
        public string HesapAdi { get; set; }
        public string ProjeNo { get; set; }
        public string ProjeAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyelerProjeBankaHesapNumaralari> Data { get; set; }

    }
    public class FrYevmiyelerProjeBankaHesapNumaralari : YevmiyelerProjeBankaHesapNumaralari
    {
        public string IslemYapan { get; set; }
        public FrYevmiyelerProjeBankaHesapNumaralari()
        {
        }
    }
    public class FmYevmiyeSendikaBilgileri : PagerOption
    {
        public string HesapKod { get; set; }
        public string VergiKimlikNo { get; set; }
        public string AdSoyad { get; set; }
        public string IBanNo { get; set; }
        public string KisaAdi { get; set; }
        public string Aciklama { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrYevmiyelerSendikaBilgileri> Data { get; set; }

    }
    public class FrYevmiyelerSendikaBilgileri : YevmiyelerSendikaBilgileri
    {
        public string IslemYapan { get; set; }
        public FrYevmiyelerSendikaBilgileri()
        {
        }
    }
    public class FmVASurecleriBirimVerileriAylikToplam
    {
        public bool IsAktif { get; set; }
        public int VASurecID { get; set; }
        public int BirimID { get; set; }
        public int AyID { get; set; }
        public List<FrVASurecleriBirimVerileriAylikToplam> Data = new List<FrVASurecleriBirimVerileriAylikToplam>();
    }

    public class FrVASurecleriBirimVerileriAylikToplam : VASurecleriBirimVerileriAylikToplam
    {
        public bool IsGirilenOrHesaplananToplam { get; set; }
    }
    public class FmYevmiyeler : PagerOption
    {
        public int? Yil { get; set; }
        public int? YevmiyeHarcamaBirimID { get; set; }
        public int? YevmiyeHesapKodTurID { get; set; }
        public bool? GelirKaydiOlacak { get; set; }
        public int? YevmiyeNo { get; set; }
        public string VergiKimlikNo { get; set; }
        public string HarcamaBirimKod { get; set; }
        public string HesapKod { get; set; }
        public string Aciklama { get; set; }

        public List<FrYevmiyeler> Data = new List<FrYevmiyeler>();
    }

    public class FrYevmiyeler : Yevmiyeler
    {
        public string BirimAdi { get; set; }
    }
    public class YevmiyeDetayModel : Yevmiyeler
    {
        public decimal? YevmiyeNoToplamGv { get; set; }
        public decimal? YevmiyeNoToplamDv { get; set; }
        public bool Is1003BYevmiyeParcalamaOlacak { get; set; }
        public bool Is1003AGelirKaydiOlacak { get; set; }
        public bool Is1003A10_24GelirKaydiOlacak { get; set; }
        public bool IsKdvVergiKaydiOlacak { get; set; }
        public bool IsEmekliKesenekKaydiOlacak { get; set; }
        public bool IsTasinirKontrolKaydiOlacak { get; set; }
        public bool IsSendikaKaydiOlacak { get; set; }
        public string EKHarcamaBirimAdi { get; set; }
        public bool IsgelirVergisiDamgaVergisiVar { get; set; }
        public SelectList SHesapKodlari1003A { get; set; }
        public SelectList EKHarcamaBirim { get; set; } 
    }

    public class RpModelToplamsalModel
    {
        public int BirimID { get; set; }
        public string BirimAdi { get; set; }
        public string DonemAdi { get; set; }
        public List<RpRowBirimToplamsal> Data { get; set; }
    }
    public class RpRowBirimToplamsal
    {
        public int BirimID { get; set; }

        public string BirimAdi { get; set; }
        public string BirimKodu { get; set; }
        public string BildirgeAdi { get; set; }
        public string BildirgeNo { get; set; }

        public decimal PrimeEsasKazancTutar { get; set; }
        public decimal PrimTutari { get; set; }
        public decimal Agi { get; set; }
        public decimal DvKesinti { get; set; }
        public decimal GvMatrah { get; set; }
        public decimal GvKesinti { get; set; }
        public int KisiSayisi { get; set; }
    }
    public class RpVASurecleriBirimVerileri : VASurecleriBirimVerileri
    {

    }
    #endregion

    #region Enums
    public static class HesapKoduTuru
    {
        public static byte SSKPrimHesapKodlari1003B = 1;
        public static byte VergiTevkifatHesapKodlari1003A = 2;
        public static byte KDVTevkifatHesapKodlari = 3;
        public static byte EmekliKesintiHesapKodlari = 4;
        public static byte TasinirKontrolHesapKodlari = 5;
        public static byte SendikaIslemleriHesapKodlari = 6;
    }
    #endregion
}