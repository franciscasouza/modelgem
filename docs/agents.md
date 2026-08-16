# Agentes de execução

Subagentes do projeto em `.cursor/agents/`. Acione pelo nome ou peça ao agente principal para delegar conforme a fase.

## Ordem recomendada

1. `exec-orchestrator` — define fase e próximo incremento (consultar `docs/backlog-fase1.md` na Fase 1).
2. Agente da fase/módulo.
3. `exec-docs` — se houve decisão de escopo, arquitetura ou contrato.

## Mapa

| Agente | Fase / papel | Quando usar |
|---|---|---|
| `exec-orchestrator` | Todas | Planejamento, priorização, dúvida de ordem |
| `exec-discovery` | 0 | Entrevistas, medidas, qualidade, exemplos autorizados |
| `exec-pattern-core` | 1 | Bases saia/vestido, geometria, testes |
| `exec-platform` | 1+ | API .NET, tenant, clientes, arquivos, jobs |
| `exec-studio` | 1+ | Next.js, editor 2D, confirmação humana |
| `exec-export` | 1 | PDF A4 / SVG, escala, impressão |
| `exec-interpretation` | 2 | IA/OCR estruturada; nunca molde final |
| `exec-atelier` | 3 | Ficha, custo, pedidos, billing |
| `exec-docs` | Transversal | Docs e ADRs |

## Regras comuns

- Ler `AGENTS.md` e docs canônicos antes de implementar.
- Não pular a Fase 1 para entregar IA.
- Centímetros no domínio; conversão só na exportação.
- Multi-tenant, auditoria e versionamento desde o início.
- Definição de pronto: código + testes + docs + erros + observabilidade + segurança básica.
