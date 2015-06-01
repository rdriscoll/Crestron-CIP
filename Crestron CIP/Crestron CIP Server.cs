using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using avplus;
using avplus.sockets;
using System.Text.RegularExpressions;

namespace avplus
{
    class Crestron_CIP_Server : SocketServer
    {
        //public List<CrestronDevice> devices = new List<CrestronDevice>();
        public CrestronDevice ui_01;
        public CrestronDevice ui_02;
        public CrestronDevice ui_03;
        UiHandler uiHandler;
        List<CrestronDevice> uis = new List<CrestronDevice>();

        private bool tempBool;

        const int PORT_CIP = 41794;
        private Object _bufferLock = new Object();

        public Crestron_CIP_Server(ClientForm myForm)
            : base(myForm, PORT_CIP)
        {
            uiHandler = new UiHandler(this);
            Console.WriteLine("Crestron_CIP_Server created");
            ui_01 = new CrestronDevice(0x04, this);
            ui_02 = new CrestronDevice(0x05, this);
            ui_03 = new CrestronDevice(0x06, this);

            uis.Add(ui_01);
            uis.Add(ui_02);
            uis.Add(ui_03);
        }

        public void StartServer()
        {
            StartServer(PORT_CIP);
        }

        public void Send(CrestronDevice dev, string msg)
        {
            byte[] b = Encoding.Default.GetBytes(msg);
            //Debug("SendSocket: " + Utils.createHexPrintableString(b));
            foreach (Connection cl in dev.connections)
                if (cl.ClientSocket.Connected)
                    cl.ClientSocket.Send(b);
        }

        public CrestronDevice GetCrestronDevice(byte ipid)
        {
            return uis.Find(x => x.id == ipid);
        }

        #region button_events
        public void DigitalEventIn(CrestronDevice device, ushort idx, bool val)
        {
            //Debug(String.Format("Digital[{0}]: {1}", idx, val));
            //UiWithMeta ui = uis.Find(x => x.id == id);
            uiHandler.DigitalEventIn(device, idx, val);
        }
        public void AnalogueEventIn(CrestronDevice device, ushort idx, ushort val)
        {
            Debug(String.Format("Analogue[{0}]: {1}", idx, val));
        }
        public void SerialEventIn(CrestronDevice device, ushort idx, string val)
        {
            //Debug(String.Format("String[{0}]: {1}", idx, val));
            parent.Invoke(parent.serFb, new Object[] { idx, val });
        }

        public void DigitalSmartObjectEventIn(CrestronDevice device, byte smartId, ushort idx, bool val)
        {
            Debug(String.Format("IPID {0} SmartObject ID {1} Digital[{2}]:{3}", device.id, smartId, idx, val));
            uiHandler.DigitalSmartObjectEventIn(device, smartId, idx, val);
        }
        public void AnalogueSmartObjectEventIn(CrestronDevice device, byte smartId, ushort idx, ushort val)
        {
            Debug(String.Format("SmartObject {0} Analogue[{1}]: {2}", smartId, idx, val));
        }
        public void SerialSmartObjectEventIn(CrestronDevice device, byte smartId, ushort idx, string val)
        {
            Debug(String.Format("SmartObject {0} String[{1}]: {2}", smartId, idx, val));
        }

