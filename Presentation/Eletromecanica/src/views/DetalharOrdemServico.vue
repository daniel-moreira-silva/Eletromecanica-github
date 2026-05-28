<template>
  <Loading :active="loading" />
  <v-container fluid class="detalhar-ocorrencia__container">

    <!-- Header da página -->
    <div class="detalhe-header mb-4">

      <!-- Linha 1: Voltar | título centralizado | ações -->
      <div class="detalhe-header__top">
        <div class="detalhe-header__side">
          <BaseButton label="Voltar" type="back" iconPosition="left" @click="goBack" />
        </div>

        <div class="detalhe-header__center">
          <span class="text-h6 font-weight-bold">O.S. Número {{ ocorrencia.codigo || '—' }}</span>
          <v-chip v-if="modoEdicao" size="large" color="warning" variant="tonal" class="ms-2">
            <font-awesome-icon start size="large" icon="pencil" class="pr-2"/>
            Editando
          </v-chip>
        </div>

        <div class="detalhe-header__side detalhe-header__side--right">
          <template v-if="!modoEdicao">
            <BaseButton label="Gerar Sub-OS" type="save" @click="abrirSubOs">
              <template #prepend>
                <font-awesome-icon icon="plus" class="me-1" />
              </template>
            </BaseButton>
            <BaseButton label="Editar" type="edit" @click="iniciarEdicao">
              <template #prepend>
                <font-awesome-icon icon="pen" class="me-1" />
              </template>
            </BaseButton>
          </template>
          <template v-else>
            <BaseButton label="Cancelar" type="cancel" @click="cancelarEdicao" />
            <BaseButton label="Salvar alterações" type="save" :loading="loadingSalvar" @click="salvarEdicao" />
          </template>
        </div>
      </div>

      <!-- Linha 2: status + data (centralizada) -->
      <div class="detalhe-header__info">
        <v-chip :color="statusColor" size="large" variant="tonal" class="me-2">
          {{ ocorrencia.status || '—' }}
        </v-chip>
        <span v-if="dataSolicitacaoFormatada" class="text-body-2 text-medium-emphasis d-flex align-center gap-1">
          <font-awesome-icon icon="calendar-days" size="large" class="pr-1" />
          <div>Data de abertura {{ dataSolicitacaoFormatada }}</div>
        </span>
      </div>
    </div>

    <!-- Bloco 1: Dados da ocorrência -->
    <v-card class="mb-4" :class="{ 'card-editavel': modoEdicao }">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados da ocorrência
        <v-chip v-if="modoEdicao" size="large" color="warning" variant="tonal" class="ms-3">
          <font-awesome-icon icon="pencil" size="large" class="pr-2" />
          editável
        </v-chip>
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="4">
            <v-text-field
              label="Código da ordem serviço"
              :model-value="ocorrencia.codigo"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-if="!modoEdicao"
              label="Estação da ordem serviço"
              :model-value="ocorrencia.estacao?.nome"
              variant="solo-filled"
              readonly
            />
            <v-autocomplete
              v-else
              v-model="edit.estacao"
              :items="estacoesOptions"
              return-object
              label="Estação *"
              clearable
              variant="solo-filled"
              item-title="nome"
              item-value="id"
              no-data-text="Nenhuma estação encontrada"
              :loading="loadingOpcoes"
              @update:modelValue="onEditEstacaoChange"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              label="Status da ordem serviço"
              :model-value="ocorrencia.status"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
        </v-row>

        <!-- Equipamentos e Serviços (modo leitura) -->
        <v-row v-if="!modoEdicao">
          <v-col cols="12" md="6">
            <v-card rounded="lg">
              <v-card-title class="d-flex align-center py-3">
                <font-awesome-icon icon="microchip" class="me-2 text-primary" />
                Equipamentos
                <v-spacer />
                <v-chip size="small" variant="flat">{{ (equipamentosList ?? []).length }}</v-chip>
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <div v-if="(equipamentosList ?? []).length" class="list-scroll">
                  <v-list density="compact" class="py-1">
                    <v-list-item v-for="(e, idx) in (equipamentosList ?? [])" :key="idx">
                      <v-list-item-title class="text-body-2">{{ formatEquipamento(e) }}</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </div>
                <div v-else class="pa-4 text-body-2"><em>Sem equipamentos (OS para a estação inteira).</em></div>
              </v-card-text>
            </v-card>
          </v-col>

          <v-col cols="12" md="6">
            <v-card rounded="lg">
              <v-card-title class="d-flex align-center py-3">
                <font-awesome-icon icon="wrench" class="me-2 text-primary" />
                Serviços solicitados
                <v-spacer />
                <v-chip size="small" variant="flat">{{ (servicosList ?? []).length }}</v-chip>
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <div v-if="(servicosList ?? []).length" class="list-scroll">
                  <v-list density="compact" class="py-1">
                    <v-list-item v-for="(s, idx) in (servicosList ?? [])" :key="idx">
                      <v-list-item-title class="text-body-2">{{ s.descricao }}</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </div>
                <div v-else class="pa-4 text-body-2"><em>Sem serviços solicitados.</em></div>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>

        <!-- Equipamentos e Serviços (modo edição — igual ao Step 1) -->
        <v-row v-else>
          <v-col cols="12" md="7" class="mx-auto">

            <!-- Autocomplete Equipamentos -->
            <v-autocomplete
              v-model="edit.equipamentos"
              v-model:menu="menuEquipamentos"
              :items="equipamentosOptions"
              :disabled="!edit.estacao?.id || menuServicos"
              multiple
              return-object
              chips
              closable-chips
              clearable
              hide-selected
              variant="solo-filled"
              :label="edit.estacao?.id ? 'Equipamentos (opcional)' : 'Selecione uma estação para carregar os equipamentos'"
              item-title="tagnome"
              item-value="id"
              no-data-text="Nenhum equipamento encontrado"
              class="mt-3"
              attach="body"
              :loading="loadingEquipamentos"
            >
              <template #chip="{ props, item }">
                <v-chip v-bind="props" size="small" class="me-1">
                  {{ item?.raw?.tag || 'SEM TAG' }}
                </v-chip>
              </template>
            </v-autocomplete>

            <!-- Card de equipamentos selecionados -->
            <v-card v-if="edit.equipamentos?.length && !menuEquipamentos" class="mt-3" variant="elevated" elevation="0" border rounded="lg">
              <v-card-title class="d-flex align-center">
                <font-awesome-icon icon="microchip" class="me-2 text-primary" />
                Equipamentos selecionados
                <v-spacer />
                <v-chip size="small" variant="flat">{{ edit.equipamentos.length }}</v-chip>
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <v-list density="compact">
                  <v-list-item v-for="e in edit.equipamentos" :key="e.id">
                    <v-list-item-title class="text-body-2">{{ e.tagnome }}</v-list-item-title>
                    <template #append>
                      <v-tooltip text="Remover equipamento" location="top">
                        <template #activator="{ props }">
                          <v-btn v-bind="props" icon variant="text" size="small" @click="removerEquipamento(e.id)">
                            <font-awesome-icon icon="trash" />
                          </v-btn>
                        </template>
                      </v-tooltip>
                    </template>
                  </v-list-item>
                </v-list>
              </v-card-text>
              <v-divider />
              <v-card-actions class="justify-end">
                <v-btn variant="text" @click="edit.equipamentos = []">Limpar seleção</v-btn>
              </v-card-actions>
            </v-card>

            <!-- Autocomplete Serviços -->
            <v-autocomplete
              v-model="edit.servicos"
              v-model:menu="menuServicos"
              :items="servicosOptions"
              label="Serviços Solicitados *"
              multiple
              return-object
              chips
              closable-chips
              clearable
              hide-selected
              variant="solo-filled"
              item-title="descricao"
              item-value="id"
              no-data-text="Nenhum serviço encontrado"
              class="mt-6"
              attach="body"
              :disabled="menuEquipamentos"
              :loading="loadingOpcoes"
            >
              <template #chip="{ props, item }">
                <v-chip v-bind="props" size="small" class="me-1">
                  <font-awesome-icon icon="wrench" class="me-2" />
                  {{ item?.raw?.codigo }}
                </v-chip>
              </template>
            </v-autocomplete>

            <!-- Card de serviços selecionados -->
            <v-card v-if="edit.servicos?.length && !menuServicos" class="mt-3" variant="elevated" elevation="0" border rounded="lg">
              <v-card-title class="d-flex align-center">
                <font-awesome-icon icon="list-check" class="me-2 text-primary" />
                Serviços selecionados
                <v-spacer />
                <v-chip size="small" variant="flat">{{ edit.servicos.length }}</v-chip>
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <v-list density="compact">
                  <v-list-item v-for="s in edit.servicos" :key="s.id">
                    <v-list-item-title class="text-body-2">{{ s.descricao }}</v-list-item-title>
                    <template #append>
                      <v-tooltip text="Remover serviço" location="top">
                        <template #activator="{ props }">
                          <v-btn v-bind="props" icon variant="text" size="small" @click="removerServico(s.id)">
                            <font-awesome-icon icon="trash" />
                          </v-btn>
                        </template>
                      </v-tooltip>
                    </template>
                  </v-list-item>
                </v-list>
              </v-card-text>
              <v-divider />
              <v-card-actions class="justify-end">
                <v-btn variant="text" @click="edit.servicos = []">Limpar seleção</v-btn>
              </v-card-actions>
            </v-card>

          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Bloco 2: Dados da estação (sempre somente leitura) -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados da estação
        <v-chip v-if="modoEdicao" size="large" color="default" variant="tonal" class="ms-3">
          <font-awesome-icon icon="eye" size="large" class="pr-2" />
          somente leitura
        </v-chip>
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="4">
            <v-text-field
              label="Endereço"
              :model-value="dadosEstacao?.endereco"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              label="Número"
              :model-value="dadosEstacao?.numero"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              label="Bairro"
              :model-value="dadosEstacao?.bairro"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field
              label="Complemento"
              :model-value="dadosEstacao?.complemento"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              label="Ponto de referência"
              :model-value="dadosEstacao?.pontoReferencia"
              variant="solo-filled"
              readonly
              :append-inner-icon="modoEdicao ? 'fa-lock' : undefined"
            />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Bloco 3: Dados do reclamante -->
    <v-card class="mb-4" :class="{ 'card-editavel': modoEdicao }">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Dados do reclamante
        <v-chip v-if="modoEdicao" size="large" color="warning" variant="tonal" class="ms-3">
          <font-awesome-icon icon="pencil" size="large" class="pr-2" />
          editável
        </v-chip>
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field v-if="!modoEdicao" label="Nome" :model-value="ocorrencia.nome" variant="solo-filled" readonly />
            <v-text-field v-else label="Nome" v-model="edit.nome" variant="solo-filled" />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-if="!modoEdicao" label="CPF/CNPJ" :model-value="ocorrencia.numeroDocumento" variant="solo-filled" readonly />
            <v-text-field v-else label="CPF/CNPJ" v-model="edit.cpf" variant="solo-filled" />
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field v-if="!modoEdicao" label="Telefone" :model-value="ocorrencia.telefone" variant="solo-filled" readonly />
            <v-text-field v-else label="Telefone" v-model="edit.telefone" variant="solo-filled" />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-if="!modoEdicao" label="E-mail" :model-value="ocorrencia.email" variant="solo-filled" readonly />
            <v-text-field v-else label="E-mail" v-model="edit.email" variant="solo-filled" />
          </v-col>
        </v-row>
      </v-card-text>
    </v-card>

    <!-- Bloco 4: Localização no mapa (sempre somente leitura) -->
    <v-card class="mb-4">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Localização no mapa
        <v-chip v-if="modoEdicao" size="large" color="default" variant="tonal" class="ms-3">
          <font-awesome-icon icon="eye" size="large" class="pr-2" />
          somente leitura
        </v-chip>
      </v-card-title>
      <v-card-text>
        <GoogleMap
          v-if="mapaCenter"
          ref="mapRef"
          :api-key="apiKey"
          style="width: 100%; height: 300px"
          :center="mapaCenter"
          :zoom="15"
          mapId="MAPA_RESUMO_OCORRENCIA"
        >
          <AdvancedMarker :options="{ position: mapaCenter }" />
        </GoogleMap>
      </v-card-text>
    </v-card>

    <!-- Bloco 5: Observações -->
    <v-card class="mb-4" :class="{ 'card-editavel': modoEdicao }">
      <v-card-title>
        <font-awesome-icon icon="circle-info" class="me-2" />
        Observações
        <v-chip v-if="modoEdicao" size="large" color="warning" variant="tonal" class="ms-3">
          <font-awesome-icon icon="pencil" size="large" class="pr-2" />
          editável
        </v-chip>
      </v-card-title>
      <v-card-text>
        <template v-if="!modoEdicao">
          <div v-if="ocorrencia.observacao" class="ql-snow quill-readonly">
            <div class="ql-editor ql-editor-readonly" v-html="ocorrencia.observacao"></div>
          </div>
          <div v-else class="text-body-2"><em>Sem observações.</em></div>
        </template>
        <template v-else>
          <QuillEditor
            v-model:content="edit.observacao"
            contentType="html"
            theme="snow"
            toolbar="full"
            class="quill-editor-detalhe"
          />
        </template>
      </v-card-text>
    </v-card>

    <!-- Bloco 6: Fotos e Documentos -->
    <v-card class="mb-4">
      <v-card-title class="d-flex align-center">
        <font-awesome-icon icon="images" class="me-2" />
        Fotos e Documentos
        <v-chip class="ms-3" size="small" variant="flat" color="primary">
          {{ documentosOrdemServico.length }}
        </v-chip>
        <v-spacer />
        <BaseButton label="Gerenciar" type="edit" @click="documentosDialog = true">
          <template #prepend>
            <font-awesome-icon icon="folder-open" class="me-1" />
          </template>
        </BaseButton>
      </v-card-title>

      <!-- Seção: Fotos -->
      <v-card-text class="pb-2">
        <div class="preview-secao-header">
          <font-awesome-icon icon="camera" class="me-2 text-primary" />
          <span class="text-subtitle-2 font-weight-bold">Fotos da execução</span>
          <v-chip class="ms-2" size="x-small" variant="tonal" color="primary">{{ fotosCount }}</v-chip>
        </div>

        <div v-if="fotosPreview.length" class="fotos-gallery mt-3">
          <div
            v-for="doc in fotosPreview"
            :key="doc.id"
            class="foto-thumb"
            title="Clique para visualizar"
            @click="abrirLightboxFoto(doc)"
          >
            <img v-if="doc.previewUrl" :src="doc.previewUrl" :alt="doc.nomeOriginal" />
            <div v-else class="foto-thumb-placeholder">
              <font-awesome-icon icon="image" />
            </div>
            <div class="foto-thumb-overlay">
              <font-awesome-icon icon="eye" />
            </div>
          </div>
          <div
            v-if="fotosCount > fotosPreview.length"
            class="foto-thumb foto-thumb-mais"
            title="Ver todas as fotos"
            @click="documentosDialog = true"
          >
            <span>+{{ fotosCount - fotosPreview.length }}</span>
            <small>ver todas</small>
          </div>
        </div>
        <div v-else class="preview-vazio mt-2">
          <font-awesome-icon icon="camera" class="preview-vazio-icone" />
          <span>Nenhuma foto anexada</span>
        </div>
      </v-card-text>

      <v-divider />

      <!-- Seção: Arquivos -->
      <v-card-text class="pt-3">
        <div class="preview-secao-header">
          <font-awesome-icon icon="folder-open" class="me-2" style="color: #F9A825;" />
          <span class="text-subtitle-2 font-weight-bold">Arquivos e documentos</span>
          <v-chip class="ms-2" size="x-small" variant="tonal" color="warning">{{ arquivosCount }}</v-chip>
        </div>

        <div v-if="arquivosPreview.length" class="arquivos-lista mt-3">
          <div
            v-for="doc in arquivosPreview"
            :key="doc.id"
            class="arquivo-item"
            @click="abrirLightboxArquivo(doc)"
          >
            <div class="arquivo-item-icone">
              <font-awesome-icon :icon="getIconeDocumentoPreview(doc.nomeOriginal)" />
            </div>
            <div class="arquivo-item-info">
              <span class="arquivo-item-nome" :title="doc.nomeOriginal">{{ doc.nomeOriginal }}</span>
              <span class="arquivo-item-meta">{{ formatarTamanhoPreview(doc.tamanhoBytes) }}</span>
            </div>
            <font-awesome-icon icon="up-right-from-square" class="arquivo-item-link" />
          </div>

          <div
            v-if="arquivosCount > arquivosPreview.length"
            class="arquivo-ver-mais"
            @click="documentosDialog = true"
          >
            <font-awesome-icon icon="ellipsis" class="me-1" />
            ver mais {{ arquivosCount - arquivosPreview.length }} arquivo(s)
          </div>
        </div>
        <div v-else class="preview-vazio mt-2">
          <font-awesome-icon icon="folder-open" class="preview-vazio-icone" />
          <span>Nenhum arquivo anexado</span>
        </div>
      </v-card-text>
    </v-card>

    <!-- Bloco 7: Ordens de Serviço Relacionadas -->
    <v-card class="mb-4">
      <v-card-title class="d-flex align-center">
        <font-awesome-icon icon="sitemap" class="me-2" />
        Ordens de Serviço Relacionadas
        <v-chip class="ms-3" size="small" variant="flat" color="red">{{ subOsRelacionadas.length }}</v-chip>
        <v-progress-circular v-if="loadingRelacionadas" indeterminate size="18" width="2" class="ms-3" />
      </v-card-title>
      <v-card-text>
        <div v-if="eSubOS" class="mb-3">
          <v-chip color="warning" variant="tonal" size="large" prepend-icon="fa-circle-info">
            <font-awesome-icon>information</font-awesome-icon>
            Esta é uma Sub-OS ({{ ocorrencia.codigo }}). A OS pai é
            <strong class="ms-1">{{ codigoPaiEfetivo }}</strong>.
          </v-chip>
        </div>

        <v-data-table
          :headers="subOsColumns"
          :items="subOsRelacionadas"
          density="compact"
          hide-default-footer
          disable-pagination
          no-data-text="Nenhuma OS relacionada encontrada"
        >
          <template #item="{ item, columns }">
            <tr>
              <td v-for="col in columns" :key="col.key" class="py-2 px-3">
                <template v-if="col.key === 'codigo'">
                  <v-chip size="small" :color="item.subOS === 0 ? 'primary' : 'default'" variant="tonal">
                    {{ item.codigo }}
                  </v-chip>
                  <v-chip v-if="item.subOS === 0" size="x-small" color="primary" class="ms-2">OS Pai</v-chip>
                </template>
                <v-btn
                  v-else-if="col.key === 'acao'"
                  size="x-small"
                  variant="tonal"
                  color="primary"
                  @click="navegarParaOS(item.id)"
                >
                  <font-awesome-icon icon="up-right-from-square" class="me-1" />
                  Abrir
                </v-btn>
                <span v-else>{{ item[col.key] }}</span>
              </td>
            </tr>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>

  <!-- Diálogo de Confirmação: Gerar Sub-OS -->
  <v-dialog v-model="subOsConfirmDialog" max-width="520" persistent scrim="rgba(0,0,0,0.7)">
    <v-card class="pa-2">
      <v-card-title class="d-flex align-center pa-4 pb-2">
        <font-awesome-icon icon="plus" class="text-primary me-2" />
        <span class="text-h6">Gerar Sub-OS</span>
        <v-spacer />
        <font-awesome-icon icon="xmark" class="cursor-pointer" @click="subOsConfirmDialog = false" />
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-4">
        <v-alert type="info" variant="tonal" class="mb-4">
          <template v-if="eSubOS">
            Você está em uma <strong>Sub-OS</strong> ({{ ocorrencia.codigo }}).
            A nova Sub-OS será criada como <strong>irmã</strong>, vinculada à OS pai
            <strong>{{ codigoPaiEfetivo }}</strong> — e não como filha desta sub-OS.
          </template>
          <template v-else>
            Será criada uma nova <strong>Sub-OS</strong> vinculada à O.S.
            <strong>{{ ocorrencia.codigo }}</strong>.
          </template>
        </v-alert>
        <p class="text-body-2 text-medium-emphasis">Deseja continuar?</p>
      </v-card-text>
      <v-divider />
      <v-card-actions class="pa-4 justify-end">
        <BaseButton label="Cancelar" type="cancel" extraClass="me-2" @click="subOsConfirmDialog = false" />
        <BaseButton label="Sim, gerar Sub-OS" type="confirm" @click="confirmarGerarSubOs" />
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Dialog Sub-OS (fullscreen) -->
  <v-dialog v-model="subOsDialog" fullscreen persistent scrim="rgba(0,0,0,0.7)">
    <v-card rounded="0">
      <v-toolbar density="comfortable" color="white" elevation="1">
        <v-toolbar-title>
          <font-awesome-icon icon="plus" class="text-primary me-2" />
          Criar Sub-OS vinculada a {{ codigoPaiEfetivo }}
        </v-toolbar-title>
        <v-spacer />
        <font-awesome-icon icon="xmark" class="text-black cursor-pointer mr-4" @click="fecharSubOs" />
      </v-toolbar>
      <v-divider />
      <NovaOrdemServico
        v-if="subOsDialog && ordemServicoPaiIdEfetivo"
        :ordemServicoPaiId="ordemServicoPaiIdEfetivo"
        :modoSubOS="true"
        @fechar="fecharSubOs"
        @salvo="handleSubOSSalva"
      />
    </v-card>
  </v-dialog>

  <OrdemServicoDocumentos
    ref="documentosRef"
    v-model="documentosDialog"
    :ordemServico="ordemServicoParaDocumentos"
    @atualizado="onDocumentosAtualizado"
  />

  <DocumentoLightbox
    v-model="lightboxAberto"
    :documentos="lightboxDocumentosAtivos"
    :indice="lightboxIndice"
    :fn-carregar-preview="carregarPreviewDocumento"
    :fn-baixar="baixarDocumentoPreview"
  />

  <Snackbar :retorno="retorno" :timeout="3000" :tipo="sucesso ? 'sucesso' : 'erro'"
    :mensagemRetorno="mensagemRetorno" @ocultarRetorno="() => { retorno = false }" />
