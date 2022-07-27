using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebService
{
    [Serializable]
    public class LoginResult:WsResult
    {
        public bool AuthToken{ get; set; }
        public DateTime LoginDate { get; set; }

        public LoginResult() : base() { }
        public LoginResult(int code,string text) : base(code, text) { }
        public LoginResult(WsResult w)
        {
            
            if (w != null)
            {
                this.StatusCode = w.StatusCode;
                this.StatusText = w.StatusText;
            }
        }

    }
}
