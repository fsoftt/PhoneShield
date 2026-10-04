# Cómo funciona

## Cuando entra una llamada

Antes de que suene el teléfono, Tranqui revisa el número y muestra un aviso encima de la pantalla de llamada:

<div class="states">
  <div class="state-contact"><strong>✓ Verde</strong>El número está en tus contactos. Esto se decide en tu teléfono, sin consultar a nadie.</div>
  <div class="state-identified"><strong>ⓘ Azul</strong>La comunidad identifica el número, por ejemplo "Pizzería Juan".</div>
  <div class="state-spam"><strong>⚠ Rojo</strong>La comunidad lo reportó como spam, con su etiqueta, por ejemplo "Spam Claro".</div>
  <div class="state-unknown"><strong>? Naranja</strong>No hay datos del número, es un número privado o no hubo conexión.</div>
</div>

Si un número tiene varios nombres, el aviso muestra el más usado y puedes ver los demás con **Otro nombre**.

::: info Los nombres vienen de la comunidad
Un nombre en azul es como lo guardaron otras personas en su agenda (al menos 3 coinciden), no un dato verificado.
Puede no ser exacto: un número reasignado puede conservar por un tiempo el nombre de su dueño anterior.
:::

Si nada responde a tiempo, la llamada **siempre suena**: Tranqui nunca te hace perder una llamada por un error.
Si no hubo conexión, Tranqui sigue intentando durante un minuto después de la llamada y, si la comunidad conoce el
número, te avisa quién era.

## Bloquear

- **Desde el aviso**, con el botón **Bloquear**. Las próximas llamadas de ese número no sonarán.
- **Automáticamente**, si lo activas en Ajustes:
  - el spam reportado por la comunidad,
  - los números privados,
  - las llamadas internacionales,
  - los números que empiecen por un **prefijo** que elijas, por ejemplo 601 para los fijos de Bogotá o +1 para
    Norteamérica. Tus contactos siempre suenan.

Cada vez que bloqueamos una llamada te llega una notificación con el motivo y la hora. Tu lista de bloqueados se
guarda en tu teléfono. Si lo dejas activado en Ajustes, cada bloqueo también cuenta como una señal pequeña de spam
(se envía solo como código); puedes apagarlo cuando quieras y retiramos tus bloqueos del servidor.

## Después de la llamada

Tranqui te pregunta **"¿Cómo fue esta llamada?"** con tres opciones: **Es spam**, **Spam con etiqueta…** (la escribes
ahí mismo, por ejemplo "Spam Claro" o "Cobranzas") o **No es spam**. También puedes reportar desde el **Historial**.

Cada persona tiene un solo voto por número. En **Cuenta → Mis reportes** ves tus reportes y puedes cambiarlos o
retirarlos.

## Funciona sin conexión

Reportar, cambiar o retirar un reporte y bloquear funcionan aunque no tengas internet: se guardan en tu teléfono y se
envían solos apenas vuelve la conexión, aunque tengas la app cerrada. Saber quién llama sí necesita conexión; para los
números que te llamaron hace poco, Tranqui recuerda la respuesta 24 horas.

## Apariencia

Tema claro, oscuro o igual que el teléfono, en **Ajustes**.

## Cómo decide la comunidad

- Un número se marca como spam cuando suficientes personas lo reportan y esos reportes pesan claramente más que los votos de "no es spam".
- Un número guardado en la agenda de muchas personas es más difícil de marcar como spam, porque eso indica que es alguien conocido.
- Los votos pierden peso con el tiempo, porque los números se reasignan.
- Las cuentas creadas hace menos de 7 días votan con la mitad del peso, para frenar campañas con cuentas falsas.
- **Quien suele acertar pesa más.** Cada día comparamos los reportes de cada persona con lo que la comunidad ya
  decidió: quien coincide casi siempre vota con hasta 1,5 veces el peso normal, y quien casi nunca coincide, con un
  cuarto.
- **Ráfagas frenadas.** Los votos de un mismo día cuentan completos solo hasta cierto punto; por encima, valen mucho
  menos. Así, una campaña organizada contra (o a favor de) un número no lo marca de la noche a la mañana.
- **Bloquear también cuenta, pero poco.** Cuando bloqueas un número, eso suma una señal pequeña de spam (un cuarto de
  un reporte), porque también se bloquea a conocidos. Viene activado y la app te lo explica la primera vez que la abres;
  puedes apagarlo en Ajustes.
- Un nombre solo se muestra si **al menos 3 personas distintas** lo usaron.
- **Sin groserías ni nombres personales.** Los nombres ofensivos y los de relación ("Mamá", "Amor") nunca salen de tu
  teléfono: solo se aporta que tienes el número guardado. Si alguien reporta spam con una etiqueta ofensiva, el reporte
  cuenta pero la etiqueta se descarta.
- Puedes **retirar un reporte** cuando quieras.

## Lo que necesita en tu teléfono

| Permiso | Para qué |
|---|---|
| Identificar y filtrar llamadas | Revisar quién llama antes de que suene |
| Mostrar sobre otras apps | El aviso de color encima de la llamada |
| Contactos | Mostrar en verde a quien ya conoces. Se lee solo en tu teléfono |
| Notificaciones | Avisarte de llamadas bloqueadas y preguntarte si fue spam |

La app te explica cada permiso antes de pedirlo.
