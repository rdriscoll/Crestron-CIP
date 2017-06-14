// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="Connection.cs" company="AVPlus Integration Pty Ltd">
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
    using System.Net.Sockets;
    using System.Threading;
    using AVPlus.CrestronCIP;

    public class Connection
    {
        private SocketServer parent; // todo, replace with events
        public event EventHandler<StringEventArgs> Debug;

        public Socket ClientSocket { get; set; }
        public string ConnectionName { get; set; }
        public DateTime LastActiveTime { get; set; }
        public byte[] buff = new byte[1024 * 10];
        public CircularBuffer cb = new CircularBuffer();
        public bool _readlock;
        public Object _bufferLock = new Object();


        public virtual void ParseString(object sender, CallbackEventArgs e) { }
        public virtual void ParseByteString(byte[] args) { }
        //public virtual void BufferDataIn() { }

        public event EventHandler<Closed_EventArgs> Closed_EventHandler;
        public class Closed_EventArgs : EventArgs
        {
            public Socket socket { get; set; }
        }
        public class CallbackEventArgs : EventArgs
        {
            public byte[] buffer { get; set; }
        }

        public Connection(SocketServer parent, Socket clientSocket)
            : this(parent, clientSocket, string.Empty)
        {
        }

        public Connection(SocketServer parent, Socket clientSocket, string connectionName)
        {
            this.parent = parent;
            this.ClientSocket = clientSocket;
            this.ConnectionName = connectionName;
            /*
            if (Accept_EventHandler != null)
            {
                Accept_EventArgs args = new Accept_EventArgs();
                args.ip = TcpSocket.RemoteEndPoint.ToString();
                Accept_EventHandler(this, args);
            }
             * */
            clientSocket.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(WriteToBuffer), clientSocket);
            parent.OnDebug(eDebugEventType.Info, "Connection accepted from " + clientSocket.RemoteEndPoint.ToString());
            parent.SendAcceptMessage(this);
        }

        public void WriteToBuffer(IAsyncResult ar)
        {
            try
            {
                int revCount = ClientSocket.EndReceive(ar);
                if (revCount > 0)
                {
                    Byte[] a = new Byte[revCount];
                    //byte[] buff = Connections.Find(x => x.ClientSocket == client).buff;
                    //IEnumerable<Connection> query = Connections.Where(x => x.ClientSocket == client);
                    Buffer.BlockCopy(buff, 0, a, 0, revCount);
                    cb.Write(a, 0, a.Length);
                }
                ClientSocket.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(WriteToBuffer), ClientSocket);
                BufferDataIn();
            }
            catch (SocketException ex)
            {
                parent.OnDebug(eDebugEventType.Info, "SocketException: {0} ", ex.ErrorCode);
                switch(ex.ErrorCode)
                {
                    case 10053: // An established connection was aborted by the software in your host machine
                        parent.OnDebug(eDebugEventType.Info, "Software caused connection abort");
                        Disconnect(ClientSocket);
                        break;
                    case 10054: // An existing connection was forcibly closed by the remote host.
                        parent.OnDebug(eDebugEventType.Info, "Connection reset by peer");
                        Disconnect(ClientSocket);
                        break;
                }
            }
            finally
            {
            }
        }

        public void BufferDataIn()
        {
            if (!_readlock)
            {
                _readlock = true;
                lock (_bufferLock)
                {
                    parent.ProcessBuffer(this);
                    //ProcessBuffer();
                }
                if (cb.GetLength() > 0)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReadBuffer_Callback));
                else
                    _readlock = false;
            }
        }

        public void ReadBuffer_Callback(object o)
        {
            try
            {
                while (cb.GetLength() > 0)
                {
                    Thread.Sleep(500);
                    lock (_bufferLock)
                    {
                        parent.ProcessBuffer(this);
                        //ProcessBuffer();
                    }
                }
                _readlock = false;
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Info, "ReadBuffer_Callback Exception: " + e.Message);
            }
        }

        private void Disconnect(Socket sc)
        {
            OnDebug(eDebugEventType.Info, "Disconnect");
            if (Closed_EventHandler != null)
            {
                Closed_EventArgs args = new Closed_EventArgs();
                args.socket = sc;
                Closed_EventHandler(this, args);
            }
        }

        public void OnDebug(eDebugEventType eventType, string str, params object[] list)
        {
            if (Debug != null)
                Debug(this, new StringEventArgs(String.Format(str, list)));
        }

    }
}