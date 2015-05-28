using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace avplus
{
    class UiHandler
    {
        Crestron_CIP_Server parent;

        const int PAGE_LOGIN = 10;
        const int PAGE_MAIN = 11;

        const int SUB_SPLASH = 21;
        const int SUB_PASSWORD = 22;

        const int SMART_KEYPAD  = 1;
        const int SMART_TOPMENU = 2;
        const int SMART_LIST    = 3;


        private string passwordCurrent = "1234";
        private string passwordEntered;

        public UiHandler(Crestron_CIP_Server parent)
        {
            this.parent = parent;
        }

        public void DigitalSmartObjectEventIn(CrestronDevice device, byte smartId, ushort idx, bool val)
        {
            switch (smartId)
            {
                case SMART_KEYPAD:
                {
                    if (val) // press
                    {
                        if (idx < 10)
                        {
                            passwordEntered = passwordEntered + idx.ToString();
                            parent.SendSerial(device, 1, passwordEntered);
                        }
                        else if (idx == 11)
                        {
                            passwordEntered = "";
                            parent.SendSerial(device, 1, passwordEntered);
                        }
                        else
                        {
                            parent.SendSerial(device, 1, passwordEntered == passwordCurrent ? "Right" : "Wrong");
                            if (passwordEntered == passwordCurrent)
                            {
                                passwordEntered = "";
                                Thread.Sleep(500);
                                parent.SendDigital(device, PAGE_MAIN, true);
                                parent.SendDigital(device, PAGE_MAIN, false);
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
                            case 14:
                            {
                                parent.SendDigital(device, PAGE_LOGIN, true);
                                parent.SendDigital(device, PAGE_LOGIN, false);
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
                        parent.SendSerialSmartObject(device, smartId, (ushort)(idx - 10), "ITEM " + (idx - 10).ToString());
                    }
                    break;
                }
            }
        }

        public void DigitalEventIn(CrestronDevice device, ushort idx, bool val)
        {
        }


        public void Online(CrestronDevice currentDevice)
        {
 //           ((BasicTriList)currentDevice.ui).BooleanInput[SUB_PASSWORD].BoolValue = true; // show password page          
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
