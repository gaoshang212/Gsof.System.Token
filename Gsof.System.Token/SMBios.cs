using Gsof.Extensions;
using System;
using System.IO;
using System.Linq;
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

            var index = 0;

            var uuid = new byte[16];

            while (index < len)
            {
                DmiHeader h = new DmiHeader(data, index);

                if (h.Length < 4)
                    break;
                if (h.Type == 0x01 && h.Length >= 0x19)
                {
                    //index += 0x08; //UUID is at offset 0x08

                    var buffer = data.Take(index + 8, 16).ToArray();

                    // check if there is a valid UUID (not all 0x00 or all 0xff)
                    bool all_zero = true, all_one = true;
                    for (int i = 0; i < 16 && (all_zero || all_one); i++)
                    {
                        if (buffer[i] != 0x00)
                        {
                            all_zero = false;
                        }
                        if (buffer[i] != 0xFF)
                        {
                            all_one = false;
                        }
                    }

                    if (!all_zero && !all_one)
                    {
                        /* As off version 2.6 of the SMBIOS specification, the first 3 fields
                        of the UUID are supposed to be encoded on little-endian. (para 7.2.1) */
                        uuid[0] = buffer[3];
                        uuid[1] = buffer[2];
                        uuid[2] = buffer[1];
                        uuid[3] = buffer[0];
                        uuid[4] = buffer[5];
                        uuid[5] = buffer[4];
                        uuid[6] = buffer[7];
                        uuid[7] = buffer[6];
                        for (int i = 8; i < 16; i++)
                        {
                            uuid[i] = buffer[i];
                        }
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

            return uuid.ToHex().Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-").ToUpper();
        }
    }
}
