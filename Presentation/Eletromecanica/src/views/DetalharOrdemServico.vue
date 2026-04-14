<template>
  <Loading :active="loading" />
  <v-container fluid class="detalhar-ocorrencia__container">
    <!-- Botão Voltar -->
    <div class="mb-4">
      <BaseButton label="Voltar" type="back" iconPosition="left" @click="goBack" />
    </div>

    <!-- Bloco 1: Dados do tipo de ocorrência -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados da ocorrência
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="4">
            <v-text-field label="Código da ordem serviço" :model-value="ocorrencia.codigo" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field label="Estação da ordem serviço" :model-value="ocorrencia.estacao?.nome" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field label="Status da ordem serviço" :model-value="ocorrencia.status" variant="solo-filled" readonly />
          </v-col>
        </v-row>
        <v-row>
            <!-- Equipamentos -->
            <v-col cols="12" md="6">
                <v-card rounded="lg">
                <v-card-title class="d-flex align-center py-3">
                    <font-awesome-icon icon="microchip" class="me-2 text-primary" />
                    Equipamentos
                    <v-spacer />
                    <v-chip size="x-small" variant="flat">
                    {{ (equipamentosList ?? []).length }}
                    </v-chip>
                </v-card-title>

                <v-divider />

                <v-card-text class="pa-0">
                    <div v-if="(equipamentosList ?? []).length" class="list-scroll">
                    <v-list density="compact" class="py-1">
                        <v-list-item v-for="(e, idx) in (equipamentosList ?? [])" :key="idx">
                        <v-list-item-title class="text-body-2">
                            {{ formatEquipamento(e) }}
                        </v-list-item-title>
                        </v-list-item>
                    </v-list>
                    </div>

                    <div v-else class="pa-4 text-body-2">
                    <em>Sem equipamentos (OS para a estação inteira).</em>
                    </div>
                </v-card-text>
                </v-card>
            </v-col>

            <!-- Serviços Solicitados -->
            <v-col cols="12" md="6">
                <v-card rounded="lg">
                <v-card-title class="d-flex align-center py-3">
                    <font-awesome-icon icon="wrench" class="me-2 text-primary" />
                    Serviços solicitados
                    <v-spacer />
                    <v-chip size="x-small" variant="flat">
                    {{ (servicosList ?? []).length }}
                    </v-chip>
                </v-card-title>

                <v-divider />

                <v-card-text class="pa-0">
                    <div v-if="(servicosList ?? []).length" class="list-scroll">
                    <v-list density="compact" class="py-1">
                        <v-list-item v-for="(s, idx) in (servicosList ?? [])" :key="idx">
                        <v-list-item-title class="text-body-2">
                            {{ s.descricao }}
                        </v-list-item-title>
                        </v-list-item>
                    </v-list>
                    </div>

                    <div v-else class="pa-4 text-body-2">
                    <em>Sem serviços solicitados.</em>
                    </div>
                </v-card-text>
                </v-card>
            </v-col>
            </v-row>

      </v-card-text>
    </v-card>

    <!-- Bloco 2: Dados do local -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados da estação
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="4">
            <v-text-field label="Endereço" :model-value="ocorrencia.estacao?.endereco" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field label="Número" :model-value="ocorrencia.estacao?.numero" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field label="Bairro" :model-value="ocorrencia.estacao?.bairro" variant="solo-filled" readonly />
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field label="Complemento" :model-value="ocorrencia.estacao?.complemento" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field label="Ponto de referência" :model-value="ocorrencia.estacao?.pontoReferencia" variant="solo-filled" readonly />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Bloco 3: Dados do reclamante -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados do reclamante
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field label="Nome" :model-value="ocorrencia.nome" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field label="CPF" :model-value="ocorrencia.numeroDocumento" variant="solo-filled" readonly />
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field label="Telefone" :model-value="ocorrencia.telefone" variant="solo-filled" readonly />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field label="E-mail" :model-value="ocorrencia.email" variant="solo-filled" readonly />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Bloco 4: Localização no mapa -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Localização no mapa
      </v-card-title>
      <v-card-text>
        <GoogleMap
          v-if="ocorrencia.estacao?.lat && ocorrencia.estacao?.long"
          ref="mapRef"
          :api-key="apiKey"
          style="width: 100%; height: 300px"
          :center="center"
          :zoom="15"
          mapId="MAPA_RESUMO_OCORRENCIA"
        >
          <AdvancedMarker
            :options="{ position: { lat: parseFloat(ocorrencia.estacao?.lat), lng: parseFloat(ocorrencia.estacao?.long) } }"
          />
        </GoogleMap>
      </v-card-text>
    </v-card>

    <!-- Bloco 5: Observações -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Observações
      </v-card-title>
      <v-card-text>
        <div v-if="ocorrencia.observacao" class="ql-snow quill-readonly">
          <div class="ql-editor ql-editor-readonly" v-html="ocorrencia.observacao"></div>
        </div>
        <div v-else class="text-body-2">
          <em>Sem observações.</em>
        </div>
      </v-card-text>
    </v-card>

    <!-- Bloco 6: Histórico de Ocorrências -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="list-check" class="me-2" />
        Histórico de Ocorrências
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="historicoColumns" :items="ocorrencia.historico" dense hide-default-footer
          disable-pagination no-data-text="Nenhuma ocorrência associada encontrada">
          <template #header="{ headers }">
            <thead>
              <tr>
                <th v-for="col in headers" :key="col.key" class="text-start px-3 py-2">
                  {{ col.title }}
                </th>
              </tr>
            </thead>
          </template>
          <template #item="{ item, columns }">
            <tr>
              <td v-for="col in columns" :key="col.key" class="py-2 px-3">
                {{ item[col.key] }}
              </td>
            </tr>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>
  <Snackbar :retorno="retorno" :timeout="3000" :tipo="sucesso ? 'sucesso' : 'erro'"
        :mensagemRetorno="mensagemRetorno" @ocultarRetorno="() => { retorno = false }" />