</template>

<script setup>
import { ref, computed, reactive, inject, watch, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import BaseButton from '@/components/base/BaseButton.vue'
import { GoogleMap, AdvancedMarker } from 'vue3-google-map'
import { QuillEditor } from '@vueup/vue-quill'
import OcorrenciaService from '@/services/ordem-servico/ordem-servico-service'
import EstacaoService from '@/services/configuracoes/estacao-service.js'
import ServicoSolicitadoService from '@/services/configuracoes/servico-solicitado-service'
import EquipamentoService from '@/services/configuracoes/equipamento-service.js'
import DocumentoService from '@/services/configuracoes/documento-service'
import Loading from '@/components/base/LoadingOverlay.vue'
import Snackbar from '@/components/base/Snackbar.vue'
import NovaOrdemServico from '@/views/NovaOrdemServico.vue'
import OrdemServicoDocumentos from '@/views/ordem-servico/OrdemServicoDocumentos.vue'
import DocumentoLightbox from '@/components/common/DocumentoLightbox.vue'

const endpoint = inject('endpoint')
const headerPadrao = inject('headerPadrao')
const chaveSeguranca = inject('chaveSeguranca')
const usuarioSeguranca = inject('usuarioSeguranca')

const ocorrenciaService = new OcorrenciaService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const estacaoService = new EstacaoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const servicoSolicitadoService = new ServicoSolicitadoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const equipamentoService = new EquipamentoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)
const documentoService = new DocumentoService(endpoint, headerPadrao, chaveSeguranca, usuarioSeguranca)

