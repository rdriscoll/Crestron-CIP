using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace avplus
{
    class UiWithMeta : CrestronDevice
    {
        List<CrestronDevice> smartGraphics = new List<CrestronDevice>();
        public UiWithMeta(byte IPID, Crestron_CIP_Server ControlSystem)
            : base(IPID)
        {

        }
    }
}
