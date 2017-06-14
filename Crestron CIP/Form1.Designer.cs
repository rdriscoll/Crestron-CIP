namespace AVPlus.CrestronCIP
{
    partial class ClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnDig = new System.Windows.Forms.Button();
            this.tbSer = new System.Windows.Forms.TextBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 23);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(298, 187);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // btnDig
            // 
            this.btnDig.Location = new System.Drawing.Point(12, 216);
            this.btnDig.Name = "btnDig";
            this.btnDig.Size = new System.Drawing.Size(78, 33);
            this.btnDig.TabIndex = 3;
            this.btnDig.Text = "send digital";
            this.btnDig.UseVisualStyleBackColor = true;
            this.btnDig.Click += new System.EventHandler(this.btnDig_Click);
            // 
            // tbSer
            // 
            this.tbSer.Location = new System.Drawing.Point(180, 223);
            this.tbSer.Name = "tbSer";
            this.tbSer.Size = new System.Drawing.Size(130, 20);
            this.tbSer.TabIndex = 4;
            this.tbSer.TextChanged += new System.EventHandler(this.tbSer_TextChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(96, 224);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(78, 20);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 261);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.tbSer);
            this.Controls.Add(this.btnDig);
            this.Controls.Add(this.richTextBox1);
            this.Name = "ClientForm";
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.ClientForm_Shown);
            this.Resize += new System.EventHandler(this.ClientForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnDig;
        private System.Windows.Forms.TextBox tbSer;
        private System.Windows.Forms.NumericUpDown numericUpDown1;

    }
}

