# Schema de medidas — saia reta e vestido simples

Unidade de negócio: **centímetros (cm)**. Conversões só nas bordas de impressão/exportação.

Escopo: bases paramétricas do MVP (saia reta e vestido simples), alinhado ao product-brief e à Fase 0/1 do roadmap. Não cobre gradação industrial, peças complexas nem modelagem 3D.

## Convenções

| Campo | Significado |
| --- | --- |
| `id` | Identificador estável para API e regras paramétricas |
| Obrigatória | Necessária para gerar a base sem estimativa |
| Opcional | Melhora o ajuste; se ausente, a regra usa default documentado |
| Alias | Nomes comuns em ateliê / WhatsApp |
| min–max | Faixa de sanidade (adulto feminino sob medida); fora disso → rejeitar ou exigir confirmação |

Medidas corporais e de peça são dados sensíveis: minimizar, autorizar e auditar o acesso.

---

## Medidas corporais compartilhadas

Usadas por saia e vestido. Histórico versionado por cliente.

| id | Nome | Obrig. | Aliases | min–max (cm) | Notas de validação |
| --- | --- | --- | --- | --- | --- |
| `waist_circ` | Circunferência da cintura | Sim | cintura, CC | 50–150 | Medir no ponto mais estreito ou na linha de cintura marcada na peça. Par / ímpar ok; arredondar a 0,5 cm na UI se útil. |
| `hip_circ` | Circunferência do quadril | Sim | quadril, CQ | 70–180 | No ponto mais protuberante dos glúteos. Deve ser ≥ `waist_circ` − 2 cm (folga anatômica mínima); se não, avisar. |
| `waist_to_hip` | Distância cintura → quadril | Não* | altura do quadril, CQ vertical | 14–30 | *Obrigatória se a base exigir curva de quadril explícita; senão default 20 cm. |
| `body_height` | Altura corporal | Não | estatura | 140–200 | Só para contexto/ficha; não gera o molde sozinha. |

---

## Saia reta

Base: saia tubular/reta, **2 partes (frente + costas) simétricas** na base v1 (`docs/pattern-core.md`); cintura na linha natural, sem pregas/godê no MVP. Cós separado (`waistband_height` > 0) fora da base v1.

### Obrigatórias (peça / ajuste)

| id | Nome | Aliases | min–max (cm) | Notas |
| --- | --- | --- | --- | --- |
| `waist_circ` | Cintura | (ver acima) | 50–150 | Entrada principal do perímetro superior. |
| `hip_circ` | Quadril | (ver acima) | 70–180 | Controla a largura na linha de quadril. |
| `skirt_length` | Comprimento da saia | comprimento, comprimento total | 30–120 | Da cintura até a barra (acabada ou bruta? ver `length_includes_hem`). |
| `ease_waist` | Folga na cintura | folga cintura | 0–8 | Default sugerido: 2 cm no perímetro total. |
| `ease_hip` | Folga no quadril | folga quadril | 0–12 | Default sugerido: 4 cm no perímetro total. |

### Opcionais

| id | Nome | Aliases | min–max (cm) | Default | Notas |
| --- | --- | --- | --- | --- | --- |
| `waist_to_hip` | Cintura → quadril | altura do quadril | 14–30 | 20 | Posiciona a linha de quadril no molde. |
| `hem_allowance` | Margem de barra | barra, bainha | 1–8 | 3 | Separada da margem de costura lateral. |
| `seam_allowance` | Margem de costura | margem, MA | 0,5–2,5 | 1,0 | Aplicada nas laterais e cintura (exceto se `waist_finish` = elástico embutido com regra própria). |
| `waistband_height` | Altura do cós | cós | 0–8 | 0 | 0 = sem cós separado (só acabamento). |
| `length_includes_hem` | Comprimento inclui barra? | — | boolean | `false` | Se `true`, `skirt_length` já contém a barra; não somar `hem_allowance` de novo. |

### Regras de consistência (saia)

1. `hip_circ + ease_hip` ≥ `waist_circ + ease_waist`.
2. `skirt_length` ≥ `waist_to_hip` + 8 cm (barra mínima abaixo do quadril).
3. Perímetros convertidos em metade/frente-costas nas regras geométricas; este schema guarda só o perímetro total.
4. Qualquer valor fora de min–max → status `needs_confirmation`, nunca cálculo silencioso.

---

## Vestido simples

Base: vestido de uma peça, silhueta reta ou levemente acinturada, sem mangas complexas no MVP (alça/ombro opcional simples), sem godê, drapeado ou forro estruturado.

### Obrigatórias

