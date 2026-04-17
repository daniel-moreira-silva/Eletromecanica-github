SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================================================
   1) LOG
   ========================================================= */
IF OBJECT_ID('dbo.Log', 'U') IS NOT NULL DROP TABLE dbo.Log;
GO

CREATE TABLE dbo.Log(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Log PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Aplicacao VARCHAR(255) NOT NULL,
    DataHora DATETIME NOT NULL,
    Nivel VARCHAR(50) NOT NULL,
    Mensagem NVARCHAR(MAX) NULL,
    Origem VARCHAR(255) NULL,
    Endereco VARCHAR(1000) NULL,
    Excecao NVARCHAR(MAX) NULL,
    Usuario VARCHAR(255) NULL
);
GO

/* =========================================================
   2) TIPO ESTACAO + ESTACAO
   ========================================================= */
IF OBJECT_ID('dbo.Estacao', 'U') IS NOT NULL DROP TABLE dbo.Estacao;
IF OBJECT_ID('dbo.TipoEstacao', 'U') IS NOT NULL DROP TABLE dbo.TipoEstacao;
GO

CREATE TABLE dbo.TipoEstacao (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TipoEstacao PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Nome NVARCHAR(50) NOT NULL CONSTRAINT UQ_TipoEstacao_Nome UNIQUE,
    Ativo BIT NOT NULL CONSTRAINT DF_TipoEstacao_Ativo DEFAULT 1
);
GO

CREATE TABLE dbo.Estacao (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Estacao PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    TipoEstacaoId UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(150) NOT NULL,
    Observacoes NVARCHAR(500) NULL,

    -- Endereço
    Endereco NVARCHAR(255) NULL,
    Bairro NVARCHAR(255) NULL,
    Lat NVARCHAR(50) NULL,
    Long NVARCHAR(50) NULL,
    Numero NVARCHAR(20) NULL,
    Complemento NVARCHAR(255) NULL,
    PontoReferencia NVARCHAR(255) NULL,

    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Estacao_DataCriacao DEFAULT SYSDATETIME(),
    Ativo BIT NOT NULL CONSTRAINT DF_Estacao_Ativo DEFAULT 1,

    CONSTRAINT FK_Estacao_TipoEstacao
        FOREIGN KEY (TipoEstacaoId) REFERENCES dbo.TipoEstacao(Id)
);
GO

-- Unicidade de Nome por Tipo
CREATE UNIQUE INDEX UX_Estacao_Tipo_Nome
ON dbo.Estacao (TipoEstacaoId, Nome);
GO

/* =========================================================
   3) TIPO EQUIPAMENTO + EQUIPAMENTO (2 níveis)
   ========================================================= */
IF OBJECT_ID('dbo.MedidorVazao', 'U') IS NOT NULL DROP TABLE dbo.MedidorVazao;
IF OBJECT_ID('dbo.Nobreak', 'U') IS NOT NULL DROP TABLE dbo.Nobreak;
IF OBJECT_ID('dbo.CLP', 'U') IS NOT NULL DROP TABLE dbo.CLP;
IF OBJECT_ID('dbo.Motor', 'U') IS NOT NULL DROP TABLE dbo.Motor;
IF OBJECT_ID('dbo.Bomba', 'U') IS NOT NULL DROP TABLE dbo.Bomba;
IF OBJECT_ID('dbo.CaracteristicaEquipamento', 'U') IS NOT NULL DROP TABLE dbo.CaracteristicaEquipamento;
IF OBJECT_ID('dbo.Equipamento', 'U') IS NOT NULL DROP TABLE dbo.Equipamento;
IF OBJECT_ID('dbo.TipoEquipamento', 'U') IS NOT NULL DROP TABLE dbo.TipoEquipamento;
GO

