# ADR-0005 — Publicação local com Docker Compose

## Status

Aceito

## Contexto

O monorepo precisa de um caminho reproduzível para subir PostgreSQL, API e web sem instalar stack completa na máquina, e para publicar a Fase 1+AuthN de forma verificável.

## Decisão

1. `docker-compose.yml` na raiz com serviços `db` (Postgres 16), `api` (.NET 8), `web` (Next.js) e `gateway` (nginx).
2. **Entrada do browser:** `http://localhost:8080` (gateway). Nginx faz proxy de `/` → web e `/api` → API na mesma origem (cookies/CORS estáveis no Windows).
3. `NEXT_PUBLIC_API_URL=http://localhost:8080` (mesma origem do gateway).
4. API aplica migrations quando `Database__ApplyMigrations=true`.
5. CORS via `Cors__Origins` (inclui `http://localhost:8080`).
6. Dockerfiles: `apps/api/Dockerfile` (contexto monorepo) e `apps/web/Dockerfile` (contexto `apps/web`).

## Consequências

- Abrir `localhost:3000` / `3080` / `5074` separados no browser é frágil no Windows; use **:8080**.
- `docker compose up --build` sobe o studio completo.
- Segredos (`POSTGRES_PASSWORD`, `AUTH_JWT_SIGNING_KEY`) devem ser trocados fora de demo.