const loading = ref(false)
const route = useRoute()
const router = useRouter()
const apiKey = inject('apiKeyMaps')
const center = ref(null)
const equipamentosList = ref([])
const servicosList = ref([])

// ── Documentos ───────────────────────────────────────────────────────────────
const documentosDialog = ref(false)
const documentosRef = ref(null)
const documentosOrdemServico = ref([])
const documentosObjectUrls = ref(new Set())

const FOTOS_PREVIEW_MAX = 8
const ARQUIVOS_PREVIEW_MAX = 4

const fotosOrdemServico = computed(() =>
  [...documentosOrdemServico.value.filter(d => d.fotoExecucao === true)]
    .sort((a, b) => (a.ordem ?? 0) - (b.ordem ?? 0))
)
const arquivosOrdemServico = computed(() => documentosOrdemServico.value.filter(d => d.fotoExecucao !== true))

const fotosCount = computed(() => fotosOrdemServico.value.length)
const arquivosCount = computed(() => arquivosOrdemServico.value.length)

const fotosPreview = computed(() => fotosOrdemServico.value.slice(0, FOTOS_PREVIEW_MAX))
const arquivosPreview = computed(() => arquivosOrdemServico.value.slice(0, ARQUIVOS_PREVIEW_MAX))

const ordemServicoParaDocumentos = computed(() => {
  const os = ocorrencia.value
  if (!os?.id) return null
  return {
    id: os.id,
    codigo: os.codigo,
    endereco: os.estacao?.endereco || '',
    servico: (os.servicosSolicitados || [])[0]?.descricao || '',
  }
})

