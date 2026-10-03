# Tranqui

Identificador de llamadas y bloqueador de spam **open source y orientado a la privacidad**, para Android, que nunca guarda números de teléfono en texto plano.

> Estado: en desarrollo. Sitio: https://fsoftt.github.io/Tranqui/ · [especificación v1](docs/especificacion-v1.md) · [notas legales para Colombia](docs/legal-colombia.md).

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

La app lee dos valores al compilar (no son secretos). Para desarrollo, crea `src/App/Tranqui.App.local.props` (no se versiona):

```xml
<Project>
  <PropertyGroup>
    <TranquiFirebaseApiKey>tu-web-api-key-de-firebase</TranquiFirebaseApiKey>
    <!-- Opcional; en Debug por defecto es http://10.0.2.2:5254/ (el API corriendo en tu PC, visto desde el emulador) -->
    <TranquiApiBaseUrl>http://10.0.2.2:5254/</TranquiApiBaseUrl>
  </PropertyGroup>
</Project>
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
| `RateLimiting:ContactUploads:BatchesPerDay` | Opcional. Lotes de contactos por usuario (por defecto 20/día, de hasta 500 contactos) | `appsettings.json` / variables de entorno |
| `RateLimiting:Appeals:PerHour` | Opcional. Solicitudes de apelación por IP (por defecto 10/hora); aparte, cada número tiene un SMS y una apelación al mes | `appsettings.json` / variables de entorno |
| `Cors:AllowedOrigins:0` | Origen del sitio web que llama a los endpoints de apelación (ej. `https://fsoftt.github.io`) | variable de entorno `Cors__AllowedOrigins__0` |
| `PhoneHashing:CurrentKeyVersion` | Versión vigente de la clave de hash (empieza en `1`) | user-secrets / variable de entorno `PhoneHashing__CurrentKeyVersion` |
| `PhoneHashing:Keys:<versión>` | **Secreto.** Clave HMAC en Base64, mínimo 32 bytes. Todas las versiones desde la 1 deben seguir configuradas. Si se pierde, todos los hashes quedan inservibles: guarda una copia cifrada fuera del servidor | user-secrets / variable de entorno `PhoneHashing__Keys__1` |

## Sitio web

El sitio, en español e inglés (portada, privacidad, política de tratamiento de datos, términos), está en `website/` (VitePress) y se publica en GitHub Pages con cada cambio en `main`:

```bash
cd website && npm install && npm run dev
```

### Formulario de apelación

Las personas sin cuenta demuestran que un número es suyo con un SMS de Firebase, desde el sitio (`/guia/apelacion`).
El API permite **un SMS y una apelación por número cada 30 días**. Para activarlo:

1. En Firebase: plan Blaze (requerido para SMS; los primeros 10 SMS/día son gratis), proveedor **Teléfono** habilitado,
   `fsoftt.github.io` en dominios autorizados, política de región de SMS solo para Colombia y una alerta de presupuesto.
2. En GitHub → Settings → Secrets and variables → Actions → **Variables** (son valores públicos, no secretos):
   `TRANQUI_API_URL`, `FIREBASE_API_KEY`, `FIREBASE_AUTH_DOMAIN`, `FIREBASE_PROJECT_ID`. Sin ellas el formulario muestra
   el correo de contacto.
3. En el servidor: `WEBSITE_ORIGIN` en `deploy/.env` si el sitio no está en `https://fsoftt.github.io`.

Límite honesto: la clave web de Firebase es pública, así que alguien decidido podría pedirle SMS a Firebase sin pasar
por nuestro API. Lo mitigan reCAPTCHA, la política de región y las cuotas de Firebase; nuestro API igual acepta solo
una apelación por número al mes. Las apelaciones `ReviewSpam` quedan pendientes en la tabla `appeals` para revisión
manual en la v1.

## Despliegue

Ver [`deploy/README.md`](deploy/README.md): un VPS de ~US$5/mes con Docker Compose (API, PostgreSQL y Caddy con HTTPS automático).

## Licencia

[AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html).
