// src/composables/usePermissoesTela.js
import { computed, inject } from 'vue';
import { useRoute } from 'vue-router';
import CryptoJS from 'crypto-js';

/**
 * Composable para verificar permissões da tela atual,
 * baseado nos dados de login armazenados no localStorage.
 */
export function usePermissoesTela() {
  const route = useRoute();
  const chaveSeguranca = inject('chaveSeguranca');

  // Recupera e decodifica dados do usuário logado
  const loginData = computed(() => {
    const loginEncrypted = localStorage.getItem('loginNovoSanegeo');
    if (!loginEncrypted) return null;
    const bytes = CryptoJS.AES.decrypt(loginEncrypted, chaveSeguranca);
    const login = JSON.parse(bytes.toString(CryptoJS.enc.Utf8));
    return login.data.usuario;
  });

  // Função para comparar rota atual com padrão da tela (suporta :param)
  function matchRoute(urlPattern, currentPath) {
    let pattern = urlPattern.replace(/[.+?^${}()|[\]\\]/g, '\\$&');
    pattern = pattern.replace(/:([^/]+)/g, '[^/]+');
    const regex = new RegExp(`^${pattern}$`);
    return regex.test(currentPath);
  }

  // Encontra o objeto da tela baseado na rota
  const currentTela = computed(() => {
    const usuario = loginData.value;
    if (!usuario?.modulos) return null;
    for (const modulo of usuario.modulos) {
      if (!modulo.telas) continue;
      const found = modulo.telas.find(tela => matchRoute(tela.url, route.path));
      if (found) return found;
    }
    return null;
  });

  // Extrai array de permissões disponíveis para a tela
  const permissoes = computed(() => {
    const tela = currentTela.value;
    const origem = tela?.permissoesUsuario?.[0]?.permissaoDisponivel || tela?.permissaoDisponivel || '';
    return origem.replace(/\[|\]/g, '').split(',').map(p => p.trim());
  });

  // Verifica se a ação está permitida
  function hasPermission(action) {
    return permissoes.value.includes(action);
  }

  return { hasPermission, permissoes, currentTela };
}
