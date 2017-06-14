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

    class TestView1 : AUserInterfaceEvents
    {
        #region page dictionaries

        // Pages, customise for each project
        Dictionary<string, UserInterfacePage> PageList = new Dictionary<string, UserInterfacePage>();
        Dictionary<byte, string> CurrentList = new Dictionary<byte, string>();

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

        #endregion

        #region join constants

        const ushort PAGE_LOGIN = 10;
        const ushort PAGE_MAIN  = 11;

        const byte SMART_KEYPAD = 1;
        const byte SMART_TOPMENU = 2;
        const byte SMART_LIST = 3;

        const byte SMART_IDX_SET_LIST_SIZE = 3;

        const ushort DIG_SUB_SINGLE_LIST   = 26;

        const ushort DIG_SUB_PASSWORD      = 35;

        const ushort DIG_SUB_CONFIRM       = 23;
        const ushort DIG_SUB_CLEAN_COUNT   = 24;
        const ushort DIG_SUB_BAR           = 25;

        const ushort DIG_SUB_BLURAY        = 30;

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

        #endregion

        #region variables

        const int COLOUR_TITLE = 0x004080;  // dark blue
        const int COLOUR_INSTRUCT = 0x004080;
        const string COLOUR_LIST = "white";

        private string CurrentTitle;
        private string CurrentConfirm;

        private string passwordCurrent = "1234";
        private string passwordEntered;

        private byte cleanTotalMinutes = 4;
        private byte cleanCurrentMinutes;

        #endregion

        public TestView1()
        {
            CreatePages();
        }

        #region events from user interface

        public override void DeviceSignIn  (CrestronDevice device)
        {
            DoPageFlip(device, PAGE_LOGIN);
        }
        public override void DigitalEventIn(CrestronDevice device, ushort idx, bool val)
        {
            //OnDebug(eDebugEventType.Info, "DigitalEventIn: " + idx.ToString());
            if (val) // press
            {
                OnDebug(eDebugEventType.Info, "DigitalEventIn press: " + idx.ToString());
                switch (idx)
                {
                    case DIG_CLEANING:
                        ShowConfirmSub(device, "Cleaning", String.Format("Selecting start will enable the cleaning lights for {0} minutes", cleanTotalMinutes));
                        OnSetDigital(device, DIG_SUB_PASSWORD, false);
                        break;
                    case DIG_CONFIRM:
                        if (CurrentConfirm == "Cleaning")
                            StartCleaning(device);
                        else
                        {
                            //StartSystem(device);
                            OnSetDigital(device, DIG_SUB_CONFIRM, false);
                            OnSetDigital(device, DIG_SUB_BAR, true);
                            OnSetSerial(device, SER_CONFIRM_TITLE, "System Starting");
                            OnSetAnalog(device, ANA_BAR, 0x5FFF);
                            OnSetSerial(device, SER_CONFIRM_MSG, "Please wait. 2 Seconds remain");
                            Thread.Sleep(1000);
                            OnSetAnalog(device, ANA_BAR, 0xAFFF);
                            OnSetSerial(device, SER_CONFIRM_MSG, "Please wait. 1 Seconds remain");
                            Thread.Sleep(1000);
                            OnSetDigital(device, DIG_SUB_BAR, false);
                            SetCurrentPage(device, MenuModes, "Menu", "Select a menu option");
                        }
                        break;
                    case DIG_CANCEL:
                        OnSetDigital(device, DIG_SUB_CONFIRM, false);
                        OnSetDigital(device, DIG_SUB_PASSWORD, false);
                        if (CurrentConfirm == "Cleaning")
                            OnSetDigital(device, DIG_SUB_CLEAN_COUNT, false);
                        else
                            SetCurrentPage(device, OperatingModes, "Modes", "Select an operating mode");
                        break;
                    case DIG_START:
                        OnSetDigital(device, DIG_SUB_PASSWORD, true);
                        passwordEntered = "";
                        OnSetSerial(device, SER_KEYPAD, passwordEntered);
                        break;
                    //case DIG_BACK: break;
                    case DIG_HOME:
                        DoPageFlip(device, PAGE_MAIN);
                        SetCurrentPage(device, MenuModes, "Menu", "Select a menu option");
                        break;
                }
            }

        }
        public override void AnalogEventIn (CrestronDevice device, ushort idx, ushort val)
        {
            OnDebug(eDebugEventType.Info, "AnalogEventIn, join:{0}, val:{1}", idx.ToString(), val.ToString());
        }
        public override void SerialEventIn (CrestronDevice device, ushort idx, string val)
        {
            OnDebug(eDebugEventType.Info, "SerialEventIn, join:{0}, val:{1}", idx.ToString(), val);
        }

        public override void DigitalSmartObjectEventIn(CrestronDevice device, byte id, ushort idx, bool val)
        {
            switch (id)
            {
                case SMART_KEYPAD : DigitalKeypadEventIn (device, id, idx, val); break;
                case SMART_TOPMENU: DigitalTopMenuEventIn(device, id, idx, val); break;
                //case SMART_LIST: // smart ser idx starts at \x0A
                default           : DigitalMainMenuEventIn(device, id, idx, val); break;
            }
        }
        public override void AnalogSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, ushort val)
        {
            OnDebug(eDebugEventType.Info, "AnalogSmartObjectEventIn, ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val.ToString());
        }
        public override void SerialSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, string val)
        {
            OnDebug(eDebugEventType.Info, "SerialSmartObjectEventIn, ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val);
        }

        private void DigitalKeypadEventIn  (CrestronDevice device, byte id, ushort idx, bool val)
        {
            if (val) // press
            {
                if (idx < 10) // numbers
                {
                    passwordEntered = passwordEntered + idx.ToString();
                    OnSetSerial(device, 1, passwordEntered);
                }
                else if (idx == 11) // del
                {
                    passwordEntered = passwordEntered.Substring(0, passwordEntered.Length - 1);
                    OnSetSerial(device, SER_KEYPAD, passwordEntered);
                }
                else // enter
                {
                    OnSetSerial(device, SER_KEYPAD, passwordEntered == passwordCurrent ? "Right" : "Wrong");
                    if (passwordEntered == passwordCurrent)
                    {
                        Thread.Sleep(500);
                        passwordEntered = "";
                        OnSetSerial(device, SER_KEYPAD, passwordEntered);
                        OnSetDigital(device, DIG_SUB_PASSWORD, false);
                        SetCurrentPage(device, OperatingModes, "Modes", "Select an operating mode");
                    }
                }
            }
        }
        private void DigitalTopMenuEventIn (CrestronDevice device, byte id, ushort idx, bool val)
        {
            if (val) // press
            {
                switch (idx)
                {
                    case MENU_SEL_1: break;
                    case MENU_SEL_ADMIN:
                        SetCurrentPage(device, AdminModes, "Admin", "Select an admin function");
                        break;
                    case MENU_SEL_LOGOUT:
                        OnPulseDigital(device, PAGE_LOGIN, 20);
                        break;
                    case MENU_SEL_SHUTDOWN:
                        OnPulseDigital(device, PAGE_LOGIN, 20);
                        break;
                }
            }
        }
        private void DigitalMainMenuEventIn(CrestronDevice device, byte id, ushort idx, bool val)
        {
            if (val) // press
            {
                switch (CurrentTitle)
                {
                    case "AV": // (CurrentList == AvModes)
                        //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                        switch (idx)
                        {
                            case 11: // "Menu"
                                SetCurrentPage(device, MenuModes, CurrentList.Values.ElementAt(idx - 11), "Select a menu option");
                                break;
                            case 12: // "Bluray"
                                SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                OnSetDigital(device, DIG_SUB_BLURAY, true);
                                SetInstruct(device, "Press any button to control the Bluray player");
                                break;
                            case 13: // "Presets"
                            case 14: // "Routing"
                            case 15: // "Matrix"
                                SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                SetInstruct(device, "Under construction");
                                break;
                        }
                        break;
                    case "Modes": // (CurrentList == OperatingModes)
                        //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                        switch (idx)
                        {
                            case 11: // "Rehearse"
                            case 12: // "Present"
                            case 13: // "Operate"
                                ShowConfirmSub(device, CurrentList.Values.ElementAt(idx - 11), "Please confirm to start the system");
                                //SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx-11), "Please confirm to start the system");
                                break;
                        }
                        break;
                    case "Menu":
                        switch (idx)
                        {
                            case 11: // "Modes"
                                SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx - 11), "Select an operating mode");
                                break;
                            case 12: // "Admin"
                                SetCurrentPage(device, AdminModes, CurrentList.Values.ElementAt(idx - 11), "Select an admin function");
                                break;
                            case 13: // "CCTV"
                                SetCurrentPage(device, CctvModes, CurrentList.Values.ElementAt(idx - 11), "Select an CCTV function");
                                break;
                            case 14: // "AV"
                                SetCurrentPage(device, AvModes, CurrentList.Values.ElementAt(idx - 11), "Select an AV function");
                                break;
                            case 15: // "Inputs"
                            case 16: // "Zones"
                            case 17: // "Stage Management"
                            case 18: // "Letern"
                            case 19: // "Lighting"
                                SetTitle(device, CurrentList.Values.ElementAt(idx - 11));
                                //OnSetDigital(device, DIG_LIST_VISIBLE, false);
                                SetInstruct(device, "Under construction");
                                break;
                            case 20: // "Foyer"
                                SetCurrentPage(device, FoyerModes, CurrentList.Values.ElementAt(idx - 11), "Select an function for the foyer");
                                break;
                        }
                        break;
                    case "User PIN": break;
                    case "Naming": break;
                    case "Admin": // (CurrentList == AdminModes)
                        DoPageFlip(device, PAGE_MAIN);
                        //SetTitle(device, CurrentList.Values.ElementAt(idx-11));
                        switch (idx)
                        {
                            case 11: // "Modes"
                                SetCurrentPage(device, OperatingModes, CurrentList.Values.ElementAt(idx - 11), "Select an operating mode");
                                break;
                            case 12: // "Menu"
                                SetCurrentPage(device, MenuModes, CurrentList.Values.ElementAt(idx - 11), "Select a menu option");
                                break;
                            case 13: // "PINS"
                                SetCurrentPage(device, PinModes, CurrentList.Values.ElementAt(idx - 11), "Select a user to edit");
                                break;
                        }
                        break;
                }
            }
        }

        #endregion

        private void DoPageFlip    (CrestronDevice device, ushort page)
        {
            OnPulseDigital(device, page, 20);
            device.currentPage = page;
        }
        private void SetTitle      (CrestronDevice device, string s)
        {
            OnSetSerial(device, SER_TITLE, StringHelper.FormatTextForUi(s.ToUpper(), FONT_SIZE_TITLE, COLOUR_TITLE));
            CurrentTitle = s;
        }
        private void SetInstruct   (CrestronDevice device, string s)
        {
            OnSetSerial(device, SER_INSTRUCT, StringHelper.FormatTextForUi(s, FONT_SIZE_INSTRUCT, COLOUR_INSTRUCT));
        }
        private void SetCurrentPage(CrestronDevice device, Dictionary<byte, string> l, string title, string msg)
        {
            SetTitle(device, title);
            CurrentList = l;
            //OnSetDigital(device, DIG_LIST_VISIBLE, true);
            OnSetAnalogSmartObject(device, SMART_LIST, SMART_IDX_SET_LIST_SIZE, (ushort)CurrentList.Count); // set list size
            SetInstruct(device, msg);
            foreach (var o in CurrentList)
                OnSetSerialSmartObject(device, SMART_LIST, o.Key, StringHelper.FormatTextForUi(o.Value, FONT_SIZE_LIST, StringHelper.GetColour(COLOUR_LIST)));
            OnPulseDigital(device, PAGE_MAIN, 20);
        }
        private void StartCleaning (CrestronDevice device)
        {
            //parent.Debug("StartCleaning");
            if (cleanCurrentMinutes < 1)
            {
                cleanCurrentMinutes = cleanTotalMinutes;
                OnSetDigital(device, DIG_SUB_CONFIRM, false);
                OnSetDigital(device, DIG_SUB_CLEAN_COUNT, true);
                OnSetAnalog(device, ANA_CLEAN_TIME, cleanCurrentMinutes);
                while (cleanCurrentMinutes > 0)
                {
                    Thread.Sleep(1000);
                    cleanCurrentMinutes--;
                    OnSetAnalog(device, ANA_CLEAN_TIME, cleanCurrentMinutes);

                }
                OnSetDigital(device, DIG_SUB_CLEAN_COUNT, false);
            }
        }
        private void ShowConfirmSub(CrestronDevice device, string title, string msg)
        {
            CurrentConfirm = title;
            OnSetSerial(device, SER_CONFIRM_TITLE, StringHelper.FormatTextForUi(CurrentConfirm, FONT_SIZE_INSTRUCT, COLOUR_INSTRUCT));
            OnSetSerial(device, SER_CONFIRM_MSG  , StringHelper.FormatTextForUi(msg, FONT_SIZE_INSTRUCT, COLOUR_INSTRUCT));
            OnSetDigital(device, DIG_SUB_CONFIRM, true);
        }

        private void CreatePages()
        {
            PageList.Add("Splash", new UserInterfacePage());
            PageList.Add("Login", new UserInterfacePage());
            PageList.Add("Cleaning", new UserInterfacePage());
            PageList.Add("Modes", new UserInterfacePage());
            PageList.Add("Menu", new UserInterfacePage());
            PageList.Add("User PINS", new UserInterfacePage());
            PageList.Add("Naming", new UserInterfacePage());
            PageList.Add("Admin", new UserInterfacePage());
            PageList.Add("CCTV", new UserInterfacePage());
            PageList.Add("Video", new UserInterfacePage());
            PageList.Add("AV", new UserInterfacePage());
            PageList.Add("Inputs", new UserInterfacePage());
            PageList.Add("Zones", new UserInterfacePage());
            PageList.Add("Stage Management", new UserInterfacePage());
            PageList.Add("Letern", new UserInterfacePage());
            PageList.Add("Lighting", new UserInterfacePage());
            PageList.Add("Foyer", new UserInterfacePage());
            PageList.Add("Operator", new UserInterfacePage());
            PageList.Add("User", new UserInterfacePage());
            PageList.Add("Guest", new UserInterfacePage());
            PageList.Add("Outputs", new UserInterfacePage());
            PageList.Add("Pages", new UserInterfacePage());
            PageList.Add("DVD", new UserInterfacePage());
            PageList.Add("Main", new UserInterfacePage());
            PageList.Add("Gallery", new UserInterfacePage());
            PageList.Add("Front Fill", new UserInterfacePage());
            PageList.Add("Audio", new UserInterfacePage());
            PageList.Add("Shutdown", new UserInterfacePage());
            PageList.Add("CCTV Presets", new UserInterfacePage());
            PageList.Add("CCTV Routing", new UserInterfacePage());
            PageList.Add("CCTV Matrix", new UserInterfacePage());
            PageList.Add("AV Presets", new UserInterfacePage());
            PageList.Add("AV Routing", new UserInterfacePage());
            PageList.Add("AV Matrix", new UserInterfacePage());
            // add page references (should be using if(PageList.ContainsKey("Main")))

            /////////////////////////////

            pages.Add(10, "Login");
            pages.Add(11, "Main");
            pages.Add(12, "Modes");

            OperatingModes.Add(1, "Rehearse");
            OperatingModes.Add(2, "Present");
            OperatingModes.Add(3, "Operate");

            AdminModes.Add(1, "Modes");
            AdminModes.Add(2, "Menu");
            AdminModes.Add(3, "User PINS");
            AdminModes.Add(4, "Naming");

            MenuModes.Add(1, "Modes");
            MenuModes.Add(2, "Admin");
            MenuModes.Add(3, "CCTV");
            MenuModes.Add(4, "AV");
            MenuModes.Add(5, "Inputs");
            MenuModes.Add(6, "Zones");
            MenuModes.Add(7, "Stage Management");
            MenuModes.Add(8, "Letern");
            MenuModes.Add(9, "Lighting");
            MenuModes.Add(10, "Foyer");

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
    }
}
