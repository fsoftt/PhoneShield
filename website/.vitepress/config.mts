import { defineConfig, type DefaultTheme } from 'vitepress'

const repo = 'https://github.com/fsoftt/Tranqui'

function spanish(): DefaultTheme.Config {
  return {
    nav: [
      { text: 'Cómo funciona', link: '/guia/como-funciona' },
      { text: 'Privacidad', link: '/privacidad/' },
      { text: 'Legal', link: '/legal/politica-de-tratamiento' },
      { text: 'Cómo está hecho', link: '/proyecto/arquitectura' },
    ],
    sidebar: [
      {
        text: 'Para usuarios',
        items: [
          { text: 'Cómo funciona', link: '/guia/como-funciona' },
          { text: 'Cómo protegemos tus datos', link: '/privacidad/' },
          { text: '¿Tu número aparece mal?', link: '/guia/apelacion' },
          { text: 'Preguntas frecuentes', link: '/guia/preguntas-frecuentes' },
        ],
      },
      {
        text: 'Legal',
        items: [
          { text: 'Política de tratamiento de datos', link: '/legal/politica-de-tratamiento' },
          { text: 'Términos y condiciones', link: '/legal/terminos' },
        ],
      },
      {
        text: 'El proyecto',
        items: [
          { text: 'Cómo está hecho', link: '/proyecto/arquitectura' },
          { text: 'Código fuente', link: repo },
        ],
      },
    ],
    editLink: { pattern: `${repo}/edit/main/website/:path`, text: 'Editar esta página en GitHub' },
    outline: { level: [2, 3], label: 'En esta página' },
    docFooter: { prev: 'Anterior', next: 'Siguiente' },
    lastUpdated: { text: 'Actualizado' },
    darkModeSwitchLabel: 'Apariencia',
    returnToTopLabel: 'Volver arriba',
    sidebarMenuLabel: 'Menú',
    langMenuLabel: 'Cambiar idioma',
    footer: {
      message: 'Código abierto bajo la licencia AGPL-3.0.',
      copyright: 'Tranqui: identifica llamadas sin exponer tus contactos',
    },
  }
}

function english(): DefaultTheme.Config {
  return {
    nav: [
      { text: 'How it works', link: '/en/guia/como-funciona' },
      { text: 'Privacy', link: '/en/privacidad/' },
      { text: 'Legal', link: '/en/legal/politica-de-tratamiento' },
      { text: 'How it is built', link: '/en/proyecto/arquitectura' },
    ],
    sidebar: [
      {
        text: 'For users',
        items: [
          { text: 'How it works', link: '/en/guia/como-funciona' },
          { text: 'How we protect your data', link: '/en/privacidad/' },
          { text: 'Is your number shown wrongly?', link: '/en/guia/apelacion' },
          { text: 'FAQ', link: '/en/guia/preguntas-frecuentes' },
        ],
      },
      {
        text: 'Legal',
        items: [
          { text: 'Data processing policy', link: '/en/legal/politica-de-tratamiento' },
          { text: 'Terms and conditions', link: '/en/legal/terminos' },
        ],
      },
      {
        text: 'The project',
        items: [
          { text: 'How it is built', link: '/en/proyecto/arquitectura' },
          { text: 'Source code', link: repo },
        ],
      },
    ],
    editLink: { pattern: `${repo}/edit/main/website/:path`, text: 'Edit this page on GitHub' },
    outline: { level: [2, 3], label: 'On this page' },
    footer: {
      message: 'Open source under the AGPL-3.0 license.',
      copyright: 'Tranqui: know who is calling without exposing your contacts',
    },
  }
}

export default defineConfig({
  title: 'Tranqui',
  base: '/Tranqui/',
  cleanUrls: true,
  lastUpdated: true,
  head: [['link', { rel: 'icon', type: 'image/svg+xml', href: '/Tranqui/favicon.svg' }]],

  locales: {
    root: {
      label: 'Español',
      lang: 'es-CO',
      description: 'Identifica llamadas y bloquea spam sin exponer tus contactos. Código abierto y pensado para la privacidad.',
      themeConfig: spanish(),
    },
    en: {
      label: 'English',
      lang: 'en',
      link: '/en/',
      description: 'Know who is calling and block spam without exposing your contacts. Open source and privacy first.',
      themeConfig: english(),
    },
  },

  themeConfig: {
    logo: '/favicon.svg',
    socialLinks: [{ icon: 'github', link: repo }],
    search: {
      provider: 'local',
      options: {
        locales: {
          root: {
            translations: {
              button: { buttonText: 'Buscar', buttonAriaLabel: 'Buscar' },
              modal: {
                noResultsText: 'No hay resultados para',
                resetButtonTitle: 'Borrar búsqueda',
                footer: { selectText: 'elegir', navigateText: 'navegar', closeText: 'cerrar' },
              },
            },
          },
        },
      },
    },
  },
})
