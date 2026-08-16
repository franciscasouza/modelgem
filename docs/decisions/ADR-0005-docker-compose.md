# ADR-0005 — Publicação local com Docker Compose

## Status

Aceito

## Contexto

O monorepo precisa de um caminho reproduzível para subir PostgreSQL, API e web sem instalar stack completa na máquina, e para publicar a Fase 1+AuthN de forma verificável.

## Decisão

1. `docker-compose.yml` na raiz com serviços `db` (Postgres 16), `api` (.NET 8) e `web` (Next.js standalone).
2. `NEXT_PUBLIC_API_URL` aponta para a URL **do browser no host** (ex.: `http://localhost:5074`), não para o hostname interno `api`.
3. API aplica migrations quando `Database__ApplyMigrations=true` (compose define isso).
4. CORS via `Cors__Origins` (default `http://localhost:3000`).
5. Dockerfiles: `apps/api/Dockerfile` (contexto monorepo) e `apps/web/Dockerfile` (contexto `apps/web`).

## Consequências

- `docker compose up --build` sobe o studio completo.
- Segredos (`POSTGRES_PASSWORD`, `AUTH_JWT_SIGNING_KEY`) devem ser trocados fora de demo.
- Imagens não incluem Redis/S3 ainda (ADR-0002).
