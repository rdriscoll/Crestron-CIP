using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
