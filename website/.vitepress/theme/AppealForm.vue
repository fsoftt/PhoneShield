<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import { useData, withBase } from 'vitepress'
import type { Auth, ConfirmationResult, RecaptchaVerifier } from 'firebase/auth'

// Public values, set at build time (see README). Without them the page shows the email fallback.
const config = {
  apiUrl: import.meta.env.VITE_TRANQUI_API_URL as string | undefined,
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY as string | undefined,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN as string | undefined,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID as string | undefined,
}
const configured = Boolean(config.apiUrl && config.apiKey && config.authDomain && config.projectId)

type Kind = 'HideNames' | 'ReviewSpam'
type Step = 'details' | 'code' | 'done'

const texts = {
  es: {
    number: 'Tu número',
    numberHint: 'Con indicativo si no es de Colombia. Ej.: 300 123 4567 o +57 300 123 4567',
    kind: '¿Qué necesitas?',
    hideNames: 'Ocultar los nombres asociados a mi número',
    reviewSpam: 'Mi número no es spam: revisarlo',
    reason: 'Cuéntanos por qué (opcional)',
    email: 'Correo para responderte (opcional)',
    emailHint: 'Solo lo usamos para responder esta solicitud y lo borramos al resolverla.',
    consent: 'Autorizo el uso de mi número para atender esta solicitud, según la',
    policy: 'política de tratamiento de datos',
    sendCode: 'Enviarme el código por SMS',
    smsNote: 'Te enviamos un solo SMS al mes por número. Si no te llega, espera al mes siguiente o escríbenos.',
    code: 'Código de 6 dígitos que te llegó por SMS',
    codeSentTo: 'Enviamos el código a',
    submit: 'Enviar solicitud',
    working: 'Un momento…',
    applied: 'Listo. Ya no mostramos nombres para tu número. Si estaba marcado como spam, eso no cambia.',
    pending: 'Recibimos tu solicitud. Una persona la revisará dentro de los plazos de ley; si dejaste un correo, te responderemos ahí.',
    notConfigured: 'El formulario aún no está activo. Mientras tanto, escríbenos al correo de contacto de esta página.',
    errors: {
      invalidNumber: 'Revisa el número: no parece válido.',
      limit: 'Este número ya usó su SMS o su solicitud de este mes. Inténtalo el mes siguiente o escríbenos.',
      tooMany: 'Demasiados intentos desde tu conexión. Inténtalo más tarde.',
      badCode: 'El código no es correcto. Revísalo e inténtalo de nuevo.',
      expiredCode: 'El código venció. Escríbenos para terminar tu solicitud.',
      invalidEmail: 'Revisa el correo: no parece válido.',
      unexpected: 'Algo falló. Inténtalo de nuevo en unos minutos.',
      connection: 'No pudimos conectarnos. Revisa tu conexión e inténtalo de nuevo.',
    },
  },
  en: {
    number: 'Your number',
    numberHint: 'Include the country code if it is not from Colombia. E.g. +57 300 123 4567',
    kind: 'What do you need?',
    hideNames: 'Hide the names linked to my number',
    reviewSpam: 'My number is not spam: review it',
    reason: 'Tell us why (optional)',
    email: 'Email to answer you (optional)',
    emailHint: 'We only use it to answer this request and delete it once resolved.',
    consent: 'I authorize the use of my number to handle this request, under the',
    policy: 'data processing policy',
    sendCode: 'Send me the code by SMS',
    smsNote: 'We send one SMS per number per month. If it does not arrive, wait until next month or write to us.',
    code: '6-digit code you got by SMS',
    codeSentTo: 'We sent the code to',
    submit: 'Send request',
    working: 'One moment…',
    applied: 'Done. We no longer show names for your number. If it was marked as spam, that does not change.',
    pending: 'We got your request. A person will review it within the legal deadlines; if you left an email, we will answer there.',
    notConfigured: 'The form is not active yet. Meanwhile, write to the contact email on this page.',
    errors: {
      invalidNumber: 'Check the number: it does not look valid.',
      limit: 'This number already used its SMS or its request this month. Try next month or write to us.',
      tooMany: 'Too many attempts from your connection. Try again later.',
      badCode: 'The code is not right. Check it and try again.',
      expiredCode: 'The code expired. Write to us to finish your request.',
      invalidEmail: 'Check the email: it does not look valid.',
      unexpected: 'Something went wrong. Try again in a few minutes.',
      connection: 'We could not connect. Check your connection and try again.',
    },
  },
} as const

