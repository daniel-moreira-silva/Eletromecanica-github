import FetchService from "../fetch-service.js";

class PerfilTelaService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async salvar(obj) {
    const route = `${this.endpoint}perfil-tela`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}perfil-tela`;
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async selecionar(perfilId, telaId) {
    const route = `${this.endpoint}perfil-tela/id?perfilId=${perfilId}&telaId=${telaId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async excluir(perfilId, telaId) {
    const route = `${this.endpoint}perfil-tela?perfilId=${perfilId}&telaId=${telaId}`;
    return await this.fetchResponse(
      "DELETE",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }

  async listarPorPerfil(perfilId) {
    const route = `${this.endpoint}perfil-tela/telas?perfilId=${perfilId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }
}

export default PerfilTelaService;