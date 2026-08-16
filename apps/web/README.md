# apps/web — ModelaFlow Studio

Next.js (App Router) + TypeScript. UI Fase 1 (F1.0–F1.6) em português, alinhada ao Figma (`docs/design.md`).

## Como rodar

```bash
# na pasta apps/web
npm install
npm run dev
```

Abre em [http://localhost:3000](http://localhost:3000).

API esperada (ASP.NET): `http://localhost:5074` — ver `apps/api/Properties/launchSettings.json`.

```bash
# na raiz do monorepo (exemplo)
dotnet run --project apps/api --launch-profile http
```

## Variáveis de ambiente

| Variável | Default | Uso |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | `http://localhost:5074` | Base da API |
| `NEXT_PUBLIC_TENANT_ID` | — | Fallback de tenant se bootstrap/localStorage não existirem |

Tenant resolution (`src/lib/api.ts`):

1. `localStorage` key `mf_tenant_id`
2. `POST /api/v1/dev/bootstrap` (se existir)
3. `NEXT_PUBLIC_TENANT_ID`

Copie `.env.example` para `.env.local` se precisar sobrescrever.

## Rotas

| Rota | Incremento |
| --- | --- |
| `/` | F1.2 Dashboard |
| `/clients`, `/clients/[id]` | F1.1 Clientes + medidas |
| `/patterns/new` | F1.3 Wizard paramétrico |
| `/patterns/[id]/canvas` | F1.4 Editor 2D SVG |
| `/patterns/[id]/tech-pack` | F1.5/F1.6 Ficha + export |
| `/ai` | Stub Fase 2 |
| `/settings` | Placeholder |

## Scripts

- `npm run dev` — desenvolvimento (Turbopack)
- `npm run build` — build de produção
- `npm run start` — serve o build
- `npm run lint` — ESLint

## Limitações (Fase 1)

- Sem AuthN real; tenant via bootstrap/env/localStorage.
- Endpoints de patterns/overview/exports podem ainda não existir na API — a UI compila e mostra empty/warning states.
- Editor IA desabilitado (Fase 2).
- Canvas usa fixture local só se a API não serializar `PatternDocument`.
- Exportação PDF é job assíncrono na API (não gera PDF no browser).
