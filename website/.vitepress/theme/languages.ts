export type Language = 'es' | 'en'

const choiceKey = 'tranqui.language'

/** Same page in the other language: English pages mirror the Spanish paths under /en/. */
export function pathIn(language: Language, relativePath: string): string {
  const page = relativePath.replace(/(^|\/)index\.md$/, '$1').replace(/\.md$/, '').replace(/^en\//, '')
  return language === 'en' ? `/en/${page}` : `/${page}`
}

/** Spanish for Spanish browsers, English for everyone else. */
export function browserLanguage(): Language {
  const languages = navigator.languages?.length ? navigator.languages : [navigator.language]
  return languages.some((language) => language?.toLowerCase().startsWith('es')) ? 'es' : 'en'
}

/** The language the visitor picked with the switch, if any. Storage can be unavailable (private mode). */
export function savedChoice(): Language | null {
  try {
    const value = localStorage.getItem(choiceKey)
    return value === 'es' || value === 'en' ? value : null
  } catch {
    return null
  }
}

export function saveChoice(language: Language): void {
  try {
    localStorage.setItem(choiceKey, language)
  } catch {
    // Without storage the browser language keeps deciding; nothing else to do.
  }
}
