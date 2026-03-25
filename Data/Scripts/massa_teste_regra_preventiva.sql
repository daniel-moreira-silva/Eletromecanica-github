SET NOCOUNT ON;

DECLARE 
    @TipoEstacaoElevatoria UNIQUEIDENTIFIER,
    @TipoEstacaoReservatorio UNIQUEIDENTIFIER,
    @TipoEstacaoBooster UNIQUEIDENTIFIER,

    @TipoEqConjuntoMotobomba UNIQUEIDENTIFIER,
    @TipoEqPainel UNIQUEIDENTIFIER,
    @TipoEqBomba UNIQUEIDENTIFIER,
    @TipoEqMotor UNIQUEIDENTIFIER,
    @TipoEqCLP UNIQUEIDENTIFIER,
    @TipoEqMedidorVazao UNIQUEIDENTIFIER,
    @TipoEqNobreak UNIQUEIDENTIFIER,

    @ServicoPreventiva UNIQUEIDENTIFIER,
    @ServicoLubrificar UNIQUEIDENTIFIER,
    @ServicoValidarSensores UNIQUEIDENTIFIER,
    @ServicoManutencaoMotor UNIQUEIDENTIFIER,
    @ServicoManutencaoBomba UNIQUEIDENTIFIER,

    @StatusSolicitada UNIQUEIDENTIFIER,
    @StatusEmAndamento UNIQUEIDENTIFIER,
    @RegiaoNorte UNIQUEIDENTIFIER,
    @RegiaoSul UNIQUEIDENTIFIER,

    @EstacaoEAB01 UNIQUEIDENTIFIER,
    @EstacaoRES01 UNIQUEIDENTIFIER,
    @EstacaoBST01 UNIQUEIDENTIFIER,

    @EqCMB01 UNIQUEIDENTIFIER,
    @EqBomba01 UNIQUEIDENTIFIER,
    @EqMotor01 UNIQUEIDENTIFIER,
    @EqPainel01 UNIQUEIDENTIFIER,
    @EqClp01 UNIQUEIDENTIFIER,

    @EqCMB02 UNIQUEIDENTIFIER,
    @EqBomba02 UNIQUEIDENTIFIER,
    @EqMotor02 UNIQUEIDENTIFIER,
    @EqMedidor01 UNIQUEIDENTIFIER,

    @EqBooster01 UNIQUEIDENTIFIER,
    @EqNobreak01 UNIQUEIDENTIFIER,

    @Regra1 UNIQUEIDENTIFIER,
    @Regra2 UNIQUEIDENTIFIER,
    @Regra3 UNIQUEIDENTIFIER,
    @Regra4 UNIQUEIDENTIFIER,

    @OS1 UNIQUEIDENTIFIER,
    @OS2 UNIQUEIDENTIFIER;

/* =========================================================
   1) BUSCA IDS DE APOIO
   ========================================================= */
SELECT @TipoEstacaoElevatoria = Id FROM dbo.TipoEstacao WHERE Nome = N'Elevatórias de Água';
SELECT @TipoEstacaoReservatorio = Id FROM dbo.TipoEstacao WHERE Nome = N'Reservatórios';
SELECT @TipoEstacaoBooster = Id FROM dbo.TipoEstacao WHERE Nome = N'Booster';

SELECT @TipoEqConjuntoMotobomba = Id FROM dbo.TipoEquipamento WHERE Nome = N'Conjunto Motobomba';
SELECT @TipoEqPainel           = Id FROM dbo.TipoEquipamento WHERE Nome = N'Painel de Acionamento/Telemetria';
SELECT @TipoEqBomba            = Id FROM dbo.TipoEquipamento WHERE Nome = N'Bomba';
SELECT @TipoEqMotor            = Id FROM dbo.TipoEquipamento WHERE Nome = N'Motor Elétrico';
SELECT @TipoEqCLP              = Id FROM dbo.TipoEquipamento WHERE Nome = N'CLP';
SELECT @TipoEqMedidorVazao     = Id FROM dbo.TipoEquipamento WHERE Nome = N'Medidor de Vazão';
SELECT @TipoEqNobreak          = Id FROM dbo.TipoEquipamento WHERE Nome = N'Nobreak';

