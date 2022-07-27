using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using BiskaUtil;
using System.Threading;
using System.IO;
using System.Web.Hosting;

namespace WebApp.Models
{
    public class ApplicationClock
    {
        void tick(object _)
        {
            lock (tickLock)
            {
                try
                {
                    var nowdate = DateTime.Now; 
                    if (nowdate.Hour >= 12 && nowdate.Hour <= 16)
                    {
                        var ProjectPath = HostingEnvironment.ApplicationPhysicalPath;
                        var folderPath = ProjectPath + "TempDocumentFolder";

                        string[] filePaths = Directory.GetFiles(folderPath);
                        Management.SistemBilgisiKaydet("Dosya temizleme zamanlayıcısı başlatıldı.\r\n Kontrol edilecek dosya yolu:" + folderPath, "AplicationClock", BilgiTipi.Bilgi);
                        Management.SistemBilgisiKaydet("Dosya temizleme zamanlayıcısı başlatıldı. \r\n Kontrol edilecek dosyalar:\r\n" + string.Join("\r\n", filePaths), "AplicationClock", BilgiTipi.Bilgi);
                        foreach (var path in filePaths)
                        {
                            FileInfo fi = new FileInfo(path);
                            var created = fi.CreationTime;
                            if (created.AddDays(3).Date <= nowdate.Date)
                                System.IO.File.Delete(path);
                        }
                    }

                }
                catch (Exception e)
                {
                    Management.SistemBilgisiKaydet("Dosya temizleme zamanlayıcısı çalıştırılırken bir hata oluştu! hata:" + e.ToExceptionMessage(), e.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
        }
        readonly Object tickLock = new Object();
        Timer ticker = null;
        public ApplicationClock Start()
        {
            ticker = new Timer(tick, null, 0, 60 * 60 * 1000);
            return this;
        }
    }
}