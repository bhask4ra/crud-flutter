# Plan actual — CRUD de Médicos (API C# + App Flutter)

## [OBJETIVO ACTUAL]
Construir un CRUD completo de la entidad **Médico** expuesto por una API REST en
ASP.NET Core (.NET 10) escuchando en el puerto 5000, con persistencia SQLite,
consumida por una app móvil en Flutter. Entregar artefactos de despliegue para
IIS en Windows.

## [ARQUITECTURA/DISEÑO]

### Entidad `Medico`
| Campo        | Tipo C#  | Notas                                  |
|--------------|----------|----------------------------------------|
| IdMedico     | int      | PK, identity                           |
| Nombre       | string   | requerido                              |
| ApPaterno    | string   | requerido                              |
| ApMaterno    | string   | opcional                               |
| Telefono     | string   | opcional                               |
| Direccion    | string   | opcional                               |
| Especialidad | string   | requerido                              |
| Estatus      | bool     | true = activo, false = inactivo (def.) |

### API (C# / .NET 10 / EF Core + SQLite)
- **Opción A:** Controller usa `DbContext` directo. Simple, menos código.
- **Opción B (ELEGIDA):** `IMedicoRepository` ← `MedicoRepository` (EF Core);
  el controller depende de la abstracción.
  *Justificación (SOLID):* DIP + testabilidad — permite tests unitarios del
  controller sin base de datos real y deja el proveedor de persistencia
  intercambiable (SQLite → SQL Server) sin tocar la capa web. El costo extra de
  una interfaz es bajo frente al beneficio en el gate de QA.
- DTOs separados de la entidad (no exponer el modelo EF directamente → evita
  over-posting; SRP).
- Validación con DataAnnotations en los DTOs de entrada.
- CORS abierto para el cliente móvil.
- Kestrel fijado a `http://0.0.0.0:5000` (accesible desde emulador/dispositivo).
- Endpoints REST:
  - `GET    /api/medicos`
  - `GET    /api/medicos/{id}`
  - `POST   /api/medicos`
  - `PUT    /api/medicos/{id}`
  - `DELETE /api/medicos/{id}`
- `web.config` + guía de publicación para IIS (Windows).

### App móvil (Flutter)
- **Opción A:** llamadas HTTP dentro de los widgets. Acopla UI y red.
- **Opción B (ELEGIDA):** capa `MedicoApiService` (http) + modelo `Medico` +
  pantallas (lista / formulario alta-edición). *Justificación:* SRP —
  separación UI/red, servicio testeable de forma aislada.
- `baseUrl` configurable; `10.0.2.2:5000` para emulador Android, `localhost`
  para desktop/web.

## [PASOS DE IMPLEMENTACIÓN (Checklist)]
- [x] Instalar .NET 10 SDK en `~/.dotnet` — 10.0.404
- [x] Scaffold proyecto API `api/` (escrito a mano) + paquetes EF Core SQLite
- [x] Modelo `Medico`, `AppDbContext`, seed + migración `InitialCreate` generada
- [x] DTOs + `IMedicoRepository` / `MedicoRepository`
- [x] `MedicosController` con los 5 endpoints + validación + CORS + Kestrel :5000
- [x] `web.config` + sección de despliegue IIS en el README
- [x] Tests API: unitarios (repo falso) + integración (WebApplicationFactory) — VERDE 10/10
- [x] Scaffold app Flutter `mobile/` (platform folders generados)
- [x] Modelo + `MedicoApiService` + pantallas lista/formulario
- [x] Test del servicio Flutter (mock http) + `flutter analyze` — VERDE (5/5, 0 issues)
- [x] Gate QA (qa-test-gate): matriz unit/integration/static/smoke — completa (.docs/qa-matrix.md)
- [x] README raíz: cómo correr API, app, y desplegar en IIS

## [CRITERIOS DE ACEPTACIÓN]
1. `dotnet build` de la API sin errores; API levanta en `:5000`.
2. Los 5 endpoints responden correctamente (verificado por smoke con curl):
   crear, listar, obtener por id, actualizar, eliminar un médico.
3. Persistencia SQLite real: los datos sobreviven a un reinicio de la API.
4. Tests de la API en verde (unit + integración).
5. App Flutter compila (`flutter analyze` limpio) y su servicio tiene test en verde.
6. `web.config` presente y README con pasos de publicación en IIS documentados.
7. Matriz de qa-test-gate completa (cada nivel PASSED/FAILED/BLOCKED/NA con evidencia).
