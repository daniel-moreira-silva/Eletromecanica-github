INSERT INTO dbo.TipoEstacao (Nome)
SELECT v.Nome
FROM (VALUES
    (N'Captações/Poços'),
    (N'Elevatórias de Água'),
    (N'Booster'),
    (N'ETAs'),
    (N'Reservatórios'),
    (N'VRPs'),
    (N'Elevatórias de Esgoto'),
    (N'Ar Condicionado Sede'),
    (N'Sistema Supervisório')
) AS v(Nome)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TipoEstacao te WHERE te.Nome = v.Nome);
GO

/* =========================================================
   6) SEED: TIPOEQUIPAMENTO (sugestão inicial)
   ========================================================= */
INSERT INTO dbo.TipoEquipamento (Nome, Categoria, Nivel)
SELECT v.Nome, v.Categoria, v.Nivel
FROM (VALUES
    -- NÍVEL 1 (PRINCIPAL)
    (N'Conjunto Motobomba',              N'Hidráulico',       CAST(1 AS TINYINT)),
    (N'Booster',                         N'Hidráulico',       CAST(1 AS TINYINT)),
    (N'VRP (Conjunto)',                  N'Hidráulico',       CAST(1 AS TINYINT)),
    (N'Gerador',                         N'Elétrico',         CAST(1 AS TINYINT)),
    (N'Transformador',                   N'Elétrico',         CAST(1 AS TINYINT)),
    (N'QGBT',                            N'Elétrico',         CAST(1 AS TINYINT)),
    (N'CCM',                             N'Elétrico',         CAST(1 AS TINYINT)),
    (N'Painel de Acionamento/Telemetria',N'Automação',        CAST(1 AS TINYINT)),
    (N'Sistema Supervisório (Host)',     N'Automação',        CAST(1 AS TINYINT)),

    -- NÍVEL 2 (COMPONENTE) - inclui instrumentos
    (N'Bomba',                           N'Hidráulico',       CAST(2 AS TINYINT)),
    (N'Válvula',                         N'Hidráulico',       CAST(2 AS TINYINT)),
    (N'Válvula de Retenção',             N'Hidráulico',       CAST(2 AS TINYINT)),
    (N'Ventosa',                         N'Hidráulico',       CAST(2 AS TINYINT)),
                                         
    (N'Motor Elétrico',                  N'Elétrico',         CAST(2 AS TINYINT)),
    (N'Disjuntor',                       N'Elétrico',         CAST(2 AS TINYINT)),
    (N'Contator',                        N'Elétrico',         CAST(2 AS TINYINT)),
    (N'Relé Térmico',                    N'Elétrico',         CAST(2 AS TINYINT)),
    (N'Fonte 24V',                       N'Elétrico',         CAST(2 AS TINYINT)),
    (N'DPS',                             N'Elétrico',         CAST(2 AS TINYINT)),
    (N'Fusível',                         N'Elétrico',         CAST(2 AS TINYINT)),
                                         
    (N'CLP',                             N'Automação',        CAST(2 AS TINYINT)),
    (N'IHM',                             N'Automação',        CAST(2 AS TINYINT)),
    (N'Módulo de I/O',                   N'Automação',        CAST(2 AS TINYINT)),
    (N'Rádio/Telemetria (RTU)',          N'Automação',        CAST(2 AS TINYINT)),
    (N'Switch',                          N'Automação',        CAST(2 AS TINYINT)),
    (N'Roteador',                        N'Automação',        CAST(2 AS TINYINT)),
    (N'Modem 3G/4G/5G',                  N'Automação',        CAST(2 AS TINYINT)),
    (N'Conversor de Mídia',              N'Automação',        CAST(2 AS TINYINT)),
    (N'Antena',                          N'Automação',        CAST(2 AS TINYINT)),
                                         
    (N'Inversor de Frequência (VFD)',    N'Acionamento',      CAST(2 AS TINYINT)),
    (N'Soft Starter',                    N'Acionamento',      CAST(2 AS TINYINT)),
    (N'Driver',                          N'Acionamento',      CAST(2 AS TINYINT)),
                                         
    (N'Nobreak',                         N'Automação',        CAST(2 AS TINYINT)),
    (N'Medidor de Vazão',                N'Instrumentação',   CAST(2 AS TINYINT)),
    (N'Sensor/Transmissor de Pressão',   N'Instrumentação',   CAST(2 AS TINYINT)),
    (N'Sensor/Transmissor de Nível',     N'Instrumentação',   CAST(2 AS TINYINT)),
    (N'Sensor de Temperatura',           N'Instrumentação',   CAST(2 AS TINYINT))
) AS v(Nome, Categoria, Nivel)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TipoEquipamento te WHERE te.Nome = v.Nome);
GO

