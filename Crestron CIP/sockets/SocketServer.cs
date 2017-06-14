// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="SocketServer.cs" company="AVPlus Integration Pty Ltd">
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

namespace AVPlus.sockets
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using AVPlus.CrestronCIP;

    public class SocketServer
    {
        #region declarations

        public event EventHandler<StringEventArgs> Debug;

        protected byte[] delim = { 13, 10 };
        //private byte[] buff = new byte[1024 * 10];
        //protected CircularBuffer cb = new CircularBuffer();

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ConnectionList Connections = new ConnectionList();

        public delegate void Accept_Delegate(object sender, Accept_EventArgs e);
        //public event Accept_Delegate Accept_EventHandler;

        public class Accept_EventArgs : EventArgs
        {
            public string ip { get; set; }
        }
        #endregion

        public SocketServer()
        {
        }

        public void StartServer(int nPort)
        {
            OnDebug(eDebugEventType.Info, "StartServer");
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), nPort));
            socket.Listen(10);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Accept));
        }

        public void Accept(object o)
        {
            OnDebug(eDebugEventType.Info, "Accept");
            while (true)
            {
                Thread.Sleep(500);
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            OnDebug(eDebugEventType.Info, "AcceptCallback");
            Socket s = (Socket)ar.AsyncState;
            Socket TcpSocket = s.EndAccept(ar);
            Connection c = new Connection(this, TcpSocket);
            c.Debug += new EventHandler<StringEventArgs>(connection_Debug);
            Connections.Add(c);
        }

        public virtual void Send(Connection c, string msg)
        {
            byte[] b = Encoding.Default.GetBytes(msg);
            //OnDebug(eDebugEventType.Info, "SendSocket: " + StringHelper.CreateHexPrintableString(b));
            /*
            foreach (Connection cl in Connections)
                if (cl.ClientSocket.Connected)
                    cl.ClientSocket.Send(b);
            */
            if (c.ClientSocket.Connected)
                c.ClientSocket.Send(b);
        }

        public virtual void ProcessBuffer(Connection c) { }
        public virtual void SendAcceptMessage(Connection c) { }

        void connection_Debug(object sender, StringEventArgs e)
        {
            if (Debug != null)
                Debug(sender, e);
        }
        public void OnDebug(eDebugEventType eventType, string str, params object[] list)
        {
            if (Debug != null)
                Debug(this, new StringEventArgs(String.Format(str, list)));
            //parent.Invoke(parent.Debug, new Object[] { str });
        }
    }

}