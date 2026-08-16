# ModelaFlow

SaaS brasileiro para desenvolvimento de roupas (modelagem assistida). A IA sugere; o núcleo paramétrico calcula; a profissional confirma.

## Layout do monorepo

```text
apps/
  api/           ASP.NET Core Web API (.NET 8) — Identity, Customer, Audit, EF Core
  api.tests/     Testes xUnit (isolamento por tenant + versionamento de medidas)
  web/           Next.js + TypeScript (App Router) — UI placeholder
packages/
  pattern-core/  Biblioteca C# — DTOs/tipos de medidas em cm (sem regras geométricas completas neste incremento)
docs/
  product-brief.md
  architecture.md
  roadmap.md
  decisions/     ADRs
  discovery/     Artefatos de descoberta (Fase 0)
```

Solução .NET: `ModelaFlow.sln`

## Pré-requisitos

- .NET 8 SDK (ou superior com targeting `net8.0`)
- Node.js 20+ (para `apps/web`)
- PostgreSQL (opcional em local; connection string configurável)

## Como rodar

### API

```powershell
cd apps/api
# Opcional: $env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=modelaflow;Username=modelaflow;Password=CHANGE_ME"
dotnet run
```

Swagger em Development: `/swagger`  
Health: `GET /health`

Aplicar migrations (com PostgreSQL disponível):

```powershell
dotnet ef database update --project apps/api/ModelaFlow.Api.csproj --startup-project apps/api/ModelaFlow.Api.csproj
```

### Web

```powershell
cd apps/web
npm install
npm run dev
```

### Testes

```powershell
dotnet test ModelaFlow.sln
```

## Variáveis de ambiente

Veja `.env.example`. Não commitar segredos reais.

## Pendências (não neste incremento)

- Redis, storage S3-compatível e fila de jobs (documentados como TODO)
- AuthN completa (JWT/cookies), OCR, IA, Interpretation, editor 2D, PDF, billing
