/*
 * Based on project from https://github.com/jocover/sdrsharp-limesdr
 * 
 * modifications by YT7PWR 2018
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading;

namespace SDRSharp.LimeSDR
{
    //Enumeration of LMS7 TEST signal types
    public enum lms_testsig_t
    {
        LMS_TESTSIG_NONE = 0,     ///<Disable test signals. Return to normal operation
        LMS_TESTSIG_NCODIV8,    ///<Test signal from NCO half scale
        LMS_TESTSIG_NCODIV4,    ///<Test signal from NCO half scale
        LMS_TESTSIG_NCODIV8F,   ///<Test signal from NCO full scale
        LMS_TESTSIG_NCODIV4F,   ///<Test signal from NCO full scale
        LMS_TESTSIG_DC          ///<DC test signal
    }

    #region structures

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct lms_range_t
    {
        public double min;
        public double max;
        public double step;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LMS7Parameter
    {
        public UInt16 address;
        public byte msb;
        public byte lsb;
        public UInt16 defaultValue;
        public string name;
        public string tooltip;
    };

    public enum dataFmt
    {
        LMS_FMT_F32 = 0,    /**<32-bit floating point*/
        LMS_FMT_I16 = 1,      /**<16-bit integers*/
        LMS_FMT_I12 = 2       /**<12-bit integers stored in 16-bit variables*/
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct lms_stream_t
    {
#if WIN64
        public UInt64 handle;
#else
        public uint handle;
#endif
        public bool isTx;
        public uint channel;
        public uint fifoSize;
        public float throughputVsLatency;
        internal dataFmt dataFmt;
    }

    /**Streaming status structure*/
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct lms_stream_status_t
    {
        ///Indicates whether the stream is currently active
        bool active;
        ///Number of samples in FIFO buffer
        public uint fifoFilledCount;
        ///Size of FIFO buffer
        public uint fifoSize;
        ///FIFO underrun count
        public uint underrun;
        ///FIFO overrun count
        public uint overrun;
        ///Number of dropped packets by HW
        public uint droppedPackets;
        ///Sampling rate of the stream
        public double sampleRate;
        ///Combined data rate of all stream of the same direction (TX or RX)
        public double linkRate;
        ///Current HW timestamp
        public ulong timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]

    /**Metadata structure used in sample transfers*/
    public struct lms_stream_meta_t
    {
        /**
         * Timestamp is a value of HW counter with a tick based on sample rate.
         * In RX: time when the first sample in the returned buffer was received
         * In TX: time when the first sample in the submitted buffer should be send
         */
        public ulong timestamp;

        /**In TX: wait for the specified HW timestamp before broadcasting data over
         * the air
         * In RX: wait for the specified HW timestamp before starting to receive
         * samples
         */
        public bool waitForTimestamp;

        /**Indicates the end of send/receive transaction. Currently has no effect
         * @todo force send samples to HW (ignore transfer size) when selected
         */
        public bool flushPartialPacket;
    }

    [StructLayout(LayoutKind.Sequential)]
    /**Device information structure*/
    public struct lms_dev_info_t
    {
        public char[] deviceName;            ///<The display name of the device
        public char[] expansionName;         ///<The display name of the expansion card
        public char[] firmwareVersion;       ///<The firmware version as a string
        public char[] hardwareVersion;       ///<The hardware version as a string
        public char[] protocolVersion;       ///<The protocol version as a string
        public ulong boardSerialNumber;     ///<A unique board serial number
        public char[] gatewareVersion;       ///<Gateware version as a string
        public char[] gatewareTargetBoard;   ///<Which board should use this gateware
    }

    public enum lms_loopback_t
    {
        LMS_LOOPBACK_NONE   /**<Return to normal operation (disable loopback)*/
    }

    #endregion

    class NativeMethods
    {
        #region Dll import

        [DllImport("LimeSuite", EntryPoint = "LMS_GetDeviceList", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_GetDeviceList(byte* dev_list);

        [DllImport("LimeSuite", EntryPoint = "LMS_Open", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Open(out IntPtr device, string info, string args);

        [DllImport("LimeSuite", EntryPoint = "LMS_Close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Close(IntPtr device);

        [DllImport("LimeSuite", EntryPoint = "LMS_IsOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LMS_IsOpen(IntPtr device, int port);

        [DllImport("LimeSuite", EntryPoint = "LMS_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Init(IntPtr device);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNumChannels", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetNumChannels(IntPtr device, bool dir_tx);

#if WIN64
        [DllImport("LimeSuite", EntryPoint = "LMS_EnableChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_EnableChannel(IntPtr device, bool dir_tx, UInt64 chan, bool enabled);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetLOFrequency(IntPtr device, bool dir_tx, uint chan, double frequency);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetLOFrequency(IntPtr device, bool dir_tx, UInt64 chan, ref double frequency);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetNCOFrequency(IntPtr device, bool dir_tx, UInt64 chan, double* frequency,
            double pho);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_GetNCOFrequency(IntPtr device, bool dir_tx, UInt64 chan, double* frequency,
            double* pho);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNCOIndex(IntPtr device, bool dir_tx, UInt64 chan, int index, bool downconv);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetNCOIndex(IntPtr device, bool dir_tx, UInt64 chan);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOPhase", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNCOPhase(IntPtr device, bool dir_tx, UInt64 chan, double phase, double fcw);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOPhase", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetNCOPhase(IntPtr device, bool dir_tx, UInt64 chan, ref double phase, ref double fcw);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetSampleRateDir", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRateDir(IntPtr device, bool dir_tx, double rate, UInt64 oversample);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRate(IntPtr device, double rate, UInt64 oversample);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetSampleRate(IntPtr device, bool dir_tx, UInt64 chan, ref double host_Hz,
            ref double rf_Hz);

        [DllImport("LimeSuite", EntryPoint = "LMS_RecvStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_RecvStream(IntPtr stream, void* samples, UInt64 sample_count,
            ref lms_stream_meta_t meta, UInt64 timeout_ms);

        [DllImport("LimeSuite", EntryPoint = "LMS_SendStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SendStream(IntPtr stream, void* samples, UInt64 sample_count,
            ref lms_stream_meta_t meta, UInt64 timeout_ms);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetAntenna", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetAntenna(IntPtr device, bool dir_tx, UInt64 chan, UInt64 index);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetTestSignal", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetTestSignal(IntPtr device, bool dir_tx, UInt64 chan, lms_testsig_t sig,
            Int16 dc_i, Int16 dc_q);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIOWrite", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIOWrite(IntPtr device, char* buffer, UInt64 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIODirWrite", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIODirWrite(IntPtr device, char* buffer, UInt64 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIORead", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIORead(IntPtr device, char* buffer, UInt64 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIODirRead", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIODirRead(IntPtr device, char* buffer, UInt64 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGaindB", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetGaindB(IntPtr device, bool dir_tx, UInt64 chan, UInt64 gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetGaindB", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetGaindB(IntPtr device, bool dir_tx, UInt64 chan, UInt64* gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNormalizedGain", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNormalizedGain(IntPtr device, bool dir_tx, UInt64 chan, double gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNormalizedGain", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetNormalizedGain(IntPtr device, bool dir_tx, UInt64 chan, double* gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetChipTemperature", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetChipTemperature(IntPtr dev, UInt64 ind, double* temp);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLPFBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetLPFBW(IntPtr device, bool dir_tx, UInt64 chan, double bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLPFBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetLPFBW(IntPtr device, bool dir_tx, UInt64 chan, double* bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLPFBWRange", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetLPFBWRange(IntPtr device, bool dir_tx, UInt64 chan, lms_range_t* range);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLPF", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetLPF(IntPtr device, bool dir_tx, UInt64 chan, bool enable);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGFIRLPF", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetGFIRLPF(IntPtr device, bool dir_tx, UInt64 chan, bool enable, double bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGFIRCoeff", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetGFIRCoeff(IntPtr device, bool dir_tx, UInt64 chan, IntPtr filt,
            double* coef, UInt64 count);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetGFIRCoeff", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetGFIRCoeff(IntPtr device, bool dir_tx, UInt64 chan, IntPtr filt,
            double* coef);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetAntennaBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetAntennaBW(IntPtr device, bool dir_tx, UInt64 chan, UInt64 path,
            lms_range_t* range);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetClockFreq", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetClockFreq(IntPtr device, UInt64 clk_id, double* freq);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetClockFreq", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetClockFreq(IntPtr device, UInt64 clk_id, double freq);

        [DllImport("LimeSuite", EntryPoint = "LMS_Calibrate", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_Calibrate(IntPtr device, bool dir_tx, UInt64 chan, double bw, UInt64 flags);
#else

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetLOFrequency(IntPtr device, bool dir_tx, UInt32 chan, double frequency);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetLOFrequency(IntPtr device, bool dir_tx, UInt32 chan, ref double frequency);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetNCOFrequency(IntPtr device, bool dir_tx, UInt32 chan, double* frequency,
            double pho);

        [DllImport("LimeSuite", EntryPoint = "LMS_EnableChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_EnableChannel(IntPtr device, bool dir_tx, UInt32 chan, bool enabled);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_GetNCOFrequency(IntPtr device, bool dir_tx, UInt32 chan, double* frequency,
            double* pho);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNCOIndex(IntPtr device, bool dir_tx, UInt32 chan, int index, bool downconv);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetNCOIndex(IntPtr device, bool dir_tx, UInt32 chan);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNCOPhase", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNCOPhase(IntPtr device, bool dir_tx, UInt32 chan, double phase, double fcw);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNCOPhase", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetNCOPhase(IntPtr device, bool dir_tx, UInt32 chan, ref double phase, ref double fcw);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetSampleRateDir", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRateDir(IntPtr device, bool dir_tx, double rate, UInt32 oversample);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRate(IntPtr device, double rate, UInt32 oversample);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetSampleRate(IntPtr device, bool dir_tx, UInt32 chan, ref double host_Hz,
        ref double rf_Hz);

        [DllImport("LimeSuite", EntryPoint = "LMS_RecvStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_RecvStream(IntPtr stream, void* samples, UInt32 sample_count,
            ref lms_stream_meta_t meta, UInt32 timeout_ms);

        [DllImport("LimeSuite", EntryPoint = "LMS_SendStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SendStream(IntPtr stream, void* samples, uint sample_count,
            ref lms_stream_meta_t meta, uint timeout_ms);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetAntenna", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetAntenna(IntPtr device, bool dir_tx, UInt32 chan, UInt32 index);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetTestSignal", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetTestSignal(IntPtr device, bool dir_tx, UInt32 chan, lms_testsig_t sig,
            Int16 dc_i, Int16 dc_q);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIOWrite", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIOWrite(IntPtr device, char* buffer, UInt32 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIODirWrite", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIODirWrite(IntPtr device, char* buffer, UInt32 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIORead", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIORead(IntPtr device, char* buffer, UInt32 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_GPIODirRead", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GPIODirRead(IntPtr device, char* buffer, UInt32 length);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGaindB", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetGaindB(IntPtr device, bool dir_tx, UInt32 chan, UInt32 gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetGaindB", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetGaindB(IntPtr device, bool dir_tx, UInt32 chan, UInt32* gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetNormalizedGain", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNormalizedGain(IntPtr device, bool dir_tx, UInt32 chan, double gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetNormalizedGain", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetNormalizedGain(IntPtr device, bool dir_tx, UInt32 chan, double* gain);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetChipTemperature", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetChipTemperature(IntPtr dev, UInt32 ind, double* temp);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLPFBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetLPFBW(IntPtr device, bool dir_tx, UInt32 chan, double bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLPFBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetLPFBW(IntPtr device, bool dir_tx, UInt32 chan, double* bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLPFBWRange", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetLPFBWRange(IntPtr device, bool dir_tx, UInt32 chan, lms_range_t* range);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetLPF", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetLPF(IntPtr device, bool dir_tx, UInt32 chan, bool enable);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGFIRLPF", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetGFIRLPF(IntPtr device, bool dir_tx, UInt32 chan, bool enable, double bandwidth);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetGFIRCoeff", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetGFIRCoeff(IntPtr device, bool dir_tx, UInt32 chan, IntPtr filt,
            double* coef, UInt32 count);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetGFIRCoeff", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetGFIRCoeff(IntPtr device, bool dir_tx, UInt32 chan, IntPtr filt,
            double* coef);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetAntennaBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetAntennaBW(IntPtr device, bool dir_tx, UInt32 chan, UInt32 path,
            lms_range_t* range);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetClockFreq", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetClockFreq(IntPtr device, UInt32 clk_id, double* freq);

        [DllImport("LimeSuite", EntryPoint = "LMS_SetClockFreq", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetClockFreq(IntPtr device, UInt32 clk_id, double freq);

        [DllImport("LimeSuite", EntryPoint = "LMS_Calibrate", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_Calibrate(IntPtr device, bool dir_tx, UInt32 chan, double bw, UInt32 flags);
#endif

        [DllImport("LimeSuite", EntryPoint = "LMS_SetupStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetupStream(IntPtr dev, IntPtr stream);

        [DllImport("LimeSuite", EntryPoint = "LMS_StartStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_StartStream(IntPtr stream);

        [DllImport("LimeSuite", EntryPoint = "LMS_StopStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_StopStream(IntPtr stream);

        [DllImport("LimeSuite", EntryPoint = "LMS_DestroyStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_DestroyStream(IntPtr dev, IntPtr stream);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLastErrorMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LMS_GetLastErrorMessage();

        public static string limesdr_strerror()
        {
            IntPtr ret = LMS_GetLastErrorMessage();
            if (ret != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(ret);
            return String.Empty;
        }

        [DllImport("LimeSuite", EntryPoint = "LMS_WriteParam", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_WriteParam(IntPtr device, LMS7Parameter param, UInt16 val);

        [DllImport("LimeSuite", EntryPoint = "LMS_ReadParam", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_ReadParam(IntPtr device, LMS7Parameter param, ushort* val);

        [DllImport("LimeSuite", EntryPoint = "LMS_Reset", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_Reset(IntPtr device);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetStreamStatus", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_GetStreamStatus(IntPtr stream, ref lms_stream_status_t status);

        [DllImport("LimeSuite", EntryPoint = "LMS_GetLibraryVersion", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern char* LMS_GetLibraryVersion();

        [DllImport("LimeSuite", EntryPoint = "LMS_GetDeviceInfo", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void* LMS_GetDeviceInfo(IntPtr device);

        [DllImport("LimeSuite", EntryPoint = "LMS_SaveConfig", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SaveConfig(IntPtr device, string fileName);

        [DllImport("LimeSuite", EntryPoint = "LMS_LoadConfig", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_LoadConfig(IntPtr device, string fileName);

        [DllImport("LimeSuite", EntryPoint = "LMS_VCTCXOWrite", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_VCTCXOWrite(IntPtr device, ushort val);

        [DllImport("LimeSuite", EntryPoint = "LMS_VCTCXORead", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_VCTCXORead(IntPtr device, ushort* val);

        [DllImport("LimeSuite", EntryPoint = "LMS_WriteFPGAReg", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_WriteFPGAReg(IntPtr device, UInt32 address, UInt16 val);

        [DllImport("LimeSuite", EntryPoint = "LMS_ReadFPGAReg", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_ReadFPGAReg(IntPtr device, UInt32 address, UInt16* val);

        #endregion
    }
}
