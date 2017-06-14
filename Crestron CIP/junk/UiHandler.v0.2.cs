// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="UIHandler.cs" company="AVPlus Integration Pty Ltd">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Drawing;

    class UiPage
    {
        public ushort join;
        public string name;
        public string label;
        public UiPage parentPage;
        public UiPage lastPage;
        public Dictionary<ushort, UiPage> MainList = new Dictionary<ushort, UiPage>();
        public Dictionary<ushort, UiPage> TopList = new Dictionary<ushort, UiPage>();
        public Dictionary<ushort, UiPage> Links = new Dictionary<ushort, UiPage>();
        public UiPage()
        {
            //this.name = name;
        }

        public void SetMainList(Dictionary<ushort, UiPage> pages)
        {
            this.MainList = pages;
        }
    }

    class UiHandler
    {
        string strJson;

        Crestron_CIP_Server parent;
        Dictionary<string, UiPage> PageList = new Dictionary<string, UiPage>();
        Dictionary<byte, string> CurrentList = new Dictionary<byte, string>();

        UiPage uiTree;

        Dictionary<ushort, string> pages = new Dictionary<ushort, string>();
        Dictionary<byte, string> OperatingModes = new Dictionary<byte, string>();
        Dictionary<byte, string> AdminModes = new Dictionary<byte, string>();
        Dictionary<byte, string> MenuModes = new Dictionary<byte, string>();
        Dictionary<byte, string> PinModes = new Dictionary<byte, string>();
        Dictionary<byte, string> NamingModes = new Dictionary<byte, string>();
        Dictionary<byte, string> CctvModes = new Dictionary<byte, string>();
        Dictionary<byte, string> AvModes = new Dictionary<byte, string>();
        Dictionary<byte, string> ZoneMuteModes = new Dictionary<byte, string>();
        Dictionary<byte, string> FoyerModes = new Dictionary<byte, string>();
        Dictionary<byte, string> ShutDownModes = new Dictionary<byte, string>();

        const ushort PAGE_LOGIN = 10;
        const ushort PAGE_MAIN  = 11;

        const byte SMART_KEYPAD = 1;
        const byte SMART_TOPMENU = 2;
        const byte SMART_LIST = 3;

        const byte SMART_IDX_SET_LIST_SIZE = 3;

        const ushort DIG_LIST_VISIBLE      = 6;
        const ushort DIG_FBS_RESET         = 7;
        const ushort DIG_FBS_RESET_VISIBLE = 7;
        const ushort DIG_FBS_RESET_CANCEL  = 8;

        const ushort DIG_SUB_PASSWORD      = 22;
        const ushort DIG_SUB_CONFIRM       = 23;
        const ushort DIG_SUB_CLEAN_COUNT   = 24;
        const ushort DIG_SUB_BAR           = 25;
        const ushort DIG_SUB_BLURAY       = 26;

        const ushort DIG_START    = 50;
        const ushort DIG_CANCEL   = 51;
        const ushort DIG_CLEANING = 52;
        const ushort DIG_CONFIRM  = 53;

        const ushort DIG_HOME     = 60;
        const ushort DIG_BACK     = 61;

        const ushort ANA_CLEAN_TIME = 1;
        const ushort ANA_BAR        = 3;

        const ushort SER_KEYPAD        = 1;
        const ushort SER_TITLE         = 2;
        const ushort SER_INSTRUCT      = 3;
        const ushort SER_SUBTITLE      = 4;
        const ushort SER_CONFIRM_TITLE = 5;
        const ushort SER_CONFIRM_MSG   = 6;

        const byte FONT_SIZE_TITLE    = 40;
        const byte FONT_SIZE_INSTRUCT = 40;
        const byte FONT_SIZE_LIST     = 40;

        const byte MENU_SEL_1        = 11;  // idx start at 11
        const byte MENU_SEL_ADMIN    = 12;
        const byte MENU_SEL_LOGOUT   = 13;
        const byte MENU_SEL_SHUTDOWN = 14;

        const byte SMART_LIST_1 = 11;  // idx start at 11
        const byte SMART_LIST_2 = 12;
        const byte SMART_LIST_3 = 13;
        const byte SMART_LIST_4 = 14;

        const byte OP_MODE_REHEARSE = 1;
        const byte OP_MODE_PRESENT = 2;
        const byte OP_MODE_OPERATE = 3;

        const byte PAGE_FUNC_LOGIN = 1;

        const int COLOUR_TITLE = 0x004080;  // dark blue
        const int COLOUR_INSTRUCT = 0x004080;
        const string COLOUR_LIST = "white";

        private string CurrentTitle;
        private string CurrentConfirm;

        private string passwordCurrent = "1234";
        private string passwordEntered;

        private byte cleanTotalMinutes = 4;
        private byte cleanCurrentMinutes;

        public UiHandler(Crestron_CIP_Server parent)
        {
            this.parent = parent;

            // create all pages
            PageList.Add("Splash"          , new UiPage());
            PageList.Add("Login"           , new UiPage());
            PageList.Add("Cleaning"        , new UiPage());
            PageList.Add("Modes"           , new UiPage());
            PageList.Add("Menu"            , new UiPage());
            PageList.Add("User PINS"       , new UiPage());
            PageList.Add("Naming"          , new UiPage());
            PageList.Add("FBS Reset"       , new UiPage());
            PageList.Add("Admin"           , new UiPage());
            PageList.Add("CCTV"            , new UiPage());
            PageList.Add("Video"           , new UiPage());
            PageList.Add("AV"              , new UiPage());
            PageList.Add("Inputs"          , new UiPage());
            PageList.Add("Zones"           , new UiPage());
            PageList.Add("Stage Management", new UiPage());
            PageList.Add("Letern"          , new UiPage());
            PageList.Add("Lighting"        , new UiPage());
            PageList.Add("Foyer"           , new UiPage());
            PageList.Add("Operator"        , new UiPage());
            PageList.Add("User"            , new UiPage());
            PageList.Add("Guest"           , new UiPage());
            PageList.Add("Outputs"         , new UiPage());
            PageList.Add("Pages"           , new UiPage());
            PageList.Add("DVD"             , new UiPage());
            PageList.Add("Main"            , new UiPage());
            PageList.Add("Gallery"         , new UiPage());
            PageList.Add("Front Fill"      , new UiPage());
            PageList.Add("Audio"           , new UiPage());
            PageList.Add("Shutdown"        , new UiPage());
            PageList.Add("CCTV Presets"    , new UiPage());
            PageList.Add("CCTV Routing"    , new UiPage());
            PageList.Add("CCTV Matrix"     , new UiPage());
            PageList.Add("AV Presets"      , new UiPage());
            PageList.Add("AV Routing"      , new UiPage());
            PageList.Add("AV Matrix"       , new UiPage());
            // add page references (should be using if(PageList.ContainsKey("Main")))
/*
            PageList["Modes"].parentPage = PageList["Splash"];
            string[] modeNames = { "Reheasre", "Present", "Operate" };
            for (byte i = 1; i <= modeNames.Length; i++)
            {
                UiPage p = new UiPage();
                p.name = String.Format("Mode {0}", i+1);
                p.label = modeNames[i + 1];
                p.parentPage = PageList["Modes"];
                PageList.Add(p.name, p);
                PageList["Modes"].MainList.Add(i, p);
            }
            PageList["Modes"].Links.Add(1, PageList["Admin"]);
            PageList["Modes"].Links.Add(2, PageList["Shutdown"]);

            PageList["Splash"].Links.Add(1, PageList["Login"]);

            PageList["Login"].parentPage = PageList["Splash"];
            PageList["Login"].Links.Add(1, PageList["Modes"]);
            PageList["Login"].Links.Add(2, PageList["Cleaning"]);

            PageList["Cleaning"].parentPage = PageList["Splash"];
            PageList["Cleaning"].Links.Add(1, PageList["Splash"]);

            PageList["Shutdown"].parentPage = PageList["Splash"];
            string[] areaNames = { "Stage", "Auditorium", "Bio", "Paging", "Foyer" };
            for (byte i = 1; i <= areaNames.Length; i++)
            {
                UiPage p = new UiPage();
                p.name = String.Format("Area {0}", i + 1);
                p.label = areaNames[i + 1];
                p.parentPage = PageList["Shutdown"];
                PageList.Add(p.name, p);
                PageList["Shutdown"].MainList.Add(i, p);
            }
            PageList["Admin"].parentPage = PageList["Modes"];
            PageList["Admin"].MainList.Add(1, PageList["User PINS"]);
            PageList["Admin"].MainList.Add(2, PageList["Naming"]);
            PageList["Admin"].MainList.Add(3, PageList["FBS Reset"]);
            PageList["Admin"].Links.Add(1, PageList["Menu"]);

            PageList["Menu"].parentPage = PageList["Modes"];
            PageList["Menu"].MainList.Add(1, PageList["CCTV"]);
            PageList["Menu"].MainList.Add(2, PageList["Video"]);
            PageList["Menu"].MainList.Add(3, PageList["AV"]);
            PageList["Menu"].MainList.Add(4, PageList["Inputs"]);
            PageList["Menu"].MainList.Add(5, PageList["Zones"]);
            PageList["Menu"].MainList.Add(6, PageList["Stage Management"]);
            PageList["Menu"].MainList.Add(7, PageList["Lectern"]);
            PageList["Menu"].MainList.Add(8, PageList["Lighting"]);
            PageList["Menu"].MainList.Add(9, PageList["Foyer"]);
            PageList["Menu"].Links.Add(1, PageList["Admin"]);

            PageList["CCTV"].parentPage = PageList["Menu"];
            PageList["CCTV"].MainList.Add(1, PageList["CCTV Presets"]);
            PageList["CCTV"].MainList.Add(2, PageList["CCTV Routing"]);
            PageList["CCTV"].MainList.Add(3, PageList["CCTV Matrix"]);
            PageList["CCTV"].Links.Add(1, PageList["Menu"]);

            PageList["CCTV Presets"].label = "Presets";
            PageList["CCTV Routing"].label = "Routing";
            PageList["CCTV Matrix" ].label = "Matrix";

            PageList["AV"].parentPage = PageList["Menu"];
            PageList["AV"].MainList.Add(1, PageList["DVD"]);
            PageList["AV"].MainList.Add(2, PageList["AV Presets"]);
            PageList["AV"].MainList.Add(3, PageList["AV Routing"]);
            PageList["AV"].MainList.Add(4, PageList["AV Matrix"]);
            PageList["AV"].Links.Add(1, PageList["Menu"]);

            PageList["AV Presets"].label = "Presets";
            PageList["AV Routing"].label = "Routing";
            PageList["AV Matrix" ].label = "Matrix";

            // add names and default labels 
            foreach (var p in PageList)
            {
                p.Value.name = p.Key;
                if (String.IsNullOrEmpty(p.Value.label)) p.Value.label = p.Key;
            }

*/
            /////////////////////////////


            pages.Add(10, "Login");
            pages.Add(11, "Main" );
            pages.Add(12, "Modes" );

            OperatingModes.Add(1, "Rehearse");
            OperatingModes.Add(2, "Present");
            OperatingModes.Add(3, "Operate");

            AdminModes.Add(1, "Modes");
            AdminModes.Add(2, "Menu");
            AdminModes.Add(3, "User PINS");
            AdminModes.Add(4, "Naming");
            AdminModes.Add(5, "FBS Reset");

            MenuModes.Add(1, "Modes");
            MenuModes.Add(2, "Admin");
            MenuModes.Add(3, "CCTV");
            MenuModes.Add(4, "AV");
            MenuModes.Add(5, "Inputs");
            MenuModes.Add(6, "Zones");
            MenuModes.Add(7, "Stage Management");
            MenuModes.Add(8, "Letern");
            MenuModes.Add(9, "Lighting");
            MenuModes.Add(10,"Foyer");

            PinModes.Add(1, "Admin");
            PinModes.Add(2, "Operator");
            PinModes.Add(3, "User");
            PinModes.Add(4, "Guest");

            NamingModes.Add(1, "Inputs");
            NamingModes.Add(2, "Outputs");
            NamingModes.Add(3, "Presets");
            NamingModes.Add(4, "Modes");
            NamingModes.Add(5, "Pages");

            CctvModes.Add(1, "Menu");
            CctvModes.Add(2, "Presets");
            CctvModes.Add(3, "Routing");
            CctvModes.Add(4, "Matrix");

            AvModes.Add(1, "Menu");
            AvModes.Add(2, "Bluray");
            AvModes.Add(3, "Presets");
            AvModes.Add(4, "Routing");
            AvModes.Add(5, "Matrix");

            ZoneMuteModes.Add(1, "Menu");
            ZoneMuteModes.Add(2, "Main");
            ZoneMuteModes.Add(3, "Gallery");
            ZoneMuteModes.Add(4, "Front Fill");

            FoyerModes.Add(1, "Menu");
            FoyerModes.Add(2, "Audio");
            FoyerModes.Add(3, "AV");
            FoyerModes.Add(4, "Lighting");

            ShutDownModes.Add(1, "Modes");
            ShutDownModes.Add(2, "Admin");
            ShutDownModes.Add(3, "CCTV");
            ShutDownModes.Add(4, "AV");
            ShutDownModes.Add(5, "Inputs");
            ShutDownModes.Add(6, "Zones");

            //strJson = JsonConvert.SerializeObject(MenuModes);
            //{"1":"Modes","2":"Admin","3":"CCTV","4":"AV","5":"Inputs","6":"Zones","7":"Stage Management","8":"Letern","9":"Lighting","10":"Foyer"}
        }

        public void DoPageFlip(CrestronDevice device, ushort page)
        {
            parent.PulseDigital(device, page, 20);
            device.currentPage = page;
        }

        public string FormatText(string str, string font, byte fontSize, int colour, bool bold, bool underline, bool italic)
        {
            //<FONT size="30" face="Crestron Sans Pro" color="#ffffff">text</FONT>
            string s = bold ? "<B>" + str + "</B>" : str;
            s = underline   ? "<U>" + s   + "</U>" : s;
            s = italic      ? "<I>" + s   + "</I>" : s;
            s = String.Format("<FONT size=\"{0}\" face=\"{1}\" color=\"#{2:x6}\">{3}</FONT>", fontSize, font, 0xFFFFFF & colour, s);
            parent.Debug("string out: " + s);
            return s;
        }
        public string FormatText(string str, byte fontSize, int colour)
        {
            return FormatText(str, "Crestron Sans Pro", fontSize, colour, false, false, false);
        }
        public int GetColour(string colour)
        {
            return Color.FromName(colour).ToArgb();
        }
        public int GetColour(int colour)
        {
            return colour;
        }
      
        public void SetTitle(CrestronDevice device, string s)
        {
            parent.SendSerial(device, SER_TITLE, FormatText(s.ToUpper(), FONT_SIZE_TITLE, COLOUR_TITLE));
            CurrentTitle = s;
        }
        public void SetInstruct(CrestronDevice device, string s)
        {
            parent.SendSerial(device, SER_INSTRUCT, FormatText(s, FONT_SIZE_INSTRUCT, GetColour(COLOUR_INSTRUCT)));
        }

        public void SetCurrentPage(CrestronDevice device, Dictionary<byte, string> l, string title, string msg)
        {
            SetTitle(device, title);
            CurrentList = l;
            parent.SendDigital(device, DIG_LIST_VISIBLE, true);
            parent.SendAnalogueSmartObject(device, SMART_LIST, SMART_IDX_SET_LIST_SIZE, (ushort)CurrentList.Count); // set list size
            SetInstruct(device, msg);
            foreach (var o in CurrentList)
                parent.SendSerialSmartObject(device, SMART_LIST, o.Key, FormatText(o.Value, FONT_SIZE_LIST, GetColour(COLOUR_LIST)));
            parent.PulseDigital(device, PAGE_MAIN, 20);
        }

        public void DigitalSmartObjectEventIn(CrestronDevice device, byte smartId, ushort idx, bool val)
        {
            switch (smartId)
            {
                case SMART_KEYPAD:
                {
                    if (val) // press
                    {
                        if (idx < 10) // numbers
                        {
                            passwordEntered = passwordEntered + idx.ToString();
                            parent.SendSerial(device, 1, passwordEntered);
                        }
                        else if (idx == 11) // del
                        {
                            passwordEntered = passwordEntered.Substring(0,passwordEntered.Length-1);
                            parent.SendSerial(device, SER_KEYPAD, passwordEntered);
                        }
                        else // enter
                        {
                            parent.SendSerial(device, SER_KEYPAD, passwordEntered == passwordCurrent ? "Right" : "Wrong");
                            //if (passwordEntered == passwordCurrent)
                            if (true)
                            {
                                Thread.Sleep(500);
                                passwordEntered = "";
                                parent.SendSerial(device, SER_KEYPAD, passwordEntered);
                                parent.SendDigital(device, DIG_SUB_PASSWORD, false);
                                SetCurrentPage(device, OperatingModes, "Modes", "Select an operating mode");
                           }
                        }
                    }
                    break;
                }
                case SMART_TOPMENU:
                {
                    if (val) // press
                    {
                        switch (idx)
                        {
                            case MENU_SEL_1:
                                break;
                            case MENU_SEL_ADMIN:
                            {
                                SetCurrentPage(device, AdminModes, "Admin", "Select an admin function");
                                break;
                            }
                            case MENU_SEL_LOGOUT:
                            {
                                parent.PulseDigital(device, PAGE_LOGIN, 20);
                                parent.SendDigital(device, DIG_LIST_VISIBLE, false);
                                break;
                            }
                            case MENU_SEL_SHUTDOWN:
                            {
                                parent.PulseDigital(device, PAGE_LOGIN, 20);
                                break;
                            }
                        }
                    }
                    break;
                }
                //case SMART_LIST: // smart ser idx starts at \x0A
                default:
                {
                    if (val) // press
                    {
                        switch (CurrentTitle)
                        {
                            case "AV": // (CurrentList == AvModes)
                            {
                                //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                                switch (idx)
                                {
                                    case 11: // "Menu"
                                    {
                                        SetCurrentPage(device, MenuModes, CurrentList.Values.ElementAt(idx - 11), "Select a menu option");
                                        break;
                                    }
                                    case 12: // "Bluray"
                                    {
                                        SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                        parent.SendDigital(device, DIG_LIST_VISIBLE, false);
                                        parent.SendDigital(device, DIG_SUB_BLURAY, true);
                                        SetInstruct(device, "Press any button to control the Bluray player");
                                        break;
                                    }
                                    case 13: // "Presets"
                                    case 14: // "Routing"
                                    case 15: // "Matrix"
                                    {
                                        SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                        parent.SendDigital(device, DIG_LIST_VISIBLE, false);
                                        SetInstruct(device, "Under construction");
                                        break;
                                    }
                                }
                                break;
                            }
                            case "Modes": // (CurrentList == OperatingModes)
                            {
                                //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                                switch (idx)
                                {
                                    case 11: // "Rehearse"
                                    case 12: // "Present"
                                    case 13: // "Operate"
                                    {
                                        ShowConfirmSub(device, CurrentList.Values.ElementAt(idx - 11), "Please confirm to start the system");
                                        //SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx-11), "Please confirm to start the system");
                                        break;
                                    }
                                }
                                break;
                            }
                            case "Menu":
                            {
                                switch (idx)
                                {
                                    case 11: // "Modes"
                                    {
                                        SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx-11), "Select an operating mode");
                                        break;
                                    }
                                    case 12: // "Admin"
                                    {
                                        SetCurrentPage(device, AdminModes, CurrentList.Values.ElementAt(idx-11), "Select an admin function");
                                        break;
                                    }
                                    case 13: // "CCTV"
                                    {
                                        SetCurrentPage(device, CctvModes, CurrentList.Values.ElementAt(idx-11), "Select an CCTV function");
                                        break;
                                    }
                                    case 14: // "AV"
                                    {
                                        SetCurrentPage(device, AvModes, CurrentList.Values.ElementAt(idx - 11), "Select an AV function");
                                        break;
                                    }
                                    case 15: // "Inputs"
                                    case 16: // "Zones"
                                    case 17: // "Stage Management"
                                    case 18: // "Letern"
                                    case 19: // "Lighting"
                                    {
                                        SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                        parent.SendDigital(device, DIG_LIST_VISIBLE, false);
                                        SetInstruct(device, "Under construction");
                                        break;
                                    }
                                    case 20: // "Foyer"
                                    {
                                        SetCurrentPage(device, FoyerModes, CurrentList.Values.ElementAt(idx-11), "Select an function for the foyer");
                                        break;
                                    }
                                }
                                break;
                            }
                            case "User PIN":
                            {
                                break;
                            }
                            case "Naming":
                            {
                                break;
                            }
                            case "Admin": // (CurrentList == AdminModes)
                            {
                                DoPageFlip(device, PAGE_MAIN);
                                //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                                switch (idx)
                                {
                                    case 11: // "Modes"
                                    {
                                        SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx-11), "Select an operating mode");
                                        break;
                                    }
                                    case 12: // "Menu"
                                    {
                                        SetCurrentPage(device, MenuModes, CurrentList.Values.ElementAt(idx-11), "Select a menu option");
                                        break;
                                    }
                                    case 13: // "PINS"
                                    {
                                        SetCurrentPage(device, PinModes, CurrentList.Values.ElementAt(idx-11), "Select a user to edit");
                                        break;
                                    }
                                    case 14: // "Naming"
                                    {
                                        SetCurrentPage(device, NamingModes, CurrentList.Values.ElementAt(idx-11), "Select a category to edit");
                                        break;
                                    }
                                    case 15: // "FBS Reset"
                                    {
                                        parent.SendDigital(device, DIG_LIST_VISIBLE, false);
                                        parent.SendDigital(device, DIG_FBS_RESET_VISIBLE, true);
                                        SetInstruct(device, "Please confirm");
                                        break;
                                    }
                                }
                           }
                           break;
                        }
                    }
                    break;
                }
            }
        }
        
        public void StartCleaning(CrestronDevice device)
        {
            //parent.Debug("StartCleaning");
            if (cleanCurrentMinutes < 1)
            {
                cleanCurrentMinutes = cleanTotalMinutes;
                parent.SendDigital(device, DIG_SUB_CONFIRM, false);
                parent.SendDigital(device, DIG_SUB_CLEAN_COUNT, true);
                parent.SendAnalogue(device, ANA_CLEAN_TIME, cleanCurrentMinutes);
                while (cleanCurrentMinutes > 0)
                {
                    Thread.Sleep(1000);
                    cleanCurrentMinutes--;
                    parent.SendAnalogue(device, ANA_CLEAN_TIME, cleanCurrentMinutes);

                }
                parent.SendDigital(device, DIG_SUB_CLEAN_COUNT, false);
            }
        }
        public void ShowConfirmSub(CrestronDevice device, string title, string msg)
        {
            CurrentConfirm = title;
            parent.SendSerial(device, SER_CONFIRM_TITLE, FormatText(CurrentConfirm, FONT_SIZE_INSTRUCT, GetColour(COLOUR_INSTRUCT)));
            parent.SendSerial(device, SER_CONFIRM_MSG  , FormatText(msg, FONT_SIZE_INSTRUCT, GetColour(COLOUR_INSTRUCT)));
            parent.SendDigital(device, DIG_SUB_CONFIRM, true);
        }
        public void DigitalEventIn(CrestronDevice device, ushort idx, bool val)
        {
            if (val) // press
            {
                parent.Debug("DigitalEventIn press: " + idx);
                switch (idx)
                {
                    case DIG_CLEANING:
                    {
                        ShowConfirmSub(device, "Cleaning", String.Format("Selecting start will enable the cleaning lights for {0} minutes", cleanTotalMinutes));
                        parent.SendDigital(device, DIG_SUB_PASSWORD, false);
                        break;
                    }
                    case DIG_CONFIRM:
                    {
                        if (CurrentConfirm == "Cleaning")
                            StartCleaning(device);
                        else
                        {
                            //StartSystem(device);
                            parent.SendDigital(device, DIG_SUB_CONFIRM, false);
                            parent.SendDigital(device, DIG_SUB_BAR, true);
                            parent.SendSerial(device, SER_CONFIRM_TITLE, "System Starting");
                            parent.SendAnalogue(device, ANA_BAR, 0x5FFF);
                            parent.SendSerial(device, SER_CONFIRM_MSG, "Please wait. 2 Seconds remain");
                            Thread.Sleep(1000);
                            parent.SendAnalogue(device, ANA_BAR, 0xAFFF);
                            parent.SendSerial(device, SER_CONFIRM_MSG, "Please wait. 1 Seconds remain");
                            Thread.Sleep(1000);
                            parent.SendDigital(device, DIG_SUB_BAR, false);
                            SetCurrentPage(device, MenuModes, "Menu", "Select a menu option");
                        }
                        break;
                    }
                    case DIG_CANCEL:
                    {
                        parent.SendDigital(device, DIG_SUB_CONFIRM, false);
                        parent.SendDigital(device, DIG_SUB_PASSWORD, false);
                        if (CurrentConfirm == "Cleaning")
                            parent.SendDigital(device, DIG_SUB_CLEAN_COUNT, false);
                        else
                            SetCurrentPage(device, OperatingModes, "Modes", "Select an operating mode");
                        break;
                    }
                    case DIG_START:
                    {
                        parent.SendDigital(device, DIG_SUB_PASSWORD, true);
                        passwordEntered = "";
                        parent.SendSerial(device, SER_KEYPAD, passwordEntered);
                        break;
                    }
                   case DIG_FBS_RESET:
                    {
                        SetInstruct(device, "FBS Resetting");
                        Thread.Sleep(1000);
                        parent.SendDigital(device, DIG_FBS_RESET_VISIBLE, false);
                        SetCurrentPage(device, AdminModes, "Admin", "Select an admin function");
                        break;
                    }
                    case DIG_FBS_RESET_CANCEL:
                    {
                        parent.SendDigital(device, DIG_FBS_RESET_VISIBLE, false);
                        SetCurrentPage(device, AdminModes, "Admin", "Select an admin function");
                        break;
                    }
                }
            }

        }
        public void DeviceSignIn(CrestronDevice device)
        {
        }

    }
}
