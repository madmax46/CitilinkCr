using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class CitilinkProduct
    {
        public uint Id { get; set; }
        public string Link { get; set; }
        public uint CitilinkProductId { get; set; }

        public uint CategoryProduct { get; set; }
        public string Name { get; set; }
    
        /// <summary>
        /// 1 в процессе, 2 - загружено, 3 - ошибка 
        /// </summary>
        public uint Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
