using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitilinkCr.Classes
{
    public class ProductPriceHistoryItem
    {
        public uint Id { get; set; }

        public uint ProductId { get; set; }
        public double Price { get; set; }
        public bool IsInStock { get; set; }
        public DateTime DateCheck { get; set; }
    }
}
