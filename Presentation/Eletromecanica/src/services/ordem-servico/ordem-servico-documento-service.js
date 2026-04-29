import FetchService from '../fetch-service.js'

class OrdemServicoDocumentoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async salvar(obj) {
    const route = `${this.endpoint}ordem-servico-documento`
    return await this.fetchResponse('POST', this.headerPadrao, obj, false, route, true)
  }

  async atualizar(obj) {
    const route = `${this.endpoint}ordem-servico-documento`
    return await this.fetchResponse('PUT', this.headerPadrao, obj, false, route, true)
  }

  async listar(ordemServicoId) {
    const route = `${this.endpoint}ordem-servico-documento/lista?ordemServicoId=${ordemServicoId}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async excluir(id) {
    const route = `${this.endpoint}ordem-servico-documento?id=${id}`
    return await this.fetchResponse('DELETE', this.headerPadrao, null, false, route, true)
  }

  async atualizarOrdem(id, ordem) {
    const route = `${this.endpoint}ordem-servico-documento/ordem?id=${id}&ordem=${ordem}`
    return await this.fetchResponse('PATCH', this.headerPadrao, null, false, route, true)
  }
}

export default OrdemServicoDocumentoService