        public void DeviceSignIn(CrestronDevice device)
        {
            Debug("Sign on IPID: " + device.id.ToString());
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

        #endregion

        #region string_parsing

        public void SendDigital(CrestronDevice device, ushort idx, bool val)
        {
            ushort NewIdx = (ushort)Utils.SetBit(idx - 1, 15, !val);
            byte[] b = { (byte)(NewIdx % 0x100), (byte)(NewIdx / 0x100) };
            string str = "\x05\x00\x06\x00\x00\x03\x00" + Encoding.Default.GetString(b);
            Send(device, str);
            //Debug("SendDigital: " + Utils.createHexPrintableString(str));
            // local feedback
            if (device.digitals.Where(x => x.pos == idx).Count() == 0)
                device.digitals.Add(new Digital(idx, false));
            Digital d = device.digitals.Find(x => x.pos == idx);
            d.value = val;
        }
        public void SendAnalogue(CrestronDevice device, ushort idx, ushort val)
        {
            byte idxLowByte = (byte)((idx - 1) % 0x100);
            byte idxHighByte = (byte)((idx - 1) / 0x100);
            byte LevelLowByte  = (byte)(val % 0x100);
            byte LevelHighByte = (byte)(val / 0x100);
            if (idxHighByte == 0)
            {
                byte[] b = { idxLowByte, LevelHighByte, LevelLowByte };
                string s = Encoding.Default.GetString(b);
                string str = "\x05\x00\x07\x00\x00\x04\x01" + s;
                //Debug("SendCrestron: " + Utils.createHexPrintableString(str));
                Send(device, str);
            }
            else
            {
                byte[] b = { idxHighByte, idxLowByte, LevelHighByte, LevelLowByte };
                string s = Encoding.Default.GetString(b);
                string str = "\x05\x00\x08\x00\x00\x05\x01" + s;
                Send(device, str);
            }
        }
        public void SendSerial(CrestronDevice device, ushort idx, string val)
        {
            string str = "";
            if (val.Length < 7)
            {
                byte[] b1 = { (byte)(val.Length + 7) };
                byte[] b2 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
                byte[] b3 = Encoding.Default.GetBytes(val);
                str = "\x05\x00"
                    + Encoding.Default.GetString(b1)
                    + "\x00\x00\x09\x15"
                    + Encoding.Default.GetString(b2)
                    + "\x03"
                    + Encoding.Default.GetString(b3);
            }
            else
            {
                //\x12\x00\x4d\x00\x00\x00\x49\x34\x00\x01\x03<FONT.. //
                byte[] b1 = { (byte)(val.Length + 8) };
                byte[] b2 = { (byte)(val.Length + 4) };
                byte[] b3 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
                byte[] b4 = Encoding.Default.GetBytes(val);
                str = "\x12\x00"
                   + Encoding.Default.GetString(b1)
                   + "\x00\x00\x00"
                   + Encoding.Default.GetString(b2)
                   + "\x34"
                   + Encoding.Default.GetString(b3)
                   + "\x03"
                   + Encoding.Default.GetString(b4);
                  //Debug("SendSerial: " + Utils.createHexPrintableString(str.Substring(0, 11)) + Utils.createAsciiPrintableString(str.Substring(11, str.Length-11)));
            }
            Send(device, str);
        }

        public void ToggleDigital(CrestronDevice device, ushort idx)
        {
            if (device.digitals.Where(x => x.pos == idx).Count() == 0)
                device.digitals.Add(new Digital(idx, false));
            IEnumerable<Digital> query = device.digitals.Where(x => x.pos == idx);
            foreach (Digital d in query)
                SendDigital(device, idx, !d.value);
        }
        public void PulseDigital(CrestronDevice device, ushort idx, int msec)
        {
            SendDigital(device, idx, false);
            AddToWaitQueue(device, idx, true, msec);
        }
        public void SendAnaloguePercent(CrestronDevice device, ushort idx, byte val)
        {
            SendAnalogue(device, idx, (ushort)Utils.convertRanges(val, 0, 100, 0, 0xFFFF));
        }
        public void SendSerialUnicode(CrestronDevice device, ushort idx, string val)
        {
            //\x12\x00\x0A\x00\x00\x00\x06\x34\x00\x00\x07\x31\x00 // "1"
            //\x12\x00\x0C\x00\x00\x00\x08\x34\x00\x00\x07\x31\x00\x31\x00\x31\x00 // "111"
            byte[] b1 = { (byte)(val.Length + 9) };
            byte[] b2 = { (byte)(val.Length + 5) };
            byte[] b3 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
            byte[] b4 = Encoding.Unicode.GetBytes(val);
            string str = "\x12\x00"
                + Encoding.Default.GetString(b1)
                + "\x00\x00\x00"
                + Encoding.Default.GetString(b2)
                + "\x34"
                + Encoding.Default.GetString(b3)
                + "\x07"
                + Encoding.Default.GetString(b4);
            Send(device, str);
        }
        public void SendSerialSmartObject(CrestronDevice device, ushort id, byte idx, string val)
        {
            byte[] bLen1 = { (byte)(val.Length + 15) };
            byte[] bLen2 = { (byte)(val.Length + 11) };
            byte[] bLen3 = { (byte)(val.Length + 4) };
            byte[] bId   = { (byte)(id) };
            byte[] bVal  = Encoding.Default.GetBytes(val);
            byte[] bIdx = { (byte)((idx+9) / 0x100), (byte)((idx+9) % 0x100) };
            string sIdx = Encoding.Default.GetString(bIdx);
            string str = "\x12\x00"
                + Encoding.Default.GetString(bLen1)
                + "\x00\x00\x00"
                + Encoding.Default.GetString(bLen2)
                + "\x39\x00\x00\x00"
                + Encoding.Default.GetString(bId)
                + "\x00"
                + Encoding.Default.GetString(bLen3)
                + "\x34"
                + sIdx
                + "\x03"
                + Encoding.Default.GetString(bVal);
            //Debug("SendSerialSmartObject: " + Utils.createHexPrintableString(str));
            Send(device, str);
        }
        public void SendDigitalSmartObject(CrestronDevice device, byte id, byte idx, bool val)
        {
            tempBool = !tempBool;
            // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x00\x00 // id 3, press 1
            // \x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00\x03\x03\x27\x01\x80 // id 3, release 2
            byte[] b1 = { (byte)id };
            ushort NewIdx = (ushort)Utils.SetBit(idx - 1, 15, tempBool);
            byte[] b2 = { (byte)(NewIdx % 0x100), (byte)(NewIdx / 0x100) };
            string str = "\x05\x00\x0C\x00\x00\x09\x38\x00\x00\x00"
                + Encoding.Default.GetString(b1)
                + "\x03\x27"
                + Encoding.Default.GetString(b2);
            Send(device, str);
        }
        public void SendAnalogueSmartObject(CrestronDevice device, byte id, byte idx, ushort val)
        {
            // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x03\x05\x14\x00\x03\x00\x03 // id 3 analog setNumItems(idx 3) val 3
            // \x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00\x05\x05\x14\x00\x0a\x00\x09 // id 5 analog setItem1IconAna(idx 11) val 9
            byte[] b1 = { (byte)id };
            ushort NewIdx = (ushort)Utils.SetBit(idx - 1, 15, tempBool);
            byte[] b2 = { (byte)idx };
            byte[] b3 = { (byte)(val / 0x100), (byte)(val % 0x100) };
            string str = "\x05\x00\x0E\x00\x00\x0B\x38\x00\x00\x00"
                + Encoding.Default.GetString(b1)
                + "\x05\x14\x00"
                + Encoding.Default.GetString(b2)
                + Encoding.Default.GetString(b3);
            Send(device, str);
        }

        public void ParseDeviceDetails(Connection c, byte[] b1)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            if (b1.Length > 8)
            {
                if (b1[7] == 0x30) // run time status
                {
                    string s = Utils.createAsciiPrintableString(b1.Skip(11).Take(b1.Length - 11).ToArray());
                    Debug("status: " + s);
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
                    Debug("details: " + Utils.createAsciiPrintableString(b1.Skip(16).Take(b1.Length - 16).ToArray()));
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
                Debug("ParseCrestron sending details: " + Utils.createHexPrintableString(b1));
            }
        }

        public void ParseSerialUnicodeEvent(Connection c, byte[] b1)
        {
            Debug("ParseCrestron Serial unicode: " + Utils.createHexPrintableString(b1));
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            ushort idx = (ushort)(b1[8] * 0x100 + b1[9] + 1);
            //\x12\x00\x0A\x00\x00\x00\x06\x34\x00\x00\x07\x31\x00 // "1"
            //\x12\x00\x0C\x00\x00\x00\x08\x34\x00\x00\x07\x31\x00\x31\x00\x31\x00 // "111"
            //String str = "";
            //for (int i=11; i<b1.Length; i+=2)
            //    str = str + Encoding.Default.GetString(b1, i, 1);
            String str = Encoding.Unicode.GetString(b1, 11, b1.Length-11);
            SerialEventIn(dev, idx, str);
        }
        public void ParseJoinEvent(Connection c, byte[] b1)
        {
            //Debug("Join event");
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            switch (b1[6]) 
            {
                case 0x00:
                case 0x27: // digital
                { 
                    ushort idx = (ushort)((b1[8] & 0x7F) * 0x100 + b1[7] + 1);
                    // Debug("ParseCrestron Digital: " + Utils.createHexPrintableString(b1));
                    bool val = !Utils.GetBit(b1[8], 7);
                    DigitalEventIn(dev, idx, val);
                    break;
                }
                case 0x01:
                case 0x14:  // analogue
                {
                    Debug("ParseCrestron Analogue: " + Utils.createHexPrintableString(b1));
                    ushort idx = (ushort)(b1[7] * 0x100 + b1[8] + 1);
                    ushort val = (ushort)(b1[9] * 0x100 + b1[10]);
                    AnalogueEventIn(dev, idx, val);
                    break;
                }
                case 0x02: // serial type 1
                {
                    Debug("ParseCrestron Serial Type 1: " + Utils.createHexPrintableString(b1));
                    string str = Encoding.Default.GetString(b1);
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
                    Debug("ParseCrestron Serial Type 2: " + Utils.createHexPrintableString(b1));
                    break;
                }
                case 0x15: // serial type 3
                {
                    Debug("ParseCrestron Serial Type 3: " + Utils.createHexPrintableString(b1));
                    ushort idx = (ushort)(b1[7] * 0x100 + b1[8] + 1);
                    // "$05,$00,$08,$00,$00,$09,$15,$00,$00,$03,'a'"		= ser 1 to a
                    // "$05,$00,$0A,$00,$00,$09,$15,$00,$02,$03,'abc'"		= ser 3 to abc
                    string str = Encoding.Default.GetString(b1);
                    string val = str.Substring(10);
                    SerialEventIn(dev, idx, val);
                    break;
                }
                case 0x38: // smart object
                {
                    Byte ipid = dev != null ? dev.id : uis[0].id;
                    Debug("ParseCrestron Smart object " + Utils.createHexPrintableString(b1));
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
                            bool val = !Utils.GetBit(b1[14], 7);
                            DigitalSmartObjectEventIn(dev, onjectId, idx, val);
                            //Debug(String.Format("IPID {0} Smart object ID {1} Digital[{2}]:{3}", ipid, onjectId, idx, val));
                        }
                        else // ana, b[12]=0x14
                        {
                            ushort idx = (ushort)0;
                            ushort val = (ushort)b1[16];
                            AnalogueSmartObjectEventIn(dev, onjectId, idx, val);
                            Debug(String.Format("IPID {0} Smart object ID {1} Analog[{2}]:{3}", ipid, onjectId, idx, val));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug("Exception: " + e.ToString());
                    }
                    break;
                }
                default : 
                {
                    Debug(String.Format("ParseCrestron Unhandled join type {0}: {1}", b1[6], Utils.createHexPrintableString(b1)));
                    break;
                }
			}
        }
        public void SignOn(Connection c, byte[] b1, byte pos)
        {
            if (b1.Length > pos)
                DeviceSignIn(RegisterConnection(c, b1[pos]));
            else
                Debug("Sign on error: " + Utils.createHexPrintableString(b1));
        }
        public void ParseCrestronString(Connection c, byte[] b1)
        {
            //Debug("ParseCrestron: " + Utils.createHexPrintableString(b1));
            //string s1 = Encoding.UTF8.GetString(b1);
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            Byte ipid = dev != null ? dev.id : uis[0].id;
            switch (b1[0])
            {
                case 0x00: { Debug("Ack"); break; }
                case 0x01: { SignOn(c, b1, 8); break; }
                case 0x02: { Debug("Connection accepted" + dev == null ? " IPID " + dev.id : ""); break; }
                case 0x04: { Debug("Connection refused"  + dev == null ? " IPID " + dev.id : ""); break; }
                case 0x05: { ParseJoinEvent(c, b1); break; }
                case 0x0A: { SignOn(c, b1, 4); break; }
                case 0x0D: { Send(c, "\x0E\x00\x02\x00\x00"); break; } //Debug("Ping");
                case 0x0E: { break; } //Debug("Pong");
                case 0x0F: { Debug("Query"); break; }
                case 0x12: { ParseSerialUnicodeEvent(c, b1); break; }
                default: { Debug("ParseCrestron Unknown: " + Utils.createHexPrintableString(b1)); break; }

            }
        }

        public override void SendAcceptMessage(Connection c)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            string s = dev == null ? "": " IPID " + dev.id.ToString();
            Debug("SendAcceptMessage" + s);
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
                    Debug("Exception: " + e.ToString());
                }
            }
        }
        #endregion

        public void AddToWaitQueue(CrestronDevice device, ushort idx, bool state, int msec)
        {
            Thread.Sleep(msec);
            SendDigital(device, idx, state);
        }
    }

    class CrestronDevice : CrestronJoins
    {
        public List<CrestronJoins> smartGraphics = new List<CrestronJoins>();
        public List<Connection> connections = new List<Connection>();
        public string version { set; get; }
        public ushort currentPage;

        public CrestronDevice(byte IPID, Crestron_CIP_Server ControlSystem)
            : base(IPID)
        {

        }
    }

}