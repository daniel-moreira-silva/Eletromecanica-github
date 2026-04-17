import FetchService from "../fetch-service.js";

class CargoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async buscarTodos() {
    const route = `${this.endpoint}cargos`;
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }
}

export default CargoService;