using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebService
{
    /// <summary>
    /// Summary description for wsDis
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class wsDis : System.Web.Services.WebService
    {

        [WebMethod]
        public LoginResult Login(Auth auth)
        {
            LoginResult lr = new LoginResult();
            var vl = auth.Validate();
            if (vl != WsResults.OK)
            {
                lr = new LoginResult(vl);
            }
            else
            {
                lr = new LoginResult(vl);
            }
            return lr;
        }
        [WebMethod]
        public xResult XMethod(Auth auth)
        {
            if (auth.Validate() != WsResults.OK)
            {
                return new xResult(auth.Validate());
            }
            xResult r = new xResult();

            return r;
        }
    }
}
