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
        
        public static string CreateAsciiPrintableString(byte[] bArgs)
        {
            string strOut_ = "";
            byte[] b = Encoding.Default.GetBytes(" ~");
            foreach (byte bIndex_ in bArgs)
            {
                if (bIndex_ >= b[0] && bIndex_ <= b[1]) // ASCII
                {
                    //strOut_ += String.Format("{0,4}", bIndex_);
                    strOut_ += Encoding.ASCII.GetString(new byte[] { bIndex_ });
                }
                else
                {
                    string sHexOutput_ = String.Format("{0:X}", bIndex_);
                    strOut_ += @"\x" + String.Format("{0:X}", sHexOutput_).PadLeft(2, '0');
                }
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
        public static byte[] GetBytes(string str) // because no encodings work the way we need
        {
            //CrestronConsole.PrintLine("GetBytes {0}", str);
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
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
            String p1 = @"(\\[xX][0-9a-fA-F]{2}|.)";
            //String p2 = @"\\x([xX][0-9a-fA-F]{2})";
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
                    s1 = s1 + Encoding.Default.GetString(new byte[] { b2 });
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
            int r = val == true ? b | (1 << bitNumber) : b & ~(1 << bitNumber); ;
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
        public static int Atoi(string strArg) // "hello 123 there" returns 123, because ToInt throws exceptions when non numbers are inserted
        {
            String m = Regex.Match(strArg, @"\d+").Value;
            return (m.Length == 0 ? 0 : Convert.ToInt32(m));
        }
        public static int ConvertRanges(int val, int inMin, int inMax, int outMin, int outMax)
        {
            int inRange = inMax - inMin;
            int outRange = outMax - outMin;
            int result = ((val - inMin) * outRange) / inRange + outMin;
            return result;
        }
 
        public static string FormatTextForUi(string text, ushort fontSize, eCrestronFont font, eNamedColour colour)
        {
            if (fontSize == 0)
                fontSize = defaultfontSize;
            string str = String.Format("<FONT size=\x22{0}\x22 face=\x22{1}\x22 color=\x22#{2}\x22>{3}</FONT>", fontSize, font, NamedColour[colour], text);
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
            { 43, "Phone Alt" },
            { 44, "Phone" },
            { 42, "Music Note Alt" },
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

        #region colour dictionaries

        // made BasicColour private instead of deleting it
        private enum eBasicColour
        {
            Black,
            Silver,
            Gray,
            White,
            Maroon,
            Red,
            Purple,
            Fuchsia,
            Green,
            Lime,
            Olive,
            Yellow,
            Navy,
            Blue,
            Teal,
            Aqua
        };
        private static Dictionary<eBasicColour, string> Html4Colour = new Dictionary<eBasicColour, string>()
        {
            { eBasicColour.Black  , "000000" },
            { eBasicColour.Silver , "C0C0C0" },
            { eBasicColour.Gray   , "808080" },
            { eBasicColour.White  , "FFFFFF" },
            { eBasicColour.Maroon , "800000" },
            { eBasicColour.Red    , "FF0000" },
            { eBasicColour.Purple , "800080" },
            { eBasicColour.Fuchsia, "FF00FF" },
            { eBasicColour.Green  , "008000" },
            { eBasicColour.Lime   , "00FF00" },
            { eBasicColour.Olive  , "808000" },
            { eBasicColour.Yellow , "FFFF00" },
            { eBasicColour.Navy   , "000080" },
            { eBasicColour.Blue   , "0000FF" },
            { eBasicColour.Teal   , "008080" },
            { eBasicColour.Aqua   , "00FFFF" }
        };

        public enum eNamedColour
        {
            Black,
            Navy,
            DarkBlue,
            MediumBlue,
            Blue,
            DarkGreen,
            Green,
            Teal,
            DarkCyan,
            DeepSkyBlue,
            DarkTurquoise,
            MediumSpringGreen,
            Lime,
            SpringGreen,
            Aqua,
            Cyan,
            MidnightBlue,
            DodgerBlue,
            LightSeaGreen,
            ForestGreen,
            SeaGreen,
            DarkSlateGray,
            LimeGreen,
            MediumSeaGreen,
            Turquoise,
            RoyalBlue,
            SteelBlue,
            DarkSlateBlue,
            MediumTurquoise,
            Indigo,
            DarkOliveGreen,
            CadetBlue,
            CornflowerBlue,
            MediumAquaMarine,
            DimGray,
            SlateBlue,
            OliveDrab,
            SlateGray,
            LightSlateGray,
            MediumSlateBlue,
            LawnGreen,
            Chartreuse,
            Aquamarine,
            Maroon,
            Purple,
            Olive,
            Gray,
            SkyBlue,
            LightSkyBlue,
            BlueViolet,
            DarkRed,
            DarkMagenta,
            SaddleBrown,
            DarkSeaGreen,
            LightGreen,
            MediumPurple,
            DarkViolet,
            PaleGreen,
            DarkOrchid,
            YellowGreen,
            Sienna,
            Brown,
            DarkGray,
            LightBlue,
            GreenYellow,
            PaleTurquoise,
            LightSteelBlue,
            PowderBlue,
            FireBrick,
            DarkGoldenRod,
            MediumOrchid,
            RosyBrown,
            DarkKhaki,
            Silver,
            MediumVioletRed,
            IndianRed,
            Peru,
            Chocolate,
            Tan,
            LightGrey,
            PaleVioletRed,
            Thistle,
            Orchid,
            GoldenRod,
            Crimson,
            Gainsboro,
            Plum,
            BurlyWood,
            LightCyan,
            Lavender,
            DarkSalmon,
            Violet,
            PaleGoldenRod,
            LightCoral,
            Khaki,
            AliceBlue,
            HoneyDew,
            Azure,
            SandyBrown,
            Wheat,
            Beige,
            WhiteSmoke,
            MintCream,
            GhostWhite,
            Salmon,
            AntiqueWhite,
            Linen,
            LightGoldenRodYellow,
            OldLace,
            Red,
            Fuchsia,
            Magenta,
            DeepPink,
            OrangeRed,
            Tomato,
            HotPink,
            Coral,
            Darkorange,
            LightSalmon,
            Orange,
            LightPink,
            Pink,
            Gold,
            PeachPuff,
            NavajoWhite,
            Moccasin,
            Bisque,
            MistyRose,
            BlanchedAlmond,
            PapayaWhip,
            LavenderBlush,
            SeaShell,
            Cornsilk,
            LemonChiffon,
            FloralWhite,
            Snow,
            Yellow,
            LightYellow,
            Ivory,
            White,
        };
        public static Dictionary<eNamedColour, string> NamedColour = new Dictionary<eNamedColour, string>()
        {
            { eNamedColour.Black	        , "000000" },
            { eNamedColour.Navy	            , "000080" },
            { eNamedColour.DarkBlue	        , "00008B" },
            { eNamedColour.MediumBlue	    , "0000CD" },
            { eNamedColour.Blue	            , "0000FF" },
            { eNamedColour.DarkGreen	    , "006400" },
            { eNamedColour.Green	        , "008000" },
            { eNamedColour.Teal	            , "008080" },
            { eNamedColour.DarkCyan	        , "008B8B" },
            { eNamedColour.DeepSkyBlue      , "00BFFF" },
            { eNamedColour.DarkTurquoise	, "00CED1" },
            { eNamedColour.MediumSpringGreen, "00FA9A" },
            { eNamedColour.Lime	            , "00FF00" },	
            { eNamedColour.SpringGreen	    , "00FF7F" },	
            { eNamedColour.Aqua	            , "00FFFF" },	
            { eNamedColour.Cyan	            , "00FFFF" },	
            { eNamedColour.MidnightBlue	    , "191970" },	
            { eNamedColour.DodgerBlue	    , "1E90FF" },	
            { eNamedColour.LightSeaGreen	, "20B2AA" },	
            { eNamedColour.ForestGreen	    , "228B22" },	
            { eNamedColour.SeaGreen	        , "2E8B57" },	
            { eNamedColour.DarkSlateGray	, "2F4F4F" },	
            { eNamedColour.LimeGreen	    , "32CD32" },	
            { eNamedColour.MediumSeaGreen	, "3CB371" },	
            { eNamedColour.Turquoise	    , "40E0D0" },	
            { eNamedColour.RoyalBlue	    , "4169E1" },	
            { eNamedColour.SteelBlue	    , "4682B4" },	
            { eNamedColour.DarkSlateBlue	, "483D8B" },	
            { eNamedColour.MediumTurquoise  , "48D1CC" },	
            { eNamedColour.Indigo	        , "4B0082" },	
            { eNamedColour.DarkOliveGreen	, "556B2F" },	
            { eNamedColour.CadetBlue	    , "5F9EA0" },	
            { eNamedColour.CornflowerBlue	, "6495ED" },	
            { eNamedColour.MediumAquaMarine , "66CDAA" },	
            { eNamedColour.DimGray	        , "696969" },	
            { eNamedColour.SlateBlue	    , "6A5ACD" },	
            { eNamedColour.OliveDrab	    , "6B8E23" },	
            { eNamedColour.SlateGray	    , "708090" },	
            { eNamedColour.LightSlateGray	, "778899" },	
            { eNamedColour.MediumSlateBlue  , "7B68EE" },	
            { eNamedColour.LawnGreen	    , "7CFC00" },	
            { eNamedColour.Chartreuse	    , "7FFF00" },	
            { eNamedColour.Aquamarine	    , "7FFFD4" },	
            { eNamedColour.Maroon	        , "800000" },	
            { eNamedColour.Purple	        , "800080" },	
            { eNamedColour.Olive	        , "808000" },	
            { eNamedColour.Gray	            , "808080" },	
            { eNamedColour.SkyBlue	        , "87CEEB" },	
            { eNamedColour.LightSkyBlue	    , "87CEFA" },	
            { eNamedColour.BlueViolet	    , "8A2BE2" },	
            { eNamedColour.DarkRed	        , "8B0000" },	
            { eNamedColour.DarkMagenta	    , "8B008B" },	
            { eNamedColour.SaddleBrown	    , "8B4513" },	
            { eNamedColour.DarkSeaGreen	    , "8FBC8F" },	
            { eNamedColour.LightGreen	    , "90EE90" },	
            { eNamedColour.MediumPurple	    , "9370D8" },	
            { eNamedColour.DarkViolet	    , "9400D3" },	
            { eNamedColour.PaleGreen	    , "98FB98" },	
            { eNamedColour.DarkOrchid	    , "9932CC" },	
            { eNamedColour.YellowGreen	    , "9ACD32" },	
            { eNamedColour.Sienna	        , "A0522D" },	
            { eNamedColour.Brown	        , "A52A2A" },	
            { eNamedColour.DarkGray	        , "A9A9A9" },	
            { eNamedColour.LightBlue	    , "ADD8E6" },	
            { eNamedColour.GreenYellow	    , "ADFF2F" },	
            { eNamedColour.PaleTurquoise	, "AFEEEE" },	
            { eNamedColour.LightSteelBlue	, "B0C4DE" },	
            { eNamedColour.PowderBlue	    , "B0E0E6" },	
            { eNamedColour.FireBrick	    , "B22222" },	
            { eNamedColour.DarkGoldenRod	, "B8860B" },	
            { eNamedColour.MediumOrchid	    , "BA55D3" },	
            { eNamedColour.RosyBrown	    , "BC8F8F" },	
            { eNamedColour.DarkKhaki	    , "BDB76B" },	
            { eNamedColour.Silver	        , "C0C0C0" },	
            { eNamedColour.MediumVioletRed  , "C71585" },	
            { eNamedColour.IndianRed	    , "CD5C5C" },	
            { eNamedColour.Peru	            , "CD853F" },	
            { eNamedColour.Chocolate	    , "D2691E" },	
            { eNamedColour.Tan	            , "D2B48C" },	
            { eNamedColour.LightGrey	    , "D3D3D3" },	
            { eNamedColour.PaleVioletRed	, "D87093" },	
            { eNamedColour.Thistle	        , "D8BFD8" },	
            { eNamedColour.Orchid	        , "DA70D6" },	
            { eNamedColour.GoldenRod	    , "DAA520" },	
            { eNamedColour.Crimson	        , "DC143C" },	
            { eNamedColour.Gainsboro	    , "DCDCDC" },	
            { eNamedColour.Plum	            , "DDA0DD" },	
            { eNamedColour.BurlyWood	    , "DEB887" },	
            { eNamedColour.LightCyan	    , "E0FFFF" },	
            { eNamedColour.Lavender	        , "E6E6FA" },	
            { eNamedColour.DarkSalmon	    , "E9967A" },	
            { eNamedColour.Violet	        , "EE82EE" },	
            { eNamedColour.PaleGoldenRod	, "EEE8AA" },	
            { eNamedColour.LightCoral	    , "F08080" },	
            { eNamedColour.Khaki	        , "F0E68C" },	
            { eNamedColour.AliceBlue	    , "F0F8FF" },	
            { eNamedColour.HoneyDew	        , "F0FFF0" },	
            { eNamedColour.Azure	        , "F0FFFF" },	
            { eNamedColour.SandyBrown	    , "F4A460" },	
            { eNamedColour.Wheat	        , "F5DEB3" },	
            { eNamedColour.Beige	        , "F5F5DC" },	
            { eNamedColour.WhiteSmoke	    , "F5F5F5" },	
            { eNamedColour.MintCream	    , "F5FFFA" },	
            { eNamedColour.GhostWhite	    , "F8F8FF" },	
            { eNamedColour.Salmon	        , "FA8072" },	
            { eNamedColour.AntiqueWhite	    , "FAEBD7" },	
            { eNamedColour.Linen	        , "FAF0E6" },	
            { eNamedColour.LightGoldenRodYellow,"FAFAD2" }, 	
            { eNamedColour.OldLace	        , "FDF5E6" },	
            { eNamedColour.Red	            , "FF0000" },	
            { eNamedColour.Fuchsia	        , "FF00FF" },	
            { eNamedColour.Magenta	        , "FF00FF" },	
            { eNamedColour.DeepPink	        , "FF1493" },	
            { eNamedColour.OrangeRed	    , "FF4500" },	
            { eNamedColour.Tomato	        , "FF6347" },	
            { eNamedColour.HotPink	        , "FF69B4" },	
            { eNamedColour.Coral	        , "FF7F50" },	
            { eNamedColour.Darkorange	    , "FF8C00" },	
            { eNamedColour.LightSalmon	    , "FFA07A" },	
            { eNamedColour.Orange	        , "FFA500" },	
            { eNamedColour.LightPink	    , "FFB6C1" },	
            { eNamedColour.Pink	            , "FFC0CB" },	
            { eNamedColour.Gold	            , "FFD700" },	
            { eNamedColour.PeachPuff	    , "FFDAB9" },	
            { eNamedColour.NavajoWhite	    , "FFDEAD" },	
            { eNamedColour.Moccasin	        , "FFE4B5" },	
            { eNamedColour.Bisque	        , "FFE4C4" },	
            { eNamedColour.MistyRose	    , "FFE4E1" },	
            { eNamedColour.BlanchedAlmond	, "FFEBCD" },	
            { eNamedColour.PapayaWhip	    , "FFEFD5" },	
            { eNamedColour.LavenderBlush	, "FFF0F5" },	
            { eNamedColour.SeaShell	        , "FFF5EE" },	
            { eNamedColour.Cornsilk	        , "FFF8DC" },	
            { eNamedColour.LemonChiffon	    , "FFFACD" },	
            { eNamedColour.FloralWhite 	    , "FFFAF0" },	
            { eNamedColour.Snow	            , "FFFAFA" },	
            { eNamedColour.Yellow	        , "FFFF00" },	
            { eNamedColour.LightYellow      , "FFFFE0" },	
            { eNamedColour.Ivory	        , "FFFFF0" },	
            { eNamedColour.White	        , "FFFFFF" }
        };

        #endregion

        #region font dictionary

        public enum eCrestronFont
        {
            Arial,
            Crestron_Sans_Pro,
            Crestron_AV,
        };
        public static Dictionary<eCrestronFont, string> CrestronFonts = new Dictionary<eCrestronFont, string>()
        {
            { eCrestronFont.Arial               , "Arial" },
            { eCrestronFont.Crestron_Sans_Pro   , "Crestron Sans Pro" },
            { eCrestronFont.Crestron_AV         , "Crestron AV" }
        };

        #endregion

    }
}