CREATE TABLE dbo.TipoEquipamento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TipoEquipamento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Nome NVARCHAR(80) NOT NULL CONSTRAINT UQ_TipoEquipamento_Nome UNIQUE,
    Categoria NVARCHAR(50) NOT NULL,              -- Ex.: Hidráulico, Elétrico, Automação, Instrumentação
    Nivel TINYINT NOT NULL,                       -- 1=Principal, 2=Componente (Instrumento entra como componente)
    Ativo BIT NOT NULL CONSTRAINT DF_TipoEquipamento_Ativo DEFAULT 1,

    CONSTRAINT CK_TipoEquipamento_Nivel
        CHECK (Nivel IN (1,2))
);
GO


CREATE TABLE Equipamento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Equipamento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    EstacaoId UNIQUEIDENTIFIER NOT NULL,
    TipoEquipamentoId UNIQUEIDENTIFIER NOT NULL,

    -- 2 níveis: NULL = Principal, NOT NULL = Componente do principal
    EquipamentoPrincipalId UNIQUEIDENTIFIER NULL,

    Nome NVARCHAR(150) NOT NULL,                  -- nome amigável/operacional
    Tag NVARCHAR(50) NULL,                        -- opcional (muitos componentes não têm tag "oficial")
    Fabricante NVARCHAR(100) NULL,
    Modelo NVARCHAR(100) NULL,
    NumeroSerie NVARCHAR(100) NULL,
    Observacoes NVARCHAR(500) NULL,
    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Equipamento_DataCriacao DEFAULT SYSDATETIME(),
    Ativo BIT NOT NULL CONSTRAINT DF_Equipamento_Ativo DEFAULT 1,

    CONSTRAINT FK_Equipamento_Estacao
        FOREIGN KEY (EstacaoId) REFERENCES dbo.Estacao(Id),

    CONSTRAINT FK_Equipamento_TipoEquipamento
        FOREIGN KEY (TipoEquipamentoId) REFERENCES dbo.TipoEquipamento(Id),

    CONSTRAINT FK_Equipamento_EquipamentoPrincipal
        FOREIGN KEY (EquipamentoPrincipalId) REFERENCES dbo.Equipamento(Id),

    -- Constraint para evitar que um equipamento seja seu próprio principal
    CONSTRAINT CK_Equipamento_Principal_Diferente
        CHECK (EquipamentoPrincipalId IS NULL OR EquipamentoPrincipalId <> Id)
);
GO

-- Unicidade de Tag (somente quando informada)
-- Principal: Tag única por Estação
CREATE UNIQUE INDEX UX_Equipamento_Principal_Tag ON dbo.Equipamento (EstacaoId, Tag)
WHERE Tag IS NOT NULL AND EquipamentoPrincipalId IS NULL;
GO

-- Componente: Tag única por EquipamentoPrincipal
CREATE UNIQUE INDEX UX_Equipamento_Componente_Tag ON dbo.Equipamento (EquipamentoPrincipalId, Tag)
WHERE Tag IS NOT NULL AND EquipamentoPrincipalId IS NOT NULL;
GO

/* Trigger para garantir “2 níveis”:
   - Se for componente (tem EquipamentoPrincipalId),
     o registro principal apontado NÃO pode ser componente (ou seja, principal deve ter EquipamentoPrincipalId IS NULL)
*/
CREATE OR ALTER TRIGGER dbo.TR_Equipamento_ValidaDoisNiveis
ON dbo.Equipamento
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Equipamento p ON p.Id = i.EquipamentoPrincipalId
        WHERE i.EquipamentoPrincipalId IS NOT NULL
          AND p.EquipamentoPrincipalId IS NOT NULL
    )
    BEGIN
        RAISERROR('EquipamentoPrincipalId deve apontar para um equipamento principal (EquipamentoPrincipalId IS NULL).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

/* =========================================================
   4) CARACTERISTICAS (EAV reforçado)
   ========================================================= */
CREATE TABLE dbo.CaracteristicaEquipamento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CaracteristicaEquipamento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    EquipamentoId UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(100) NOT NULL,

    -- texto livre
    Valor NVARCHAR(500) NULL,

    -- valor numérico para filtros/relatórios
    ValorNumerico DECIMAL(18,6) NULL,
    Unidade NVARCHAR(30) NULL, -- O que vamos guardar aqui ? Ex.: m, m³/h, kW, etc.

    -- 0=text, 1=decimal, 2=int, 3=bool, 4=date
    TipoValor TINYINT NOT NULL CONSTRAINT DF_Caracteristica_TipoValor DEFAULT 0,

    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Caracteristica_DataCriacao DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Caracteristica_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE,

    CONSTRAINT UQ_Caracteristica_Equipamento_Nome
        UNIQUE (EquipamentoId, Nome),

    CONSTRAINT CK_Caracteristica_TipoValor
        CHECK (TipoValor IN (0,1,2,3,4))
);
GO