function getExtensaoPreview(nomeArquivo) {
  if (!nomeArquivo) return ''
  const idx = nomeArquivo.lastIndexOf('.')
  return idx >= 0 ? nomeArquivo.slice(idx).toLowerCase() : ''
}

function formatarTamanhoPreview(bytes) {
  const v = Number(bytes || 0)
  if (v < 1024) return `${v} B`
  if (v < 1024 * 1024) return `${(v / 1024).toFixed(1)} KB`
  return `${(v / (1024 * 1024)).toFixed(2)} MB`
}

function getIconeDocumentoPreview(nomeArquivo) {
  const ext = getExtensaoPreview(nomeArquivo)
  if (['.jpg', '.jpeg', '.png', '.webp'].includes(ext)) return 'image'
  if (ext === '.pdf') return 'file-pdf'
  if (['.doc', '.docx'].includes(ext)) return 'file-word'
  if (['.xls', '.xlsx', '.csv'].includes(ext)) return 'file-excel'
  if (ext === '.txt') return 'file-lines'
  if (ext === '.mp3') return 'file-audio'
  if (ext === '.mp4') return 'file-video'
  return 'file'
}

async function carregarPreviewDocumento(doc) {
  if (!doc?.id || doc.previewUrl) return
  const resp = await documentoService.viewDocumentoBlob(doc.id)
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) return
  const url = URL.createObjectURL(resp.data)
  doc.previewUrl = url
  documentosObjectUrls.value.add(url)
}

