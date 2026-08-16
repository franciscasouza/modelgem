# ModelaFlow — contexto para agentes

## Objetivo

Construir um SaaS brasileiro para desenvolvimento de roupas. A plataforma deve aceitar desenhos, imagens, fotos de moldes e criação manual; auxiliar na interpretação e modelagem; gerar moldes editáveis; documentar a peça; calcular custos; e organizar o fluxo de produção.

## Princípios obrigatórios

1. A IA é assistente, não autoridade final. Toda interpretação relevante deve ser confirmada pela profissional.
2. Não prometer molde perfeito a partir de qualquer imagem. Registrar limitações e nível de confiança.
3. O núcleo confiável é paramétrico, determinístico, versionado e testável.
4. Preservar a autoria, os arquivos originais e o histórico de alterações.
5. Projetar multi-tenant desde o início, com isolamento de dados e auditoria.
6. Começar por saias e vestidos simples antes de ampliar para peças complexas ou 3D.
7. Toda mudança de produto ou arquitetura deve atualizar `docs/` e, quando for relevante, criar um ADR.

## Ordem de leitura

1. Este arquivo.
2. `docs/product-brief.md`.
3. `docs/architecture.md`.
4. `docs/roadmap.md`.
5. ADRs relevantes em `docs/decisions/`.
6. `docs/agents.md` — mapa dos agentes de execução em `.cursor/agents/`.

## Agentes de execução

Para trabalho multi-etapa, começar pelo subagente `exec-orchestrator`. Delegar em seguida ao agente da fase (`exec-discovery`, `exec-pattern-core`, `exec-platform`, `exec-studio`, `exec-export`, `exec-interpretation`, `exec-atelier`) e usar `exec-docs` quando a decisão alterar escopo, arquitetura ou contratos.

## Regras de implementação

- Não inventar integrações, credenciais, endpoints ou regras de modelagem.
- Não alterar o contrato de arquivos ou o modelo geométrico sem testes de regressão.
- Separar interpretação de IA, regras de modelagem e exportação.
- Validar unidades, escala, medidas, margens e compatibilidade das costuras.
- Processamentos de imagem, IA e geração de arquivos devem ser assíncronos.
- Dados de clientes e medidas corporais são sensíveis: aplicar minimização, autorização e auditoria.
- Toda funcionalidade deve ter critérios de aceitação e testes.

## Definição de pronto

Uma mudança só está pronta quando possui implementação, testes relevantes, documentação atualizada, tratamento de erros, observabilidade básica e revisão de segurança.
