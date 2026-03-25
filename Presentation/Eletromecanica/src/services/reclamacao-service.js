// src/services/categoria-servico-service.js
import FetchService from "./fetch-service.js";

class ReclamacaoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }
  // POST /reclamacao/lista
  async listar(filtros) {
    const route = `${this.endpoint}reclamacao/lista`;
    return await this.fetchResponse("POST", this.headerPadrao, filtros, false, route, true);
  }
}

export default ReclamacaoService;