const { localeIndex } = useData()
const lang = computed(() => (localeIndex.value === 'en' ? 'en' : 'es'))
const t = computed(() => texts[lang.value])
const policyLink = computed(() =>
  withBase(lang.value === 'en' ? '/en/legal/politica-de-tratamiento' : '/legal/politica-de-tratamiento'))

const step = ref<Step>('details')
const busy = ref(false)
const error = ref('')
const phone = ref('')
const kind = ref<Kind>('HideNames')
const reason = ref('')
const email = ref('')
const consent = ref(false)
const code = ref('')
const verifiedNumber = ref('')
const status = ref<'Applied' | 'Pending'>('Applied')

let auth: Auth | undefined
let recaptcha: RecaptchaVerifier | undefined
let confirmation: ConfirmationResult | undefined

class ApiError extends Error {
  constructor(readonly status: number, readonly code: string | undefined) {
    super(code ?? String(status))
  }
}

async function post<T>(path: string, body: unknown, token?: string): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(new URL(path, config.apiUrl), { method: 'POST', headers, body: JSON.stringify(body) })
  if (!response.ok) {
    const problem = await response.json().catch(() => ({}))
    throw new ApiError(response.status, problem.code)
  }

  return response.json() as Promise<T>
}

function messageFor(failure: unknown): string {
  const errors = t.value.errors
  if (failure instanceof ApiError) {
    if (failure.code === 'appeal_limit_reached') return errors.limit
    if (failure.code === 'rate_limited') return errors.tooMany
    if (failure.code === 'validation_failed') return step.value === 'details' ? errors.invalidNumber : errors.invalidEmail
    return errors.unexpected
  }

  switch ((failure as { code?: string })?.code) {
    case 'auth/invalid-phone-number': return errors.invalidNumber
    case 'auth/invalid-verification-code': return errors.badCode
    case 'auth/code-expired': return errors.expiredCode
    case 'auth/too-many-requests':
    case 'auth/quota-exceeded': return errors.tooMany
    case 'auth/network-request-failed': return errors.connection
  }

  return failure instanceof TypeError ? errors.connection : errors.unexpected
}

async function firebaseAuth() {
  // Loaded on demand: visitors who never open the form never download Firebase.
  const [{ initializeApp }, firebaseAuthModule] = await Promise.all([import('firebase/app'), import('firebase/auth')])
  if (!auth) {
    const app = initializeApp({ apiKey: config.apiKey, authDomain: config.authDomain, projectId: config.projectId })
    auth = firebaseAuthModule.getAuth(app)
  }
  auth.languageCode = lang.value

  return { auth, module: firebaseAuthModule }
}

async function sendCode() {
  busy.value = true
  error.value = ''
  try {
    const { auth, module } = await firebaseAuth()
    recaptcha ??= new module.RecaptchaVerifier(auth, 'appeal-send-code', { size: 'invisible' })

    // Our API allows one SMS per number per month and returns the normalized number to verify.
    const { phoneNumber } = await post<{ phoneNumber: string }>('/v1/appeals/verification-requests', { phoneNumber: phone.value })
    verifiedNumber.value = phoneNumber
    confirmation = await module.signInWithPhoneNumber(auth, phoneNumber, recaptcha)
    step.value = 'code'
  } catch (failure) {
    error.value = messageFor(failure)
  } finally {
    busy.value = false
  }
}

async function submit() {
  if (!confirmation) return

  busy.value = true
  error.value = ''
  try {
    const { user } = await confirmation.confirm(code.value.trim())
    try {
      const token = await user.getIdToken()
      const result = await post<{ status: 'Applied' | 'Pending' }>('/v1/appeals', {
        kind: kind.value,
        reason: reason.value.trim() || null,
        contactEmail: kind.value === 'ReviewSpam' ? email.value.trim() || null : null,
      }, token)
      status.value = result.status
      step.value = 'done'
    } finally {
      // The phone sign-in exists only to prove ownership: remove it from Firebase right away.
      await user.delete().catch(() => auth?.signOut())
    }
  } catch (failure) {
    error.value = messageFor(failure)
  } finally {
    busy.value = false
  }
}

