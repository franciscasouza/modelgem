# Exemplos de descoberta (Fase 0)

Acervo para validação **manual** de interpretação e qualidade de molde. Não é dataset de produção nem pasta para treino público de modelos.

## Pastas

| Pasta | Conteúdo |
| --- | --- |
| `authorized/` | Somente arquivos com consentimento completo e metadados (ver checklist). Pode estar vazia no git (`.gitkeep`). |

## Regras

1. Nada entra em `authorized/` sem passar por [`../example-intake-checklist.md`](../example-intake-checklist.md).
2. Preferir nomes `ex-NNN_<tipo>.ext` (ex.: `ex-001_hand_sketch.jpg`).
3. Não versionar PII, consentimentos nominais nem planilhas com dados de clientas.
4. Foto de molde sem escala: permitido só para estudar limitações; não tratar como verdade em cm.
5. Autoria e origem devem ser rastreáveis via `example_id` + `consent_id` (IDs opacos).

## Relacionados

- [`../measurement-schema.md`](../measurement-schema.md) — medidas canônicas
- [`../quality-criteria.md`](../quality-criteria.md) — aceite e rejeição
- [`../interview-guide.md`](../interview-guide.md) — pedido ético de exemplos nas entrevistas
