# Backlog — Fase 1 (telas Figma → entregas)

Fonte de design: [`docs/design.md`](design.md) · produto: [`product-brief.md`](product-brief.md) · roadmap: [`roadmap.md`](roadmap.md).

**Já entregue (não repetir):** monorepo, Identity/Customer/Audit + medidas versionadas (API), `pattern-core` saia/vestido, agentes e discovery.

**Regra:** IA generativa / OCR / “Upload Reference (AI)” completo → **Fase 2**. Na Fase 1, só shell/nav e CTAs desabilitados ou com aviso.

---

## Status UI (apps/web)

| Incremento | UI | Notas |
| --- | --- | --- |
| F1.0 | **Implementado** | Tokens CSS + AppShell (sidebar/tabs) + rotas |
| F1.1 | **Implementado** | Lista/busca/criar clientes + measurement-sets (cm) |
| F1.2 | **Implementado** | Welcome, atalhos, recentes, overview real |
| F1.3 | **Implementado** | Wizard: `POST patterns` + `POST .../generate` (contrato alinhado) |
| F1.4 | **Implementado** | SVG + versão com geometria da API; fixture só se 404 |
| F1.5 | **Implementado** | Export job + download em `/exports/{jobId}` |
| F1.6 | **Implementado** | Ficha PUT/GET technical-sheet |
| F1.7 | **API** | AuthN cookie+JWT multi-tenant (ADR-0004); UI login → `exec-studio` |

**Integração:** client `apps/web/src/lib/api.ts` adaptado aos DTOs da API (não usar `POST /patterns/generate` solto).

---

## Ordem sugerida de incrementos

| # | Incremento | Agentes | Telas Figma |
| --- | --- | --- | --- |
| F1.0 | Design system no web (tokens + shell) | `exec-studio`, `exec-docs` | `3:2`, `3:240` + chrome comum |
| F1.1 | Gestão de clientes + medidas (UI) | `exec-studio`, `exec-platform` | `1:809` |
| F1.2 | Dashboard / biblioteca (sem IA) | `exec-studio`, `exec-platform` | `1:2` |
| F1.3 | Fluxo base paramétrica → molde | `exec-studio`, `exec-pattern-core`, `exec-platform` | parte de `1:2` → `1:359` |
| F1.4 | Editor 2D mínimo | `exec-studio`, `exec-pattern-core` | `1:359` |
| F1.5 | Exportação PDF A4 | `exec-export`, `exec-studio` | fatia de `1:501` |
| F1.6 | Ficha técnica básica (leitura) | `exec-atelier` (mínimo) / `exec-studio` | fatia de `1:501` |
| F1.7 | AuthN mínimo (API) | `exec-platform` | — (sessão antes de Fase 2) |
| — | Dark mode | `exec-studio` | variantes dark — **depois** do light estável |
| — | Editor IA completo | `exec-interpretation` | `1:183` — **Fase 2** |

---

## Épicos e histórias

### F1.0 — Shell e tokens

**Objetivo:** `apps/web` deixa de ser placeholder e ganha chrome do Figma (sidebar + top nav + tokens).

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.0-1 | Tokens CSS a partir do board Color/Typography | Variáveis de cor/tipo documentadas; usadas no layout | Pixel-perfect dark |
| F1.0-2 | App shell (sidebar + top tabs) | Nav: Biblioteca, Editor IA (disabled/Fase 2), Canvas 2D, Clientes, Config; tabs: Dashboard, AI Editor, 2D, Tech Pack | Auth real |
| F1.0-3 | Rotas Next.js por área | `/`, `/clients`, `/patterns/:id/canvas`, `/patterns/:id/tech-pack`; `/ai` → stub Fase 2 | Micro-frontends |

**Figma:** chrome de `1:2` / `1:809`.

---

### F1.1 — Gestão de clientes (`1:809`)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.1-1 | Lista / busca de clientes | Consome API; filtro por nome/id; isolamento tenant | Import CSV |
| F1.1-2 | Detalhe do cliente | Nome, notas, projetos vinculados (mesmo stub se Design ainda mínimo) | CRM completo |
| F1.1-3 | Medidas versionadas na UI | Criar MeasurementSet; listar versões; chaves `measurements.v1`; cm | Gráfico antropométrico avançado do mock como dado real |
| F1.1-4 | Formulário de medidas saia/vestido | Campos obrigatórios do schema; validação min/max; defaults de folga | Estimativa por foto |

**API:** endpoints atuais `/api/v1/tenants/...`. Completar auth/tenant no request quando houver sessão (placeholder de `tenantId` documentado até AuthN).

---

### F1.2 — Dashboard / biblioteca (`1:2`)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.2-1 | Welcome + 3 atalhos | **Parametric Base** e **Blank Canvas** ativos; **Upload Reference (AI)** desabilitado com copy de “em breve / Fase 2” | Geração por IA |
| F1.2-2 | Recent Models | Cards com nome, ref, cliente, data; link para canvas | Stats inventados (1.248 patterns) — usar contagens reais ou ocultar |
| F1.2-3 | Overview widget | Totais reais (modelos, clientes); “pending approvals” só se houver modelo de status | Fake metrics |
| F1.2-4 | Persistência Design mínima | Modelo com versão, cliente opcional, base (`straight_skirt` / `simple_dress` / blank) | Marketplace |

