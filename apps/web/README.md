# apps/web — ModelaFlow Studio

Next.js (App Router) + TypeScript. UI Fase 1 (F1.0–F1.7) em português, alinhada ao Figma (`docs/design.md`).

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

## Fluxo AuthN (F1.7)

1. Abrir `/register` — criar organização (nome org, e-mail, nome, senha).
2. Se o register não abrir sessão automaticamente, entrar em `/login`.
3. Cookie HttpOnly é enviado em todos os `fetch` com `credentials: "include"`.
4. Studio (`/`, clientes, moldes…) exige sessão: sem cookie válido → redirect `/login`.
5. Já autenticada em `/login` ou `/register` → redirect `/`.
6. Sidebar / Config mostram organização + usuária; **Sair** chama `POST /api/v1/auth/logout`.

### CORS + credentials

A API em Development deve permitir origem `http://localhost:3000` **com credentials** (`AllowCredentials` + origem explícita — não `*`). Sem isso o browser bloqueia o cookie e `/auth/me` falha após o login.

### Contrato usado pelo client (`src/lib/api.ts`)

| Método | Path | Body / resposta |
| --- | --- | --- |
| POST | `/api/v1/auth/register` | `{ organizationName, email, displayName?, password }` |
| POST | `/api/v1/auth/login` | `{ email, password }` |
| POST | `/api/v1/auth/logout` | — |
| GET | `/api/v1/auth/me` | `{ userId, email, displayName, tenantId, organizationName, role }` |

Tenant das rotas `/api/v1/tenants/{tenantId}/...` vem de `/me` (sessão em memória). **Não** usar `localStorage.mf_tenant_id` no fluxo autenticado.

### Fallback Dev (só se `/me` falhar)

Ordem: `POST /api/v1/dev/bootstrap` → `NEXT_PUBLIC_TENANT_ID` → tenant seed fixo. Preferir AuthN; o fallback existe para ambiente local enquanto a API Auth ainda sobe.

## Variáveis de ambiente

| Variável | Default | Uso |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | `http://localhost:5074` | Base da API |
| `NEXT_PUBLIC_TENANT_ID` | — | Fallback Dev se `/me` e bootstrap falharem |

Copie `.env.example` para `.env.local` se precisar sobrescrever.

## Rotas

| Rota | Incremento |
| --- | --- |
| `/login`, `/register` | F1.7 AuthN (layout público) |
| `/` | F1.2 Dashboard |
| `/clients`, `/clients/[id]` | F1.1 Clientes + medidas |
| `/patterns/new` | F1.3 Wizard paramétrico |
| `/patterns/[id]/canvas` | F1.4 Editor 2D SVG |
| `/patterns/[id]/tech-pack` | F1.5/F1.6 Ficha + export |
| `/ai` | Stub Fase 2 |
| `/settings` | Conta / org / sair |

## Scripts

- `npm run dev` — desenvolvimento (Turbopack)
- `npm run build` — build de produção
- `npm run start` — serve o build
- `npm run lint` — ESLint

## Limitações (Fase 1)

- AuthN depende da API F1.7; se os endpoints ainda não existirem, a UI compila e mostra erro claro (404/conexão/CORS).
- Fallback Dev de tenant permanece documentado, mas o fluxo preferido é cookie + `/me`.
- Editor IA desabilitado (Fase 2).
- Canvas usa fixture local só se a API não serializar `PatternDocument`.
- Exportação PDF é job assíncrono na API (não gera PDF no browser).