| id | Nome | Aliases | min–max (cm) | Notas |
| --- | --- | --- | --- | --- |
| `bust_circ` | Circunferência do busto | busto, CB | 70–160 | No ponto mais cheio; fita horizontal. |
| `waist_circ` | Cintura | — | 50–150 | Idem saia. |
| `hip_circ` | Quadril | — | 70–180 | Idem saia. |
| `dress_length` | Comprimento do vestido | comprimento | 70–160 | Da base do pescoço/ombro (definir `length_from`) até a barra. |
| `ease_bust` | Folga no busto | folga busto | 0–12 | Default sugerido: 4 cm. |
| `ease_waist` | Folga na cintura | — | 0–8 | Default: 2 cm. |
| `ease_hip` | Folga no quadril | — | 0–12 | Default: 4 cm. |

### Opcionais

| id | Nome | Aliases | min–max (cm) | Default | Notas |
| --- | --- | --- | --- | --- | --- |
| `back_width` | Largura das costas | costas | 28–45 | derivado de `bust_circ`/4 + ajuste | Se ausente, derivar na regra; não inventar no intake. |
| `front_chest_width` | Largura do peito | peito | 28–48 | derivado | Idem. |
| `shoulder_width` | Largura do ombro | ombro | 10–18 | 13 | Por ombro; útil se houver cava/alça. |
| `armhole_depth` | Profundidade da cava | cava | 15–28 | 20 | Só se a base tiver cava. |
| `neck_circ` | Circunferência do pescoço | pescoço | 30–45 | 36 | Para decote básico. |
| `waist_to_hip` | Cintura → quadril | — | 14–30 | 20 | Idem saia. |
| `bust_to_waist` | Busto → cintura | — | 14–28 | 20 | Posiciona linha de cintura. |
| `shoulder_to_bust` | Ombro → busto | altura do busto | 20–32 | 26 | Para posicionar o busto na frente. |
| `hem_allowance` | Margem de barra | — | 1–8 | 3 | Idem saia. |
| `seam_allowance` | Margem de costura | — | 0,5–2,5 | 1,0 | Idem saia. |
| `length_from` | Origem do comprimento | — | enum | `shoulder` | Valores: `shoulder` \| `waist` \| `nape`. Documentar na ficha. |
| `length_includes_hem` | Comprimento inclui barra? | — | boolean | `false` | Idem saia. |

### Regras de consistência (vestido)

1. `bust_circ + ease_bust` ≥ `waist_circ + ease_waist` (com tolerância de aviso se silhueta for “justa no busto”).
2. `hip_circ + ease_hip` ≥ `waist_circ + ease_waist`.
3. Se `length_from` = `shoulder`, então `dress_length` ≥ `shoulder_to_bust` + `bust_to_waist` + `waist_to_hip` + 10 cm.
4. Campos derivados (`back_width`, etc.) não sobrescrevem medida informada pela profissional sem confirmação.

---

## Folgas e acabamentos (comum)

| id | Nome | Tipo | Default | Notas |
| --- | --- | --- | --- | --- |
| `ease_waist` / `ease_hip` / `ease_bust` | Folgas | cm no perímetro | ver tabelas | Sempre perímetro total, não metade. |
| `seam_allowance` | Margem de costura | cm | 1,0 | Uniforme no MVP; exceções futuras por aresta. |
| `hem_allowance` | Barra | cm | 3 | Independente da margem lateral. |
| `grainline_policy` | Política de fio | enum | `parallel_center` | Ver `quality-criteria.md`. |

---

## Aliases e normalização

- Aceitar entrada em português com variação de acento; normalizar para `id` canônico.
- Rejeitar polegadas na camada de domínio; se a UI receber in, converter na borda e gravar cm.
- Não mapear “tamanho P/M/G” para medidas sem tabela explícita versionada (fora do schema mínimo).

---

## Hipóteses testáveis (Fase 0)

| ID | Hipótese | Métrica |
| --- | --- | --- |
| H1 | Com `waist_circ`, `hip_circ`, comprimento e folgas, a modelista gera primeira versão útil de saia reta sem medidas extras. | ≥ 80% das entrevistas: “suficiente para base” |
| H2 | Defaults de folga (2/4/4 cm) são aceitos ou ajustados em ≤ 2 edições. | Média de correções de folga ≤ 2 por molde-piloto |
| H3 | `waist_to_hip` default 20 cm cobre a maioria dos casos sob medida do público inicial. | ≤ 30% dos exemplos exigem override |

---

## Pendências para o núcleo paramétrico (Fase 1)

- [x] Cós (`waistband_height`): **fora da base v1** — deve ser 0; cós separado fica para ficha/acabamento posterior (`docs/pattern-core.md`, `StraightSkirtPattern`).
- [x] Saia MVP: **sempre 2 partes** (frente + costas), simétricas; 1 peça não entra na base v1.
- [x] Congelar `schema_version` (`measurements.v1` / int `1` em `ModelaFlow.PatternCore.Measurements.MeasurementSchema`) alinhado às chaves canônicas no Customer.
- [ ] Não bloqueia schema: coleta autorizada de exemplos e checklist de qualidade (artefatos irmãos).
- [ ] Validação de campo com 3–5 modelistas sobre defaults de folga e `waist_to_hip` (hipóteses H1–H3).
