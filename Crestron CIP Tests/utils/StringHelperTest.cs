// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="StringHelperTest.cs" company="AVPlus Integration Pty Ltd">
//     {c} AV Plus Pty Ltd 2017.
//     http://www.avplus.net.au
//     20170617 Rod Driscoll
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
    using System.Linq;
    using NUnit.Framework;
    using System.Text;

    [TestFixture]
    public class StringHelperTest
    {
        [TestCase(null)]
        [TestCase("")]
        public void CreateAsciiPrintableString_NullOrEmptyInput_ReturnsEmpty(string sut) //UnitOfWork__StateUnderTest__ExpectedBehavior
        {
            string result = StringHelper.CreateAsciiPrintableString(sut);
            string expected = String.Empty;
            Console.WriteLine("Result: {0}", result);
            Assert.That(result.Equals(expected));
        }
        [Test]
        public void CreateAsciiPrintableString_StringWithAllPrintableAscii_DoesNotChange() 
        {
            byte[] b1 = Encoding.ASCII.GetBytes(" ~");
            byte[] b2 = new byte[b1[1] - b1[0] + 1];
            for (byte i = 0; i < b2.Count(); i++) 
                b2[i] = (byte)(i + b1[0]); // populate with all ascii characters from " " to "~"
            string sut = StringHelper.GetString(b2);
            Console.WriteLine("Input : {0}", sut);
            string result = StringHelper.CreateAsciiPrintableString(sut);
            Console.WriteLine("Result: {0}", result);
            Assert.That(result.Equals(sut));
        }
        [Test]
        public void CreateAsciiPrintableString_MixedAsciiAndHexString_ReturnsMixedReadable() 
        {
            string sut = "\x02PON\x03";
            string expected = "\\x02PON\\x03";
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.CreateAsciiPrintableString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [Test]
        public void CreateAsciiPrintableString_16BitString_ReturnsMixedReadableAsciiAndAsciiHex() 
        {
            string sut       = "\x00\x11\x22\x44\x88\x90\xA0\xB0\xC0\xD0\xE0\xF0\xFF";
            string expected = "\\x00\\x11\x22\x44\\x88\\x90\\xA0\\xB0\\xC0\\xD0\\xE0\\xF0\\xFF";
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.CreateAsciiPrintableString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
 
        [Test]
        public void CreateHexPrintableString_16BitString_ReturnsReadableAsciiHex() 
        {
            string sut       = "\x00\x11\x22\x44\x88\x90\xA0\xB0\xC0\xD0\xE0\xF0\xFF";
            string expected = "\\x00\\x11\\x22\\x44\\x88\\x90\\xA0\\xB0\\xC0\\xD0\\xE0\\xF0\\xFF";
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.CreateHexPrintableString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [Test]
        public void GetBytes_16BitString_Returns16BitBytes() 
        {
            string sut =                  "\x00\x11\x22\x44\x88\x90\xA0\xB0\xC0\xD0\xE0\xF0\xFF";
            byte[] expected = new byte[] { 0x00,0x11,0x22,0x44,0x88,0x90,0xA0,0xB0,0xC0,0xD0,0xE0,0xF0,0xFF };
            Console.WriteLine("{0} : Input", sut);
            Console.WriteLine("{0} : Expected", StringHelper.GetString(expected));
            byte[] result = StringHelper.GetBytes(sut);
            Console.WriteLine("{0} : Result", StringHelper.GetString(result));
            Assert.That(result.SequenceEqual(expected));
        }

        [TestCase(null)]
        [TestCase(new byte[] { })]
        public void GetString_NullOrEmptyInput_ReturnsEmptyString(byte[] sut) 
        {
            string expected = String.Empty;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.GetString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
 
        [Test]
        public void CreateBytesFromHexString_16BitString_Returns16BitBytes()
        {
            string sut     = @"\x00\x11\x22\x44\x88\x90\xA0\xB0\xC0\xD0\xE0\xF0\xFF";
            string expected = "\x00\x11\x22\x44\x88\x90\xA0\xB0\xC0\xD0\xE0\xF0\xFF";
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.CreateBytesFromHexString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [TestCase(null)]
        [TestCase("")]
        public void CreateBytesFromHexString_NullOrEmptyString_ReturnsEmptyString(string sut)
        {
            string expected = String.Empty;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            string result = StringHelper.CreateBytesFromHexString(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [TestCase("10101010")]
        [TestCase("00001111")]
        [TestCase("11110000")]
        [TestCase("11111111")]
        [TestCase("00000000")]
        public void GetBit_SomeBitsHigh_ResultMatchesInput(string sut)
        {
            byte b = Convert.ToByte(sut, 2); // convert binary to byte
            byte expected = b;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            byte result = 0;
            for (byte i = 0; i < 9; i++)
                result += (byte)(StringHelper.GetBit(b, i) ? 1 << i : 0); 
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [Test]
        public void GetBit_OutOfRangeBits_ResultAlwaysFalse()
        {
            byte sut = 0;
            bool expected = false;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            bool result = false;
            for (byte i = 9; i < 50; i++)
                result |= StringHelper.GetBit(sut, i);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [TestCase("10101010")]
        [TestCase("00001111")]
        [TestCase("11110000")]
        [TestCase("11111111")]
        [TestCase("00000000")]
        public void SetBit_SetOddBitsHigh_ResultMatchTestValue(string sut)
        {
            byte b = Convert.ToByte(sut, 2); // convert binary to byte
            byte expected = b;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            byte result = 0;
            for (byte i = 0; i < 9; i++)
                result = (byte)StringHelper.SetBit(b, i, StringHelper.GetBit(b, i));
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [Test]
        public void SetBit_OutOfRangeBits_AlwaysReturnsFalse()
        {
            byte sut = 0;
            bool expected = false;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            bool result = false;
            for (byte i = 9; i < 50; i++)
                result |= StringHelper.GetBit(sut, i);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [Test]
        public void Atoi_MixedString_ReturnsFirst32bitNumber()
        {
            string sut = "Item 98765 Pressed ... or maybe 8";
            int expected = 98765;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            int result = StringHelper.Atoi(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [Test]
        public void Atoi_MixedStringWithNegtive_ReturnsFirstNegativeNumber()
        {
            string sut = "Item -98765 Pressed ... or maybe 8";
            int expected = -98765;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            int result = StringHelper.Atoi(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [TestCase("Item Pressed")]
        [TestCase("")]
        [TestCase(null)]
        public void Atoi_NoNumberOrNull_ReturnsZero(string sut)
        {
            int expected = 0;
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: {0}", expected);
            int result = StringHelper.Atoi(sut);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }
        [TestCase("99999999999999999")]
        [TestCase("-9999999999999999")]
        public void Atoi_Int32_ThrowsOverflowException(string sut)
        {
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: OverflowException");
            Assert.That(() => StringHelper.Atoi(sut), Throws.TypeOf<OverflowException>());
        }

        [TestCase(0, 0, 100, 0, 65535)]
        [TestCase(-50, -50, 50, 0, 65535)]
        [TestCase(100, 100, 200, -24, 12)]
        [TestCase(0, 0, 65535, -24, 12)]
        [TestCase(-20, -20, 1000, -12, 12)]
        public void ConvertRanges_ValidInputs_ReturnsMinimum(int sut, int inMin, int inMax, int outMin, int outMax)
        {
            Console.WriteLine("Input   : {0}", sut);
            int expected = outMin;
            Console.WriteLine("Expected: {0}", expected);
            int result = StringHelper.ConvertRanges(sut, inMin, inMax, outMin, outMax);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [TestCase(  100,   0,   100,    0, 65535)]
        [TestCase(   50, -50,    50,    0, 65535)]
        [TestCase(  200, 100,   200,  -24,    12)]
        [TestCase(65535,   0, 65535,  -24,    12)]
        [TestCase( 1000, -20,  1000,  -12,    12)]
        [TestCase(  -12, -20,   -12,    0,   100)]
        public void ConvertRanges_ValidInputs_ReturnsMaximum(int sut, int inMin, int inMax, int outMin, int outMax)
        {
            Console.WriteLine("Input   : {0}", sut);
            int expected = outMax;
            Console.WriteLine("Expected: {0}", expected);
            int result = StringHelper.ConvertRanges(sut, inMin, inMax, outMin, outMax);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Equals(expected));
        }

        [TestCase( -1,  0, 100, 0, 65535)]
        [TestCase(101,  0, 100, 0, 65535)]
        [TestCase( 98, 99, 100, 0,   100)]
        public void ConvertRanges_InvalidInputs_ThrowsArgumentOutOfRangeException(int sut, int inMin, int inMax, int outMin, int outMax)
        {
            Console.WriteLine("Input   : {0}", sut);
            Console.WriteLine("Expected: ArgumentOutOfRangeException");
            Assert.That(() => StringHelper.ConvertRanges(sut, inMin, inMax, outMin, outMax)
                        , Throws.TypeOf<ArgumentOutOfRangeException>()
                            .With.Matches< ArgumentOutOfRangeException>(x => x.ParamName == "val"));
        }

        [TestCase(  "", (ushort)0, eCrestronFont.Arial, eNamedColour.White)]
        [TestCase(null, (ushort)0, eCrestronFont.Arial, eNamedColour.White)]
        public void FormatTextForUi_NullOrEmptyInput_ReturnsValidString(string sut, ushort fontSize, eCrestronFont font, eNamedColour colour)
        {
            Console.WriteLine("Input   : {0}", sut);
            string result = StringHelper.FormatTextForUi(sut, fontSize, font, colour);
            Console.WriteLine("Result  : {0}", result);
            Assert.That(result.Length > 0);
        }
    }
}
