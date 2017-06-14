// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="ConnectionList.cs" company="AVPlus Integration Pty Ltd">
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
}