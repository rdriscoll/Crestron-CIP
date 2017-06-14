// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="CrestronJoins.cs" company="AVPlus Integration Pty Ltd">
//     {c} AV Plus Pty Ltd 2017.
//     http://www.avplus.net.au
//     20170611 Rod Driscoll
//     e: rdriscoll@avplus.net.au
//     m: +61 428 969 608
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//     
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//     
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
//
//     For more details please refer to the LICENSE file located in the root folder 
//      of the project source code;
// </copyright>

namespace AVPlus.CrestronCIP
{
    using System;
    using System.Collections.Generic;

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

    public class DigitalEventArgs : EventArgs
    {
        public CrestronDevice device;
        public ushort join;
        public bool val;

        public DigitalEventArgs(CrestronDevice device, ushort join, bool val)
        {
            this.device = device;
            this.join = join;
            this.val = val;
        }
    }
    public class AnalogEventArgs : EventArgs
    {
        public CrestronDevice device;
        public ushort join;
        public ushort val;

        public AnalogEventArgs(CrestronDevice device, ushort join, ushort val)
        {
            this.device = device;
            this.join = join;
            this.val = val;
        }
    }
    public class SerialEventArgs : EventArgs
    {
        public CrestronDevice device;
        public ushort join;
        public string val;

        public SerialEventArgs(CrestronDevice device, ushort join, string val)
        {
            this.device = device;
            this.join = join;
            this.val = val;
        }
    }

    public class DigitalSmartObjectEventArgs : EventArgs
    {
        public CrestronDevice device;
        public byte id;
        public ushort join;
        public bool val;

        public DigitalSmartObjectEventArgs(CrestronDevice device, byte id, ushort join, bool val)
        {
            this.device = device;
            this.id = id;
            this.join = join;
            this.val = val;
        }
    }
    public class AnalogSmartObjectEventArgs : EventArgs
    {
        public CrestronDevice device;
        public byte id;
        public ushort join;
        public ushort val;

        public AnalogSmartObjectEventArgs(CrestronDevice device, byte id, ushort join, ushort val)
        {
            this.device = device;
            this.id = id;
            this.join = join;
            this.val = val;
        }
    }
    public class SerialSmartObjectEventArgs : EventArgs
    {
        public CrestronDevice device;
        public byte id;
        public ushort join;
        public string val;

        public SerialSmartObjectEventArgs(CrestronDevice device, byte id, ushort join, string val)
        {
            this.device = device;
            this.id = id;
            this.join = join;
            this.val = val;
        }
    }

}
