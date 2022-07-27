using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using WebApp.Models;

namespace WebApp.Jobs
{
    public class Jobs : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var nowdate = DateTime.Now;
                var _geciciDosyaYolu = SistemAyar.GeciciDosyaYolu.getAyar("TempDocumentFolder").ToString();
                var _geciciDosyaOlusturulduktanKacGunSonraSilinsin = SistemAyar.GeciciDosyaOlusturulduktanKacGunSonraSilinsin.getAyar("3").ToIntObj().Value;
                var ProjectPath = HostingEnvironment.ApplicationPhysicalPath;
                var folderPath = ProjectPath + _geciciDosyaYolu;

                string[] filePaths = Directory.GetFiles(folderPath); 
                Management.SistemBilgisiKaydet("Dosya temizleme işlemi başlatıldı. \r\n Kontrol edilecek dosyalar:\r\n" + string.Join("\r\n", filePaths), "Jobs", BilgiTipi.Bilgi);
                List<string> Silinenler = new List<string>();
                foreach (var path in filePaths)
                {
                    FileInfo fi = new FileInfo(path);
                    try
                    {

                        var created = fi.CreationTime;
                        if (created.AddDays(_geciciDosyaOlusturulduktanKacGunSonraSilinsin).Date <= nowdate.Date)
                        {
                            System.IO.File.Delete(path);
                            Silinenler.Add(created.ToString("dd.MM.yyyy - HH:mm") + " " + fi.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Management.SistemBilgisiKaydet("Geçici Dosya silinirken bir hata oluştu!\r\n Dosya: " + fi.Name + "\r\n hata:" + e.ToExceptionMessage(), e.ToExceptionStackTrace(), BilgiTipi.Hata);
                    }
                }
                Management.SistemBilgisiKaydet("Dosya temizleme işlemi tamamlandı. \r\n Silinen dosyalar:\r\n" + string.Join("\r\n", Silinenler), "Jobs", BilgiTipi.Bilgi);

            }
            catch (Exception e)
            {
                Management.SistemBilgisiKaydet("Dosya temizleme zamanlayıcısı çalıştırılırken bir hata oluştu! hata:" + e.ToExceptionMessage(), e.ToExceptionStackTrace(), BilgiTipi.Hata);
            }
        }
        public static ITrigger TriggerGeciciDosyaTemizleme()
        {
            var geciciDosyaTemizlemeSaati = SistemAyar.GeciciDosyaTemizlemeSaati.getAyar("15").Split(':').Select(s => s.ToIntObj().Value).ToList();
            var _saat = geciciDosyaTemizlemeSaati[0] < 0 || geciciDosyaTemizlemeSaati[0] > 24 ? 15 : geciciDosyaTemizlemeSaati[0];
            var _dk = geciciDosyaTemizlemeSaati.Count > 1 ? (geciciDosyaTemizlemeSaati[1] < 0 || geciciDosyaTemizlemeSaati[1] > 59 ? 0 : geciciDosyaTemizlemeSaati[1]) : 0;
            var trigger = TriggerBuilder.Create().WithIdentity("GeciciDosyaTemizlemeCron")
                                                 .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(_saat, _dk))
                                                 .StartNow()
                                                 .Build();
            return trigger;
        }

        public static IJobDetail JobDetailGeciciDosyaTemizleme()
        {
            var job = JobBuilder.Create<Jobs>()
                .WithIdentity("GeciciDosyaTemizlemeJob")
                .Build();

            return job;
        }
    }
}