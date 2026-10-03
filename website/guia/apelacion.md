# ¿Tu número aparece mal?

No necesitas tener cuenta ni instalar la app. Solo tienes que demostrar que el número es tuyo con un código que te
enviamos por SMS.

## Qué puedes pedir

- **Ocultar los nombres asociados a tu número.** Se aplica de inmediato. Ocultar nombres **no** cambia si un número
  está marcado como spam, para que nadie pueda usarlo para limpiar un número de spam.
- **"Mi número no es spam".** Una persona revisa cada solicitud y responde dentro de los plazos de la ley colombiana
  (consultas en 10 días hábiles y reclamos en 15 días hábiles).

## Solicitud

<AppealForm />

Cada número recibe **un solo SMS y puede hacer una sola solicitud al mes**. Así mantenemos el servicio gratuito y
evitamos que alguien use el formulario para enviar mensajes a números ajenos.

Si no te llega el SMS o prefieres no usarlo, escribe a <span class="placeholder">[CORREO DE CONTACTO]</span>.

## Qué hacemos con tus datos

- El número no se guarda: igual que en el resto de Tranqui, solo guardamos su hash con clave secreta.
- El código lo envía Firebase (Google), que procesa el número para mandar el SMS. Apenas confirmas el código, borramos
  la sesión que Firebase crea para verificarlo.
- Si dejas un correo, lo usamos solo para responder y lo borramos al resolver la solicitud.
