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
    [StructLayout(LayoutKind.Sequential)]

    /**Metadata structure used in sample transfers*/
    public struct lms_stream_meta_t
    {
        /**
         * Timestamp is a value of HW counter with a tick based on sample rate.
         * In RX: time when the first sample in the returned buffer was received
         * In TX: time when the first sample in the submitted buffer should be send
         */
        public UInt64 timestamp;

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
        public uint handle;
        public bool isTx;
        public UInt32 channel;
        public UInt32 fifoSize;
        public float throughputVsLatency;
        internal dataFmt dataFmt;

    }

    public enum lms_loopback_t
    {
        LMS_LOOPBACK_NONE   /**<Return to normal operation (disable loopback)*/
    }

    public enum lms_testsig_t
    {
        LMS_TESTSIG_NONE = 0,     /**<Disable test signals. Return to normal operation*/
        LMS_TESTSIG_NCODIV8,    /**<Test signal from NCO half scale*/
        LMS_TESTSIG_NCODIV4,    /**<Test signal from NCO half scale*/
        LMS_TESTSIG_NCODIV8F,   /**<Test signal from NCO full scale*/
        LMS_TESTSIG_NCODIV4F,   /**<Test signal from NCO full scale*/
        LMS_TESTSIG_DC          /**<DC test signal*/
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
        public UInt64 boardSerialNumber;     ///<A unique board serial number
        public char[] gatewareVersion;       ///<Gateware version as a string
        public char[] gatewareTargetBoard;   ///<Which board should use this gateware

        /*public void Init()
        {
            deviceName = new char[32];
            expansionName = new char[32];
            firmwareVersion = new char[16];
            hardwareVersion = new char[16];
            protocolVersion = new char[16];
            gatewareVersion = new char[16];
            gatewareTargetBoard = new char[32];
        }*/
    }

    class NativeMethods
    {
        const string APIDLL = "LimeSuite";

        [DllImport(APIDLL, EntryPoint = "LMS_GetDeviceList", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_GetDeviceList(byte *dev_list);

        [DllImport(APIDLL, EntryPoint = "LMS_Open", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Open(out IntPtr device, string info, string args);

        [DllImport(APIDLL, EntryPoint = "LMS_Close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Close(IntPtr device);

        [DllImport(APIDLL, EntryPoint = "LMS_Disconnect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Disconnect(IntPtr device);

        [DllImport(APIDLL, EntryPoint = "LMS_IsOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LMS_IsOpen(IntPtr device, int port);

        [DllImport(APIDLL, EntryPoint = "LMS_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_Init(IntPtr device);

        [DllImport(APIDLL, EntryPoint = "LMS_EnableChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_EnableChannel(IntPtr device, bool dir_tx, uint chan, bool enabled);

        [DllImport(APIDLL, EntryPoint = "LMS_SetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetLOFrequency(IntPtr device, bool dir_tx, uint chan, double frequency);

        [DllImport(APIDLL, EntryPoint = "LMS_SetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRate(IntPtr device, double rate, uint oversample);

        [DllImport(APIDLL, EntryPoint = "LMS_SetSampleRateDir", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetSampleRateDir(IntPtr device, bool dir_tx, double rate, uint oversample);

        [DllImport(APIDLL, EntryPoint = "LMS_GetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetSampleRate(IntPtr device, bool dir_tx, uint chan, ref double host_Hz, ref double rf_Hz);

        [DllImport(APIDLL, EntryPoint = "LMS_SetupStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetupStream(IntPtr dev, IntPtr stream);

        [DllImport(APIDLL, EntryPoint = "LMS_StartStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_StartStream(IntPtr stream);

        [DllImport(APIDLL, EntryPoint = "LMS_StopStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_StopStream(IntPtr stream);

        [DllImport(APIDLL, EntryPoint = "LMS_RecvStream", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_RecvStream(IntPtr stream, void* samples, uint sample_count, ref lms_stream_meta_t meta, uint timeout_ms);

        [DllImport(APIDLL, EntryPoint = "LMS_GetLastErrorMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LMS_GetLastErrorMessage();

        public static string limesdr_strerror()
        {
            IntPtr ret = LMS_GetLastErrorMessage();
            if (ret != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(ret);
            return String.Empty;
        }

        [DllImport(APIDLL, EntryPoint = "LMS_SetAntenna", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetAntenna(IntPtr device, bool dir_tx, uint chan, uint index);

        [DllImport(APIDLL, EntryPoint = "LMS_SetTestSignal", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetTestSignal(IntPtr device, bool dir_tx, uint chan, lms_testsig_t sig, Int16 dc_i, Int16 dc_q);

        [DllImport(APIDLL, EntryPoint = "LMS_WriteParam", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_WriteParam(IntPtr device, LMS7Parameter param, UInt16 val);

        [DllImport(APIDLL, EntryPoint = "LMS_GetLOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_GetLOFrequency(IntPtr device, bool dir_tx, uint chan, ref double frequency);

        [DllImport(APIDLL, EntryPoint = "LMS_SetGaindB", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetGaindB(IntPtr device, bool dir_tx, uint chan, uint gain);

        [DllImport(APIDLL, EntryPoint = "LMS_SetLPFBW", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_SetLPFBW(IntPtr device, bool dir_tx, uint chan, double bandwidth);

        [DllImport(APIDLL, EntryPoint = "LMS_SetNCOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LMS_SetNCOIndex(IntPtr device, bool dir_tx, uint chan, int index, bool downconv);

        [DllImport(APIDLL, EntryPoint = "LMS_SetNCOFrequency", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int LMS_SetNCOFrequency(IntPtr device, bool dir_tx, uint chan, double* frequency,
    double pho);

        [DllImport(APIDLL, EntryPoint = "LMS_GetDeviceInfo", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void* LMS_GetDeviceInfo(IntPtr device);

        [DllImport(APIDLL, EntryPoint = "LMS_Calibrate", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int LMS_Calibrate(IntPtr device, bool dir_tx, uint chan, double bw, uint flags);

        [DllImport(APIDLL, EntryPoint = "LMS_GetLibraryVersion", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern char* LMS_GetLibraryVersion();
    }
}
