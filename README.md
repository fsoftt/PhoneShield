# Tranqui

Identificador de llamadas y bloqueador de spam **open source y orientado a la privacidad**, para Android, que nunca guarda números de teléfono en texto plano.

> Estado: en diseño. Ver la [especificación v1](docs/especificacion-v1.md) y las [notas legales para Colombia](docs/legal-colombia.md).

## Cómo funciona

- Cuando entra una llamada, la app muestra quién llama:
  - 🟢 **Verde** — está en tus contactos.
  - 🔵 **Azul** — la comunidad lo identifica.
  - 🔴 **Rojo** — la comunidad lo reportó como spam.
  - 🟠 **Naranja** — no hay datos.
- Puedes bloquear números y, después de la llamada, reportarlos como spam con una etiqueta (ej. "Spam Claro").
- Puedes aportar tu agenda de forma opcional. El servidor solo guarda un hash con clave secreta de cada número, nunca el número.

## Desarrollo local

Requisitos: SDK de .NET 10, Docker (también para los tests de integración, que levantan PostgreSQL con Testcontainers). Para la app: workload `maui-android`, JDK 21 y Android SDK.

```bash
cp .env.example .env                      # y define POSTGRES_PASSWORD
docker compose up -d                      # PostgreSQL y Seq
dotnet tool restore                       # dotnet-ef para migraciones
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Database=tranqui;Username=tranqui;Password=<POSTGRES_PASSWORD>" --project src/Api
dotnet user-secrets set "Firebase:ProjectId" "<id-del-proyecto-firebase>" --project src/Api
dotnet user-secrets set "PhoneHashing:CurrentKeyVersion" "1" --project src/Api
dotnet user-secrets set "PhoneHashing:Keys:1" "$(openssl rand -base64 32)" --project src/Api
dotnet user-secrets set "NameProtection:Key" "$(openssl rand -base64 32)" --project src/Api
dotnet user-secrets set "ContributorIds:Key" "$(openssl rand -base64 32)" --project src/Api
dotnet build Tranqui.Backend.slnf     # backend + tests (sin la app)
dotnet test Tranqui.Backend.slnf
dotnet run --project src/Api              # aplica migraciones al arrancar; GET /health

# Nueva migración tras cambiar el modelo:
dotnet ef migrations add <Nombre> --project src/Infrastructure --output-dir Persistence/Migrations

dotnet workload install maui-android
dotnet build src/App -f net10.0-android   # requiere Android SDK
```

Logs estructurados en Seq: http://localhost:8081.

| Configuración | Para qué | Dónde |
|---|---|---|
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL local | `.env` (no se versiona) |
| `ConnectionStrings:Postgres` | Conexión a PostgreSQL (incluye la contraseña) | user-secrets / variable de entorno `ConnectionStrings__Postgres` |
| `Firebase:ProjectId` | ID del proyecto de Firebase; el API solo acepta tokens emitidos para ese proyecto | user-secrets / variable de entorno `Firebase__ProjectId` |
| `NameProtection:Key` | **Secreto.** Clave maestra en Base64 (mínimo 32 bytes) para cifrar los nombres. Si se pierde, los nombres guardados quedan ilegibles | user-secrets / variable de entorno `NameProtection__Key` |
| `ContributorIds:Key` | **Secreto.** Clave en Base64 (mínimo 32 bytes) que convierte cada cuenta en un identificador seudónimo de aportante. Si cambia, los aportes previos ya no se pueden asociar ni retirar | user-secrets / variable de entorno `ContributorIds__Key` |
| `RateLimiting:Lookup:PerHour` / `PerDay` | Opcional. Consultas por usuario (por defecto 60/hora y 300/día) | `appsettings.json` / variables de entorno |
| `RateLimiting:Reports:PerDay` | Opcional. Reportes por usuario (por defecto 20/día) | `appsettings.json` / variables de entorno |
| `PhoneHashing:CurrentKeyVersion` | Versión vigente de la clave de hash (empieza en `1`) | user-secrets / variable de entorno `PhoneHashing__CurrentKeyVersion` |
| `PhoneHashing:Keys:<versión>` | **Secreto.** Clave HMAC en Base64, mínimo 32 bytes. Todas las versiones desde la 1 deben seguir configuradas. Si se pierde, todos los hashes quedan inservibles: guarda una copia cifrada fuera del servidor | user-secrets / variable de entorno `PhoneHashing__Keys__1` |

## Licencia

[AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html).
