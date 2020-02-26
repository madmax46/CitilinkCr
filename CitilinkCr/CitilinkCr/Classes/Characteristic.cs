using System;
using System.Collections.Generic;
using System.Text;

namespace CitilinkCr.Classes
{
    public class Characteristic
    {
        public uint Id { get; set; }
        public uint IdGroup { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }

        public Characteristic(string group, string name)
        {
            Group = group.Trim();
            Name = name.Trim();
        }
        public override int GetHashCode()
        {
            return Group.GetHashCode() ^ Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null) return false;
            var other = obj as Characteristic;
            return Group.Equals(other.Group) && Name.Equals(other.Name);
        }
    }
}
