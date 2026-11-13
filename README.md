# CRUD de Médicos — API C# (.NET 10) + App Flutter

CRUD completo de la entidad **Médico** con:

- **Backend:** ASP.NET Core (.NET 10) + Entity Framework Core + **SQLite**, en el puerto **5000**.
- **Frontend móvil:** Flutter, consumiendo la API por HTTP.
- **Despliegue:** IIS en Windows (se incluye `web.config` + guía).

```
crud/
├── api/          # API ASP.NET Core (.NET 10)
├── api.Tests/    # Tests xUnit (unitarios + integración)
├── mobile/       # App Flutter
└── .docs/        # Plan y estado de sesión
```

## Entidad `Medico`

| Campo        | Tipo    | Requerido |
|--------------|---------|-----------|
| IdMedico     | int     | PK (auto) |
| Nombre       | string  | sí        |
| ApPaterno    | string  | sí        |
| ApMaterno    | string  | no        |
| Telefono     | string  | no        |
| Direccion    | string  | no        |
| Especialidad | string  | sí        |
| Estatus      | bool    | sí (def. true = activo) |

## Endpoints

| Método | Ruta                 | Descripción            |
|--------|----------------------|------------------------|
| GET    | `/api/medicos`       | Lista todos            |
| GET    | `/api/medicos/{id}`  | Obtiene uno            |
| POST   | `/api/medicos`       | Crea                   |
| PUT    | `/api/medicos/{id}`  | Actualiza              |
| DELETE | `/api/medicos/{id}`  | Elimina                |

En desarrollo, Swagger queda en `http://localhost:5000/swagger`.

---

## 1. Correr la API (desarrollo, Linux/Windows/Mac)

> En esta máquina Linux el SDK se instaló en `~/.dotnet`. Agregalo al PATH:
> ```bash
> export PATH="$HOME/.dotnet:$PATH"
> ```
> En tu computadora Windows, instalá el **.NET 10 SDK** y `dotnet` queda en el PATH solo.

```bash
cd api
dotnet restore
dotnet run
```

La API levanta en `http://0.0.0.0:5000`. La base `medicos.db` se crea sola al
arrancar (migración automática + datos de ejemplo).

### Prueba de humo rápida (curl)

```bash
curl http://localhost:5000/api/medicos
curl -X POST http://localhost:5000/api/medicos \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Juan","apPaterno":"Pérez","especialidad":"Cirugía","estatus":true}'
```

## 2. Tests de la API

```bash
cd api.Tests
dotnet test
```

Cubre tests unitarios del controller (repositorio falso en memoria) e
integración de los 5 endpoints contra SQLite en memoria.

## 3. Correr la app Flutter

```bash
cd mobile
flutter pub get
flutter run            # elegí dispositivo/emulador
# o, para probar rápido en escritorio/web:
flutter run -d linux
flutter run -d chrome
```

`baseUrl` se resuelve solo:
- **Emulador Android:** `http://10.0.2.2:5000` (el host se ve así desde el emulador).
- **Desktop/web:** `http://localhost:5000`.

> Si corrés en un **dispositivo físico**, cambiá `baseUrl` por la IP de tu PC
> (ej. `http://192.168.1.50:5000`) al construir `MedicoApiService`.

### Tests Flutter

```bash
cd mobile
flutter test
```

---

## 4. Despliegue en IIS (Windows)

1. Instalá en el servidor Windows el **.NET 10 Hosting Bundle** (incluye el
   módulo `AspNetCoreModuleV2` para IIS) y reiniciá IIS (`iisreset`).
2. Publicá la API:
   ```powershell
   cd api
   dotnet publish -c Release -o .\publish
   ```
   El `web.config` ya está incluido y se copia a `publish`.
3. En **IIS Manager**: creá un sitio (o aplicación) apuntando a la carpeta
   `publish`. En el *binding* del sitio configurá el puerto **5000** (o el que
   uses). El `Application Pool` debe estar en modo **"No Managed Code"**
   (porque .NET corre vía el módulo nativo, no el CLR de IIS).
4. Dale permisos de escritura a la carpeta al usuario del Application Pool
   (`IIS AppPool\<NombreDelPool>`) para que SQLite pueda crear/escribir
   `medicos.db`.
5. Probá `http://<servidor>:5000/api/medicos`.

> Nota: bajo IIS el binding lo controla el sitio, así que la línea
> `UseUrls("http://0.0.0.0:5000")` del `Program.cs` se ignora en ese entorno
> (es la que fija el puerto al correr con `dotnet run` en desarrollo).
