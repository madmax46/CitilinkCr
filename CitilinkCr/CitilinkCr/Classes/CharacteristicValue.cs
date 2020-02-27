using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitilinkCr.Classes
{
    public class CharacteristicValue
    {

        public uint ProductId { get; set; }
        public Characteristic Characteristic { get; set; }

        public string Value { get; set; }

    }
}
