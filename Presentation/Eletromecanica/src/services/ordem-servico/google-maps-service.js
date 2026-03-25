import FetchService from "../fetch-service.js";

class GoogleMapsService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async buscarEndereco(lat, long) {
    const route = `${this.endpoint}google-maps/buscar-endereco?latLong=${lat},${long}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async buscarEnderecoTextoLivre(textoLivre, cidadeCliente, estadoCliente) {
    const route = `${this.endpoint}google-maps/buscar-endereco?textoLivre=${textoLivre}, ${cidadeCliente} - ${estadoCliente}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }
}

export default GoogleMapsService;
