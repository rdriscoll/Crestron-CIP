using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVPlus.CrestronCIP
{
    public abstract class AUserInterfaceEvents
    {
        public void OnDebug(eDebugEventType eventType, string str, params object[] id)
        {
            if (Debug != null)
                Debug(this, new StringEventArgs(String.Format(str, id)));
        }

        #region eventHandlers

        public event EventHandler<StringEventArgs> Debug;

        public event EventHandler<DigitalEventArgs> SetDigital;
        public event EventHandler<DigitalEventArgs> ToggleDigital;
        public event EventHandler<AnalogEventArgs> PulseDigital;
        public event EventHandler<AnalogEventArgs> SetAnalog;
        public event EventHandler<SerialEventArgs> SetSerial;

        public event EventHandler<DigitalSmartObjectEventArgs> SetDigitalSmartObject;
        public event EventHandler<DigitalSmartObjectEventArgs> ToggleDigitalSmartObject;
        public event EventHandler<AnalogSmartObjectEventArgs> PulseDigitalSmartObject;
        public event EventHandler<AnalogSmartObjectEventArgs> SetAnalogSmartObject;
        public event EventHandler<SerialSmartObjectEventArgs> SetSerialSmartObject;

        #endregion

        #region exposed events

        protected void OnPulseDigital (CrestronDevice device, ushort join, ushort msec)
        {
            if (PulseDigital != null)
                PulseDigital(this, new AnalogEventArgs(device, join, msec));
        }
        protected void OnToggleDigital(CrestronDevice device, ushort join)
        {
            if (ToggleDigital != null)
                ToggleDigital(this, new DigitalEventArgs(device, join, false));
        }
        protected void OnSetDigital   (CrestronDevice device, ushort join, bool val)
        {
            if (SetDigital != null)
                SetDigital(this, new DigitalEventArgs(device, join, val));
        }
        protected void OnSetAnalog    (CrestronDevice device, ushort join, ushort val)
        {
            if (SetAnalog != null)
                SetAnalog(this, new AnalogEventArgs(device, join, val));
        }
        protected void OnSetSerial    (CrestronDevice device, ushort join, string val)
        {
            if (SetSerial != null)
                SetSerial(this, new SerialEventArgs(device, join, val));
        }

        protected void OnPulseDigitalSmartObject  (CrestronDevice device, byte id, ushort join, ushort val)
        {
            if (PulseDigitalSmartObject != null)
                PulseDigitalSmartObject(this, new AnalogSmartObjectEventArgs(device, id, join, val));
        }
        protected void OnSetDigitalSmartObject    (CrestronDevice device, byte id, ushort join, bool val)
        {
            if (SetDigitalSmartObject != null)
                SetDigitalSmartObject(this, new DigitalSmartObjectEventArgs(device, id, join, val));
        }
        protected void OnToggleDigitalSmartObject (CrestronDevice device, byte id, ushort join)
        {
            if (ToggleDigitalSmartObject != null)
                ToggleDigitalSmartObject(this, new DigitalSmartObjectEventArgs(device, id, join, false));
        }
        protected void OnSetAnalogSmartObject     (CrestronDevice device, byte id, ushort join, ushort val)
        {
            if (SetAnalogSmartObject != null)
                SetAnalogSmartObject(this, new AnalogSmartObjectEventArgs(device, id, join, val));
        }
        protected void OnSetSerialSmartObject     (CrestronDevice device, byte id, ushort join, string val)
        {
            if (SetSerialSmartObject != null)
                SetSerialSmartObject(this, new SerialSmartObjectEventArgs(device, id, join, val));
        }

        #endregion

        #region alias methods for exposed events

        protected void ToggleSmartObjectDigitalJoin(CrestronDevice currentDevice, byte id, ushort idx)
        {
            OnToggleDigitalSmartObject(currentDevice, id, idx);
        }
        protected void ToggleSmartObjectSelected   (CrestronDevice currentDevice, byte id, ushort idx)
        {
            OnToggleDigitalSmartObject(currentDevice, id, idx);
        }
        protected void SetSmartObjectDigitalJoin   (CrestronDevice currentDevice, byte id, ushort idx, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, idx, val);
        }
        protected void SetSmartObjectSelected      (CrestronDevice currentDevice, byte id, ushort idx, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, idx, val);
        }
        protected void ToggleDynamicListSelected   (CrestronDevice currentDevice, byte id, ushort idx)
        {
            OnToggleDigitalSmartObject(currentDevice, id, (ushort)(idx + 10));
        }
        protected void SetDynamicListSelected      (CrestronDevice currentDevice, byte id, ushort idx, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, (ushort)(idx + 10), val);
        }
        protected void ToggleSmartObjectVisible    (CrestronDevice currentDevice, byte id, ushort idx)
        {
            OnToggleDigitalSmartObject(currentDevice, id, (ushort)(idx + 0x0FAA));
        }
        protected void SetSmartObjectVisible(CrestronDevice currentDevice, byte id, ushort idx, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, (ushort)(idx + 0x0FAA), val);
        }
        protected void SetSmartObjectEnabled       (CrestronDevice currentDevice, byte id, ushort idx, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, (ushort)(idx + 0x07DA), val);
        }
        protected void ToggleSmartObjectEnabled    (CrestronDevice currentDevice, byte id, ushort idx)
        {
            OnToggleDigitalSmartObject(currentDevice, id, (ushort)(idx + 0x07DA));
        }
        protected void ToggleSmartObjectEnabled    (CrestronDevice currentDevice, byte id)
        {
            OnToggleDigitalSmartObject(currentDevice, id, (ushort)1);
        }
        protected void SetSmartObjectEnabled       (CrestronDevice currentDevice, byte id, bool val)
        {
            OnSetDigitalSmartObject(currentDevice, id, (ushort)1, val);
        }

        protected void SetSmartObjectIconAnalog(CrestronDevice currentDevice, byte id, ushort idx, ushort val)
        {
            OnSetAnalogSmartObject(currentDevice, id, idx, val);
        }
        protected void SetDynamicListIconAnalog(CrestronDevice currentDevice, byte id, ushort idx, ushort val)
        {
            OnSetAnalogSmartObject(currentDevice, id, (ushort)(idx + 9), val);
        }

        protected void SetSmartObjectText      (CrestronDevice currentDevice, byte id, ushort idx, string val)
        {
            OnSetSerialSmartObject(currentDevice, id, (ushort)(idx - 1), val);
        }
        protected void SetDynamicListText      (CrestronDevice currentDevice, byte id, ushort idx, string val)
        {
            OnSetSerialSmartObject(currentDevice, id, (ushort)(idx + 9), val);
        }
        protected void SetSmartObjectIconSerial(CrestronDevice currentDevice, byte id, ushort idx, string val)
        {
            OnSetSerialSmartObject(currentDevice, id, (ushort)(idx + 0x07D9), val);
        }

        //bool GetSmartObjectDigitalJoin(CrestronDevice currentDevice, ushort id, ushort idx)

        #endregion

        public abstract void DeviceSignIn  (CrestronDevice device);
        public abstract void DigitalEventIn(CrestronDevice device, ushort idx, bool val);
        public abstract void AnalogEventIn (CrestronDevice device, ushort idx, ushort val);
        public abstract void SerialEventIn (CrestronDevice device, ushort idx, string val);
        public abstract void DigitalSmartObjectEventIn(CrestronDevice device, byte id, ushort idx, bool val);
        public abstract void AnalogSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, ushort val);
        public abstract void SerialSmartObjectEventIn (CrestronDevice device, byte id, ushort idx, string val);
    }
}
