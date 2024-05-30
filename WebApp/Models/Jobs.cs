using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Quartz;

namespace WebApp.Models
{
    public class Jobs : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var nowdate = DateTime.Now;
                var geciciDosyaYolu = SistemAyar.GeciciDosyaYolu.getAyar("TempDocumentFolder");
                var geciciDosyaOlusturulduktanKacGunSonraSilinsin = SistemAyar.GeciciDosyaOlusturulduktanKacGunSonraSilinsin.getAyar("3").ToIntObj().Value;
                var projectPath = HostingEnvironment.ApplicationPhysicalPath;
                var folderPath = projectPath + geciciDosyaYolu;

                string[] filePaths = Directory.GetFiles(folderPath); 
                Management.SistemBilgisiKaydet("Dosya temizleme işlemi başlatıldı. \r\n Kontrol edilecek dosyalar:\r\n" + string.Join("\r\n", filePaths), "Jobs", BilgiTipi.Bilgi);
                List<string> silinenler = new List<string>();
                foreach (var path in filePaths)
                {
                    FileInfo fi = new FileInfo(path);
                    try
                    {

                        var created = fi.CreationTime;
                        if (created.AddDays(geciciDosyaOlusturulduktanKacGunSonraSilinsin).Date <= nowdate.Date)
                        {
                            File.Delete(path);
                            silinenler.Add(created.ToString("dd.MM.yyyy - HH:mm") + " " + fi.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Management.SistemBilgisiKaydet("Geçici Dosya silinirken bir hata oluştu!\r\n Dosya: " + fi.Name + "\r\n hata:" + e.ToExceptionMessage(), e.ToExceptionStackTrace(), BilgiTipi.Hata);
                    }
                }
                Management.SistemBilgisiKaydet("Dosya temizleme işlemi tamamlandı. \r\n Silinen dosyalar:\r\n" + string.Join("\r\n", silinenler), "Jobs", BilgiTipi.Bilgi);

            }
            catch (Exception e)
            {
                Management.SistemBilgisiKaydet("Dosya temizleme zamanlayıcısı çalıştırılırken bir hata oluştu! hata:" + e.ToExceptionMessage(), e.ToExceptionStackTrace(), BilgiTipi.Hata);
            }

            return Task.CompletedTask;
        }
        public static ITrigger TriggerGeciciDosyaTemizleme()
        {
            var geciciDosyaTemizlemeSaati = SistemAyar.GeciciDosyaTemizlemeSaati.getAyar("15").Split(':').Select(s => s.ToIntObj().Value).ToList();
            var saat = geciciDosyaTemizlemeSaati[0] < 0 || geciciDosyaTemizlemeSaati[0] > 24 ? 15 : geciciDosyaTemizlemeSaati[0];
            var dk = geciciDosyaTemizlemeSaati.Count > 1 ? (geciciDosyaTemizlemeSaati[1] < 0 || geciciDosyaTemizlemeSaati[1] > 59 ? 0 : geciciDosyaTemizlemeSaati[1]) : 0;
            var trigger = TriggerBuilder.Create().WithIdentity("GeciciDosyaTemizlemeCron")
                                                 .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(saat, dk))
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