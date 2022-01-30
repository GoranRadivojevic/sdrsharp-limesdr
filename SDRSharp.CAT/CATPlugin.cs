

using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SDRSharp.CAT
{
    #region enum

    public enum DSPMode
    {
        FIRST = -1,
        LSB,
        USB,
        DSB,
        CWL,
        CWU,
        FMN,
        AM,
        DIGU,
        SPEC,
        DIGL,
        SAM,
        DRM,
        WFM,
        LAST,
    }

    public enum Filter
    {
        FIRST = -1,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        VAR1,
        VAR2,
        NONE,
        LAST,
    }

    public enum AGCMode
    {
        FIRST = -1,
        FIXD,
        LONG,
        SLOW,
        MED,
        FAST,
        CUSTOM,
        LAST,
    }

    #endregion

    public class CATPlugin : ISharpPlugin
    {
        #region variable

        private const string _displayName = "CAT control";
        private CATPanel _catPanel;
        private static SerialPort _serialPort;
        private ISharpControl _sdr;
        private Thread _sendThread;
        private AutoResetEvent _sendEvent;
        private static bool _run;
        private static bool _waiting;
        public delegate void CATCrossThreadCallback(string type, int parm1, int[] parm2, string parm3);
        private bool CATclosing = false;
        private bool CATEcho = true;
        private byte CATRigAddress = 0x70;
        private string CommBuffer;
        private byte[] send_data = new byte[1024];
        private ASCIIEncoding AE = new ASCIIEncoding();

        #endregion

        #region properties

        public string DisplayName
        {
            get { return "CAT control"; }
        }

        public UserControl Gui
        {
            get
            {
                return (UserControl)this._catPanel;
            }
        }

        private int cat_nr_gain = 0;
        public int CATNRgain
        {
            get { return 0; }
            set
            {
                if (_catPanel != null)
                {
                    value = Math.Max(1, value);
                    value = Math.Min(9999, value);
                    //SetupForm.udLMSNRgain.Value = value;
                    cat_nr_gain = value;
                }
            }
        }

        public int VOXSens
        {
            get
            {
                //if (ptbVOX != null) return ptbVOX.Value;
                //else return -1;
                return 0;
            }
            set
            {
                //udVOX.Value = value;
                //if (ptbVOX != null) ptbVOX.Value = value;
            }
        }

        public bool VOXEnable
        {
            get
            {
                //if (ptbVOX != null) return ptbVOX.Value;
                //else return -1;
                return false;
            }
            set
            {
                //udVOX.Value = value;
                //if (ptbVOX != null) ptbVOX.Value = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Frequency
        {
            get
            {
                double freq = (double)_sdr.Frequency;
                return freq;
            }
            set
            {
                long newFrequency = (long)(value * 1e6);
                long newCenterFrequency = newFrequency + (_sdr.RFBandwidth / 8);
                _sdr.Frequency = newFrequency;
                _sdr.ResetFrequency(newFrequency, newCenterFrequency);
                _sdr.Perform();
            }
        }

        private DSPMode current_dsp_mode = DSPMode.FIRST;
        public DSPMode CurrentDSPMode
        {
            get { return current_dsp_mode; }
            set
            {
                try
                {

                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                }
            }
        }

        private Filter current_filter = Filter.FIRST;
        public Filter CurrentFilter
        {
            get { return current_filter; }
            set
            {

            }
        }

        public AGCMode current_agc_mode = AGCMode.FAST;
        public AGCMode CurrentAGCMode
        {
            get { return current_agc_mode; }
            set
            {
                current_agc_mode = value;
            }
        }

        #endregion

        #region Init/Close

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        public void Initialize(ISharpControl control)
        {
            _sdr = control;
            _run = false;
            _waiting = false;
            _catPanel = new CATPanel(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            CATclosing = true;

            _run = false;

            if(_sendEvent != null)
                _sendEvent.Set();

            while (_waiting)
                Thread.Sleep(10);

            if (_serialPort != null)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialRXEventHandler);
                Thread.Sleep(100);
                //_serialPort.Close();
                _serialPort = (SerialPort)null;
            }

            if(_sendThread != null)
                this._sendThread.Join();
        }

        #endregion

        #region serial port

        /// <summary>
        /// Create and open serial port
        /// </summary>
        /// <param name="name"></param>
        /// <param name="speed"></param>
        /// <param name="parity"></param>
        public bool OpenSerialPort(string name, int speed, string parity)
        {
            try
            {
                _sdr.FilterBandwidth = 2500;
                _serialPort = new SerialPort(name, speed, Parity.None, 8, StopBits.One);
                _serialPort.Handshake = Handshake.None;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialRXEventHandler);
                _serialPort.Open();

                if(_serialPort.IsOpen)
                {
                    _run = true;
                    _sendEvent = new AutoResetEvent(false);
                    _sendThread = new Thread(new ThreadStart(SendThread));
                    _sendThread.Name = "Serial send Process Thread ";
                    _sendThread.Priority = ThreadPriority.Normal;
                    _sendThread.IsBackground = true;
                    _sendThread.Start();
                }

                return _serialPort.IsOpen;
            }
            catch(Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Close serial port
        /// </summary>
        /// <param name="name"></param>
        /// <param name="speed"></param>
        /// <param name="parity"></param>
        public bool CloseSerialPort()
        {
            try
            {
                if (_serialPort != null)
                {
                    _run = false;
                    _sendEvent.Set();
                    Thread.Sleep(100);
                    _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialRXEventHandler);
                    //_serialPort.Close();
                    _serialPort = (SerialPort)null;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SerialRXEventHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Regex rex = new Regex(".*?;");
                byte[] out_string = new byte[1024];
                int num_to_read = _serialPort.BytesToRead;
                byte[] inbuf = new byte[num_to_read];
                _serialPort.Read(inbuf, 0, num_to_read);
                CommBuffer += AE.GetString(inbuf, 0, inbuf.Length);

                byte[] buffer = new byte[inbuf.Length + 1];
                byte[] question = new byte[16];
                byte[] answer = new byte[16];
                byte[] question1 = new byte[1];
                int j = 0;

                for (int i = 0; i < inbuf.Length; i++)
                {
                    if (inbuf[i] == 0xfd)
                    {
                        question[j] = inbuf[i];

                        if (question[2] == CATRigAddress)
                        {
                            if (CATEcho)
                            {
                                if (question1.Length != j + 1)
                                    question1 = new byte[j + 1];

                                for (int k = 0; k < j + 1; k++)
                                    question1[k] = question[k];

                                send_data = question1;           // echo
                                _sendEvent.Set();
                                Thread.Sleep(10);
                            }

                            answer = Get(question);
                            send_data = answer;
                            _sendEvent.Set();
                            j = 0;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        question[j] = inbuf[i];
                        j++;
                    }
                }

                CommBuffer = "";
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendThread()
        {
            try
            {
                while (_run)
                {
                    _sendEvent.WaitOne();

                    if (_run)
                    {
                        lock (this)
                        {
                            if (_serialPort != null)
                            {
                                _serialPort.Write(send_data, 0, send_data.Length);
                            }
                        }
                    }
                }

                _waiting = false;
            }
            catch(Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCmdString"></param>
        /// <returns></returns>
        public byte[] Get(byte[] pCmdString)
        {
            byte[] answer = new byte[16];

            switch (pCmdString[4])
            {
                case 0:
                    answer = CMD_00(pCmdString);
                    break;

                case 1:
                    answer = CMD_01(pCmdString);
                    break;

                case 2:
                    answer = CMD_02(pCmdString);
                    break;

                case 3:
                    answer = CMD_03(pCmdString);
                    break;

                case 4:
                    answer = CMD_04(pCmdString);
                    break;

                case 5:
                    answer = CMD_05(pCmdString);
                    break;

                case 6:
                    answer = CMD_06(pCmdString);
                    break;

                case 7:
                    answer = CMD_07(pCmdString);
                    break;

                case 0x0f:
                    answer = CMD_0F(pCmdString);
                    break;

                case 0x11:
                    answer = CMD_11(pCmdString);
                    break;

                case 0x14:
                    answer = CMD_14(pCmdString);                // Various Level Settings
                    break;

                case 0x15:
                    answer = CMD_15(pCmdString);                // S meter
                    break;

                case 0x16:
                    answer = CMD_16(pCmdString);
                    break;

                case 0x19:
                    answer = CMD_19(pCmdString);
                    break;

                case 0x1A:
                    switch (pCmdString[5])
                    {
                        case 0x00:
                            answer = CMD_1A_00(pCmdString);
                            break;

                        case 0x03:
                            answer = CMD_1A_03(pCmdString);
                            break;

                        case 0x05:
                            answer = CMD_1A_03(pCmdString);
                            break;

                        default:
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = pCmdString[3];
                            answer[3] = pCmdString[2];
                            answer[4] = 0xfa;           // error code
                            answer[6] = 0xfd;
                            break;
                    }
                    break;

                case 0x1b:
                    answer = CMD_1B(pCmdString);
                    break;

                case 0x1C:
                    switch (pCmdString[5])
                    {
                        case 0x00:
                            answer = CMD_1C_00(pCmdString);
                            break;

                        case 0x01:
                            answer = CMD_1C_01(pCmdString);
                            break;

                        default:
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = pCmdString[3];
                            answer[3] = pCmdString[2];
                            answer[4] = 0xfa;           // error code
                            answer[6] = 0xfd;
                            break;
                    }
                    break;

                case 0x25:
                    answer = CMD_25(pCmdString);
                    break;

                case 0x26:
                    answer = CMD_26(pCmdString);
                    break;

                default:
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = pCmdString[3];
                    answer[3] = pCmdString[2];
                    answer[4] = 0xfa;           // error code
                    answer[6] = 0xfd;
                    break;
            }

            return answer;
        }

        #region ICOM CAT commands

        public byte[] CMD_00(byte[] command)     // transfer op freq data
        {
            try
            {
                return command;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_01(byte[] command)     // transfer op mode data
        {
            try
            {
                return command;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_02(byte[] command)     // read lower/upper freq data
        {
            try
            {
                return command;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_03(byte[] command)     // read op freq data
        {
            try
            {
                if (command[5] == 0xfd)
                {
                    byte[] answer = new byte[11];
                    byte[] frequency = new byte[10];
                    string freq = Frequency.ToString();
                    freq = freq.PadLeft(10, '0');
                    ASCIIEncoding buff = new ASCIIEncoding();
                    buff.GetBytes(freq, 0, freq.Length, frequency, 0);
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0x03;
                    answer[5] = (byte)((frequency[8] - 0x30) << 4 | (frequency[9] - 0x30));
                    answer[6] = (byte)((frequency[6] - 0x30) << 4 | (frequency[7] - 0x30));
                    answer[7] = (byte)((frequency[4] - 0x30) << 4 | (frequency[5] - 0x30));
                    answer[8] = (byte)((frequency[2] - 0x30) << 4 | (frequency[3] - 0x30));
                    answer[9] = (byte)((frequency[0] - 0x30) << 4 | (frequency[1] - 0x30));
                    answer[10] = 0xfd;

                    return answer;
                }
                else
                {
                    byte[] answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0xfa;               // error
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_04(byte[] command)     // read op mode data
        {
            try
            {
                byte[] answer = new byte[8];

                if (command[5] == 0xfd)
                {
                    switch (CurrentDSPMode)
                    {
                        case DSPMode.LSB:
                            answer[5] = 0;
                            break;

                        case DSPMode.USB:
                            answer[5] = 1;
                            break;

                        case DSPMode.AM:
                            answer[5] = 2;
                            break;

                        case DSPMode.CWU:
                            answer[5] = 3;
                            break;

                        case DSPMode.FMN:
                            answer[5] = 5;
                            break;

                        case DSPMode.CWL:
                            answer[5] = 7;
                            break;

                        case DSPMode.SAM:
                            answer[5] = 0x11;
                            break;
                    }

                    switch (CurrentFilter)
                    {
                        case Filter.F5:
                            answer[6] = 1;
                            break;

                        case Filter.F7:
                            answer[6] = 2;
                            break;

                        case Filter.F9:
                            answer[6] = 3;
                            break;

                        default:
                            answer[6] = 1;
                            break;
                    }

                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0x04;
                    answer[7] = 0xfd;
                    return answer;
                }
                else
                {
                    answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0xfa;               // error
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_05(byte[] command)     // write op freq to VFO
        {
            try
            {
                ASCIIEncoding buff = new ASCIIEncoding();
                byte[] answer = new byte[6];
                byte[] frequency = new byte[10];
                int[] parm2 = new int[1];

                frequency[1] = (byte)((command[9] & 0x0f) + 0x30);
                frequency[0] = (byte)((command[9] >> 4 & 0x0f) + 0x30);
                frequency[3] = (byte)((command[8] & 0x0f) + 0x30);
                frequency[2] = (byte)((command[8] >> 4 & 0x0f) + 0x30);
                frequency[5] = (byte)((command[7] & 0x0f) + 0x30);
                frequency[4] = (byte)((command[7] >> 4 & 0x0f) + 0x30);
                frequency[7] = (byte)((command[6] & 0x0f) + 0x30);
                frequency[6] = (byte)((command[6] >> 4 & 0x0f) + 0x30);
                frequency[9] = (byte)((command[5] & 0x0f) + 0x30);
                frequency[8] = (byte)((command[5] >> 4 & 0x0f) + 0x30);
                string freq = buff.GetString(frequency);

                double vfoA = double.Parse(freq);
                vfoA /= 1e6;

               Frequency = double.Parse(freq) / 1e6;

                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];         // swapp address
                answer[3] = command[2];
                answer[4] = 0xfb;               // OK
                answer[5] = 0xfd;

                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_06(byte[] command)                                                // write op mode to VFO
        {
            try
            {
                if (command[5] == 0xfd)
                {

                }
                else                                                                        // set mode
                {
                    switch (command[5])
                    {
                        case 0:
                            CurrentDSPMode = DSPMode.LSB;
                            break;

                        case 1:
                            CurrentDSPMode = DSPMode.USB;
                            break;

                        case 2:
                            CurrentDSPMode = DSPMode.AM;
                            break;

                        case 3:
                            CurrentDSPMode = DSPMode.CWU;
                            break;

                        case 5:
                        case 6:
                            CurrentDSPMode = DSPMode.FMN;
                            break;

                        case 7:
                            CurrentDSPMode = DSPMode.CWL;
                            break;

                        case 0x11:
                            CurrentDSPMode = DSPMode.SAM;
                            break;
                    }

                    switch (command[6])
                    {
                        case 1:
                            CurrentFilter = Filter.F5;
                            break;

                        case 2:
                            CurrentFilter = Filter.F7;
                            break;

                        case 3:
                            CurrentFilter = Filter.F9;
                            break;
                    }
                }

                byte[] answer = new byte[6];
                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0xfb;           // ok
                answer[5] = 0xfd;
                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_07(byte[] command)                                                // select VFO mode
        {
            try
            {
                byte[] answer = new byte[7];
                int[] parm2 = new int[1];

                if (command[5] == 0xfd)
                {
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0x07;
                    answer[5] = 0; // (byte)CATVFOMODE;
                    answer[6] = 0xfd;
                }
                else                                                                        // set mode
                {
                    switch (command[5])
                    {
                        case 0:         // VFOA
                            if (!CATclosing)
                                CATCallback("VFO mode", 0, parm2, "");
                            break;

                        case 1:         // VFOB
                            if (!CATclosing)
                                CATCallback("VFO mode", 1, parm2, "");
                            break;

                        case 0xa0:      // VFOA=VFOB
                            if (!CATclosing)
                                CATCallback("VFO mode", 0xa0, parm2, "");
                            break;

                        case 0xb0:      // VFOA/VFOB exchange
                            if (!CATclosing)
                                CATCallback("VFO mode", 0xb0, parm2, "");
                            break;

                        case 0xb1:      // VFOA=VFOB
                            if (!CATclosing)
                                CATCallback("VFO mode", 0xb1, parm2, "");
                            break;

                        case 0xc0:      // SUB RX off
                            if (!CATclosing)
                                CATCallback("VFO mode", 0xc0, parm2, "");
                            break;

                        case 0xc1:      // SUB RX on
                            if (!CATclosing)
                                CATCallback("VFO mode", 0xc1, parm2, "");
                            break;

                        default:
                            answer = new byte[6];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfa;           // error
                            answer[5] = 0xfd;
                            return answer;
                    }

                    switch (command[6])
                    {
                        case 1:
                            CurrentFilter = Filter.F5;
                            break;

                        case 2:
                            CurrentFilter = Filter.F7;
                            break;

                        case 3:
                            CurrentFilter = Filter.F9;
                            break;
                    }
                }

                answer = new byte[6];
                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0xfb;           // ok
                answer[5] = 0xfd;
                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_0F(byte[] command)     // SPLIT mode
        {
            try
            {
                byte[] answer = new byte[7];
                int[] parm2 = new int[1];

                if (command[5] == 0xfd)
                {
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0x0f;

                    //if (!SplitAB_TX)
                        //answer[5] = 0x00;
                    //else
                        answer[5] = 0x01;

                    answer[6] = 0xfd;
                    return answer;
                }
                else
                {
                    answer = new byte[6];

                    if (!CATclosing)
                        CATCallback("SPLIT", command[5], parm2, "");

                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0xfb;               // ok
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_11(byte[] command)        // ATT state
        {
            try
            {
                byte[] answer = new byte[7];
                int[] parm2 = new int[1];

                if (command[5] == 0xfd)
                {
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0x11;

                    answer[5] = 0x00;

                    answer[6] = 0xfd;
                    return answer;
                }
                else
                {
                    answer = new byte[6];

                    if (command[5] == 0)
                    {
                        if (!CATclosing)
                            CATCallback("ATT", 0, parm2, "");
                    }
                    else if (command[5] == 0x12)
                        if (!CATclosing)
                            CATCallback( "ATT", 1, parm2, "");

                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0xfb;               // ok
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_14(byte[] command)                        // Various Level Settings
        {
            try
            {
                byte[] answer = new byte[6];
                int[] parm2 = new int[1];
                int parm1 = 0;
                string v = "";
                string s = "";

                if (command[6] == 0xfd)
                {
                    switch (command[5])
                    {
                        case 1:                                         // AF level
                            answer = new byte[9];

                            /*if (MUT)
                            {
                                answer[6] = 0x00;
                                answer[7] = 0x00;
                            }
                            else*/
                            {
                                int vol = 0; // (int)(AF * 2.55);
                                s = vol.ToString("D");

                                if (s.Length == 3)
                                {
                                    string s1 = s.Remove(0, 1);
                                    int j = int.Parse(s1, NumberStyles.HexNumber);
                                    string s2 = s.Remove(1);
                                    int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                    answer[6] = (byte)(j1);
                                    answer[7] = (byte)(j);
                                }
                                else if (s.Length == 2 || s.Length == 1)
                                {
                                    int j = int.Parse(s, NumberStyles.HexNumber);
                                    answer[7] = (byte)(j & 0x00ff);
                                    answer[6] = (byte)(j >> 8);
                                }
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x01;
                            answer[8] = 0xfd;
                            return answer;

                        case 2:                                         // RF level
                            answer = new byte[9];
                            int rf = 0; // (int)(((RF + 20) * 255) / 140);
                            s = rf.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x02;
                            answer[8] = 0xfd;
                            return answer;

                        case 3:                                         // SQL level
                            answer = new byte[9];

                            int sql = 0; // (int)((SquelchMainRX * 255) / 160);
                            s = sql.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x03;
                            answer[8] = 0xfd;
                            return answer;

                        case 6:                                         // NR gain
                            answer = new byte[9];
                            int nr = (int)((CATNRgain * 255) / 1000);
                            s = nr.ToString("D");

                            if (s.Length == 3 || s.Length == 4)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2 || s.Length == 1)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x06;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x0a:                                         // PWR level
                            answer = new byte[9];

                            int pwr = 0; // (int)(PWR * 2.55);
                            s = pwr.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x0a;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x0b:                                         // mic gain
                            answer = new byte[9];

                            int mic = 0; // (int)(CATMIC * 2.55);
                            s = mic.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x0b;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x0d:                                         // NOTCH gain
                            answer = new byte[9];
                            int notch = 0; // (int)(((CATNOTCHManual + 5000) * 255) / 10000);
                            s = notch.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x0d;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x0e:                                         // COMP level
                            answer = new byte[9];

                            int cmp = 0; // (int)(COMPVal * 255 / 20);
                            s = cmp.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2 || s.Length == 1)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x0e;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x12:                                         // NB level
                            answer = new byte[9];

                            int nb_level = 0; // (int)(CATNB1Threshold * 2.55);
                            s = nb_level.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x12;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x16:                                         // VOX level
                            answer = new byte[9];
                            int vox = (int)((VOXSens * 255) / 1000);
                            s = vox.ToString("D");

                            if (s.Length == 3)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x16;
                            answer[8] = 0xfd;
                            return answer;

                        case 0x1a:                                         // manual NOTCH shift
                            answer = new byte[9];
                            int notch_manual = 0; // (int)(((CATNOTCHManual + 5000) * 255) / 10000);
                            s = notch_manual.ToString("D");

                            if (s.Length == 3 || s.Length == 4)
                            {
                                string s1 = s.Remove(0, 1);
                                int j = int.Parse(s1, NumberStyles.HexNumber);
                                string s2 = s.Remove(1);
                                int j1 = int.Parse(s2, NumberStyles.HexNumber);
                                answer[6] = (byte)(j1);
                                answer[7] = (byte)(j);
                            }
                            else if (s.Length == 2)
                            {
                                int j = int.Parse(s, NumberStyles.HexNumber);
                                answer[7] = (byte)(j & 0x00ff);
                                answer[6] = (byte)(j >> 8);
                            }

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x14;
                            answer[5] = 0x1a;
                            answer[8] = 0xfd;
                            return answer;

                        default:
                            answer = new byte[6];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfa;               // error
                            answer[5] = 0xfd;
                            return answer;

                    }
                }
                else
                {
                    switch (command[5])
                    {
                        case 1:                                         // AF level
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 100) / 255);

                            if (!CATclosing)
                                CATCallback( "AF", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 2:                                         // RF gain
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)(((parm1 - 20) * 140) / 255);

                            if (!CATclosing)
                                CATCallback( "RF", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 3:                                         // SQL level
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            if (!CATclosing)
                                CATCallback( "SQL VFOA", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 6:                                         // NR gain
                            answer = new byte[6];
                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 1000) / 255);

                            if (!CATclosing)
                                CATCallback( "NR gain", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 9:                                         //  gain
                            answer = new byte[6];
                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 1000) / 255);

                            if (!CATclosing)
                                CATCallback( "NR gain", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x0a:                                     // PWR gain
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 100) / 255);

                            if (!CATclosing)
                                CATCallback( "PWR", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x0b:                                     // MIC gain
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 70) / 255);

                            if (!CATclosing)
                                CATCallback( "MIC", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x0d:                                     // ANF state
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 10000) / 255);

                            if (!CATclosing)
                                CATCallback( "NOTCH manual", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x0e:                                     // COMP level
                            answer = new byte[6];

                            if (command[7] == 0xfd)
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }
                            else
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }

                            parm1 = (int)((parm1 * 20) / 255);

                            if (!CATclosing)
                                CATCallback( "COMP level", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x16:                                     // VOX sense
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            parm1 = (int)((parm1 * 1000) / 255);

                            if (!CATclosing)
                                CATCallback( "VOX Gain", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        case 0x1a:                                     // manual NOTCH frequency
                            answer = new byte[6];

                            if (command[8] == 0xfd)                     // 2 byte value
                            {
                                v = command[7].ToString("X");
                                parm1 = int.Parse(v);
                                parm1 += command[6] * 100;
                            }
                            else if (command[7] == 0xfd)                // 1 byte value
                            {
                                v = command[6].ToString("X");
                                parm1 = int.Parse(v);
                            }

                            if (parm1 > +128)
                                parm1 = (int)((parm1 - 128) * (5000 / 64));
                            else
                                parm1 = (int)(((parm1 - 64) * 5000) / 64) - 5000;

                            if (!CATclosing)
                                CATCallback( "NOTCH manual", parm1, parm2, "");

                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfb;
                            answer[5] = 0xfd;
                            return answer;

                        default:
                            answer = new byte[6];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0xfa;               // error
                            answer[5] = 0xfd;
                            return answer;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_15(byte[] command)                        // S meter
        {
            try
            {
                byte[] answer = new byte[6];
                int[] parm2 = new int[1];

                switch (command[5])
                {
                    case 2:                                         // S meter
                        short sm = 0;
                        float num = 0f;

                        num = 0; // DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AVG_SIGNAL_STRENGTH);
                        num = Math.Max(0, num);
                        num = Math.Min(512, num);
                        sm = (short)(num * (512 / 130));

                        if (sm > 0x0100)
                            sm += 0x80;

                        answer = new byte[9];
                        answer[0] = 0xfe;
                        answer[1] = 0xfe;
                        answer[2] = command[3];
                        answer[3] = command[2];
                        answer[4] = 0x15;
                        answer[5] = 0x02;
                        answer[6] = (byte)(sm >> 8);
                        answer[7] = (byte)(sm & 0x00ff);
                        answer[8] = 0xfd;
                        return answer;

                    case 0x11:
                        sm = 0;
                        int pwr = 0;

                        //if (PowerOn)
                            //pwr = 50; // *(int)PAPower((int)g59.fwd_PWR);

                        answer = new byte[9];
                        answer[0] = 0xfe;
                        answer[1] = 0xfe;
                        answer[2] = command[3];
                        answer[3] = command[2];
                        answer[4] = 0x15;
                        answer[5] = 0x11;
                        answer[6] = (byte)(pwr >> 8);
                        answer[7] = (byte)(pwr & 0x00ff);
                        answer[8] = 0xfd;
                        return answer;

                    case 0x12:
                        sm = 0;
                        int swr = 0;

                        //if (PowerOn)
                            //swr = 50; // *(int)PAPower((int)g59.Ref_PWR);

                        answer = new byte[9];
                        answer[0] = 0xfe;
                        answer[1] = 0xfe;
                        answer[2] = command[3];
                        answer[3] = command[2];
                        answer[4] = 0x15;
                        answer[5] = 0x12;
                        answer[6] = (byte)(swr >> 8);
                        answer[7] = (byte)(swr & 0x00ff);
                        answer[8] = 0xfd;
                        return answer;

                    default:
                        answer[0] = 0xfe;
                        answer[1] = 0xfe;
                        answer[2] = command[3];
                        answer[3] = command[2];
                        answer[4] = 0xfa;           // error
                        answer[5] = 0xfd;
                        return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_16(byte[] command)                        // many stuff
        {
            try
            {
                byte[] answer = new byte[6];
                int[] parm2 = new int[1];

                switch (command[5])
                {
                    case 0x02:                                      // RF preamp
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x02;

                            answer[6] = 0;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else if (command[6] == 0)
                        {
                            if (!CATclosing)
                                CATCallback( "RF preamp", 0, parm2, "");
                        }
                        else if (command[6] == 1 || command[6] == 2)
                            if (!CATclosing)
                                CATCallback( "RF preamp", 1, parm2, "");
                        break;

                    case 0x12:                                      // AGC
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x12;

                            if (current_agc_mode == AGCMode.FAST)
                                answer[6] = 1;
                            else if (current_agc_mode == AGCMode.MED)
                                answer[6] = 2;
                            if (current_agc_mode == AGCMode.SLOW)
                                answer[6] = 3;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                        {
                            switch (command[6])
                            {
                                case 1:
                                    if (!CATclosing)
                                        CATCallback( "AGC mode",
                                        (int)AGCMode.FAST, parm2, "");
                                    break;

                                case 2:
                                    if (!CATclosing)
                                        CATCallback( "AGC mode",
                                        (int)AGCMode.MED, parm2, "");
                                    break;

                                case 3:
                                    if (!CATclosing)
                                        CATCallback( "AGC mode",
                                        (int)AGCMode.SLOW, parm2, "");
                                    break;
                            }
                        }
                        break;

                    case 0x22:                                      // NB
                        {
                            if (command[6] == 0xfd)
                            {
                                answer = new byte[8];
                                answer[0] = 0xfe;
                                answer[1] = 0xfe;
                                answer[2] = command[3];
                                answer[3] = command[2];
                                answer[4] = 0x16;
                                answer[5] = 0x22;

                                //if (CATNB1 == 1)
                                    //answer[6] = 0x01;
                                //else
                                    answer[6] = 0x00;

                                answer[7] = 0xfd;
                                return answer;
                            }
                            else if (command[6] == 1)
                            {
                                if (!CATclosing)
                                    CATCallback( "NB1", 1, parm2, "");
                            }
                            else
                                if (!CATclosing)
                                CATCallback( "NB1", 0, parm2, "");
                        }
                        break;

                    case 0x40:                                      // NR
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x40;

                            //if (CATNR == 1)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else if (command[6] == 1)
                        {
                            if (!CATclosing)
                                CATCallback( "NR", 1, parm2, "");
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "NR", 0, parm2, "");
                        break;

                    case 0x41:                                      // ANF
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x41;

                            //if (CATANF == 1)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else if (command[6] == 1)
                        {
                            if (!CATclosing)
                                CATCallback( "ANF", 1, parm2, "");
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "ANF", 0, parm2, "");
                        break;

                    case 0x42:                                      // Repeater tone
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x42;

                            //if (CTCSS)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "RPT tone", command[6], parm2, "");
                        break;

                    case 0x43:                                      // squelch
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x43;

                            //if (CATSquelch == 1)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "SQL VFOA Enable", command[6], parm2, "");
                        break;

                    case 0x44:                                      // COMP
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x44;

                            //if (COMP == true)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "COMP", command[6], parm2, "");
                        break;

                    case 0x45:                                      // MON state
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x45;

                            //if (MON)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "MON", command[6], parm2, "");
                        break;

                    case 0x46:                                      // VOX
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x46;

                            if (VOXEnable)
                                answer[6] = 0x01;
                            else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "VOX", command[6], parm2, "");
                        break;

                    case 0x48:                                      // Manual NOTCH or ANF
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x48;

                           //if (CATNOTCHenable == 1)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "NOTCH manual enable",
                            command[6], parm2, "");
                        break;

                    case 0x50:                                      // dial lock
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x50;

                            //if (CATVFOLock)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "VFO Lock", command[6], parm2, "");
                        break;

                    case 0x51:                                      // Manual NOTCH2
                        if (command[6] == 0xfd)
                        {
                            answer = new byte[8];
                            answer[0] = 0xfe;
                            answer[1] = 0xfe;
                            answer[2] = command[3];
                            answer[3] = command[2];
                            answer[4] = 0x16;
                            answer[5] = 0x51;

                            //if (CATNOTCHenable == 1)
                                //answer[6] = 0x01;
                            //else
                                answer[6] = 0x00;

                            answer[7] = 0xfd;
                            return answer;
                        }
                        else
                            if (!CATclosing)
                            CATCallback( "NOTCH manual enable",
                            command[6], parm2, "");
                        break;


                    default:
                        answer[0] = 0xfe;
                        answer[1] = 0xfe;
                        answer[2] = command[3];
                        answer[3] = command[2];
                        answer[4] = 0xfa;                           // error
                        answer[5] = 0xfd;
                        return answer;
                }

                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0xfb;                                   // ok
                answer[5] = 0xfd;

                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_19(byte[] command)        // ID
        {
            try
            {
                byte[] answer = new byte[7];
                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0x19;
                answer[5] = 0x70; // (byte)CATRigAddress;           // IC-7000 adress
                answer[6] = 0xfd;
                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_1B(byte[] command)     // read repeater mode
        {
            try
            {
                byte[] answer = new byte[7];
                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0x19;
                answer[5] = 0x70;
                answer[6] = 0xfd;
                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_1A_00(byte[] command)     // R/W ext memory
        {
            try
            {
                byte[] answer = new byte[7];
                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0x19;
                answer[5] = 0x70;
                answer[6] = 0xfd;
                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_1A_03(byte[] command)     // R/W IF filter
        {
            try
            {
                byte[] answer = new byte[8];
                int[] parm2 = new int[1];

                answer[0] = 0xfe;
                answer[1] = 0xfe;
                answer[2] = command[3];
                answer[3] = command[2];
                answer[4] = 0x1a;
                answer[5] = 0x03;

                if (command[6] == 0xfd)
                {
                    switch (CurrentDSPMode)
                    {
                        case DSPMode.CWL:
                        case DSPMode.CWU:
                            switch (CurrentFilter)
                            {
                                case Filter.F1:
                                    answer[6] = 14;
                                    break;

                                case Filter.F2:
                                    answer[6] = 12;
                                    break;

                                case Filter.F3:
                                    answer[6] = 11;
                                    break;

                                case Filter.F4:
                                    answer[6] = 10;
                                    break;

                                case Filter.F5:
                                    answer[6] = 9;
                                    break;

                                case Filter.F6:
                                    answer[6] = 7;
                                    break;

                                case Filter.F7:
                                    answer[6] = 4;
                                    break;

                                case Filter.F8:
                                    answer[6] = 1;
                                    break;

                                case Filter.F9:
                                    answer[6] = 0;
                                    break;

                                default:
                                    answer[6] = 0;
                                    break;
                            }
                            break;

                        case DSPMode.USB:
                        case DSPMode.LSB:
                            switch (CurrentFilter)
                            {
                                case Filter.F1:
                                    answer[6] = 0;
                                    break;

                                case Filter.F2:
                                    answer[6] = 0;
                                    break;

                                case Filter.F3:
                                    answer[6] = 58;
                                    break;

                                case Filter.F4:
                                    answer[6] = 55;
                                    break;

                                case Filter.F5:
                                    answer[6] = 45;
                                    break;

                                case Filter.F6:
                                    answer[6] = 43;
                                    break;

                                case Filter.F7:
                                    answer[6] = 40;
                                    break;

                                case Filter.F8:
                                    answer[6] = 37;
                                    break;

                                case Filter.F9:
                                    answer[6] = 34;
                                    break;

                                case Filter.F10:
                                    answer[6] = 14;
                                    break;

                                default:
                                    answer[6] = 0;
                                    break;
                            }
                            break;

                        default:

                            break;
                    }

                    answer[7] = 0xfd;
                    return answer;
                }
                else
                {
                    switch (CurrentDSPMode)
                    {
                        case DSPMode.CWL:
                        case DSPMode.CWU:
                            switch (command[6])
                            {
                                case 0x14:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F1, parm2, "");
                                    break;

                                case 0x12:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F2, parm2, "");
                                    break;

                                case 0x11:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F3, parm2, "");
                                    break;

                                case 0x10:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F4, parm2, "");
                                    break;

                                case 9:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F5, parm2, "");
                                    break;

                                case 7:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F6, parm2, "");
                                    break;

                                case 4:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F7, parm2, "");
                                    break;

                                case 1:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F8, parm2, "");
                                    break;

                                case 0:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F9, parm2, "");
                                    break;

                                default:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F1, parm2, "");
                                    break;
                            }
                            break;

                        case DSPMode.USB:
                        case DSPMode.LSB:
                            switch (command[6])
                            {
                                case 0x40:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F3, parm2, "");
                                    break;

                                case 0x37:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F4, parm2, "");
                                    break;

                                case 0x33:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F5, parm2, "");
                                    break;

                                case 0x31:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F6, parm2, "");
                                    break;

                                case 0x28:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F7, parm2, "");
                                    break;

                                case 0x25:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F8, parm2, "");
                                    break;

                                case 0x22:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F9, parm2, "");
                                    break;

                                case 0x14:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F10, parm2, "");
                                    break;

                                default:
                                    if (!CATclosing)
                                        CATCallback( "Filter", (int)Filter.F1, parm2, "");
                                    break;
                            }
                            break;

                        default:
                            break;
                    }

                    answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0xfb;
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_1C_00(byte[] command)     // PTT
        {
            try
            {
                byte[] answer = new byte[6];
                int[] parm2 = new int[1];

                if (command[6] == 0xfd)
                {
                    answer = new byte[8];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0x1c;
                    answer[5] = 0x00;

                    //if (MOX)
                        //answer[6] = 0x01;
                    //else
                        answer[6] = 0x00;

                    answer[7] = 0xfd;
                }
                else
                {
                    if (command[6] == 0x00)
                    {
                        if (!CATclosing)
                            CATCallback( "MOX", 0, parm2, "");
                    }
                    else if (command[6] == 0x01)
                        if (!CATclosing)
                            CATCallback( "MOX", 1, parm2, "");

                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0xfb;           // ok
                    answer[5] = 0xfd;
                }

                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_1C_01(byte[] command)     // Tuner on/off
        {
            try
            {
                byte[] answer = new byte[8];
                int[] parm2 = new int[1];

                if (command[6] == 0xfd)
                {
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0x1c;
                    answer[5] = 0x00;

                    //if (TUN)
                        //answer[6] = 0x01;
                    //else
                        answer[6] = 0x00;

                    answer[7] = 0xfd;
                }
                else
                {
                    if (command[6] == 0x00)
                    {
                        if (!CATclosing)
                            CATCallback( "TUN Enable", 0, parm2, "");
                    }
                    else if (command[6] == 0x01 || command[6] == 0x02)
                        if (!CATclosing)
                            CATCallback( "TUN Enable", 1, parm2, "");

                    answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];
                    answer[3] = command[2];
                    answer[4] = 0xfb;           // ok
                    answer[5] = 0xfd;
                }

                return answer;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_25(byte[] command)     // read SUB RX op mode data
        {
            try
            {
                byte[] answer = new byte[6];

                if (command[5] == 0xfd)
                {
                    switch (CurrentDSPMode)
                    {
                        case DSPMode.LSB:
                            answer[5] = 0;
                            break;

                        case DSPMode.USB:
                            answer[5] = 1;
                            break;

                        case DSPMode.AM:
                            answer[5] = 2;
                            break;

                        case DSPMode.CWU:
                            answer[5] = 3;
                            break;

                        case DSPMode.FMN:
                            answer[5] = 5;
                            break;

                        case DSPMode.CWL:
                            answer[5] = 7;
                            break;

                        case DSPMode.SAM:
                            answer[5] = 0x11;
                            break;
                    }

                    switch (CurrentFilter)
                    {
                        case Filter.F5:
                            answer[6] = 1;
                            break;

                        case Filter.F7:
                            answer[6] = 2;
                            break;

                        case Filter.F9:
                            answer[6] = 3;
                            break;

                        default:
                            answer[6] = 1;
                            break;
                    }

                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0xfb;               // OK
                    answer[5] = 0xfd;
                    return answer;
                }
                else
                {
                    answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0xfa;               // error
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        public byte[] CMD_26(byte[] command)     // write op freq to VFO B
        {
            try
            {
                if (command[5] == 0xfd)
                {
                    byte[] answer = new byte[11];
                    byte[] frequency = new byte[10];
                    string freq = Frequency.ToString();
                    freq = freq.PadLeft(10, '0');
                    ASCIIEncoding buff = new ASCIIEncoding();
                    buff.GetBytes(freq, 0, freq.Length, frequency, 0);
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0x03;
                    answer[5] = (byte)((frequency[8] - 0x30) << 4 | (frequency[9] - 0x30));
                    answer[6] = (byte)((frequency[6] - 0x30) << 4 | (frequency[7] - 0x30));
                    answer[7] = (byte)((frequency[4] - 0x30) << 4 | (frequency[5] - 0x30));
                    answer[8] = (byte)((frequency[2] - 0x30) << 4 | (frequency[3] - 0x30));
                    answer[9] = (byte)((frequency[0] - 0x30) << 4 | (frequency[1] - 0x30));
                    answer[10] = 0xfd;

                    return answer;
                }
                else
                {
                    byte[] answer = new byte[6];
                    answer[0] = 0xfe;
                    answer[1] = 0xfe;
                    answer[2] = command[3];         // swapp address
                    answer[3] = command[2];
                    answer[4] = 0xfa;               // error
                    answer[5] = 0xfd;
                    return answer;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return command;
            }
        }

        #endregion

        #region CAT callback

        public void CATCallback(string type, int parm1, int[] parm2, string parm3)
        {
            /*if (this.InvokeRequired)
            {
                this.BeginInvoke(new CATCrossThreadCallback(CATCallback), type, parm1, parm2, parm3);
                return;
            }*/

            switch (type)
            {
                case "Restore":
                   /*if (this.WindowState == FormWindowState.Minimized)
                    {
                        this.WindowState = FormWindowState.Normal;
                        this.BringToFront();
                        this.Show();
                    }
                    else
                        this.WindowState = FormWindowState.Minimized;*/
                    break;
                case "AF":
                    //AF = parm1;
                    break;
                case "AF_mute":
                    //chkMUT.Checked = !chkMUT.Checked;
                    break;
                case "AF+":
                    //AF += 1;
                    break;
                case "AF-":
                    //AF -= 1;
                    break;
                case "RF":
                    //RF = parm1;
                    break;
                case "MIC":
                    //CATMIC = parm1;
                    break;
                case "CW Monitor":
                    //CATCWMonitor = parm1.ToString();
                    break;
                case "CW Speed":
                    //CATCWSpeed = parm1;
                    break;
                case "CW Pitch":
                    //SetupForm.CATCWPitch = parm1;
                    break;
                case "BreakIn Delay":
                    //BreakInDelay = parm1;
                    break;
                case "BreakIn":
                    /*if (parm1 == 1)
                        BreakInEnabled = true;
                    else
                        BreakInEnabled = false;*/
                    break;
                case "Show CW TXfreq":
                    /*if (parm1 == 1)
                        ShowCWTXFreq = true;
                    else
                        ShowCWTXFreq = false;*/
                    break;
                case "CW Iambic":
                    /*if (parm1 == 1)
                        CWIambic = true;
                    else
                        CWIambic = false;*/
                    break;
                case "COMP":
                    /*if (parm1 == 1)
                        COMP = true;
                    else
                        COMP = false;*/
                    break;
                case "COMP Threshold":
                    //SetupForm.CATCompThreshold = parm1;
                    break;
                case "COMP level":
                    /*arm1 = (int)Math.Min(udCOMP.Maximum, parm1);
                    parm1 = (int)Math.Max(udCOMP.Minimum, parm1);
                    udCOMP.Value = parm1;*/
                    break;
                case "CMPD":
                    //CATCmpd = parm1;
                    break;
                case "CPDR":
                    //CPDRVal = parm1;
                    break;
                case "VOX":
                    /*if (parm1 == 1)
                        VOXEnable = true;
                    else
                        VOXEnable = false;*/
                    break;
                case "VOX Gain":
                    //VOXSens = parm1;
                    break;
                case "DSP Mode VFOA":
                    //CurrentDSPMode = (DSPMode)parm1;
                    break;
                case "DSP Mode VFOB":
                    //CurrentDSPModeSubRX = (DSPMode)parm1;
                    break;
                case "AGC mode":
                    //CurrentAGCMode = (AGCMode)parm1;
                    break;
                case "Filter":
                    if (current_filter != (Filter)parm1)
                        CurrentFilter = (Filter)parm1;
                    break;
                case "Filter Width":
                    //CATFilterWidth = parm1;
                    break;
                case "Filter Shift":
                    //CATFilterShift = parm1;
                    break;
                case "Filter Low":
                    //FilterLowValue = parm1;
                    //UpdateFilters(parm1, FilterHighValue);
                    break;
                case "Filter High":
                    //FilterHighValue = parm1;
                    //UpdateFilters(FilterLowValue, parm1);
                    break;
                case "Filter VFOB":
                    /*if (current_filter_subRX != (Filter)parm1)
                        CurrentFilterSubRX = (Filter)parm1;*/
                    break;
                case "TXFilter High":
                    //SetupForm.TXFilterHigh = parm1;
                    break;
                case "TXFilter Low":
                   //SetupForm.TXFilterLow = parm1;
                    break;
                case "SQL VFOA":
                    //SquelchMainRX = parm1;
                    break;
                case "SQL VFOA Enable":
                    //CATSquelch = parm1;
                    break;
                case "SQL VFOB":
                    //SquelchSubRX = parm1;
                    break;
                case "SQL VFOB Enable":
                    //CATSquelchSubRX = parm1;
                    break;
                case "CWX Remote Msg":
                    /*byte[] msg = new byte[parm2.Length];

                    for (int i = 0; i < msg.Length; i++)
                        msg[i] = (byte)parm2[i];

                    CWXForm.RemoteMessage(msg);*/
                    break;
                case "CWX Stop":
                    //CWX_Playing = false;
                    break;
                case "CWX Start":
                    /*if (CWXForm == null || CWXForm.IsDisposed)
                    {
                        try
                        {
                            CWXForm = new CWX(this);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.ToString());
                        }
                    }*/
                    break;
                case "PWR":
                    //PWR = parm1;
                    break;
                case "RIT Up":
                    break;
                case "RIT Down":
                    //RITValue -= parm1;
                    break;
                case "RIT Clear":
                    //RITValue = 0;
                    break;
                case "RIT Enable":
                    /*if (parm1 == 1)
                        RITOn = true;
                    else
                        RITOn = false;*/
                    break;
                case "RIT Value":
                    //RITValue = parm1;
                    break;
                case "XIT Up":
                    break;
                case "XIT Down":
                    break;
                case "XIT Status":
                   /*if (parm1 == 1)
                        XITOn = true;
                    else
                        XITOn = false;*/
                    break;
                case "XIT Value":
                    //XITValue = parm1;
                    break;
                case "StepSize VFOA":
                    //StepSize = parm1;
                    break;
                case "StepSize VFOA up":
                    //CATTuneStepUp = parm1.ToString();
                    break;
                case "StepSize VFOA down":
                    //CATTuneStepDown = parm1.ToString();
                    break;
                case "StepSize VFOB":
                    //StepSizeSubRX = parm1;
                    break;
                case "VFOA down":
                    //VFOAFreq = vfoAFreq - Step2Freq(parm1);
                    break;
                case "VFOA step down":
                    //VFOAFreq = vfoAFreq - wheel_tune_list[StepSize];
                    break;
                case "VFOA step up":
                    //VFOAFreq = vfoAFreq + wheel_tune_list[StepSize];
                    break;
                case "VFOA up":
                    //VFOAFreq = vfoAFreq + Step2Freq(parm1);
                    break;
                case "VFOA freq":
                    //VFOAFreq = double.Parse(parm3) / 1e6;
                    break;
                case "VFOB freq":
                    //VFOBFreq = double.Parse(parm3) / 1e6;
                    break;
                case "VFOB down":
                    //VFOBFreq = vfoBFreq - Step2Freq(parm1);
                    break;
                case "VFOB up":
                    //VFOBFreq = vfoBFreq + Step2Freq(parm1);
                    break;
                case "VFO Lock":
                    /*if (parm1 == 1)
                        chkVFOLock.Checked = true;
                    else
                        chkVFOLock.Checked = false;*/
                    break;
                case "VFO Swap":
                    //CATVFOSwap(parm1.ToString());
                    break;
                case "VFO mode":
                    //CATVFOMODE = parm1;
                    break;
                case "BandGrp":
                    //CATBandGroup = parm1;
                    break;
                case "BIN":
                   //CATBIN = parm1;
                    break;
                case "DisplayAVG":
                    //CATDisplayAvg = parm1;
                    break;
                case "Display Mode":
                    //CurrentDisplayMode = (DisplayMode)parm1;
                    //comboDisplayMode.Text = "Panafall";
                    break;
                case "Display Peak":
                   //CATDispPeak = parm1.ToString();
                    break;
                case "Display Zoom":
                    //CATDispZoom = parm1.ToString();
                    break;
                case "DX":
                    //CATPhoneDX = parm1.ToString();
                    break;
                case "RX EQU":
                    //EQForm.RXEQ = parm2;
                    break;
                case "RX EQU Enable":
                    /*if (parm1 == 1)
                        EQForm.RXEQEnabled = true;
                    else
                        EQForm.RXEQEnabled = false;*/
                    break;
                case "TX EQU":
                    //EQForm.TXEQ = parm2;
                    break;
                case "TX EQU Enable":
                    /*if (parm1 == 1)
                        EQForm.TXEQEnabled = true;
                    else
                        EQForm.TXEQEnabled = false;*/
                    break;
                case "Power":
                    /*if (parm1 == 1)
                        chkPower.Checked = true;
                    else
                        chkPower.Checked = false;*/
                    break;
                case "Power toggle":
                    //chkPower.Checked = !chkPower.Checked;
                    break;
                case "Memory Recall":
                    //CATMemoryQR();
                    break;
                case "Memory Save":
                    //CATMemoryQS();
                    break;
                case "RTTY OffsetHigh":
                    //SetupForm.RttyOffsetHigh = parm1;
                    break;
                case "RTTY OffsetLow":
                    //SetupForm.RttyOffsetLow = parm1;
                    break;
                case "SUB RX Enable":
                    /*if (parm1 == 1)
                        EnableSubRX = true;
                    else
                        EnableSubRX = false;*/
                    break;
                case "MOX":
                    /*if (parm1 == 1)
                        MOX = true;
                    else
                        MOX = false;*/
                    break;
                case "RXOnly":
                    /*if (parm1 == 1)
                        SetupForm.RXOnly = true;
                    else
                        SetupForm.RXOnly = false;*/
                    break;
                case "TUN Power":
                    //SetupForm.TunePower = parm1;
                    break;
                case "TX Profile":
                    //CATTXProfile = parm1;
                    break;
                case "TUN Enable":
                    /*if (parm1 == 1)
                        TUN = true;
                    else
                        TUN = false;*/
                    break;
                case "VAC":
                    /*if (parm1 == 1)
                        SetupForm.VACEnable = true;
                    else
                        SetupForm.VACEnable = false;*/
                    break;
                case "VAC RX gain":
                    //VACRXGain = parm1;
                    break;
                case "VAC TX gain":
                    //VACTXGain = parm1;
                    break;
                case "VAC SampleRate":
                    //VACSampleRate = parm1.ToString();
                    break;
                case "CAT Serial Destroy":
                    /*Siolisten.SIO.run = false;
                    Siolisten.SIO.rx_event.Set();
                    Siolisten.SIO.Destroy();*/
                    break;
                case "Band set":
                    //CurrentBand = (Band)parm1;
                    break;
                case "Band up":
                    //int band = Math.Min((int)Band.LAST, (int)(current_band + 1));
                    //CurrentBand = (Band)band;
                    break;
                case "Band down":
                    //band = Math.Max((int)Band.FIRST, (int)(current_band - 1));
                    //CurrentBand = (Band)band;
                    break;
                case "Meter RXMode":
                    //CurrentMeterRXMode = (MeterRXMode)parm1;
                    break;
                case "Meter TXMode":
                    //CurrentMeterTXMode = (MeterTXMode)parm1;
                    break;
                case "AF preamp":
                    /*if (parm1 == 1)
                        AF_button = true;
                    else
                        AF_button = false;*/
                    break;
                case "RF preamp":
                    //CATRFPreampStatus = parm1;
                    break;
                case "ATT":
                    /*if (parm1 == 1)
                        ATT_button = true;
                    else
                        ATT_button = false;*/
                    break;
                case "Noise Gate":
                    /*f (parm1 == 1)
                        NoiseGateEnabled = true;
                    else
                        NoiseGateEnabled = false;*/
                    break;
                case "Noise Gate Level":
                    //NoiseGate = parm1;
                    break;
                case "DSP Size":
                    //SetupForm.RXDSPBufferSize = parm1;
                    break;
                case "MUT":
                   /*if (parm1 == 1)
                        MUT = true;
                    else
                        MUT = false;
                    break;*/
                case "MON":
                    /*if (parm1 == 1)
                        MON = true;
                    else
                        MON = false;*/
                    break;
                case "PAN Swap":
                    //CATPanSwap = parm1.ToString();
                    break;
                case "SUB Rx":
                    //CATSubRX = parm1.ToString();
                    break;
                case "NB1":
                    //CATNB1 = parm1;
                    break;
                case "NB1 Threshold":
                    //SetupForm.CATNB1Threshold = parm1;
                    break;
                case "NB2":
                    //CATNB2 = parm1;
                    break;
                case "NB2 Threshold":
                    //SetupForm.CATNB2Threshold = parm1;
                    break;
                case "NR":
                    //CATNR = parm1;
                    break;
                case "NR gain":
                    //CATNRgain = parm1;
                    break;
                case "ANF":
                    //CATANF = parm1;
                    break;
                case "ANF gain":
                    //CATNOTCHgain = parm1;
                    break;
                case "NOTCH manual":
                    //CATNOTCHManual = parm1;
                    break;
                case "NOTCH manual enable":
                    //CATNOTCHenable = parm1;
                    break;
                case "RPT tone":
                    /*if (parm1 == 1)
                        CTCSS = true;
                    else
                        CTCSS = false;*/
                    break;
                case "SPLIT":
                    /*switch (parm1)
                    {
                        case 0:             // SPLIT disable
                            SplitAB_TX = false;
                            break;

                        case 1:             // SPLIT enable
                            SplitAB_TX = true;
                            break;

                        case 0x10:          // cancel DUPLEX
                            RPTRmode = RPTRmode.simplex;
                            break;

                        case 0x11:          // DUP-
                            RPTRmode = RPTRmode.low;
                            break;

                        case 0x12:          // DUP+
                            RPTRmode = RPTRmode.high;
                            break;
                    }*/
                    break;
                case "Memory up":
                    /*if (MemoryNumber < 99)
                        MemoryNumber++;
                    else if (MemoryNumber == 99)
                        MemoryNumber = 1;

                    txtMemory_fill();*/
                    break;
                case "Memory down":
                    /*if (MemoryNumber > 1)
                        MemoryNumber--;
                    else if (MemoryNumber == 1)
                        MemoryNumber = 99;*/

                    //txtMemory_fill();
                    break;
                case "Memory recall":
                    //btnMemoryQuickRestore_Click(this, EventArgs.Empty);
                    break;
                case "Memory store":
                    //btnMemoryQuickSave_Click(this, EventArgs.Empty);
                    break;
                case "Memory clear":
                    //btnEraseMemory_Click(this, EventArgs.Empty);
                    break;
                case "Restore VFOA":
                    //btnVFOA_Click(this, EventArgs.Empty);
                    break;
                case "CLOSE":
                    this.Close();
                    break;
            }
        }

        #endregion
    }
}
