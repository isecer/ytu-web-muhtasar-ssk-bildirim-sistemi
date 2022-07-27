using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebService
{
    [Serializable]
    public class Auth
    {
        public string Uid { get; set; }
        public string Pwd { get; set; }

        WsResult w = null;
        public WsResult Validate(bool force=false)
        {
             
            if (w == null || force)
            {
                w = new WsResult();
                Database.MusskDBEntities db = new Database.MusskDBEntities(); 
                return w;
            }
            return w;
        }
    }
}