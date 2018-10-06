namespace SDRSharp.LimeSDR
{
  public  partial  class LimeSDRControllerDialog
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
            this.close = new System.Windows.Forms.Button();
            this.samplerateComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gainBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.gainDB = new System.Windows.Forms.Label();
            this.rx0 = new System.Windows.Forms.RadioButton();
            this.rx1 = new System.Windows.Forms.RadioButton();
            this.ant_h = new System.Windows.Forms.RadioButton();
            this.ant_l = new System.Windows.Forms.RadioButton();
            this.ant_w = new System.Windows.Forms.RadioButton();
            this.grpChannel = new System.Windows.Forms.GroupBox();
            this.grpAntenna = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.LPBWcomboBox = new System.Windows.Forms.ComboBox();
            this.udSpecOffset = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblLimeSDR_LNAGain = new System.Windows.Forms.Label();
            this.lblLimeSDR_TIAGain = new System.Windows.Forms.Label();
            this.lblLimeSDR_PGAGain = new System.Windows.Forms.Label();
            this.tbLimeSDR_PGAGain = new System.Windows.Forms.TrackBar();
            this.label8 = new System.Windows.Forms.Label();
            this.tbLimeSDR_TIAGain = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.tbLimeSDR_LNAGain = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.udFrequencyDiff = new System.Windows.Forms.NumericUpDown();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtSerialNumber = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtGatewareVersion = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtFirm_version = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtRadioModel = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtLimeSuiteVersion = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtRadioSerialNo = new System.Windows.Forms.TextBox();
            this.txtModule = new System.Windows.Forms.TextBox();
            this.txtRadioName = new System.Windows.Forms.TextBox();
            this.comboRadioModel = new System.Windows.Forms.ComboBox();
            this.btnRadioRefresh = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gainBar)).BeginInit();
            this.grpChannel.SuspendLayout();
            this.grpAntenna.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udSpecOffset)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_PGAGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_TIAGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_LNAGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFrequencyDiff)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // close
            // 
            this.close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.close.Location = new System.Drawing.Point(221, 465);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 25);
            this.close.TabIndex = 0;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // samplerateComboBox
            // 
            this.samplerateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.samplerateComboBox.FormattingEnabled = true;
            this.samplerateComboBox.Location = new System.Drawing.Point(13, 34);
            this.samplerateComboBox.Name = "samplerateComboBox";
            this.samplerateComboBox.Size = new System.Drawing.Size(199, 21);
            this.samplerateComboBox.TabIndex = 1;
            this.samplerateComboBox.SelectedIndexChanged += new System.EventHandler(this.samplerateComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Sample Rate";
            // 
            // gainBar
            // 
            this.gainBar.AutoSize = false;
            this.gainBar.Location = new System.Drawing.Point(22, 80);
            this.gainBar.Maximum = 70;
            this.gainBar.Name = "gainBar";
            this.gainBar.Size = new System.Drawing.Size(199, 18);
            this.gainBar.TabIndex = 3;
            this.gainBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.gainBar.Scroll += new System.EventHandler(this.gainBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Gain";
            // 
            // gainDB
            // 
            this.gainDB.AutoSize = true;
            this.gainDB.Location = new System.Drawing.Point(183, 63);
            this.gainDB.Name = "gainDB";
            this.gainDB.Size = new System.Drawing.Size(19, 13);
            this.gainDB.TabIndex = 5;
            this.gainDB.Text = "db";
            // 
            // rx0
            // 
            this.rx0.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rx0.Location = new System.Drawing.Point(20, 28);
            this.rx0.Name = "rx0";
            this.rx0.Size = new System.Drawing.Size(46, 18);
            this.rx0.TabIndex = 7;
            this.rx0.Text = "RX0";
            this.rx0.UseVisualStyleBackColor = true;
            this.rx0.CheckedChanged += new System.EventHandler(this.rx0_CheckedChanged);
            // 
            // rx1
            // 
            this.rx1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rx1.Location = new System.Drawing.Point(20, 57);
            this.rx1.Name = "rx1";
            this.rx1.Size = new System.Drawing.Size(46, 18);
            this.rx1.TabIndex = 8;
            this.rx1.Text = "RX1";
            this.rx1.UseVisualStyleBackColor = true;
            this.rx1.CheckedChanged += new System.EventHandler(this.rx1_CheckedChanged);
            // 
            // ant_h
            // 
            this.ant_h.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ant_h.Location = new System.Drawing.Point(12, 22);
            this.ant_h.Name = "ant_h";
            this.ant_h.Size = new System.Drawing.Size(63, 18);
            this.ant_h.TabIndex = 10;
            this.ant_h.Text = "LNA_H";
            this.ant_h.UseVisualStyleBackColor = true;
            this.ant_h.CheckedChanged += new System.EventHandler(this.ant_h_CheckedChanged);
            // 
            // ant_l
            // 
            this.ant_l.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ant_l.Location = new System.Drawing.Point(12, 46);
            this.ant_l.Name = "ant_l";
            this.ant_l.Size = new System.Drawing.Size(63, 18);
            this.ant_l.TabIndex = 11;
            this.ant_l.Text = "LNA_L";
            this.ant_l.UseVisualStyleBackColor = true;
            this.ant_l.CheckedChanged += new System.EventHandler(this.ant_l_CheckedChanged);
            // 
            // ant_w
            // 
            this.ant_w.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ant_w.Location = new System.Drawing.Point(12, 69);
            this.ant_w.Name = "ant_w";
            this.ant_w.Size = new System.Drawing.Size(63, 18);
            this.ant_w.TabIndex = 12;
            this.ant_w.Text = "LNA_W";
            this.ant_w.UseVisualStyleBackColor = true;
            this.ant_w.CheckedChanged += new System.EventHandler(this.ant_w_CheckedChanged);
            // 
            // grpChannel
            // 
            this.grpChannel.Controls.Add(this.rx0);
            this.grpChannel.Controls.Add(this.rx1);
            this.grpChannel.Location = new System.Drawing.Point(21, 224);
            this.grpChannel.Name = "grpChannel";
            this.grpChannel.Size = new System.Drawing.Size(87, 93);
            this.grpChannel.TabIndex = 13;
            this.grpChannel.TabStop = false;
            this.grpChannel.Text = "Channel";
            // 
            // grpAntenna
            // 
            this.grpAntenna.Controls.Add(this.ant_h);
            this.grpAntenna.Controls.Add(this.ant_l);
            this.grpAntenna.Controls.Add(this.ant_w);
            this.grpAntenna.Location = new System.Drawing.Point(135, 224);
            this.grpAntenna.Name = "grpAntenna";
            this.grpAntenna.Size = new System.Drawing.Size(87, 93);
            this.grpAntenna.TabIndex = 14;
            this.grpAntenna.TabStop = false;
            this.grpAntenna.Text = "Antenna";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 333);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "LPBW";
            // 
            // LPBWcomboBox
            // 
            this.LPBWcomboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LPBWcomboBox.FormattingEnabled = true;
            this.LPBWcomboBox.Items.AddRange(new object[] {
            "1.5MHz",
            "2MHz",
            "4MHz",
            "8MHz",
            "10MHz",
            "20MHz",
            "40MHz",
            "50MHz",
            "60MHz"});
            this.LPBWcomboBox.Location = new System.Drawing.Point(144, 328);
            this.LPBWcomboBox.Name = "LPBWcomboBox";
            this.LPBWcomboBox.Size = new System.Drawing.Size(68, 24);
            this.LPBWcomboBox.TabIndex = 16;
            this.LPBWcomboBox.Text = "1.5MHz";
            this.LPBWcomboBox.SelectedIndexChanged += new System.EventHandler(this.LPBWcomboBox_SelectedIndexChanged);
            // 
            // udSpecOffset
            // 
            this.udSpecOffset.DecimalPlaces = 1;
            this.udSpecOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udSpecOffset.Location = new System.Drawing.Point(160, 359);
            this.udSpecOffset.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udSpecOffset.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.udSpecOffset.Name = "udSpecOffset";
            this.udSpecOffset.Size = new System.Drawing.Size(55, 22);
            this.udSpecOffset.TabIndex = 17;
            this.udSpecOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udSpecOffset.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udSpecOffset.ValueChanged += new System.EventHandler(this.udSpecOffset_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 363);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Spectrum offset";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblLimeSDR_LNAGain);
            this.groupBox3.Controls.Add(this.lblLimeSDR_TIAGain);
            this.groupBox3.Controls.Add(this.lblLimeSDR_PGAGain);
            this.groupBox3.Controls.Add(this.tbLimeSDR_PGAGain);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.tbLimeSDR_TIAGain);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.tbLimeSDR_LNAGain);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.udFrequencyDiff);
            this.groupBox3.Controls.Add(this.samplerateComboBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.udSpecOffset);
            this.groupBox3.Controls.Add(this.gainBar);
            this.groupBox3.Controls.Add(this.grpAntenna);
            this.groupBox3.Controls.Add(this.grpChannel);
            this.groupBox3.Controls.Add(this.LPBWcomboBox);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.gainDB);
            this.groupBox3.Location = new System.Drawing.Point(257, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(242, 443);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Settings";
            // 
            // lblLimeSDR_LNAGain
            // 
            this.lblLimeSDR_LNAGain.AutoSize = true;
            this.lblLimeSDR_LNAGain.Location = new System.Drawing.Point(186, 103);
            this.lblLimeSDR_LNAGain.Name = "lblLimeSDR_LNAGain";
            this.lblLimeSDR_LNAGain.Size = new System.Drawing.Size(32, 13);
            this.lblLimeSDR_LNAGain.TabIndex = 29;
            this.lblLimeSDR_LNAGain.Text = "21dB";
            // 
            // lblLimeSDR_TIAGain
            // 
            this.lblLimeSDR_TIAGain.AutoSize = true;
            this.lblLimeSDR_TIAGain.Location = new System.Drawing.Point(186, 142);
            this.lblLimeSDR_TIAGain.Name = "lblLimeSDR_TIAGain";
            this.lblLimeSDR_TIAGain.Size = new System.Drawing.Size(26, 13);
            this.lblLimeSDR_TIAGain.TabIndex = 28;
            this.lblLimeSDR_TIAGain.Text = "6dB";
            // 
            // lblLimeSDR_PGAGain
            // 
            this.lblLimeSDR_PGAGain.AutoSize = true;
            this.lblLimeSDR_PGAGain.Location = new System.Drawing.Point(186, 184);
            this.lblLimeSDR_PGAGain.Name = "lblLimeSDR_PGAGain";
            this.lblLimeSDR_PGAGain.Size = new System.Drawing.Size(32, 13);
            this.lblLimeSDR_PGAGain.TabIndex = 27;
            this.lblLimeSDR_PGAGain.Text = "10dB";
            // 
            // tbLimeSDR_PGAGain
            // 
            this.tbLimeSDR_PGAGain.AutoSize = false;
            this.tbLimeSDR_PGAGain.LargeChange = 3;
            this.tbLimeSDR_PGAGain.Location = new System.Drawing.Point(22, 197);
            this.tbLimeSDR_PGAGain.Maximum = 37;
            this.tbLimeSDR_PGAGain.Minimum = 1;
            this.tbLimeSDR_PGAGain.Name = "tbLimeSDR_PGAGain";
            this.tbLimeSDR_PGAGain.Size = new System.Drawing.Size(199, 18);
            this.tbLimeSDR_PGAGain.TabIndex = 25;
            this.tbLimeSDR_PGAGain.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLimeSDR_PGAGain.Value = 10;
            this.tbLimeSDR_PGAGain.Scroll += new System.EventHandler(this.tbLimeSDR_PGAGain_Scroll);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(22, 180);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "PGA Gain";
            // 
            // tbLimeSDR_TIAGain
            // 
            this.tbLimeSDR_TIAGain.AutoSize = false;
            this.tbLimeSDR_TIAGain.LargeChange = 3;
            this.tbLimeSDR_TIAGain.Location = new System.Drawing.Point(22, 158);
            this.tbLimeSDR_TIAGain.Maximum = 12;
            this.tbLimeSDR_TIAGain.Minimum = 3;
            this.tbLimeSDR_TIAGain.Name = "tbLimeSDR_TIAGain";
            this.tbLimeSDR_TIAGain.Size = new System.Drawing.Size(199, 18);
            this.tbLimeSDR_TIAGain.SmallChange = 3;
            this.tbLimeSDR_TIAGain.TabIndex = 23;
            this.tbLimeSDR_TIAGain.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLimeSDR_TIAGain.Value = 6;
            this.tbLimeSDR_TIAGain.Scroll += new System.EventHandler(this.tbLimeSDR_TIAGain_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 141);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "TIA Gain";
            // 
            // tbLimeSDR_LNAGain
            // 
            this.tbLimeSDR_LNAGain.AutoSize = false;
            this.tbLimeSDR_LNAGain.LargeChange = 3;
            this.tbLimeSDR_LNAGain.Location = new System.Drawing.Point(22, 119);
            this.tbLimeSDR_LNAGain.Maximum = 27;
            this.tbLimeSDR_LNAGain.Minimum = 1;
            this.tbLimeSDR_LNAGain.Name = "tbLimeSDR_LNAGain";
            this.tbLimeSDR_LNAGain.Size = new System.Drawing.Size(199, 18);
            this.tbLimeSDR_LNAGain.TabIndex = 21;
            this.tbLimeSDR_LNAGain.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLimeSDR_LNAGain.Value = 21;
            this.tbLimeSDR_LNAGain.Scroll += new System.EventHandler(this.tbLimeSDR_LNAGain_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 102);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "LNA Gain";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 392);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Frequency diff. (KHz)";
            // 
            // udFrequencyDiff
            // 
            this.udFrequencyDiff.DecimalPlaces = 3;
            this.udFrequencyDiff.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udFrequencyDiff.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.udFrequencyDiff.Location = new System.Drawing.Point(143, 388);
            this.udFrequencyDiff.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.udFrequencyDiff.Name = "udFrequencyDiff";
            this.udFrequencyDiff.Size = new System.Drawing.Size(72, 22);
            this.udFrequencyDiff.TabIndex = 19;
            this.udFrequencyDiff.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udFrequencyDiff.ValueChanged += new System.EventHandler(this.udFrequencyDiff_ValueChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtSerialNumber);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.txtGatewareVersion);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.txtFirm_version);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.txtRadioModel);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.txtLimeSuiteVersion);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.txtRadioSerialNo);
            this.groupBox4.Controls.Add(this.txtModule);
            this.groupBox4.Controls.Add(this.txtRadioName);
            this.groupBox4.Controls.Add(this.comboRadioModel);
            this.groupBox4.Controls.Add(this.btnRadioRefresh);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Location = new System.Drawing.Point(6, 14);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(242, 441);
            this.groupBox4.TabIndex = 20;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Device Info";
            this.groupBox4.Enter += new System.EventHandler(this.groupBox4_Enter);
            // 
            // txtSerialNumber
            // 
            this.txtSerialNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSerialNumber.Location = new System.Drawing.Point(17, 363);
            this.txtSerialNumber.Name = "txtSerialNumber";
            this.txtSerialNumber.ReadOnly = true;
            this.txtSerialNumber.Size = new System.Drawing.Size(208, 22);
            this.txtSerialNumber.TabIndex = 44;
            this.txtSerialNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(86, 347);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 13);
            this.label17.TabIndex = 43;
            this.label17.Text = "Serial number";
            // 
            // txtGatewareVersion
            // 
            this.txtGatewareVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGatewareVersion.Location = new System.Drawing.Point(17, 322);
            this.txtGatewareVersion.Name = "txtGatewareVersion";
            this.txtGatewareVersion.ReadOnly = true;
            this.txtGatewareVersion.Size = new System.Drawing.Size(208, 22);
            this.txtGatewareVersion.TabIndex = 42;
            this.txtGatewareVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(93, 306);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(56, 13);
            this.label16.TabIndex = 41;
            this.label16.Text = "Gateware ";
            // 
            // txtFirm_version
            // 
            this.txtFirm_version.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFirm_version.Location = new System.Drawing.Point(17, 281);
            this.txtFirm_version.Name = "txtFirm_version";
            this.txtFirm_version.ReadOnly = true;
            this.txtFirm_version.Size = new System.Drawing.Size(208, 22);
            this.txtFirm_version.TabIndex = 40;
            this.txtFirm_version.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(78, 265);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(86, 13);
            this.label15.TabIndex = 39;
            this.label15.Text = "Firmware version";
            // 
            // txtRadioModel
            // 
            this.txtRadioModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRadioModel.Location = new System.Drawing.Point(17, 199);
            this.txtRadioModel.Name = "txtRadioModel";
            this.txtRadioModel.ReadOnly = true;
            this.txtRadioModel.Size = new System.Drawing.Size(208, 22);
            this.txtRadioModel.TabIndex = 38;
            this.txtRadioModel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(88, 183);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(66, 13);
            this.label14.TabIndex = 37;
            this.label14.Text = "Radio model";
            // 
            // txtLimeSuiteVersion
            // 
            this.txtLimeSuiteVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLimeSuiteVersion.Location = new System.Drawing.Point(17, 240);
            this.txtLimeSuiteVersion.Name = "txtLimeSuiteVersion";
            this.txtLimeSuiteVersion.ReadOnly = true;
            this.txtLimeSuiteVersion.Size = new System.Drawing.Size(208, 22);
            this.txtLimeSuiteVersion.TabIndex = 36;
            this.txtLimeSuiteVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(92, 224);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 13);
            this.label10.TabIndex = 35;
            this.label10.Text = "Library info";
            // 
            // txtRadioSerialNo
            // 
            this.txtRadioSerialNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRadioSerialNo.Location = new System.Drawing.Point(17, 158);
            this.txtRadioSerialNo.Name = "txtRadioSerialNo";
            this.txtRadioSerialNo.ReadOnly = true;
            this.txtRadioSerialNo.Size = new System.Drawing.Size(208, 22);
            this.txtRadioSerialNo.TabIndex = 34;
            this.txtRadioSerialNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtModule
            // 
            this.txtModule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtModule.Location = new System.Drawing.Point(17, 117);
            this.txtModule.Name = "txtModule";
            this.txtModule.ReadOnly = true;
            this.txtModule.Size = new System.Drawing.Size(208, 22);
            this.txtModule.TabIndex = 33;
            this.txtModule.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtRadioName
            // 
            this.txtRadioName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRadioName.Location = new System.Drawing.Point(17, 76);
            this.txtRadioName.Name = "txtRadioName";
            this.txtRadioName.ReadOnly = true;
            this.txtRadioName.Size = new System.Drawing.Size(208, 22);
            this.txtRadioName.TabIndex = 32;
            this.txtRadioName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // comboRadioModel
            // 
            this.comboRadioModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRadioModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRadioModel.FormattingEnabled = true;
            this.comboRadioModel.Location = new System.Drawing.Point(17, 33);
            this.comboRadioModel.Name = "comboRadioModel";
            this.comboRadioModel.Size = new System.Drawing.Size(208, 24);
            this.comboRadioModel.TabIndex = 31;
            this.comboRadioModel.SelectedIndexChanged += new System.EventHandler(this.comboRadioModel_SelectedIndexChanged);
            // 
            // btnRadioRefresh
            // 
            this.btnRadioRefresh.Location = new System.Drawing.Point(81, 404);
            this.btnRadioRefresh.Name = "btnRadioRefresh";
            this.btnRadioRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRadioRefresh.TabIndex = 30;
            this.btnRadioRefresh.Text = "Refresh";
            this.btnRadioRefresh.UseVisualStyleBackColor = true;
            this.btnRadioRefresh.Click += new System.EventHandler(this.btnRadioRefresh_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(101, 17);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 13);
            this.label13.TabIndex = 29;
            this.label13.Text = "Device";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(100, 101);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(42, 13);
            this.label12.TabIndex = 28;
            this.label12.Text = "Module";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(96, 142);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Serial no.";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(104, 60);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Name";
            // 
            // LimeSDRControllerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.close;
            this.ClientSize = new System.Drawing.Size(516, 498);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.close);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LimeSDRControllerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "LimeSDR Controller YT7PWR   v0.1";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LimeSDRControllerDialog_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.gainBar)).EndInit();
            this.grpChannel.ResumeLayout(false);
            this.grpAntenna.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udSpecOffset)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_PGAGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_TIAGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLimeSDR_LNAGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFrequencyDiff)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button close;
        public System.Windows.Forms.ComboBox samplerateComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar gainBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label gainDB;
        private System.Windows.Forms.RadioButton rx0;
        private System.Windows.Forms.RadioButton rx1;
        private System.Windows.Forms.RadioButton ant_h;
        private System.Windows.Forms.RadioButton ant_l;
        private System.Windows.Forms.RadioButton ant_w;
        private System.Windows.Forms.GroupBox grpAntenna;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox LPBWcomboBox;
        private System.Windows.Forms.NumericUpDown udSpecOffset;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtRadioSerialNo;
        private System.Windows.Forms.TextBox txtModule;
        private System.Windows.Forms.TextBox txtRadioName;
        private System.Windows.Forms.ComboBox comboRadioModel;
        private System.Windows.Forms.Button btnRadioRefresh;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblLimeSDR_LNAGain;
        private System.Windows.Forms.Label lblLimeSDR_TIAGain;
        private System.Windows.Forms.Label lblLimeSDR_PGAGain;
        private System.Windows.Forms.TrackBar tbLimeSDR_PGAGain;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar tbLimeSDR_TIAGain;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar tbLimeSDR_LNAGain;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown udFrequencyDiff;
        private System.Windows.Forms.TextBox txtLimeSuiteVersion;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtRadioModel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtGatewareVersion;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtFirm_version;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtSerialNumber;
        private System.Windows.Forms.Label label17;
        public System.Windows.Forms.GroupBox grpChannel;
    }
}