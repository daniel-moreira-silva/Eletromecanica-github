import Response from './response'
import StorageService from './storage-service'
import CryptoJS from 'crypto-js'

class FetchService extends StorageService {
    constructor(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca) {
        super()
        this.endpoint = endpoint
        this.headerPadrao = headerPadrao
        this.chaveSeguranca = chaveSeguranca
        this.usuarioSeguranca = usuarioSeguranca
    }

    async fetchResponse(method, headers, body, blob, route, validate = true) {
        // const validado = validate ? await this.validarToken() : true
        // if (!validado) return
        const requestHeaders = { ...(headers || {}) };

        let requestOptions = {
            method: method,
            headers: requestHeaders
        }

        // Adiciona o token
        requestHeaders.Authorization = `Bearer ${localStorage.getItem('tokenSeguranca')}`

        // Adiciona o login criptografado no cabeçalho
        if (localStorage.getItem('loginNovoSanegeo')) {
            const bytes = CryptoJS.AES.decrypt(localStorage.getItem('loginNovoSanegeo'), this.chaveSeguranca)
            const loginData = JSON.parse(bytes.toString(CryptoJS.enc.Utf8))
            requestHeaders.Usuario = loginData.id
        }

        if (body) {
            if (body instanceof FormData) {
                requestOptions.body = body;

                if (requestHeaders["Content-Type"]) {
                    delete requestHeaders["Content-Type"];
                }

            } else {
                requestOptions.body = JSON.stringify(body);
            }
        }

        if (blob)
            requestOptions.responseType = 'blob'

        let response = new Response()
        try {
            const fetchResponse = await fetch(route, requestOptions)
            let data = null

            if (blob && fetchResponse.ok) {
                data = await fetchResponse.blob()
            } else {
                // tenta json, mas se não for json, cai no texto
                const contentType = fetchResponse.headers.get("content-type") || ""
                if (contentType.includes("application/json")) {
                    data = await fetchResponse.json()
                } else {
                    data = await fetchResponse.text()
                }
            }

            response.statusCode = fetchResponse.status
            response.data = data

            // Token expirado -> renovar automaticamente
            if (response.statusCode === 401 && data.data?.erroToken) {

                const resultadoToken = await this.obterToken()
                if (resultadoToken.statusCode === 200) {

                    localStorage.setItem('tokenSeguranca', resultadoToken?.data.token)
                    return await this.fetchResponse(method, requestHeaders, body, blob, route, validate)
                }
            }
        } catch {
            response.statusCode = 400
            response.data = {
                message: 'Serviço indisponível. Tente novamente mais tarde ou contate o administrador do sistema.'
            }
        }

        return response
    }

    async obterToken() {

        const route = `${this.endpoint}seguranca/token`
        return await this.fetchResponse(
            'POST',
            this.headerPadrao,
            this.usuarioSeguranca,
            false,
            route,
            false
        )
    }
}

export default FetchService