async function carregarDocumentos() {
  if (!ocorrencia.value?.id) return
  const result = await documentoService.listarDocumentosPorEntidade(ocorrencia.value.id)
  if (result?.statusCode === 200) {
    for (const url of documentosObjectUrls.value) URL.revokeObjectURL(url)
    documentosObjectUrls.value.clear()
    const mime = x => x.mimeType || 'application/octet-stream'
    documentosOrdemServico.value = (result.data ?? []).map(x => ({
      ...x,
      imagem: (mime(x)).toLowerCase().startsWith('image/'),
      pdf: (mime(x)).toLowerCase() === 'application/pdf',
      audio: (mime(x)).startsWith('audio/'),
      video: (mime(x)).startsWith('video/'),
      previewUrl: null,
    }))
    // carrega preview das fotos de execução
    const fotos = documentosOrdemServico.value
      .filter(d => d.fotoExecucao === true)
      .sort((a, b) => (a.ordem ?? 0) - (b.ordem ?? 0))
      .slice(0, FOTOS_PREVIEW_MAX)
    for (const doc of fotos) {
      carregarPreviewDocumento(doc)
    }
  } else if (result?.statusCode === 404) {
    documentosOrdemServico.value = []
  }
}

function onDocumentosAtualizado() {
  carregarDocumentos()
}

