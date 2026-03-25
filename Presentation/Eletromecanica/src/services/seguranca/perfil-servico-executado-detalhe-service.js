import FetchService from "../fetch-service.js";

class PerfilServicoExecutadoDetalheService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async salvar(obj) {
    const route = `${this.endpoint}perfil-servico-executado-detalhe`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}perfil-servico-executado-detalhe`;
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async selecionar(perfilId, servicoExecutadoDetalheId) {
    const route = `${this.endpoint}perfil-servico-executado-detalhe/id?perfilId=${perfilId}&servicoExecutadoDetalheId=${servicoExecutadoDetalheId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async excluir(perfilId, servicoExecutadoDetalheId) {
    const route = `${this.endpoint}perfil-servico-executado-detalhe?perfilId=${perfilId}&servicoExecutadoDetalheId=${servicoExecutadoDetalheId}`;
    return await this.fetchResponse(
      "DELETE",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async listarPorPerfil(perfilId) {
    const route = `${this.endpoint}perfil-servico-executado-detalhe/servicos?perfilId=${perfilId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }
}

export default PerfilServicoExecutadoDetalheService;