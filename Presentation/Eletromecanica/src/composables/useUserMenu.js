import { computed, inject } from 'vue'
import CryptoJS from 'crypto-js'

export function useUserMenu() {
  const chave = inject('chaveSeguranca')
  let userData = null
  const encrypted = localStorage.getItem('loginNovoSanegeo')
  if (encrypted && chave) {
    const bytes = CryptoJS.AES.decrypt(encrypted, chave)
    userData = JSON.parse(bytes.toString(CryptoJS.enc.Utf8)).data.usuario
  }

  const iconMap = {
    'Atendimento / Solicitações': 'fas fa-headset',
    'Gestão de serviços e Equipes': 'fas fa-person-digging',
    'Módulos Complementares': 'fas fa-screwdriver-wrench',
    'Relatórios': 'fas fa-magnifying-glass-chart',
    'Configurações': 'fas fa-cogs',
    'Segurança': 'fas fa-shield-halved',
    'Manobras e Avisos': 'fas fa-grip'
  }

  const filteredMenu = computed(() => {
    if (!userData?.modulos) return []
    return userData.modulos
      .filter(m => m.ativo)
      .sort((a, b) => a.ordenacao - b.ordenacao)
      .map(m => ({
        title: m.descricao,
        // adiciona a URL do módulo para navegação direta
        value: m.urlControle,
        icon: iconMap[m.descricao] || 'fas fa-folder',
        children: m.telas
          .filter(t => t.ativo && t.permissoesUsuario?.length && t.exibirMenu)
          .sort((a, b) => a.ordenacao - b.ordenacao)
          .map(t => ({ title: t.descricao, value: t.url }))
      }))
  })
  return { filteredMenu }
}