// ── Lightbox (fotos e documentos) ─────────────────────────────────────────
const lightboxAberto = ref(false)
const lightboxIndice = ref(0)
const lightboxTipo = ref('foto') // 'foto' | 'arquivo'

const lightboxDocumentosAtivos = computed(() =>
  lightboxTipo.value === 'foto' ? fotosOrdemServico.value : arquivosOrdemServico.value
)

function abrirLightboxFoto(doc) {
  lightboxTipo.value = 'foto'
  const idx = fotosOrdemServico.value.findIndex(d => d.id === doc.id)
  lightboxIndice.value = idx >= 0 ? idx : 0
  lightboxAberto.value = true
}

function abrirLightboxArquivo(doc) {
  lightboxTipo.value = 'arquivo'
  const idx = arquivosOrdemServico.value.findIndex(d => d.id === doc.id)
  lightboxIndice.value = idx >= 0 ? idx : 0
  lightboxAberto.value = true
}

async function baixarDocumentoPreview(item) {
  if (!item?.id) return
  const resp = await documentoService.downloadDocumentoBlob(item.id)
  if (resp?.statusCode !== 200 || !(resp.data instanceof Blob)) return
  const url = URL.createObjectURL(resp.data)
  documentosObjectUrls.value.add(url)
  const a = document.createElement('a')
  a.href = url
  a.download = item.nomeOriginal || 'download'
  a.click()
}

onBeforeUnmount(() => {
  for (const url of documentosObjectUrls.value) URL.revokeObjectURL(url)
  documentosObjectUrls.value.clear()
})

function formatEquipamento(e) {
  return `${e.tag} — ${e.nome}`
}

function goBack() {
  router.back()
}

const ocorrencia = ref({})
const mensagemRetorno = ref(null)
const sucesso = ref(true)
const retorno = ref(null)

// ── Edição in-place ──────────────────────────────────────────────────────────
const modoEdicao = ref(false)
const loadingSalvar = ref(false)
const loadingOpcoes = ref(false)
const loadingEquipamentos = ref(false)
const menuEquipamentos = ref(false)
const menuServicos = ref(false)

const edit = reactive({
  estacao: null,
  equipamentos: [],
  servicos: [],
  nome: '',
  telefone: '',
  cpf: '',
  email: '',
  observacao: '',
})

const estacoesOptions = ref([])
const equipamentosOptions = ref([])
const servicosOptions = ref([])

watch(menuEquipamentos, (abriu) => { if (abriu) menuServicos.value = false })
watch(menuServicos, (abriu) => { if (abriu) menuEquipamentos.value = false })

const dataSolicitacaoFormatada = computed(() => {
  if (!ocorrencia.value.dataSolicitacao) return null
  return new Date(ocorrencia.value.dataSolicitacao).toLocaleDateString('pt-BR', {
    second: '2-digit', minute: '2-digit' ,hour: '2-digit' ,day: '2-digit', month: '2-digit', year: 'numeric'
  })
})

const statusColor = computed(() => {
  switch (ocorrencia.value.status) {
    case 'Solicitada':    return 'info'
    case 'Iniciada':      return 'warning'
    case 'Em Andamento':  return 'purple'
    case 'Finalizada':    return 'success'
    case 'Cancelada':     return 'error'
    case 'Pendente':      return 'default'
    default:              return 'default'
  }
})

const dadosEstacao = computed(() =>
  modoEdicao.value ? (edit.estacao || ocorrencia.value.estacao) : ocorrencia.value.estacao
)

const mapaCenter = computed(() => {
  const estacao = modoEdicao.value ? edit.estacao : ocorrencia.value.estacao
  if (estacao?.lat && estacao?.long) {
    return { lat: parseFloat(estacao.lat), lng: parseFloat(estacao.long) }
  }
  return center.value
})

