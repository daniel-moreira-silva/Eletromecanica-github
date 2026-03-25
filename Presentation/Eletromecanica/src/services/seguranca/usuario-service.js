import FetchService from '../fetch-service'

class UsuarioService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async login(obj) {
    const route = `${this.endpoint}usuario/login`
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      false
    )
  }

  async salvar(obj) {
    const route = `${this.endpoint}usuario`
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true)
  }

  async atualizar(obj) {
    const route = `${this.endpoint}usuario`
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true)
  }

  async atualizarStatus(usuarioId, ativo) {
    const route = `${this.endpoint}usuario/status?id=${usuarioId}&ativo=${ativo}`
    return await this.fetchResponse(
      "PATCH",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }

  async listar(filtro) {
    const route = `${this.endpoint}usuario/lista`
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      filtro,
      false,
      route,
      true)
  }

  async alterarSenha(usuarioId, senha, repetirSenha) {
    const route = `${this.endpoint}usuario/senha`
      + `?id=${encodeURIComponent(usuarioId)}`
      + `&senha=${encodeURIComponent(senha)}`
      + `&repetirSenha=${encodeURIComponent(repetirSenha)}`
    return await this.fetchResponse(
      "PATCH",
      this.headerPadrao,
      null,
      false,
      route,
      true)
  }
}

export default UsuarioService