import FetchService from "../fetch-service.js";

class FuncionarioService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async salvar(obj) {
    const route = `${this.endpoint}funcionarios`;
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}funcionarios`;
    return await this.fetchResponse("PUT", this.headerPadrao, obj, false, route, true);
  }

  async atualizarStatus(id, ativo) {
    const route = `${this.endpoint}funcionarios/status?id=${id}&ativo=${ativo}`;
    return await this.fetchResponse("PATCH", this.headerPadrao, null, false, route, true);
  }

  async selecionar(id) {
    const route = `${this.endpoint}funcionarios/${id}`;
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async listar(obj) {
    const route = `${this.endpoint}funcionarios/lista`;
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async buscarTodos() {
    const route = `${this.endpoint}funcionarios`;
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }
}

export default FuncionarioService;