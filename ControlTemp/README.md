# Control de Temperaturas

App para Windows 10/11 que muestra las temperaturas del PC (CPU, GPU, discos), avisa cuando algo se calienta y permite controlar los ventiladores (modo fijo o curva según temperatura).

Instrucciones de uso: [LEEME.txt](LEEME.txt). Requiere el controlador gratuito [PawnIO](https://pawnio.eu/).

## Compilar
Con el SDK de .NET 8:
```
cd ControlTemp/src
dotnet publish -c Release -o ../publish
```
Genera `publish/ControlTemp.exe` (un solo archivo, no necesita instalar .NET).

Usa [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) para leer los sensores.
