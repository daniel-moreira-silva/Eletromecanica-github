SET NOCOUNT ON;
BEGIN TRANSACTION;

/* =====================================================================
   REFERÊNCIAS — GUIDs fixos do carga.sql
   ===================================================================== */
DECLARE
    @stSolicitada   UNIQUEIDENTIFIER = 'E4011FE2-1CFC-46A4-9BB7-02F1E154C00E',
    @stIniciada     UNIQUEIDENTIFIER = '2CF3AF37-18CD-4DF6-BE22-91B4DE792B25',
    @stAndamento    UNIQUEIDENTIFIER = '132AE55D-A544-439E-8D07-CDC643CE4E78',
    @stFinalizada   UNIQUEIDENTIFIER = '8330AEC6-5E4D-4B67-8620-0C13FAEC04C5',
    @stCancelada    UNIQUEIDENTIFIER = 'FC34601C-803F-4DA4-B4C9-0843AA1E8E4A',
    @stPendente     UNIQUEIDENTIFIER = '642A2B4C-1B28-4857-9FB9-C652B889392B';

/* =====================================================================
   1) ESTAÇÕES
   ===================================================================== */
DECLARE
    @idEstElev  UNIQUEIDENTIFIER = NEWID(),
    @idEstBoost UNIQUEIDENTIFIER = NEWID();

DECLARE @tipoElev  UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.TipoEstacao WHERE Nome = N'Elevatórias de Água');
DECLARE @tipoBoost UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.TipoEstacao WHERE Nome = N'Booster');

INSERT INTO dbo.Estacao (Id, TipoEstacaoId, Nome, Endereco, Bairro, Numero, Ativo)
VALUES
    (@idEstElev,  @tipoElev,  N'Elevatória Jardim Sul',  N'Rua das Palmeiras', N'Jardim Sul',    N'100', 1),
    (@idEstBoost, @tipoBoost, N'Booster São Sebastião',  N'Av. Central',       N'São Sebastião', N'250', 1);

/* =====================================================================
   2) EQUIPAMENTOS PRINCIPAIS
   ===================================================================== */
DECLARE
    @idEq1 UNIQUEIDENTIFIER = NEWID(),
    @idEq2 UNIQUEIDENTIFIER = NEWID(),
    @idEq3 UNIQUEIDENTIFIER = NEWID(),
    @idEq4 UNIQUEIDENTIFIER = NEWID();

DECLARE @tipoMotobomba UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.TipoEquipamento WHERE Nome = N'Conjunto Motobomba');
DECLARE @tipoPainel    UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.TipoEquipamento WHERE Nome = N'Painel de Acionamento/Telemetria');

INSERT INTO dbo.Equipamento (Id, EstacaoId, TipoEquipamentoId, EquipamentoPrincipalId, Nome, Tag, Ativo)
VALUES
    (@idEq1, @idEstElev,  @tipoMotobomba, NULL, N'Conjunto Motobomba 01', N'CMB-01', 1),
    (@idEq2, @idEstElev,  @tipoMotobomba, NULL, N'Conjunto Motobomba 02', N'CMB-02', 1),
    (@idEq3, @idEstBoost, @tipoMotobomba, NULL, N'Conjunto Motobomba 01', N'CMB-03', 1),
    (@idEq4, @idEstBoost, @tipoPainel,    NULL, N'Painel de Acionamento', N'PAI-01', 1);

/* =====================================================================
   3) ORDENS DE SERVIÇO
   ===================================================================== */
DECLARE @hoje DATETIME = DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), 0);
DECLARE @ano  NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
DECLARE @os   UNIQUEIDENTIFIER;
DECLARE @base DATETIME;

/* ── Mês -5 ── */

