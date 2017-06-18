// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="AVPlus Integration Pty Ltd">
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

namespace AVPlus.CrestronCIP
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Drawing;
    using System.Collections.Generic;
    using System.Linq;

    public class StringEventArgs : EventArgs
    {
        public string val;

        public StringEventArgs(string val)
        {
            this.val = val;
        }
    }

    public static class StringHelper
    {
        public static eCrestronFont defaultFont = eCrestronFont.Arial;
        public static eNamedColour defaultColour = eNamedColour.White;
        public static ushort defaultfontSize = 20;
        
        public static string CreateAsciiPrintableString(byte[] b)
        {
            string strOut_ = String.Empty;
            byte[] b1 = Encoding.ASCII.GetBytes(" ~");
            foreach (byte bIndex_ in b)
            {   
                if (bIndex_ >= b1[0] && bIndex_ <= b1[1]) // ASCII
                    strOut_ += Encoding.ASCII.GetString(new byte[] { bIndex_ });
                else
                    strOut_ += String.Format("\\x{0:X2}", bIndex_);
            }
            return strOut_;
        }
        public static string CreateAsciiPrintableString(string str)
        {
            byte[] b = GetBytes(str);
            return CreateAsciiPrintableString(b);
        }
        public static string CreateHexPrintableString(byte[] bArgs)
        {
            string strOut_ = "";
            foreach (byte bIndex_ in bArgs)
                strOut_ += String.Format("\\x{0:X2}", bIndex_);
            return strOut_;
        }
        public static string CreateHexPrintableString(string str)
        {
            byte[] b = GetBytes(str);
            return CreateHexPrintableString(b);
        }
        public static byte[] GetBytes(string str) // because no other encodings work the way we need
        {
            if (str == null)
                return new byte[0];
            else
                return Encoding.GetEncoding("ISO-8859-1").GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
            if (bytes == null) 
                return String.Empty;
            else
                return Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 0, bytes.Length);
        }
        //public static byte[] GetBytes(string str) // because no encodings work the way we need
        //{
        //    var bytes = new byte[str.Length];
        //    try
        //    {
        //        for (int i = 0; i < str.Length; i++)
        //        {
        //            byte b = (byte)str[i];
        //            bytes[i] = checked(b); // Slower but throws OverflowException if there is an invalid character
        //            //bytes[i] = unchecked((byte)str[i]); // Faster
        //        }
        //    }
        //    catch (OverflowException e)
        //    {
        //        Console.WriteLine("GetBytes OverflowException: str[{0}]:{1}, {2}", i.ToString(), b.ToString(), e.Message);
        //    }
        //    return bytes;
        //}
        public static string CreateBytesFromHexString(string str)
        {
            if (str == null)
                return String.Empty;
            String p1 = @"(\\[xX][0-9a-fA-F]{2}|.)";
            Regex r1 = new Regex(p1);
            MatchCollection m = r1.Matches(str);
            string s1 = "";
            foreach (Match m1 in m)
            {
                string s2 = m1.Value;
                if (m1.Value.IndexOf("\\x") > -1)
                {
                    string s3 = m1.Value.Remove(0, 2);
                    byte b2 = Byte.Parse(s3, System.Globalization.NumberStyles.HexNumber);
                    s1 = s1 + GetString(new byte[] { b2 });
                }
                else
                {
                    byte[] b1 = GetBytes(m1.Value);
                    s1 = s1 + GetString(b1);
                }
            }
            byte[] b = GetBytes(s1);
            return s1;
        }
        public static bool GetBit(byte b, byte bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }
        public static int SetBit(int b, byte bitNumber, bool val)
        {
            if (Math.Pow(2, bitNumber) > int.MaxValue)
                return b;
            else
                return val == true ? b | (1 << bitNumber) : b & ~(1 << bitNumber);
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
        public static int Atoi(string strArg) // "hello 123 there" returns 123, because ToInt throws exceptions when non numbers are inserted
        {
            if (strArg == null)
                return 0;
            else
            {
                String m = Regex.Match(strArg, @"[-]*\d+").Value;
                return (m.Length == 0 ? 0 : Convert.ToInt32(m));
            }
        }
        public static int ConvertRanges(int val, int inMin, int inMax, int outMin, int outMax)
        {
            if (val < inMin || val > inMax)
            {
                Console.WriteLine(String.Format("ConvertRanges: input val not within valid range inMin:{0}, inMax: {1}", inMin, inMax));
                throw new ArgumentOutOfRangeException("val", String.Format("input val not within valid range inMin:{0}, inMax: {1}", inMin, inMax));
            }
            else
            {
                int inRange = inMax - inMin;
                int outRange = outMax - outMin;
                int result = ((val - inMin) * outRange) / inRange + outMin;
                return result;
            }
        }
 
        public static string FormatTextForUi(string text, ushort fontSize, eCrestronFont font, eNamedColour colour)
        {
            if (fontSize == 0)
                fontSize = defaultfontSize;
            string str = String.Format("<FONT size=\x22{0}\x22 face=\x22{1}\x22 color=\x22#{2:X6}\x22>{3}</FONT>", fontSize, font, (uint)colour, text);
            return str;
        }
        public static string FormatTextForUi(string text)
        {
            return FormatTextForUi(text, defaultfontSize, defaultFont, defaultColour);
        }
        public static string FormatTextForUi(string str, string font, byte fontSize, int colour, bool bold, bool underline, bool italic)
        {
            //<FONT size="30" face="Crestron Sans Pro" color="#ffffff">text</FONT>
            string s = bold ? "<B>" + str + "</B>" : str;
            s = underline   ? "<U>" + s   + "</U>" : s;
            s = italic      ? "<I>" + s   + "</I>" : s;
            s = String.Format("<FONT size=\"{0}\" face=\"{1}\" color=\"#{2:x6}\">{3}</FONT>", fontSize, font, 0xFFFFFF & colour, s);
            return s;
        }
        public static string FormatTextForUi(string str, byte fontSize, int colour)
        {
            return FormatTextForUi(str, "Crestron Sans Pro", fontSize, colour, false, false, false);
        }
        public static int GetColour(string colour)
        {
            return Color.FromName(colour).ToArgb();
        }

        #region font dictionary

        public static Dictionary<eCrestronFont, string> CrestronFonts = new Dictionary<eCrestronFont, string>()
        {
            { eCrestronFont.Arial                 , "Arial" },
            { eCrestronFont.Crestron_Sans_Pro     , "Crestron Sans Pro" },
            { eCrestronFont.Crestron_AV           , "Crestron AV" },
            { eCrestronFont.Crestron_Simple_Icons , "Crestron Simple Icons" }
        };

        #endregion
        #region icon dictionaries

        public static Dictionary<ushort, string> IconsLgDict = new Dictionary<ushort, string>()
        {
            { 0, "AM-FM" },
            { 1, "CD" },
            { 2, "Climate" },
            { 3, "Display Alt" }, // black LCD monitor
            { 4, "Display" }, // blue LCD monitor
            { 5, "DVR" }, // DVR text on red btn
            { 6, "Energy Management" },
            { 7, "Favorites" },
            { 8, "Film Reel" },
            { 9, "Home" },
            { 10, "Internet Radio" },
            { 11, "iPod" },
            { 12, "iServer" },
            { 13, "Lights" },
            { 14, "Music Note" },
            { 15, "News" },
            { 16, "Pandora" },
            { 17, "Power" },
            { 18, "Satellite Alt" },
            { 19, "Satellite" },
            { 20, "Sec-Cam" }, // black on pivot mount
            { 21, "Security" },
            { 22, "Shades" },
            { 23, "User Group" },
            { 24, "Video Conferencing" },
            { 25, "Video Switcher" },
            { 26, "Wand" },
            { 27, "Weather" },
            { 29, "Speaker" },
            { 30, "Mic" },
            { 31, "Projector" },
            { 32, "Screen" },
            { 33, "Gear" },
            { 34, "Sec-Cam Alt" }, // white no PTZ
            { 35, "Document Camera" }, // lens over paper
            { 36, "Backgrounds" },
            { 37, "Gamepad" },
            { 38, "iMac" },
            { 39, "Laptop Alt" },
            { 40, "Laptop" },
            { 41, "MacBook Pro" },
            { 42, "Music Note Alt" },
            { 43, "Phone Alt" },
            { 44, "Phone" },
            { 45, "Pool" },
            { 46, "Airplay" },
            { 47, "Alarm Clock" },
            { 48, "AppleTV" },
            { 49, "AUX Plate" },
            { 50, "Document Camera Alt" }, // full doccam
            { 51, "Door Station" },
            { 52, "DVR Alt" }, // DVR and remote
            { 53, "Front Door Alt" },
            { 54, "Front Door" },
            { 55, "Jukebox" },
            { 56, "Piano" },
            { 57, "Playstation 3" },
            { 58, "Playstation Logo" },
            { 59, "Room Door" },
            { 60, "SmarTV" },
            { 61, "Sprinkler" },
            { 62, "Tablet" },
            { 63, "TV" }, // TV with remote
            { 64, "VCR" },
            { 65, "Video Conferencing Alt" },
            { 67, "Wii-U Logo" },
            { 69, "Wii" },
            { 70, "Xbox 360" },
            { 71, "Xbox Logo" },
            { 72, "Amenities" },
            { 73, "DirecTV" },
            { 74, "Dish Network" },
            { 75, "Drapes" },
            { 76, "Garage" },
            { 77, "Macros" },
            { 78, "Scheduler" },
            { 79, "Sirius-XM Satellite Radio" },
            { 80, "TiVo" },
            { 81, "Blu-ray" },
            { 82, "DVD" },
            { 83, "Record Player" },
            { 84, "Vudu" },
            { 85, "Home Alt" },
            { 86, "Sirius Satellite Radio" },
            { 87, "Rhapsody" },
            { 88, "Spotify" },
            { 89, "Tunein" },
            { 90, "XM Satellite Radio" },
            { 91, "LastFM" },
            { 92, "You Tube" },
            { 93, "Kaleidescape" },
            { 94, "Hulu" },
            { 95, "Netflix" },
            { 96, "Clapper" },
            { 98, "Web" },
            { 99, "PC" },
            { 100, "Amazon" },
            { 101, "Chrome" },
            { 102, "Blank" },
            { 103, "Fireplace" }
        };

        public static Dictionary<ushort, string> IconsMediaTransportsDict = new Dictionary<ushort, string>()
        {
            { 0, "Alert" },
            { 1, "Audio Note" },
            { 2, "Blu-ray" },
            { 3, "Bolt" },
            { 4, "CD" },
            { 5, "Check" },
            { 6, "Climate" },
            { 7, "Delete" },
            { 8, "Down Alt" },
            { 9, "Down" },
            { 10, "Eject" },
            { 11, "Enter" },
            { 12, "Film" },
            { 13, "Fwd" },
            { 14, "Home" },
            { 15, "Left" },
            { 16, "Left Alt" },
            { 17, "Lights" },
            { 18, "Live" },
            { 19, "Minus" },
            { 20, "Next Page" },
            { 21, "Next" },
            { 22, "Pause" },
            { 23, "Phone" },
            { 24, "Play" },
            { 25, "Play-Pause" },
            { 26, "Plus" },
            { 27, "Power" },
            { 28, "Prev Page" },
            { 29, "Previous" },
            { 30, "Rec" },
            { 31, "Repeat" },
            { 32, "Replay" },
            { 33, "Rew" },
            { 34, "Right Alt" },
            { 35, "Right" },
            { 36, "RSS" },
            { 37, "Shuffle" },
            { 38, "Stop" },
            { 39, "Theatre" },
            { 40, "Thumb Down" },
            { 41, "Thumb Up" },
            { 42, "Triangle" },
            { 43, "Up Alt" },
            { 44, "Up" },
            { 45, "Video Screen" },
            { 46, "Volume Hi" },
            { 47, "Volume Lo" },
            { 48, "Volume Mute" },
            { 49, "Address Book" },
            { 50, "Alarm" },
            { 51, "Calendar" },
            { 52, "Clock" },
            { 53, "Eye" },
            { 54, "Game" },
            { 55, "Gear" },
            { 56, "Globe" },
            { 57, "Help" },
            { 58, "Image" },
            { 59, "Info" },
            { 60, "Keypad" },
            { 61, "Magnifying Glass" },
            { 62, "Mic" },
            { 63, "Phone Down" },
            { 65, "Snow Flake" },
            { 66, "Sun" },
            { 67, "Users" },
            { 68, "Door" },
            { 69, "Drapes" },
            { 70, "Fire" },
            { 71, "iPad" },
            { 72, "iPhone-iPod Touch" },
            { 73, "iPod" },
            { 74, "Mic Mute" },
            { 75, "Padlock Closed" },
            { 76, "Padlock Open" },
            { 77, "Pool" },
            { 78, "Settings" },
            { 79, "Shades" },
            { 80, "Share" },
            { 81, "Shield" },
            { 82, "Slow" },
            { 83, "TV" },
            { 84, "User" },
            { 85, "Wi-Fi" },
            { 86, "Repeat Item" },
            { 87, "Repeat Off" },
            { 88, "Shuffle Item" },
            { 89, "Shuffle Off" },
            { 90, "Song Add" },
            { 91, "Star" },
            { 92, "User Bookmark" },
            { 93, "Play All" },
            { 94, "Play Alt" },
            { 95, "Play Library" },
            { 96, "Play List" },
            { 97, "Weather" },
            { 98, "Projector" },
            { 99, "Camera" },
            { 100, "Download Cloud" },
            { 101, "Radio Signal" },
            { 102, "Satellite" },
            { 103, "Laptop" },
            { 104, "DVD" },
            { 105, "Pen" },
            { 106, "Brush" },
            { 107, "Checkbox Checked" },
            { 108, "Checkbox Off" },
            { 109, "List" },
            { 110, "Android" },
            { 111, "Apple" },
            { 112, "Battery Low" },
            { 113, "Battery Charging" },
            { 114, "Battery Empty" },
            { 115, "Battery Full" },
            { 116, "Bluetooth" },
            { 117, "Brightness" },
            { 118, "Cart" },
            { 119, "Connector Plate" },
            { 120, "Connector" },
            { 121, "Contrast" },
            { 122, "Dashboard" },
            { 123, "Delete Alt" },
            { 124, "Download" },
            { 125, "Garage" },
            { 126, "Graph Alt" },
            { 127, "Graph" },
            { 128, "Grid" },
            { 129, "Guide" },
            { 130, "HD" },
            { 131, "Hot Tub" },
            { 132, "Keyboard" },
            { 133, "Lights Off" },
            { 134, "Lync" },
            { 135, "Media Server" },
            { 136, "Mouse" },
            { 137, "Outlet" },
            { 138, "System" },
            { 139, "Trashcan" },
            { 143, "Video Input" },
            { 144, "Video Output" },
            { 145, "Windows" },
            { 146, "Wireless Device" },
            { 147, "Wrench" },
            { 148, "Stopwatch" },
            { 149, "Comment Check" },
            { 150, "Comment" },
            { 151, "Crestron" },
            { 152, "LastFM" },
            { 153, "Location Minus" },
            { 154, "Location Plus" },
            { 155, "Location" },
            { 156, "Pandora" },
            { 157, "Rhapsody" },
            { 158, "Sirius" },
            { 159, "SiriusXM" },
            { 160, "Spotify" },
            { 161, "User Minus" },
            { 162, "XM" },
            { 163, "User Check" },
            { 164, "Disk" },
            { 165, "Ban" },
            { 166, "Heart" },
            { 167, "DND" },
            { 168, "Eraser" },
            { 169, "Blank" },
            { 170, "Mic Muted" },
            { 171, "Volume Muted" },
            { 172, "Options Off" },
            { 173, "Brightness Medium" },
            { 174, "Brightness Max" },
            { 175, "Folder" },
            { 176, "DND On" },
            { 177, "Options On" },
            { 178, "Network Wi-Fi Off" },
            { 179, "Network Wi-Fi Low" },
            { 180, "Network Wi-Fi Med" },
            { 181, "Network Wi-Fi Max" },
            { 182, "Fireplace" },
            { 183, "More" }
        };

        #endregion
    }

    public enum eBasicColour
    {
        Black   = 0x000000,
        Silver  = 0xC0C0C0,
        Gray    = 0x808080,
        White   = 0xFFFFFF,
        Maroon  = 0x800000,
        Red     = 0xFF0000,
        Purple  = 0x800080,
        Fuchsia = 0xFF00FF,
        Green   = 0x008000,
        Lime    = 0x00FF00,
        Olive   = 0x808000,
        Yellow  = 0xFFFF00,
        Navy    = 0x000080,
        Blue    = 0x0000FF,
        Teal    = 0x008080,
        Aqua    = 0x00FFFF
    };
    public enum eNamedColour
    {
        Black             = 0x000000,
        Navy              = 0x000080,
        DarkBlue          = 0x00008B,
        MediumBlue        = 0x0000CD,
        Blue              = 0x0000FF,
        DarkGreen         = 0x006400,
        Green             = 0x008000,
        Teal              = 0x008080,
        DarkCyan          = 0x008B8B,
        DeepSkyBlue       = 0x00BFFF,
        DarkTurquoise     = 0x00CED1,
        MediumSpringGreen = 0x00FA9A,
        Lime              = 0x00FF00,
        SpringGreen       = 0x00FF7F,
        Aqua              = 0x00FFFF,
        Cyan              = 0x00FFFF,
        MidnightBlue      = 0x191970,
        DodgerBlue        = 0x1E90FF,
        LightSeaGreen     = 0x20B2AA,
        ForestGreen       = 0x228B22,
        SeaGreen          = 0x2E8B57,
        DarkSlateGray     = 0x2F4F4F,
        LimeGreen         = 0x32CD32,
        MediumSeaGreen    = 0x3CB371,
        Turquoise         = 0x40E0D0,
        RoyalBlue         = 0x4169E1,
        SteelBlue         = 0x4682B4,
        DarkSlateBlue     = 0x483D8B,
        MediumTurquoise   = 0x48D1CC,
        Indigo            = 0x4B0082,
        DarkOliveGreen    = 0x556B2F,
        CadetBlue         = 0x5F9EA0,
        CornflowerBlue    = 0x6495ED,
        MediumAquaMarine  = 0x66CDAA,
        DimGray           = 0x696969,
        SlateBlue         = 0x6A5ACD,
        OliveDrab         = 0x6B8E23,
        SlateGray         = 0x708090,
        LightSlateGray    = 0x778899,
        MediumSlateBlue   = 0x7B68EE,
        LawnGreen         = 0x7CFC00,
        Chartreuse        = 0x7FFF00,
        Aquamarine        = 0x7FFFD4,
        Maroon            = 0x800000,
        Purple            = 0x800080,
        Olive             = 0x808000,
        Gray              = 0x808080,
        SkyBlue           = 0x87CEEB,
        LightSkyBlue      = 0x87CEFA,
        BlueViolet        = 0x8A2BE2,
        DarkRed           = 0x8B0000,
        DarkMagenta       = 0x8B008B,
        SaddleBrown       = 0x8B4513,
        DarkSeaGreen      = 0x8FBC8F,
        LightGreen        = 0x90EE90,
        MediumPurple      = 0x9370D8,
        DarkViolet        = 0x9400D3,
        PaleGreen         = 0x98FB98,
        DarkOrchid        = 0x9932CC,
        YellowGreen       = 0x9ACD32,
        Sienna            = 0xA0522D,
        Brown             = 0xA52A2A,
        DarkGray          = 0xA9A9A9,
        LightBlue         = 0xADD8E6,
        GreenYellow       = 0xADFF2F,
        PaleTurquoise     = 0xAFEEEE,
        LightSteelBlue    = 0xB0C4DE,
        PowderBlue        = 0xB0E0E6,
        FireBrick         = 0xB22222,
        DarkGoldenRod     = 0xB8860B,
        MediumOrchid      = 0xBA55D3,
        RosyBrown         = 0xBC8F8F,
        DarkKhaki         = 0xBDB76B,
        Silver            = 0xC0C0C0,
        MediumVioletRed   = 0xC71585,
        IndianRed         = 0xCD5C5C,
        Peru              = 0xCD853F,
        Chocolate         = 0xD2691E,
        Tan               = 0xD2B48C,
        LightGrey         = 0xD3D3D3,
        PaleVioletRed     = 0xD87093,
        Thistle           = 0xD8BFD8,
        Orchid            = 0xDA70D6,
        GoldenRod         = 0xDAA520,
        Crimson           = 0xDC143C,
        Gainsboro         = 0xDCDCDC,
        Plum              = 0xDDA0DD,
        BurlyWood         = 0xDEB887,
        LightCyan         = 0xE0FFFF,
        Lavender          = 0xE6E6FA,
        DarkSalmon        = 0xE9967A,
        Violet            = 0xEE82EE,
        PaleGoldenRod     = 0xEEE8AA,
        LightCoral        = 0xF08080,
        Khaki             = 0xF0E68C,
        AliceBlue         = 0xF0F8FF,
        HoneyDew          = 0xF0FFF0,
        Azure             = 0xF0FFFF,
        SandyBrown        = 0xF4A460,
        Wheat             = 0xF5DEB3,
        Beige             = 0xF5F5DC,
        WhiteSmoke        = 0xF5F5F5,
        MintCream         = 0xF5FFFA,
        GhostWhite        = 0xF8F8FF,
        Salmon            = 0xFA8072,
        AntiqueWhite      = 0xFAEBD7,
        Linen             = 0xFAF0E6,
        LightGoldenRodYellow = 0xFAFAD2,
        OldLace           = 0xFDF5E6,
        Red               = 0xFF0000,
        Fuchsia           = 0xFF00FF,
        Magenta           = 0xFF00FF,
        DeepPink          = 0xFF1493,
        OrangeRed         = 0xFF4500,
        Tomato            = 0xFF6347,
        HotPink           = 0xFF69B4,
        Coral             = 0xFF7F50,
        Darkorange        = 0xFF8C00,
        LightSalmon       = 0xFFA07A,
        Orange            = 0xFFA500,
        LightPink         = 0xFFB6C1,
        Pink              = 0xFFC0CB,
        Gold              = 0xFFD700,
        PeachPuff         = 0xFFDAB9,
        NavajoWhite       = 0xFFDEAD,
        Moccasin          = 0xFFE4B5,
        Bisque            = 0xFFE4C4,
        MistyRose         = 0xFFE4E1,
        BlanchedAlmond    = 0xFFEBCD,
        PapayaWhip        = 0xFFEFD5,
        LavenderBlush     = 0xFFF0F5,
        SeaShell          = 0xFFF5EE,
        Cornsilk          = 0xFFF8DC,
        LemonChiffon      = 0xFFFACD,
        FloralWhite       = 0xFFFAF0,
        Snow              = 0xFFFAFA,
        Yellow            = 0xFFFF00,
        LightYellow       = 0xFFFFE0,
        Ivory             = 0xFFFFF0,
        White             = 0xFFFFFF
    };

    public enum eCrestronFont
    {
        Arial,
        Crestron_Sans_Pro,
        Crestron_AV,
        Crestron_Simple_Icons
    };

    public enum eCrestronIconType
    {
        None = 0,
        Font = 1,
        AnalogIconsLg = 2,
        SerialIconsLg = 3,
        AnalogIconsMediaTransports = 4,
        SerialIconsMediaTransports = 5
    };

    public enum eIconsLg 
    {
        AM_FM                   = 0,
        CD                      = 1,
        Climate                 = 2,
        DisplayAlt              = 3, // black LCD monitor
        Display                 = 4, // blue LCD monitor
        DVR                     = 5, // DVR text on red btn
        EnergyManagement        = 6,
        Favorites               = 7,
        FilmReel                = 8,
        Home                    = 9,
        InternetRadio           = 10, 
        iPod                    = 11, 
        iServer                 = 12, 
        Lights                  = 13, 
        MusicNote               = 14, 
        News                    = 15, 
        Pandora                 = 16, 
        Power                   = 17, 
        SatelliteAlt            = 18, 
        Satellite               = 19, 
        Sec_Cam                 = 20, // black on pivot mount
        Security                = 21, 
        Shades                  = 22, 
        UserGroup               = 23, 
        VideoConferencing       = 24, 
        VideoSwitcher           = 25, 
        Wand                    = 26, 
        Weather                 = 27, 
        Speaker                 = 29, 
        Mic                     = 30, 
        Projector               = 31, 
        Screen                  = 32, 
        Gear                    = 33, 
        Sec_CamAlt              = 34, // white no PTZ 
        DocumentCamera          = 35,  // lens over paper
        Backgrounds             = 36, 
        Gamepad                 = 37, 
        iMac                    = 38, 
        LaptopAlt               = 39, 
        Laptop                  = 40, 
        MacBookPro              = 41, 
        MusicNoteAlt            = 42, 
        PhoneAlt                = 43, 
        Phone                   = 44, 
        Pool                    = 45, 
        Airplay                 = 46, 
        AlarmClock              = 47, 
        AppleTV                 = 48, 
        AUXPlate                = 49, 
        DocumentCameraAlt       = 50,  // full doccam
        DoorStation             = 51, 
        DVRAlt                  = 52,  // DVR and remote
        FrontDoorAlt            = 53, 
        FrontDoor               = 54, 
        Jukebox                 = 55, 
        Piano                   = 56, 
        Playstation3            = 57, 
        PlaystationLogo         = 58, 
        RoomDoor                = 59, 
        SmarTV                  = 60, 
        Sprinkler               = 61, 
        Tablet                  = 62, 
        TV                      = 63,  // TV with remote
        VCR                     = 64, 
        VideoConferencingAlt    = 65, 
        Wii_ULogo               = 67, 
        Wii                     = 69, 
        Xbox360                 = 70, 
        XboxLogo                = 71, 
        Amenities               = 72, 
        DirecTV                 = 73, 
        DishNetwork             = 74, 
        Drapes                  = 75, 
        Garage                  = 76, 
        Macros                  = 77, 
        Scheduler               = 78, 
        Sirius_XMSatelliteRadio = 79, 
        TiVo                    = 80, 
        Blu_ray                 = 81, 
        DVD                     = 82, 
        RecordPlayer            = 83, 
        Vudu                    = 84, 
        HomeAlt                 = 85, 
        SiriusSatelliteRadio    = 86, 
        Rhapsody                = 87, 
        Spotify                 = 88, 
        Tunein                  = 89, 
        XMSatelliteRadio        = 90, 
        LastFM                  = 91, 
        YouTube                 = 92, 
        Kaleidescape            = 93, 
        Hulu                    = 94, 
        Netflix                 = 95, 
        Clapper                 = 96, 
        Web                     = 98, 
        PC                      = 99,
        Amazon                  = 100,
        Chrome                  = 101,
        Blank                   = 102,
        Fireplace               = 103
    };
    public enum eIconsMediaTransports
    {
        Alert            = 0,
        AudioNote        = 1,
        Blu_ray          = 2,
        Bolt             = 3,
        CD               = 4,
        Check            = 5,
        Climate          = 6,
        Delete           = 7,
        DownAlt          = 8,
        Down             = 9,
        Eject            = 10,
        Enter            = 11,
        Film             = 12,
        Fwd              = 13,
        Home             = 14,
        Left             = 15,
        LeftAlt          = 16,
        Lights           = 17,
        Live             = 18,
        Minus            = 19,
        NextPage         = 20,
        Next             = 21,
        Pause            = 22,
        Phone            = 23,
        Play             = 24,
        Play_Pause       = 25,
        Plus             = 26,
        Power            = 27,
        PrevPage         = 28,
        Previous         = 29,
        Rec              = 30,
        Repeat           = 31,
        Replay           = 32,
        Rew              = 33,
        RightAlt         = 34,
        Right            = 35,
        RSS              = 36,
        Shuffle          = 37,
        Stop             = 38,
        Theatre          = 39,
        ThumbDown        = 40,
        ThumbUp          = 41,
        Triangle         = 42,
        UpAlt            = 43,
        Up               = 44,
        VideoScreen      = 45,
        VolumeHi         = 46,
        VolumeLo         = 47,
        VolumeMute       = 48,
        AddressBook      = 49,
        Alarm            = 50,
        Calendar         = 51,
        Clock            = 52,
        Eye              = 53,
        Game             = 54,
        Gear             = 55,
        Globe            = 56,
        Help             = 57,
        Image            = 58,
        Info             = 59,
        Keypad           = 60,
        MagnifyingGlass  = 61,
        Mic              = 62,
        PhoneDown        = 63,
        SnowFlake        = 65,
        Sun              = 66,
        Users            = 67,
        Door             = 68,
        Drapes           = 69,
        Fire             = 70,
        iPad             = 71,
        iPhone_iPodTouch = 72,
        iPod             = 73,
        MicMute          = 74,
        PadlockClosed    = 75,
        PadlockOpen      = 76,
        Pool             = 77,
        Settings         = 78,
        Shades           = 79,
        Share            = 80,
        Shield           = 81,
        Slow             = 82,
        TV               = 83,
        User             = 84,
        Wi_Fi            = 85,
        RepeatItem       = 86,
        RepeatOff        = 87,
        ShuffleItem      = 88,
        ShuffleOff       = 89,
        SongAdd          = 90,
        Star             = 91,
        UserBookmark     = 92,
        PlayAll          = 93,
        PlayAlt          = 94,
        PlayLibrary      = 95,
        PlayList         = 96,
        Weather          = 97,
        Projector        = 98,
        Camera           = 99,
        DownloadCloud    = 100,
        RadioSignal      = 101,
        Satellite        = 102,
        Laptop           = 103,
        DVD              = 104,
        Pen              = 105,
        Brush            = 106,
        CheckboxChecked  = 107,
        CheckboxOff      = 108,
        List             = 109,
        Android          = 110,
        Apple            = 111,
        BatteryLow       = 112,
        BatteryCharging  = 113,
        BatteryEmpty     = 114,
        BatteryFull      = 115,
        Bluetooth        = 116,
        Brightness       = 117,
        Cart             = 118,
        ConnectorPlate   = 119,
        Connector        = 120,
        Contrast         = 121,
        Dashboard        = 122,
        DeleteAlt        = 123,
        Download         = 124,
        Garage           = 125,
        GraphAlt         = 126,
        Graph            = 127,
        Grid             = 128,
        Guide            = 129,
        HD               = 130,
        HotTub           = 131,
        Keyboard         = 132,
        LightsOff        = 133,
        Lync             = 134,
        MediaServer      = 135,
        Mouse            = 136,
        Outlet           = 137,
        System           = 138,
        Trashcan         = 139,
        VideoInput       = 143,
        VideoOutput      = 144,
        Windows          = 145,
        WirelessDevice   = 146,
        Wrench           = 147,
        Stopwatch        = 148,
        CommentCheck     = 149,
        Comment          = 150,
        Crestron         = 151,
        LastFM           = 152,
        LocationMinus    = 153,
        LocationPlus     = 154,
        Location         = 155,
        Pandora          = 156,
        Rhapsody         = 157,
        Sirius           = 158,
        SiriusXM         = 159,
        Spotify          = 160,
        UserMinus        = 161,
        XM               = 162,
        UserCheck        = 163,
        Disk             = 164,
        Ban              = 165,
        Heart            = 166,
        DND              = 167,
        Eraser           = 168,
        Blank            = 169,
        MicMuted         = 170,
        VolumeMuted      = 171,
        OptionsOff       = 172,
        BrightnessMedium = 173,
        BrightnessMax    = 174,
        Folder           = 175,
        DNDOn            = 176,
        OptionsOn        = 177,
        NetworkWi_FiOff  = 178,
        NetworkWi_FiLow  = 179,
        NetworkWi_FiMed  = 180,
        NetworkWi_FiMax  = 181,
        Fireplace        = 182,
        More             = 183
    };
}
