<script setup lang="ts">
import { computed } from 'vue'
import { useData, withBase } from 'vitepress'

// Same page in the other language: English pages mirror the Spanish paths under /en/.
const { page, localeIndex } = useData()

const isEnglish = computed(() => localeIndex.value === 'en')
const label = computed(() => (isEnglish.value ? 'Español' : 'English'))
const lang = computed(() => (isEnglish.value ? 'es' : 'en'))
const target = computed(() => {
  const path = page.value.relativePath.replace(/(^|\/)index\.md$/, '$1').replace(/\.md$/, '')
  const spanishPath = isEnglish.value ? path.replace(/^en\//, '') : path
  return withBase(isEnglish.value ? `/${spanishPath}` : `/en/${spanishPath}`)
})
</script>

<template>
  <a class="language-switch" :href="target" :hreflang="lang" :lang="lang">
    <span aria-hidden="true">🌐</span> {{ label }}
  </a>
</template>

<style scoped>
.language-switch {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-left: 12px;
  padding: 4px 12px;
  border: 1px solid var(--vp-c-brand-1);
  border-radius: 20px;
  color: var(--vp-c-brand-1);
  font-size: 14px;
  font-weight: 500;
  white-space: nowrap;
  transition: background-color 0.2s, color 0.2s;
}

.language-switch:hover {
  background-color: var(--vp-c-brand-1);
  color: var(--vp-c-white);
}
</style>
