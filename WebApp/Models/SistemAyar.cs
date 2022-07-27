using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Models
{
    public static class SistemAyar
    {
        public const string AyarSMTP_Host = "Smtp Host Adresi";
        public const string AyarSMTP_Port = "Smtp Port Adresi";
        public const string AyarSMTP_SSL = "Smtp SSL";
        public const string AyarSMTP_Mail = "Smtp Mail Adresi";
        public const string AyarSMTP_User = "Smtp Kullanıcı Adı";
        public const string AyarSMTP_Password = "Smtp Şifre";
        public const string AyarSistemErisimAdresi = "Sistem Erişim Adresi";  
        public const string KullaniciHesapKaydiKimlikDogrula = "Hesap Kaydında Kimlik Doğrulaması Yap";
        public const string KullaniciResimYolu = "Kullanıcı Resim Yolu";
        public const string KullaniciDefaultResim = "Kullanıcı Default Resim"; 

        public const string KullaniciResimKaydiBoyutlandirma = "Resim Kaydında Boyutlandırma Yap";
        public const string KullaniciResimKaydiWidthPx = "Resim Kaydı Width (Px)";
        public const string KullaniciResimKaydiHeightPx = "Resim Kaydı Height (Px)";
        public const string KullaniciResimKaydiKaliteOpt = "Resim Kaydında Kalite Optimizasyonu Yap";
        public const string RotasyonuDegisenResimleriLogla = "Rotasyonu Değişen Resimleri Logla";

        public const string WindowsServisiniAktifOlarakCalistir = "Windows Servisini Aktif Olarak Çalıştır";
        public const string GeciciDosyalarOtomatikOlarakKaldirilsin = "Geçici Dosyalar Otomatik Kaldırılsın";
        public const string GeciciDosyaYolu = "Geçici Dosya Yolu";
        public const string GeciciDosyaOlusturulduktanKacGunSonraSilinsin = "Geçici Dosyalar Oluşturulduktan Kaç Gün Sonra Silinsin"; 
        public const string GeciciDosyaTemizlemeSaati = "Geçici Dosya Temizleme Saati";

        public static void setAyar(string AyarAdi, string AyarDegeri)
        {
            using (var db = new MusskDBEntities())
            {
                var qq = db.Ayarlars.Where(p => p.AyarAdi == AyarAdi).FirstOrDefault();
                if (qq != null)
                {
                    qq.AyarDegeri = AyarDegeri;
                }
                else
                {
                    db.Ayarlars.Add(new Ayarlar { AyarAdi = AyarAdi, AyarDegeri = AyarDegeri });

                }
                db.SaveChanges();
            }

        }
        public static string getAyar(this string AyarAdi, string VarsayilanDeger = "")
        {
            using (var db = new MusskDBEntities())
            {
                var qq = db.Ayarlars.Where(p => p.AyarAdi == AyarAdi).FirstOrDefault();
                if (qq != null)
                {
                    return qq.AyarDegeri;
                }
                else
                {
                    return VarsayilanDeger;

                }
            }
        }
    }
    
}