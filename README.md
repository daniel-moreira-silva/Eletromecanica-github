# 1️⃣ O que a planilha representa (visão de negócio)

A planilha **não é uma lista de equipamentos**.
Ela é uma **fotografia operacional de uma estação**.

Em uma única linha, a planilha mistura coisas de **naturezas diferentes**:

* Onde fica (estação)
* O que existe lá (equipamentos)
* Quantas vezes se repete (sensores)
* Detalhes técnicos específicos (potência, vazão, tensão…)

👉 Esse é o ponto-chave:
**o Excel achatou o domínio**, o banco **não pode fazer isso**.

---

## Exemplo típico da planilha (conceitual)

Imagine uma linha com informações como:

* Estação: *Elevatória XPTO*
* Tipo: *Elevatória*
* Bomba: *Bomba 1 – 10 m³/h*
* Motor: *Motor 7,5 CV – 380V*
* PLC: *Siemens S7*
* Vazão 1
* Vazão 2
* Sensor de nível
* Sensor de pressão

No Excel isso vira **uma linha só**.
No mundo real, isso são **várias entidades diferentes**.

---

# 2️⃣ Primeiro conceito-chave: **Estação é o centro de tudo**

A estação representa:

* Um local físico
* Um conjunto operacional
* Um agrupador natural

Por isso ela vira:

```text
Estacao
```

Tudo que existe **pertence a uma estação**:

* Equipamentos
* Instrumentos

📌 **Nada existe solto**.

---

## Exemplo real da planilha

Planilha:

> Estação: Elevatória Jardim Azul

Banco:

```text
Estacao
Id = 1
Nome = "Elevatória Jardim Azul"
TipoEstacao = "Elevatória"
```

---

# 3️⃣ Equipamento ≠ Instrumento

### Equipamento

* Atua fisicamente no processo
* Pode funcionar sozinho
* Tem potência, tensão, modelo

Exemplos da planilha:

* Bomba
* Motor
* PLC
* Nobreak
* Válvula

➡ Vira tabela **Equipamento**

---

### Instrumento

* Mede alguma coisa
* Pode existir em quantidade variável
* Normalmente se repete

Exemplos da planilha:

* Vazão 1
* Vazão 2
* Sensor de nível
* Sensor de pressão

➡ Vira tabela **Instrumento**

📌 Isso resolve o problema:

> “Hoje tem 2 vazões, amanhã tem 3”

---

# 4️⃣ Por que existe `TipoEquipamento`?

No Excel:

* “Bomba”
* “Motor”
* “PLC”

No banco:

```text
TipoEquipamento
```

Isso permite:

* Classificar
* Filtrar
* Criar regras no código
* Evoluir sem alterar schema

Exemplo:

```text
Equipamento
Id = 10
EstacaoId = 1
TipoEquipamento = Bomba
Tag = "BOM-01"
```

---

# 5️⃣ Por que existe tabela específica (Bomba, Motor, PLC)?

Aqui entra a **divergência de atributos**.

### No Excel:

* Potência aparece para bomba, motor, nobreak
* Vazão só para bomba
* RPM só para motor

### No banco:

❌ Não misturamos tudo
✔ Cada tipo crítico ganha sua própria tabela

```text
Equipamento (dados comuns)
Bomba (dados hidráulicos)
Motor (dados elétricos)
PLC (dados de automação)
```

📌 Um registro em `Bomba` **só existe se o equipamento for uma bomba**.

---

## Exemplo real

Planilha:

> Bomba 1 – 10 m³/h – 15 CV

Banco:

```text
Equipamento
Id = 20
TipoEquipamento = Bomba
Tag = "BOM-01"

Bomba
EquipamentoId = 20
Vazao = 10
AlturaManometrica = X
Potencia = 15
```

---

# 6️⃣ Instrumentação: resolvendo o maior problema da planilha

No Excel a gente tem colunas como:

* Vazão 1
* Vazão 2
* Vazão 3

Isso **não escala**.

### No banco:

```text
Instrumento
Id
EstacaoId
TipoInstrumentoId
Tag
```

Cada sensor vira **uma linha**, não uma coluna.

---

## Exemplo real

Planilha:

> Vazão 1, Vazão 2

Banco:

```text
Instrumento
Id = 1 | Tipo = Vazão | Tag = "VZ-01"
Id = 2 | Tipo = Vazão | Tag = "VZ-02"
```

Se amanhã tiver **5 vazões**:
✔ Só insere mais linhas
❌ Não altera banco

---

# 7️⃣ Sistema

Na planilha você vê:

* Sistema Elétrico
* Sistema Hidráulico
* Automação

Isso é:

* Organização lógica
* Visão humana
* Não entidade obrigatória

Por isso:

* **Não virou tabela**
* Está implícito no tipo

Exemplo:

* Bomba → Hidráulico
* Motor → Elétrico
* PLC → Automação

📌 Se o negócio pedir, vira tabela depois.

---

# 8️⃣ JSON: por que ele existe?

Porque **nem tudo vale uma tabela**.

Exemplo da planilha:

* Observações
* Detalhes muito específicos
* Campos raros

Isso vai em:

```text
DadosEspecificos (JSON)
```

Sem poluir o modelo.

---

# 9️⃣ Como tudo se relaciona (visão mental)

```
Estacao
 ├── Equipamento
 │     ├── Bomba
 │     ├── Motor
 │     ├── PLC
 │     └── Nobreak
 │
 └── Instrumento
       ├── Medidor de Vazao
       ├── Sensor de Nivel
       └── Sensor de Pressao
```
---

# 🔟 Estrutura

✔ Reflete o domínio real
✔ Escala com novas estações
✔ Aguenta novos equipamentos
✔ Aguenta novos sensores
✔ Não exige refatoração pesada
✔ Performance alta

---