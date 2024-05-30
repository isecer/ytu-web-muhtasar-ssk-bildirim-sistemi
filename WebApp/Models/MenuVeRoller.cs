using BiskaUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class _Menuler : IMenu
    {

        [MenuAttribute(MenuID = 80000, MenuAdi = "İşçi İşlemleri", MenuCssClass = "fa fa-dollar", MenuUrl = "", DilCeviriYap = false, SiraNo = 6)]
        public const string VeriGirisIslemleri = "Veri Giriş İşlemleri";

        [MenuAttribute(MenuID = 82000, MenuAdi = "Yevmiye İşlemleri", MenuCssClass = "fa fa-dollar", MenuUrl = "", DilCeviriYap = false, SiraNo = 7)]
        public const string YevmiyeIslemleri = "Yevmiye İşlemleri";

        [MenuAttribute(MenuID = 84000, MenuAdi = "Rapor İşlemleri", MenuCssClass = "fa fa-area-chart", MenuUrl = "", DilCeviriYap = false, SiraNo = 12)]
        public const string RaporIslemleri = "RaporIslemleri";

        [MenuAttribute(MenuID = 84500, MenuAdi = "Kullanıcı İşlemleri", MenuCssClass = "fa fa-group", MenuUrl = "", DilCeviriYap = false, SiraNo = 15)]
        public const string KullaniciIslemleri = "KullaniciIslemleri";

        [MenuAttribute(MenuID = 85000, MenuAdi = "Tanımlamalar", MenuCssClass = "fa fa-gears", MenuUrl = "", DilCeviriYap = false, SiraNo = 18)]
        public const string Tanimlamalar = "Tanımlamalar";

        [MenuAttribute(MenuID = 100000, MenuAdi = "Sistem", MenuCssClass = "fa fa-desktop", MenuUrl = "", DilCeviriYap = false, SiraNo = 30)]
        public const string Sistem = "Sistem";
    }
    public class RoleNames : IRoleName, IMenu
    {

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Süreç İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "SurecIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Süreç İşlemleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string SurecIslemleri = "Süreç İşlemleri";
        [RoleAttribute(GorunurAdi = "Süreç İşlemleri Kayıt Yetkisi", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string SurecIslemleriKayitYetkisi = "Süreç İşlemleri Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Veri Girişi", MenuCssClass = "fa fa-file-text-o", MenuUrl = "VeriGirisi/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Veri Girişi", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string VeriGirisi = "Veri Girişi";
        [RoleAttribute(GorunurAdi = "Veri Girişi Kayıt Yetkisi", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string VeriGirisiKayitYetkisi = "Veri Girişi Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Belge Mahiyet Tipleri", MenuCssClass = "fa fa-gear", MenuUrl = "BelgeMahiyetTipleri/Index", DilCeviriYap = false, SiraNo = 7)]
        [RoleAttribute(GorunurAdi = "Belge Mahiyet Tipleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string BelgeMahiyetTipleri = "Belge Mahiyet Tipleri";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Belge Türleri", MenuCssClass = "fa fa-gear", MenuUrl = "BelgeTurleri/Index", DilCeviriYap = false, SiraNo = 10)]
        [RoleAttribute(GorunurAdi = "Belge Türleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string BelgeTurleri = "Belge Türleri";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "İşten Çıkış Nedenleri", MenuCssClass = "fa fa-gear", MenuUrl = "IstenCikisNedenleri/Index", DilCeviriYap = false, SiraNo = 13)]
        [RoleAttribute(GorunurAdi = "İşten Çıkış Nedenleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string IstenCikisNedenleri = "İşten Çıkış Nedenleri";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Eksik Gün Nedenleri", MenuCssClass = "fa fa-gear", MenuUrl = "EksikGunNedenleri/Index", DilCeviriYap = false, SiraNo = 16)]
        [RoleAttribute(GorunurAdi = "Eksik Gün Nedenleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string EksikGunNedenleri = "Eksik Gün Nedenleri";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Meslek Türleri", MenuCssClass = "fa fa-gear", MenuUrl = "MeslekTurleri/Index", DilCeviriYap = false, SiraNo = 19)]
        [RoleAttribute(GorunurAdi = "Meslek Türleri", Kategori = "İşçi İşlemleri", Aciklama = "")]
        public const string MeslekTurleri = "Meslek Türleri";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Yevmiyeler", MenuCssClass = "fa fa-file-text-o", MenuUrl = "Yevmiyeler/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Yevmiyeler", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string Yevmiyeler = "YevmiyelerListesi";
        [RoleAttribute(GorunurAdi = "Yevmiyeler Kayıt Yetkisi", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyelerKayitYetkisi = "YevmiyelerKayıtYetkisi";
        [RoleAttribute(GorunurAdi = "Yevmiyeler Excel Yükleme", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyelerExcelYukleme = "YevmiyelerExcelYukleme";
        [RoleAttribute(GorunurAdi = "Yevmiye Silme Yevmiye No ile", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyelerYevmiyeSilmeYevmiyeNoile = "YevmiyelerYevmiyeSilmeYevmiyeNoile";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "1003B Ssk Primleri", MenuCssClass = "fa fa-file-text-o", MenuUrl = "Yevmiye1003BSskPrimleri/Index", DilCeviriYap = false, SiraNo = 2)]
        [RoleAttribute(GorunurAdi = "Yevmiye - 1003B Ssk Primleri", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string Yevmiyeler1003BSskPrimleri = "Yevmiyeler1003BSskPrimleri";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "1003A Muhtasar Dökümü", MenuCssClass = "fa fa-file-text-o", MenuUrl = "Yevmiye1003AMuhatasarDokumu/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Yevmiye - 1003A Muhtasar Dökümü", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string Yevmiyeler1003AMuhtasarDokumu = "Yevmiyeler1003AMuhtasarDokumu"; 

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Kdv Tevkifat Dökümü", MenuCssClass = "fa fa-file-text-o", MenuUrl = "YevmiyeKdvTevkifatiDokumu/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Yevmiye - Kdv Tevkifat Dökümü", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyelerKdvTevkifatDokumu = "YevmiyelerKdvTevkifatDokumu"; 

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "E.Kesenek Toplamları", MenuCssClass = "fa fa-file-text-o", MenuUrl = "YevmiyeEmekliKesenekToplamlari/Index", DilCeviriYap = false, SiraNo = 5)]
        [RoleAttribute(GorunurAdi = "Yevmiye - E.Kesenek Toplamları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyelerEmekliKesenekToplamlari = "YevmiyelerEmekliKesenekToplamlari"; 

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Taşınır Kontrol Dökümü", MenuCssClass = "fa fa-file-text-o", MenuUrl = "YevmiyeTasinirKontrolDokumu/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Yevmiye - Taşınır Kontrol Dökümü", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string  YevmiyelerTasinirKontrolDokumu = "YevmiyelerTasinirKontrolDokumu"; 


        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Sendika Toplamları", MenuCssClass = "fa fa-file-text-o", MenuUrl = "YevmiyeSendikaToplamlari/Index", DilCeviriYap = false, SiraNo = 9)]
        [RoleAttribute(GorunurAdi = "Yevmiye - Sendika Toplamları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeSendikaToplamlari = "YevmiyeSendikaToplamlari";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Bes Toplamları", MenuCssClass = "fa fa-file-text-o", MenuUrl = "YevmiyeBesToplamlari/Index", DilCeviriYap = false, SiraNo = 13)]
        [RoleAttribute(GorunurAdi = "Yevmiye - Bes Toplamları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeBesToplamlari = "YevmiyeBesToplamlari";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Harcama Birimleri", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeHarcamaBirimleri/Index", DilCeviriYap = false, SiraNo = 17)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Harcama Birimleri", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeHarcamaBirimleri = "YevmiyeHarcamaBirimleri";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Hesap Kodu Eşleştirme", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeHesapKoduEslestirme/Index", DilCeviriYap = false, SiraNo = 21)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Hesap Kodu Eşleştirme", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeHesapKoduEslestirme = "HesapKoduEşleştirme";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Belge Kodları", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeBelgeKodlari/Index", DilCeviriYap = false, SiraNo = 25)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Belge Kodları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeBegleKodlari = "BelgeKodlari";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "KDV Kodları", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeKdvKodlari/Index", DilCeviriYap = false, SiraNo = 29)]
        [RoleAttribute(GorunurAdi = "Tanımlama - KDV Kodları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeKDVKodlari = "KDVKodlari";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Proje Banka Hesapları", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeProjeBankaHesaplari/Index", DilCeviriYap = false, SiraNo = 33)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Proje Banka Hesapları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeProjeBankaHesaplari = "ProjeBankaHesapları";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Sendika Bilgileri", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeSendikaBilgileri/Index", DilCeviriYap = false, SiraNo = 27)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Sendika Bilgileri", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeSendikaBilgileri = "YevmiyeSendikaBilgileri";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "Vergi Kimlik Numaraları", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeVergiKimlikNumaralari/Index", DilCeviriYap = false, SiraNo = 41)]
        [RoleAttribute(GorunurAdi = "Tanımlama - Vergi Kimlik Numaraları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeVergiKimlikNumaralari = "YevmiyeVergiKimlikNumaralari";

        [MenuAttribute(BagliMenuID = 82000, MenuAdi = "BES Banka Hesap Numaraları", MenuCssClass = "fa fa-gear", MenuUrl = "YevmiyeBesBankaHesapNumaralari/Index", DilCeviriYap = false, SiraNo = 45)]
        [RoleAttribute(GorunurAdi = "Tanımlama - BES Banka Hesap Numaraları", Kategori = "Yevmiye İşlemleri", Aciklama = "")]
        public const string YevmiyeBESBankaNumaralari = "YevmiyeBESBankaNumaralari";




        [MenuAttribute(BagliMenuID = 84000, MenuAdi = "Birimlere Göre Toplamsal", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprBirimToplamsal/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Birimlere Göre Toplamsal", Kategori = "Rapor İşlemleri", Aciklama = "")]
        public const string BirimToplamsalRapor = "Birimlere Göre Toplamsal";
        [MenuAttribute(BagliMenuID = 84000, MenuAdi = "Birim Veri Çıktılarını Al", MenuCssClass = "fa fa-list-alt", MenuUrl = "RprBirimExcelListExport/Index", DilCeviriYap = false, SiraNo = 2)]
        [RoleAttribute(GorunurAdi = "Birim Veri Çıktılarını Al", Kategori = "Rapor İşlemleri", Aciklama = "")]
        public const string BirimVeriCiktilariniAl = "Birim Veri Çıktılarını Al";

        [MenuAttribute(BagliMenuID = 84000, MenuAdi = "Bildirgeye Göre Toplamsal", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprBildirgeToplamsal/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Bildirgeye Göre Toplamsal", Kategori = "Rapor İşlemleri", Aciklama = "")]
        public const string BildirgeToplamsalRapor = "Bildirgeye Göre Toplamsal";


        [MenuAttribute(BagliMenuID = 84500, MenuAdi = "Kullanıcılar", MenuCssClass = "fa fa-user-o", MenuUrl = "Kullanicilar/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Kullanıcılar", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string Kullanicilar = "Kullanıcılar Listesi";
        [RoleAttribute(GorunurAdi = "Kullanıcılar Kayıt", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string KullaniciKayit = "Kullanıcı Kayıt";

        [MenuAttribute(BagliMenuID = 84500, MenuAdi = "Yetki Grupları", MenuCssClass = "fa fa-lock", MenuUrl = "YetkiGruplari/Index", DilCeviriYap = false, SiraNo = 2)]
        [RoleAttribute(GorunurAdi = "Yetki Grupları", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string YetkiGruplari = "Yetki Grupları";


        [MenuAttribute(BagliMenuID = 85000, MenuAdi = "Birimler", MenuCssClass = "fa fa-home", MenuUrl = "Birimler/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Birimler", Kategori = "Tanımlamalar", Aciklama = "")]
        public const string Birimler = "Birimler";

        [MenuAttribute(BagliMenuID = 85000, MenuAdi = "Ünvanlar", MenuCssClass = "fa fa-list-alt", MenuUrl = "Unvanlar/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Ünvanlar", Kategori = "Tanımlamalar", Aciklama = "")]
        public const string Unvanlar = "Ünvanlar";






        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail Şablonları", MenuCssClass = "fa fa-pencil", MenuUrl = "MailSablonlari/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Mail Şablonları", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlari = "Mail Şablonları";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail Şablonları (Sistem)", MenuCssClass = "fa fa-gear", MenuUrl = "MailSablonlariSistem/Index", DilCeviriYap = false, SiraNo = 6)]
        [RoleAttribute(GorunurAdi = "Mail Şablonları (Sistem)", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlariSistem = "Mail Şablonları (Sistem)";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail İşlemleri", MenuCssClass = "fa fa-envelope", MenuUrl = "MailIslemleri/Index", DilCeviriYap = false, SiraNo = 9)]
        [RoleAttribute(GorunurAdi = "Mail İşlemleri", Kategori = "Sistem", Aciklama = "")]
        public const string MailIslemleri = "Mail İşlemleri";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mesaj Kategorileri", MenuCssClass = "fa fa-pencil", MenuUrl = "MesajKategorileri/Index", DilCeviriYap = false, SiraNo = 12)]
        [RoleAttribute(GorunurAdi = "Mesaj Kategorileri", Kategori = "Sistem", Aciklama = "")]
        public const string MesajlarKategorileri = "Mesaj Kategorileri";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Gelen Mesajlar", MenuCssClass = "fa fa-envelope", MenuUrl = "Mesajlar/Index", DilCeviriYap = false, SiraNo = 15)]
        [RoleAttribute(GorunurAdi = "Gelen Mesajlar", Kategori = "Sistem", Aciklama = "")]
        public const string Mesajlar = "Gelen Mesajlar";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Duyurular", MenuCssClass = "fa fa-bullhorn", MenuUrl = "Duyurular/Index", DilCeviriYap = false, SiraNo = 18)]
        [RoleAttribute(GorunurAdi = "Duyurular", Kategori = "Sistem", Aciklama = "")]
        public const string Duyurular = "Duyurular";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Sistem Ayarları", MenuCssClass = "fa fa-puzzle-piece", MenuUrl = "SistemAyarlari/Index", DilCeviriYap = false, SiraNo = 21)]
        [RoleAttribute(GorunurAdi = "Sistem Ayarları", Kategori = "Sistem", Aciklama = "")]
        public const string SistemAyarlari = "Sistem Ayarları";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Sistem Bilgilendirme", MenuCssClass = "fa fa-info", MenuUrl = "SistemBilgilendirme/Index", DilCeviriYap = false, SiraNo = 24)]
        [RoleAttribute(GorunurAdi = "Sistem Bilgilendirme", Kategori = "Sistem", Aciklama = "")]
        public const string SistemBilgilendirme = "Sistem Bilgilendirme";


    }
}