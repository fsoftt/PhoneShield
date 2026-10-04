# Cómo protegemos tus datos

Aquí explicamos qué hacemos con los datos, sin letra pequeña. Si algo de lo que decimos aquí no coincide con el código, el código (que es público) manda, y queremos saberlo.

## Lo que prometemos

1. **Nunca guardamos números de teléfono.** Guardamos un código calculado a partir del número con una clave secreta (HMAC-SHA256). Si alguien robara la base de datos, no obtendría números.
2. **Tu agenda no se guarda como agenda.** Nadie puede ver "los contactos de una persona".
3. **Un nombre solo se muestra si al menos 3 personas distintas lo guardaron igual.**
4. **Nadie puede buscar a una persona por su nombre.** Solo ves un nombre si ya tienes el número, porque te llamó.
5. **Los nombres se guardan cifrados** con una clave que depende de cada número.
6. **No guardamos quién consultó qué número.** El historial de llamadas vive solo en tu teléfono.
7. **Puedes retirar tu aporte y borrar tu cuenta cuando quieras.** Cualquier persona puede pedir que se oculte el nombre asociado a su número.

## Lo que no prometemos (y por qué)

No decimos que sea "imposible" saber un número a partir de su código. Hay pocos números de teléfono posibles, así que quien tenga la **clave secreta** podría calcular los códigos de todos y compararlos. Por eso la clave se guarda fuera de la base de datos y se protege como lo más importante del sistema.

## Tu cuenta

- Te registras con correo y contraseña (y pronto con Google) a través de Firebase Authentication.
- **Tu número de teléfono no se vincula a tu cuenta.**
- Guardamos la fecha en que aceptaste los términos y qué versión aceptaste, como exige la ley.

## Aportar tu agenda

Es **opcional**; la app funciona igual sin aportar.

Si decides aportar:

- La app envía los números y nombres por una conexión cifrada.
- El servidor convierte cada número en su código y cifra cada nombre. Nunca guarda el número.
- Los nombres que describen una relación ("Mamá", "Mi amor", "Jefe") **no se usan**; solo cuenta que alguien tiene ese número guardado.
- Cada aporte se registra con un identificador que no está ligado directamente a tu cuenta.
- Puedes **dejar de aportar** en Ajustes: se borra todo lo que aportaste.

## Lo que se queda en tu teléfono

- Tu lista de números bloqueados (si compartes tus bloqueos como señal de spam, el servidor recibe solo el código de
  cada número; puedes apagarlo en Ajustes).
- Tus ajustes de bloqueo y los prefijos que bloqueas.
- **Mis reportes**: la lista de lo que reportaste, para que puedas cambiarlo o retirarlo. El servidor no puede
  listarlos: solo tiene códigos.
- Lo pendiente de enviar mientras no hay conexión.
- El historial de llamadas (30 días, máximo 200).
- Una caché de consultas recientes (24 horas), para identificar más rápido a quien vuelve a llamar.

## Tus derechos

Puedes conocer, actualizar, rectificar y suprimir tus datos, y revocar tu autorización. Desde la app:

- **Cuenta → Ver qué datos guardamos.**
- **Cuenta → Borrar mi cuenta**: borra tu cuenta, tus reportes y tus aportes. Los bloqueos que compartiste se conservan
  como señal de spam sin ningún vínculo contigo, salvo que marques **Borrar también los bloqueos que compartí**.

Si no tienes cuenta y tu número aparece mal identificado, mira [¿Tu número aparece mal?](/guia/apelacion).

Los detalles legales están en la [Política de tratamiento de datos](/legal/politica-de-tratamiento).