SELECT @ServicoPreventiva      = Id FROM dbo.ServicoSolicitado WHERE Codigo = N'S230';
SELECT @ServicoLubrificar      = Id FROM dbo.ServicoSolicitado WHERE Codigo = N'S234';
SELECT @ServicoValidarSensores = Id FROM dbo.ServicoSolicitado WHERE Codigo = N'S239';
SELECT @ServicoManutencaoMotor = Id FROM dbo.ServicoSolicitado WHERE Codigo = N'S245';
SELECT @ServicoManutencaoBomba = Id FROM dbo.ServicoSolicitado WHERE Codigo = N'S246';

SELECT @StatusSolicitada = Id FROM dbo.StatusOrdemServico WHERE Descricao = N'Solicitada';
SELECT @StatusEmAndamento = Id FROM dbo.StatusOrdemServico WHERE Descricao = N'Em Andamento';

SELECT @RegiaoNorte = Id FROM dbo.Regiao WHERE Descricao = N'Região Norte';
SELECT @RegiaoSul   = Id FROM dbo.Regiao WHERE Descricao = N'Região Sul';

/* =========================================================
   2) ESTAÇÕES
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Estacao WHERE Nome = N'EAB-01' AND TipoEstacaoId = @TipoEstacaoElevatoria)
BEGIN
    INSERT INTO dbo.Estacao
    (
        TipoEstacaoId, Nome, Observacoes,
        Endereco, Bairro, Lat, Long, Numero, Complemento, PontoReferencia,
        Ativo
    )
    VALUES
    (
        @TipoEstacaoElevatoria, N'EAB-01', N'Elevatória de água para testes de preventiva',
        N'Rua das Bombas', N'Centro', N'-23.5501', N'-46.6321', N'100', N'Pátio interno', N'Próximo ao reservatório',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Estacao WHERE Nome = N'RES-01' AND TipoEstacaoId = @TipoEstacaoReservatorio)
BEGIN
    INSERT INTO dbo.Estacao
    (
        TipoEstacaoId, Nome, Observacoes,
        Endereco, Bairro, Lat, Long, Numero, Complemento, PontoReferencia,
        Ativo
    )
    VALUES
    (
        @TipoEstacaoReservatorio, N'RES-01', N'Reservatório com instrumentação para testes',
        N'Avenida da Água', N'Jardim Azul', N'-23.5510', N'-46.6300', N'250', N'Área externa', N'Ao lado da elevatória',
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Estacao WHERE Nome = N'BST-01' AND TipoEstacaoId = @TipoEstacaoBooster)
BEGIN
    INSERT INTO dbo.Estacao
    (
        TipoEstacaoId, Nome, Observacoes,
        Endereco, Bairro, Lat, Long, Numero, Complemento, PontoReferencia,
        Ativo
    )
    VALUES
    (
        @TipoEstacaoBooster, N'BST-01', N'Booster para testes rápidos',
        N'Rua Pressurização', N'Parque Sul', N'-23.5590', N'-46.6280', N'45', N'Casa de bombas', N'Próximo à praça',
        1
    );
END;

SELECT @EstacaoEAB01 = Id FROM dbo.Estacao WHERE Nome = N'EAB-01';
SELECT @EstacaoRES01 = Id FROM dbo.Estacao WHERE Nome = N'RES-01';
SELECT @EstacaoBST01 = Id FROM dbo.Estacao WHERE Nome = N'BST-01';

/* =========================================================
   3) EQUIPAMENTOS - EAB-01
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'CMB-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoEAB01, @TipoEqConjuntoMotobomba, NULL,
        N'CMB-01', N'CMB-01', N'KSB', N'Megabloc', N'CMBEAB0101', N'Conjunto principal da elevatória', 1
    );
END;

SELECT @EqCMB01 = Id 
FROM dbo.Equipamento 
WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'CMB-01';

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqCMB01 AND Nome = N'Bomba CMB-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoEAB01, @TipoEqBomba, @EqCMB01,
        N'Bomba CMB-01', N'BMB-01', N'KSB', N'HG-200', N'BMBEAB0101', N'Bomba do conjunto CMB-01', 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqCMB01 AND Nome = N'Motor CMB-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoEAB01, @TipoEqMotor, @EqCMB01,
        N'Motor CMB-01', N'MTR-01', N'WEG', N'W22', N'MTREAB0101', N'Motor elétrico do conjunto CMB-01', 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'Painel EAB-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoEAB01, @TipoEqPainel, NULL,
        N'Painel EAB-01', N'PAINEL-EAB-01', N'Schneider', N'PTA-500', N'PAIEAB0101', N'Painel principal da estação', 1
    );
END;

SELECT @EqBomba01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'Bomba CMB-01';
SELECT @EqMotor01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'Motor CMB-01';
SELECT @EqPainel01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'Painel EAB-01';

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqPainel01 AND Nome = N'CLP Painel EAB-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoEAB01, @TipoEqCLP, @EqPainel01,
        N'CLP Painel EAB-01', N'CLP-EAB-01', N'Siemens', N'S7-1200', N'CLPEAB0101', N'CLP responsável pelo acionamento', 1
    );
END;

SELECT @EqClp01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoEAB01 AND Nome = N'CLP Painel EAB-01';

/* =========================================================
   4) EQUIPAMENTOS - RES-01
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EstacaoId = @EstacaoRES01 AND Nome = N'CMB-02')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoRES01, @TipoEqConjuntoMotobomba, NULL,
        N'CMB-02', N'CMB-02', N'KSB', N'Megabloc', N'CMBRES0101', N'Conjunto reserva do reservatório', 1
    );
END;

SELECT @EqCMB02 = Id 
FROM dbo.Equipamento 
WHERE EstacaoId = @EstacaoRES01 AND Nome = N'CMB-02';

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqCMB02 AND Nome = N'Bomba CMB-02')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoRES01, @TipoEqBomba, @EqCMB02,
        N'Bomba CMB-02', N'BMB-02', N'KSB', N'HG-250', N'BMBRES0101', N'Bomba do conjunto CMB-02', 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqCMB02 AND Nome = N'Motor CMB-02')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoRES01, @TipoEqMotor, @EqCMB02,
        N'Motor CMB-02', N'MTR-02', N'WEG', N'W22', N'MTRRES0101', N'Motor do conjunto CMB-02', 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EstacaoId = @EstacaoRES01 AND Nome = N'Medidor Vazão RES-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoRES01, @TipoEqMedidorVazao, NULL,
        N'Medidor Vazão RES-01', N'MV-RES-01', N'Endress+Hauser', N'Promag', N'MVRES0101', N'Medidor principal do reservatório', 1
    );
END;

SELECT @EqBomba02   = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoRES01 AND Nome = N'Bomba CMB-02';
SELECT @EqMotor02   = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoRES01 AND Nome = N'Motor CMB-02';
SELECT @EqMedidor01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoRES01 AND Nome = N'Medidor Vazão RES-01';

/* =========================================================
   5) EQUIPAMENTOS - BST-01
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EstacaoId = @EstacaoBST01 AND Nome = N'Booster BST-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoBST01, @TipoEqConjuntoMotobomba, NULL,
        N'Booster BST-01', N'BST-01-CMB', N'Grundfos', N'Hydro', N'BST0101', N'Booster de teste', 1
    );
END;

SELECT @EqBooster01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoBST01 AND Nome = N'Booster BST-01';

IF NOT EXISTS (SELECT 1 FROM dbo.Equipamento WHERE EquipamentoPrincipalId = @EqBooster01 AND Nome = N'Nobreak Booster BST-01')
BEGIN
    INSERT INTO dbo.Equipamento
    (
        EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId,
        Nome, Tag, Fabricante, Modelo, NumeroSerie, Observacoes, Ativo
    )
    VALUES
    (
        @EstacaoBST01, @TipoEqNobreak, @EqBooster01,
        N'Nobreak Booster BST-01', N'NOB-BST-01', N'Intelbras', N'XNB 1800', N'NOBBST0101', N'Nobreak do painel do booster', 1
    );
END;

SELECT @EqNobreak01 = Id FROM dbo.Equipamento WHERE EstacaoId = @EstacaoBST01 AND Nome = N'Nobreak Booster BST-01';

/* =========================================================
   6) REGRAS PREVENTIVAS
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqCMB01 AND Nome = N'Preventiva mensal CMB-01')
BEGIN
    INSERT INTO dbo.RegraPreventiva
    (
        EquipamentoId, Nome, UnidadePeriodo, Intervalo,
        DataInicio, ProximaExecucao, UltimaExecucao,
        Prioridade, Ativo
    )
    VALUES
    (
        @EqCMB01, N'Preventiva mensal CMB-01', 3, 1,
        DATEFROMPARTS(2026, 1, 1),
        DATEADD(DAY, -10, SYSDATETIME()),
        DATEADD(MONTH, -1, SYSDATETIME()),
        2, 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqBomba01 AND Nome = N'Lubrificação bomba CMB-01')
BEGIN
    INSERT INTO dbo.RegraPreventiva
    (
        EquipamentoId, Nome, UnidadePeriodo, Intervalo,
        DataInicio, ProximaExecucao, UltimaExecucao,
        Prioridade, Ativo
    )
    VALUES
    (
        @EqBomba01, N'Lubrificação bomba CMB-01', 2, 2,
        DATEFROMPARTS(2026, 2, 1),
        DATEADD(DAY, 5, SYSDATETIME()),
        DATEADD(DAY, -9, SYSDATETIME()),
        1, 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqMedidor01 AND Nome = N'Validação medidor de vazão RES-01')
BEGIN
    INSERT INTO dbo.RegraPreventiva
    (
        EquipamentoId, Nome, UnidadePeriodo, Intervalo,
        DataInicio, ProximaExecucao, UltimaExecucao,
        Prioridade, Ativo
    )
    VALUES
    (
        @EqMedidor01, N'Validação medidor de vazão RES-01', 3, 1,
        DATEFROMPARTS(2026, 3, 1),
        DATEADD(DAY, 2, SYSDATETIME()),
        NULL,
        3, 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqMotor02 AND Nome = N'Revisão anual motor CMB-02')
BEGIN
    INSERT INTO dbo.RegraPreventiva
    (
        EquipamentoId, Nome, UnidadePeriodo, Intervalo,
        DataInicio, ProximaExecucao, UltimaExecucao,
        Prioridade, Ativo
    )
    VALUES
    (
        @EqMotor02, N'Revisão anual motor CMB-02', 4, 1,
        DATEFROMPARTS(2025, 6, 1),
        DATEADD(MONTH, 4, SYSDATETIME()),
        DATEADD(MONTH, -8, SYSDATETIME()),
        2, 0
    );
END;

SELECT @Regra1 = Id FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqCMB01 AND Nome = N'Preventiva mensal CMB-01';
SELECT @Regra2 = Id FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqBomba01 AND Nome = N'Lubrificação bomba CMB-01';
SELECT @Regra3 = Id FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqMedidor01 AND Nome = N'Validação medidor de vazão RES-01';
SELECT @Regra4 = Id FROM dbo.RegraPreventiva WHERE EquipamentoId = @EqMotor02 AND Nome = N'Revisão anual motor CMB-02';

/* =========================================================
   7) VÍNCULO REGRA -> SERVIÇOS SOLICITADOS
   ========================================================= */