</template>

<script setup>
import { ref, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import BaseButton from '@/components/base/BaseButton.vue'
import { GoogleMap, AdvancedMarker  } from "vue3-google-map"
import OcorrenciaService from '@/services/ordem-servico/ordem-servico-service'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const ocorrenciaService = new OcorrenciaService(
  endpoint,
  headerPadrao,
  chaveSeguranca,
  usuarioSeguranca
)

const loading = ref(false)
const route = useRoute()
const router = useRouter()
const apiKey = inject('apiKeyMaps')
const center = ref(null);
const equipamentosList = ref([]);
const servicosList = ref([]);

function formatEquipamento(e) {
    return  `${e.tag} — ${e.nome}`
}

// Função para voltar à página anterior
function goBack() {
  router.back()
}

// Simulação de dados da ocorrência selecionada
const ocorrencia = ref({});

const historicoColumns = [
  { key: 'numero', title: 'Nº Ocorrência' },
  { key: 'data', title: 'Data' },
  { key: 'servico', title: 'Serviço' },
  { key: 'status', title: 'Status' }
]

const mensagemRetorno = ref(null)
const sucesso = ref(true)
const retorno = ref(null)
async function obterDetalhes() {
  loading.value = true
  const result = await ocorrenciaService.obterDetalhes(route.params.id)
  loading.value = false
  if (result?.statusCode === 200) {
    ocorrencia.value = result.data.data
    equipamentosList.value = ocorrencia.value.equipamentos
    servicosList.value = ocorrencia.value.servicosSolicitados

    if (ocorrencia.value.estacao?.lat && ocorrencia.value.estacao?.long) {
      center.value = {
        lat: parseFloat(ocorrencia.value.estacao.lat),
        lng: parseFloat(ocorrencia.value.estacao.long)
      }
    }
  } else {
    mensagemRetorno.value = result.data.message
    sucesso.value = false
    retorno.value = true
  }
}

// inicialização
obterDetalhes()
</script>

<style scoped>
.detalhar-ocorrencia__container {
  padding: 16px;
}

:deep(.quill-editor) {
  background: #fff;
  border-radius: 8px;
}

:deep(.ql-editor) {
  height: calc(80vh - 290px) !important;
}

:deep(.quill-readonly .ql-editor-readonly) {
  min-height: 120px;
  padding: 12px;
  border: 1px solid #E0E0E0;
  border-radius: 8px;
  background: #FAFAFA;
  white-space: normal;
  overflow-wrap: anywhere;
}

.list-scroll {
  max-height: 240px;
  overflow: auto;
  padding-right: 6px;
}
</style>