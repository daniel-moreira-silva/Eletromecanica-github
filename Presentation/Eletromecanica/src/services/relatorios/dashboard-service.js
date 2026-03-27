// src/services/relatorios/dashboard-service.js
import FetchService from '../fetch-service'

class DashboardService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  _q(estacaoId) {
    return estacaoId ? `?estacaoId=${estacaoId}` : ''
  }

  async obterStatusOs(estacaoId = null) {
    const route = `${this.endpoint}dashboard/status-os${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterIndicadores(estacaoId = null) {
    const route = `${this.endpoint}dashboard/indicadores${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterDisponibilidade(estacaoId = null) {
    const route = `${this.endpoint}dashboard/disponibilidade${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterMotivacao(estacaoId = null) {
    const route = `${this.endpoint}dashboard/motivacao${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterCustos(estacaoId = null) {
    const route = `${this.endpoint}dashboard/custos${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterEstoque() {
    const route = `${this.endpoint}dashboard/estoque`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async obterOsAtrasadas(estacaoId = null) {
    const route = `${this.endpoint}dashboard/os-atrasadas${this._q(estacaoId)}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }
}

export default DashboardService