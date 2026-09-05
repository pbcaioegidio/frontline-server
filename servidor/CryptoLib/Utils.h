#pragma once
#include "pch.h"

class Utils {
public:
    static void SetCompleteHook(BYTE head, DWORD offset,...);
    static void MemorySetUnicode(LPVOID address, const wchar_t newValue[]);
    static void MemorySetChar(LPVOID address, const char newValue[]);
    static void SetByte(LPVOID address, BYTE newValue);
    static void ReplaceBytes(LPVOID address, const BYTE* newBytes, size_t numBytes);
    static void ReplaceBytes(LPVOID address, const BYTE* newBytes);
    static void MemoryCpy(LPVOID address, void* newValue);
    static void MemorySet(void* offset, BYTE value, size_t size);
    static void VirtualizeOffset(void* offset, size_t size);
    static std::string ReadDynamicString(HANDLE processHandle, LPCVOID address);
    static void LoadLibrary_(const wchar_t dllFileName[]);
    static LPCWSTR ConvertToLPCWSTR(const char* str);
    static std::wstring StringToWString(const std::string& str);
    static void WriteLog(const std::string& text);
    static void CreateConsole();
};

