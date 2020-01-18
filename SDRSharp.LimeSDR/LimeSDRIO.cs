/*
 * Based on project from https://github.com/jocover/sdrsharp-limesdr
 * 
 * modifications by YT7PWR 2018
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SDRSharp.Radio;
using System.Windows.Forms;
using System.Diagnostics;

namespace SDRSharp.LimeSDR
{
    public class LimeSDRIO : IFrontendController, IIQStreamController, IDisposable, IFloatingConfigDialogProvider, ITunableSource, ISampleRateChangeSource
    {
        #region variable

        private long _frequency;
        private LimeSDRDevice _LimeDev = null;
        private readonly LimeSDRControllerDialog _gui;
        private SDRSharp.Radio.SamplesAvailableDelegate _callback;
        public event EventHandler SampleRateChanged;
        public bool _isStreaming;
        private uint _channel = 0;      // rx0
        private uint _ant = 2;          // ant_l
        private double _lpbw = 1.5 * 1e6;
        private double _gain = 40.0;
        public double _sampleRate = 2 * 1e6;
        private float _specOffset = 100.0f;
        public string RadioName = "";
        private double _freqDiff = 0.0;
        private ushort _lnaGain = 21;
        private ushort _tiaGain = 3;
        private ushort _pgaGain = 10;

        #endregion

        #region properties

        public LimeSDRDevice Device
        {
            get { return _LimeDev; }
        }

        public IntPtr LimeSDR_Device
        {
            get
            {
                if (Device != null)
                    return Device._device;
                else
                    return IntPtr.Zero;
            }
        }

        public uint Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                _channel = value;

                if(_LimeDev != null)
                {
                    _LimeDev.Channel = _channel;
                }
            }
        }

        public double LPBW
        {
            get
            {
                return _lpbw;
            }

            set
            {
                _lpbw = value;

                if (_LimeDev != null)
                {
                    _LimeDev.LPBW = _lpbw;
                }
            }
        }

        public double Gain
        {
            get
            {
                if (_LimeDev != null)
                    return _LimeDev.Gain;
                else
                    return _gain;
            }

            set
            {
                _gain = (uint)value;

                if (_LimeDev != null)
                    _LimeDev.Gain = _gain;
            }
        }

        public uint Antenna
        {
            get
            {
                return _ant;
            }
            set
            {
                _ant = value;

                if (_LimeDev != null)
                {
                    _LimeDev.Antenna = _ant;
                }
            }
        }

        public float SpecOffset
        {
            get
            {
                return _specOffset;
            }

            set
            {
                _specOffset = value;

                if (_LimeDev != null)
                {
                    _LimeDev.SpectrumOffset = _specOffset;
                }
            }
        }

        public double FreqDiff
        {
            set
            {
                _freqDiff = value;

                if (_LimeDev != null)
                {
                    _LimeDev.FreqDiff = _freqDiff;
                }
            }
        }

        public ushort LNAgain
        {
            get
            {
                if (_LimeDev != null)
                {
                    return _LimeDev.LNAgain;
                }
                else
                    return 0;
            }

            set
            {
                _lnaGain = value;

                if (_LimeDev != null)
                {
                    _LimeDev.LNAgain = _lnaGain;
                }
            }
        }

        public ushort TIAgain
        {
            get
            {
                if (_LimeDev != null)
                {
                    return _LimeDev.TIAgain;
                }
                else
                    return 0;
            }

            set
            {
                _tiaGain = value;

                if (_LimeDev != null)
                {
                    _LimeDev.TIAgain = _tiaGain;
                }
            }
        }

        public ushort PGAgain
        {
            get
            {
                if (_LimeDev != null)
                {
                    return _LimeDev.PGAgain;
                }
                else
                    return 0;
            }

            set
            {
                _pgaGain = value;

                if (_LimeDev != null)
                {
                    _LimeDev.PGAgain = _pgaGain;
                }
            }
        }

        #endregion

        #region constructor/destructor

        public LimeSDRIO()
        {
            _gui = new LimeSDRControllerDialog(this);
            _sampleRate = _gui._sampleRate;
        }

        ~LimeSDRIO()
        {
            Dispose();
        }

        public void Dispose()
        {
            Close();

            if (_gui != null)
            {
                _gui.Close();
                _gui.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        private unsafe void LimeDevice_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
        {
            _callback(this, e.Buffer, e.Length);
        }

        public void Close()
        {
            try
            {
                if (_LimeDev == null)
                    return;
            }
            catch(Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        private void LimeSDRDevice_SampleRateChanged(object sender, EventArgs e)
        {
            EventHandler eventHandler = this.SampleRateChanged;

            if (eventHandler == null)
                return;

            eventHandler((object)this, e);
        }

        public void Open()
        {
            try
            {
                _LimeDev = new LimeSDRDevice(this);
                _LimeDev.SamplesAvailable += LimeDevice_SamplesAvailable;
                _LimeDev.SampleRateChanged += new EventHandler(this.LimeSDRDevice_SampleRateChanged);
                _LimeDev.SampleRate = _sampleRate;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public void Start(Radio.SamplesAvailableDelegate callback)
        {
            try
            {
                _gui.grpChannel.Enabled = false;
                _gui.samplerateComboBox.Enabled = false;

                if (this._LimeDev == null)
                {
                    _LimeDev = new LimeSDRDevice(this);
                    _LimeDev.SamplesAvailable += LimeDevice_SamplesAvailable;
                    _LimeDev.SampleRateChanged += LimeSDRDevice_SampleRateChanged;
                    _LimeDev.SampleRate = _sampleRate;
                }

                _callback = callback;

                if (!_LimeDev.Open(RadioName))
                {
                    _LimeDev.Close();
                    _LimeDev.Open(RadioName);
                }

                _LimeDev.GFIR_BPF_Width = _gui._GFIR_BPF_Width;
                _LimeDev.LPBW = _gui.LPBW;
                _LimeDev.SampleRate = _sampleRate;
                _LimeDev.Start(_channel, _lpbw, _gain, _ant, _sampleRate, _specOffset);
                _LimeDev.LPBW = _gui.LPBW;

                _isStreaming = true;
                _gui.btnRadioInfo.Enabled = true;
                _gui.btnRadioRefresh.Enabled = false;
            }
            catch(Exception ex)
            {
                _gui.grpChannel.Enabled = true;
                _gui.samplerateComboBox.Enabled = true;
                Debug.Write(ex.ToString());
            }
        }

        public void ReStart()
        {
            try
            {
                _gui.grpChannel.Enabled = false;
                _gui.samplerateComboBox.Enabled = false;

                if (this._LimeDev == null)
                {
                    _LimeDev = new LimeSDRDevice(this);
                    _LimeDev.SamplesAvailable += LimeDevice_SamplesAvailable;
                    _LimeDev.SampleRateChanged += LimeSDRDevice_SampleRateChanged;
                    _LimeDev.SampleRate = _sampleRate;
                }

                if (!_LimeDev.Open(RadioName))
                {
                    _LimeDev.Close();
                    _LimeDev.Open(RadioName);
                }

                _LimeDev.LPBW = _gui.LPBW;
                _LimeDev.SampleRate = _sampleRate;
                _LimeDev.Start(_channel, _lpbw, _gain, _ant, _sampleRate, _specOffset);
                _LimeDev.LPBW = _gui.LPBW;

                _isStreaming = true;
                _gui.btnRadioInfo.Enabled = true;
                _gui.btnRadioRefresh.Enabled = false;
            }
            catch (Exception ex)
            {
                _gui.grpChannel.Enabled = true;
                _gui.samplerateComboBox.Enabled = true;
                Debug.Write(ex.ToString());
            }
        }

        public void Stop()
        {
            try
            {
                _gui.btnRadioInfo.Enabled = false;
                _gui.btnRadioRefresh.Enabled = true;
                _isStreaming = false;
                _gui.grpChannel.Enabled = true;
                _gui.samplerateComboBox.Enabled = true;

                if (_LimeDev != null)
                {
                    try
                    {
                        _LimeDev.Stop();
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public bool IsSoundCardBased
        {
            get
            {
                return false;
            }
        }

        public string SoundCardHint
        {
            get
            {
                return string.Empty;
            }
        }

        public void ShowSettingGUI(IWin32Window parent)
        {
            if (this._gui.IsDisposed)
                return;

            _gui.Show();
            _gui.Activate();

        }

        public void HideSettingGUI()
        {
            if (_gui.IsDisposed)
                return;

            _gui.Hide();
        }

        public long Frequency
        {
            get
            {
                if (this._LimeDev != null)
                {
                    _frequency = this._LimeDev.Frequency;
                }

                return (long)_frequency;
            }
            set
            {
                if (this._LimeDev != null)
                {
                    this._LimeDev.Frequency = (long)value;
                    this._frequency = (long)value;
                }

                _frequency = (long)value;
            }
        }

        public double Samplerate
        {
            get
            {
                if (_LimeDev != null)
                    return _LimeDev.SampleRate;
                else
                    return 0.0;
            }
        }

        public bool CanTune
        {
            get { return true; }
        }

        public long MaximumTunableFrequency
        {

            get { return 3800000000; }
        }

        public long MinimumTunableFrequency
        {
            get { return 0; }
        }
    }
}
