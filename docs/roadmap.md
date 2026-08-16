# Roadmap inicial

## Fase 0 — Descoberta e validação

**Status:** mínimo em curso (artefatos em `docs/discovery/` pelo exec-discovery).

- entrevistar modelistas e ateliês;
- coletar desenhos e moldes com autorização;
- definir padrão de medidas;
- testar manualmente a interpretação de 20 a 50 exemplos;
- estabelecer critérios de qualidade.

## Fase 1 — Núcleo sem IA generativa

**Status:** F1.0–F1.6 entregues — API Design/generate/PDF + studio web (contratos alinhados em `apps/web/src/lib/api.ts`). Pendências: AuthN real, Redis/fila, dark mode, edição livre de curvas.

**Design / backlog:** Figma em `docs/design.md`; histórias da Fase 1 em `docs/backlog-fase1.md` (ordem F1.0→F1.6).

- organizações, usuários e clientes;
- medidas versionadas;
- bases de saia e vestido (paramétricas + testes de regressão);
- regras paramétricas;
- Design API: patterns, generate, technical-sheet, overview;
- PDF A4 com escala (job in-process);
- testes geométricos + isolamento tenant de patterns;
- editor 2D mínimo (UI).

## Fase 2 — Assistente de interpretação

- upload de referências;
- OCR;
- classificação da peça;
- extração de detalhes;
- tela de confirmação;
- registro de confiança e pendências.

## Fase 3 — Operação do ateliê

- ficha técnica;
- custos e preço;
- consumo estimado;
- pedidos;
- permissões de equipe;
- assinatura e créditos.

## Fase 4 — Profissionalização

- gradação;
- DXF;
- importação de moldes;
- comparação de versões;
- colaboração com clientes e fornecedores.

## Fase 5 — Expansão

- simulação 3D;
- marketplace de moldes;
- catálogo comercial;
- integrações de mensagens e pagamentos;
- novos tipos de peças.
