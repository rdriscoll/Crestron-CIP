// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="CircularBuffer.cs" company="AVPlus Integration Pty Ltd">
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
    using System.Text;

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
}