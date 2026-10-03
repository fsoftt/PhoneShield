---
layout: home

hero:
  name: Tranqui
  text: Contesta tranquilo
  tagline: Identifica quién llama y bloquea el spam sin exponer tus contactos. Código abierto, pensado para la privacidad y hecho en Colombia.
  image:
    src: /favicon.svg
    alt: Tranqui
  actions:
    - theme: brand
      text: Cómo funciona
      link: /guia/como-funciona
    - theme: alt
      text: Cómo protegemos tus datos
      link: /privacidad/
    - theme: alt
      text: Ver el código
      link: https://github.com/fsoftt/Tranqui

features:
  - icon: 🎨
    title: Un aviso de color en cada llamada
    details: Verde si está en tus contactos, azul si la comunidad lo identifica, rojo si es spam y naranja si no hay datos. Siempre con ícono y texto, no solo color.
    link: /guia/como-funciona
  - icon: 🔒
    title: Nunca guardamos números de teléfono
    details: Solo un código (hash) calculado con una clave secreta. Si alguien robara la base de datos, no obtendría números.
    link: /privacidad/
  - icon: 🙈
    title: Nombres protegidos
    details: Los nombres se guardan cifrados y solo se muestran si varias personas distintas los usaron. Nombres como "Mamá" o "Amor" nunca se usan.
    link: /privacidad/
  - icon: 🚫
    title: Tú decides qué bloquear
    details: Bloquea números, el spam reportado por la comunidad, los números privados o las llamadas internacionales. Te avisamos cada vez que bloqueamos una llamada.
    link: /guia/como-funciona
  - icon: 🤝
    title: Aportar es opcional
    details: Puedes aportar tu agenda para ayudar a otros, o no hacerlo. La app funciona igual, y puedes retirar tu aporte cuando quieras.
    link: /privacidad/#aportar-tu-agenda
  - icon: 🧾
    title: Código abierto
    details: Todo el código es público bajo la licencia AGPL-3.0, así que cualquiera puede revisar que hacemos lo que decimos.
    link: /proyecto/arquitectura
---

<div class="vp-doc" style="max-width: 1152px; margin: 64px auto 0; padding: 0 24px;">

## Así se ve una llamada

<div class="states">
  <div class="state-contact"><strong>✓ Mamá</strong>Está en tus contactos.</div>
  <div class="state-identified"><strong>ⓘ Pizzería Juan</strong>Así lo conoce la comunidad.</div>
  <div class="state-spam"><strong>⚠ Spam Claro</strong>Reportado por 23 personas.</div>
  <div class="state-unknown"><strong>? +57 •••••••567</strong>No tenemos datos de este número.</div>
</div>

Tranqui está en desarrollo. La primera versión será para Android y se lanzará primero en Colombia.

</div>
