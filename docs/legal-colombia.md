# Marco legal en Colombia — notas para Tranqui

> **Esto no es asesoría legal.** Es un mapa de lo que hay que resolver y una propuesta de cómo hacerlo. Antes del lanzamiento público, un abogado debe revisarlo. Opciones de bajo costo: consultorios jurídicos de universidades (gratuitos) y las guías publicadas por la SIC.

## Normas aplicables

- **Constitución, art. 15** — derecho de habeas data.
- **Ley 1581 de 2012** — régimen general de protección de datos personales.
- **Decreto 1377 de 2013** (compilado en el **Decreto 1074 de 2015**) — reglamenta la autorización, la política de tratamiento y las transferencias.
- **Autoridad:** Superintendencia de Industria y Comercio (SIC), Delegatura de Protección de Datos Personales.

Un hash con clave secreta es **seudonimización, no anonimización**: quien tiene la clave puede re-identificar. Para la ley, los hashes de números y los nombres siguen siendo datos personales. El hash reduce el riesgo, no elimina las obligaciones.

## Obligaciones y cómo las cumplimos

| Obligación | Cómo la cumplimos |
|---|---|
| **Autorización previa, expresa e informada** del titular | Pantalla de consentimiento en el registro y otra, separada, para subir la agenda. Guardamos fecha, versión del texto aceptado y `uid`. |
| **Política de tratamiento de la información** publicada | Página web pública + enlace en la app: responsable, finalidades, derechos, canal de atención, plazos. |
| **Finalidad limitada** | Solo identificar llamadas y detectar spam. Sin publicidad, sin venta de datos, sin perfiles. |
| **Derechos del titular** (conocer, actualizar, rectificar, suprimir, revocar) | Exportar datos, retirar aporte y borrar cuenta desde la app; formulario web para personas sin cuenta. |
| **Plazos de respuesta** | Consultas: 10 días hábiles. Reclamos: 15 días hábiles. Los automatizamos para responder mucho antes. |
| **Seguridad** | HMAC, cifrado de nombres, claves fuera de la BD, límites de consulta, sin PII en logs. |
| **Registro Nacional de Bases de Datos (RNBD)** | Obligatorio para sociedades con activos > 100 000 UVT (Decreto 090 de 2018). Un proyecto personal o una empresa pequeña probablemente no está obligado; confirmarlo con el abogado si se constituye empresa. |
| **Transferencia internacional** (servidores o Firebase fuera de Colombia) | Informarlo en la política y en el consentimiento. Elegir países que la SIC considere con nivel adecuado (Circular Externa 005 de 2017 y sus actualizaciones). Confirmarlo con el abogado. |

## El riesgo principal: terceros que no dieron autorización

Cuando un usuario sube su agenda, sube nombres y números de personas que **nunca** aceptaron nada. La ley exige autorización del titular, y sus excepciones (art. 10: datos públicos, urgencia médica, fines históricos o estadísticos, orden de autoridad) no cubren claramente este caso. La excepción de bases "personales o domésticas" (art. 2) protege la agenda en el teléfono, no a la copia en nuestro servidor.

No hay una solución perfecta: es un problema de toda la categoría de apps de identificación de llamadas. Lo que reduce el riesgo:

1. **Minimización:** no guardamos números en claro ni agendas completas; solo agregados con hash.
2. **Umbral `K = 3`:** un nombre solo aparece si al menos tres personas lo guardaron igual, lo que lo acerca a "cómo se conoce públicamente a este número" y no al dato privado de un usuario.
3. **Sin búsqueda por nombre** ni directorio: solo se ve un nombre si ya se tiene el número.
4. **Filtro de nombres personales** ("mamá", "amor"…): nunca se suben.
5. **Exclusión fácil para no usuarios:** formulario web gratuito, sin cuenta, para ocultar los nombres asociados a su número, atendido de forma automática.
6. **Transparencia:** código abierto (AGPL) y política de privacidad en lenguaje claro.
7. **Revisión legal** específica de este punto antes del lanzamiento.

## Otros puntos

- **Etiquetas de spam que nombran empresas** ("Spam Claro"): riesgo de reclamos por buen nombre. Mitigación: umbral `K`, filtro de groserías, y apelación con verificación de propiedad del número.
- **Términos y condiciones:** prohibir reportes falsos, uso para acoso y extracción masiva de datos.
- **Menores de edad:** la app requiere 18 años o autorización de los representantes legales.

## Documentos por producir antes del lanzamiento

- [ ] Política de tratamiento de la información.
- [ ] Términos y condiciones.
- [ ] Textos de consentimiento (registro y subida de agenda), versionados.
- [ ] Procedimiento interno para consultas y reclamos.
- [ ] Revisión por un abogado.
