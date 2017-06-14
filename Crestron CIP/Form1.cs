// License info and recommendations
//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="AVPlus Integration Pty Ltd">
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
    using System.Windows.Forms;

    public partial class ClientForm : Form
    {
        Crestron_CIP_Server Crestron;

        public ClientForm()
        {
            InitializeComponent();
        }

        #region event functions


        void UpdateMainTextbox(string s)
        {
            if (richTextBox1.InvokeRequired)
                this.Invoke(new Action<string>(UpdateMainTextbox), new object[] { s });
            else
                richTextBox1.AppendText(s + "\n");
        }
        void Crestron_Debug(object sender, StringEventArgs e)
        {
            UpdateMainTextbox(e.val);
        }

        void UpdateSerialTextbox(string s)
        {
            if (tbSer.InvokeRequired)
                this.Invoke(new Action<string>(UpdateSerialTextbox), new object[] { s });
            else
                tbSer.Text = s;
        }
        void Crestron_SetSerial(object sender, SerialEventArgs e)
        {
            switch (e.join)
            {
                case 1:
                    UpdateSerialTextbox(e.val);
                    break;
            }
        }

        #endregion

        #region events

        private void ClientForm_Shown(object sender, EventArgs e)
        {
            Crestron = new Crestron_CIP_Server();
            Crestron.Debug     += new EventHandler<StringEventArgs>(Crestron_Debug);
            Crestron.SetSerial += new EventHandler<SerialEventArgs>(Crestron_SetSerial);
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
            Crestron.ToggleDigital(Crestron.GetCrestronDevice(0x03), (ushort)numericUpDown1.Value);
            //Crestron.SendDigitalSmartObject(3,1,true);
        }

        private void tbSer_TextChanged(object sender, EventArgs e)
        {
            //Crestron.SendSerial(Crestron.GetCrestronDevice(0x03), 2, tbSer.Text);
            Crestron.SendSerial(Crestron.GetCrestronDevice(0x03), 2, tbSer.Text);
            //Crestron.SendSerial(Crestron.GetCrestronDevice(0x03), 3, tbSer.Text);
            //Crestron.SendSerialSmartObject(device, 3, 1, tbSer.Text);
            string s = StringHelper.CreateBytesFromHexString(tbSer.Text);
            //Crestron.SendSerialSmartObject(Crestron.GetCrestronDevice(0x03), 3, 1, s);
            //Crestron.Send(Crestron.GetCrestronDevice(0x03), s);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Crestron.SendAnaloguePercent(Crestron.GetCrestronDevice(0x03), 1, (byte)numericUpDown1.Value);
            Crestron.SendAnalogue(Crestron.GetCrestronDevice(0x03), 1, (byte)numericUpDown1.Value);
        }
        #endregion

    }
 }