SET NOCOUNT ON;

-- Desabilita constraints temporariamente para facilitar a limpeza
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- =========================================================
-- 1. Tabelas N:N e dependentes (mais profundas primeiro)
-- =========================================================

-- OS: serviços e equipamentos vinculados
DELETE FROM dbo.OrdemServicoServicoExecutado;
DELETE FROM dbo.OrdemServicoServicoSolicitado;
DELETE FROM dbo.OrdemServicoEquipamento;

-- Manutenção preventiva
DELETE FROM dbo.RegraPreventivaServicoSolicitado;
DELETE FROM dbo.RegraPreventiva;

-- Documentos: tags e vínculos
DELETE FROM dbo.DocumentoTag;
DELETE FROM dbo.DocumentoVinculo;

-- =========================================================
-- 2. Tabelas principais (dependem das anteriores)
-- =========================================================

DELETE FROM dbo.OrdemServico;
DELETE FROM dbo.Agendamento;

DELETE FROM dbo.CaracteristicaEquipamento;

-- Tabelas específicas de equipamento (ON DELETE CASCADE, mas explícito por clareza)
DELETE FROM dbo.MedidorVazao;
DELETE FROM dbo.Nobreak;
DELETE FROM dbo.CLP;
DELETE FROM dbo.Motor;
DELETE FROM dbo.Bomba;

-- Componentes antes dos principais (auto-referência)
DELETE FROM dbo.Equipamento WHERE EquipamentoPrincipalId IS NOT NULL;
DELETE FROM dbo.Equipamento WHERE EquipamentoPrincipalId IS NULL;

DELETE FROM dbo.Estacao;

-- =========================================================
-- 3. Catálogos e lookup tables
-- =========================================================

DELETE FROM dbo.Documento;
DELETE FROM dbo.TagDocumento;

--DELETE FROM dbo.ServicoSolicitado;
DELETE FROM dbo.ServicoExecutado;
--DELETE FROM dbo.StatusOrdemServico;
DELETE FROM dbo.MotivoCancelamento;
--DELETE FROM dbo.Regiao;

--DELETE FROM dbo.TipoEquipamento;
--DELETE FROM dbo.TipoEstacao;

-- =========================================================
-- 4. Log (independente)
-- =========================================================

DELETE FROM dbo.Log;

-- Reabilita todas as constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';

PRINT 'Limpeza concluída com sucesso.';