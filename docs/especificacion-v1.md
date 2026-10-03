# PhoneShield — Especificación v1

Identificador de llamadas y bloqueador de spam open source, orientado a la privacidad. Identifica quién llama y bloquea spam usando datos aportados por la comunidad, sin guardar nunca números de teléfono en texto plano.

Estado: **borrador de decisiones** — fuente de verdad para empezar a construir. Lo marcado como _Pendiente_ requiere decisión antes de implementar esa parte.

---

## 1. Decisiones tomadas

| Tema | Decisión |
|---|---|
| Plataforma v1 | Solo Android (.NET MAUI, `net10.0-android`, mínimo Android 10 / API 29 por `CallScreeningService` + `RoleManager`) |
| Backend | ASP.NET Core minimal APIs (.NET 10), PostgreSQL, Redis |
| Arquitectura | DDD + Clean Architecture + vertical slices + CQRS con MediatR 12.x (última versión Apache-2.0); MVVM en la app |
| Hash de números | HMAC-SHA256 calculado **en el servidor**, clave secreta versionada fuera de la base de datos |
| Autenticación | Firebase Authentication: email/contraseña, Google, Apple (ver costo de Apple en §9) |
| Número del usuario | **No** se vincula a la cuenta |
| Subida de agenda | Opcional, con consentimiento explícito; la app funciona igual sin ella |
| Reporte de spam | Después de la llamada, no durante |
| Llamada bloqueada | Se rechaza y se notifica al usuario con los detalles |
| País inicial | Colombia (región por defecto `CO`, ley 1581 de 2012) |
| Licencia | AGPL-3.0 para todo el repositorio |
| Presupuesto | Mínimo: todo lo elegido tiene plan gratuito o cuesta pocos dólares al mes (§9) |
| Proceso | Sin pipeline de agentes; ramas + PR + CI |

---

## 2. Promesas de privacidad (lo que decimos al usuario)

Estas frases deben ser **verdaderas y verificables en el código**. Ninguna otra afirmación de privacidad se publica sin revisarla contra esta lista.

1. Nunca guardamos números de teléfono: solo un hash con clave secreta (HMAC-SHA256). Si alguien roba la base de datos, no obtiene números.
2. Tu agenda no se guarda como agenda: nadie puede ver "los contactos de una persona".
3. Un nombre solo se muestra si varias personas distintas lo guardaron igual (mínimo `K = 3`).
4. Nadie puede buscar a una persona por nombre; solo ves un nombre si ya tienes el número.
5. Los nombres se guardan cifrados con una clave derivada del número.
6. No guardamos quién consultó qué número (el historial de llamadas vive solo en tu teléfono).
7. Puedes retirar tu aporte y borrar tu cuenta cuando quieras; cualquier persona puede pedir que se oculte el nombre asociado a su número.

**Prohibido decir:** "es imposible compartir tu información", "nadie puede revertir el hash", "anónimo". Quien tiene la clave podría revertir hashes por fuerza bruta (el espacio de números es pequeño); por eso la clave se protege como el activo más crítico (§6).

---

## 3. Experiencia de llamada entrante

### 3.1 Estados del popup

El color **nunca** es la única señal: cada estado tiene ícono, título en texto y descripción para lectores de pantalla (TalkBack).

| Prioridad | Condición | Color | Ícono / título |
|---|---|---|---|
| 1 | Número bloqueado por mí | — | Se rechaza sin sonar; notificación (§3.3) |
| 2 | Está en mis contactos | 🟢 Verde | ✓ "Nombre del contacto" (+ aviso discreto si la comunidad lo marca como spam) |
| 3 | Spam según la comunidad | 🔴 Rojo | ⚠ "Posible spam · {etiqueta más común}" + N reportes |
| 4 | Identificado por la comunidad, no spam | 🔵 Azul | ⓘ "{nombre más común}" (deslizable a otros nombres) |
| 5 | Sin datos / sin conexión / número oculto | 🟠 Naranja | ? "Número desconocido" / "Sin conexión" / "Número privado" |

