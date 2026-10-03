# Política de tratamiento de datos personales

::: warning Borrador
Este documento es un borrador pendiente de revisión legal. Los datos resaltados se completarán antes del lanzamiento.
:::

**Versión:** 2026-10-03

Esta política cumple la Ley 1581 de 2012 y el Decreto 1377 de 2013 (compilado en el Decreto 1074 de 2015) de Colombia.

## 1. Responsable del tratamiento

- **Nombre:** <span class="placeholder">[NOMBRE DEL RESPONSABLE]</span>
- **Identificación:** <span class="placeholder">[CÉDULA O NIT]</span>
- **Domicilio:** <span class="placeholder">[CIUDAD Y DIRECCIÓN]</span>
- **Correo para consultas y reclamos:** <span class="placeholder">[CORREO DE CONTACTO]</span>

## 2. Datos que tratamos y para qué

| Dato | Cómo lo guardamos | Finalidad |
|---|---|---|
| Correo electrónico y contraseña | Los administra Firebase Authentication (Google); nosotros solo guardamos un identificador de la cuenta | Crear y proteger tu cuenta |
| Aceptación de términos y del aporte de agenda | Fecha y versión del texto aceptado | Demostrar tu autorización |
| Números de teléfono consultados o reportados | Solo como código (hash) con clave secreta; nunca el número | Identificar llamadas y detectar spam |
| Nombres y etiquetas de spam | Cifrados con una clave derivada de cada número | Mostrar quién llama |
| Agenda de contactos (solo si decides aportarla) | Números como código y nombres cifrados; los nombres personales ("Mamá") se descartan | Identificar llamadas para la comunidad |
| Apelaciones por un número ([cómo](/guia/apelacion)) | El número solo como código; Firebase (Google) lo procesa para enviar el código por SMS y borramos esa verificación al confirmarla; un identificador del teléfono (ANDROID_ID) y la cuenta, cada uno solo como código y por separado; el motivo y el correo opcional se borran al resolver la solicitud | Verificar que el número es tuyo, atender la solicitud y limitar el uso por número, cuenta y teléfono |

**No** usamos los datos para publicidad, **no** los vendemos y **no** creamos perfiles.

## 3. Datos de personas sin cuenta

Cuando un usuario aporta su agenda, se tratan datos de terceros (números y nombres). Para reducir el impacto sobre ellos:

- Nunca se guardan números de teléfono, solo códigos.
- Un nombre solo se muestra si al menos 3 personas distintas lo guardaron igual.
- No existe búsqueda por nombre: solo se ve un nombre si ya se tiene el número.
- Cualquier persona puede pedir que se oculten los nombres de su número, sin tener cuenta ([cómo hacerlo](/guia/apelacion)).

## 4. Derechos del titular

Puedes conocer, actualizar, rectificar y suprimir tus datos, solicitar prueba de la autorización, ser informado del uso
de tus datos, revocar la autorización y presentar quejas ante la Superintendencia de Industria y Comercio.

Desde la app puedes ver qué datos guardamos, dejar de aportar tu agenda y borrar tu cuenta.

## 5. Consultas y reclamos

Escribe a <span class="placeholder">[CORREO DE CONTACTO]</span>.

- **Consultas:** respondemos dentro de los 10 días hábiles siguientes.
- **Reclamos:** respondemos dentro de los 15 días hábiles siguientes.

## 6. Seguridad

Usamos conexiones cifradas (HTTPS), códigos con clave secreta para los números, cifrado de nombres, límites de consultas
por usuario contra la extracción masiva de datos, y registros (logs) que no contienen números ni nombres.

## 7. Transferencia internacional

La autenticación la presta Firebase (Google) y los servidores pueden estar fuera de Colombia, en países con un nivel
adecuado de protección de datos. <span class="placeholder">[UBICACIÓN DE LOS SERVIDORES]</span>

## 8. Vigencia

Esta política rige desde su publicación. Si cambia, publicaremos la nueva versión aquí y te pediremos aceptarla en la app.