CREATE INDEX IX_Caracteristica_Equipamento ON dbo.CaracteristicaEquipamento (EquipamentoId);
GO

/* =========================================================
   5) TABELAS ESPECÍFICAS (opcionais, mas úteis)
   - Usar quando precisar de validação forte e relatórios fáceis
   - (Em geral, serão COMPONENTES, mas o banco não obriga)
   ========================================================= */

CREATE TABLE dbo.Bomba (
    EquipamentoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Bomba PRIMARY KEY,
    Vazao DECIMAL(10,2) NULL,
    AlturaManometrica DECIMAL(10,2) NULL,
    Potencia DECIMAL(10,2) NULL,

    CONSTRAINT FK_Bomba_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Motor (
    EquipamentoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Motor PRIMARY KEY,
    Potencia DECIMAL(10,2) NULL,
    Tensao INT NULL,
    RPM INT NULL,

    CONSTRAINT FK_Motor_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.CLP (
    EquipamentoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CLP PRIMARY KEY,
    Marca NVARCHAR(100) NULL,
    Firmware NVARCHAR(50) NULL,

    CONSTRAINT FK_CLP_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Nobreak (
    EquipamentoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Nobreak PRIMARY KEY,
    PotenciaVA INT NULL,
    AutonomiaMinutos INT NULL,

    CONSTRAINT FK_Nobreak_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.MedidorVazao (
    EquipamentoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MedidorVazao PRIMARY KEY,
    Fabricante NVARCHAR(100) NULL,
    ModeloConversor NVARCHAR(100) NULL,
    ModeloSensor NVARCHAR(100) NULL,
    Diametro DECIMAL(10,2) NULL,
    FatorK DECIMAL(10,4) NULL,
    EscalaMaxima DECIMAL(10,2) NULL,

    CONSTRAINT FK_MedidorVazao_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.RegraPreventiva (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RegraPreventiva PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    EquipamentoId UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(150) NOT NULL,

    -- 1 = Dia, 2 = Semana, 3 = Mes, 4 = Ano
    UnidadePeriodo TINYINT NOT NULL,
    
    -- 0 = Aguardando Processamento, 1 = Em Processanmento
    [Status] TINYINT NOT NULL,
    Intervalo INT NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    ProximoProcessamento DATETIME2 NULL,
    UltimoProcessamento DATETIME2 NULL,
    Prioridade TINYINT NOT NULL CONSTRAINT DF_RegraPreventiva_Prioridade DEFAULT 1,
    Ativo BIT NOT NULL CONSTRAINT DF_RegraPreventiva_Ativo DEFAULT 1,

    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_RegraPreventiva_DataCriacao DEFAULT SYSDATETIME(),

    CONSTRAINT FK_RegraPreventiva_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
        ON DELETE CASCADE,

    CONSTRAINT CK_RegraPreventiva_UnidadePeriodo
        CHECK (UnidadePeriodo IN (1,2,3,4)),

    CONSTRAINT CK_RegraPreventiva_Intervalo
        CHECK (Intervalo > 0)
);
GO

CREATE INDEX IX_RegraPreventiva_EquipamentoId
ON dbo.RegraPreventiva (EquipamentoId);
GO

CREATE INDEX IX_RegraPreventiva_Ativo_ProximoProcessamento
ON dbo.RegraPreventiva (Ativo, ProximoProcessamento);
GO

CREATE TABLE dbo.RegraPreventivaServicoSolicitado (
    RegraPreventivaId UNIQUEIDENTIFIER NOT NULL,
    ServicoSolicitadoId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_RegraPreventivaServicoSolicitado
        PRIMARY KEY (RegraPreventivaId, ServicoSolicitadoId),

    CONSTRAINT FK_RegraPreventivaServicoSolicitado_Regra
        FOREIGN KEY (RegraPreventivaId)
        REFERENCES dbo.RegraPreventiva(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_RegraPreventivaServicoSolicitado_Servico
        FOREIGN KEY (ServicoSolicitadoId)
        REFERENCES dbo.ServicoSolicitado(Id)
);
GO

CREATE INDEX IX_RegraPreventivaServicoSolicitado_Servico
ON dbo.RegraPreventivaServicoSolicitado (ServicoSolicitadoId);
GO

/* =========================================================
   6) DOCUMENTO
   ========================================================= */
IF OBJECT_ID('dbo.DocumentoTag ', 'U') IS NOT NULL DROP TABLE dbo.DocumentoTag;
IF OBJECT_ID('dbo.TagDocumento ', 'U') IS NOT NULL DROP TABLE dbo.TagDocumento;
IF OBJECT_ID('dbo.DocumentoVinculo', 'U') IS NOT NULL DROP TABLE dbo.DocumentoVinculo;
IF OBJECT_ID('dbo.Documento', 'U') IS NOT NULL DROP TABLE dbo.Documento;
GO;

CREATE TABLE dbo.Documento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Documento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

    NomeOriginal NVARCHAR(255) NOT NULL,      -- nome como veio do upload
    NomeArmazenado NVARCHAR(255) NOT NULL,    -- nome no disco (ex: guid.ext)
    Extensao NVARCHAR(10) NOT NULL,           -- ".pdf", ".docx"...
    MimeType NVARCHAR(100) NULL,              -- "application/pdf"
    TamanhoBytes BIGINT NOT NULL,             -- peso real
    CaminhoRelativo NVARCHAR(500) NOT NULL,   -- ex: "estacoes/{id}/docs/..."
    HashSHA256 CHAR(64) NULL,                 -- opcional
    Tipo INT NULL,                            -- ex: "Manual", "Projeto", "Foto", "Laudo"
    Descricao NVARCHAR(500) NULL,
    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Documento_DataCriacao DEFAULT SYSDATETIME(),
    CriadoPor NVARCHAR(255) NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_Documento_Ativo DEFAULT 1
);
GO

-- busca comum por nome
CREATE INDEX IX_Documento_NomeOriginal ON dbo.Documento (NomeOriginal);
GO

-- útil para deduplicar/checar integridade (se você usar hash)
CREATE INDEX IX_Documento_HashSHA256 ON dbo.Documento (HashSHA256) WHERE HashSHA256 IS NOT NULL;
GO

CREATE TABLE dbo.DocumentoVinculo (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DocumentoVinculo PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    DocumentoId UNIQUEIDENTIFIER NOT NULL,
    EntidadeTipo INT NOT NULL,       -- "ESTACAO", "EQUIPAMENTO", "OS", etc.
    EntidadeId UNIQUEIDENTIFIER NOT NULL,
    Observacoes NVARCHAR(500) NULL,
    DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_DocumentoVinculo_DataCriacao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DocumentoVinculo_Documento
        FOREIGN KEY (DocumentoId) REFERENCES dbo.Documento(Id)
        ON DELETE CASCADE
);
GO

-- evita anexar o mesmo documento duas vezes na mesma entidade
CREATE UNIQUE INDEX UX_DocumentoVinculo_Unique
ON dbo.DocumentoVinculo (DocumentoId, EntidadeTipo, EntidadeId);
GO

-- performance para buscar anexos por estação/equipamento
CREATE INDEX IX_DocumentoVinculo_Entidade
ON dbo.DocumentoVinculo (EntidadeTipo, EntidadeId)
INCLUDE (DocumentoId, DataCriacao);
GO

CREATE TABLE dbo.TagDocumento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TagDocumento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Nome NVARCHAR(80) NOT NULL,
    NomeNormalizado NVARCHAR(80) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_TagDocumento_Ativo DEFAULT 1,
    CONSTRAINT UQ_TagDocumento_NomeNormalizado UNIQUE (NomeNormalizado)
);
GO

-- N:N Documento <-> Tag
CREATE TABLE dbo.DocumentoTag (
    DocumentoId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_DocumentoTag PRIMARY KEY (DocumentoId, TagId),

    CONSTRAINT FK_DocumentoTag_Documento
        FOREIGN KEY (DocumentoId) REFERENCES dbo.Documento(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_DocumentoTag_TagDocumento
        FOREIGN KEY (TagId) REFERENCES dbo.TagDocumento(Id)
        ON DELETE CASCADE
);
GO

CREATE INDEX IX_DocumentoTag_TagId ON dbo.DocumentoTag (TagId);
GO

/* =========================================================
   7) OS
   ========================================================= */
IF OBJECT_ID('dbo.OrdemServicoServicoExecutado', 'U') IS NOT NULL DROP TABLE dbo.OrdemServicoServicoExecutado;
IF OBJECT_ID('dbo.OrdemServicoServicoSolicitado', 'U') IS NOT NULL DROP TABLE dbo.OrdemServicoServicoSolicitado;

IF OBJECT_ID('dbo.OrdemServicoEquipamento', 'U') IS NOT NULL DROP TABLE dbo.OrdemServicoEquipamento;

IF OBJECT_ID('dbo.OrdemServico', 'U') IS NOT NULL DROP TABLE dbo.OrdemServico;
IF OBJECT_ID('dbo.Agendamento', 'U') IS NOT NULL DROP TABLE dbo.Agendamento;
IF OBJECT_ID('dbo.MotivoCancelamento', 'U') IS NOT NULL DROP TABLE dbo.MotivoCancelamento;
IF OBJECT_ID('dbo.StatusOrdemServico', 'U') IS NOT NULL DROP TABLE dbo.StatusOrdemServico;
IF OBJECT_ID('dbo.Regiao', 'U') IS NOT NULL DROP TABLE dbo.Regiao;

IF OBJECT_ID('dbo.ServicoSolicitado', 'U') IS NOT NULL DROP TABLE dbo.ServicoSolicitado;
IF OBJECT_ID('dbo.ServicoExecutado', 'U') IS NOT NULL DROP TABLE dbo.ServicoExecutado;
GO

/* -----------------------------
   Catálogo de serviços
----------------------------- */
CREATE TABLE dbo.ServicoSolicitado (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ServicoSolicitado PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Codigo NVARCHAR(10) NOT NULL,
    Descricao NVARCHAR(80) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_ServicoSolicitado_Ativo DEFAULT 1,
    CONSTRAINT UQ_ServicoSolicitado_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE dbo.ServicoExecutado (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ServicoExecutado PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Codigo NVARCHAR(10) NOT NULL,
    Descricao NVARCHAR(80) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_ServicoExecutado_Ativo DEFAULT 1,
    CONSTRAINT UQ_ServicoExecutado_Codigo UNIQUE (Codigo)
);
GO

/* -----------------------------
   Catálogos
----------------------------- */
CREATE TABLE dbo.StatusOrdemServico (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StatusOrdemServico PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(120) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_StatusOrdemServico_Ativo DEFAULT 1
);
GO

CREATE TABLE dbo.MotivoCancelamento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MotivoCancelamento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Codigo NVARCHAR(30) NOT NULL,
    Descricao NVARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_MotivoCancelamento_Ativo DEFAULT 1,
    CONSTRAINT UQ_MotivoCancelamento_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE dbo.Regiao (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Regiao PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_Regiao_Ativo DEFAULT 1,
    CONSTRAINT UQ_Regiao_Descricao UNIQUE (Descricao)
);
GO

/* -----------------------------
   Agendamento
----------------------------- */
CREATE TABLE dbo.Agendamento (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Agendamento PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(255) NULL,

    -- 0 = Nenhum, 1 = Semanal, 2 = Mensal
    TipoRecorrencia TINYINT NOT NULL CONSTRAINT DF_Agendamento_TipoRecorrencia DEFAULT 0,
    DiaSemana TINYINT NULL, -- 1..7
    DiaMes TINYINT NULL,    -- 1..31

    DiasPrevios INT NULL,
    LimiteAgendamento INT NULL,

    AgendamentoFixo BIT NOT NULL CONSTRAINT DF_Agendamento_Fixo DEFAULT 0,
    DataAgendamentoFixo DATETIME2 NULL,

    Ativo BIT NOT NULL CONSTRAINT DF_Agendamento_Ativo DEFAULT 1,

    CONSTRAINT CK_Agendamento_TipoRecorrencia CHECK (TipoRecorrencia IN (0,1,2)),
    CONSTRAINT CK_Agendamento_DiaSemana CHECK (DiaSemana IS NULL OR (DiaSemana BETWEEN 1 AND 7)),
    CONSTRAINT CK_Agendamento_DiaMes CHECK (DiaMes IS NULL OR (DiaMes BETWEEN 1 AND 31))
);
GO

CREATE INDEX IX_Agendamento_Ativo ON dbo.Agendamento(Ativo);
GO

/* -----------------------------
   Ordem de Serviço
----------------------------- */
CREATE TABLE dbo.OrdemServico (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrdemServico PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

    Codigo NVARCHAR(255) NULL,            -- ex: NumeroOs/Ano/SubOs = 8/2026/3
    Numero INT NULL,
    SubOs INT NULL,
    Ano INT NOT NULL CONSTRAINT DF_OrdemServico_Ano DEFAULT YEAR(SYSDATETIME()),

    EstacaoId UNIQUEIDENTIFIER NOT NULL,

    AgendamentoId UNIQUEIDENTIFIER NULL,
    StatusId UNIQUEIDENTIFIER NOT NULL,
    MotivoCancelamentoId UNIQUEIDENTIFIER NULL,
    RegiaoId UNIQUEIDENTIFIER NULL,

    -- 0=Corretiva, 1=Preventiva, 2=Preditiva, 3=Inspecao
    TipoOS TINYINT NOT NULL CONSTRAINT DF_OrdemServico_TipoOS DEFAULT 0,

    -- 0=Manual, 1=Agendamento, 2=Alarme, 3=Importacao
    Origem TINYINT NOT NULL CONSTRAINT DF_OrdemServico_Origem DEFAULT 0,

    -- 0=Baixa, 1=Media, 2=Alta, 3=Critica
    Prioridade TINYINT NOT NULL CONSTRAINT DF_OrdemServico_Prioridade DEFAULT 1,

    -- Cliente
    Email NVARCHAR(255) NULL,
    Nome NVARCHAR(255) NULL,
    Telefone NVARCHAR(15) NULL,
    NumeroDocumento NVARCHAR(30) NULL,

    DataAgendamento DATETIME2 NULL,
    DataCancelamento DATETIME2 NULL,
    DataDespacho DATETIME2 NULL,
    DataDespachoProgramado DATETIME2 NULL,
    DataFinalizacao DATETIME2 NULL,
    DataParalisacao DATETIME2 NULL,
    DataSolicitacao DATETIME2 NOT NULL CONSTRAINT DF_OrdemServico_DataSolicitacao DEFAULT SYSDATETIME(),
    DataInicioExecucao DATETIME2 NULL,
    DataPrevista DATETIME2 NULL,   -- prazo de conclusão da OS
    CustoTotal DECIMAL(18, 2) NULL,   -- custo acumulado da OS
    Observacao NVARCHAR(MAX) NULL,

    -- (Opcional) controle lógico
    Ativo BIT NOT NULL CONSTRAINT DF_OrdemServico_Ativo DEFAULT 1,
    IsAgendada BIT NULL,
    IsProgramada BIT NULL,

    CONSTRAINT FK_OrdemServico_Estacao
        FOREIGN KEY (EstacaoId) REFERENCES dbo.Estacao(Id),

    CONSTRAINT FK_OrdemServico_Agendamento
        FOREIGN KEY (AgendamentoId) REFERENCES dbo.Agendamento(Id),

    CONSTRAINT FK_OrdemServico_Status
        FOREIGN KEY (StatusId) REFERENCES dbo.StatusOrdemServico(Id),

    CONSTRAINT FK_OrdemServico_MotivoCancelamento
        FOREIGN KEY (MotivoCancelamentoId) REFERENCES dbo.MotivoCancelamento(Id),

    CONSTRAINT FK_OrdemServico_Regiao
        FOREIGN KEY (RegiaoId) REFERENCES dbo.Regiao(Id),

    CONSTRAINT CK_OrdemServico_Datas CHECK
    (
        (DataFinalizacao IS NULL OR DataInicioExecucao IS NOT NULL)
        AND (DataCancelamento IS NULL OR MotivoCancelamentoId IS NOT NULL)
    )
);
GO

-- Índices para telas
CREATE INDEX IX_OrdemServico_Status_Data ON dbo.OrdemServico (StatusId, DataSolicitacao DESC);
CREATE INDEX IX_OrdemServico_Estacao_Data ON dbo.OrdemServico (EstacaoId, DataSolicitacao DESC);
CREATE INDEX IX_OrdemServico_Ano ON dbo.OrdemServico (Ano);
CREATE INDEX IX_OrdemServico_DataPrevista ON dbo.OrdemServico (DataPrevista) WHERE DataPrevista IS NOT NULL;
GO

/* -----------------------------
   Vínculo OS -> Equipamentos (1 OS pode ter N equipamentos)
   - modelo N:N (correto no domínio), mas você usa como 1:N na UI
----------------------------- */
CREATE TABLE dbo.OrdemServicoEquipamento (
    OrdemServicoId UNIQUEIDENTIFIER NOT NULL,
    EquipamentoId UNIQUEIDENTIFIER NOT NULL,

    -- opcional, ajuda na UI (ex: "Equipamento principal da OS")
    Principal BIT NOT NULL CONSTRAINT DF_OS_Equipamento_Principal DEFAULT 0,

    Observacoes NVARCHAR(500) NULL,

    CONSTRAINT PK_OrdemServicoEquipamento PRIMARY KEY (OrdemServicoId, EquipamentoId),

    CONSTRAINT FK_OS_Equipamento_OS
        FOREIGN KEY (OrdemServicoId) REFERENCES dbo.OrdemServico(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_OS_Equipamento_Equipamento
        FOREIGN KEY (EquipamentoId) REFERENCES dbo.Equipamento(Id)
);
GO

CREATE INDEX IX_OS_Equipamento_EquipamentoId ON dbo.OrdemServicoEquipamento (EquipamentoId);
GO

/* -----------------------------
   Serviços solicitados (N:N)
----------------------------- */
CREATE TABLE dbo.OrdemServicoServicoSolicitado (
    OrdemServicoId UNIQUEIDENTIFIER NOT NULL,
    ServicoSolicitadoId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_OS_ServicoSolicitado PRIMARY KEY (OrdemServicoId, ServicoSolicitadoId),

    CONSTRAINT FK_OS_ServicoSolicitado_OS
        FOREIGN KEY (OrdemServicoId) REFERENCES dbo.OrdemServico(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_OS_ServicoSolicitado_Catalogo
        FOREIGN KEY (ServicoSolicitadoId) REFERENCES dbo.ServicoSolicitado(Id)
);
GO

CREATE INDEX IX_OS_ServicoSolicitado_Servico ON dbo.OrdemServicoServicoSolicitado (ServicoSolicitadoId);
GO

/* -----------------------------
   Serviços executados (N:N)
----------------------------- */
CREATE TABLE dbo.OrdemServicoServicoExecutado (
    OrdemServicoId UNIQUEIDENTIFIER NOT NULL,
    ServicoExecutadoId UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_OS_ServicoExecutado PRIMARY KEY (OrdemServicoId, ServicoExecutadoId),

    CONSTRAINT FK_OS_ServicoExecutado_OS
        FOREIGN KEY (OrdemServicoId) REFERENCES dbo.OrdemServico(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_OS_ServicoExecutado_Catalogo
        FOREIGN KEY (ServicoExecutadoId) REFERENCES dbo.ServicoExecutado(Id)
);
GO

CREATE INDEX IX_OS_ServicoExecutado_Servico ON dbo.OrdemServicoServicoExecutado (ServicoExecutadoId);
GO
/* =========================================================
   8) FUNCIONARIO
     - Tabelas de catálogo + tabela principal com FKs
   ========================================================= */
IF OBJECT_ID('dbo.Funcionario', 'U')       IS NOT NULL DROP TABLE dbo.Funcionario;
IF OBJECT_ID('dbo.Cargo', 'U')             IS NOT NULL DROP TABLE dbo.Cargo;
IF OBJECT_ID('dbo.Setor', 'U')             IS NOT NULL DROP TABLE dbo.Setor;
IF OBJECT_ID('dbo.TipoFuncionario', 'U')   IS NOT NULL DROP TABLE dbo.TipoFuncionario;
GO

/* -----------------------------
   Cargo
----------------------------- */
CREATE TABLE dbo.Cargo (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Cargo PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_Cargo_Ativo DEFAULT 1,

    CONSTRAINT UQ_Cargo_Descricao UNIQUE (Descricao)
);
GO

CREATE INDEX IX_Cargo_Ativo ON dbo.Cargo(Ativo);
GO

/* -----------------------------
   Setor
----------------------------- */
CREATE TABLE dbo.Setor (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Setor PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_Setor_Ativo DEFAULT 1,

    CONSTRAINT UQ_Setor_Descricao UNIQUE (Descricao)
);
GO

CREATE INDEX IX_Setor_Ativo ON dbo.Setor(Ativo);
GO

/* -----------------------------
   TipoFuncionario
   Ex.: Operador, Supervisor, Técnico, Engenheiro...
----------------------------- */
CREATE TABLE dbo.TipoFuncionario (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TipoFuncionario PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Descricao NVARCHAR(255) NOT NULL,
    Ativo BIT NOT NULL CONSTRAINT DF_TipoFuncionario_Ativo DEFAULT 1,

    CONSTRAINT UQ_TipoFuncionario_Descricao UNIQUE (Descricao)
);
GO

CREATE INDEX IX_TipoFuncionario_Ativo ON dbo.TipoFuncionario(Ativo);
GO

/* -----------------------------
   Funcionario
----------------------------- */
CREATE TABLE dbo.Funcionario (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Funcionario PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

    -- FKs obrigatórias
    CargoId UNIQUEIDENTIFIER NOT NULL,
    SetorId UNIQUEIDENTIFIER NOT NULL,
    TipoFuncionarioId UNIQUEIDENTIFIER NOT NULL,

    -- Identificação
    Codigo NVARCHAR(255) NOT NULL,
    Nome NVARCHAR(255) NOT NULL,
    Terceirizado BIT NOT NULL CONSTRAINT DF_Funcionario_Terceirizado DEFAULT 0,
    Ativo BIT NOT NULL CONSTRAINT DF_Funcionario_Ativo DEFAULT 1,

    CONSTRAINT FK_Funcionario_Cargo FOREIGN KEY (CargoId) REFERENCES dbo.Cargo(Id),
    CONSTRAINT FK_Funcionario_Setor FOREIGN KEY (SetorId) REFERENCES dbo.Setor(Id),
    CONSTRAINT FK_Funcionario_TipoFuncionario FOREIGN KEY (TipoFuncionarioId) REFERENCES dbo.TipoFuncionario(Id)
);
GO

-- Índices para consultas frequentes
CREATE INDEX IX_Funcionario_CargoId            ON dbo.Funcionario(CargoId);
CREATE INDEX IX_Funcionario_SetorId            ON dbo.Funcionario(SetorId);
CREATE INDEX IX_Funcionario_TipoFuncionarioId  ON dbo.Funcionario(TipoFuncionarioId);
CREATE INDEX IX_Funcionario_Ativo              ON dbo.Funcionario(Ativo);
CREATE INDEX IX_Funcionario_Nome               ON dbo.Funcionario(Nome);
GO