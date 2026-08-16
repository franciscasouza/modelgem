# Critérios de qualidade — molde e interpretação assistida

Critérios de aceite para validação manual (Fase 0) e para o núcleo paramétrico / exportação (Fase 1+). A IA **não** é autoridade final: toda interpretação relevante exige confirmação da profissional (ADR-0001).

Unidade: **centímetros**. Escala de impressão tratada na borda PDF.

---

## 1. Aceite do molde geométrico (núcleo)

Um molde de saia reta ou vestido simples é **aceito** somente se todos os itens obrigatórios passarem.

### 1.1 Escala

| Critério | Aceite | Rejeitar |
| --- | --- | --- |
| Unidade interna | Tudo em cm | Mistura cm/polegada no domínio |
| Escala no PDF A4 | Régua/escala legível; 1 cm no molde = 1 cm real na impressão 100% | PDF sem escala, escala ambígua ou “encaixe na página” sem aviso de redução |
| Redução A4 | Se houver mosaico/tiles, cada página indica fator e marcas de união | Páginas sem indicação de que não estão em escala real |
| Foto de molde de entrada | Só usar se houver referência de escala explícita (régua, papel quadriculado conhecido) | Foto sem escala → não gerar molde “em tamanho real” a partir dela |

Tolerância de escala na validação manual impressa: **±2 mm em 10 cm** (2%) medidos com régua sobre a impressão.

### 1.2 Margens

| Critério | Aceite | Rejeitar |
| --- | --- | --- |
| Margem de costura | Presente e constante no valor declarado (`seam_allowance`, default 1,0 cm) nas arestas de costura | Arestas de costura sem margem ou com valor inconsistente sem marcação |
| Barra | `hem_allowance` distinta e visível | Barra confundida com margem lateral |
| Linha de costura vs corte | Linha de costura e linha de corte distinguíveis (traço, legenda ou camada) | Uma única linha sem legenda de o que é |
| Sobreposição de margens | Cantos com encontro coerente (sem “buraco” óbvio > 1 mm no desenho vetorial) | Cantos abertos ou margem negativa |

Tolerância dimensional das medidas-chave do perímetro (cintura/quadril convertidos): **±0,3 cm** em relação ao cálculo esperado da regra, na mesma versão de base.

### 1.3 Fio (sentido do tecido)

| Critério | Aceite | Rejeitar |
| --- | --- | --- |
| Marcação | Seta ou linha de fio em cada parte principal | Parte sem fio |
| Direção | Paralela ao eixo longitudinal da peça (política `parallel_center` no MVP) | Fio oblíquo sem indicação de vies |
| Legenda | Texto “fio” ou símbolo padrão do ateliê documentado | Símbolo ambíguo sem legenda na primeira exportação |

### 1.4 Piques e sinais

| Critério | Aceite | Rejeitar |
| --- | --- | --- |
| Laterais / união | Piques correspondentes frente↔costas nas mesmas alturas relativas (cintura, quadril, barra) | Piques só de um lado ou desalinhados > **0,5 cm** na altura |
| Centro | Marcação de centro frente e centro costas quando a base tiver dobra ou meio | Centro ausente em parte com dobra |
| Cava/decote (vestido) | Piques ou entalhes nos pontos de união ombro/lateral se a base tiver essas arestas | Uniões sem sinal quando há ≥ 2 partes |

### 1.5 Identificação das partes

Cada parte deve trazer, no mínimo: nome da parte, quantidade a cortar, indicação frente/costas, versão do modelo, e se há dobra. Sem identificação → rejeitar exportação “pronta para corte”.

### 1.6 Consistência com medidas

| Critério | Aceite |
| --- | --- |
| Perímetro na cintura (molde + folgas) | Compatível com `waist_circ + ease_waist` ± 0,3 cm |
| Perímetro no quadril | Compatível com `hip_circ + ease_hip` ± 0,3 cm |
| Comprimento | Compatível com `skirt_length` / `dress_length` e flags de barra ± 0,5 cm |
| Avisos | Inconsistências listadas como pendência, não silenciadas |

---

## 2. O que rejeitar de imediato (molde)

- Medidas fora de min–max do schema sem confirmação explícita.
- Comprimento menor que a soma mínima das alturas (ver `measurement-schema.md`).
- Quadril efetivo menor que cintura efetiva além da regra de aviso sem override da modelista.
- Exportação marcada como “escala real” quando o fator de impressão ≠ 100%.
- Partes espelhadas incorretas (duas frentes, nenhuma costas) sem aviso.
- Qualquer molde gerado só por IA sem etapa de confirmação.

---

## 3. Qualidade da interpretação assistida (Fase 0 manual / Fase 2 produto)

A interpretação sugere parâmetros; **não** grava molde final.

### 3.1 Saída estruturada mínima

- `schema_version`
- campos identificados vs estimados
- confiança por campo (ou global + lista de pendências)
- evidências (recorte/descrição da referência)
- perguntas pendentes para a profissional

### 3.2 Critérios de aceite da interpretação (revisão humana)

| Critério | Aceite | Rejeitar / devolver |
| --- | --- | --- |
| Tipo de peça | Classificação saia reta ou vestido simples correta ou marcada como “incerta” | Forçar tipo errado sem flag |
| Medidas extraídas | Só valores com suporte na referência ou marcados como estimados | Inventar cm sem marcar estimativa |
| Detalhes fora do MVP | Godê, mangas complexas, drapeado → pendência “fora da base” | Absorver no molde sem avisar |
| Confiança | Baixa confiança bloqueia auto-aplicação | Aplicar parâmetros com confiança baixa sem UI de confirmação |
| Transparência | Limitações visíveis (ex.: foto sem escala) | Ocultar limitação |

### 3.3 Validação manual de 20–50 exemplos (protocolo)

Para cada exemplo autorizado:

1. Registrar entrada (tipo de referência, qualidade da imagem, presença de escala).
2. Preencher “interpretação humana” dos parâmetros do schema (gabarito).
3. Comparar com sugestão futura da IA (quando existir) ou com a leitura de outra modelista.
4. Anotar: acertos, falsos positivos, campos que deveriam ser `pending`.
5. Decisão: `apto_base_parametrica` | `precisa_editor` | `fora_de_escopo_mvp`.

Métricas alinhadas ao product-brief:

- tempo referência → primeira versão útil (meta qualitativa na descoberta; baseline na Fase 1);
- número de correções por molde após a base;
- taxa de exemplos classificados como `fora_de_escopo_mvp` (alerta se > 40% da amostra).

---

## 4. Hipóteses testáveis

| ID | Hipótese | Como medir |
| --- | --- | --- |
| Q1 | Impressão A4 em 100% mantém ±2 mm / 10 cm | 10 impressões piloto com régua |
| Q2 | Piques laterais com tolerância 0,5 cm bastam para montagem de saia reta | Feedback em entrevista + 5 provas práticas |
| Q3 | Modelistas rejeitam molde sem fio/piques mesmo com medidas corretas | Pergunta binária no roteiro de entrevista |

---

## 5. Relação com outros artefatos

- Medidas e faixas: `measurement-schema.md`
- Consentimento da amostra: `example-intake-checklist.md`
- Perguntas de campo: `interview-guide.md`
