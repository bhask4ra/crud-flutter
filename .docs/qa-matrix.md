# Matriz QA — CRUD de Médicos

**Fecha:** 2026-11-13 · Árbol final verificado (no un estado intermedio).

## API (ASP.NET Core .NET 10)

| Nivel | Estado | Evidencia / comando |
|-------|--------|---------------------|
| Unit | **PASSED** | `dotnet test api.Tests` — 7 tests del `MedicosController` con repositorio falso en memoria (GetAll, GetById 404, Create 201, Update 204/404, Delete 204/404). |
| Integration | **PASSED** | Mismos `dotnet test` — 3 tests con `WebApplicationFactory<Program>` sobre SQLite en memoria: seed inicial, ciclo CRUD completo (POST→GET→PUT→DELETE→404), y POST inválido → 400. **10/10 en total.** |
| Static | **PASSED (con advertencias)** | `dotnet build` → 0 errores, 8 advertencias NU1902/NU1903 (vulnerabilidades conocidas en paquetes **transitivos** de Microsoft: Microsoft.OpenApi 2.0.0, SQLitePCLRaw 2.1.11, System.Security.Cryptography.Pkcs 9.0.0). No bloquean; ver nota. |
| Smoke | **PASSED** | API levantada en `:5000`; curl sobre los 5 endpoints: GET lista (seed), POST=201, GET/{id}=200, PUT=204 (cambio reflejado), DELETE=204, GET post-delete=404, POST inválido=400. **Persistencia** verificada: registro sobrevive a reinicio de la API (SQLite `medicos.db`). OpenAPI doc=200. |

## App Flutter

| Nivel | Estado | Evidencia / comando |
|-------|--------|---------------------|
| Unit | **PASSED** | `flutter test` — 5 tests de `MedicoApiService` con `MockClient`: getAll parseo, create POST→201, update PUT→204, delete→204, error 500→`ApiException`. |
| Integration | **NOT APPLICABLE** | La integración real UI↔API se cubre del lado API (contrato HTTP) + el test del servicio con cliente simulado. No hay segundo componente interno que integrar en el cliente; un test de widget end-to-end contra la API viva excede el alcance del CRUD. |
| Static | **PASSED** | `flutter analyze` → "No issues found!". |
| Smoke | **PASSED** | `flutter build web` → `✓ Built build/web` (`main.dart.js`, 2.5MB). Compila a artefacto ejecutable. (La UI corre con `flutter run -d chrome/linux/<emulador>`.) |

## Nota sobre advertencias de seguridad (NU1902/NU1903)
Son advisories en dependencias **transitivas** que arrastran los propios
paquetes de .NET 10 (`Microsoft.AspNetCore.OpenApi` → `Microsoft.OpenApi`;
`EFCore.Sqlite` → `SQLitePCLRaw`). En una API de desarrollo en localhost no son
explotables. Para producción conviene fijar versiones parcheadas cuando estén
disponibles (`<PackageReference>` explícito). No se forzó el pin para no romper
la cadena de restore sobre una red lenta; queda como recomendación.
