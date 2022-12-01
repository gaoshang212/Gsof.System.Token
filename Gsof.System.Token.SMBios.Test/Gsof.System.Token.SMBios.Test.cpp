// Gsof.System.Token.SMBios.Test.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <windows.h>
//#include <sysinfoapi.h>

struct RawSMBIOSData
{
	BYTE    Used20CallingMethod;
	BYTE    SMBIOSMajorVersion;
	BYTE    SMBIOSMinorVersion;
	BYTE    DmiRevision;
	DWORD   Length;
	BYTE	SMBIOSTableData[];
};


struct dmi_header
{
	BYTE type;
	BYTE length;
	WORD handle;
	BYTE data[1];
};

bool biosuuid(unsigned char* uuid)
{
	bool result = false;

	RawSMBIOSData* smb = nullptr;
	BYTE* data;

	DWORD size = 0;

	// Get size of BIOS table
	size = GetSystemFirmwareTable('RSMB', 0, smb, size);
	smb = (RawSMBIOSData*)malloc(size);

	// Get BIOS table
	GetSystemFirmwareTable('RSMB', 0, smb, size);

	//Go through BIOS structures
	data = smb->SMBIOSTableData;
	while (data < smb->SMBIOSTableData + smb->Length)
	{
		BYTE* next;
		dmi_header* h = (dmi_header*)data;

		if (h->length < 4)
			break;

		//Search for System Information structure with type 0x01 (see para 7.2)
		if (h->type == 0x01 && h->length >= 0x19)
		{
			data += 0x08; //UUID is at offset 0x08

			// check if there is a valid UUID (not all 0x00 or all 0xff)
			bool all_zero = true, all_one = true;
			for (int i = 0; i < 16 && (all_zero || all_one); i++)
			{
				if (data[i] != 0x00) all_zero = false;
				if (data[i] != 0xFF) all_one = false;
			}
			if (!all_zero && !all_one)
			{
				/* As off version 2.6 of the SMBIOS specification, the first 3 fields
				of the UUID are supposed to be encoded on little-endian. (para 7.2.1) */
				*uuid++ = data[3];
				*uuid++ = data[2];
				*uuid++ = data[1];
				*uuid++ = data[0];
				*uuid++ = data[5];
				*uuid++ = data[4];
				*uuid++ = data[7];
				*uuid++ = data[6];
				for (int i = 8; i < 16; i++)
					*uuid++ = data[i];

				result = true;
			}
			break;
		}

		//skip over formatted area
		next = data + h->length;

		//skip over unformatted area of the structure (marker is 0000h)
		while (next < smb->SMBIOSTableData + smb->Length && (next[0] != 0 || next[1] != 0))
			next++;
		next += 2;

		data = next;
	}
	free(smb);
	return result;
}

int main()
{
	std::cout << "Hello World!\n";

	BYTE uuid[16];
	if (biosuuid(uuid)) {
		std::cout << "Hello World!\n";
	}

	//DWORD error = ERROR_SUCCESS;
	//DWORD smBiosDataSize = 0;
	//RawSMBIOSData* smBiosData = NULL; // Defined in this link
	//DWORD bytesWritten = 0;

	//// Query size of SMBIOS data.
	//smBiosDataSize = GetSystemFirmwareTable('RSMB', 0, NULL, 0);

	//// Allocate memory for SMBIOS data
	//smBiosData = (RawSMBIOSData*)HeapAlloc(GetProcessHeap(), 0, smBiosDataSize);
	//if (!smBiosData) {
	//	error = ERROR_OUTOFMEMORY;

	//}

	//// Retrieve the SMBIOS table
	//bytesWritten = GetSystemFirmwareTable('RSMB', 0, smBiosData, smBiosDataSize);

	//if (bytesWritten != smBiosDataSize) {
	//	error = ERROR_INVALID_DATA;
	//}
}


// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
