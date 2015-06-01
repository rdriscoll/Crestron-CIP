using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace avplus
{
    public class CrestronJoins
    {
        public String name;
        public byte id { get; set; }

        public List<Digital> digitals = new List<Digital>();
        public List<Analog> analogs = new List<Analog>();
        public List<Serial> serials = new List<Serial>();

        public CrestronJoins(byte id)
        {
            this.id = id;
        }
    }
    public class Digital
    {
        public bool value;
        public ushort pos;
        public Digital(ushort pos, bool value)
        {
            this.value = value;
            this.pos = pos;
        }
    }
    public class Analog
    {
        public ushort value;
        public ushort pos;
        public Analog(ushort pos, ushort value)
        {
            this.value = value;
            this.pos = pos;
        }
    }
    public class Serial
    {
        public String value;
        public ushort pos;
        public Serial(ushort pos, string value)
        {
            this.value = value;
            this.pos = pos;
        }
   }
}
