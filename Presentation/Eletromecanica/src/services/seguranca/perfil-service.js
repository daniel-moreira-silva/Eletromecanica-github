import FetchService from "../fetch-service.js";

class PerfilService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async salvar(obj) {
    const route = `${this.endpoint}perfil`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}perfil`;
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true);
  }

  async atualizarStatus(id, ativo) {
    const route = `${this.endpoint}perfil/status?id=${id}&ativo=${ativo}`;
    return await this.fetchResponse(
      "PATCH",
      this.headerPadrao,
      null,
      false,
      route,
      true);
  }

  async listar(filtros) {
    const route = `${this.endpoint}perfil/lista`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      filtros,
      false,
      route,
      true);
  }

  async descricoes(listaCompleta = false) {
    const route = `${this.endpoint}perfil/descricoes?listaCompleta=${!!listaCompleta}`
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }
}
export default PerfilService;
