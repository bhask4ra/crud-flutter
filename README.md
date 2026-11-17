# CRUD de Productos — API .NET 10 + Flutter (desplegado en IIS)

CRUD de **Productos** con:

- **Backend:** ASP.NET Core (.NET 10) + EF Core + SQLite, desplegado en **IIS** (`http://localhost:5000`).
- **Frontend:** Flutter, corriendo en un emulador Android de Android Studio.
- **Base de datos:** SQLite (`productos.db`), se crea sola con migraciones automáticas y datos de ejemplo al primer arranque.

> Nota de naming: el `.csproj` y algunos archivos dicen `Medicos`, pero el código real (controlador, modelo, repositorio, Flutter) es de **Productos**. No afecta la ejecución.

```
crud-flutter/
├── api/                  # Código fuente ASP.NET Core (.NET 10)
├── api.Tests/            # Tests xUnit (unitarios + integración)
├── mobile/               # App Flutter
├── publish/api/          # Binarios publicados que usa IIS
└── deploy-iis.ps1        # Script de despliegue inicial en IIS
```

---

## 1. Requisitos (una sola vez)

En la PC donde se corre:

- **.NET 10 SDK** y **.NET 10 Hosting Bundle** (para que IIS pueda hospedar la API).
- **IIS** con `WebAdministration` (servicio `W3SVC`).
- **Flutter SDK** en el PATH.
- **Android Studio** con un emulador creado.
- **ADB** en el PATH (viene con `platform-tools` de Android SDK).

Verificar:

```powershell
dotnet --list-runtimes
```

```powershell
Get-Service W3SVC
```

```powershell
flutter doctor
```

```powershell
adb devices
```

---

## 2. Demo en vivo (caso normal: ya está todo desplegado)

IIS arranca solo con Windows, así que la API ya está disponible sin hacer nada.

**Paso 1 — abrir el emulador de Android Studio** (Device Manager → Play).

**Paso 2 — verificar que la API responde en IIS:**

```bash
curl http://localhost:5000/api/productos
```

La primera llamada puede tardar ~15 s (EF Core crea la base y aplica migraciones en frío). Las siguientes son instantáneas.

**Paso 3 — correr la app Flutter en el emulador (con hot reload):**

```powershell
cd C:\Users\damia\Documents\crud-flutter\mobile
```

```powershell
flutter run
```

Si hay varios dispositivos (Chrome, Edge, Windows, emulador), forzá el emulador:

```powershell
flutter run -d emulator-5554
```

Dentro de `flutter run`:

- `r` → hot reload
- `R` → hot restart
- `q` → salir

**Esos son los únicos comandos necesarios para una demo sin cambios de código.**

---

## 3. Despliegue inicial en IIS (una sola vez)

Si la PC no tiene el sitio IIS creado todavía:

**3.1. Publicar la API:**

```powershell
cd C:\Users\damia\Documents\crud-flutter\api
```

```powershell
dotnet publish -c Release -o ..\publish\api
```

**3.2. Correr el script de despliegue como Administrador** (crea Application Pool, sitio IIS en puerto 5000, copia archivos y da permisos a SQLite):

```powershell
cd C:\Users\damia\Documents\crud-flutter
```

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy-iis.ps1
```

**3.3. Verificar que el sitio quedó arriba:**

```bash
curl http://localhost:5000/api/productos
```

---

## 4. Re-desplegar la API cuando cambie código .NET

```powershell
cd C:\Users\damia\Documents\crud-flutter\api
```

```powershell
dotnet publish -c Release -o ..\publish\api
```

```powershell
Stop-Website -Name "CatProductosApi"
```

```powershell
Copy-Item -Path "C:\Users\damia\Documents\crud-flutter\publish\api\*" -Destination "C:\inetpub\wwwroot\catproductos-api" -Recurse -Force
```

```powershell
Start-Website -Name "CatProductosApi"
```

```bash
curl http://localhost:5000/api/productos
```

> IIS puede tener los DLL bloqueados mientras el worker corre; por eso `Stop-Website` antes de copiar.

---

## 5. Re-compilar e instalar la app Flutter sin hot reload

Alternativa a `flutter run` (útil si querés dejar el APK instalado y cerrar la terminal):

```powershell
cd C:\Users\damia\Documents\crud-flutter\mobile
```

```powershell
flutter pub get
```

```powershell
flutter build apk --debug
```

```powershell
adb -s emulator-5554 install -r build\app\outputs\flutter-apk\app-debug.apk
```

```powershell
adb -s emulator-5554 shell am start -n com.example.medicos_app/.MainActivity
```

Detenerla cuando quieras:

```powershell
adb -s emulator-5554 shell am force-stop com.example.medicos_app
```

---

## 6. Comandos útiles de diagnóstico

**Ver quién escucha en el puerto 5000:**

```powershell
Get-NetTCPConnection -LocalPort 5000 -State Listen
```

**Ver sitios de IIS:**

```powershell
Get-Website | Format-Table Name, State, PhysicalPath
```

**Probar la API desde la IP de LAN (útil para dispositivo físico, no el emulador):**

```powershell
(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike '*Loopback*' -and $_.IPAddress -notlike '169.*' }).IPAddress
```

```bash
curl http://<IP_de_tu_PC>:5000/api/productos
```

**Ver logs de la app Flutter en vivo:**

```powershell
adb -s emulator-5554 logcat --pid=$(adb -s emulator-5554 shell pidof com.example.medicos_app) *:I
```

**Reiniciar IIS completo:**

```powershell
iisreset
```

---

## 7. ¿Cómo se conecta Flutter con la API?

El servicio [producto_api_service.dart](mobile/lib/services/producto_api_service.dart) resuelve la URL base según plataforma:

| Plataforma           | URL base                     |
|----------------------|------------------------------|
| Emulador Android     | `http://10.0.2.2:5000`       |
| Escritorio / Web     | `http://localhost:5000`      |
| Dispositivo físico   | pasar `--dart-define=API_BASE_URL=http://<IP_PC>:5000` |

> `10.0.2.2` es el alias que el emulador de Android usa para alcanzar al `localhost` de la PC anfitriona.

Para dispositivo físico:

```powershell
flutter run --dart-define=API_BASE_URL=http://192.168.1.222:5000
```

---

## 8. Endpoints de la API

Base: `http://localhost:5000`

| Método | Ruta                                | Descripción                       |
|--------|-------------------------------------|-----------------------------------|
| GET    | `/api/productos`                    | Lista (filtros: `search`, `tipo`, `estatus`) |
| GET    | `/api/productos/{id}`               | Obtiene uno                       |
| GET    | `/api/productos/resumen`            | Totales de inventario             |
| POST   | `/api/productos`                    | Crea                              |
| PUT    | `/api/productos/{id}`               | Actualiza                         |
| PATCH  | `/api/productos/{id}/existencias`   | Ajusta stock (`{"delta": N}`)     |
| DELETE | `/api/productos/{id}`               | Elimina                           |

---

## 9. Tests

**API:**

```powershell
cd C:\Users\damia\Documents\crud-flutter\api.Tests
```

```powershell
dotnet test
```

**Flutter:**

```powershell
cd C:\Users\damia\Documents\crud-flutter\mobile
```

```powershell
flutter test
```