- "Mis contactos" se calcula **en el teléfono**; nunca requiere red.
- El nombre local del contacto gana siempre sobre el de la comunidad.
- Acciones en el popup: **Bloquear** (siempre), **Ver otros nombres** (deslizar, estados 3–4).

### 3.2 Restricciones de Android

- `CallScreeningService` (rol `ROLE_CALL_SCREENING`) decide permitir o rechazar; tiene ~5 s para responder.
- La consulta al servidor tiene **timeout de 2 s**; si vence → estado naranja "Sin conexión" y se reintenta en segundo plano para la notificación posterior.
- Caché local de resultados recientes (en el teléfono, cifrado, TTL 24 h) para responder al instante a números repetidos.
- El popup sobre la llamada requiere el permiso "Mostrar sobre otras apps" (`SYSTEM_ALERT_WINDOW`); si el usuario no lo concede, se usa una notificación de alta prioridad.
- Limitación conocida de v1: sin lista offline de spam (con HMAC en el servidor, el teléfono no puede calcular el hash). Se resuelve en el futuro con OPRF (§11).

### 3.3 Llamada bloqueada → notificación

"Bloqueamos una llamada de **+57 300 •••• 567** · {motivo: lo bloqueaste tú / spam automático / número oculto} · {etiqueta si existe} · {hora}". Acciones: **Desbloquear**, **Ver detalles**. El número completo se muestra solo dentro de la app.

### 3.4 Después de la llamada

Para llamadas de números que no están en mis contactos: notificación "¿Cómo fue esta llamada?" → **Es spam** (con etiqueta opcional, ej. "Spam Claro") · **No es spam** · **Sugerir nombre** · descartar.

---

## 4. Datos y algoritmos

### 4.1 Normalización y hash

- Normalización a E.164 con `libphonenumber-csharp`, región por defecto `CO`. Vive en **un solo lugar** (`Domain`), compartido por app y servidor; el servidor siempre re-normaliza.
- `PhoneHash = HMAC-SHA256(K_hash[v], E164)`, guardado junto con la versión `v` de la clave.
- Rotación sin conocer los números: `PhoneHash' = HMAC-SHA256(K_hash[v+1], PhoneHash)` (encadenado). Las consultas aplican la cadena completa.
- El número en texto plano solo existe en memoria durante la petición. Nunca en logs, excepciones, métricas, trazas ni respuestas de error.

### 4.2 Nombres

- Clave de cifrado por número: `K_name = HKDF(HMAC-SHA256(K_names, E164))`; cifrado AES-256-GCM.
- Para agrupar nombres iguales sin descifrar: `NameKey = HMAC(K_name, normalizar(nombre))` (minúsculas, sin tildes, espacios colapsados).
- **Filtro antes de subir (en el teléfono) y al recibir (en el servidor):** se descartan nombres de relación o personales ("mamá", "papá", "amor", "mi vida", "jefe", "ex", "vecina"…), nombres solo con emojis o símbolos, de 1 carácter, o que contengan números de teléfono, y groserías. La lista vive en un archivo de configuración versionado.
- **El número sí se sube aunque su nombre se descarte**, como señal de confianza sin nombre (§4.3).
- Se muestra el nombre con más aportantes distintos (con decaimiento temporal); los demás que superan `K = 3` se pueden ver deslizando, ordenados por aportantes.

### 4.3 Señales y puntaje de spam

| Señal | Origen | Efecto |
|---|---|---|
| `SavedByCount` | Aportantes distintos que tienen el número en su agenda (con o sin nombre) | Confianza: resta al puntaje de spam |
| Reportes de spam | Reporte post-llamada, 1 por usuario y número | Suma |
| Reportes "no es spam" | Reporte post-llamada | Resta |
| Bloqueos | Usuarios que bloquean el número | Suma (peso bajo) |
| Reputación del reportante | Antigüedad de la cuenta, Play Integrity, historial de reportes coincidentes con la comunidad | Multiplica el peso de sus votos |
| Decaimiento temporal | Vida media de 90 días | Los números se reasignan; votos viejos pesan menos |

