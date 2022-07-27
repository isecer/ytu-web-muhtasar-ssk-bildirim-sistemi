using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiskaUtil
{
    public class NameValue
    {
        public NameValue()
        {
        }

        public NameValue(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        public NameValue(string name, string value,bool ischecked)
        {
            this.Name = name;
            this.Value = value;
            this.Checked = ischecked;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
    }
}