// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="TestView1.cs" company="AVPlus Integration Pty Ltd">
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

    class TestView2 : AUserInterfaceEvents
    {
        #region page dictionaries

        #endregion
        #region join constants

        // Digital joins
        const ushort DIG_TOGGLE_POWER = 1;
        const ushort DIG_MACRO = 2;
        // Analog joins
        const ushort ANA_BAR_GRAPH = 1;
        const ushort ANA_RANDOM = 2;
        // Serial joins
        const ushort SER_TITLE = 1;
        const ushort SER_VALUE = 2;
        const ushort SER_INPUT = 3;

        // SmartObject objects
        const byte SG_BTN_LIST = 1;
        const byte SG_DPAD = 2;
        const byte SG_KEYPAD = 3;
        const byte SG_DYNAMIC_BTN_LIST = 4;
        const byte SG_DYNAMIC_ICON_LIST = 5;

        #endregion
        #region variables

        string pin = "1234";
        string keypadText = String.Empty;

        #endregion

        public TestView2()
        {
        }

        #region events from user interface

        public override void DigitalEventIn(CrestronDevice currentDevice, ushort idx, bool val)
        {
            //OnDebug(eDebugEventType.Info, "DigitalEventIn, join:{0}, val:{1}", idx.ToString(), val.ToString());
            var sig = currentDevice.digitalInputs.Find(x => x.pos == idx);
            if (sig == null || sig.value != val)  // changed
            {
                if (val)  // press
                {
                    OnDebug(eDebugEventType.Info, "DigitalEventIn (changed), join:{0}, val:{1}", idx.ToString(), val.ToString());
                    switch (idx)
                    {
                        case DIG_TOGGLE_POWER:
                            OnToggleDigital(currentDevice, idx);
                            break;
                        case DIG_MACRO:
                            OnPulseDigital(currentDevice, DIG_TOGGLE_POWER, 200);
                            var randomNumber = new Random().Next(ushort.MaxValue);
                            OnSetAnalog(currentDevice, ANA_RANDOM, (ushort)randomNumber);
                            OnSetSerial(currentDevice, SER_VALUE, randomNumber.ToString());
                            break;
                    }
                }
                // not processing releases
            }
            else
            {
                OnDebug(eDebugEventType.Info, "DigitalEventIn (held), join:{0}, val:{1}", idx.ToString(), val.ToString());
            }
        }

        public override void AnalogEventIn (CrestronDevice currentDevice, ushort idx, ushort val)
        {
            OnDebug(eDebugEventType.Info, "AnalogEventIn, join:{0}, val:{1}", idx.ToString(), val.ToString());
            switch(idx)
            {
                case ANA_BAR_GRAPH:
                    OnSetAnalog(currentDevice, idx, val);
                    OnSetSerial(currentDevice, SER_VALUE , val.ToString());
                    break;
                case ANA_RANDOM:
                    OnSetAnalog(currentDevice, ANA_BAR_GRAPH, val);
                    break;
            }
        }
        public override void SerialEventIn (CrestronDevice currentDevice, ushort idx, string val)
        {
            OnDebug(eDebugEventType.Info, "SerialEventIn, join:{0}, val:{1}", idx.ToString(), val);
            switch (idx)
            {
                case SER_INPUT:
                    OnSetSerial(currentDevice, SER_VALUE, val.ToString());
                    break;
            }
        }
        public override void DeviceSignIn  (CrestronDevice currentDevice) 
        {
            OnDebug(eDebugEventType.Info, "DeviceSignIn, ID:{0}", currentDevice.id.ToString());
            for (byte idx = 1; idx < 5; idx++)
            {
                SetSmartObjectVisible(currentDevice, SG_DYNAMIC_BTN_LIST , idx, true);
                //SetSmartObjectVisible(currentDevice, SG_DYNAMIC_ICON_LIST, idx, true);
            }
        }

        public override void DigitalSmartObjectEventIn(CrestronDevice device, byte id, ushort idx, bool val)
        {
            //OnDebug(eDebugEventType.Info, "DigitalSmartObjectEventIn, ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val.ToString());
            var so = device.smartObjects.Find(x => x.id == id);
            Digital sig = null;
            if (so != null)
                sig = so.digitalInputs.Find(x => x.pos == idx);
            if (sig == null || sig.value != val)  // changed
            {
                OnDebug(eDebugEventType.Info, "DigitalSmartObjectEventIn (changed), ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val.ToString());
                switch (id)
                {
                    case SG_DPAD             : SmartObject_DPad_DigSigChange       (device, idx, val); break;
                    case SG_KEYPAD           : SmartObject_KeyPad_DigSigChange     (device, idx, val); break;
                    case SG_BTN_LIST         : SmartObject_BtnList_DigSigChange    (device, idx, val); break;
                    case SG_DYNAMIC_BTN_LIST : SmartObject_DynBtnList_DigSigChange (device, idx, val); break;
                    case SG_DYNAMIC_ICON_LIST: SmartObject_DynIconList_DigSigChange(device, idx, val); break;
                }
            }
            else // held
            {
                OnDebug(eDebugEventType.Info, "DigitalSmartObjectEventIn (held), ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val.ToString());
                switch (id)
                {
                    case SG_DPAD             : SmartObject_DPad_DigSigChange       (device, idx, val); break;
                }
            }

        }
        public override void AnalogSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, ushort val)
        {
            //OnDebug(eDebugEventType.Info, "AnalogSmartObjectEventIn, ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val.ToString());
        }
        public override void SerialSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, string val)
        {
            //OnDebug(eDebugEventType.Info, "SerialSmartObjectEventIn, ID:{0}, join:{1}, val:{2}", id.ToString(), idx.ToString(), val);
        }

        void SmartObject_BtnList_DigSigChange    (CrestronDevice currentDevice, ushort idx, bool val)
        {
            if (val)
            {
                //OnDebug(eDebugEventType.Info, "Button List Press event");
                switch (idx)
                {
                    //case 1: break;
                    default:                     
                        // toggle the button feedback and put some text onto it
                        //ToggleSmartObjectSelected(currentDevice, SG_BTN_LIST, idx); // standard button lists don't support feedback  so this doesn't do anything
                        string buttonText = "Item " + idx.ToString();
                        SetSmartObjectText       (currentDevice, SG_BTN_LIST, idx, buttonText);

                        // soDynBtnList uses dynamic IconAnalogs, of type MediaTransports
                        //ToggleSmartObjectVisible (currentDevice, SG_DYNAMIC_BTN_LIST, idx);       // toggle visibility
                        ToggleSmartObjectEnabled   (currentDevice, SG_DYNAMIC_BTN_LIST, idx);       // toggle visibility
                        //SetSmartObjectEnabled    (currentDevice, SG_DYNAMIC_BTN_LIST, idx, true); // enable
                        ////SetDynamicListIconAnalog (currentDevice, SG_DYNAMIC_BTN_LIST, idx, 0); // set icon to the next analog
                        SetDynamicListText       (currentDevice, SG_DYNAMIC_BTN_LIST, idx, buttonText);

                        //// soDynIconList uses dynamic IconSerials, of type IconsLg
                        ToggleSmartObjectVisible (currentDevice, SG_DYNAMIC_ICON_LIST, idx);       // toggle visibility
                        ////SetSmartObjectEnabled    (currentDevice, SG_DYNAMIC_ICON_LIST, idx, true); // enable
                        ////SetSmartObjectIconSerial (currentDevice, SG_DYNAMIC_ICON_LIST, idx, StringHelper.IconsLgDict[(ushort)0]); // set icon to the next serial
                        break;
                }
            }
            else
            {
                //OnDebug(eDebugEventType.Info, "Release event");
            }
        }
        void SmartObject_DynBtnList_DigSigChange (CrestronDevice currentDevice, ushort idx, bool val)
        {
            if (val)
            {   // With standard lists the digital index is offset by 10 so we need to compensate
                ushort number = idx < 11 ? idx : (ushort)(idx - 10); 
                //OnDebug(eDebugEventType.Info, "Press event");
                switch (number)
                {
                    default:
                        string buttonText = "Item " + idx.ToString();
                        string formattedText = StringHelper.FormatTextForUi(buttonText, 20, eCrestronFont.Crestron_Sans_Pro, eNamedColour.White);
                        SetSmartObjectText      (currentDevice, SG_BTN_LIST, number, buttonText);

                        // toggle the button feedback and put some text onto it
                        ToggleDynamicListSelected(currentDevice, SG_DYNAMIC_BTN_LIST, number);
                        SetDynamicListText       (currentDevice, SG_DYNAMIC_BTN_LIST, number, buttonText);
                        SetDynamicListIconAnalog (currentDevice, SG_DYNAMIC_BTN_LIST, number, number); // set icon to the next analog
                        //SetSmartObjectText      (currentDevice, SG_DYNAMIC_BTN_LIST, number, formattedText);
                        //SetSmartObjectIconAnalog(currentDevice, SG_DYNAMIC_BTN_LIST, number, number); // set icon to the next analog
                        
                        ToggleSmartObjectEnabled (currentDevice, SG_DYNAMIC_ICON_LIST, number);       // enable
                        SetSmartObjectIconSerial (currentDevice, SG_DYNAMIC_ICON_LIST, idx, StringHelper.IconsLgDict[(ushort)0]); // set icon to the next serial
                    break;
                }
            }
            else
            {
                //OnDebug(eDebugEventType.Info, "Release event");
            }
        }
        void SmartObject_DynIconList_DigSigChange(CrestronDevice currentDevice, ushort idx, bool val)
        {
            if (val)
            {
                //OnDebug(eDebugEventType.Info, "Press event");
                ushort number = idx < 11 ? idx : (ushort)(idx - 10);
                switch (idx)
                {
                    default:
                        string buttonText = "xItem " + idx.ToString();
                        SetSmartObjectText          (currentDevice, SG_BTN_LIST, idx, buttonText);
                        ToggleSmartObjectEnabled    (currentDevice, SG_DYNAMIC_BTN_LIST, idx);       // enable
 
                        //toggle the button feedback and put some text onto it
                        ToggleSmartObjectDigitalJoin(currentDevice, SG_DYNAMIC_ICON_LIST, idx);
                        SetSmartObjectIconSerial    (currentDevice, SG_DYNAMIC_ICON_LIST, number, StringHelper.IconsLgDict[(ushort)number]); // set icon to the next serial
                        //SetSmartObjectText(so, (int)idx, buttonText);
                        //SetSmartObjectText          (currentDevice, SG_DYNAMIC_ICON_LIST, idx, buttonText);
                        break;
                }
            }
            else
            {
                //OnDebug(eDebugEventType.Info, "Release event");
            }
        }
        void SmartObject_DPad_DigSigChange       (CrestronDevice currentDevice, ushort idx, bool val)
        { 
            if (val)
            {
                string str = String.Empty;
                switch (idx)
                {
                    case 1: str = "Up"    ; break; // up
                    case 2: str = "Down"  ; break; // dn
                    case 3: str = "Left"  ; break; // le
                    case 4: str = "Right" ; break; // ri
                    case 5: str = "Center"; break; // OK
                    default: 
                        str = "Unhandled keypad button {0}" + idx.ToString(); 
                        break;
                }
                str = str + " pressed";
                OnDebug(eDebugEventType.Info, str);
                OnSetSerial(currentDevice, SER_TITLE, str);
            }
        }
        void SmartObject_KeyPad_DigSigChange     (CrestronDevice currentDevice, ushort idx, bool val)
        {
            if (val)
            {
                if(idx < 11) // 1 to 9
                    keypadText += idx.ToString();
                else if (idx == 11) // MISC_1 - could be anything but we'll make it clear for this example
                    keypadText = "";
                else if (idx == 12) // MISC_2 - could be anything but we'll make it enter for this example
                {
                    keypadText = "PIN " + (keypadText.Equals(pin) ? "Correct": "Wrong");
                    //Thread keypad = new Thread(ResetPinTextThread, currentDevice);
                    var start = new ParameterizedThreadStart(ResetPinTextThread);
                    new Thread(start).Start(currentDevice);
                }
                OnSetSerial(currentDevice, SER_INPUT, keypadText);
            }
            else // release
            {
            }
        }
        void ResetPinTextThread(object o) // not thread safe!
        {
            try
            {
                OnDebug(eDebugEventType.Info, "UResetPinText");
                //Thread.Sleep(1000);
                System.Threading.Timer pinTimer = null;
                pinTimer = new Timer((x) =>
                {
                    keypadText = "";
                    var ui = o as CrestronDevice;
                    if (ui != null)
                        OnSetSerial(ui, SER_INPUT, keypadText);
                    pinTimer.Dispose();
                }, null, 1000, 1000);
            }
            catch (Exception e)
            {
                OnDebug(eDebugEventType.Info, "ResetPinText exception: {0}", e.Message);
            }
        }

        #endregion
    }
}