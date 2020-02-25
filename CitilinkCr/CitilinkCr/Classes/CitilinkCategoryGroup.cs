using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class CitilinkCategoryGroup
    {
        public string CategoryName { get; set; }
        public HrefTag CategoryHref { get; set; }
        public List<HrefTag> Categories { get; set; }


        public CitilinkCategoryGroup(string categoryName, HrefTag categoryHref)
        {
            CategoryName = categoryName;
            CategoryHref = categoryHref;
            Categories = new List<HrefTag>();
        }

    }
}
