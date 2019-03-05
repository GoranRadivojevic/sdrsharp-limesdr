/*
 * Based on project from https://github.com/jocover/sdrsharp-limesdr
 * 
 * modifications by YT7PWR 2018
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SDRSharp.Radio;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SDRSharp.LimeSDR
{
    public partial class LimeSDRControllerDialog : Form
    {
        private readonly LimeSDRIO _owner;
        private bool _initialized;
        public double _sampleRate = 1.5 * 1e6;
        public double _freqDiff = 0.0;

        public LimeSDRControllerDialog(LimeSDRIO owner)
        {
            try
            {
                InitializeComponent();
                float dpi = this.CreateGraphics().DpiX;
                float ratio = dpi / 96.0f;
                string font_name = this.Font.Name;
                float size = (float)(8.25 / ratio);
                System.Drawing.Font new_font = new System.Drawing.Font(font_name, size);
                this.Font = new_font;
                this.PerformAutoScale();
                this.PerformLayout();
                _owner = owner;
                InitSampleRates();
                GetDeviceList();

                _initialized = true;

                comboRadioModel.Text = Utils.GetStringSetting("LimeSDR model", "");
                gainBar.Value = Utils.GetIntSetting("LimeSDR Gain", 40);
                gainBar_Scroll(this, EventArgs.Empty);
                gainDB.Text = gainBar.Value.ToString();
                samplerateComboBox.Text = Utils.GetStringSetting("LimeSDR SampleRate", "768000");
                LPBWcomboBox.Text = Utils.GetStringSetting("LimeSDR LPBW", "1.5MHz");
                rx0.Checked = Utils.GetBooleanSetting("LimeSDR RX0");
                rx1.Checked = Utils.GetBooleanSetting("LimeSDR RX1");
                ant_l.Checked = Utils.GetBooleanSetting("LimeSDR ANT_L");
                ant_h.Checked = Utils.GetBooleanSetting("LimeSDR ANT_H");
                ant_w.Checked = Utils.GetBooleanSetting("LimeSDR ANT_W");
                udSpecOffset.Value = (decimal)Utils.GetDoubleSetting("LimeSDR SpecOffset", 50);
                udFrequencyDiff.Value = (decimal)Utils.GetDoubleSetting("LimeSDR Frequency diff.", 0.0);
            }
            catch(Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private bool Initialized
        {
            get
            {
                return _initialized && _owner != null;
            }
        }

        private void InitSampleRates()
        {
            samplerateComboBox.Items.Add("192000");
            samplerateComboBox.Items.Add("384000");
            samplerateComboBox.Items.Add("768000");
            samplerateComboBox.Items.Add("1536000");
            samplerateComboBox.Items.Add("2304000");
            samplerateComboBox.Items.Add("3072000");
            samplerateComboBox.Items.Add("6144000");
            samplerateComboBox.Items.Add("8192000");
            samplerateComboBox.Items.Add("12288000");
            samplerateComboBox.Items.Add("19200000");
            samplerateComboBox.Items.Add("24576000");
            samplerateComboBox.Items.Add("30000000");
            samplerateComboBox.Items.Add("35000000");
            samplerateComboBox.Items.Add("40000000");
            samplerateComboBox.Items.Add("49152000");
            samplerateComboBox.Items.Add("55296000");
            samplerateComboBox.Text = "768000";
        }

        public double LPBW
        {
            get { return (double)(UInt32)(double.Parse(LPBWcomboBox.Text.Replace("MHz", "")) * 1e6); }
        }

        private void samplerateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                _sampleRate = double.Parse((samplerateComboBox.Text));
                return;
            }

            try
            {
                _sampleRate = double.Parse((samplerateComboBox.Text));
                Utils.SaveSetting("LimeSDR SampleRate", samplerateComboBox.Text);
                _owner._sampleRate = _sampleRate;

                if (_owner.Device != null)
                    _owner.Device.SampleRate = _sampleRate;
            }
            catch
            {
                _sampleRate = 1.5 * 1e6;
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void LimeSDRControllerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }       

        private void gainBar_Scroll(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            _owner.Gain = gainBar.Value;
            gainDB.Text = gainBar.Value + " dB";
            Utils.SaveSetting("LimeSDR Gain", (int)gainBar.Value);
        }

        private void rx0_CheckedChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            if(rx0.Checked)
                _owner.Channel = 0;

            Utils.SaveSetting("LimeSDR RX0", rx0.Checked);
        }

        private void rx1_CheckedChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            if(rx1.Checked)
                _owner.Channel = 1;

            Utils.SaveSetting("LimeSDR RX1", rx1.Checked);
        }

        private void ant_h_CheckedChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            if (ant_h.Checked)
                _owner.Antenna = 1;

            Utils.SaveSetting("LimeSDR ANT_H", ant_h.Checked);
        }

        private void ant_l_CheckedChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            if(ant_l.Checked)
                _owner.Antenna = 2;

            Utils.SaveSetting("LimeSDR ANT_L", ant_l.Checked);
        }

        private void ant_w_CheckedChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            if (ant_w.Checked)
                _owner.Antenna = 3;

            Utils.SaveSetting("LimeSDR ANT_W", ant_w.Checked);
        }

        private void LPBWcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            try
            {
                _owner.LPBW = (double)(UInt32)(double.Parse(LPBWcomboBox.Text.Replace("MHz", "")) * 1e6);
                Utils.SaveSetting("LimeSDR LPBW", LPBWcomboBox.Text);
            }
            catch
            {
                _owner.LPBW = 1.5 * 1e6;
            }
        }

        private void udSpecOffset_ValueChanged(object sender, EventArgs e)
        {
            if (!Initialized)
            {
                return;
            }

            try
            {
                if(_owner != null && _owner.Device != null)
                    _owner.Device.SpectrumOffset = (float)udSpecOffset.Value;

                if(_owner != null)
                    _owner.SpecOffset = (float)udSpecOffset.Value;

                Utils.SaveSetting("LimeSDR SpecOffset", udSpecOffset.Value.ToString());
            }
            catch
            {
                
            }
        }

        private void btnRadioRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (_owner != null && !_owner._isStreaming)
                {
                    comboRadioModel.Items.Clear();
                    GetDeviceList();
                    comboRadioModel.Text = Utils.GetStringSetting("LimeSDR model", "");
                }

                GetLimeSDRDeviceInfo();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        unsafe public void GetDeviceList()
        {
            try
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] info = new byte[2048];
                int count = 0;

                fixed (byte* deviceList = &info[0])
                {
                    count = NativeMethods.LMS_GetDeviceList(deviceList);
                }

                for (int i = 0; i < count; i++)
                    comboRadioModel.Items.Add(ascii.GetString(info, i * 256, 256).Trim('\0'));
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void comboRadioModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string[] data = new string[5];
                data = comboRadioModel.SelectedItem.ToString().Split(',');
                txtRadioName.Text = data[0];
                txtRadioSerialNo.Text = data[3].Replace("serial=", "");
                txtModule.Text = data[2].Replace("module=", "");
                _owner.RadioName = comboRadioModel.SelectedItem.ToString();
                Utils.SaveSetting("LimeSDR model", comboRadioModel.Text);

                txtRadioModel.Text = "";
                txtFirm_version.Text = "";
                txtSerialNumber.Text = "";
                txtGatewareVersion.Text = "";
                txtLimeSuiteVersion.Text = "";
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void udFrequencyDiff_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _freqDiff = (double)udFrequencyDiff.Value * 1e3;

                if(_owner != null)
                    _owner.FreqDiff = (double)udFrequencyDiff.Value * 1e3;

                Utils.SaveSetting("LimeSDR Frequency diff.", udFrequencyDiff.Value.ToString());
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void tbLimeSDR_LNAGain_Scroll(object sender, EventArgs e)
        {
            try
            {
                lblLimeSDR_LNAGain.Text = tbLimeSDR_LNAGain.Value.ToString() + "dB";

                if (_owner != null)
                    _owner.LNAgain = (ushort)tbLimeSDR_LNAGain.Value;

            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void tbLimeSDR_TIAGain_Scroll(object sender, EventArgs e)
        {
            try
            {
                lblLimeSDR_TIAGain.Text = tbLimeSDR_TIAGain.Value.ToString() + "dB";

                if (_owner != null)
                    _owner.TIAgain = (ushort)tbLimeSDR_TIAGain.Value;

            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void tbLimeSDR_PGAGain_Scroll(object sender, EventArgs e)
        {
            try
            {
                lblLimeSDR_PGAGain.Text = tbLimeSDR_PGAGain.Value.ToString() + "dB";

                if (_owner != null)
                    _owner.PGAgain = (ushort)tbLimeSDR_PGAGain.Value;

            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public unsafe void GetLimeSDRDeviceInfo()
        {
            try
            {
                if (_owner.LimeSDR_Device != IntPtr.Zero)
                {
                    lms_dev_info_t info = new lms_dev_info_t();

                    info.deviceName = new char[32];
                    info.expansionName = new char[32];
                    info.firmwareVersion = new char[16];
                    info.hardwareVersion = new char[16];
                    info.protocolVersion = new char[16];
                    info.boardSerialNumber = 0;
                    info.gatewareVersion = new char[16];
                    info.gatewareTargetBoard = new char[32];
                    IntPtr deviceInfo;

                    deviceInfo = (IntPtr)NativeMethods.LMS_GetDeviceInfo(_owner.LimeSDR_Device);
                    byte[] buff = new byte[168];
                    Marshal.Copy(deviceInfo, buff, 0, 168);
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    string s = ascii.GetString(buff);
                    string deviceName = ascii.GetString(buff, 0, 32).Trim('\0');
                    string expansionName = ascii.GetString(buff, 32, 32).Trim('\0');
                    string firmwareVersion = ascii.GetString(buff, 64, 16).Trim('\0');
                    string hardwareVersion = ascii.GetString(buff, 80, 16).Trim('\0');
                    string protocolVersion = ascii.GetString(buff, 96, 16).Trim('\0');
                    UInt64 serial = 0;

                    for (int i = 8; i > 0; i--)
                    {
                        serial += buff[111 + i];

                        if (i > 1)
                            serial = serial << 8;
                    }

                    string boardSerialNumber = serial.ToString("X");
                    string gatewareVersion = ascii.GetString(buff, 120, 16).Trim('\0');
                    string gatewareTargetBoard = ascii.GetString(buff, 136, 32).Trim('\0');

                    IntPtr libVersion;
                    libVersion = (IntPtr)NativeMethods.LMS_GetLibraryVersion();
                    string limeSuiteVersion = Marshal.PtrToStringAnsi(libVersion);

                    txtRadioModel.Text = deviceName;
                    txtFirm_version.Text = firmwareVersion;
                    txtSerialNumber.Text = boardSerialNumber;
                    txtGatewareVersion.Text = gatewareVersion;
                    txtLimeSuiteVersion.Text = limeSuiteVersion;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                txtRadioModel.Text = "";
                txtFirm_version.Text = "";
                txtSerialNumber.Text = "";
                txtGatewareVersion.Text = "";
                txtLimeSuiteVersion.Text = "";
            }
        }
    }
}
