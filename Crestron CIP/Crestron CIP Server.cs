using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using avplus;
using avplus.sockets;

namespace avplus
{
    class Crestron_CIP_Server : SocketServer
    {
        const int PORT_CIP = 41795;
        private static bool _readlock;
        private Object _bufferLock = new Object(); 

        public Crestron_CIP_Server(ClientForm myForm)
            : base(myForm, PORT_CIP)
        {
            Console.WriteLine("Crestron_CIP_Server created");
        }

        public void ParseCrestronString(byte[] b1)
        {
            Debug("ParseCrestronString: " + Utils.createHexPrintableString(b1));
            //string s1 = Encoding.UTF8.GetString(b1);
            switch (b1[0])
            {
                case 0x00:
                {
                    Debug("Ack");
                    break;
                }
                case 0x01:
                {
                    Debug("Sign on");
                    break;
                }
                case 0x02:
                {
                    Debug("Connection accepted");
                    break;
                }
                case 0x04:
                {
                    Debug("Connection refused");
                    break;
                }
                case 0x05:
                {
                    Debug("Join event");
                    break;
                }
                case 0x0A:
                {
                    Debug("EISC 3 Series Sign on");
                    break;
                }
                case 0x0D:
                {
                    Debug("Ping");
                    string sMsg_ = "\x0E\x00\x02\x00\x00"; //pong
                    Send(sMsg_);
                    break;
                }
                case 0x0E:
                {
                    Debug("Pong");
                    break;
                }
                case 0x0F:
                {
                    Debug("Query");
                    break;
                }
                default:
                {
                    Debug("Unknown string in");
                    break;
                }

            }
        }

        // 3rd byte says how many bytes incoming
        public void ProcessBuffer()
        {
            lock (_bufferLock)
            {
                byte[] b1;
                while (true)
                {
                    b1 = cb.GetBuffer(); // look at currentbuffer (non-destructive)
                    if (b1 == null || b1.Length < 1)
                        break;
                    try
                    {
                        //string s1 = Encoding.UTF8.GetString(b1);
                        //Debug("Rx: " + s1);
                        if (b1.Length > 2)
                        {
                            if (b1.Length > b1[2]+2)
                            {
                                ParseCrestronString(cb.Read(0, b1[2]+3));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug("Exception: " + e.ToString());
                    }
                }
            }
        }

        public override void BufferDataIn() 
        { 
            if (!_readlock)
            {
                _readlock = true;
                ProcessBuffer();
                if (cb.GetLength() > 0)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReadBuffer_Callback));
                else
                   _readlock = false;
            }
        }

        public void ReadBuffer_Callback(object o)
        {
            while (cb.GetLength() > 0)
            {
                Thread.Sleep(500);
                ProcessBuffer();
            }
            _readlock = false;
        }

    }
}