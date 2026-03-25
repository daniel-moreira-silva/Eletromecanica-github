// SVG core + estilos mínimos do FontAwesome
import { config, library } from '@fortawesome/fontawesome-svg-core'
import '@fortawesome/fontawesome-svg-core/styles.css'

// Impede injeção automática de CSS duplicado
config.autoAddCss = false

import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import * as fas from '@fortawesome/free-solid-svg-icons'
import * as fab from '@fortawesome/free-brands-svg-icons'
import * as far from '@fortawesome/free-regular-svg-icons'

// Seleciona todos os ícones sólidos (prefix 'fas') e de marca (prefix 'fab')
const solidIcons = Object.values(fas).filter(icon => icon.prefix === 'fas')
const brandIcons = Object.values(fab).filter(icon => icon.prefix === 'fab')
const regularIcons = Object.values(far).filter(i => i.prefix === 'far')

// Adiciona todos os ícones à biblioteca do FontAwesome
library.add(...solidIcons, ...brandIcons, ...regularIcons)

/**
 * Registra globalmente o componente <FontAwesomeIcon> no Vue
 * @param {import('vue').App} app
 */
export function registerFontAwesome(app) {
  app.component('FontAwesomeIcon', FontAwesomeIcon)
}

// Exporta o componente para uso pontual, se necessário
export default FontAwesomeIcon