IF NOT EXISTS (
    SELECT 1 FROM dbo.RegraPreventivaServicoSolicitado
    WHERE RegraPreventivaId = @Regra1 AND ServicoSolicitadoId = @ServicoPreventiva
)
BEGIN
    INSERT INTO dbo.RegraPreventivaServicoSolicitado (RegraPreventivaId, ServicoSolicitadoId)
    VALUES (@Regra1, @ServicoPreventiva);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.RegraPreventivaServicoSolicitado
    WHERE RegraPreventivaId = @Regra1 AND ServicoSolicitadoId = @ServicoManutencaoBomba
)
BEGIN
    INSERT INTO dbo.RegraPreventivaServicoSolicitado (RegraPreventivaId, ServicoSolicitadoId)
    VALUES (@Regra1, @ServicoManutencaoBomba);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.RegraPreventivaServicoSolicitado
    WHERE RegraPreventivaId = @Regra2 AND ServicoSolicitadoId = @ServicoLubrificar
)
BEGIN
    INSERT INTO dbo.RegraPreventivaServicoSolicitado (RegraPreventivaId, ServicoSolicitadoId)
    VALUES (@Regra2, @ServicoLubrificar);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.RegraPreventivaServicoSolicitado
    WHERE RegraPreventivaId = @Regra3 AND ServicoSolicitadoId = @ServicoValidarSensores
)
BEGIN
    INSERT INTO dbo.RegraPreventivaServicoSolicitado (RegraPreventivaId, ServicoSolicitadoId)
    VALUES (@Regra3, @ServicoValidarSensores);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.RegraPreventivaServicoSolicitado
    WHERE RegraPreventivaId = @Regra4 AND ServicoSolicitadoId = @ServicoManutencaoMotor
)
BEGIN
    INSERT INTO dbo.RegraPreventivaServicoSolicitado (RegraPreventivaId, ServicoSolicitadoId)
    VALUES (@Regra4, @ServicoManutencaoMotor);
