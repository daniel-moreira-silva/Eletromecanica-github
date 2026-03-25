import FetchService from "../fetch-service"

class UsuarioTelaService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async salvar(obj) {
    const route = `${this.endpoint}usuario-tela`
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true)
  }

  async atualizar(obj) {
    const route = `${this.endpoint}usuario-tela`
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true)
  }

  async listarTelasDoCadastrante(usuarioId, perfilId) {
    const route = `${this.endpoint}usuario-tela/telas?usuarioId=${usuarioId}&perfilId=${perfilId}`
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }

  async listarPermissoesUsuario(usuarioId, permissaoExtra) {
    const route = `${this.endpoint}usuario-tela/permissoes?usuarioId=${usuarioId}&permissaoExtra=${permissaoExtra}`
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }

  async excluir(usuarioTelaId) {
    const route = `${this.endpoint}usuario-tela?id=${usuarioTelaId}`
    return await this.fetchResponse(
      "DELETE",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }
}

export default UsuarioTelaService