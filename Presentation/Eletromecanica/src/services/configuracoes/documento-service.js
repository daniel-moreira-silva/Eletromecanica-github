import FetchService from "../fetch-service.js";

class DocumentoService extends FetchService {
  constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
    super(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
  }

  async adicionarDocumento(form) {
        const route = `${this.endpoint}documentos/adicionarDocumento`;
        return await this.fetchResponse("POST", this.headerPadrao, form, false, route, true);
    }
    async atualizarDocumento(obj) {
        const route = `${this.endpoint}documentos/atualizarDocumento`;
        return await this.fetchResponse("PUT", this.headerPadrao, obj, false, route, true);
    }
    async excluirDocumento(id) {
        const route = `${this.endpoint}documentos/excluirDocumento?id=${id}`;
        return await this.fetchResponse("DELETE", this.headerPadrao, null, false, route, true);
    }
    async listarDocumentosPorEntidade(entidadeId) {
        const route = `${this.endpoint}documentos/listarDocumentosPorEntidade?entidadeId=${entidadeId}`;
        return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
    }

    async gerarPDF(obj) {
        const route = `${this.endpoint}documentos/gerarPDF`;
        return await this.fetchResponse("POST", this.headerPadrao, obj, true, route, true);
    }

    async viewDocumentoBlob(id) {
        const route = `${this.endpoint}documentos/${id}/view`;
        return await this.fetchResponse("GET", this.headerPadrao, null, true, route, true);
    }

    async downloadDocumentoBlob(id) {
        const route = `${this.endpoint}documentos/${id}/download`;
        return await this.fetchResponse("GET", this.headerPadrao, null, true, route, true);
    }

    async listarTags(search) {
        const s = encodeURIComponent(search || "");
        const route = `${this.endpoint}documentos/tags?search=${s}`;
        return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
    }

    async criarTag(nome) {
        const route = `${this.endpoint}documentos/tags`;
        return await this.fetchResponse("POST", this.headerPadrao, { nome }, false, route, true);
    }

    async listarTagsDoDocumento(documentoId) {
        const route = `${this.endpoint}documentos/${documentoId}/tags`;
        return await this.fetchResponse("GET", this.headerPadrao, null, false, route, true);
    }

    async salvarTagsDoDocumento(documentoId, tagIds) {
        const route = `${this.endpoint}documentos/${documentoId}/tags`;
        return await this.fetchResponse("POST", this.headerPadrao, { tagIds }, false, route, true);
    }

}

export default DocumentoService;