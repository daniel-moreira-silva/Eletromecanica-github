import FetchService from "../fetch-service.js";

class PerfilHorarioAcessoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca);
  }

  async listar(filtro) {
    const route = `${this.endpoint}perfil-horario-acesso/lista?perfilId=${filtro.perfilId}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true
    );
  }

  async selecionar(id) {
    const route = `${this.endpoint}perfil-horario-acesso/id?id=${id}`;
    return await this.fetchResponse(
      "GET",
      this.headerPadrao,
      null,
      false,
      route,
      true
    );
  }

  async salvar(obj) {
    const route = `${this.endpoint}perfil-horario-acesso`;
    return await this.fetchResponse(
      "POST",
      this.headerPadrao,
      obj,
      false,
      route,
      true
    );
  }

  async atualizar(obj) {
    const route = `${this.endpoint}perfil-horario-acesso`;
    return await this.fetchResponse(
      "PUT",
      this.headerPadrao,
      obj,
      false,
      route,
      true
    );
  }

  async excluir(perfilHorarioAcessoId) {
    const route = `${this.endpoint}perfil-horario-acesso?id=${perfilHorarioAcessoId}`;
    return await this.fetchResponse(
      "DELETE",
      this.headerPadrao,
      null,
      false,
      route,
      true
    );
  }
}

export default PerfilHorarioAcessoService;