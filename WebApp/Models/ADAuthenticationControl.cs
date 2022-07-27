using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;


namespace WebApp.Models
{
    public class ADAuthenticationControl
    {

        public static string IsAuthenticated(string domain, string username, string pwd)
        {
            string retVal = "";
            try
            {
                var de = new DirectoryEntry(domain, username, pwd); 
                if (de != null)
                {
                    retVal = true.ToString();
                }
                else
                {
                    retVal = "Kullanıcı Adı Yada Şifre Hatalı";
                }
            }
            catch (Exception ex)
            {
                retVal = "Hata:" + ex.ToExceptionMessage();
            }
            return retVal;
        }
    }
}