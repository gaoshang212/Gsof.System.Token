using Gsof.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Gsof.System.Token
{
    public class SMBios
    {
        static RawSMBIOSData? GetSystemFirmwareTable(Provider provider, int table)
        {

            int size;
            try
            {
                size = NativeMethods.GetSystemFirmwareTable(provider, table, IntPtr.Zero, 0);
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }

            if (size <= 0)
            {
                return null;
            }

            using (var buffer = new Native.Buffer(size))
            {
                var bytesWritten = NativeMethods.GetSystemFirmwareTable(provider, table, buffer, size);
                if (bytesWritten != size)
                {
                    throw new InvalidDataException();
                }

                if (Marshal.GetLastWin32Error() != 0)
                    return null;

                return new RawSMBIOSData(buffer);
            }
        }

        static string[]? EnumSystemFirmwareTables(Provider provider)
        {
            int size;
            try
            {
                size = NativeMethods.EnumSystemFirmwareTables(provider, IntPtr.Zero, 0);
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }

            using (var buffer = new Native.Buffer(size))
            {
                NativeMethods.EnumSystemFirmwareTables(provider, buffer, size);

                string[] result = new string[size / 4];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = buffer.ReadString(Encoding.ASCII, 4 * i, 4);
                }

                return result;
            }
        }

        public static string UUID()
        {
            var smb = SMBios.GetSystemFirmwareTable(Provider.RSMB, 0);

            if (smb == null)
            {
                return null;
            }

            // see https://gist.github.com/neacsum/5199627fec4143c1e8d553d38dabed68

            var data = smb.Value.SMBIOSTableData;
            var len = smb.Value.Length;
            var ver = smb.Value.SMBIOSMajorVersion * 0x100 + smb.Value.SMBIOSMinorVersion;

            var index = 0;

            //var uuid = new byte[16];

            while (index < len)
            {
                DmiHeader h = new DmiHeader(data, index);

                if (h.Length < 4)
                    break;
                if (h.Type == 0x01 && h.Length >= 0x19)
                {
                    //index += 0x08; //UUID is at offset 0x08

                    var buffer = data.Take(index + 8, 16).ToArray();

                    var uuid = DmiSystemUuid(buffer, ver);
                    if (!string.IsNullOrEmpty(uuid))
                    {
                        return uuid!;
                    }
                    break;
                }

                var next = index + h.Length;
                while (next < len && next + 1 < len && (data[next]) != 0 || data[next + 1] != 0)
                {
                    next += 1;
                }

                next += 2;

                index = next;
            }

            return null; //uuid.ToHex().Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-").ToUpper();
        }

        static string? DmiSystemUuid(byte[] p, int ver)
        {
            bool only0xFF = true;
            bool only0x00 = true;

            for (int i = 0; i < 16 && (only0x00 || only0xFF); i++)
            {
                if (p[i] != 0x00) only0x00 = false;
                if (p[i] != 0xFF) only0xFF = false;
            }

            if (only0xFF)
            {
                return null;
            }

            if (only0x00)
            {
                return null;
            }

            var uuid = p.ToArray();

            if (ver >= 0x0206)
            {
                uuid[0] = p[3];
                uuid[1] = p[2];
                uuid[2] = p[1];
                uuid[3] = p[0];
                uuid[4] = p[5];
                uuid[5] = p[4];
                uuid[6] = p[7];
                uuid[7] = p[6];
                for (int i = 8; i < 16; i++)
                {
                    uuid[i] = p[i];
                }
            }

            return uuid.ToHex().Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-").ToUpper();
        }
    }
}
