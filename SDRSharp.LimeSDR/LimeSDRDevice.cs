/*
 * Based on project from https://github.com/jocover/sdrsharp-limesdr
 * 
 * modifications by YT7PWR 2018
 */

using System;
using SDRSharp.Radio;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace SDRSharp.LimeSDR
{
    public class LimeSDRDevice
    {
        #region variable 

        LimeSDRIO _parrent;
        private const double DefaultSamplerate = 768000;
        private const long DefaultFrequency = 101700000;
        private const uint SampleTimeoutMs = 1000;

        public IntPtr _device = IntPtr.Zero;
        IntPtr _stream = IntPtr.Zero;
        private bool _isStreaming;
        private Thread _sampleThread = null;
        private uint _channel = 0;  // rx0
        private uint _ant = 2;      // ant_l

        private static uint _readLength = 5000;
        private UnsafeBuffer _iqBuffer;
        private unsafe Complex* _iqPtr;
        private UnsafeBuffer _samplesBuffer;
        private unsafe float* _samplesPtr;
        private readonly SamplesAvailableEventArgs _eventArgs = new SamplesAvailableEventArgs();

        private double _sampleRate = DefaultSamplerate;
        private double _centerFrequency = DefaultFrequency;
        private double _gain = 40;

        public const bool LMS_CH_TX = true;
        public const bool LMS_CH_RX = false;

        [method: CompilerGenerated]
        [CompilerGenerated]
        public event SamplesAvailableDelegate SamplesAvailable;

        private double _lpbw = 1.5 * 1e6;
        public float SpectrumOffset = 100.0f;
        private double _freqDiff = 0.0;
        private ushort _lnaGain = 21;
        private ushort _tiaGain = 3;
        private ushort _pgaGain = 10;
        LMS7Parameter PGA_gain = new LMS7Parameter();
        LMS7Parameter TIA_gain = new LMS7Parameter();
        LMS7Parameter LNA_gain = new LMS7Parameter();

        public double GFIR_BPF_Width = 0.0;

        double phase_accumulator1;
        public double input_noise_source_scale = 0.001;
        public double input_signal_source_scale = 0.005;
        private Random r = new Random();
        private double y2 = 0.0;
        private bool use_last = false;
        public double sine_freq1 = 0.05;
        public bool test_signal = false;

        #endregion

        #region Properties

        public bool IsStreaming
        {
            get
            {
                return _isStreaming;
            }
        }

        public double FreqDiff
        {
           set
            {
                _freqDiff = value;

                if (_isStreaming)
                    Frequency = (long)_centerFrequency;   // force reload
            }
        }

        unsafe public ushort LNAgain
        {
            get
            {
                ushort[] buffer = new ushort[10];

                if (_device != IntPtr.Zero)
                {
                    fixed (ushort* value = &buffer[0])
                    {
                        if (NativeMethods.LMS_ReadParam(_device, LNA_gain, &value[0]) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }
                    }
                }

                _lnaGain = buffer[0];

                return _lnaGain;
            }

            set
            {
                _lnaGain = value;

                if (_isStreaming)
                {
                    if (NativeMethods.LMS_WriteParam(_device, LNA_gain, _lnaGain) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
            }
        }

        unsafe public ushort TIAgain
        {
            get
            {
                ushort[] buffer = new ushort[10];

                if (_device != IntPtr.Zero)
                {
                    fixed (ushort* value = &buffer[0])
                    {
                        if (NativeMethods.LMS_ReadParam(_device, TIA_gain, &value[0]) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }
                    }
                }

                _tiaGain = buffer[0];

                return _tiaGain;
            }

            set
            {
                _tiaGain = value;

                if (_isStreaming)
                {
                    if (NativeMethods.LMS_WriteParam(_device, TIA_gain, _tiaGain) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
            }
        }

        unsafe public ushort PGAgain
        {
            get
            {
                ushort[] buffer = new ushort[10];

                if (_device != IntPtr.Zero)
                {
                    fixed (ushort* value = &buffer[0])
                    {
                        if (NativeMethods.LMS_ReadParam(_device, PGA_gain, &value[0]) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }
                    }
                }

                _pgaGain = buffer[0];

                return _pgaGain;
            }

            set
            {
                _pgaGain = value;

                if (_isStreaming)
                {
                    if (NativeMethods.LMS_WriteParam(_device, PGA_gain, _pgaGain) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
            }
        }

        #endregion

        #region callback

        private unsafe void ReceiveSamples_sync()
        {
            lms_stream_meta_t meta = new lms_stream_meta_t();
            meta.timestamp = 0;
            meta.flushPartialPacket = false;
            meta.waitForTimestamp = false;

            while (_isStreaming)
            {

                if (_iqBuffer == null || _iqBuffer.Length != _readLength)
                {
                    _iqBuffer = UnsafeBuffer.Create((int)_readLength, sizeof(Complex));
                    _iqPtr = (Complex*)_iqBuffer;
                    Thread.Sleep(100);
                }

                if (_samplesBuffer == null || _samplesBuffer.Length != (2 * _readLength))
                {
                    _samplesBuffer = UnsafeBuffer.Create((int)(2 * _readLength), sizeof(float));
                    _samplesPtr = (float*)_samplesBuffer;
                    Thread.Sleep(100);
                }

                var ptrIq = _iqPtr;

                if (test_signal)
                {
                    SineWaveWithNoise(ptrIq, _readLength, phase_accumulator1, sine_freq1);
                    phase_accumulator1 = CosineWaveWithNoise(ptrIq, _readLength, phase_accumulator1, sine_freq1);
                }
                else
                {
                    NativeMethods.LMS_RecvStream(_stream, _samplesPtr, _readLength, ref meta, SampleTimeoutMs);

                    for (int i = 0; i < _readLength; i++)
                    {
                        ptrIq->Real = _samplesPtr[i * 2] / SpectrumOffset;
                        ptrIq->Imag = _samplesPtr[i * 2 + 1] / SpectrumOffset;
                        ptrIq++;
                    }
                }

                ComplexSamplesAvailable(_iqPtr, _iqBuffer.Length);
            }

            NativeMethods.LMS_StopStream(_stream);
            Close();
        }

        private double boxmuller(double m, double s)
        {
            double x1, x2, w, y1;
            if (use_last)		        /* use value from previous call */
            {
                y1 = y2;
                use_last = false;
            }
            else
            {
                do
                {
                    x1 = (2.0 * r.NextDouble() - 1.0);
                    x2 = (2.0 * r.NextDouble() - 1.0);
                    w = x1 * x1 + x2 * x2;
                } while (w >= 1.0);

                w = Math.Sqrt((-2.0 * Math.Log(w)) / w);
                y1 = x1 * w;
                y2 = x2 * w;
                use_last = true;
            }

            return (m + y1 * s);
        }

        // returns updated phase accumulator
        unsafe public double SineWaveWithNoise(Complex* buf, uint samples, double phase, double freq)
        {
            double phase_step = freq / _sampleRate * 2 * Math.PI;
            double cosval = Math.Cos(phase);
            double sinval = Math.Sin(phase);
            double cosdelta = Math.Cos(phase_step);
            double sindelta = Math.Sin(phase_step);
            double tmpval;

            for (int i = 0; i < samples; i++)       // noise part
            {
                buf[i].Real = (float)(boxmuller(0.0, 0.2) * input_noise_source_scale) / SpectrumOffset;
            }

            for (int i = 0; i < samples; i++)       // signal part
            {
                tmpval = cosval * cosdelta - sinval * sindelta;
                sinval = cosval * sindelta + sinval * cosdelta;
                cosval = tmpval;

                buf[i].Real += (float)(sinval * input_signal_source_scale) / SpectrumOffset;

                phase += phase_step;

                if (phase > Math.PI)
                    phase -= 2 * Math.PI;
            }

            return phase;
        }

        // returns updated phase accumulator
        unsafe public double CosineWaveWithNoise(Complex* buf, uint samples, double phase, double freq)
        {
            double phase_step = freq / _sampleRate * 2 * Math.PI;
            double cosval = Math.Cos(phase);
            double sinval = Math.Sin(phase);
            double cosdelta = Math.Cos(phase_step);
            double sindelta = Math.Sin(phase_step);
            double tmpval;

            for (int i = 0; i < samples; i++)       // noise part
            {
                buf[i].Imag = (float)(boxmuller(0.0, 0.2) * input_noise_source_scale) / SpectrumOffset;
            }

            for (int i = 0; i < samples; i++)
            {
                tmpval = cosval * cosdelta - sinval * sindelta;
                sinval = cosval * sindelta + sinval * cosdelta;
                cosval = tmpval;

                buf[i].Imag += (float)(cosval * input_signal_source_scale) / SpectrumOffset;

                phase += phase_step;

                if (phase > Math.PI)
                    phase -= 2 * Math.PI;
            }

            return phase;
        }

        #endregion

        public LimeSDRDevice(LimeSDRIO parrent)
        {
            _parrent = parrent;

            PGA_gain.address = 0x0119;
            PGA_gain.msb = 4;
            PGA_gain.lsb = 0;
            PGA_gain.defaultValue = 11;
            PGA_gain.name = "G_PGA_RBB";
            PGA_gain.tooltip = "This is the gain of the PGA";

            TIA_gain.address = 0x0113;
            TIA_gain.msb = 1;
            TIA_gain.lsb = 0;
            TIA_gain.defaultValue = 3;
            TIA_gain.name = "G_TIA_RFE";
            TIA_gain.tooltip = "Controls the Gain of the TIA";

            LNA_gain.address = 0x0113;
            LNA_gain.msb = 9;
            LNA_gain.lsb = 6;
            LNA_gain.defaultValue = 15;
            LNA_gain.name = "G_LNA_RFE";
            LNA_gain.tooltip = "Controls the gain of the LNA";
        }

        ~LimeSDRDevice()
        {
            Dispose();
        }

        private unsafe void ComplexSamplesAvailable(Complex* buffer, int length)
        {
            if (SamplesAvailable != null)
            {
                _eventArgs.Buffer = buffer;
                _eventArgs.Length = length;
                SamplesAvailable(this, _eventArgs);
            }
        }

        public void Stop()
        {
            if (!_isStreaming)
                return;

            _isStreaming = false;
        }

        public void Dispose()
        {
            this.Stop();
            GC.SuppressFinalize(this);
            _device = IntPtr.Zero;
        }

        public unsafe bool Open(string name)
        {
            if (NativeMethods.LMS_GetDeviceList(null) < 1)
            {
                throw new Exception("Cannot found LimeSDR device. Is the device locked somewhere?");
            }

            if (NativeMethods.LMS_Open(out _device, name, null) != 0)
            {
                throw new ApplicationException("Cannot open LimeSDR device. Is the device locked somewhere?");
            }

            return true;
        }

        public unsafe void Close()
        {
            if (NativeMethods.LMS_Close(_device) != 0)
            {
                throw new ApplicationException("Cannot open LimeSDR device. Is the device locked somewhere?");
            }

            _device = IntPtr.Zero;
        }

        public unsafe void Start(uint ch, double lpbw, double gain, uint ant, double sr, float specOffset)
        {
            _ant = ant;
            _channel = ch;
            _lpbw = lpbw;
            _gain = (uint)gain;
            _sampleRate = sr;
            SpectrumOffset = specOffset;

            NativeMethods.LMS_Init(_device);

            if (NativeMethods.LMS_EnableChannel(_device, LMS_CH_RX, _channel, true) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            if (NativeMethods.LMS_SetAntenna(_device, LMS_CH_RX, _channel, _ant) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            this.SampleRate = _sampleRate;

            if (SampleRate < 384000)
            {
                if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 32) != 0)
                {
                    MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                    //throw new ApplicationException(NativeMethods.limesdr_strerror());
                }
            }
            else
            {
                if (SampleRate > 32000000)
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 0) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
                else if (SampleRate > 16000000)
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 4) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
                else if (SampleRate > 8000000)
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 8) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
                else if (SampleRate > 4000000)
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, (double)(SampleRate), 16) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
                else if (SampleRate > 2000000)
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 32) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
                else
                {
                    if (NativeMethods.LMS_SetSampleRateDir(_device, LMS_CH_RX, SampleRate, 32) != 0)
                    {
                        MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                        //throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
            }

            if (NativeMethods.LMS_SetNormalizedGain(_device, LMS_CH_RX, _channel, _gain / 73.0) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            LPBW = _lpbw;

            if (NativeMethods.LMS_SetGFIRLPF(_device, LMS_CH_RX, _channel, true, GFIR_BPF_Width) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            lms_stream_t streamId = new lms_stream_t();
            streamId.handle = 0;
            streamId.channel = _channel;                //channel number
            streamId.fifoSize = 16 * 1024;              //fifo size in samples
            streamId.throughputVsLatency = 0.5f;        //balance
            streamId.isTx = false;                      //RX channel
            streamId.dataFmt = dataFmt.LMS_FMT_F32;
            _stream = Marshal.AllocHGlobal(Marshal.SizeOf(streamId));

            Marshal.StructureToPtr(streamId, _stream, false);

            if (NativeMethods.LMS_SetupStream(_device, _stream) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            if (NativeMethods.LMS_StartStream(_stream) != 0)
            {
                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                //throw new ApplicationException(NativeMethods.limesdr_strerror());
            }

            _isStreaming = true;
            this.Frequency = (long)_centerFrequency;

            _sampleThread = new Thread(ReceiveSamples_sync);
            _sampleThread.Name = "limesdr_samples_rx";
            _sampleThread.Priority = ThreadPriority.Highest;
            _sampleThread.Start();
        }

        public unsafe long Frequency
        {

            get
            {
                if (_device != IntPtr.Zero)
                {
                    if (NativeMethods.LMS_GetLOFrequency(_device, LMS_CH_RX, _channel, ref _centerFrequency) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }

                return (long)_centerFrequency;
            }
            set
            {
                _centerFrequency = value;

                try
                {
                    if (_isStreaming)
                    {
                        if (value >= 30 * 1e6)
                        {
                            if (NativeMethods.LMS_SetNCOIndex(_device, LMS_CH_RX, _channel, 15, true) != 0)   // 0.0 NCO
                            {
                                _isStreaming = false;
                                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                                //throw new ApplicationException(NativeMethods.limesdr_strerror());
                            }

                            if (NativeMethods.LMS_SetLOFrequency(_device, LMS_CH_RX, _channel, _centerFrequency + _freqDiff) != 0)
                            {
                                _isStreaming = false;
                                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                                //throw new ApplicationException(NativeMethods.limesdr_strerror());
                            }
                        }
                        else
                        {
                            if (NativeMethods.LMS_SetLOFrequency(_device, LMS_CH_RX, _channel, 30.0 * 1e6) != 0)
                            {
                                _isStreaming = false;
                                MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                                //throw new ApplicationException(NativeMethods.limesdr_strerror());
                            }

                            double[] losc_freq = new double[16];
                            double[] pho = new double[1];

                            fixed (double* freq = &losc_freq[0])
                            fixed (double* pho_ptr = &pho[0])
                            {
                                losc_freq[0] = (30.0 * 1e6) - _centerFrequency;
                                losc_freq[15] = 0.0;

                                if (NativeMethods.LMS_SetNCOFrequency(_device, LMS_CH_RX, _channel, freq, 0.0) != 0)
                                {
                                    _isStreaming = false;
                                    MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                                    //throw new ApplicationException(NativeMethods.limesdr_strerror());
                                }

                                if (NativeMethods.LMS_SetNCOIndex(_device, LMS_CH_RX, _channel, 0, false) != 0)
                                {
                                    _isStreaming = false;
                                    MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                                    //throw new ApplicationException(NativeMethods.limesdr_strerror());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(NativeMethods.limesdr_strerror(), "");
                    //throw new ApplicationException(NativeMethods.limesdr_strerror());
                }

            }

        }

        public event EventHandler SampleRateChanged;

        public void OnSampleRateChanged()
        {
            if (SampleRateChanged != null)
                SampleRateChanged(this, EventArgs.Empty);
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

                if (_isStreaming)
                {
                    if (_device != IntPtr.Zero || _device != null)
                    {
                        if (NativeMethods.LMS_SetAntenna(_device, LMS_CH_RX, _channel, _ant) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }
                    }
                }
            }
        }

        public double SampleRate
        {
            get
            {
                return _sampleRate;
            }

            set
            {
                _sampleRate = value;
                OnSampleRateChanged();
            }
        }

        unsafe public double Gain
        {
            get
            {
                double[] buffer = new double[1];

                if (_device != IntPtr.Zero)
                {
                    fixed (double* value = &buffer[0])
                    {
                        if (NativeMethods.LMS_GetNormalizedGain(_device, LMS_CH_RX, _channel, &value[0]) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }
                    }
                }

                _gain = buffer[0];

                return _gain;
            }

            set
            {
                _gain = (uint)value;

                if (_isStreaming)
                {
                    if (_device != IntPtr.Zero)
                    {
                        if (NativeMethods.LMS_SetNormalizedGain(_device, LMS_CH_RX, _channel, value / 73.0) != 0)
                        {
                            throw new ApplicationException(NativeMethods.limesdr_strerror());
                        }

                    }
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

                if (_isStreaming)
                {
                    if (NativeMethods.LMS_SetLPFBW(_device, LMS_CH_RX, _channel, _lpbw) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }
                }
            }
        }

        public void Set_GFIR_BPF_Width(double width)
        {
            try
            {
                GFIR_BPF_Width = width;

                if (NativeMethods.LMS_SetGFIRLPF(_device, LMS_CH_RX, _channel, true, width) != 0)
                {
                    throw new ApplicationException(NativeMethods.limesdr_strerror());
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public unsafe double ReadTemperature()
        {
            try
            {
                double[] buffer = new double[10];

                fixed (double* buf = &buffer[0])
                    if (NativeMethods.LMS_GetChipTemperature(_device, 0, buf) != 0)
                    {
                        throw new ApplicationException(NativeMethods.limesdr_strerror());
                    }

                return buffer[0];
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());

                return 0.0;
            }
        }

        public bool RXcalibration(double RX_LPFBW)
        {
            try
            {
                    int result = NativeMethods.LMS_Calibrate(_device, LMS_CH_RX, _channel, Math.Max(RX_LPFBW, 5e6), 0);

                    if (result == 0)
                        return true;
                    else
                    {
                        return false;
                    }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());

                return false;
            }
        }
    }

    public sealed class SamplesAvailableEventArgs : EventArgs
    {
        public int Length { get; set; }
        public unsafe Complex* Buffer { get; set; }
    }

    public delegate void SamplesAvailableDelegate(object sender, SamplesAvailableEventArgs e);

}