Regla inicial (todos los umbrales son constantes con nombre, configurables y documentadas):

```
spam = (Σ pesoSpam ≥ 5) y (Σ pesoSpam ≥ 2 × (Σ pesoNoSpam + log2(1 + SavedByCount)))
```

Un número guardado como "Mamá" por muchas personas **no** es spam por defecto (su `SavedByCount` lo protege), pero reportes de spam suficientes y de cuentas confiables pueden superarlo.

### 4.4 Aporte de agenda

- Permiso de lectura de contactos (para el verde, local) y consentimiento de subida (para la comunidad) se piden **por separado**.
- Subida inicial por lotes y luego incremental (solo cambios); máximo 5 000 contactos por cuenta.
- Cada aporte guarda `ContributorId = HMAC(K_contrib, FirebaseUid)` para poder retirarlo, nunca el uid en claro y nunca junto a la agenda completa de forma consultable.
- Retirar el aporte o borrar la cuenta elimina todos sus aportes y recalcula los agregados.

---

## 5. Cuenta, gestión, apelación

### 5.1 Cuenta

- Registro e inicio de sesión con Firebase (email/contraseña con verificación de correo, Google, Apple).
- El servidor valida el JWT de Firebase en cada petición; el usuario se identifica por el `uid` del token, nunca por un valor enviado por el cliente.
- Borrar cuenta: borra la cuenta de Firebase, los aportes y los reportes (los agregados se recalculan).

### 5.2 Gestión (en la app)

- Lista de bloqueados (guardada en el teléfono; números en claro solo localmente).
- Bloqueo automático: spam de la comunidad, números ocultos, internacionales, por prefijo.
- Historial de llamadas identificadas y bloqueadas (solo local).
- Mis reportes y etiquetas (editar o retirar).
- Aporte de agenda: activar, desactivar, retirar todo.
- Privacidad: ver qué se envía, exportar mis datos, borrar cuenta.
- Apariencia: tema claro/oscuro/sistema.

### 5.3 Apelación y "ocultar mi número" (web pública, sin cuenta)

- Formulario web con captcha (Cloudflare Turnstile, gratis).
- **Verificación de propiedad** del número por SMS (Firebase Phone Auth); la cuenta temporal de Firebase se borra apenas termina la verificación, para no dejar el número guardado.
- Acciones: "Este número no es spam" (revisión de los reportes; se congela el estado rojo mientras se revisa) y "Ocultar los nombres asociados a mi número".
- Ocultar nombres **no** borra los reportes de spam; un spammer no puede limpiar su número así.
- Plazos legales en Colombia: consultas 10 días hábiles, reclamos 15 días hábiles (ver `docs/legal-colombia.md`).

---

## 6. Seguridad

- **Claves** (`K_hash`, `K_names`, `K_contrib`): nunca en el repo ni en la base de datos. En v1, secretos de Docker en el servidor, con permisos restringidos y copia de respaldo cifrada fuera del servidor. Rotación documentada (§4.1).
- **Rate limiting** (Redis, por `uid` y por IP):

  | Endpoint | Límite inicial |
  |---|---|
  | Consulta de número | 60/hora y 300/día por cuenta |
  | Reporte de spam | 20/día por cuenta |
  | Subida de agenda | 5 lotes/día, 5 000 contactos totales |
  | Formularios públicos | 5/hora por IP + captcha |
  | Global | 1 000/min por IP |

- **Anti-abuso:** Play Integrity API (gratis hasta 10 000 llamadas/día), cuentas nuevas con peso reducido, detección de ráfagas de reportes coordinados hacia un mismo número.
- **Anti-enumeración:** límites de consulta + `K = 3` + sin búsqueda por nombre.
- **Sin metadatos de llamadas:** las consultas no se persisten con el usuario; los logs de acceso no incluyen el cuerpo de la petición.
- HTTPS obligatorio; Cloudflare (gratis) delante del servidor.

