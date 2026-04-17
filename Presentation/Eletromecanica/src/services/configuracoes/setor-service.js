import FetchService from "../fetch-service.js";

class SetorService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async buscarTodos() {
    const route = `${this.endpoint}setores`;
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }
}

export default SetorService;