import { createRouter, createWebHistory } from 'vue-router'
import CryptoJS from 'crypto-js'
import { inject } from 'vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/relatorios/Dashboard.vue')
  },
  {
    path: '/acesso-negado',
    name: 'AcessoNegado',
    component: () => import('@/views/AcessoNegado.vue')
  },
  {
    path: '/estacoes',
    name: 'Estacoes',
    component: () => import('@/views/configuracoes/Estacao.vue')
  },
  {
    path: '/equipamentos',
    name: 'Equipamentos',
    component: () => import('@/views/configuracoes/Equipamento.vue')
  },
  {
    path: '/equipamentos/:id',
    name: 'EquipamentoDetalhe',
    component: () => import('@/views/configuracoes/EquipamentoDetalhe.vue')
  },
  {
    path: '/servicos-solicitados',
    name: 'ServicosSolicitados',
    component: () => import('@/views/configuracoes/ServicoSolicitado.vue')
  },
  {
    path: '/servicos-executados',
    name: 'ServicosExecutados',
    component: () => import('@/views/configuracoes/ServicoExecutado.vue')
  },
  {
    path: '/ocorrencia-tabs',
    name: 'OcorrenciaTabs',
    component: () => import('@/views/OrdemServicoTabs.vue')
  },
  {
    path: '/nova-ocorrencia',
    name: 'NovaOcorrencia',
    component: () => import('@/views/NovaOrdemServico.vue')
  },
  {
    path: '/consulta-ordem-servico',
    name: 'ConsultaOrdemServico',
    component: () => import('@/views/ConsultaOrdemServico.vue')
  },
  {
    path: '/detalhar-ordem-servico/:id',
    name: 'DetalharOrdemServico',
    component: () => import('@/views/DetalharOrdemServico.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/relatorios/Dashboard.vue')
  },
  {
    path: '/operacao',
    name: 'Operacao',
    component: () => import('@/views/Operacao.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

router.beforeResolve((to, from, next) => {
  if (to.path === '/') return next();

  const encrypted = localStorage.getItem('loginNovoSanegeo');
  if (!encrypted) {
    localStorage.setItem('rotaNavegacao', to.fullPath);
    return next('/');
  } 
  
  try {
    const bytes = CryptoJS.AES.decrypt(localStorage.getItem('loginNovoSanegeo'), inject('chaveSeguranca'))
    const login = JSON.parse(bytes.toString(CryptoJS.enc.Utf8))
  
    // array de rotas permitidas
    let rotasLiberadas = ['/painel-manobras']
    login.data.usuario.modulos?.forEach(modulo => {
      //rotasLiberadas.push(modulo.urlControle) => vou controlar no componente e menu
      modulo.telas?.forEach(tela => rotasLiberadas.push(tela.url)) //Informações de acesso do backend
    })
  
    // Função de match que suporta rotas com parâmetros (:id) e wildcard (*)
    const matchesAllowed = (path, allowed) => {
      if (!Array.isArray(allowed)) return false;
      // 1) Bate exato primeiro
      if (allowed.includes(path)) return true;
      // 2) Converte padrões tipo "/perfil/:perfilId/telas" em regex: ^/perfil/[^/]+/telas$
      const toRegex = (pattern) => {
        if (!pattern || typeof pattern !== 'string') return null;
        // Escapa especiais, preservando ":" e "*"
        let s = pattern.replace(/[.+?^${}()|[\]\\]/g, '\\$&');
        // ":param" -> [^/]+
        s = s.replace(/:([^/]+)/g, '[^/]+');
        // "*" -> ".*" (se algum dia o backend enviar)
        s = s.replace(/\*/g, '.*');
        return new RegExp(`^${s}$`);
      };
      return allowed.some(p => {
        const rx = toRegex(p);
        return rx ? rx.test(path) : false;
      });
    };
  
    const podeAcessar = matchesAllowed(to.path, rotasLiberadas) || to.path === '/acesso-negado';
    next(podeAcessar ? undefined : '/acesso-negado');
  } catch {
    localStorage.removeItem('loginNovoSanegeo');
    return next('/');
  }
})

export default router