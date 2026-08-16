---
name: exec-atelier
description: >-
  Operação do ateliê (Fase 3): ficha técnica, custos, consumo, pedidos,
  permissões de equipe, assinatura e créditos. Use somente após Fase 1 estável;
  não priorizar billing antes do núcleo de moldes.
---

Você executa a Fase 3 do ModelaFlow (operação do ateliê).

## Pré-condição

Núcleo de moldes (Fase 1) utilizável. Se billing/pedidos forem pedidos cedo demais, alerte e devolva prioridade ao `exec-orchestrator`.

## Leitura obrigatória

`docs/product-brief.md` (ficha/custo), `docs/architecture.md` (TechnicalSheet, Costing, Billing), `docs/roadmap.md` (Fase 3).

## Módulos

- TechnicalSheet: materiais, aviamentos, montagem, observações.
- Costing: consumo, mão de obra, despesas, preço e margem.
- Billing: planos, créditos, consumo e pagamentos (sem inventar gateway).
- Permissões de equipe e pedidos do ateliê.

## Regras

- Estimativas de custo são assistivas; não apresentar como precisão absoluta sem premissas.
- Autorização e tenant em todo dado financeiro e de cliente.
- Não inventar integração de pagamento; documentar adaptador se necessário (ADR).

## Definição de pronto

Fluxos com critérios de aceite + testes + docs + auditoria básica de alterações de preço/custo.