INSERT INTO dbo.ServicoSolicitado (Codigo, Descricao, Sla)
SELECT v.Codigo, v.Descricao, v.Sla
FROM (VALUES
        ('S214', N'S214 - VERIFICAR FUNCIONAMENTO DE EQUIPAMENTOS', 0),
        ('S215', N'S215 - VAZAMENTO EM ELEVATÓRIA / RESERVATÓRIO', 0),
        ('S216', N'S216 - VISTORIA TÉCNICA', 0),
        ('S217', N'S217 - SEM COMUNICAÇÃO', 0),
        ('S218', N'S218 - BARULHO ANORMAL', 0),
        ('S219', N'S219 - RETIRAR EQUIPAMENTO', 0),
        ('S220', N'S220 - RETIRAR PONTO QUENTE', 0),
        ('S221', N'S221 - INSTALAR EQUIPAMENTO', 0),
        ('S222', N'S222 - VERIFICAR FUNCIONAMENTO BOMBA DOSADORA', 0),
        ('S223', N'S223 - VERIFICAR FUNCIONAMENTO DO SISTEMA GERAÇÃO DE CLORO/GEOCÁLCIO', 0),
        ('S224', N'S224 - ACOMPANHAR ABASTECIMENTO DE GERADOR DE ENERGIA', 0),
        ('S225', N'S225 - ACOMPANHAR MEDIÇÃO PREDITIVA', 0),
        ('S226', N'S226 - ALTERAÇÃO DE PARMETROS DE FUNCIONAMENTO DOS EQUIPAMENTOS', 0),
        ('S227', N'S227 - LIGAR E DESLIGAR CABOS DOS MOTORES', 0),
        ('S228', N'S228 - APOIO TÉCNICO', 0),
        ('S229', N'S229 - VERIFICAR GERADOR DE ENERGIA', 0),
        ('S230', N'S230 - MANUTENÇÃO PREVENTIVA', 0),
        ('S231', N'S231 - SUBESTAÇÃO DE MÉDIA TENSÃO', 0),
        ('S232', N'S232 - SOLDA', 0),
        ('S233', N'S233 - SERVIÇOS DE SERRALHERIA', 0),
        ('S234', N'S234 - LUBRIFICAR', 0),
        ('S235', N'S235 - ALINHAMENTO', 0),
        ('S236', N'S236 - SUBSTITUIÇÃO  DE TUBULAÇÃO DENTRO DA ELEVATÓRIA', 0),
        ('S237', N'S237 - SUBSTITUIÇÃO DE PEÇAS', 0),
        ('S238', N'S238 - USINAGEM DE PEÇAS', 0),
        ('S239', N'S239 - VALIDAR INDICAÇÃO DOS SENSORES DE NÍVEL/PRESSÃO/VAZÃO', 0),
        ('S240', N'S240 - VERIFICAR FALHA NO INVERSOR DE FREQUÊNCIA', 0),
        ('S241', N'S241 - CONFIGURAR CLP/INVERSOR', 0),
        ('S242', N'S242 - ACIONAMENTO DE CONJUNTOS MOTOBOMBA', 0),
        ('S243', N'S243 - INSTALAR E TESTAR PROGRAMAS DE CLP', 0),
        ('S244', N'S244 - INSTALAR INVERSOR DE FREQUÊNCIA', 0),
        ('S245', N'S245 - MANUTENÇÃO EM MOTORES', 0),
        ('S246', N'S246 - MANUTENÇÃO EM BOMBAS', 0),
        ('S247', N'S247 - MANUTENÇÃO EM REDUTORES', 0),
        ('S248', N'S248 - SUBSTITUIR EQUIPAMENTOS', 0),
        ('S249', N'S249 - LIMPEZA DE FILTRO DE ÁGUA', 0),
        ('S251', N'S251 - REPARAR PLACAS ELETRONICAS', 0),
        ('S252', N'S252 - FALTA DE ENERGIA', 0),
        ('S253', N'S253 - MEDIÇÃO DE VAZÃO', 0)
) AS v(Codigo, Descricao, Sla)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ServicoSolicitado ss WHERE ss.Codigo = v.Codigo);
GO

