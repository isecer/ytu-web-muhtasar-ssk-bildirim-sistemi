using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService
{
    [Serializable]
    public class WsResult
    {
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        public WsResult() {
            this.StatusCode = 0;            
        }
        public WsResult(int code,string text)
        {
            this.StatusCode = code;
            this.StatusText = text;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() == typeof(WsResult)) {
                var x = (WsResult)obj;
                return x.StatusCode.Equals(this.StatusCode);
            }
            return false;
        }
        public override int GetHashCode()
        {
            //return StatusCode.GetHashCode();
            return base.GetHashCode();
        }
    }
    [Serializable]
    public static class WsResults
    {
        public static WsResult OK = new WsResult(0, "İşlem Başarılı");

        public static WsResult UserNameOrPasswordInvalid = new WsResult(1, "Kullanıcı Adı veya Şifre Hatalı");
        public static WsResult SystemError = new WsResult(-1, "Kullanıcı Adı veya Şifre Hatalı");

        static WsResults()
        {
            //implementation
        }
    }
}