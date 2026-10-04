# Despliegue de la API

Un VPS pequeño (por ejemplo Hetzner CX22, ~US$5/mes) con Docker corre todo: la API, el back office, PostgreSQL y Caddy, que obtiene
y renueva el certificado HTTPS automáticamente (Let's Encrypt, gratis).

## Primera vez

1. **Servidor:** Ubuntu 24.04, con Docker instalado (`curl -fsSL https://get.docker.com | sh`). Abre solo los puertos 22, 80 y 443.
2. **Dominio:** crea dos registros DNS `A` que apunten a la IP del servidor: uno para el API (por ejemplo
   `api.tudominio.com`) y otro para el back office (por ejemplo `admin.tudominio.com`).
   Con Cloudflare (gratis) puedes además ocultar la IP y frenar ataques; usa el modo SSL "Full (strict)".
3. **Código:** `git clone https://github.com/fsoftt/Tranqui.git && cd Tranqui/deploy`
4. **Secretos:** `cp .env.example .env` y completa cada valor. Genera cada clave con `openssl rand -base64 32`.
5. **Arranque:** `docker compose up -d --build`. La API aplica las migraciones de la base de datos al arrancar.
6. **Verifica:** `curl https://api.tudominio.com/health` debe responder `Healthy`.

## Las claves son lo más importante

`PHONE_HASHING_KEY_1`, `NAME_PROTECTION_KEY` y `CONTRIBUTOR_IDS_KEY` protegen toda la base de datos.

- **Si se pierden**, los datos guardados quedan inservibles (no hay forma de recuperarlos).
- **Si se filtran**, alguien con la base de datos podría revertir los hashes por fuerza bruta.

Guarda una copia cifrada fuera del servidor (por ejemplo en un gestor de contraseñas) y nunca las subas al repositorio.

## Actualizar

```bash
cd Tranqui && git pull && cd deploy && docker compose up -d --build
```

## Copias de seguridad

Respaldo diario de PostgreSQL (programa este comando con `cron`):

```bash
docker compose exec -T postgres pg_dump -U tranqui tranqui | gzip > backup-$(date +%F).sql.gz
```

Súbelo cifrado a un almacenamiento externo (por ejemplo Cloudflare R2, 10 GB gratis). La base de datos solo contiene
hashes y nombres cifrados, pero trata los respaldos como datos sensibles igualmente.
