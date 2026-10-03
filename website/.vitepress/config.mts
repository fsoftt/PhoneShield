import { defineConfig } from 'vitepress'

const repo = 'https://github.com/fsoftt/Tranqui'

export default defineConfig({
  lang: 'es-CO',
  title: 'Tranqui',
  description: 'Identifica llamadas y bloquea spam sin exponer tus contactos. Código abierto y pensado para la privacidad.',
  base: '/Tranqui/',
  cleanUrls: true,
  lastUpdated: true,
  head: [
    ['link', { rel: 'icon', type: 'image/svg+xml', href: '/Tranqui/favicon.svg' }],
    ['meta', { property: 'og:title', content: 'Tranqui: identifica llamadas sin exponer tus contactos' }],
    ['meta', { property: 'og:description', content: 'Identificador de llamadas y bloqueador de spam de código abierto, orientado a la privacidad.' }],
  ],

  themeConfig: {
    logo: '/favicon.svg',
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
    socialLinks: [{ icon: 'github', link: repo }],
    editLink: { pattern: `${repo}/edit/main/website/:path`, text: 'Editar esta página en GitHub' },
    search: {
      provider: 'local',
      options: {
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
    outline: { level: [2, 3], label: 'En esta página' },
    docFooter: { prev: 'Anterior', next: 'Siguiente' },
    lastUpdated: { text: 'Actualizado' },
    darkModeSwitchLabel: 'Apariencia',
    returnToTopLabel: 'Volver arriba',
    sidebarMenuLabel: 'Menú',
    footer: {
      message: 'Código abierto bajo la licencia AGPL-3.0.',
      copyright: 'Tranqui: identifica llamadas sin exponer tus contactos',
    },
  },
})
