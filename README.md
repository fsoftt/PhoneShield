# PhoneShield

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

Requisitos: SDK de .NET 10, Docker. Para la app: workload `maui-android`, JDK 21 y Android SDK.

```bash
cp .env.example .env                      # y define POSTGRES_PASSWORD
docker compose up -d                      # PostgreSQL, Redis y Seq
dotnet build PhoneShield.Backend.slnf     # backend + tests (sin la app)
dotnet test PhoneShield.Backend.slnf
dotnet run --project src/Api              # GET /health

dotnet workload install maui-android
dotnet build src/App -f net10.0-android   # requiere Android SDK
```

Logs estructurados en Seq: http://localhost:8081.

| Configuración | Para qué | Dónde |
|---|---|---|
| `POSTGRES_PASSWORD` | Contraseña de PostgreSQL local | `.env` (no se versiona) |

## Licencia

[AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html).
