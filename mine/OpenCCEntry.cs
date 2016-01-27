using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenCCEntry
{
    public class OpenCC
    {
        [DllImport("opencc/opencc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opencc_open
            ([MarshalAs(UnmanagedType.LPStr)] string configFileName);

        [DllImport("opencc/opencc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opencc_close(IntPtr opencc);

        [DllImport("opencc/opencc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int opencc_convert_utf8_to_buffer(IntPtr opencc, byte* input, int length, byte* output);

        [DllImport("opencc/opencc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern string opencc_error();

        private IntPtr openccInstance = new IntPtr(0);
        private int protectLength = 0;

        /// <summary>
        /// Create a new instance of OpenCC converter
        /// </summary>
        /// <param name="configFileName">Location of configuration file</param>
        /// <param name="buffProtect">Prevent memory leak in unsafe code</param>
        public OpenCC(string configFileName = "opencc/s2t.json", int buffProtect = 2000)
        {
            openccInstance = opencc_open(configFileName);
            protectLength = buffProtect;
            if (openccInstance == new IntPtr(-1))
                throw new Exception("Create OpenCC Engine Failed");
        }

        ~OpenCC()
        {
            opencc_close(openccInstance);
        }

        /// <summary>
        /// Convert string content using OpenCC converter
        /// </summary>
        /// <param name="input">Source string</param>
        /// <returns>Convert Result</returns>
        unsafe public string Convert(string input)
        {
            var src = Encoding.UTF8.GetBytes(input);
            var dst = new byte[src.Length + protectLength];

            int resultLength = 0;
            fixed (byte* ptr = src, optr = dst)
            { resultLength = opencc_convert_utf8_to_buffer(openccInstance, ptr, src.Length, optr); }
            if (resultLength == -1)
                throw new Exception("Conversion Failed");

            return Encoding.UTF8.GetString(dst, 0, resultLength);
        }
    }
}