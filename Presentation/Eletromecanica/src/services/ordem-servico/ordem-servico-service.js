import FetchService from '../fetch-service'

class OrdemServicoService extends FetchService {

  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async enviarWhatsApp(obj) {
    const route = `${this.endpoint}twilio-whatsapp`
    return await this.fetchResponse('POST', this.headerPadrao, obj, false, route, true)
  }

  async buscarPorEndereco(busca) {
    const route = `${this.endpoint}ordem-servico/buscar-por-endereco?busca=${busca}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async buscarOrdensServicoProximas(lat, lon) {
    const route = `${this.endpoint}ordem-servico/buscar-orderns-servico-proximas?lat=${lat}&lon=${lon}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async criarOrdemServico(obj) {
    const route = `${this.endpoint}ordem-servico`
    return await this.fetchResponse('POST', this.headerPadrao, obj, false, route, true)
  }

  async listar(filtros) {
    const route = `${this.endpoint}ordem-servico/lista`
    return await this.fetchResponse('POST', this.headerPadrao, filtros, false, route, true)
  }

  async listarCount(filtros) {
    const route = `${this.endpoint}ordem-servico/lista-count`
    return await this.fetchResponse('POST', this.headerPadrao, filtros, false, route, true)
  }

  async obterDetalhes(id) {
    const route = `${this.endpoint}ordem-servico?id=${id}`
    return await this.fetchResponse('GET', this.headerPadrao, null, false, route, true)
  }

  async iniciarOrdemServico(obj) {
    const route = `${this.endpoint}ordem-servico/iniciar`
    return await this.fetchResponse('PATCH', this.headerPadrao, obj, false, route, true)
  }

  async devolverOrdemServico(obj) {
    const route = `${this.endpoint}ordem-servico/devolver`
    return await this.fetchResponse('PATCH', this.headerPadrao, obj, false, route, true)
  }

  async cancelarOrdemServico(obj) {
    const route = `${this.endpoint}ordem-servico/cancelar`
    return await this.fetchResponse('PATCH', this.headerPadrao, obj, false, route, true)
  }

  async despacharOrdemServico(obj) {
    const route = `${this.endpoint}ordem-servico/despachar`
    return await this.fetchResponse('PATCH', this.headerPadrao, obj, false, route, true)
  }
}

export default OrdemServicoService
