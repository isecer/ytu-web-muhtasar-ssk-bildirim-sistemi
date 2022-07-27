using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService
{
    [Serializable]
    public class xResult:WsResult
    {
        public xResult() : base() {  }
        public xResult(int code, string text) : base(code, text) { }
        public xResult(WsResult w)
        {
            if (w != null)
            {
                this.StatusCode = w.StatusCode;
                this.StatusText = w.StatusText;
            }
        }
    }
}