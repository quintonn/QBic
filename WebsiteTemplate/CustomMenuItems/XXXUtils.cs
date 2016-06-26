using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WebsiteTemplate.CustomMenuItems
{
    /// <summary>
    /// Replace the name XXX with a name of my framework????
    /// TOOD: ???
    /// </summary>
    public class XXXUtils
    {
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
                                            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
                                            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)]
                                            byte[] pBuffer,
                                            int cbSize,
                                            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
                                            int dwMimeFlags,
                                            out IntPtr ppwzMimeOut,
                                            int dwReserved
            );


        public static string GetMimeFromBytes(byte[] dataBytes)
        {
            if (dataBytes == null)
            {
                throw new ArgumentNullException("dataBytes");
            }
            var mimeRet = String.Empty;
            var suggestPtr = IntPtr.Zero;
            var filePtr = IntPtr.Zero;
            var outPtr = IntPtr.Zero;
            var mimeProposed = "text/plain";
            var ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length, mimeProposed, 0, out outPtr, 0);
            if (ret == 0 && outPtr != IntPtr.Zero)
            {
                //todo: this leaks memory outPtr must be freed
                return Marshal.PtrToStringUni(outPtr);
            }
            return mimeRet;
        }
    }
}