async function listarEstacoes() {
  loadingOpcoes.value = true
  const result = await estacaoService.buscarEstacoes()
  loadingOpcoes.value = false
  if (result?.statusCode === 200) estacoesOptions.value = result?.data?.data || []
}

async function listarEquipamentosPorEstacao(estacaoId) {
  loadingEquipamentos.value = true
  const result = await equipamentoService.listarEquipamentosPorEstacao(estacaoId, null)
  loadingEquipamentos.value = false
  if (result?.statusCode === 200) {
    equipamentosOptions.value = (result?.data?.data || []).map(x => ({ ...x, tagnome: `${x.tag} | ${x.nome}` }))
  }
}

async function listarServicosSolicitados() {
  loadingOpcoes.value = true
  const result = await servicoSolicitadoService.buscarTodos(false)
  loadingOpcoes.value = false
  if (result?.statusCode === 200) servicosOptions.value = result?.data?.data || []
}

async function onEditEstacaoChange(nova) {
  edit.equipamentos = []
  equipamentosOptions.value = []
  if (nova?.id) await listarEquipamentosPorEstacao(nova.id)
}

function removerEquipamento(id) {
  edit.equipamentos = edit.equipamentos.filter(x => x.id !== id)
}

function removerServico(id) {
  edit.servicos = edit.servicos.filter(x => x.id !== id)
}

async function iniciarEdicao() {
  edit.nome = ocorrencia.value.nome || ''
  edit.telefone = (ocorrencia.value.telefone || '').replace(/^\+55/, '')
  edit.cpf = ocorrencia.value.numeroDocumento || ''
  edit.email = ocorrencia.value.email || ''
  edit.observacao = ocorrencia.value.observacao || ''

  if (!estacoesOptions.value.length) await listarEstacoes()
  if (!servicosOptions.value.length) await listarServicosSolicitados()

  const estacao = estacoesOptions.value.find(e => e.id === ocorrencia.value.estacaoId)
  edit.estacao = estacao || null

  if (estacao?.id) {
    await listarEquipamentosPorEstacao(estacao.id)
    edit.equipamentos = equipamentosOptions.value.filter(e =>
      (ocorrencia.value.equipamentos || []).some(oe => oe.id === e.id)
    )
  }

  edit.servicos = servicosOptions.value.filter(s =>
    (ocorrencia.value.servicosSolicitados || []).some(ss => ss.id === s.id)
  )

  modoEdicao.value = true
}

function cancelarEdicao() {
  modoEdicao.value = false
}

async function salvarEdicao() {
  const payload = {
    id: ocorrencia.value.id,
    codigo: ocorrencia.value.codigo,
    numero: ocorrencia.value.numero,
    subOS: ocorrencia.value.subOS,
    ano: ocorrencia.value.ano,
    statusId: ocorrencia.value.statusId,
    dataSolicitacao: ocorrencia.value.dataSolicitacao,
    ordemServicoPaiId: ocorrencia.value.ordemServicoPaiId,
    tipoOS: ocorrencia.value.tipoOS,
    prioridade: ocorrencia.value.prioridade,
    EstacaoId: edit.estacao?.id,
    Equipamentos: (edit.equipamentos || []).map(e => ({
      OrdemServicoId: ocorrencia.value.id,
      EquipamentoId: e.id,
    })),
    servicosSolicitados: (edit.servicos || []).map(s => ({
      servicoSolicitadoId: s.id,
    })),
    nome: edit.nome,
    telefone: edit.telefone ? `+55${edit.telefone.replace(/\D/g, '')}` : '',
    numeroDocumento: edit.cpf,
    email: edit.email,
    observacao: edit.observacao,
  }

  loadingSalvar.value = true
  const result = await ocorrenciaService.atualizarOrdemServico(payload)
  loadingSalvar.value = false

  if (result?.statusCode === 200) {
    modoEdicao.value = false
    mensagemRetorno.value = 'Ordem de serviço atualizada com sucesso.'
    sucesso.value = true
    retorno.value = true
    await obterDetalhes()
  } else {
    mensagemRetorno.value = result?.data?.message || 'Falha ao atualizar Ordem de Serviço.'
    sucesso.value = false
    retorno.value = true
  }
}

// ── Sub-OS ───────────────────────────────────────────────────────────────────
const ordemServicoPaiIdEfetivo = computed(() =>
  ocorrencia.value.ordemServicoPaiId || ocorrencia.value.id || null
)

const codigoPaiEfetivo = computed(() => {
  const n = ocorrencia.value.numero
  const a = ocorrencia.value.ano
  if (!n || !a) return '—'
  return `${n}/${a}/0`
})

const eSubOS = computed(() => (ocorrencia.value.subOS ?? 0) > 0)

const subOsConfirmDialog = ref(false)
const subOsDialog = ref(false)
const subOsRelacionadas = ref([])
const loadingRelacionadas = ref(false)

const subOsColumns = [
  { key: 'codigo', title: 'Código' },
  { key: 'status', title: 'Status' },
  { key: 'dataSolicitacaoFormatada', title: 'Data Solicitação' },
  { key: 'acao', title: '' },
]

function abrirSubOs() {
  subOsConfirmDialog.value = true
}

function confirmarGerarSubOs() {
  subOsConfirmDialog.value = false
  subOsDialog.value = true
}

