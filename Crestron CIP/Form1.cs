using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace avplus
{
    public partial class ClientForm : Form
    {
        Crestron_CIP_Server Crestron;
        CrestronDevice device;

        public delegate void StringDelegate(string str);
        public StringDelegate debug;

        //public delegate void CrestronDeviceDelegate(int idx, string str);
        //public StringWithIndexDelegate serFb;

        public delegate void StringWithIndexDelegate(int idx, string str);
        public StringWithIndexDelegate serFb;

        public ClientForm()
        {
            InitializeComponent();
            debug  = new StringDelegate(Debug);
            serFb = new StringWithIndexDelegate(doSerFb);
        }

        #region delegate functions
        public void Debug(string str) // string str
        {
            richTextBox1.AppendText(str + "\n");
        }

        public void doSerFb(int idx, string str) // string str
        {
            switch (idx)
            {
                case 1:
                    tbSer.Text = str;
                    break;
            }
        }
        #endregion

        #region events

        private void ClientForm_Shown(object sender, EventArgs e)
        {
            Crestron = new Crestron_CIP_Server(this);
            device = new CrestronDevice(0x03, Crestron);
            Crestron.StartServer();
        }
      
        private void set_OnActivateStrComplete(int nTransactionID, int nAbilityCode, byte bSuccess, string pszOutputs, int nUserPassBack)
        {
            Console.WriteLine("Complete!");
        }

        private void ClientForm_Resize(object sender, EventArgs e)
        {
            richTextBox1.Height = this.Height - 100;
            richTextBox1.Width = this.Width - 40;
            btnDig.Top = this.Height - 70;
            numericUpDown1.Top = this.Height - 74;
            tbSer.Top = this.Height - 74;
            tbSer.Width = this.Width - 200;
        }

        private void btnDig_Click(object sender, EventArgs e)
        {
            Crestron.ToggleDigital(device, 1);
            //Crestron.SendDigitalSmartObject(3,1,true);
        }

        private void tbSer_TextChanged(object sender, EventArgs e)
        {
            //Crestron.SendSerial(devices[1], 1, tbSer.Text);
            //Crestron.SendSerialSmartObject(device, 3, 1, tbSer.Text);
            string s = Utils.createBytesFromHexString(tbSer.Text);
            //Crestron.SendSerialSmartObject(Crestron.GetCrestronDevice(0x03), 3, 1, s);
            Crestron.Send(Crestron.GetCrestronDevice(0x03), s);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Crestron.SendAnaloguePercent(device, 1, (byte)numericUpDown1.Value);
        }
        #endregion

    }
 }