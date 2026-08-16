# Checklist de intake — exemplos autorizados (20–50)

Protocolo para coletar desenhos, fotos de referência e moldes **somente com autorização e consentimento**. Sem checklist completo, o arquivo **não** entra em `docs/discovery/examples/authorized/`.

Meta Fase 0: **20–50 exemplos** para validação manual de interpretação e qualidade — **sem** treinar/implementar IA de produção neste momento.

---

## A. Antes de receber qualquer arquivo

- [ ] Pessoa responsável identificada (nome + contato profissional)
- [ ] Papel: modelista / dona de ateliê / outra (especificar)
- [ ] Explicado o uso: pesquisa de produto ModelaFlow (schema, qualidade, interpretação assistida), não revenda de moldes
- [ ] Explicado o que **não** faremos: uso comercial da imagem da cliente final; treino público; publicação identificável sem novo consentimento
- [ ] Direito de retirada comunicado (pedido por e-mail/WhatsApp → exclusão do acervo de descoberta)
- [ ] Preferência: material anonimizado (sem rosto, sem nome de cliente, sem telefone na imagem)

**Parar aqui se qualquer item acima for “não”.**

---

## B. Consentimento (obrigatório)

Registrar por escrito (mensagem, e-mail ou termo curto). Campos:

| Campo | Valor |
| --- | --- |
| Data do consentimento | |
| Texto resumido aceito | Ex.: “Autorizo o uso deste material anonimizado apenas para pesquisa interna do ModelaFlow (Fase 0), com direito de retirada.” |
| Canal | WhatsApp / e-mail / formulário |
| Consentimento para armazenamento | sim / não |
| Consentimento para análise humana | sim / não |
| Consentimento para uso futuro em testes de IA **internos** | sim / não / adiado |
| Inclui imagem de pessoa identificável | sim / não — se sim, exige anonimização ou recusa |

- [ ] Consentimento arquivado fora do git ou em pasta restrita (não commitar PII)
- [ ] ID interno do consentimento anotado no metadado do exemplo (ex.: `consent_id`)

Medidas corporais de clientes reais: só com autorização explícita; preferir faixas/exemplos sintéticos ou medidas já anonimizadas pela profissional.

---

## C. Metadados por exemplo

Preencher **um registro por arquivo** (planilha interna ou YAML local não versionado). Nome sugerido do arquivo em `authorized/`: `ex-NNN_<tipo>.ext` (sem nome de pessoa).

| Metadado | Obrigatório | Valores / notas |
| --- | --- | --- |
| `example_id` | Sim | `ex-001` … |
| `consent_id` | Sim | Ligação ao termo |
| `contributor_role` | Sim | modelista / atelier / outro |
| `received_at` | Sim | data |
| `source_type` | Sim | `hand_sketch` \| `tech_sketch` \| `garment_photo` \| `paper_pattern_photo` \| `reference_image` \| `measurement_sheet` \| `other` |
| `garment_family` | Sim | `straight_skirt` \| `simple_dress` \| `other` |
| `mvp_in_scope` | Sim | sim / não / incerto |
| `has_scale_reference` | Sim | sim / não / n/a — crítico para foto de molde |
| `scale_notes` | Se foto de molde | régua, quadriculado, medida conhecida |
| `units` | Sim | cm (converter na borda se necessário) |
| `image_quality` | Sim | boa / regular / ruim |
| `contains_pii` | Sim | sim / não — se sim, anonimizar antes de copiar para `authorized/` |
| `license_notes` | Sim | “uso pesquisa Fase 0”; restrições extras |
| `linked_measures` | Não | ids do schema presentes ou “nenhuma” |
| `human_interpretation` | Sim (após review) | parâmetros preenchidos manualmente + pendências |
| `quality_review` | Sim (após review) | `apto_base_parametrica` \| `precisa_editor` \| `fora_de_escopo_mvp` |
| `notes` | Não | livre |

---

## D. Critérios para aceitar no acervo `authorized/`

- [ ] Consentimento B completo
- [ ] Sem PII visível (ou arquivo já redigido)
- [ ] `source_type` e `garment_family` preenchidos
- [ ] Se `paper_pattern_photo`: `has_scale_reference` resolvido (se “não”, manter só para estudo de limitações, marcar `mvp_in_scope=não` para escala real)
- [ ] Hash ou tamanho do arquivo anotado (integridade)
- [ ] Original preservado; não substituir silenciosamente

**Não aceitar**

- prints de redes sem direito claro da contribuinte;
- fotos de clientas identificáveis sem anonimização;
- moldes de terceiros “baixados” da internet;
- material sob NDA de marca que proíba pesquisa.

---

## E. Checklist de validação manual (por exemplo)

Alinhado a `quality-criteria.md` §3.3:

- [ ] Classificar família da peça e se está no MVP
- [ ] Listar parâmetros do `measurement-schema.md` que dá para inferir vs que ficam `pending`
- [ ] Marcar presença/ausência de escala, fio, piques, margens (se molde)
- [ ] Decisão `quality_review`
- [ ] Tempo gasto na revisão (minutos) — proxy de “referência → primeira leitura”

Meta de amostra:

| Família | Mínimo sugerido |
| --- | --- |
| Saia reta | 10 |
| Vestido simples | 10 |
| Fora de escopo / controle | 5–10 |
| Com referência de escala | ≥ 8 |
| Só foto de peça sem escala | ≥ 8 (para testar limitações) |

---

## F. Onde guardar

| Conteúdo | Onde |
| --- | --- |
| Arquivos autorizados e anonimizados | `docs/discovery/examples/authorized/` |
| Índice e regras da pasta | `docs/discovery/examples/README.md` |
| Consentimentos e planilha com PII | **fora do repositório público** (drive interno / cofre) |
| Este checklist | `docs/discovery/example-intake-checklist.md` |

Nunca commitar: rostos identificáveis, nomes de clientes, telefones, medidas nominais ligadas a pessoa sem base legal/consentimento.

---

## G. Hipótese de coleta

| ID | Hipótese | Métrica |
| --- | --- | --- |
| C1 | Em 4 semanas é possível reunir ≥ 20 exemplos autorizados com metadados completos | Contagem em `authorized/` + planilha |
| C2 | ≥ 60% dos exemplos in-scope têm interpretação humana cobrindo medidas obrigatórias do schema | % com campos obrigatórios preenchidos ou explicitamente `pending` |
