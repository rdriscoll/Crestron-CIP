// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="Crestron_CIP_Server.cs" company="AVPlus Integration Pty Ltd">
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
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Text.RegularExpressions;
    using AVPlus.sockets;

    public class Crestron_CIP_Server : SocketServer
    {
        public event EventHandler<SerialEventArgs> SetSerial;

        AUserInterfaceEvents uiHandler = new TestView2();
        List<CrestronDevice> uis = new List<CrestronDevice>();

        private bool tempBool;

        const int PORT_CIP = 41794;
        private Object _bufferLock = new Object();

        public Crestron_CIP_Server()
        {
            OnDebug(eDebugEventType.Info, "Crestron_CIP_Server created");

            uiHandler.PulseDigital  += new EventHandler<AnalogEventArgs> (uiHandler_PulseDigital);
            uiHandler.ToggleDigital += new EventHandler<DigitalEventArgs>(uiHandler_ToggleDigital);
            uiHandler.SetDigital    += new EventHandler<DigitalEventArgs>(uiHandler_SetDigital);
            uiHandler.SetAnalog     += new EventHandler<AnalogEventArgs> (uiHandler_SetAnalog);
            uiHandler.SetSerial     += new EventHandler<SerialEventArgs> (uiHandler_SetSerial);
            
            uiHandler.PulseDigitalSmartObject  += new EventHandler<AnalogSmartObjectEventArgs> (uiHandler_PulseDigitalSmartObject);
            uiHandler.ToggleDigitalSmartObject += new EventHandler<DigitalSmartObjectEventArgs>(uiHandler_ToggleDigitalSmartObject);
            uiHandler.SetDigitalSmartObject    += new EventHandler<DigitalSmartObjectEventArgs>(uiHandler_SetDigitalSmartObject);
            uiHandler.SetAnalogSmartObject     += new EventHandler<AnalogSmartObjectEventArgs> (uiHandler_SetAnalogSmartObject);
            uiHandler.SetSerialSmartObject     += new EventHandler<SerialSmartObjectEventArgs> (uiHandler_SetSerialSmartObject);

            uiHandler.Debug += new EventHandler<StringEventArgs>(uiHandler_Debug);

            uis.Add(new CrestronDevice(0x03, this));
            uis.Add(new CrestronDevice(0x04, this));
            uis.Add(new CrestronDevice(0x05, this));
        }

        protected void uiHandler_Debug(object sender, StringEventArgs e)
        {
            OnDebug(eDebugEventType.Info, e.val);
        }
        public void StartServer()
        {
            StartServer(PORT_CIP);
        }

        public void Send(CrestronDevice dev, byte[] msg)
        {
            //OnDebug(eDebugEventType.Info, "SendSocket: " + StringHelper.CreateHexPrintableString(b));
            foreach (Connection cl in dev.connections)
                if (cl.ClientSocket.Connected)
                    cl.ClientSocket.Send(msg);
        }
        public void Send(CrestronDevice dev, string msg)
        {
            Send(dev, StringHelper.GetBytes(msg));
        }

        public CrestronDevice GetCrestronDevice(byte ipid)
        {
            return uis.Find(x => x.id == ipid);
        }

        #region button feedback events from program

        public void DigitalEventIn (CrestronDevice device, ushort idx, bool val)
        {
            //OnDebug(eDebugEventType.Info, "Digital[{0}]: {1}", idx.ToString(), val.ToString()); 
            uiHandler.DigitalEventIn(device, idx, val);
            setInputSigState(device, 0, idx, val); // do this after handling so the handler can tell if the state has changed
        }
        public void AnalogEventIn  (CrestronDevice device, ushort idx, ushort val)
        {
            OnDebug(eDebugEventType.Info, "Analogue[{0}]: {1}", idx.ToString(), val.ToString());
            uiHandler.AnalogEventIn(device, idx, val);
            setInputSigState(device, 0, idx, val); // do this after handling so the handler can tell if the state has changed
        }
        public void SerialEventIn  (CrestronDevice device, ushort idx, string val)
        {
            OnDebug(eDebugEventType.Info, "Serial[{0}]: {1}", idx.ToString(), val);
            uiHandler.SerialEventIn(device, idx, val);
            setInputSigState(device, 0, idx, val); // do this after handling so the handler can tell if the state has changed
        }

        public void OnSetSerial(SerialEventArgs args)
        {
            if (SetSerial != null)
                SetSerial(this, args);
        }

        public void DigitalSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, bool val)
        {
            //OnDebug(eDebugEventType.Info, "IPID {0} SmartObject ID {1} Digital[{2}]:{3}", device.id, id, idx, val);
            uiHandler.DigitalSmartObjectEventIn(device, id, idx, val);
            setInputSigState(device, id, idx, val); // do this after handling so the handler can tell if the state has changed
        }
        public void AnalogueSmartObjectEventIn(CrestronDevice device, byte id, ushort idx, ushort val)
        {
            //OnDebug(eDebugEventType.Info, "SmartObject {0} Analogue[{1}]: {2}", id.ToString(), idx.ToString(), val.ToString());
            setInputSigState(device, id, idx, val); // do this after handling so the handler can tell if the state has changed
        }
        public void SerialSmartObjectEventIn  (CrestronDevice device, byte id, ushort idx, string val)
        {
            OnDebug(eDebugEventType.Info, "SmartObject {0} String[{1}]: {2}", id.ToString(), idx.ToString(), val);
            setInputSigState(device, id, idx, val); // do this after handling so the handler can tell if the state has changed
        }

        public void DeviceSignIn(CrestronDevice device)
        {
            OnDebug(eDebugEventType.Info, "Sign on IPID: " + device.id.ToString());
            string sMsg_ = "\x02\x00\x04\x00\x00\x00\x03"; // accept connection
            Send(device, sMsg_);
            uiHandler.DeviceSignIn(device);
        }
        public CrestronDevice RegisterConnection(Connection c, byte ipid)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            if(dev != null) // look for existing connections
                dev.id = ipid;
            else if (uis.Exists(x => x.id.Equals(ipid))) // look for existing ui
            {
                dev = uis.Find(x => x.id.Equals(ipid));
                dev.connections.Add(c);
            }
            else // create new ui
            {
                dev = new CrestronDevice(ipid, this);
                dev.connections.Add(c);
                uis.Add(dev);
            }
            return dev;
        }

        private void setInputSigState(CrestronDevice device, byte id, ushort idx, string val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.serialInputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Serial(idx, val);
                joins.serialInputs.Add(sig);
            }
            else
                sig.value = val;
        }
        private void setInputSigState(CrestronDevice device, byte id, ushort idx, ushort val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.analogInputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Analog(idx, val);
                joins.analogInputs.Add(sig);
            }
            else
                sig.value = val;
        }
        private void setInputSigState(CrestronDevice device, byte id, ushort idx, bool val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.digitalInputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Digital(idx, val);
                joins.digitalInputs.Add(sig);
            }
            else
                sig.value = val;
        }

        #endregion

        #region button press events from ui

        void uiHandler_PulseDigital (object sender, AnalogEventArgs e)
        {
            PulseDigital(e.device, e.join, e.val);
        }
        void uiHandler_SetDigital   (object sender, DigitalEventArgs e)
        {
            SendDigital(e.device, e.join, e.val);
        }
        void uiHandler_ToggleDigital(object sender, DigitalEventArgs e)
        {
            ToggleDigital(e.device,e.join);
        }
        void uiHandler_SetAnalog    (object sender, AnalogEventArgs e)
        {
            SendAnalogue(e.device, e.join, e.val);
        }
        void uiHandler_SetSerial    (object sender, SerialEventArgs e)
        {
            SendSerial(e.device, e.join, e.val);
        }

        void uiHandler_PulseDigitalSmartObject (object sender, AnalogSmartObjectEventArgs e)
        {
            PulseDigitalSmartObject(e.device, e.id, e.join, e.val);
        }
        void uiHandler_SetDigitalSmartObject   (object sender, DigitalSmartObjectEventArgs e)
        {
            SendDigitalSmartObject(e.device, e.id, e.join, e.val);
        }
        void uiHandler_ToggleDigitalSmartObject(object sender, DigitalSmartObjectEventArgs e)
        {
            ToggleDigitalSmartObject(e.device, e.id, e.join);           
        }
        void uiHandler_SetAnalogSmartObject    (object sender, AnalogSmartObjectEventArgs e)
        {
            SendAnalogSmartObject(e.device, e.id, e.join, e.val);
        }
        void uiHandler_SetSerialSmartObject    (object sender, SerialSmartObjectEventArgs e)
        {
            SendSerialSmartObject(e.device, e.id, e.join, e.val);
        }

        public void SendDigital (CrestronDevice device, ushort idx, bool val)
        {
            setOutputSigState(device, 0, idx, val);
            ushort NewIdx = (ushort)StringHelper.SetBit(idx - 1, 15, !val);
            byte[] b = { (byte)(NewIdx % 0x100), (byte)(NewIdx / 0x100) };
            string str = "\x05\x00\x06\x00\x00\x03\x00" + StringHelper.GetString(b);
            Send(device, str);
            //Debug("SendDigital: " + StringHelper.CreateHexPrintableString(str));
            // local feedback
            if (device.digitalOutputs.Where(x => x.pos == idx).Count() == 0)
                device.digitalOutputs.Add(new Digital(idx, false));
            Digital d = device.digitalOutputs.Find(x => x.pos == idx);
            d.value = val;
        }
        public void SendAnalogue(CrestronDevice device, ushort idx, ushort val)
        {
            setOutputSigState(device, 0, idx, val);
            byte idxLowByte = (byte)((idx - 1) % 0x100);
            byte idxHighByte = (byte)((idx - 1) / 0x100);
            byte LevelLowByte  = (byte)(val % 0x100);
            byte LevelHighByte = (byte)(val / 0x100);
            if (idxHighByte == 0)
            {
                byte[] b = { idxLowByte, LevelHighByte, LevelLowByte };
                string s = StringHelper.GetString(b);
                string str = "\x05\x00\x07\x00\x00\x04\x01" + s;
                //Debug("SendCrestron: " + StringHelper.CreateHexPrintableString(str));
                Send(device, str);
            }
            else
            {
                byte[] b = { idxHighByte, idxLowByte, LevelHighByte, LevelLowByte };
                string s = StringHelper.GetString(b);
                string str = "\x05\x00\x08\x00\x00\x05\x01" + s;
                Send(device, str);
            }
        }
        public void SendSerial  (CrestronDevice device, ushort idx, string val)
        {
            setOutputSigState(device, 0, idx, val);
            string str = "";
            if (val.Length < 7)
            {
                byte[] b1 = { (byte)(val.Length + 7) };
                byte[] b2 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
                byte[] b3 = StringHelper.GetBytes(val);
                str = "\x05\x00"
                    + StringHelper.GetString(b1)
                    + "\x00\x00\x09\x15"
                    + StringHelper.GetString(b2)
                    + "\x03"
                    + StringHelper.GetString(b3);
            }
            else
            {
                //\x12\x00\x4d\x00\x00\x00\x49\x34\x00\x01\x03<FONT.. //
                byte[] b1 = { (byte)(val.Length + 8) };
                byte[] b2 = { (byte)(val.Length + 4) };
                byte[] b3 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
                byte[] b4 = StringHelper.GetBytes(val);
                str = "\x12\x00"
                   + StringHelper.GetString(b1)
                   + "\x00\x00\x00"
                   + StringHelper.GetString(b2)
                   + "\x34"
                   + StringHelper.GetString(b3)
                   + "\x03"
                   + StringHelper.GetString(b4);
                  //Debug("SendSerial: " + StringHelper.CreateHexPrintableString(str.Substring(0, 11)) + StringHelper.CreateAsciiPrintableString(str.Substring(11, str.Length-11)));
            }
            Send(device, str);
        }
        public void SendSerialUnicode      (CrestronDevice device, ushort idx, string val)
        {
            var sig = device.serialOutputs.Find(x => x.pos == idx);
            if (sig == null)
                device.serialOutputs.Add(new Serial(idx, val));
            else
                sig.value = val;

            //\x12\x00\x0A\x00\x00\x00\x06\x34\x00\x00\x07\x31\x00 // "1"
            //\x12\x00\x0C\x00\x00\x00\x08\x34\x00\x00\x07\x31\x00\x31\x00\x31\x00 // "111"
            byte[] b1 = { (byte)(val.Length + 9) };
            byte[] b2 = { (byte)(val.Length + 5) };
            byte[] b3 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
            byte[] b4 = StringHelper.GetBytes(val);
            string str = "\x12\x00"
                + StringHelper.GetString(b1)
                + "\x00\x00\x00"
                + StringHelper.GetString(b2)
                + "\x34"
                + StringHelper.GetString(b3)
                + "\x07"
                + StringHelper.GetString(b4);
            Send(device, str);
        }

        public void ToggleDigital           (CrestronDevice device, ushort idx)
        {
            if (device.digitalOutputs.Where(x => x.pos == idx).Count() == 0)
                device.digitalOutputs.Add(new Digital(idx, false));
            IEnumerable<Digital> query = device.digitalOutputs.Where(x => x.pos == idx);
            foreach (Digital d in query)
                SendDigital(device, idx, !d.value);
        }
        public void PulseDigital            (CrestronDevice device, ushort idx, int msec)
        {
            SendDigital(device, idx, false);
            var pulseData = new PulseData(device, 0, idx, true);
            System.Threading.Timer pulseTimer = null;
            pulseTimer = new Timer((pulseCallback) =>
            {
                SendDigital(pulseData.device, pulseData.idx, pulseData.state);
                pulseTimer.Dispose();
            }, pulseData, msec, msec);
        }
        public void SendAnalogPercent       (CrestronDevice device, ushort idx, byte val)
        {
            SendAnalogue(device, idx, (ushort)StringHelper.ConvertRanges(val, 0, 100, 0, 0xFFFF));
        }
        public void ToggleDigitalSmartObject(CrestronDevice device, byte id, ushort idx)
        {
            OnDebug(eDebugEventType.Info, "ToggleDigitalSmartObject:{0}:{1} ",id, idx);
            CrestronJoins smartObject = device.smartObjects.Find(x => x.id == id);
            if (smartObject == null)
            {
                smartObject = new CrestronJoins(id);
                device.smartObjects.Add(smartObject);
            }
            Digital dig = smartObject.digitalOutputs.Find(x => x.pos == idx);
            if (dig == null)
            {
                dig = new Digital(idx, false);
                device.smartObjects.Add(smartObject);
            }
            SendDigitalSmartObject(device, id, idx, !dig.value);
        }
        public void PulseDigitalSmartObject (CrestronDevice device, byte id, ushort idx, int msec)
        {
            SendDigitalSmartObject(device, id, (byte)idx, false);
            var pulseData = new PulseData(device, id, idx, true);
            System.Threading.Timer pulseTimer = null;
            pulseTimer = new Timer((pulseCallback) =>
            {
                SendDigitalSmartObject(pulseData.device, pulseData.id, pulseData.idx, pulseData.state);
                pulseTimer.Dispose();
            }, pulseData, msec, msec);
        }

        public void SendSerialSmartObject   (CrestronDevice device, byte id, ushort idx, string val)
        {
            setOutputSigState(device, id, idx, val);
            byte[] bLen1 = { (byte)(val.Length + 15) };
            byte[] bLen2 = { (byte)(val.Length + 11) };
            byte[] bLen3 = { (byte)(val.Length + 4) };
            byte[] bId   = { (byte)(id) };
            byte[] bVal = StringHelper.GetBytes(val);
            byte[] bIdx = { (byte)((idx) / 0x100), (byte)((idx) % 0x100) };
            string sIdx = StringHelper.GetString(bIdx);
            string str = "\x12\x00"
                + StringHelper.GetString(bLen1)
                + "\x00\x00\x00"
                + StringHelper.GetString(bLen2)
                + "\x39\x00\x00\x00"
                + StringHelper.GetString(bId)
                + "\x00"
                + StringHelper.GetString(bLen3)
                + "\x34"
                + sIdx 
                + "\x03"
                + StringHelper.GetString(bVal);
            //Debug("SendSerialSmartObject: " + StringHelper.CreateHexPrintableString(str));
            Send(device, str);
        }
        public void SendDigitalSmartObject  (CrestronDevice device, byte id, ushort idx, bool val)
        {
            setOutputSigState(device, id, idx, val);
            tempBool = !tempBool;
            // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x00\x00 // id 3, press 1
            // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x01\x80 // id 3, release 2
            byte[] b1 = { (byte)id };
            ushort NewIdx = (ushort)StringHelper.SetBit(idx - 1, 15, tempBool);
            byte[] b2 = { (byte)(NewIdx % 0x100), (byte)(NewIdx / 0x100) };
            string str = "\x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00"
                + StringHelper.GetString(b1)
                + "\x03\x00" //"\x03\x27"
                + StringHelper.GetString(b2);
            //OnDebug(eDebugEventType.Info, "SendDigitalSmartObject: " + StringHelper.CreateHexPrintableString(str));
            Send(device, str);
        }
        public void SendAnalogSmartObject   (CrestronDevice device, byte id, ushort idx, ushort val)
        {
            setOutputSigState(device, id, idx, val);
            // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x03\x05\x14\x00\x03\x00\x03 // id 3 analog setNumItems(idx 3) val 3
            // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x05\x05\x14\x00\x0a\x00\x09 // id 5 analog setItem1IconAna(idx 11) val 9
            byte[] b1 = { (byte)id };
            ushort NewIdx = (ushort)StringHelper.SetBit(idx - 1, 15, tempBool);
            byte[] b2 = { (byte)idx };
            byte[] b3 = { (byte)(val / 0x100), (byte)(val % 0x100) };
            string str = "\x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00"
                + StringHelper.GetString(b1)
                + "\x05\x14\x00"
                + StringHelper.GetString(b2)
                + StringHelper.GetString(b3);
            Send(device, str);
        }

        private CrestronJoins getSmartObject(CrestronDevice device, byte id)
        {
            var so = device.smartObjects.Find(x => x.id == id);
            if (so == null)
            {
                so = new CrestronJoins(id);
                device.smartObjects.Add(so);
            }
            return so;
        }
        private void setOutputSigState      (CrestronDevice device, byte id, ushort idx, string val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.serialOutputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Serial(idx, val);
                joins.serialOutputs.Add(sig);
            }
            else
                sig.value = val;
        }
        private void setOutputSigState      (CrestronDevice device, byte id, ushort idx, ushort val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.analogOutputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Analog(idx, val);
                joins.analogOutputs.Add(sig);
            }
            else
                sig.value = val;
        }
        private void setOutputSigState      (CrestronDevice device, byte id, ushort idx, bool val)
        {
            CrestronJoins joins = id > 0 ? getSmartObject(device, id) : device;
            var sig = joins.digitalOutputs.Find(x => x.pos == idx);
            if (sig == null)
            {
                sig = new Digital(idx, val);
                joins.digitalOutputs.Add(sig);
            }
            else
                sig.value = val;
        }
       
        #endregion
        
        #region string_parsing

        public void ParseDeviceDetails(Connection c, byte[] b1)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            if (b1.Length > 8)
            {
                if (b1[7] == 0x30) // run time status
                {
                    string s = StringHelper.CreateAsciiPrintableString(b1.Skip(11).Take(b1.Length - 11).ToArray());
                    OnDebug(eDebugEventType.Info, "status: " + s);
                    Match m;
                    m = new Regex(@"Core3Version.(.+)").Match(s);
                    if (m.Success)
                        dev.version = m.Value;

                    m = new Regex(@"IPID=(\d+)").Match(s);
                    if (m.Success)
                    {
                        byte b = Convert.ToByte(m.Groups[1].Value);
                        CrestronDevice d = RegisterConnection(c, b);
                        DeviceSignIn(d);
                    }
                }
                else // if (b1[7] == 0x31) // device status
                {
                    OnDebug(eDebugEventType.Info, "details: " + StringHelper.CreateAsciiPrintableString(b1.Skip(16).Take(b1.Length - 16).ToArray()));
                }
                #region device detail strings
                /*
                \x05\x00\x1E\x00\x00\x1B\x03\x30\x01\x01\x16	Core 3 Ver. 2.06.04.06

                \x05\x00\x11\x00\x00\x04\x03\x31\x00\x0A\x05	\x03ENV\x03	DPI\x00
                \x05\x00\x17\x00\x00\x04\x03\x31\x00\x10\x05	\x03ENV\x09	ChromaKey\x00
                \x05\x00\x19\x00\x00\x04\x03\x31\x00\x12\x05	\x03ENV\x0B	FirmwareVer\x00
                \x05\x00\x1A\x00\x00\x04\x03\x31\x00\x13\x05	\x03ENV\x0C	FlashVersion\x00
                \x05\x00\x1A\x00\x00\x04\x03\x31\x00\x13\x05	\x03ENV\x0C	MemAvailable\x00
                \x05\x00\x24\x00\x00\x04\x03\x31\x00\x1D\x01	\x03ENV\x0C	Core3Version\x0A2.06.04.06
                \x05\x00\x1B\x00\x00\x04\x03\x31\x00\x14\x05	\x03ENV\x0D	WidthInPixels\x00
                \x05\x00\x1B\x00\x00\x04\x03\x31\x00\x14\x05	\x03ENV\x0D	WidthInInches\x00
                \x05\x00\x1C\x00\x00\x04\x03\x31\x00\x15\x05	\x03ENV\x0E	HeightInPixels\x00
                \x05\x00\x1C\x00\x00\x04\x03\x31\x00\x15\x05	\x03ENV\x0E	HeightInInches\x00
                \x05\x00\x22\x00\x00\x04\x03\x31\x00\x1B\x05	\x03ENV\x14	MultiVideoRequestVer\x00
                \x05\x00\x25\x00\x00\x04\x03\x31\x00\x1E\x05	\x03ENV\x17	SupportsIPLinkSubtype38\x00
                \x05\x00\x2A\x00\x00\x04\x03\x31\x00\x23\x05	\x03ENV\x1C	IsSeamlessSwitchingSupported\x00

                \x05\x00\x39\x00\x00\x36\x03\x30\x01\x01\x31	Splash Screen: Attempting to connect to 127.0.0.1
                \x05\x00\x42\x00\x00\x3F\x03\x30\x01\x01\x3A	Splash Screen: Sending update request to control system...
                \x05\x00\x42\x00\x00\x3F\x03\x30\x01\x01\x3A	Splash Screen: Connection to control system established...
                \x05\x00\x42\x00\x00\x3F\x03\x30\x01\x01\x3A	Splash Screen: Sending update request to control system...
                \x05\x00\x27\x00\x00\x24\x03\x30\x01\x01\x1F	Splash Screen: Loading Project.
                \x05\x00\x25\x00\x00\x22\x03\x30\x01\x01\x1D	Splash Screen: Loading page 1
                \x05\x00\x26\x00\x00\x23\x03\x30\x01\x01\x1E	Splash Screen: Loading Themes.
                \x05\x00\x26\x00\x00\x23\x03\x30\x01\x01\x1E	Splash Screen: Loading Themes.
                \x05\x00\x23\x00\x00\x20\x03\x30\x01\x01\x1B	Container loading complete.
                \x05\x00\x2A\x00\x00\x27\x03\x30\x01\x01\x22	Splash Screen: Loading Complete...
                \x05\x00\x25\x00\x00\x22\x03\x30\x01\x01\x1D	Startup parameter XPanel=true
                \x05\x00\x29\x00\x00\x26\x03\x30\x01\x01\x21	Startup parameter enableSSL=false
                \x05\x00\x20\x00\x00\x1D\x03\x30\x01\x01\x18	Startup parameter IPID=3
                \x05\x00\x28\x00\x00\x25\x03\x30\x01\x01\x20	Startup parameter Host=127.0.0.1
                \x05\x00\x24\x00\x00\x21\x03\x30\x01\x01\x1C	Startup parameter Port=41794
                \x05\x00\x77\x00\x00\x74\x03\x30\x01\x01\x6F	Background loading of pages stopped.  Remaining Unrendered Pages: 0 Memory Used:32.876MB Memory Available: 0 MB
                */
                #endregion device detail strings
            }
            else // sending details
            {
                // \x05\x00\x05\x00\x00\x02\x03\x00 // sent on connection
                OnDebug(eDebugEventType.Info, "ParseCrestron sending details: " + StringHelper.CreateHexPrintableString(b1));
            }
        }

        public void ParseSerialUnicodeEvent(Connection c, byte[] b1)
        {
            OnDebug(eDebugEventType.Info, "ParseCrestron Serial unicode: " + StringHelper.CreateHexPrintableString(b1));
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            //\x12\x00\x0A\x00\x00\x00\x06\x34\x00\x00\x07\x31\x00 // "1"
            //\x12\x00\x0C\x00\x00\x00\x08\x34\x00\x00\x07\x31\x00\x31\x00\x31\x00 // "111"
            //String str = "";
            //for (int i=11; i<b1.Length; i+=2)
            //    str = str + StringHelper.GetString(b1, i, 1);
            ushort idx = 0;
            String str = String.Empty;
            switch (b1[7])
            {
                case 0x39: // smart object
                    idx = (ushort)(b1[ 8] * 0x100 + b1[ 9] + 1);
                    byte id  = (byte)(b1[10] * 0x100 + b1[11] + 1);
                    str = Encoding.GetEncoding("ISO-8859-1").GetString(b1, 18, b1.Length - 18);
                    SerialSmartObjectEventIn(dev, id, idx, str);
                    break;
                default:
                    idx = (ushort)(b1[8] * 0x100 + b1[9] + 1);
                    str = Encoding.GetEncoding("ISO-8859-1").GetString(b1, 11, b1.Length-11);
                    SerialEventIn(dev, idx, str);
                    break;
            };
        }
        public void ParseJoinEvent(Connection c, byte[] b1)
        {
            //OnDebug(eDebugEventType.Info, "Join event");
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            switch (b1[6]) 
            {
                case 0x00:
                case 0x27: // digital
                { 
                    ushort idx = (ushort)((b1[8] & 0x7F) * 0x100 + b1[7] + 1);
                    //OnDebug(eDebugEventType.Info, "ParseCrestron Digital: " + StringHelper.CreateHexPrintableString(b1));
                    bool val = !StringHelper.GetBit(b1[8], 7);
                    DigitalEventIn(dev, idx, val);
                    break;
                }
                case 0x01:
                case 0x14:  // analogue
                {
                    //OnDebug(eDebugEventType.Info, "ParseCrestron Analogue: " + StringHelper.CreateHexPrintableString(b1));
                    ushort idx = (ushort)(b1[7] * 0x100 + b1[8] + 1);
                    ushort val = (ushort)(b1[9] * 0x100 + b1[10]);
                    AnalogEventIn(dev, idx, val);
                    break;
                }
                case 0x02: // serial type 1
                {
                    OnDebug(eDebugEventType.Info, "ParseCrestron Serial Type 1: " + StringHelper.CreateHexPrintableString(b1));
                    string str = StringHelper.GetString(b1);
                    // "$05,$00,$0F,$00,$00,$0C,$02,'#1,',$0D,'#1,aaa',$0D" = ser 1 to aaa
                    Match m = Regex.Match(str, @".*#(.*),.*#.*,(.*)\x0D");
                    if (m.Groups.Count < 2)
                    {
                        ushort idx = Convert.ToUInt16(m.Groups[1].Value);
                        string val = m.Groups[1].Value;
                        SerialEventIn(dev, idx, val);
                    }
                    break;
                }
                case 0x03: { ParseDeviceDetails(c, b1); break; } // device details
                case 0x12: // serial type 2
                {
                    OnDebug(eDebugEventType.Info, "ParseCrestron Serial Type 2: " + StringHelper.CreateHexPrintableString(b1));
                    break;
                }
                case 0x15: // serial type 3
                {
                    OnDebug(eDebugEventType.Info, "ParseCrestron Serial Type 3: " + StringHelper.CreateHexPrintableString(b1));
                    ushort idx = (ushort)(b1[7] * 0x100 + b1[8] + 1);
                    // "$05,$00,$08,$00,$00,$09,$15,$00,$00,$03,'a'"		= ser 1 to a
                    // "$05,$00,$0A,$00,$00,$09,$15,$00,$02,$03,'abc'"		= ser 3 to abc
                    string str = StringHelper.GetString(b1);
                    string val = str.Substring(10);
                    SerialEventIn(dev, idx, val);
                    break;
                }
                case 0x38: // smart object
                {
                    Byte ipid = dev != null ? dev.id : uis[0].id;
                    //OnDebug(eDebugEventType.Info, "ParseCrestron Smart object " + StringHelper.CreateHexPrintableString(b1));
                    // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x00\x00 // id 3, press 1
                    // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x03\x05\x14\x00\x03\x00\x01 // id 3 analog = 1
                    // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x00\x80 // id 3 release 1
                    // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x01\x00 // id 3, press 2
                    // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x03\x05\x14\x00\x03\x00\x02
                    // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x01\x80 // id 3 release 2
                    try
                    {
                        byte onjectId = b1[10];
                        if (b1.Length < 16) // digi, b[12]=0x27
                        {
                            ushort idx = (ushort)((b1[14] & 0x7F) * 0x100 + b1[13] + 1);
                            bool val = !StringHelper.GetBit(b1[14], 7);
                            DigitalSmartObjectEventIn(dev, onjectId, idx, val);
                            //OnDebug(eDebugEventType.Info, "IPID {0} Smart object ID {1} Digital[{2}]:{3}", ipid, onjectId, idx, val);
                        }
                        else // ana, b[12]=0x14
                        {
                            ushort idx = (ushort)0;
                            ushort val = (ushort)b1[16];
                            AnalogueSmartObjectEventIn(dev, onjectId, idx, val);
                            //OnDebug(eDebugEventType.Info, "IPID {0} Smart object ID {1} Analog[{2}]:{3}", ipid, onjectId, idx, val);
                        }
                    }
                    catch (Exception e)
                    {
                        OnDebug(eDebugEventType.Info, "Exception: " + e.ToString());
                    }
                    break;
                }
                default : 
                {
                    OnDebug(eDebugEventType.Info, "ParseCrestron Unhandled join type {0}: {1}", b1[6], StringHelper.CreateHexPrintableString(b1));
                    break;
                }
			}
        }
        public void SignOn(Connection c, byte[] b1, byte pos)
        {
            if (b1.Length > pos)
                DeviceSignIn(RegisterConnection(c, b1[pos]));
            else
                OnDebug(eDebugEventType.Info, "Sign on error: " + StringHelper.CreateHexPrintableString(b1));
        }
        public void ParseCrestronString(Connection c, byte[] b1)
        {
            //OnDebug(eDebugEventType.Info, "ParseCrestron: " + StringHelper.CreateHexPrintableString(b1));
            //string s1 = Encoding.GetEncoding("ISO-8859-1").GetString(b1);
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            Byte ipid = dev != null ? dev.id : uis[0].id;
            switch (b1[0])
            {
                case 0x00: { OnDebug(eDebugEventType.Info, "Ack"); break; }
                case 0x01: { SignOn(c, b1, 8); break; }
                case 0x02: { OnDebug(eDebugEventType.Info, "Connection accepted" + dev == null ? " IPID " + dev.id : ""); break; }
                case 0x04: { OnDebug(eDebugEventType.Info, "Connection refused"  + dev == null ? " IPID " + dev.id : ""); break; }
                case 0x05: { ParseJoinEvent(c, b1); break; }
                case 0x0A: { SignOn(c, b1, 4); break; }
                case 0x0D: { Send(c, "\x0E\x00\x02\x00\x00"); break; } //OnDebug(eDebugEventType.Info, "Ping");
                case 0x0E: { break; } //OnDebug(eDebugEventType.Info, "Pong");
                case 0x0F: { OnDebug(eDebugEventType.Info, "Query"); break; }
                case 0x12: { ParseSerialUnicodeEvent(c, b1); break; }
                default: { OnDebug(eDebugEventType.Info, "ParseCrestron Unknown: " + StringHelper.CreateHexPrintableString(b1)); break; }

            }
        }

        public override void SendAcceptMessage(Connection c)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            string s = dev == null ? "": " IPID " + dev.id.ToString();
            OnDebug(eDebugEventType.Info, "SendAcceptMessage" + s);
            string sMsg_ = "\x0F\x00\x01\x02"; // accept connection
            Send(c, sMsg_);
        }
        public override void ProcessBuffer(Connection c)
        {
            byte[] b1;
            while (true)
            {
                b1 = c.cb.GetBuffer(); // look at currentbuffer (non-destructive)
                if (b1 == null || b1.Length < 1)
                    break;
                try
                {
                    // 3rd byte says how many bytes incoming
                    if (b1.Length > 2)
                    {
                        if (b1.Length > b1[2] + 2)
                        {
                            ParseCrestronString(c, c.cb.Read(0, b1[2] + 3));
                        }
                    }
                }
                catch (Exception e)
                {
                    OnDebug(eDebugEventType.Info, "Exception: " + e.ToString());
                }
            }
        }

        #endregion
    }

    struct PulseData
    {
        public CrestronDevice device;
        public byte id;
        public ushort idx;
        public bool state;

        public PulseData(CrestronDevice device, byte id, ushort idx, bool state)
        {
            this.device = device;
            this.id = id;
            this.idx = idx;
            this.state = state;
        }
    }

}