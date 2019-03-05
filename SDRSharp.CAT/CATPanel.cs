using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using SDRSharp.Radio;

namespace SDRSharp.CAT
{
    public partial class CATPanel : UserControl
    {
        #region variable

        CATPlugin _owner;

        #endregion

        #region constructor

        /// <summary>
        /// 
        /// </summary>
        public CATPanel(CATPlugin parrent)
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
            _owner = parrent;

            comboCOMport.Text = Utils.GetStringSetting("COMport name", "");
            comboSpeed.Text = Utils.GetStringSetting("COMport speed", "");
            comboParity.Text = Utils.GetStringSetting("COMport parity", "");
            chkCATenable.Checked = Utils.GetBooleanSetting("CAT enabled");
        }

        #endregion

        #region Settings

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkCATenable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkCATenable.Checked)
                    _owner.OpenSerialPort(comboCOMport.Text, int.Parse(comboSpeed.Text), comboParity.Text);
                else
                    _owner.CloseSerialPort();

                Utils.SaveSetting("CAT enabled", chkCATenable.Checked.ToString());
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Utils.SaveSetting("COMport speed", comboSpeed.Text);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboParity_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Utils.SaveSetting("COMport parity", comboParity.Text);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboCOMport_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Utils.SaveSetting("COMport name", comboCOMport.Text);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        #endregion
    }
}
