import FetchService from "../fetch-service.js";

class EstacaoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async salvar(obj) {
    const route = `${this.endpoint}estacoes`
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}estacoes`
    return await this.fetchResponse("PUT", this.headerPadrao, obj, false, route, true);
  }

  async atualizarStatus(id, ativo) {
    const route = `${this.endpoint}estacoes/status?id=${id}&ativo=${ativo}`
    return await this.fetchResponse("PATCH", this.headerPadrao, null, false, route, true);
  }

  async selecionar(id) {
    const route = `${this.endpoint}estacoes/${id}`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async listar(obj) {
    const route = `${this.endpoint}estacoes/lista`
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true
    );
  }

  async listarTiposEstacao() {
    const route = `${this.endpoint}estacoes/tiposestacao`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async buscarEstacoes() {
    const route = `${this.endpoint}estacoes`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }
}

export default EstacaoService;