INSERT INTO dbo.StatusOrdemServico (Id, Descricao)
SELECT v.Id, v.Descricao
FROM (VALUES
    (N'E4011FE2-1CFC-46A4-9BB7-02F1E154C00E', N'Solicitada'),
    (N'2CF3AF37-18CD-4DF6-BE22-91B4DE792B25', N'Iniciada'),
    (N'132AE55D-A544-439E-8D07-CDC643CE4E78', N'Em Andamento'),
    (N'8330AEC6-5E4D-4B67-8620-0C13FAEC04C5', N'Finalizada'),
    (N'FC34601C-803F-4DA4-B4C9-0843AA1E8E4A', N'Cancelada'),
    (N'642A2B4C-1B28-4857-9FB9-C652B889392B', N'Pendente')
) v(Id, Descricao)
WHERE NOT EXISTS (SELECT 1 FROM dbo.StatusOrdemServico s WHERE s.Descricao = v.Descricao);
GO

INSERT INTO dbo.Regiao (Id, Descricao)
SELECT v.Id, v.Descricao
FROM (VALUES
    (N'957D2465-27AC-48FE-BFB0-88078F5039CF', N'Região Norte'),
    (N'B8228CD2-394A-44C7-9AD9-B422DA3BB2E9', N'Região Sul'),
    (N'0B19512D-8194-44A7-91AF-933C90DD450F', N'Região Leste'),
    (N'379B8037-F61A-41E8-8E48-7BDEFD6543DA', N'Região Oeste')
) v(Id, Descricao)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Regiao s WHERE s.Descricao = v.Descricao);
GO

INSERT INTO MotivoCancelamento (Id, Codigo, Descricao, Ativo)
SELECT v.Id, v.Codigo, v.Descricao, v.Ativo
FROM (VALUES
    ('DD02DEC3-A0E2-44B9-B5ED-1F8D60AE031A', 'PR', 'PR - PREDIO EM REFORMA', 1),
    ('47FFA23F-32C0-4A0D-B369-5BAD2E42B5C3', 'DU', 'DU - DUPLICIDADE', 1),
    ('3D723AD8-6845-450F-9E51-DEC309A91A1A', 'NL', 'NL - ENDERECO OU NUMERO NAO LOCALIZADOS', 1),
    ('CABD2F20-A683-4B97-BFA8-6BDA21B1E052', 'OU', 'OU - OUTROS', 1),
    ('AF7C9E95-6FE6-46C8-AC11-0A0A901A60E4', 'AA', 'AA - A PEDIDO DO ATENDIMENTO', 1),
    ('168DC1CD-0AA8-456A-90F1-18285990C55A', 'PA', 'PA - PREDIO ABANDONADO', 1),
    ('AD9A0BDF-14BE-4B48-B85E-99E642121716', 'AI', 'AI - ABERTURA INDEVIDA', 1),
    ('1BBC2FEE-9619-4AA9-AFAF-7CAC2348D9E2', 'RD', 'RD - REGISTRO DANIFICADO', 1),
    ('1ACCAC3B-1E89-4858-A3FC-935D65D6062F', 'LD', 'LD - LIGACAO DESATIVADA', 1),
    ('A9981E8A-9F33-4EA4-A2C1-DBBFF5331ADE', 'PD', 'PD - PREDIO DEMOLIDO', 1),
    ('95D2A8A6-1110-4A5E-B56D-8A25AA40DDE2', 'RA', 'RA - REPASSADA A OUTRA ÁREA', 1),
    ('F4B48F7E-596D-4A15-8F30-4734AB38AF7B', 'NP', 'NP - NÃO PERTINENTE À CESAMA', 1),
    ('B203A126-8F64-4120-8331-64B2BE21655D', 'HT', 'HT - HIDROMETRO JA TROCADO', 1),
    ('377B16F4-041D-4A7B-911A-4C2ADB8D746C', 'HI', 'HI - HIDROMETRO IMPEDIDO', 1),
    ('489FFC0C-0D59-4315-9DF8-0B3BCE098EEE', 'RE', 'RE - REPROGRAMACAO DE SERVICO', 1),
    ('D6F92619-8EA6-41BB-A8F0-C60BE884A84A', 'PF', 'PF - PORTAO FECHADO/CASA FECHADA', 1)
) v(Id, Codigo, Descricao, Ativo)
WHERE NOT EXISTS (SELECT 1 FROM MotivoCancelamento s WHERE s.Descricao = v.Descricao);
GO