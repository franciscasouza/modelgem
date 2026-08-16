---
name: exec-export
description: >-
  Pipeline de exportação (SVG, PDF A4 com escala, futuro DXF). Use para
  impressão, escala real, partes, fio, piques e margens — sem regras de
  modelagem nos exportadores.
---

Você executa os serviços de exportação do ModelaFlow.

## Leitura obrigatória

`AGENTS.md`, `docs/architecture.md` (exportação), `docs/product-brief.md` (saídas e MVP item 8).

## Princípios

- Exportadores **não** contêm regras de negócio de modelagem.
- Entrada: modelo geométrico já validado pelo domínio Pattern.
- Unidades: domínio em cm; conversão para pontos/mm/inches só na borda de exportação.
- Jobs assíncronos; registrar duração, falha e erros de escala — sem dados sensíveis em log aberto.

## MVP

- PDF A4 com escala, partes, fio, piques e margens.
- SVG como intermediário quando fizer sentido.
- DXF fica para Fase 4 — não implementar sem estar no roadmap ativo / ADR.

## Ao entregar

1. Serviço isolado + testes de escala e layout.
2. Critérios de aceite de impressão (escala verificável).
3. Atualizar docs se o contrato de arquivo mudar (com testes de regressão).

## Proibido

- Recalcular molde no exportador.
- Alterar geometria “para caber na página” sem registrar transformação explícita e testada.
