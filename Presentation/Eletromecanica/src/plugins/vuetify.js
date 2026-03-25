// src/plugins/vuetify.js
import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import { aliases, fa } from 'vuetify/iconsets/fa-svg'
import { mdi } from 'vuetify/iconsets/mdi'
import clientTheme from '@/themes/default.json'

// Lê o modo inicial (light/dark) do localStorage
function getInitialTheme() {
  return localStorage.getItem('themeMode') || 'light'
}

/**
 * Cria instância do Vuetify com tema dinâmico (light/dark).
 * @param {'light'|'dark'} mode - Tema inicial ('light' ou 'dark').
 * @returns {import('vuetify').Vuetify} Configuração do Vuetify.
 */

export function createThemedVuetify(mode = getInitialTheme()) {
  return createVuetify({
    theme: {
      // Tema padrão usado na aplicação
      defaultTheme: mode,
      // Definição das paletas de cores para light e dark
      options: { customProperties: true },
      themes: {
        light: clientTheme.light,
        dark: clientTheme.dark,
      },
    },
    icons: {
      // Usa FontAwesome como conjunto de ícones padrão
      defaultSet: 'fa',
      aliases,
      sets: { fa, mdi },
    },
    defaults: {
      // Defaults de componentes para alinhar ao design system
      VBtn: {
        elevation: 1,
        rounded: 'md',
        style: {
          fontSize: '13px',
          fontWeight: '500'
        }
      },
      VTextField: {
        variant: 'solo-filled',
        density: 'comfortable',
        style: {
          '--v-field-label-font-size': '13px',
          '--v-field-label-font-weight': '500',
        }
      },
      VCard: {
        rounded: 'lg',
      },
      VCardTitle: {
        style: {
          fontSize: '16px',
          fontWeight: '500'
        }
      },
      VCheckbox: {
        color: 'primary',
        density: 'compact',
        style: {
          fontSize: '13px',
          fontWeight: '500'
        }
      },

      VTab: {
        style: {
          fontSize: '13px',
          fontWeight: '500'
        }
      }
    },
  })
}

// Exporta a instância-padrão já inicializada com o mode correto
const vuetify = createThemedVuetify()
export default vuetify
