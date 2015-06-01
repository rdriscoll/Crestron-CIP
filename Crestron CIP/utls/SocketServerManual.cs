using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace avplus.sockets
{
    public class SocketServer
    {
        #region declarations
        protected ClientForm parent; // for delegates

        protected byte[] delim = { 13, 10 };
        //private byte[] buff = new byte[1024 * 10];
        //protected CircularBuffer cb = new CircularBuffer();

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ConnectionList Connections = new ConnectionList();

        public delegate void Accept_Delegate(object sender, Accept_EventArgs e);
        public event Accept_Delegate Accept_EventHandler;

        public class Accept_EventArgs : EventArgs
        {
            public string ip { get; set; }
        }
        #endregion

        public SocketServer(ClientForm myForm, int nPort)
        {
            parent = myForm;
        }

        public void StartServer(int nPort)
        {
            Debug("StartServer");
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), nPort));
            socket.Listen(10);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Accept));
        }

        public void Accept(object o)
        {
            Debug("Accept");
            while (true)
            {
                Thread.Sleep(500);
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            Debug("AcceptCallback");
            Socket s = (Socket)ar.AsyncState;
            Socket TcpSocket = s.EndAccept(ar);
            Connection c = new Connection(this, TcpSocket);
            Connections.Add(c);
        }

        public virtual void Send(Connection c, string msg)
        {
            byte[] b = Encoding.Default.GetBytes(msg);
            //Debug("SendSocket: " + Utils.createHexPrintableString(b));
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

        public void Debug(string str)
        {
            parent.Invoke(parent.debug, new Object[] { str });
        }
    }

    #region Connection
    // *****************************************************************
    public class Connection
    {
        public Socket ClientSocket { get; set; }
        public string ConnectionName { get; set; }
        public DateTime LastActiveTime { get; set; }
        public byte[] buff = new byte[1024 * 10];
        public CircularBuffer cb = new CircularBuffer();
        public bool _readlock;
        public Object _bufferLock = new Object();

        private SocketServer parent;

        public virtual void ParseString(object sender, CallbackEventArgs e) { }
        public virtual void ParseByteString(byte[] args) { }
        //public virtual void BufferDataIn() { }

        public delegate void Closed_Delegate(object sender, Closed_EventArgs e);
        public event Closed_Delegate Closed_EventHandler;
        public class Closed_EventArgs : EventArgs
        {
            public Socket socket { get; set; }
        }
        //public delegate void CallbackDelegate(object sender, CallbackEventArgs e);
        //public event CallbackDelegate Callback_EventHandler;
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
            parent.Debug("Connection accepted from " + clientSocket.RemoteEndPoint.ToString());
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
                if (ex.ErrorCode == 10054)
                {
                    Disconnect(ClientSocket);
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
            }
        }

        private void Disconnect(Socket sc)
        {
            Debug("Disconnect");
            if (Closed_EventHandler != null)
            {
                Closed_EventArgs args = new Closed_EventArgs();
                args.socket = sc;
                Closed_EventHandler(this, args);
            }
        }

        protected void Debug(string str)
        {
            parent.Debug( str );
        }

    }
    // *****************************************************************
    public class ConnectionList : System.Collections.CollectionBase
    {
        public ConnectionList()
        {
        }

        public void Add(Connection value)
        {
            List.Add(value);
        }

        public void Remove(Connection value)
        {
            List.Remove(value);
        }

        public bool Contains(Connection value)
        {
            return (List.Contains(value));
        }

        public Connection this[int index]
        {
            get
            {
                return List[index] as Connection;
            }
            set
            {
                List[index] = value;
            }
        }

        public Connection this[string connectionName]
        {
            get
            {
                foreach (Connection connection in List)
                {
                    if (connection.ConnectionName == connectionName)
                        return connection;
                }
                return null;
            }
        }
    }
    #endregion

    #region CircularBuffer
    // *****************************************************************
    public class CircularBuffer
    {
        #region declarations
        private const int INITCAPACITY = 4 * 1024;
        private const int INCREMENTSIZE = 4 * 1024;

        private object _synObject = new object();

        private byte[] _buffer;
        private int _capacity;
        private int _length;
        private bool _expandable;
        private int _maxCapacity;
        private int _rPos;
        private int _wPos;
        #endregion

        public CircularBuffer()
            : this(INITCAPACITY)
        {
        }

        public CircularBuffer(int capacity)
            : this(capacity, true)
        {
        }

        public CircularBuffer(int capacity, bool expandable)
            : this(capacity, expandable, -1)
        {
        }

        public CircularBuffer(int capacity, bool expandable, int maxCapacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            if (expandable && (maxCapacity != -1 && maxCapacity < capacity))
            {
                throw new ArgumentOutOfRangeException("maxCapacity");
            }
            _length = 0;
            _capacity = capacity;
            _expandable = expandable;
            _maxCapacity = maxCapacity;
            _buffer = new byte[_capacity];
            _rPos = 0;
            _wPos = 0;
        }

        public int GetLength()
        {
            return _length;
        }

        public byte[] GetBuffer()
        {
            byte[] buffer = new byte[_length];
            PopulateBuffer(buffer, 0, _length);
            return buffer;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("null buffer");
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException("offset or count < 0");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("buffer - offset < count");
            }
            lock (_synObject)
            {
                int readLen = Math.Min(_length, count);
                if (readLen == 0)
                {
                    return 0;
                }
                ReadInternal(buffer, offset, readLen);
                return readLen;
            }
        }

        public byte[] Read()
        {
            return Read(0, _length);
        }

        public byte[] Read(int offset)
        {
            return Read(offset, _length);
        }

        public byte[] Read(int offset, int count)
        {
            lock (_synObject)
            {
                int readLen = Math.Min(_length, count);
                if (readLen == 0)
                {
                    return null;
                }
                return ReadInternal(offset, readLen);
            }
        }

        public byte[] ReadToDelimitter(byte[] delim)
        {
            byte[] buffer = new byte[_length];
            PopulateBuffer(buffer, 0, _length);
            string s1 = Encoding.ASCII.GetString(buffer);
            string s2 = Encoding.ASCII.GetString(delim);
            int i = s1.IndexOf(s2);
            return i > -1 ? Read(0, i + s2.Length) : null;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer null");
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException("offset | count < 0");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("buffer < count");
            }
            lock (_synObject)
            {
                int minCapacityNeeded = _length + count;
                ExpandStream(minCapacityNeeded);

                if (minCapacityNeeded > _capacity)
                {
                    throw new NotSupportedException("count " + count);
                }
                this.WriteInternal(buffer, offset, count);
            }
        }

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            if (_rPos > _wPos)
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _wPos, count);
            }
            else
            {
                int afterWritePosLen = _capacity - _wPos;
                if (afterWritePosLen >= count)
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _wPos, count);
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, _wPos, afterWritePosLen);
                    int restLen = count - afterWritePosLen;
                    Buffer.BlockCopy(buffer, offset + afterWritePosLen, _buffer, 0, restLen);
                }
            }
            _wPos += count;
            _wPos %= _capacity;
            _length += count;
        }

        private byte[] PopulateBuffer(byte[] buffer, int offset, int count)
        {
            if (_rPos < _wPos)
            {
                Buffer.BlockCopy(_buffer, _rPos, buffer, offset, count);
            }
            else
            {
                int afterReadPosLen = _capacity - _rPos;
                if (afterReadPosLen >= count)
                {
                    Buffer.BlockCopy(_buffer, _rPos, buffer, offset, count);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _rPos, buffer, offset, afterReadPosLen);
                    int restLen = count - afterReadPosLen;
                    Buffer.BlockCopy(_buffer, 0, buffer, afterReadPosLen, restLen);
                }
            }
            return buffer;
        }

        private void ReadInternal(byte[] buffer, int offset, int count)
        {
            PopulateBuffer(buffer, offset, count);
            _rPos += count;
            _rPos %= _capacity;
            _length -= count;
        }

        private byte[] ReadInternal(int offset, int count)
        {
            byte[] buffer = new byte[count];
            buffer = PopulateBuffer(buffer, offset, count);
            _rPos += count;
            _rPos %= _capacity;
            _length -= count;
            return buffer;
        }

        private void ExpandStream(int minSize)
        {
            if (!_expandable)
            {
                return;
            }
            if (_capacity >= minSize)
            {
                return;
            }
            if (_maxCapacity != -1 && (_maxCapacity - _capacity) < INCREMENTSIZE)
            {
                return;
            }
            int blocksNum = (int)Math.Ceiling((double)(minSize - _capacity) / INCREMENTSIZE);
            byte[] buffNew = new byte[_capacity + blocksNum * INCREMENTSIZE];
            int strLen = _length;
            ReadInternal(buffNew, 0, _length);
            _buffer = buffNew;
            _rPos = 0;
            _wPos = strLen;
            _capacity = buffNew.Length;
            _length = strLen;
        }
    }
    #endregion
}