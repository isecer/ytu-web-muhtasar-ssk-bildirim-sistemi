using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;


namespace BiskaUtil
{
    class Class1
    {
        public static void xxx()
        {

            WinImpersonate wi = new WinImpersonate();
            //geriye hata dönerse yakalamak için
            Exception ex = null;
            //kontrol 
            wi.GetWindowsIdentity("username", "domain/yada bilgisayar adı", "password", out ex);
            // login olma ve program çalıştırma

            var sifre = "password".ToCharArray();       // şifre char dizisine dönüştürülür
     
            SecureString testString = new SecureString();
            foreach (var item in sifre)
            {
                //securestringe eklenir
                testString.AppendChar(item);
            }

            wi.RunAs("domain", "username", "password", () =>
            {
                System.Diagnostics.Process.Start("Dosya Yolu", "username", testString, "domain/yada bilgisayar adı");

            });

        }

    }
}
