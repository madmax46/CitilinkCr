using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class CitilinkCategory
    {
        public string CategoryName { get; set; }
        public HrefTag CategoryHref { get; set; }
        public List<HrefTag> SubCategories { get; set; }


        public CitilinkCategory(string categoryName, HrefTag categoryHref)
        {
            CategoryName = categoryName;
            CategoryHref = categoryHref;
            SubCategories = new List<HrefTag>();
        }

    }
}
