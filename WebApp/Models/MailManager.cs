using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

using BiskaUtil;
using Database;

namespace WebApp.Models
{
    public class mdlMailMainContent
    {
        public string LogoPath { get; set; }
        public string UniversiteAdi { get; set; }
        public string Content { get; set; }

    }
    public class mailTableContent
    {
        public string AciklamaBasligi { get; set; }
        public string AciklamaDetayi { get; set; }
        public bool AciklamaTextAlingCenter { get; set; }
        public string GrupBasligi { get; set; }
        public int CaptTdWidth { get; set; }
        public List<mailTableRow> Detaylar { get; set; }
        public bool Success { get; set; }
        public mailTableContent()
        {
            CaptTdWidth = 200;
            Detaylar = new List<mailTableRow>();
            AciklamaTextAlingCenter = false;
        }

    }
    public class mailTableRow
    {
        public bool Colspan2 { get; set; }
        public int SiraNo { get; set; }
        public string Baslik { get; set; }
        public string Aciklama { get; set; }
        public mailTableRow()
        {
            Colspan2 = false;
        }
    }
    public static class MailManager
    {
        public static void sendMail(int GonderilenMailID, string Konu, string Icerik, List<string> Email, List<Attachment> attach, bool ToOrBcc = true)
        {

            #region sendMail
            var uid = UserIdentity.Current.Id;
            var uIp = UserIdentity.Ip;
            new System.Threading.Thread(() =>
            {
                var UserID = uid;
                var Ip = uIp;
                try
                {
                    using (var dbb = new MusskDBEntities())
                    {
                        var qeklenen = dbb.GonderilenMaillers.Where(p => p.GonderilenMailID == GonderilenMailID).First();
                        try
                        {
                            MailManager.sendMail(Konu, Icerik, Email, attach, ToOrBcc);
                            qeklenen.Gonderildi = true;
                            qeklenen.IslemTarihi = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            qeklenen.HataMesaji = ex.ToExceptionMessage();
                            Management.SistemBilgisiKaydet("Mail gönderim işlemi yapılamadı! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, uid, uIp);
                        }
                        dbb.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Management.SistemBilgisiKaydet("Mail gönderim işlemi sırasında bir hata oluştu! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, uid, uIp);
                }
            }).Start();

            #endregion
        }
        public static Exception sendMailRetVal(string Konu, string Icerik, string Email, List<Attachment> attach, bool ToOrBcc = true)
        {
            Exception exRet = null;

            try
            {
                MailManager.sendMail(Konu, Icerik, Email, attach, ToOrBcc);

            }
            catch (Exception ex)
            {
                exRet = ex;
            }

            return exRet;
        }
        public static Exception sendMailRetVal(string Konu, string Icerik, List<string> Email, List<Attachment> attach, bool ToOrBcc = true)
        {
            Exception exRet = null;

            try
            {
                MailManager.sendMail(Konu, Icerik, Email, attach, ToOrBcc);

            }
            catch (Exception ex)
            {
                exRet = ex;
            }

            return exRet;
        }
        public static bool sendMail(string Baslik, string Body, List<string> GonderilecekMailAdresleri, List<Attachment> attach, bool ToOrBcc)
        {

            var EmailAdresi = SistemAyar.getAyar(SistemAyar.AyarSMTP_Mail);
            var Name = SistemAyar.getAyar(SistemAyar.AyarSMTP_User);
            var Sifre = SistemAyar.getAyar(SistemAyar.AyarSMTP_Password);
            var Port = SistemAyar.getAyar(SistemAyar.AyarSMTP_Port);
            var Host = SistemAyar.getAyar(SistemAyar.AyarSMTP_Host);
            var SSL = SistemAyar.getAyar(SistemAyar.AyarSMTP_SSL);

            using (var ePosta = new MailMessage())
            {
                ePosta.From = new MailAddress(EmailAdresi, Name, System.Text.Encoding.UTF8);
                ePosta.IsBodyHtml = true;
                if (GonderilecekMailAdresleri.Count == 1 || ToOrBcc)
                    foreach (var item in GonderilecekMailAdresleri)
                        ePosta.To.Add(item);
                else
                    foreach (var item in GonderilecekMailAdresleri)
                        ePosta.Bcc.Add(item);
                ePosta.Subject = Baslik;
                ePosta.Body = Body;
                ePosta.BodyEncoding = System.Text.Encoding.UTF8;
                ePosta.Priority = MailPriority.High;
                if (attach != null)
                    foreach (var item in attach)
                        ePosta.Attachments.Add(item);
                var smtp = new SmtpClient();
                smtp.Credentials = new System.Net.NetworkCredential(EmailAdresi, Sifre);
                smtp.Port = Port.ToInt().Value;
                smtp.Host = Host;
                smtp.EnableSsl = SSL.ToBoolean(false);
                smtp.Timeout = 5 * 60 * 1000;
                smtp.Send(ePosta);

            }
            return true;


        }

  public static bool sendMail(string Baslik, string Body, string GonderilecekMailAdresi, List<Attachment> attach, bool ToOrBcc = true)
        {
            var EmailAdresi = SistemAyar.getAyar(SistemAyar.AyarSMTP_Mail);
            var Name = SistemAyar.getAyar(SistemAyar.AyarSMTP_User);
            var Sifre = SistemAyar.getAyar(SistemAyar.AyarSMTP_Password);
            var Port = SistemAyar.getAyar(SistemAyar.AyarSMTP_Port);
            var Host = SistemAyar.getAyar(SistemAyar.AyarSMTP_Host);
            var SSL = SistemAyar.getAyar(SistemAyar.AyarSMTP_SSL);

            using (var ePosta = new MailMessage())
            {
                ePosta.From = new MailAddress(EmailAdresi, Name, System.Text.Encoding.UTF8);
                ePosta.IsBodyHtml = true;
                ePosta.To.Add(GonderilecekMailAdresi);

                ePosta.Subject = Baslik;
                ePosta.Body = Body;
                ePosta.BodyEncoding = System.Text.Encoding.UTF8;
                ePosta.Priority = MailPriority.High;


                if (attach != null)
                    foreach (var item in attach)
                        ePosta.Attachments.Add(item);
                var smtp = new SmtpClient();
                smtp.Credentials = new System.Net.NetworkCredential(EmailAdresi, Sifre);
                smtp.Port = Port.ToInt().Value;
                smtp.Host = Host;
                smtp.EnableSsl = SSL.ToBoolean(false);
                smtp.Timeout = 5 * 60 * 1000;
                smtp.Send(ePosta);
            }
            return true;



        }
      
        public static Exception YeniHesapMailGonder(Kullanicilar kModel, string sfr)
        {

            using (var db = new MusskDBEntities())
            {
                var _ea = SistemAyar.getAyar(SistemAyar.AyarSistemErisimAdresi);

                var mRowModel = new List<mailTableRow>();

                var WurlAddr = _ea.Split('/').ToList();
                if (_ea.Contains("//"))
                    _ea = WurlAddr[0] + "//" + WurlAddr.Skip(2).Take(1).First();
                else
                    _ea = "http://" + WurlAddr.First();
                mRowModel.Add(new mailTableRow { Baslik = "Ad Soyad", Aciklama = kModel.Ad + " " + kModel.Soyad });

                //if (kModel.BirimKod.IsNullOrWhiteSpace() == false)
                //{
                //    var birim = db.Birimlers.Where(p => p.BirimKod == kModel.BirimKod).First();
                //    mRowModel.Add(new mailTableRow { Baslik = "Birim Adı", Aciklama = birim.BirimAdi });
                //}
                //if (kModel.UnvanID.HasValue)
                //{
                //    var unvan = db.Unvanlars.Where(p => p.UnvanID == kModel.UnvanID).First();
                //    mRowModel.Add(new mailTableRow { Baslik = "Ünvan", Aciklama = unvan.UnvanAdi });
                //}
                if (kModel.Tel.IsNullOrWhiteSpace() == false) mRowModel.Add(new mailTableRow { Baslik = "Cep Tel", Aciklama = kModel.Tel });

                mRowModel.Add(new mailTableRow { Baslik = "Kullanıcı Adı", Aciklama = kModel.KullaniciAdi });
                mRowModel.Add(new mailTableRow { Baslik = "Şifre", Aciklama = kModel.IsActiveDirectoryUser ? "Yıldız Email şifreniz ile aynı" : sfr });
                mRowModel.Add(new mailTableRow { Baslik = "Sisteme Erişim Adresi", Aciklama = "<a href='" + _ea + "' target='_blank'>" + _ea + "</a>" });
                var mmmC = new mdlMailMainContent();
                var mtc = new mailTableContent() { AciklamaBasligi = "Kullanıcı hesabınız oluşturuldu. Sisteme Giriş Bilgisi Aşağıdaki Gibidir.", Detaylar = mRowModel };
                var tavleContent = Management.RenderPartialView("Ajax", "getMailTableContent", mtc);
                mmmC.Content = tavleContent;
                mmmC.LogoPath = _ea + "/Content/assets/images/ytu_logo_tr.png";
                mmmC.UniversiteAdi = "YILDIZ TEKNİK ÜNİVERSİTESİ";
                string htmlMail = Management.RenderPartialView("Ajax", "getMailContent", mmmC);
                var User = SistemAyar.getAyar(SistemAyar.AyarSMTP_User);
                var snded = MailManager.sendMailRetVal(User, htmlMail, kModel.EMail, null);
                return snded;


            }
        }
    }



}