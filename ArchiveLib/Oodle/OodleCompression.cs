using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ArchiveLib.Oodle
{
    public static class OodleCompression
    {
        static IntPtr LibraryHandle = IntPtr.Zero;

        public enum OodleLZ_Compressor : int
        {
            LZH = 0,
            LZHLW = 1,
            LZNIB = 2,
            None = 3,
            LZB16 = 4,
            LZBLW = 5,
            LZA = 6,
            LZNA = 7,
            Kraken = 8,
            Mermaid = 9,
            BitKnit = 10,
            Selkie = 11,
            Akkorokamui = 12,
        }

        public enum OodleLZ_Compression : int
        {
            None = 0,
            SuperFast = 1,
            VertFast = 2,
            Fast = 3,
            Normal = 4,
            Optimal1 = 5,
            Optimal2 = 6,
            Optimal3 = 7,
            Optimal4 = 8,
            Optimal5 = 9,
        }

        public enum OodleLZ_FuzzSafe : int
        {
            No = 0,
            Yes = 1,
        }

        public enum OodleLZ_CheckCRC : int
        {
            No = 0,
            Yes = 1,
        }

        public enum OodleLZ_Verbosity : int
        {
            None = 0,
        }

        public enum OodleLZ_Decode : int
        {
            ThreadPhase1 = 1,
            ThreadPhase2 = 2,
            Unthreaded = 3,
        }

        // DLL loading functions
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        static extern IntPtr KernLoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
        static extern IntPtr KernGetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool KernFreeLibrary(IntPtr hModule);

        // DLL functions
        static IntPtr OodleLZ_Decompress_Ptr;
        static IntPtr OodleLZ_GetCompressedBufferSizeNeeded_Ptr;
        static IntPtr OodleLZ_Compress_Ptr;

        // DLL function delegates
        private delegate int OodleLZ_Decompress_Delegate(byte[] input, int bufferSize, byte[] output, long outputBufferSize,
            OodleLZ_FuzzSafe fuzzSafety,
            OodleLZ_CheckCRC checkCrc,
            OodleLZ_Verbosity verbosity,
            IntPtr dst_base,
            long e,
            IntPtr cb,
            IntPtr context,
            IntPtr scratch,
            long scratchSize,
            OodleLZ_Decode threadMode);

        private static OodleLZ_Decompress_Delegate OodleLZ_Decompress;

        public static void LoadLibrary(string gamePath)
        {
            string libpath = Path.Combine(gamePath, "oo2ext_7_win64.dll");
            Console.WriteLine("Loading compression library: " + libpath);

            LibraryHandle = KernLoadLibrary(@libpath);

            if (LibraryHandle == IntPtr.Zero)
                throw new Exception("Failed to load oodle library");

            OodleLZ_Decompress_Ptr = KernGetProcAddress(LibraryHandle, "OodleLZ_Decompress");
            OodleLZ_GetCompressedBufferSizeNeeded_Ptr = KernGetProcAddress(LibraryHandle, "OodleLZ_GetCompressedBufferSizeNeeded");
            OodleLZ_Compress_Ptr = KernGetProcAddress(LibraryHandle, "OodleLZ_Compress");

            if (OodleLZ_Decompress_Ptr == IntPtr.Zero || OodleLZ_GetCompressedBufferSizeNeeded_Ptr == IntPtr.Zero || OodleLZ_Compress_Ptr == IntPtr.Zero)
                throw new Exception("Compression functions not found in oodle library");

            OodleLZ_Decompress = (OodleLZ_Decompress_Delegate)Marshal.GetDelegateForFunctionPointer(OodleLZ_Decompress_Ptr, typeof(OodleLZ_Decompress_Delegate));
        }

        public static void UnloadLibrary()
        {
            if (LibraryHandle != IntPtr.Zero)
            {
                KernFreeLibrary(LibraryHandle);
                LibraryHandle = IntPtr.Zero;
            }
        }

        public static int Decompress(byte[] input, byte[] output)
        {
            return OodleLZ_Decompress(input, input.Length, output, output.Length, OodleLZ_FuzzSafe.No, OodleLZ_CheckCRC.No, OodleLZ_Verbosity.None, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, OodleLZ_Decode.Unthreaded);
        }
    }
}
