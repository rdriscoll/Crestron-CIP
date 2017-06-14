using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

namespace avplus
{
    class UiHandler
    {
        Crestron_CIP_Server parent;
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
            //parent.PulseDigital(device, PAGE_LOGIN, 20);
            //parent.SendDigital(device, DIG_SUB_PASSWORD, true);
            
 //           ((BasicTriList)currentDevice.ui).BooleanInput[DIG_SUB_PASSWORD].BoolValue = true; // show password page          
            /*
           ((BasicTriList)currentDevice).UShortInput[1].UShortValue = 2;
           int uiIndex = 0;
           if (currentDevice == ui_01) uiIndex = 1;
           else if (currentDevice == ui_02) uiIndex = 2;
           else if (currentDevice == ui_03) uiIndex = 3;
           ((BasicTriList)currentDevice).StringInput[1].StringValue = "room " + uiIndex.ToString();
            */
        }

    }
}
