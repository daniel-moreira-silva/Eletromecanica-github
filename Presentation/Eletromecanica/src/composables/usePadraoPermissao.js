// src/composables/useCustomButtonsPermission.js
import { computed, unref } from 'vue';
import { usePermissoesTela } from '@/composables/usePermissoesTela';

// Mapeamento padrão entre função de botão e permissão
const actionMap = {
  inserir: 'Criar',
  editar: 'Editar',
  excluir: 'Excluir',
  exportar: 'Exportar',
  ler: 'Ler',
  acoes: 'Ação'
};

/**
 * Filtra um array de botões customizados conforme permissões do usuário.
 * @param {Array<Object> | Ref<Array<Object>>} rawButtonsList - Definição original dos botões (function, icon, etc.)
 * @returns {import('vue').ComputedRef<Array<Object>>}
 */
export function usePadraoPermissao(rawButtonsList) {
  const { hasPermission } = usePermissoesTela();

  const customButtons = computed(() => {
    // unwrap ref or use array directly
    const list = unref(rawButtonsList) || [];
    return list.filter(btn => {
      const perm = actionMap[btn.function];
      // Se não há mapeamento, mantém botão sempre visível
      if (!perm) return true;
      return hasPermission(perm);
    });
  });

  return customButtons;
}
