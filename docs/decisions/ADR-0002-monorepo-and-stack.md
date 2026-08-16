# ADR-0002 — Monorepo e stack da plataforma

## Status

Aceito

## Contexto

O ModelaFlow precisa de uma base multi-tenant testável antes de IA, editor ou exportação. A organização do código deve separar API, UI e núcleo de medidas/geometria, sem acoplar regras geométricas aos controllers.

## Decisão

1. **Monorepo** com layout:
   - `apps/api` — ASP.NET Core Web API (.NET 8)
   - `apps/web` — Next.js + TypeScript (App Router)
   - `packages/pattern-core` — biblioteca C# (.NET 8) com medidas em cm, modelo geométrico e bases paramétricas (saia/vestido)
2. **Persistência**: PostgreSQL + Entity Framework Core; connection string via `appsettings` / variáveis de ambiente.
3. **Multi-tenant**: toda entidade de domínio carrega `tenant_id`; consultas e escritas de domínio filtram por ele. Nesta fase, `Organization.Id` == `TenantId`.
4. **Evolução posterior**: Redis, storage S3-compatível, provedor de fila (export PDF in-process no MVP — ADR-0003), OAuth/RBAC fino (AuthN mínimo — ADR-0004), OCR/IA, editor 2D UI, billing.

## Consequências

- Isolamento por tenant desde o primeiro CRUD (clientes e medidas versionadas).
- `pattern-core` pode evoluir regras geométricas sem misturar com infraestrutura HTTP.
- Migrations versionam o schema; testes usam provider InMemory para validar filtro de tenant e versionamento de forma determinística.
- Dependências de cache/fila/arquivo entram em ADRs futuros quando forem adotadas.
