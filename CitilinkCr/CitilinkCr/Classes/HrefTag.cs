using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class HrefTag
    {
        public string Href { get; set; }
        public string Name { get; set; }


        public HrefTag(string href, string name)
        {
            Href = href;
            Name = name;
        }
    }
}