---

### F1.3 — Base paramétrica → documento de molde

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.3-1 | Wizard: escolher base saia/vestido | Inputs alinhados a `StraightSkirtInput` / `SimpleDressInput` | Outras peças |
| F1.3-2 | Selecionar cliente + MeasurementSet | Pré-preenche circunferências/comprimentos | Override silencioso fora do schema |
| F1.3-3 | API gera `PatternDocument` | Chama `pattern-core`; grava versão; erros de validação explícitos | Regras no frontend |
| F1.3-4 | Abrir resultado no canvas | Navega para editor 2D com partes frente/costas | Edição destrutiva sem versionar |

---

### F1.4 — Editor 2D (`1:359`)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.4-1 | Viewport SVG das partes | Render de contornos, fio, piques, margens; pan/zoom básico | Physics / 3D |
| F1.4-2 | Lista de peças + selection | Frente/costas; highlight | Boolean ops |
| F1.4-3 | Painel de propriedades (leitura + ajustes mínimos) | Comprimento/folga/margem via **recalcular base** (não editar Bézier livre no MVP mínimo) | Curve tension livre como no mock (pode ser Fase 1.x se couber) |
| F1.4-4 | Avisos de inconsistência | Banner com issues de qualidade (`PatternQualityChecks`) | Auto-fix sem confirmação |
| F1.4-5 | Salvar versão | Nova versão do modelo; auditoria | Undo infinito |

**Nota:** o mock mostra edição fina de segmentos; o MVP Fase 1 prioriza **visualização + parâmetros**. Edição de curva livre = incremento F1.4b se o tempo permitir.

---

### F1.5 — Exportação PDF (fatia de `1:501`)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.5-1 | Job assíncrono de export | Fila/abstração; status; sem regra geométrica no exportador | DXF |
| F1.5-2 | PDF A4 com escala | Régua/escala; partes; fio; piques; margens; fator 100% marcado | “Fit to page” como escala real |
| F1.5-3 | UI Export Configuration (mínima) | Formato A4; seções: moldes (+ opcional capa); download | Plotter industrial |
| F1.5-4 | Testes de escala | Critérios de `quality-criteria.md` (±2 mm/10 cm em validação manual documentada) | |

**Agente:** `exec-export`.

---

### F1.6 — Ficha técnica básica (fatia de `1:501`)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.6-1 | Ficha mínima editável | Materiais texto, observações, tabela de medidas do MeasurementSet usado | Custos/billing (Fase 3) |
| F1.6-2 | Incluir ficha no PDF (opcional) | Toggle na export config | Tech pack completo do mock (blazer, stitching industrial) |

**Nota:** o frame Figma é rico (construction notes, stitching, measurement chart 38). Fase 1 entrega **subconjunto**; resto → Fase 3 (`exec-atelier`).

---

### F1.7 — AuthN mínimo (API)

| ID | História | Critérios de aceite | Fora |
| --- | --- | --- | --- |
| F1.7-1 | Register / login / logout / me | Password hash; cookie HttpOnly + JWT; claims `userId` + `tenant_id` | OAuth, convites |
| F1.7-2 | Gate de tenant nas rotas | `/tenants/{tenantId}/...` → 401 sem auth, 403 cross-tenant | RBAC fino |
| F1.7-3 | Bootstrap Dev | Seed `demo@modelaflow.local` / `ChangeMe!`; `POST /dev/bootstrap` só Development | Contas de produção |

**ADR:** [`ADR-0004-authn-session.md`](decisions/ADR-0004-authn-session.md). UI de login no web → incremento de `exec-studio`.

---

## Mapeamento tela → fase

| Tela Figma | Fase 1 | Fase 2+ |
| --- | --- | --- |
| Dashboard | Sim (sem IA) | CTA Upload AI |
| Editor IA | Stub / disabled | Completo + confirmação |
| Editor 2D | Sim (view + params) | Edição avançada |
| Ficha + Export | PDF + ficha mínima | Tech pack + custo |
| Clientes | Sim | Colaboração externa |
| Dark | Opcional pós-light | — |
| Tokens/Tipo | Sim | — |

---

## Critérios transversais (toda história)

- Tenant + autorização; medidas/imagens sem log aberto.
- cm no domínio; conversão só na exportação.
- Testes + critérios de aceite; docs/ADR se mudar contrato.
- IA nunca grava molde final (ADR-0001).
- Definição de pronto: `AGENTS.md`.

---

## Decisões em aberto (bloquear só se necessário)

1. Idioma da UI no MVP: **PT-BR** (recomendado) vs inglês do Figma.
2. Editor 2D: só parâmetros vs edição de curva no mesmo incremento.
3. AuthN (JWT/cookie) — **feito na API (F1.7 / ADR-0004)**; UI de login no web ainda pendente.
