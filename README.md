# ModelaFlow

SaaS brasileiro para desenvolvimento de roupas (modelagem assistida). A IA sugere; o núcleo paramétrico calcula; a profissional confirma.

## Layout do monorepo

```text
apps/
  api/           ASP.NET Core Web API (.NET 8) — Identity, Customer, Design, Audit, Auth, Export
  api.tests/     Testes xUnit
  web/           Next.js + TypeScript (App Router)
packages/
  pattern-core/  Geometria paramétrica (cm)
  pattern-export/ PDF A4 (QuestPDF)
docs/
docker-compose.yml
```

## Publicar com Docker (recomendado)

Pré-requisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/) (ou Engine + Compose v2).

```powershell
# Na raiz do repositório
copy .env.example .env   # opcional — ajuste senhas/chaves
docker compose up --build
```

| Serviço | URL |
| --- | --- |
| Web | http://localhost:3000 |
| API / health | http://localhost:5074/health |
| Swagger (se `ASPNETCORE_ENVIRONMENT=Development`) | http://localhost:5074/swagger |
| Postgres | localhost:5432 |

Login demo (Development): `demo@modelaflow.local` / `ChangeMe!`  
Ou registre em `/register`.

Parar: `docker compose down` · dados: volume `modelaflow_pg`.

Detalhes: `docs/decisions/ADR-0005-docker-compose.md`.

## Desenvolvimento sem Docker

### API

```powershell
cd apps/api
dotnet run
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

## Autenticação

- Cookie HttpOnly `mf_auth` (JWT) ou `Authorization: Bearer`
- ADR: `docs/decisions/ADR-0004-authn-session.md`

## Variáveis

Veja `.env.example` (Compose e local). Não commitar segredos.
