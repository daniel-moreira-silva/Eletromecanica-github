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

INSERT INTO dbo.ServicoSolicitado (Codigo, Descricao)
SELECT v.Codigo, v.Descricao
FROM (VALUES
        ('S214', N'S214 - VERIFICAR FUNCIONAMENTO DE EQUIPAMENTOS'),
        ('S215', N'S215 - VAZAMENTO EM ELEVATÓRIA / RESERVATÓRIO'),
        ('S216', N'S216 - VISTORIA TÉCNICA'),
        ('S217', N'S217 - SEM COMUNICAÇÃO'),
        ('S218', N'S218 - BARULHO ANORMAL'),
        ('S219', N'S219 - RETIRAR EQUIPAMENTO'),
        ('S220', N'S220 - RETIRAR PONTO QUENTE'),
        ('S221', N'S221 - INSTALAR EQUIPAMENTO'),
        ('S222', N'S222 - VERIFICAR FUNCIONAMENTO BOMBA DOSADORA'),
        ('S223', N'S223 - VERIFICAR FUNCIONAMENTO DO SISTEMA GERAÇÃO DE CLORO/GEOCÁLCIO'),
        ('S224', N'S224 - ACOMPANHAR ABASTECIMENTO DE GERADOR DE ENERGIA'),
        ('S225', N'S225 - ACOMPANHAR MEDIÇÃO PREDITIVA'),
        ('S226', N'S226 - ALTERAÇÃO DE PARMETROS DE FUNCIONAMENTO DOS EQUIPAMENTOS'),
        ('S227', N'S227 - LIGAR E DESLIGAR CABOS DOS MOTORES'),
        ('S228', N'S228 - APOIO TÉCNICO'),
        ('S229', N'S229 - VERIFICAR GERADOR DE ENERGIA'),
        ('S230', N'S230 - MANUTENÇÃO PREVENTIVA'),
        ('S231', N'S231 - SUBESTAÇÃO DE MÉDIA TENSÃO'),
        ('S232', N'S232 - SOLDA'),
        ('S233', N'S233 - SERVIÇOS DE SERRALHERIA'),
        ('S234', N'S234 - LUBRIFICAR'),
        ('S235', N'S235 - ALINHAMENTO'),
        ('S236', N'S236 - SUBSTITUIÇÃO  DE TUBULAÇÃO DENTRO DA ELEVATÓRIA'),
        ('S237', N'S237 - SUBSTITUIÇÃO DE PEÇAS'),
        ('S238', N'S238 - USINAGEM DE PEÇAS'),
        ('S239', N'S239 - VALIDAR INDICAÇÃO DOS SENSORES DE NÍVEL/PRESSÃO/VAZÃO'),
        ('S240', N'S240 - VERIFICAR FALHA NO INVERSOR DE FREQUÊNCIA'),
        ('S241', N'S241 - CONFIGURAR CLP/INVERSOR'),
        ('S242', N'S242 - ACIONAMENTO DE CONJUNTOS MOTOBOMBA'),
        ('S243', N'S243 - INSTALAR E TESTAR PROGRAMAS DE CLP'),
        ('S244', N'S244 - INSTALAR INVERSOR DE FREQUÊNCIA'),
        ('S245', N'S245 - MANUTENÇÃO EM MOTORES'),
        ('S246', N'S246 - MANUTENÇÃO EM BOMBAS'),
        ('S247', N'S247 - MANUTENÇÃO EM REDUTORES'),
        ('S248', N'S248 - SUBSTITUIR EQUIPAMENTOS'),
        ('S249', N'S249 - LIMPEZA DE FILTRO DE ÁGUA'),
        ('S251', N'S251 - REPARAR PLACAS ELETRONICAS'),
        ('S252', N'S252 - FALTA DE ENERGIA'),
        ('S253', N'S253 - MEDIÇÃO DE VAZÃO')
) AS v(Codigo, Descricao)
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