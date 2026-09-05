// pch.h: este es un archivo de encabezado precompilado.
// Los archivos que se muestran a continuación se compilan solo una vez, lo que mejora el rendimiento de la compilación en futuras compilaciones.
// Esto también afecta al rendimiento de IntelliSense, incluida la integridad del código y muchas funciones de exploración del código.
// Sin embargo, los archivos que se muestran aquí se vuelven TODOS a compilar si alguno de ellos se actualiza entre compilaciones.
// No agregue aquí los archivos que se vayan a actualizar con frecuencia, ya que esto invalida la ventaja de rendimiento.

#ifndef PCH_H
#define PCH_H

// agregue aquí los encabezados que desea precompilar
#include "framework.h"
#include <wininet.h>
#include <fstream>
#include <psapi.h>
#include <chrono>
#include <thread>
#include <iomanip>
#include <cwchar>  // Para _vswprintf_s
#include <cstdarg> // Para va_list
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iostream>
#include <string>
#include <vector>
#include <sstream>
#include <map>

#pragma comment(lib, "wininet.lib")
#pragma comment(lib, "ws2_32.lib")
#endif //PCH_H
