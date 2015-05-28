using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace avplus
{
    class Utils
    {
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            char[] ch1 = str.ToCharArray();
            System.Buffer.BlockCopy(ch1, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string createHexPrintableString(byte[] bArgs)
        {
            string strOut_ = "";
            foreach (byte bIndex_ in bArgs)
            {
                string sHexOutput_ = String.Format("{0:X}", bIndex_);
                strOut_ += @"\x" + String.Format("{0:X}", sHexOutput_).PadLeft(2, '0');
            }
            return strOut_;
        }
        public static string createHexPrintableString(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            return createHexPrintableString(b);
        }

        public static string createBytesFromHexString(string str)
        {
            String p1 = @"(\\[xX][0-9a-fA-F]{2}|.)";
            String p2 = @"\\x([xX][0-9a-fA-F]{2})";
            Regex r1 = new Regex(p1);
            //Regex r2 = new Regex(p2);
            MatchCollection m = r1.Matches(str);
            string s1 = "";
            foreach (Match m1 in m)
            {
                string s2 = m1.Value;
                if (m1.Value.IndexOf("\\x") > -1)
                {
                    string s3 = m1.Value.Remove(0, 2);
                    byte b2 = Byte.Parse(s3, System.Globalization.NumberStyles.HexNumber);
                    s1 = s1 + Encoding.Default.GetString(new byte[]{ b2 });
                }
                else
                {
                    byte[] b1 = Encoding.Default.GetBytes(m1.Value);
                    s1 = s1 + Encoding.Default.GetString(b1);
                }
            }
            byte[] b = Encoding.Default.GetBytes(s1);
            return s1;
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }
        public static int SetBit(int b, byte bitNumber, bool val)
        {
            int r = val == true ? b | (1 << bitNumber) : b & (Byte.MaxValue - (1 << bitNumber));
            return r;
        }
        /*
        public static string Right(this string str, int length)
        {
            String result;
            if (length >= str.Length)
                result = str;
            else
                result = str.Substring(str.Length - length);
            return result;
        }
        */

        public static int atoi(string strArg) // "hello 123 there" returns 123, because ToInt throws exceptions when non numbers are inserted
        {
            String m = Regex.Match(strArg, @"\d+").Value;
            return (m.Length == 0 ? 0 : Convert.ToInt32(m));
        }


        public static int convertRanges(int val, int inMin, int inMax, int outMin, int outMax)
        {
            int inRange = inMax - inMin;
            int outRange = outMax - outMin;
            int result = ((val - inMin)*outRange)/inRange + outMin;
            return result;
        }

    }
}
