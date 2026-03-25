import FetchService from "../fetch-service.js";

class EquipamentoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async salvar(obj) {
    const route = `${this.endpoint}equipamentos`
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async atualizar(obj) {
    const route = `${this.endpoint}equipamentos`
    return await this.fetchResponse("PUT", this.headerPadrao, obj, false, route, true);
  }

  async atualizarStatus(id, ativo) {
    const route = `${this.endpoint}equipamentos/status?id=${id}&ativo=${ativo}`
    return await this.fetchResponse("PATCH", this.headerPadrao, null, false, route, true);
  }

  async selecionar(id) {
    const route = `${this.endpoint}equipamentos/${id}`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async listar(obj) {
    const route = `${this.endpoint}equipamentos/lista`
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async listarTiposEquipamento() {
    const route = `${this.endpoint}equipamentos/tiposequipamento`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async listarEquipamentosPorEstacao(estacaoId, principal) {
    let route = `${this.endpoint}equipamentos/estacoes?estacaoId=${estacaoId}`
    
    if (principal !== null && principal !== undefined)
      route += `&principal=${principal}`

    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async selecionarCompleto(id) {
    const route = `${this.endpoint}equipamentos/completo/${id}`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async listarRegrasPreventivas(equipamentoId) {
    const route = `${this.endpoint}equipamentos/regras-preventivas?equipamentoId=${equipamentoId}`
    return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
  }

  async salvarRegraPreventiva(obj) {
    const route = `${this.endpoint}equipamentos/regras-preventivas`
    return await this.fetchResponse("POST", this.headerPadrao, obj, false, route, true);
  }

  async atualizarRegraPreventiva(obj) {
    const route = `${this.endpoint}equipamentos/regras-preventivas`
    return await this.fetchResponse("PUT", this.headerPadrao, obj, false, route, true);
  }

  async deletarRegraPreventiva(id) {
    const route = `${this.endpoint}equipamentos/regras-preventivas/${id}`
    return await this.fetchResponse("DELETE", this.headerPadrao, null, false, route, true);
  }
}

export default EquipamentoService;