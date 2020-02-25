using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class CitilinkCategory
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public uint ParseStatus { get; set; }
        public DateTime LastUpdateDt { get; set; }
        public uint PagesNumInCategory { get; set; }
        public uint LastParsedPage { get; set; }
    }
}