---

## 7. Arquitectura

```
src/
  Domain/          Agregados, value objects (PhoneNumber, PhoneHash), reglas de puntaje y de nombres. Sin dependencias.
  Application/     Vertical slices: Features/<Feature>/ (comando o query, handler, validador, DTO). MediatR 12.x + FluentValidation.
  Infrastructure/  EF Core (PostgreSQL), Redis, Firebase Admin, hashing/cifrado, Play Integrity.
  Api/             Un endpoint minimal API por slice. Auth, rate limiting, ProblemDetails, OpenAPI.
  Contracts/       DTOs compartidos entre Api y App.
  App/             .NET MAUI Android. MVVM (CommunityToolkit.Mvvm), Refit, SQLite local.
tests/             Un proyecto por proyecto de src + tests de arquitectura (NetArchTest).
```

Dependencias: `Api → Application/Infrastructure → Domain`; `App → Contracts + Domain` (solo normalización). `Domain` y `Application` no conocen EF Core, ASP.NET Core ni Redis.

**Librerías:** MediatR 12.x, FluentValidation, EF Core + Npgsql, StackExchange.Redis, FirebaseAdmin, libphonenumber-csharp, Serilog (+ Seq en desarrollo), CommunityToolkit.Mvvm, CommunityToolkit.Maui, Refit, Microsoft.Extensions.Http.Resilience, sqlite-net-pcl.
**Tests:** xUnit, FluentAssertions **7.x** (última versión Apache-2.0; la 8 es comercial), NSubstitute, Testcontainers, NetArchTest, `Microsoft.AspNetCore.Mvc.Testing`.

---

## 8. Diseño visual y accesibilidad

- Material 3, tema claro y oscuro, colores de estado con contraste WCAG AA (4.5:1 para texto).
- Ícono + texto en cada estado (nunca solo color); soporte de TalkBack y de tamaño de fuente del sistema.
- Áreas táctiles de al menos 48 dp; popup legible de un vistazo (nombre ≥ 20 sp).
- Todo el texto en español (es-CO) desde el inicio, en archivos de recursos para traducir después.

---

## 9. Costos estimados

| Concepto | Costo |
|---|---|
| Servidor VPS (API + PostgreSQL + Redis en Docker Compose), ej. Hetzner CX22 | ~US$5/mes |
| Cloudflare (DNS, proxy, Turnstile) | Gratis |
| Copias de seguridad (Cloudflare R2, 10 GB) | Gratis |
| Firebase Authentication (email, Google) | Gratis a este tamaño |
| Verificación SMS para apelaciones | ~US$0.05 c/u, bajo volumen |
| Google Play (cuenta de desarrollador) | US$25 una vez |
| F-Droid | Gratis |
| Dominio | ~US$10/año |
| Seq (desarrollo / un usuario) | Gratis |
| **Sign in with Apple** | **Requiere Apple Developer Program: US$99/año** |

_Pendiente:_ ¿pagamos los US$99/año para Apple en v1, o lo dejamos para cuando exista la versión iOS? Recomendación: dejarlo para después; email y Google cubren casi todo el mercado Android en Colombia.

---

## 10. Pendientes

- Apple Sign-In en v1 (costo, §9).
- Revisión legal por un abogado antes del lanzamiento público (ver `docs/legal-colombia.md`).
- Nombre definitivo, logo y paleta de la marca.
- Instalar el SDK de .NET 10 en el entorno de desarrollo en la nube (script de inicio).

## 11. Futuro (fuera de v1)

- OPRF para que el servidor nunca vea los números y para habilitar una lista de spam offline.
- Versión iOS (Call Directory Extension).
- Builds reproducibles verificables y publicación en F-Droid.
- Más países.