SET @base = DATEADD(MONTH,-5, DATEADD(DAY,2,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(1,'/',@ano,'/',0),1,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,5,@base), DATEADD(DAY,3,@base),
    850.00, N'Substituição de selo mecânico.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(MONTH,-5, DATEADD(DAY,8,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(2,'/',@ano,'/',0),2,0,@ano,@idEstElev,@stFinalizada,1,1,
    @base, DATEADD(HOUR,2,@base), DATEADD(HOUR,5,@base), DATEADD(DAY,2,@base),
    320.00, N'Manutenção preventiva mensal.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

SET @base = DATEADD(MONTH,-5, DATEADD(DAY,15, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(3,'/',@ano,'/',0),3,0,@ano,@idEstBoost,@stFinalizada,2,1,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,3,@base), DATEADD(DAY,2,@base),
    180.00, N'Análise preditiva de vibração.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq4,1);

/* ── Mês -4 ── */

SET @base = DATEADD(MONTH,-4, DATEADD(DAY,3,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(4,'/',@ano,'/',0),4,0,@ano,@idEstElev,@stFinalizada,0,3,
    @base, DATEADD(HOUR,2,@base), DATEADD(HOUR,9,@base), DATEADD(DAY,2,@base),
    1450.00, N'Queima de motor elétrico — substituição.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(MONTH,-4, DATEADD(DAY,12, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(5,'/',@ano,'/',0),5,0,@ano,@idEstBoost,@stFinalizada,1,1,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,4,@base), DATEADD(DAY,2,@base),
    290.00, N'Lubrificação e aperto de terminais.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

SET @base = DATEADD(MONTH,-4, DATEADD(DAY,20, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(6,'/',@ano,'/',0),6,0,@ano,@idEstElev,@stFinalizada,1,1,
    @base, DATEADD(HOUR,2,@base), DATEADD(HOUR,5,@base), DATEADD(DAY,2,@base),
    310.00, N'Substituição de correia.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

/* ── Mês -3 ── */

SET @base = DATEADD(MONTH,-3, DATEADD(DAY,5,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(7,'/',@ano,'/',0),7,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(MINUTE,210,DATEADD(HOUR,1,@base)), DATEADD(DAY,2,@base),
    680.00, N'Vazamento no selo — troca do anel.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

SET @base = DATEADD(MONTH,-3, DATEADD(DAY,18, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(8,'/',@ano,'/',0),8,0,@ano,@idEstBoost,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,4,@base), DATEADD(DAY,2,@base),
    920.00, N'Queima do inversor de frequência.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

SET @base = DATEADD(MONTH,-3, DATEADD(DAY,25, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(9,'/',@ano,'/',0),9,0,@ano,@idEstBoost,@stFinalizada,2,1,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,2,@base), DATEADD(DAY,2,@base),
    120.00, N'Medição de isolamento elétrico.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq4,1);

/* ── Mês -2 ── */

SET @base = DATEADD(MONTH,-2, DATEADD(DAY,7,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(10,'/',@ano,'/',0),10,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,3,@base), DATEADD(DAY,2,@base),
    540.00, N'Rolamento desgastado — substituição.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(MONTH,-2, DATEADD(DAY,15, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(11,'/',@ano,'/',0),11,0,@ano,@idEstElev,@stFinalizada,1,1,
    @base, DATEADD(HOUR,2,@base), DATEADD(HOUR,5,@base), DATEADD(DAY,2,@base),
    290.00, N'Preventiva bimestral.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(MONTH,-2, DATEADD(DAY,22, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(12,'/',@ano,'/',0),12,0,@ano,@idEstBoost,@stFinalizada,1,1,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,3,@base), DATEADD(DAY,2,@base),
    270.00, N'Lubrificação geral.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

/* ── Mês -1 ── */

SET @base = DATEADD(MONTH,-1, DATEADD(DAY,4,  @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(13,'/',@ano,'/',0),13,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(MINUTE,150,DATEADD(HOUR,1,@base)), DATEADD(DAY,2,@base),
    390.00, N'Ajuste de alinhamento.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(MONTH,-1, DATEADD(DAY,18, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(14,'/',@ano,'/',0),14,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,3,@base), DATEADD(DAY,2,@base),
    460.00, N'Troca de gaxeta.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

SET @base = DATEADD(MONTH,-1, DATEADD(DAY,25, @hoje));
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(15,'/',@ano,'/',0),15,0,@ano,@idEstBoost,@stFinalizada,2,1,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,2,@base), DATEADD(DAY,2,@base),
    140.00, N'Análise termográfica.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

/* ── Mês atual ── */

SET @base = DATEADD(DAY,-14, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(16,'/',@ano,'/',0),16,0,@ano,@idEstElev,@stFinalizada,0,2,
    @base, DATEADD(HOUR,1,@base), DATEADD(HOUR,2,@base), DATEADD(DAY,2,@base),
    280.00, N'Correção de vibração — parafuso solto.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(DAY,-10, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(17,'/',@ano,'/',0),17,0,@ano,@idEstElev,@stFinalizada,1,1,
    @base, DATEADD(HOUR,2,@base), DATEADD(HOUR,5,@base), DATEADD(DAY,2,@base),
    310.00, N'Preventiva mensal — CMB-02.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

/* ── Bloco B: Abertas / Em Andamento ── */

SET @base = @hoje;
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(18,'/',@ano,'/',0),18,0,@ano,@idEstBoost,@stSolicitada,0,3,
    @base, NULL, NULL, DATEADD(DAY,2,@base),
    NULL, N'Alarme de sobretemperatura no motor.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

SET @base = DATEADD(DAY,-1, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(19,'/',@ano,'/',0),19,0,@ano,@idEstElev,@stSolicitada,0,2,
    @base, NULL, NULL, DATEADD(DAY,2,@base),
    NULL, N'Barulho anormal na bomba.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(DAY,-2, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(20,'/',@ano,'/',0),20,0,@ano,@idEstBoost,@stAndamento,1,1,
    @base, DATEADD(HOUR,8,@base), NULL, DATEADD(DAY,3,@base),
    NULL, N'Preventiva no painel — em execução.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq4,1);

SET @base = DATEADD(DAY,-3, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(21,'/',@ano,'/',0),21,0,@ano,@idEstElev,@stAndamento,2,1,
    @base, DATEADD(HOUR,9,@base), NULL, DATEADD(DAY,5,@base),
    NULL, N'Análise preditiva em andamento.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

/* ── Bloco C: Atrasadas ── */

SET @base = DATEADD(DAY,-10, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(22,'/',@ano,'/',0),22,0,@ano,@idEstElev,@stPendente,0,3,
    @base, DATEADD(HOUR,8,@base), NULL, DATEADD(DAY,4,@base),
    NULL, N'Verificar vazamento identificado na vistoria.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq1,1);

SET @base = DATEADD(DAY,-8, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(23,'/',@ano,'/',0),23,0,@ano,@idEstBoost,@stSolicitada,1,2,
    @base, NULL, NULL, DATEADD(DAY,4,@base),
    NULL, N'Preventiva não executada por falta de técnico.', 1);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq3,1);

SET @base = DATEADD(DAY,-5, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(24,'/',@ano,'/',0),24,0,@ano,@idEstElev,@stAndamento,0,2,
    @base, DATEADD(HOUR,10,@base), NULL, DATEADD(DAY,3,@base),
    NULL, N'Ruído anormal identificado — aguardando peça.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq2,1);

SET @base = DATEADD(DAY,-4, @hoje);
SET @os = NEWID();
INSERT INTO dbo.OrdemServico (Id,Codigo,Numero,SubOs,Ano,EstacaoId,StatusId,TipoOS,Prioridade,
    DataSolicitacao,DataInicioExecucao,DataFinalizacao,DataPrevista,CustoTotal,Observacao,Origem)
VALUES(@os,CONCAT(25,'/',@ano,'/',0),25,0,@ano,@idEstBoost,@stPendente,2,1,
    @base, NULL, NULL, DATEADD(DAY,3,@base),
    NULL, N'Análise preditiva do painel pendente.', 0);
INSERT INTO dbo.OrdemServicoEquipamento (OrdemServicoId,EquipamentoId,Principal) VALUES(@os,@idEq4,1);

COMMIT TRANSACTION;

/* -----------------------------------------------------------------------
   VERIFICAÇÃO RÁPIDA
   ----------------------------------------------------------------------- */
SELECT 'Estacoes'             AS Entidade, COUNT(*) AS Total FROM dbo.Estacao    WHERE Nome IN (N'Elevatória Jardim Sul',N'Booster São Sebastião')
UNION ALL
SELECT 'Equipamentos',                     COUNT(*)          FROM dbo.Equipamento WHERE EquipamentoPrincipalId IS NULL AND EstacaoId IN (SELECT Id FROM dbo.Estacao WHERE Nome IN (N'Elevatória Jardim Sul',N'Booster São Sebastião'))
UNION ALL
SELECT 'OS total',                         COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25
UNION ALL
SELECT 'OS Finalizadas',                   COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND StatusId = '8330AEC6-5E4D-4B67-8620-0C13FAEC04C5'
UNION ALL
SELECT 'OS Abertas/Andamento',             COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND StatusId NOT IN ('8330AEC6-5E4D-4B67-8620-0C13FAEC04C5','FC34601C-803F-4DA4-B4C9-0843AA1E8E4A')
UNION ALL
SELECT 'OS Atrasadas',                     COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND DataPrevista < CAST(GETDATE() AS DATE) AND StatusId NOT IN ('8330AEC6-5E4D-4B67-8620-0C13FAEC04C5','FC34601C-803F-4DA4-B4C9-0843AA1E8E4A')
UNION ALL
SELECT 'OS Corretivas',                    COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND TipoOS = 0
UNION ALL
SELECT 'OS Preventivas',                   COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND TipoOS = 1
UNION ALL
SELECT 'OS Preditivas',                    COUNT(*)          FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND TipoOS = 2
UNION ALL
SELECT 'Custo total finalizadas',          CAST(SUM(CustoTotal) AS INT) FROM dbo.OrdemServico WHERE Numero BETWEEN 1 AND 25 AND CustoTotal IS NOT NULL;