using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gsof.Extensions;

namespace Gsof.System.Token
{
    /// <summary>
    /// Structure needed to get the SMBIOS table using GetSystemFirmwareTable API.
    /// see https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-getsystemfirmwaretable
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RawSMBIOSData
    {
        internal byte Used20CallingMethod;
        internal byte SMBIOSMajorVersion;
        internal byte SMBIOSMinorVersion;
        internal byte DmiRevision;
        internal int Length;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2000)]
        internal byte[] SMBIOSTableData;

        public RawSMBIOSData(Native.Buffer buffer)
        {
            Used20CallingMethod = buffer.ReadByte(0);
            SMBIOSMajorVersion = buffer.ReadByte(1);
            SMBIOSMinorVersion = buffer.ReadByte(2);
            DmiRevision = buffer.ReadByte(3);
            Length = Marshal.ReadInt32(buffer, 4);
            SMBIOSTableData = buffer.ReadBytes(8);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DmiHeader
    {
        internal byte Type;
        internal byte Length;
        internal int Handle;
        //internal byte[] Data;

        public DmiHeader(byte[] buffer, int index)
        {
            Type = buffer[index + 0];
            Length = buffer[index + 1];
            Handle = BitConverter.ToInt32(buffer, index + 2);
            //Data = buffer.Take(4, buffer.Length - 3).ToArray();
        }
    };

    internal enum Provider : int
    {
        ACPI = (byte)'A' << 24 | (byte)'C' << 16 | (byte)'P' << 8 | (byte)'I',
        FIRM = (byte)'F' << 24 | (byte)'I' << 16 | (byte)'R' << 8 | (byte)'M',
        RSMB = (byte)'R' << 24 | (byte)'S' << 16 | (byte)'M' << 8 | (byte)'B'
    }

    internal class NativeMethods
    {
        private const string KERNEL = "kernel32.dll";

        [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern int EnumSystemFirmwareTables(Provider firmwareTableProviderSignature, IntPtr firmwareTableBuffer, int bufferSize);

        [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern int GetSystemFirmwareTable(Provider firmwareTableProviderSignature, int firmwareTableID, IntPtr firmwareTableBuffer, int bufferSize);
    }
}
