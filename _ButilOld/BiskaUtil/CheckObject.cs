using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiskaUtil
{
    [Serializable()]
    public class CheckObject<T> where T : class
    {
        public bool? Checked { get; set; }
        public bool Disabled { get; set; }
        public T Value { get; set; }
    }
}