END;

/* =========================================================
   8) ORDENS DE SERVIÇO DE EXEMPLO
   ========================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.OrdemServico WHERE Codigo = N'OS-TESTE-0001')
BEGIN
    INSERT INTO dbo.OrdemServico
    (
        Codigo, Numero, SubOs, Ano,
        EstacaoId, StatusId, RegiaoId,
        TipoOS, Origem, Prioridade,
        DataSolicitacao, DataInicioExecucao,
        Observacao, Ativo
    )
    VALUES
    (
        N'OS-TESTE-0001', 1, 0, YEAR(SYSDATETIME()),
        @EstacaoEAB01, @StatusSolicitada, @RegiaoNorte,
        1, 1, 2,
        DATEADD(DAY, -1, SYSDATETIME()), NULL,
        N'OS criada para testar regra preventiva do conjunto motobomba CMB-01.', 1
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.OrdemServico WHERE Codigo = N'OS-TESTE-0002')
BEGIN
    INSERT INTO dbo.OrdemServico
    (
        Codigo, Numero, SubOs, Ano,
        EstacaoId, StatusId, RegiaoId,
        TipoOS, Origem, Prioridade,
        DataSolicitacao, DataInicioExecucao,
        Observacao, Ativo
    )
    VALUES
    (
        N'OS-TESTE-0002', 2, 0, YEAR(SYSDATETIME()),
        @EstacaoRES01, @StatusEmAndamento, @RegiaoSul,
        1, 1, 3,
        DATEADD(DAY, -3, SYSDATETIME()), DATEADD(DAY, -2, SYSDATETIME()),
        N'OS em andamento para teste de preventiva do medidor de vazão.', 1
    );
END;

SELECT @OS1 = Id FROM dbo.OrdemServico WHERE Codigo = N'OS-TESTE-0001';
SELECT @OS2 = Id FROM dbo.OrdemServico WHERE Codigo = N'OS-TESTE-0002';

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoEquipamento
    WHERE OrdemServicoId = @OS1 AND EquipamentoId = @EqCMB01
)
BEGIN
    INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId, EquipamentoId, Principal, Observacoes)
    VALUES (@OS1, @EqCMB01, 1, N'Equipamento principal da OS de preventiva');
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoEquipamento
    WHERE OrdemServicoId = @OS1 AND EquipamentoId = @EqBomba01
)
BEGIN
    INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId, EquipamentoId, Principal, Observacoes)
    VALUES (@OS1, @EqBomba01, 0, N'Componente associado à preventiva');
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoEquipamento
    WHERE OrdemServicoId = @OS2 AND EquipamentoId = @EqMedidor01
)
BEGIN
    INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId, EquipamentoId, Principal, Observacoes)
    VALUES (@OS2, @EqMedidor01, 1, N'Equipamento alvo da validação');
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoServicoSolicitado
    WHERE OrdemServicoId = @OS1 AND ServicoSolicitadoId = @ServicoPreventiva
)
BEGIN
    INSERT INTO dbo.OrdemServicoServicoSolicitado (OrdemServicoId, ServicoSolicitadoId)
    VALUES (@OS1, @ServicoPreventiva);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoServicoSolicitado
    WHERE OrdemServicoId = @OS1 AND ServicoSolicitadoId = @ServicoManutencaoBomba
)
BEGIN
    INSERT INTO dbo.OrdemServicoServicoSolicitado (OrdemServicoId, ServicoSolicitadoId)
    VALUES (@OS1, @ServicoManutencaoBomba);
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.OrdemServicoServicoSolicitado
    WHERE OrdemServicoId = @OS2 AND ServicoSolicitadoId = @ServicoValidarSensores
)
BEGIN
    INSERT INTO dbo.OrdemServicoServicoSolicitado (OrdemServicoId, ServicoSolicitadoId)
    VALUES (@OS2, @ServicoValidarSensores);
END;

PRINT 'Massa de teste criada com sucesso.';