onBeforeUnmount(() => recaptcha?.clear())
</script>

<template>
  <div class="appeal-form">
    <p v-if="!configured" class="appeal-note">{{ t.notConfigured }}</p>

    <form v-else-if="step === 'details'" @submit.prevent="sendCode">
      <label for="appeal-phone">{{ t.number }}</label>
      <input id="appeal-phone" v-model="phone" type="tel" autocomplete="tel" required aria-describedby="appeal-phone-hint" />
      <small id="appeal-phone-hint">{{ t.numberHint }}</small>

      <fieldset>
        <legend>{{ t.kind }}</legend>
        <label class="appeal-choice"><input v-model="kind" type="radio" value="HideNames" /> {{ t.hideNames }}</label>
        <label class="appeal-choice"><input v-model="kind" type="radio" value="ReviewSpam" /> {{ t.reviewSpam }}</label>
      </fieldset>

      <label for="appeal-reason">{{ t.reason }}</label>
      <textarea id="appeal-reason" v-model="reason" maxlength="500" rows="3" />

      <template v-if="kind === 'ReviewSpam'">
        <label for="appeal-email">{{ t.email }}</label>
        <input id="appeal-email" v-model="email" type="email" autocomplete="email" maxlength="254" aria-describedby="appeal-email-hint" />
        <small id="appeal-email-hint">{{ t.emailHint }}</small>
      </template>

      <label class="appeal-choice">
        <input v-model="consent" type="checkbox" required />
        <span>{{ t.consent }} <a :href="policyLink">{{ t.policy }}</a>.</span>
      </label>

      <button id="appeal-send-code" type="submit" :disabled="busy || !consent">{{ busy ? t.working : t.sendCode }}</button>
      <small>{{ t.smsNote }}</small>
    </form>

    <form v-else-if="step === 'code'" @submit.prevent="submit">
      <p>{{ t.codeSentTo }} <strong>{{ verifiedNumber }}</strong>.</p>
      <label for="appeal-code">{{ t.code }}</label>
      <input id="appeal-code" v-model="code" inputmode="numeric" autocomplete="one-time-code" pattern="[0-9]{6}" maxlength="6" required />
      <button type="submit" :disabled="busy">{{ busy ? t.working : t.submit }}</button>
    </form>

    <p v-else class="appeal-success" role="status">{{ status === 'Applied' ? t.applied : t.pending }}</p>

    <p v-if="error" class="appeal-error" role="alert">{{ error }}</p>
  </div>
</template>

<style scoped>
.appeal-form {
  margin: 16px 0 24px;
  padding: 20px;
  border: 1px solid var(--vp-c-divider);
  border-radius: 12px;
  background: var(--vp-c-bg-soft);
}

form {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

label {
  font-weight: 500;
}

input[type='tel'],
input[type='email'],
input:not([type]),
textarea {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  background: var(--vp-c-bg);
  font: inherit;
}

input:focus-visible,
textarea:focus-visible,
button:focus-visible {
  outline: 2px solid var(--vp-c-brand-1);
  outline-offset: 2px;
}

fieldset {
  margin: 8px 0;
  padding: 0;
  border: 0;
}

legend {
  font-weight: 500;
  margin-bottom: 4px;
}

.appeal-choice {
  display: flex;
  gap: 8px;
  align-items: flex-start;
  font-weight: 400;
}

.appeal-choice input {
  margin-top: 5px;
}

small {
  color: var(--vp-c-text-2);
}

button {
  align-self: flex-start;
  margin-top: 8px;
  padding: 10px 20px;
  border-radius: 20px;
  background: var(--vp-c-brand-1);
  color: var(--vp-c-white);
  font-weight: 600;
}

.dark button {
  color: #0b1f33;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.appeal-error {
  color: #c0262d;
  font-weight: 500;
}

.dark .appeal-error {
  color: #f28b8f;
}

.appeal-success {
  margin: 0;
  font-weight: 500;
}
</style>
