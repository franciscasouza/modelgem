# Arquitetura inicial

## Direção técnica

Aplicação web/PWA com API modular, banco relacional, armazenamento de objetos e processamento assíncrono. A arquitetura deve permitir evolução para multi-tenant, sem introduzir microserviços antes da necessidade.

Publicação local: `docker compose up --build` (Postgres + API + web) — ver ADR-0005.

## Stack de referência

- Frontend: React, Next.js e TypeScript.
- Editor: SVG/Canvas com geometria vetorial.
- Backend: ASP.NET Core em .NET 8.
- Persistência: PostgreSQL com Entity Framework Core.
- Cache e coordenação: Redis.
- Arquivos: armazenamento compatível com S3.
- Filas: abstração de jobs; o provedor pode ser escolhido na implementação.
- IA: adaptador multimodal/OCR desacoplado do domínio.
- Exportação: serviços separados para SVG, PDF e DXF.

## Layout do monorepo (Fase 1)

```text
apps/api                    API ASP.NET Core (.NET 8) + EF Core
apps/api.tests              Testes de tenant, versionamento e Design
apps/web                    Next.js (App Router)
packages/pattern-core       Medidas + modelo geométrico + bases saia/vestido
packages/pattern-core.tests Testes de regressão / determinismo / validação
packages/pattern-export     PDF A4 a partir de PatternDocument (QuestPDF)
packages/pattern-export.tests
docs/                       Brief, arquitetura, roadmap, ADRs, discovery
```

Solução: `ModelaFlow.sln`. Multi-tenant por `tenant_id` em toda entidade de domínio (ver ADR-0002). Redis e S3 permanecem previstos; jobs de export PDF rodam in-process no MVP (ADR-0003).

## Módulos de domínio

- Identity: usuários, organizações, papéis e permissões.
- Customer: clientes e medidas versionadas.
- Design: `PatternModel`, `PatternVersion` (append-only), `TechnicalSheet` mínima, `ExportJob`.
- Pattern: pontos, linhas, curvas, partes, regras paramétricas e validações (`packages/pattern-core`).
- Interpretation: resultados de IA, confiança, confirmações e avisos (Fase 2).
- TechnicalSheet: materiais, aviamentos, montagem e observações (mínimo na Fase 1).
- Costing: consumo, mão de obra, despesas, preço e margem.
- Files: origem, armazenamento, hash, permissões e ciclo de vida.
- Billing: planos, créditos, consumo e pagamentos.
- Audit: eventos relevantes e trilha de alterações.

## Design + geração (Fase 1)

Fluxo HTTP (prefixo `/api/v1/tenants/{tenantId}`):

1. Criar `PatternModel` (`straight_skirt` | `simple_dress` | `blank`).
2. `POST .../patterns/{id}/generate` chama `StraightSkirtPattern` / `SimpleDressPattern`, grava `PatternVersion` com `ParametersJson` + `GeometryJson` (`pattern.v1`) e `QualityIssuesJson`.
3. Validação (`PatternValidationException`) → HTTP 400 com `details` — sem cálculo silencioso.
4. Ficha técnica: `GET/PUT .../technical-sheet` (materiais / construção).
5. Overview: `GET .../overview` → contagens reais de clientes e modelos.

CORS em Development/API: origem `http://localhost:3000` com `AllowCredentials` (cookies). Frontend: `NEXT_PUBLIC_API_URL` (ver `.env.example`).

### AuthN (F1.7 / ADR-0004)

- `POST /api/v1/auth/register` — cria Organization (`TenantId` = Org.Id) + User (Owner) com password hash; seta cookie HttpOnly `mf_auth` (JWT).
- `POST /api/v1/auth/login` / `logout` / `GET /api/v1/auth/me`.
- JWT também aceito via `Authorization: Bearer`. Claims: `sub`/`userId`, `tenant_id`.
- Rotas `/api/v1/tenants/{tenantId}/...` exigem autenticação e match do `tenant_id` (401/403).
- Audit: `auth.register` / `auth.login` / `auth.logout` (sem senha nos logs).

### Bootstrap de tenant (Development)

Em Development, seed no startup cria o tenant estável `11111111-1111-1111-1111-111111111111` com usuário `demo@modelaflow.local` / `ChangeMe!` quando o banco está acessível. Alternativa: `POST /api/v1/dev/bootstrap` (somente Development; 404 em Production) → `{ tenantId, organizationId }` e garante a senha demo. Em produção use `register`.

## Exportação PDF

- Pacote `pattern-export`: entrada `PatternDocument` desserializado; saída bytes PDF A4.
- Não recalcula molde; desenha contornos stitch/cut, labels, fio, piques, margens, régua 10 cm e texto `escala 100% / 1:1`.
- Job: `POST .../patterns/{id}/exports` → `ExportJob`; `GET .../exports/{jobId}` (status + download URL); `GET .../exports/{jobId}/download` (bytes).

## Fluxo de referência

```text
upload → normalização → interpretação assistida → confirmação →
base paramétrica → cálculo geométrico → revisão → exportação → versionamento
```

## Contrato de interpretação

A IA deve retornar dados estruturados e nunca gravar diretamente um molde final. O resultado deve conter `schema_version`, campos identificados, campos estimados, confiança, evidências e perguntas pendentes. A aplicação só transforma a interpretação em parâmetros após confirmação.

## Modelo geométrico mínimo

O domínio deve representar pontos, segmentos, curvas, medidas, relações, margens, piques, fio e partes. As regras precisam ser determinísticas e reproduzíveis para a mesma entrada. Exportadores não podem conter regras de negócio.

## Segurança e privacidade

- isolamento por `tenant_id` (filtro de domínio + AuthN com match de claim — ADR-0004);
- autorização em toda operação de arquivo e domínio;
- senhas com hash (PasswordHasher); sessão via cookie HttpOnly + JWT;
- criptografia em trânsito e em repouso;
- consentimento para imagens de clientes;
- minimização de dados corporais;
- logs sem expor imagens, medidas ou senhas;
- exclusão e exportação de dados;
- backups e restauração testados.

## Observabilidade

Registrar duração e falha de jobs, versão do interpretador, custo de IA, exportações, erros de escala e validações rejeitadas. Não registrar conteúdo sensível em texto aberto.
