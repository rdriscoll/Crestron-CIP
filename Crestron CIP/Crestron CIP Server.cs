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
        private static bool _readlock;
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
            Debug("SendSocket: " + Utils.createHexPrintableString(b));
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
            Debug(String.Format("Digital[{0}]: {1}", idx, val));
            //UiWithMeta ui = uis.Find(x => x.id == id);
            //uiHandler.DigitalEventIn(id, idx, val);
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
        #endregion

        #region string_parsing

        public void SendDigital(CrestronDevice device, ushort idx, bool val)
        {
            ushort NewIdx = (ushort)Utils.SetBit(idx - 1, 15, val);
            byte[] b = { (byte)(NewIdx % 0x100), (byte)(NewIdx / 0x100) };
            string s = Encoding.Default.GetString(b);
            string str = "\x05\x00\x06\x00\x00\x03\x00" + s;
            Send(device, str);
            Debug("SendDigital: " + Utils.createHexPrintableString(str));
            // local feedback
            if (device.digitals.Where(x => x.pos == idx).Count() == 0)
                device.digitals.Add(new Digital(idx, false));
            //IEnumerable<Digital> query = device.digitals.Where(x => x.pos == idx);
            //foreach (Digital d in query)
            //    d.value = val;
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
            byte[] b1 = { (byte)(val.Length + 7) };
            byte[] b2 = { (byte)((idx - 1) / 0x100), (byte)((idx - 1) % 0x100) };
            byte[] b3 = Encoding.Default.GetBytes(val);
            string str = "\x05\x00"
                + Encoding.Default.GetString(b1)
                + "\x00\x00\x09\x15"
                + Encoding.Default.GetString(b2)
                + "\x03"
                + Encoding.Default.GetString(b3);
            Debug("SendSerial: " + Utils.createHexPrintableString(str));
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
        public void SendSerialSmartObject(CrestronDevice device, ushort id, ushort idx, string val)
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
        public void SendDigitalSmartObject(CrestronDevice device, byte id, ushort idx, bool val)
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

        public void ParseCrestronString(Connection c, byte[] b1)
        {
            //Debug("ParseCrestron: " + Utils.createHexPrintableString(b1));
            //string s1 = Encoding.UTF8.GetString(b1);
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            Byte ipid;
            if (dev != null)
                ipid = dev.id;
            else
                ipid = uis[0].id;
            switch (b1[0])
            {
                case 0x00:
                {
                    Debug("Ack");
                    break;
                }
                case 0x01:
                {
                    if (b1.Length > 8)
                    {
                        ipid = b1[8];
                        if(dev != null) // look for existing connections
                            dev.id = ipid;
                        else // look for existing ui
                        {
                            if (uis.Exists(x => x.id.Equals(ipid))) // look for existing ui
                                uis.Find(x => x.id.Equals(ipid)).connections.Add(c);
                            else // create new ui
                            {
                                dev = new CrestronDevice(ipid, this);
                                dev.connections.Add(c);
                                uis.Add(dev);
                            }
                        }
                        Debug("Sign on IPID: " + ipid.ToString());
                        string sMsg_ = "\x02\x00\x04\x00\x00\x00\x03"; // accept connection
                        Send(c, sMsg_);
                    }
                    break;
                }
                case 0x02:
                {
                    Debug("Connection accepted" + dev == null ? " IPID " + dev.id : "");
                    break;
                }
                case 0x04:
                {
                    Debug("Connection refused" + dev == null ? " IPID " + dev.id : "");
                    break;
                }
                case 0x05:
                {
                    //Debug("Join event");
                    switch (b1[6]) 
                    {
						case 0x03:
                        case 0x00:
                        case 0x27: // digital
                        { 
                            if (b1.Length < 9)
                            {
                                // Debug("ParseCrestron unknown short Digital: " + Utils.createHexPrintableString(b1));
                                // sent on connection
                            }
                            else if (b1.Length > 9)
                            {
                                ushort idx = (ushort)((b1[8] & 0x7F) * 0x100 + b1[7] + 1);
                                bool val = !Utils.GetBit(b1[8], 7);
                                // Debug(String.Format("ParseCrestron unknown long Digital[{0}]:{1}", idx, val));
                            }
                            else
                            {
                                ushort idx = (ushort)((b1[8] & 0x7F) * 0x100 + b1[7] + 1);
                                // Debug("ParseCrestron Digital: " + Utils.createHexPrintableString(b1));
                                bool val = !Utils.GetBit(b1[8], 7);
                                DigitalEventIn(dev, idx, val);
                            }
                            break;
                        }
                        case 0x01:
                        case 0x14:  // analogue
                        {
                            //Debug("ParseCrestron Analogue: " + Utils.createHexPrintableString(b1));
                            ushort idx = (ushort)(b1[7] * 0x100 + b1[8] + 1);
                            ushort val  = (ushort)(b1[9] * 0x100 + b1[10]);
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
                    break;
                }
                case 0x0A:
                {
                    Debug("EISC 3 Series Sign on");
                    if (b1.Length > 8)
                    {
                        ipid = b1[4];
                        if (dev != null) // look for existing connections
                            dev.id = ipid;
                        else // look for existing ui
                        {
                            if (uis.Exists(x => x.id.Equals(ipid))) // look for existing ui
                                uis.Find(x => x.id.Equals(ipid)).connections.Add(c);
                            else // create new ui
                            {
                                dev = new CrestronDevice(ipid, this);
                                dev.connections.Add(c);
                                uis.Add(dev);
                            }
                        }
                        Debug("3-Series EISC Sign on: " + ipid.ToString());
                        string sMsg_ = "\x02\x00\x04\x00\x00\x00\x03"; // accept connection
                        Send(c, sMsg_);
                    }
                    break;
                }
                case 0x0D:
                {
                    //Debug("Ping");
                    string sMsg_ = "\x0E\x00\x02\x00\x00"; //pong
                    Send(c, sMsg_);
                    break;
                }
                case 0x0E:
                {
                    //Debug("Pong");
                    break;
                }
                case 0x0F:
                {
                    Debug("Query");
                    break;
                }
                case 0x12: // serial unicode
                {
                    Debug("ParseCrestron Serial unicode: " + Utils.createHexPrintableString(b1));
                    ushort idx = (ushort)(b1[8] * 0x100 + b1[9] + 1);
                    //\x12\x00\x0A\x00\x00\x00\x06\x34\x00\x00\x07\x31\x00 // "1"
                    //\x12\x00\x0C\x00\x00\x00\x08\x34\x00\x00\x07\x31\x00\x31\x00\x31\x00 // "111"
                    //String str = "";
                    //for (int i=11; i<b1.Length; i+=2)
                    //    str = str + Encoding.Default.GetString(b1, i, 1);
                    String str = Encoding.Unicode.GetString(b1, 11, b1.Length-11);
                    SerialEventIn(dev, idx, str);
                    break;
                }
                default:
                {
                    Debug("ParseCrestron Unknown: " + Utils.createHexPrintableString(b1));
                    break;
                }

            }
        }

        public override void SendAcceptMessage(Connection c)
        {
            CrestronDevice dev = uis.Find(x => x.connections.Contains(c));
            Debug("SendAcceptMessage" + dev == null ? " IPID " + dev.id : "");
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
                    //string s1 = Encoding.UTF8.GetString(b1);
                    //Debug("Rx: " + s1);
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
    }

    class CrestronDevice : CrestronJoins
    {
        public List<CrestronJoins> smartGraphics = new List<CrestronJoins>();
        public List<Connection> connections = new List<Connection>();

        public CrestronDevice(byte IPID, Crestron_CIP_Server ControlSystem)
            : base(IPID)
        {

        }
    }


}