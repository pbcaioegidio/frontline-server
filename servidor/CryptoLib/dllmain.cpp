#include "pch.h"
#include "Utils.h"
#include <windows.h>
#include <psapi.h>

// Función auxiliar para buscar patrones en memoria
LPVOID FindPattern(LPVOID baseAddress, DWORD imageSize, const BYTE* pattern, const char* mask)
{
	DWORD patternLength = strlen(mask);
	BYTE* currentByte = (BYTE*)baseAddress;

	for (DWORD i = 0; i < imageSize - patternLength; i++)
	{
		bool found = true;

		for (DWORD j = 0; j < patternLength; j++)
		{
			// 'x' = byte exacto, cualquier otro = wildcard
			if (mask[j] == 'x' && currentByte[i + j] != pattern[j])
			{
				found = false;
				break;
			}
		}

		if (found)
		{
			return (LPVOID)(currentByte + i);
		}
	}

	return nullptr;
}

// Thread que ejecuta el parche de forma asíncrona
DWORD WINAPI PatchThread(LPVOID lpParam)
{
	Utils::WriteLog("=== Iniciando thread de parcheo ===");
	std::cout << "[*] Thread de parches iniciado\n";

	// Configuración de timeout y intervalos
	const DWORD MAX_WAIT_TIME = 10000;      // 5 segundos máximo de espera
	const DWORD CHECK_INTERVAL = 100;      // Verificar cada 100ms
	const DWORD MAX_ATTEMPTS = 30;

	HMODULE hModule = nullptr;
	MODULEINFO modInfo;
	ZeroMemory(&modInfo, sizeof(MODULEINFO));

	// ============================================
	// FASE 1: Esperar a que el módulo esté cargado
	// ============================================
	Utils::WriteLog("Esperando a que PointBlank.exe se cargue...");

	for (DWORD attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
	{
		hModule = GetModuleHandleA("PointBlank.exe");
		if (hModule)
		{
			char logMsg[256];
			sprintf_s(logMsg, "PointBlank.exe encontrado después de %dms", attempt * CHECK_INTERVAL);
			Utils::WriteLog(logMsg);
			std::cout << "[+] " << logMsg << "\n";
			break;
		}

		Sleep(CHECK_INTERVAL);
	}

	if (!hModule)
	{
		Utils::WriteLog("ERROR: Timeout - No se encontró PointBlank.exe");
		std::cout << "[-] No se encontró PointBlank.exe después de 5 segundos\n";
		MessageBoxW(NULL, L"No se encontró PointBlank.exe después de 5 segundos", L"Error", MB_ICONERROR);
		return FALSE;
	}

	// ============================================
	// FASE 2: Obtener información del módulo
	// ============================================
	HANDLE hProcess = GetCurrentProcess();
	if (!K32GetModuleInformation(hProcess, hModule, &modInfo, sizeof(MODULEINFO)))
	{
		Utils::WriteLog("ERROR: No se pudo obtener información del módulo");
		std::cout << "[-] Error al obtener información del módulo\n";
		MessageBoxW(NULL, L"Error al obtener información del módulo", L"Error", MB_ICONERROR);
		return FALSE;
	}

	char logMsg[256];
	sprintf_s(logMsg, "Base: 0x%p, Tamaño: 0x%X", modInfo.lpBaseOfDll, modInfo.SizeOfImage);
	Utils::WriteLog(logMsg);
	std::cout << "[*] " << logMsg << "\n";

	// ============================================
	// DEFINIR PATRONES
	// ============================================
	BYTE searchPattern1[] = { 0x83, 0xF8, 0x03, 0x72, 0x62, 0x83, 0xF8, 0x04 };
	const char* mask1 = "xxxxxxxx";
	BYTE patchBytes1[] = { 0x83, 0xF8, 0x00, 0x72, 0x62, 0x83, 0xF8, 0x00 };

	BYTE searchPattern2[] = { 0x83, 0xF8, 0x03, 0x72, 0x63, 0x83, 0xF8, 0x04 };
	const char* mask2 = "xxxxxxxx";
	BYTE patchBytes2[] = { 0x83, 0xF8, 0x00, 0x72, 0x63, 0x83, 0xF8, 0x00 };

	LPVOID address1 = nullptr;
	LPVOID address2 = nullptr;

	// ============================================
	// FASE 3: Buscar patrones en bucle
	// ============================================
	Utils::WriteLog("Iniciando búsqueda de patrones...");
	std::cout << "[*] Buscando patrones en memoria...\n";

	for (DWORD attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
	{
		// Buscar patrón #1 si aún no se encontró
		if (!address1)
		{
			address1 = FindPattern(modInfo.lpBaseOfDll, modInfo.SizeOfImage,
				searchPattern1, mask1);
			if (address1)
			{
				sprintf_s(logMsg, "Patrón #1 encontrado en: 0x%p (intento %d)", address1, attempt + 1);
				Utils::WriteLog(logMsg);
				std::cout << "[+] " << logMsg << "\n";
			}
		}

		// Buscar patrón #2 si aún no se encontró
		if (!address2)
		{
			address2 = FindPattern(modInfo.lpBaseOfDll, modInfo.SizeOfImage,
				searchPattern2, mask2);
			if (address2)
			{
				sprintf_s(logMsg, "Patrón #2 encontrado en: 0x%p (intento %d)", address2, attempt + 1);
				Utils::WriteLog(logMsg);
				std::cout << "[+] " << logMsg << "\n";
			}
		}

		// Si ambos patrones fueron encontrados, salir del bucle
		if (address1 && address2)
		{
			Utils::WriteLog("Todos los patrones encontrados, procediendo a aplicar parches");
			std::cout << "[+] Todos los patrones encontrados\n";
			break;
		}

		// Esperar antes del próximo intento
		Sleep(CHECK_INTERVAL);
	}

	// ============================================
	// FASE 4: Aplicar parches
	// ============================================
	bool patch1Applied = false;
	bool patch2Applied = false;

	if (address1)
	{
		Utils::ReplaceBytes(address1, patchBytes1, 8);
		patch1Applied = true;

		sprintf_s(logMsg, "✓ Parche #1 aplicado exitosamente en: 0x%p", address1);
		Utils::WriteLog(logMsg);
		std::cout << "[+] " << logMsg << "\n";
	}
	else
	{
		Utils::WriteLog("✗ Advertencia: No se encontró el patrón #1");
		std::cout << "[-] No se encontró el patrón #1\n";
	}

	if (address2)
	{
		Utils::ReplaceBytes(address2, patchBytes2, 8);
		patch2Applied = true;

		sprintf_s(logMsg, "✓ Parche #2 aplicado exitosamente en: 0x%p", address2);
		Utils::WriteLog(logMsg);
		std::cout << "[+] " << logMsg << "\n";
	}
	else
	{
		Utils::WriteLog("✗ Advertencia: No se encontró el patrón #2");
		std::cout << "[-] No se encontró el patrón #2\n";
	}

	// ============================================
	// RESULTADO FINAL
	// ============================================
	if (patch1Applied || patch2Applied)
	{
		Utils::WriteLog("=== Proceso de parcheo completado exitosamente ===");
		std::cout << "[+] Proceso de parcheo completado\n";
		return TRUE;
	}
	else
	{
		Utils::WriteLog("=== ERROR: No se aplicó ningún parche ===");
		std::cout << "[-] ERROR: No se aplicó ningún parche\n";
		MessageBoxW(NULL, L"No se encontraron los patrones en memoria", L"Advertencia", MB_ICONWARNING);
		return FALSE;
	}
}

// Función exportada que inicia el thread
extern "C" __declspec(dllexport) bool ApplyPointBlankPatch()
{
	Utils::WriteLog("ApplyPointBlankPatch() llamada - Creando thread");
	std::cout << "[*] Iniciando sistema de parches en thread separado\n";

	// Crear thread independiente para no bloquear el flujo principal
	HANDLE hThread = CreateThread(
		NULL,              // Atributos de seguridad por defecto
		0,                 // Tamaño de stack por defecto
		PatchThread,       // Función a ejecutar
		NULL,              // Parámetro (no necesitamos ninguno)
		0,                 // Flags de creación (iniciar inmediatamente)
		NULL               // No necesitamos el ID del thread
	);

	if (hThread == NULL)
	{
		Utils::WriteLog("ERROR: No se pudo crear el thread de parcheo");
		std::cout << "[-] Error al crear el thread\n";
		MessageBoxW(NULL, L"Error al crear el thread de parcheo", L"Error", MB_ICONERROR);
		return false;
	}

	// Cerrar el handle (el thread seguirá ejecutándose)
	CloseHandle(hThread);

	Utils::WriteLog("Thread de parcheo creado exitosamente");
	std::cout << "[+] Thread de parches creado correctamente\n";

	return true; // Retorna inmediatamente
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		DisableThreadLibraryCalls(hModule);

		// Crear consola para debug
		//Utils::CreateConsole();
		std::cout << "[*] DLL inyectada correctamente\n";

		// Iniciar el sistema de parches (no bloquea)
		if (ApplyPointBlankPatch())
		{
			std::cout << "[+] Sistema de parches inicializado\n";
		}
		else
		{
			std::cout << "[-] Error al inicializar sistema de parches\n";
		}
		break;

	case DLL_PROCESS_DETACH:
		Utils::WriteLog("DLL descargada");
		std::cout << "[*] DLL descargada\n";
		break;
	}
	return TRUE;
}