function fecharSubOs() {
  subOsDialog.value = false
}

async function handleSubOSSalva() {
  subOsDialog.value = false
  mensagemRetorno.value = 'Sub-OS criada com sucesso.'
  sucesso.value = true
  retorno.value = true
  await carregarSubOsRelacionadas()
}

function navegarParaOS(id) {
  //router.push({ name: 'DetalharOrdemServico', params: { id } })
  const route = router.resolve({ name: 'DetalharOrdemServico', params: { id } })
  window.open(route.href, '_blank')
}

async function carregarSubOsRelacionadas() {
  const numero = ocorrencia.value.numero
  const ano = ocorrencia.value.ano
  const id = ocorrencia.value.id
  if (!numero || !ano || !id) return

  loadingRelacionadas.value = true
  const result = await ocorrenciaService.listarSubOs(numero, ano, id)
  loadingRelacionadas.value = false

  if (result?.statusCode === 200) {
    subOsRelacionadas.value = result.data?.data || []
  }
}

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
        lng: parseFloat(ocorrencia.value.estacao.long),
      }
    }

    await carregarSubOsRelacionadas()
    await carregarDocumentos()
  } else {
    mensagemRetorno.value = result.data.message
    sucesso.value = false
    retorno.value = true
  }
}

obterDetalhes()

watch(() => route.params.id, (newId) => {
  if (newId) obterDetalhes()
})
</script>

<style scoped>
.detalhar-ocorrencia__container {
  padding: 16px;
}

/* Header da página */
.detalhe-header {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detalhe-header__top {
  display: flex;
  align-items: center;
}

.detalhe-header__side {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 8px;
}

.detalhe-header__side--right {
  justify-content: flex-end;
}

.detalhe-header__center {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  padding: 0 16px;
}

.detalhe-header__info {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  flex-wrap: wrap;
}

/* Card com campos editáveis — borda laranja no modo edição */
.card-editavel {
  border-left: 4px solid #FB8C00 !important;
}

:deep(.quill-editor-detalhe .ql-editor) {
  min-height: 200px;
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

/* ── Fotos e Documentos preview ─────────────────────────────────────── */
.preview-secao-header {
  display: flex;
  align-items: center;
}

.preview-vazio {
  display: flex;
  align-items: center;
  gap: 8px;
  color: rgba(0,0,0,0.38);
  font-size: 13px;
  font-style: italic;
  padding: 4px 0 8px;
}

.preview-vazio-icone {
  font-size: 18px;
  opacity: 0.5;
}

/* Galeria de fotos */
.fotos-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.foto-thumb {
  width: 110px;
  height: 110px;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid rgba(0,0,0,0.08);
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  transition: border-color 0.15s, transform 0.15s, box-shadow 0.15s;
  flex-shrink: 0;
}

.foto-thumb:hover {
  border-color: rgb(var(--v-theme-primary));
  transform: scale(1.04);
  box-shadow: 0 4px 14px rgba(0,0,0,0.14);
}

.foto-thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.foto-thumb-placeholder {
  font-size: 30px;
  color: rgba(var(--v-theme-primary), 0.35);
}

.foto-thumb-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 22px;
  opacity: 0;
  transition: background 0.18s, opacity 0.18s;
}

.foto-thumb:hover .foto-thumb-overlay {
  background: rgba(0,0,0,0.32);
  opacity: 1;
}

.foto-thumb-mais {
  background: rgba(var(--v-theme-primary), 0.07);
  border: 2px dashed rgba(var(--v-theme-primary), 0.4);
  flex-direction: column;
  gap: 2px;
}

.foto-thumb-mais span {
  font-size: 22px;
  font-weight: 800;
  color: rgb(var(--v-theme-primary));
  line-height: 1;
}

.foto-thumb-mais small {
  font-size: 10px;
  color: rgb(var(--v-theme-primary));
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

/* Lista de arquivos */
.arquivos-lista {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.arquivo-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid rgba(0,0,0,0.08);
  background: #FAFAFA;
  cursor: pointer;
  transition: background 0.15s, border-color 0.15s, box-shadow 0.15s;
}

.arquivo-item:hover {
  background: rgba(var(--v-theme-primary), 0.05);
  border-color: rgba(var(--v-theme-primary), 0.4);
  box-shadow: 0 2px 8px rgba(0,0,0,0.07);
}

.arquivo-item-icone {
  font-size: 22px;
  color: rgb(var(--v-theme-primary));
  flex-shrink: 0;
  width: 28px;
  text-align: center;
}

.arquivo-item-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.arquivo-item-nome {
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: rgba(0,0,0,0.85);
}

.arquivo-item-meta {
  font-size: 11px;
  color: rgba(0,0,0,0.45);
}

.arquivo-item-link {
  font-size: 12px;
  color: rgba(var(--v-theme-primary), 0.5);
  flex-shrink: 0;
  transition: color 0.15s;
}

.arquivo-item:hover .arquivo-item-link {
  color: rgb(var(--v-theme-primary));
}

.arquivo-ver-mais {
  font-size: 12px;
  color: rgb(var(--v-theme-primary));
  cursor: pointer;
  padding: 6px 14px;
  text-align: center;
  border-radius: 6px;
  border: 1px dashed rgba(var(--v-theme-primary), 0.3);
  transition: background 0.15s;
}

.arquivo-ver-mais:hover {
  background: rgba(var(--v-theme-primary), 0.05);
}
</style>
