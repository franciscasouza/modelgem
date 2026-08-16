# ModelaFlow

SaaS brasileiro para desenvolvimento de roupas (modelagem assistida). A IA sugere; o núcleo paramétrico calcula; a profissional confirma.

## Layout do monorepo

```text
apps/
  api/           ASP.NET Core Web API (.NET 8) — Identity, Customer, Design, Audit, Export jobs
  api.tests/     Testes xUnit (tenant, medidas, patterns)
  web/           Next.js + TypeScript (App Router)
packages/
  pattern-core/  Biblioteca C# — medidas cm + bases saia/vestido
  pattern-export/ PDF A4 a partir de PatternDocument (QuestPDF)
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
CORS: `http://localhost:3000`

Aplicar migrations (com PostgreSQL disponível):

```powershell
dotnet ef database update --project apps/api/ModelaFlow.Api.csproj --startup-project apps/api/ModelaFlow.Api.csproj
```

### Tenant de demonstração (Development)

- Seed no startup: tenant estável `11111111-1111-1111-1111-111111111111` (quando o banco está acessível).
- Ou: `POST http://localhost:5074/api/v1/dev/bootstrap` → `{ "tenantId", "organizationId" }`.

### Web

```powershell
cd apps/web
# Defina NEXT_PUBLIC_API_URL=http://localhost:5074 (ver .env.example)
npm install
npm run dev
```

### Testes

```powershell
dotnet test ModelaFlow.sln
```

## Variáveis de ambiente

Veja `.env.example`. Não commitar segredos reais.

- `NEXT_PUBLIC_API_URL` — base URL da API para o Next.js (ex.: `http://localhost:5074`).

## Pendências (não neste incremento)

- Redis, storage S3-compatível e provedor de fila (export usa job in-process)
- AuthN completa (JWT/cookies), OCR, IA, Interpretation, billing
