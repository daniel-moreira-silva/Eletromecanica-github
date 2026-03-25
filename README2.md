# 1️⃣ Visão geral do domínio (antes dos equipamentos)

Esse ramo (saneamento / estações / elevatórias) gira em torno de um objetivo simples:

> **Mover água de forma segura, contínua e controlada**

Tudo existe para atender isso:

* Bombas → movem água
* Motores → fornecem energia mecânica
* Elétrica → alimenta tudo
* Automação → controla e protege
* Instrumentação → mede e alerta

👉 **Manutenção existe para evitar parada.**

---

# 2️⃣ Equipamentos “principais” (os que fazem o trabalho pesado)

## 🔹 Bomba

### O que é

É quem **move a água**.
Sem bomba, não existe estação funcionando.

### Principais informações técnicas (planilha)

* Vazão (m³/h)
* Altura manométrica (mca)
* Potência (CV ou kW)

### Como funciona (simplificado)

* O motor gira o eixo
* A bomba empurra a água
* Cada bomba tem um ponto ideal de operação

### O que importa para manutenção

* Vibração
* Vazamentos
* Rolamentos
* Selo mecânico
* Horas de operação

👉 **Falha de bomba = estação parada**

### No sistema de manutenção

* Tipo: `Bomba`
* Manutenção preventiva periódica
* Histórico de falhas
* Associação direta com motor

---

## 🔹 Motor elétrico

### O que é

Fornece a **energia mecânica** para a bomba.

### Nomenclaturas importantes

* CV / kW → potência
* RPM → rotações por minuto
* Tensão (220 / 380 / 440V)
* Trifásico

### O que importa para manutenção

* Aquecimento
* Isolamento
* Corrente elétrica
* Rolamentos
* Alinhamento com a bomba

👉 **Motor queima antes da bomba se algo estiver errado**

### No sistema

* Tipo: `Motor`
* Manutenção elétrica
* Relacionamento lógico com bomba (mesmo conjunto)

---

# 3️⃣ Equipamentos elétricos e de proteção

## 🔹 Nobreak (UPS)

### O que é

Mantém o sistema ligado **quando falta energia**.

### Na planilha

* Potência (VA)
* Autonomia (minutos)

### Importância

* Evita desligamento brusco
* Protege PLC e sensores

### Manutenção

* Baterias (vida útil!)
* Testes periódicos

👉 **Nobreak falha silenciosamente** se não for testado.

---

## 🔹 Quadros elétricos / fontes / conversores

### Função

* Distribuir energia
* Converter tensão
* Proteger equipamentos

### Manutenção

* Aperto de conexões
* Aquecimento
* Poeira / umidade

👉 Geralmente não “quebra”, mas causa falhas indiretas.

---

# 4️⃣ Automação (o cérebro da estação)

## 🔹 PLC (CLP)

### O que é

Controlador lógico programável.
É quem **decide quando a bomba liga ou desliga**.

### Na planilha

* Marca (Siemens, Schneider, etc.)
* Modelo
* Firmware

### O que ele faz

* Lê sensores
* Comanda bombas
* Protege o sistema
* Envia alarmes

### Manutenção

* Backup de programa
* Fonte de alimentação
* Ventilação
* Atualização controlada

👉 **PLC raramente quebra**, mas quando quebra, ninguém sabe o que fazer se não houver backup.

---

# 5️⃣ Instrumentação (os olhos do sistema)

Aqui está a parte que mais confunde no início.

---

## 🔹 Medidor de vazão

### O que mede

Quanto de água está passando.

### Por que existem vários

* Redundância
* Pontos diferentes
* Sistemas diferentes

### Nomenclaturas comuns

* Vazão instantânea
* Vazão acumulada
* Diâmetro
* Fator K

### Manutenção

* Calibração
* Sujeira
* Falha de leitura

👉 Sensor ruim = decisão errada do PLC.

---

## 🔹 Sensor de nível

### O que mede

Altura da água no reservatório.

### Importância

* Evita transbordamento
* Evita bomba trabalhar a seco

### Tipos

* Ultrassônico
* Pressão
* Boia

### Manutenção

* Limpeza
* Calibração
* Fixação

---

## 🔹 Sensor de pressão

### O que mede

Pressão da linha.

### Importância

* Detecta entupimento
* Detecta cavitação
* Protege bomba

---

# 6️⃣ Nomenclaturas que você vai ver muito

### 🔸 CV / kW

Potência do motor/bomba
1 CV ≈ 0,735 kW

---

### 🔸 m³/h

Metros cúbicos por hora (vazão)

---

### 🔸 mca

Metro de coluna d’água
Altura manométrica

---

### 🔸 Trifásico

Sistema elétrico industrial
Mais eficiente e estável

---

### 🔸 Setpoint

Valor de referência configurado no PLC
Ex: “ligar bomba quando nível < 30%”

---

### 🔸 Redundância

Equipamento reserva
Ex: Bomba 1 + Bomba 2 (standby)

---

# 7️⃣ O que você DEVE pensar ao criar um sistema de manutenção

Agora vem a parte importante para você como dev 👇

## Não pense em equipamento isolado

Pense em:

* Conjunto motobomba
* Sistema como um todo

---

## Informações essenciais para manutenção

* Horas de operação
* Histórico de falhas
* Tipo de manutenção
* Frequência
* Criticidade

---

## Equipamento crítico ≠ equipamento caro

* Sensor barato pode parar tudo
* Nobreak ignorado causa caos

---

# 8️⃣ Conclusão

Estamos construindo um sistema para:
✔ **evitar parada**
✔ **antecipar falha**
✔ **organizar manutenção**

---