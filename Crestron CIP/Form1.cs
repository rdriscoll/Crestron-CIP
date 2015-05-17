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

        public delegate void SendStringDelegate(string str);
        public SendStringDelegate debug;

        public ClientForm()
        {
            InitializeComponent();
            debug = new SendStringDelegate(Debug);
        }
        
        public void Debug(string str) // string str
        {
            richTextBox1.AppendText(str + "\n");
        }

        public void Callback_EventHandler(EventArgs e)
        {
            Debug("Form Callback_EventHandler: " + e.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Crestron = new Crestron_CIP_Server(this);
        }
       
        private void set_OnActivateStrComplete(int nTransactionID, int nAbilityCode, byte bSuccess, string pszOutputs, int nUserPassBack)
        {
            Console.WriteLine("Complete!");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        
        }
		
    }
 }