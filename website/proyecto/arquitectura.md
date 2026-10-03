# Cómo está hecho

Tranqui es de código abierto: puedes revisar que hace lo que dice en [GitHub](https://github.com/fsoftt/Tranqui).

## Tecnologías

<div class="stack-grid">
  <div><strong>App</strong>.NET MAUI para Android, MVVM con CommunityToolkit</div>
  <div><strong>Backend</strong>.NET 10, ASP.NET Core minimal APIs, MediatR 12, FluentValidation</div>
  <div><strong>Datos</strong>PostgreSQL 17 con EF Core 10</div>
  <div><strong>Cuentas</strong>Firebase Authentication, tokens validados en el servidor</div>
  <div><strong>Criptografía</strong>HMAC-SHA256, AES-256-GCM, HKDF</div>
  <div><strong>Pruebas</strong>xUnit v3, Testcontainers, tests de arquitectura</div>
  <div><strong>Despliegue</strong>Docker Compose y Caddy con HTTPS automático</div>
  <div><strong>CI</strong>GitHub Actions: backend, imagen Docker y app Android</div>
</div>

## Arquitectura

- **Domain-Driven Design:** el dominio (números, reputación, cuentas) no depende de nada externo.
- **Clean Architecture con vertical slices:** cada funcionalidad tiene su comando o consulta, su validador y su handler juntos.
- **Tests de arquitectura** que fallan si una capa depende de otra que no debe.
- **App.Core:** toda la lógica de la app (decidir qué hacer con una llamada, la sesión, los ViewModels) es .NET puro y se prueba sin emulador.

## Privacidad desde el diseño

- El número se normaliza (formato internacional E.164) y se convierte en código **en un solo lugar** del código.
- Los números viajan en el cuerpo de las peticiones, nunca en la URL, para que no queden en registros de acceso.
- Los logs no incluyen números, nombres ni correos.
- Las claves secretas nunca están en el repositorio ni en la base de datos, y se pueden rotar.

## Licencia

[AGPL-3.0](https://github.com/fsoftt/Tranqui/blob/main/LICENSE): cualquiera puede usar, estudiar y modificar el código,
y quien ofrezca una versión modificada como servicio debe publicar sus cambios.
