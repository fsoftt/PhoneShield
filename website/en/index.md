---
layout: home

hero:
  name: Tranqui
  text: Answer calmly
  tagline: Know who is calling and block spam without exposing your contacts. Open source, privacy first, made in Colombia.
  image:
    src: /favicon.svg
    alt: Tranqui
  actions:
    - theme: brand
      text: How it works
      link: /en/guia/como-funciona
    - theme: alt
      text: How we protect your data
      link: /en/privacidad/
    - theme: alt
      text: View the code
      link: https://github.com/fsoftt/Tranqui

features:
  - icon: 🎨
    title: A colored card on every call
    details: Green if the caller is in your contacts, blue if the community knows them, red if it is spam, orange if there is no data. Always with an icon and text, never color alone.
    link: /en/guia/como-funciona
  - icon: 🔒
    title: We never store phone numbers
    details: Only a code (hash) computed with a secret key. Someone who stole the database would not get any numbers.
    link: /en/privacidad/
  - icon: 🙈
    title: Protected names
    details: Names are stored encrypted and only shown when several different people used them. Names like "Mom" or "Love" are never used.
    link: /en/privacidad/
  - icon: 🚫
    title: You decide what to block
    details: Block numbers, spam reported by the community, private numbers or international calls. We notify you every time we block a call.
    link: /en/guia/como-funciona
  - icon: 🤝
    title: Contributing is optional
    details: You can contribute your address book to help others, or not. The app works the same, and you can withdraw your contribution any time.
    link: /en/privacidad/#contributing-your-address-book
  - icon: 🧾
    title: Open source
    details: All the code is public under the AGPL-3.0 license, so anyone can check that we do what we say.
    link: /en/proyecto/arquitectura
---

<div class="vp-doc" style="max-width: 1152px; margin: 64px auto 0; padding: 0 24px;">

## What a call looks like

<div class="states">
  <div class="state-contact"><strong>✓ Mom</strong>In your contacts.</div>
  <div class="state-identified"><strong>ⓘ Pizzería Juan</strong>How the community knows them.</div>
  <div class="state-spam"><strong>⚠ Spam Claro</strong>Reported by 23 people.</div>
  <div class="state-unknown"><strong>? +57 •••••••567</strong>We have no data about this number.</div>
</div>

Tranqui is in development. The first version will be for Android and will launch in Colombia first.

</div>
