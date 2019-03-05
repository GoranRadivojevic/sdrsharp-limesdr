namespace SDRSharp.CAT
{
    partial class CATPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkCATenable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboCOMport = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboSpeed = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboParity = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // chkCATenable
            // 
            this.chkCATenable.AutoSize = true;
            this.chkCATenable.Location = new System.Drawing.Point(61, 14);
            this.chkCATenable.Name = "chkCATenable";
            this.chkCATenable.Size = new System.Drawing.Size(82, 17);
            this.chkCATenable.TabIndex = 0;
            this.chkCATenable.Text = "CAT enable";
            this.chkCATenable.UseVisualStyleBackColor = true;
            this.chkCATenable.CheckedChanged += new System.EventHandler(this.chkCATenable_CheckedChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Blue;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(33, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Radio: IC7000";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboCOMport
            // 
            this.comboCOMport.FormattingEnabled = true;
            this.comboCOMport.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM10",
            "COM11",
            "COM12",
            "COM13",
            "COM14",
            "COM15",
            "COM16",
            "COM17",
            "COM18",
            "COM19",
            "COM20",
            "COM21",
            "COM22",
            "COM23",
            "COM24",
            "COM25",
            "COM26",
            "COM27",
            "COM28",
            "COM29",
            "COM30",
            "COM31",
            "COM32"});
            this.comboCOMport.Location = new System.Drawing.Point(78, 39);
            this.comboCOMport.Name = "comboCOMport";
            this.comboCOMport.Size = new System.Drawing.Size(89, 21);
            this.comboCOMport.TabIndex = 1;
            this.comboCOMport.SelectedIndexChanged += new System.EventHandler(this.comboCOMport_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Speed";
            // 
            // comboSpeed
            // 
            this.comboSpeed.FormattingEnabled = true;
            this.comboSpeed.Items.AddRange(new object[] {
            "300",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "230400"});
            this.comboSpeed.Location = new System.Drawing.Point(78, 66);
            this.comboSpeed.Name = "comboSpeed";
            this.comboSpeed.Size = new System.Drawing.Size(89, 21);
            this.comboSpeed.TabIndex = 3;
            this.comboSpeed.SelectedIndexChanged += new System.EventHandler(this.comboSpeed_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Parity";
            // 
            // comboParity
            // 
            this.comboParity.FormattingEnabled = true;
            this.comboParity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.comboParity.Location = new System.Drawing.Point(78, 93);
            this.comboParity.Name = "comboParity";
            this.comboParity.Size = new System.Drawing.Size(89, 21);
            this.comboParity.TabIndex = 5;
            this.comboParity.SelectedIndexChanged += new System.EventHandler(this.comboParity_SelectedIndexChanged);
            // 
            // CATPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboParity);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboSpeed);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboCOMport);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkCATenable);
            this.MinimumSize = new System.Drawing.Size(204, 153);
            this.Name = "CATPanel";
            this.Size = new System.Drawing.Size(204, 153);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkCATenable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboCOMport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboSpeed;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboParity;
    }
}
