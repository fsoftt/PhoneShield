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

La app lee tres valores al compilar (no son secretos). Para desarrollo, crea `src/App/Tranqui.App.local.props` (no se versiona):

```xml
<Project>
  <PropertyGroup>
    <TranquiFirebaseApiKey>tu-web-api-key-de-firebase</TranquiFirebaseApiKey>
    <!-- Opcional; en Debug por defecto es http://10.0.2.2:5254/ (el API corriendo en tu PC, visto desde el emulador) -->
    <TranquiApiBaseUrl>http://10.0.2.2:5254/</TranquiApiBaseUrl>
    <!-- Opcional; número del proyecto de Google Cloud para Play Integrity (0 = el vinculado en Play Console) -->
    <TranquiCloudProjectNumber>0</TranquiCloudProjectNumber>
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
| `RateLimiting:Blocks:PerDay` | Opcional. Bloqueos y desbloqueos enviados por usuario (por defecto 100/día) | `appsettings.json` / variables de entorno |
| `RateLimiting:ContactUploads:BatchesPerDay` | Opcional. Lotes de contactos por usuario (por defecto 20/día, de hasta 500 contactos) | `appsettings.json` / variables de entorno |
| `RateLimiting:Appeals:PerDay` | Opcional. Llamadas de apelación por usuario, fallidas incluidas (por defecto 10/día); las exitosas las limitan las cuotas por número, cuenta y dispositivo | `appsettings.json` / variables de entorno |
| `PlayIntegrity:ServiceAccountKey` | **Secreto.** Base64 del JSON de la cuenta de servicio de Google Cloud que decodifica los tokens de Play Integrity. Sin él, las apelaciones se rechazan | user-secrets / variable de entorno `PlayIntegrity__ServiceAccountKey` |
| `BackOffice:AdminUids:<n>` | Uids de Firebase con acceso al back office (`/admin`). Sin ninguno, nadie entra | user-secrets / variable de entorno `BackOffice__AdminUids__0` |
| `BackOffice:FirebaseApiKey` | Clave web de Firebase con la que inicia sesión la página del back office (no es secreta) | `appsettings.json` / variable de entorno `BackOffice__FirebaseApiKey` |
| `BackOffice:AutomaticPurge` | Opcional. Borrado diario de datos vencidos (por defecto `true`) | `appsettings.json` / variables de entorno |
| `PlayIntegrity:PackageName` | Opcional. Paquete de la app (por defecto `com.fsoftt.tranqui`) | `appsettings.json` / variables de entorno |
| `PhoneHashing:CurrentKeyVersion` | Versión vigente de la clave de hash (empieza en `1`) | user-secrets / variable de entorno `PhoneHashing__CurrentKeyVersion` |
| `PhoneHashing:Keys:<versión>` | **Secreto.** Clave HMAC en Base64, mínimo 32 bytes. Todas las versiones desde la 1 deben seguir configuradas. Si se pierde, todos los hashes quedan inservibles: guarda una copia cifrada fuera del servidor | user-secrets / variable de entorno `PhoneHashing__Keys__1` |

## Sitio web

El sitio, en español e inglés (portada, privacidad, política de tratamiento de datos, términos), está en `website/` (VitePress) y se publica en GitHub Pages con cada cambio en `main`:

```bash
cd website && npm install && npm run dev
```

### Apelaciones

Se hacen desde la app (**Cuenta → ¿Tu número aparece mal?**); en el sitio solo se explica el canal manual por correo.
Cada SMS y cada apelación se cuentan contra el número (1 al mes), la cuenta y el dispositivo (1 al mes y 3 al año),
cada uno por separado con su propio hash con clave. La cuenta debe tener al menos una semana y cada paso exige un
veredicto de Play Integrity (app reconocida por Google Play en un dispositivo íntegro) con un nonce que amarra la acción,
el dispositivo y el número. El número se prueba con un inicio de sesión por SMS de Firebase, que la app borra al
terminar. Para activarlo:

1. Firebase: plan Blaze (requerido para SMS; los primeros 10 SMS/día son gratis), proveedor **Teléfono** habilitado,
   política de región de SMS solo para Colombia y una alerta de presupuesto.
2. Play Console → Integridad de la app: vincular un proyecto de Google Cloud y activar la Play Integrity API (cuota
   gratuita de 10 000 solicitudes/día). Registrar la huella SHA-256 de firma de la app en Firebase.
3. Google Cloud: crear una cuenta de servicio con acceso a la Play Integrity API, descargar su clave JSON y configurarla
   en base64 como `PlayIntegrity:ServiceAccountKey` (`PLAY_INTEGRITY_SERVICE_ACCOUNT_KEY` en `deploy/.env`).

Como Play Integrity solo reconoce la app instalada desde Google Play, las compilaciones de desarrollo y otras tiendas no
pueden apelar desde la app; para ellas queda el canal manual. Las apelaciones `ReviewSpam` quedan pendientes en la tabla
`appeals` para revisión manual en la v1.

### Back office

`https://<tu-api>/admin` (servido por el mismo API, con una política de seguridad de contenido estricta): resumen de
totales, revisión de apelaciones "no es spam" (aprobar hace que los reportes anteriores dejen de contar; aprobar o
rechazar borra el motivo y el correo) y borrado de datos vencidos. Los datos vencidos también se borran solos cada día
según `Domain/Retention/RetentionRules`. Entran solo las cuentas de Firebase con correo verificado cuyo uid esté en
`BackOffice:AdminUids`; el uid se ve en la consola de Firebase → Authentication.

## Despliegue

Ver [`deploy/README.md`](deploy/README.md): un VPS de ~US$5/mes con Docker Compose (API, PostgreSQL y Caddy con HTTPS automático).

## Licencia

[AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html).
