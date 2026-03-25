import FetchService from "../fetch-service.js";

class PerfilServicoSolicitadoDetalheService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async salvar(obj) {
    const route = `${this.endpoint}perfil-servico-solicitado-detalhe`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}perfil-servico-solicitado-detalhe`;
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async selecionar(perfilId, servicoSolicitadoDetalheId) {
    const route = `${this.endpoint}perfil-servico-solicitado-detalhe/id?perfilId=${perfilId}&servicoSolicitadoDetalheId=${servicoSolicitadoDetalheId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async excluir(perfilId, servicoSolicitadoDetalheId) {
    const route = `${this.endpoint}perfil-servico-solicitado-detalhe?perfilId=${perfilId}&servicoSolicitadoDetalheId=${servicoSolicitadoDetalheId}`;
    return await this.fetchResponse(
      "DELETE",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async listarPorPerfil(perfilId) {
    const route = `${this.endpoint}perfil-servico-solicitado-detalhe/servicos?perfilId=${perfilId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }
}

export default PerfilServicoSolicitadoDetalheService;