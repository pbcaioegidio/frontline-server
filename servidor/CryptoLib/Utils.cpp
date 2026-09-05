#include "pch.h"
#include "Utils.h"


void Utils::SetCompleteHook(BYTE head, DWORD offset, ...)
{
    DWORD OldProtect;
    VirtualProtect((void*)offset, 5, PAGE_EXECUTE_READWRITE, &OldProtect);
    if (head != 0xFF)
    {
        *(BYTE*)(offset) = head;
    }
    DWORD* function = &offset + 1;
    *(DWORD*)(offset + 1) = (*function) - (offset + 5);
    VirtualProtect((void*)offset, 5, OldProtect, &OldProtect);
}

void Utils::MemorySetUnicode(LPVOID address, const wchar_t newValue[])
{
    DWORD oldProtect, newProtect = PAGE_EXECUTE_READWRITE;

    // Cambiar los permisos de la memoria a lectura/escritura/ejecución.
    if (VirtualProtect(address, (wcslen(newValue) + 1) * sizeof(wchar_t), newProtect, &oldProtect))
    {
        // Copiar la cadena Unicode (wchar_t) en la dirección de memoria especificada.
        wcscpy_s(static_cast<wchar_t*>(address), wcslen(newValue) + 1, newValue);

        // Restaurar los permisos originales.
        VirtualProtect(address, (wcslen(newValue) + 1) * sizeof(wchar_t), oldProtect, &oldProtect);
    }
    else
    {
        // Mostrar mensaje de error si no se pueden cambiar los permisos.
        MessageBoxW(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::MemorySetChar(LPVOID address, const char newValue[])
{
    DWORD oldProtect, newProtect = PAGE_EXECUTE_READWRITE;
    if (VirtualProtect(address, strlen(newValue) + 1, newProtect, &oldProtect))
    {
        strcpy_s(static_cast<char*>(address), strlen(newValue) + 1, newValue);
        VirtualProtect(address, strlen(newValue) + 1, oldProtect, &oldProtect);
    }
    else
    {
        MessageBox(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::SetByte(LPVOID address, BYTE newValue)
{
    DWORD oldProtect, newProtect = PAGE_EXECUTE_READWRITE;

    if (VirtualProtect(address, 1, newProtect, &oldProtect))
    {
        *((BYTE*)address) = newValue;
        VirtualProtect(address, 1, oldProtect, &oldProtect);
    }
    else
    {
        MessageBox(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::ReplaceBytes(LPVOID address, const BYTE* newBytes, size_t numBytes)
{
    DWORD oldProtect;
    const DWORD newProtect = PAGE_EXECUTE_READWRITE;

    if (VirtualProtect(address, numBytes, newProtect, &oldProtect))
    {
        memcpy(address, newBytes, numBytes);
        VirtualProtect(address, numBytes, oldProtect, &oldProtect);

        // Opcional: Forzar la escritura si es memoria ejecutable
        FlushInstructionCache(GetCurrentProcess(), address, numBytes);
    }
    else
    {
        MessageBox(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::ReplaceBytes(LPVOID address, const BYTE* newBytes)
{
    DWORD oldProtect, newProtect = PAGE_EXECUTE_READWRITE;
    size_t numBytes = sizeof(newBytes);

    if (VirtualProtect(address, numBytes, newProtect, &oldProtect))
    {
        memcpy(address, newBytes, numBytes);
        VirtualProtect(address, numBytes, oldProtect, &oldProtect);
    }
    else
    {
        MessageBox(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::MemoryCpy(LPVOID address, void* newValue) // OK
{
    DWORD oldProtect, newProtect = PAGE_EXECUTE_READWRITE;
    size_t numBytes = sizeof(newValue);

    if (VirtualProtect(address, numBytes, newProtect, &oldProtect))
    {
        memcpy(address, newValue, numBytes);

        VirtualProtect(address, numBytes, oldProtect, &newProtect);
    }
    else
    {
        MessageBox(NULL, L"Error al cambiar los permisos de acceso.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::MemorySet(void* offset, BYTE value, size_t size)
{
    if (offset == nullptr || size == 0)
    {
        MessageBox(NULL ,L"Error: la dirección de memoria o el tamaño no son válidos.", L"Alerta", MB_OK | MB_ICONWARNING);
        return;
    }

    DWORD OldProtect;
    if (!VirtualProtect(offset, size, PAGE_EXECUTE_READWRITE, &OldProtect))
    {
        MessageBox(NULL, L"Error: no se pudo cambiar las protecciones de la memoria.", L"Alerta", MB_OK | MB_ICONWARNING);
        return;
    }

    // Configurar la memoria con el valor especificado
    memset(offset, value, size);

    // Restaurar las protecciones originales
    if (!VirtualProtect(offset, size, OldProtect, &OldProtect))
    {
        MessageBox(NULL, L"Error: no se pudo restaurar las protecciones de la memoria.", L"Alerta", MB_OK | MB_ICONWARNING);
    }
}

void Utils::VirtualizeOffset(void* offset, size_t size)
{
    // Validar parámetros
    if (size < 5)
    {
        std::cerr << "Error: el tamaño debe ser al menos 5 bytes.\n";
        return;
    }

    DWORD OldProtect;
    if (!VirtualProtect(offset, size, PAGE_EXECUTE_READWRITE, &OldProtect))
    {
        std::cerr << "Error: no se pudo cambiar las protecciones de la memoria.\n";
        return;
    }

    // Asignar memoria para el hook. Se necesitan 5 bytes para el salto más el código original.
    void* HookAddr = VirtualAlloc(nullptr, size + 5, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!HookAddr)
    {
        std::cerr << "Error: no se pudo asignar memoria.\n";
        VirtualProtect(offset, size, OldProtect, &OldProtect); // Restaurar las protecciones.
        return;
    }

    // Copiar el código original al área del Hook.
    memcpy(HookAddr, offset, size);

    // Configurar el salto al código original al final del Hook.
    if (sizeof(void*) == 8)
    { // Arquitectura x64
        // Salto RIP-relativo de 5 bytes.
        *(BYTE*)((BYTE*)HookAddr + size) = 0xE9; // Instrucción JMP.
        *(DWORD*)((BYTE*)HookAddr + size + 1) = (DWORD)((uintptr_t)offset + size - ((uintptr_t)HookAddr + size + 5));
    }
    else
    {                                              // Arquitectura x86
        *(BYTE*)((BYTE*)HookAddr + size) = 0xE9; // Instrucción JMP.
        *(DWORD*)((BYTE*)HookAddr + size + 1) = (DWORD)((uintptr_t)offset + size - ((uintptr_t)HookAddr + size + 5));
    }

    // Configurar el salto inicial desde la dirección `offset` al nuevo Hook.
    *(BYTE*)offset = 0xE9; // Instrucción JMP.

    if (sizeof(void*) == 8)
    { // Arquitectura x64
        *(DWORD*)((BYTE*)offset + 1) = (DWORD)((uintptr_t)HookAddr - ((uintptr_t)offset + 5));
    }
    else
    { // Arquitectura x86
        *(DWORD*)((BYTE*)offset + 1) = (DWORD)((uintptr_t)HookAddr - ((uintptr_t)offset + 5));
    }

    // Rellenar cualquier espacio restante con NOPs.
    memset((BYTE*)offset + 5, 0x90, size - 5);

    // Restaurar protecciones originales.
    if (!VirtualProtect(offset, size, OldProtect, &OldProtect))
    {
        std::cerr << "Error: no se pudo restaurar las protecciones de la memoria.\n";
        VirtualFree(HookAddr, 0, MEM_RELEASE); // Liberar memoria si algo falla.
        return;
    }

    std::cout << "Virtualización completada correctamente.\n";
}

std::string Utils::ReadDynamicString(HANDLE processHandle, LPCVOID address)
{
    const SIZE_T chunkSize = 64; // Leer en bloques pequeños
    std::vector<char> buffer;
    SIZE_T bytesRead = 0;
    char tempBuffer[chunkSize];

    do
    {
        if (ReadProcessMemory(processHandle, address, tempBuffer, chunkSize, &bytesRead))
        {
            buffer.insert(buffer.end(), tempBuffer, tempBuffer + bytesRead);
            address = (LPCVOID)((uintptr_t)address + bytesRead);
        }
        else
        {
            std::cerr << "Error al leer memoria: " << GetLastError() << std::endl;
            return "";  // Retorna una cadena vacía en caso de error
        }
    } while (bytesRead == chunkSize && tempBuffer[chunkSize - 1] != '\0');

    // Asegurarse de que el buffer está null-terminated
    if (buffer.back() != '\0')
        buffer.push_back('\0');

    // Convertir el vector de caracteres en un std::string y devolverlo
    return std::string(buffer.begin(), buffer.end());
}

void Utils::LoadLibrary_(const wchar_t dllFileName[])
{
    HMODULE hModule = LoadLibrary(dllFileName);
    if (hModule != NULL)
    {
        FreeLibrary(hModule);
    }
    else
    {
        MessageBox(NULL, L"No se pudo cargar la DLL.", L"Error", MB_ICONWARNING | MB_OK);
    }
}

LPCWSTR Utils::ConvertToLPCWSTR(const char* str)
{
    std::wstring* wideStr = new std::wstring(str, str + strlen(str));  // Convertir de char* a wstring
    return wideStr->c_str();
}
// Función para realizar una solicitud POST con JSON

std::wstring Utils::StringToWString(const std::string& str)
{
    return std::wstring(str.begin(), str.end());
}

std::string IntToHex(int value, bool uppercase = false) {
    std::stringstream stream;
    if (uppercase)
        stream << std::uppercase;

    stream << "0x"
        << std::hex
        << std::setw(8)  // para mostrar siempre 8 dígitos
        << std::setfill('0')
        << value;

    return stream.str();
}

void Utils::WriteLog(const std::string& text) {
    std::ofstream log("hook_log.txt", std::ios::app); // Append mode
    if (log.is_open()) {
        // Agregar timestamp
        std::time_t now = std::time(nullptr);
        char timeStr[26];
        ctime_s(timeStr, sizeof(timeStr), &now);
        timeStr[strlen(timeStr) - 1] = '\0'; // Quitar \n
        log << text << std::endl;
        //log << "[" << timeStr << "] " << text << std::endl;
        log.close();
    }
}

void Utils::CreateConsole()
{
    AllocConsole();
    FILE* fDummy;
    freopen_s(&fDummy, "CONIN$", "r", stdin);
    freopen_s(&fDummy, "CONOUT$", "w", stdout);
    freopen_s(&fDummy, "CONOUT$", "w", stderr);

    SetConsoleTitleA("Consola Debug");
    std::cout.clear();
    std::clog.clear();
    std::cerr.clear();
    std::cin.clear();

    std::cout << "[*] Consola inicializada.\n";
}
