import FetchService from "../fetch-service.js";

class ClienteService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async buscarEnderecos(busca) {
    const route = `${this.endpoint}cliente/buscar-enderecos?busca=${busca}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }
}

export default ClienteService;
