# Estado de sesión — CRUD de Médicos

**Fecha:** 2026-11-13

## Resumen
CRUD de Médicos: API ASP.NET Core (.NET 10) + EF Core **SQLite** en el puerto
5000, consumida por app Flutter. Destino de despliegue: **IIS en Windows**
(incluye `web.config` + guía en el README).

## Decisiones
- **Persistencia:** SQLite (portable a la PC Windows del usuario; migrable a
  SQL Server cambiando el provider de EF Core).
- **API:** patrón repositorio (`IMedicoRepository`) por DIP/testabilidad; DTOs
  separados de la entidad para evitar over-posting.
- **Flutter:** `MedicoApiService` separado de la UI (SRP); `baseUrl`
  autodetectado (10.0.2.2 para emulador Android, localhost para desktop/web).
- **Estatus:** booleano (activo/inactivo).

## Estado de verificación — COMPLETO
- **.NET 10 SDK:** 10.0.404 instalado en `~/.dotnet` (sin sudo).
- **API:** `dotnet build` 0 errores; `dotnet test` **10/10 PASSED** (7 unit + 3
  integración); smoke en vivo con curl sobre los 5 endpoints OK; persistencia
  SQLite verificada sobre reinicio. Migración `InitialCreate` generada.
- **Flutter:** `flutter analyze` sin issues; `flutter test` 5/5 PASSED;
  `flutter build web` ✓ (artefacto generado).
- Matriz completa en `.docs/qa-matrix.md`.

## Pendientes / recomendaciones
- 8 advertencias NU1902/NU1903 (vulnerabilidades en paquetes transitivos de
  .NET 10). No bloquean; pinnear versiones parcheadas antes de producción.
- Para correr: `export PATH="$HOME/.dotnet:$PATH"` antes de `dotnet` (en Windows
  el SDK queda en PATH solo).
- `dotnet-ef` global tool requiere `DOTNET_ROOT=$HOME/.dotnet` en esta máquina
  (el SDK está en ubicación no estándar).

## external_waits
- Ninguno.
