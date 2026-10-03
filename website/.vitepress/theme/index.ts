import { h, onMounted } from 'vue'
import DefaultTheme from 'vitepress/theme'
import { useData, withBase, type Theme } from 'vitepress'
import LanguageSwitch from './LanguageSwitch.vue'
import { browserLanguage, pathIn, savedChoice } from './languages'
import './custom.css'

export default {
  extends: DefaultTheme,
  Layout: () =>
    h(DefaultTheme.Layout, null, {
      'nav-bar-content-after': () => h(LanguageSwitch),
    }),
  setup() {
    const { page, localeIndex } = useData()

    // Open the site in the visitor's language: their explicit choice first, otherwise the browser's.
    onMounted(() => {
      const current = localeIndex.value === 'en' ? 'en' : 'es'
      const wanted = savedChoice() ?? browserLanguage()
      if (wanted !== current && !page.value.isNotFound) {
        window.location.replace(withBase(pathIn(wanted, page.value.relativePath)) + window.location.hash)
      }
    })
  },
} satisfies Theme
