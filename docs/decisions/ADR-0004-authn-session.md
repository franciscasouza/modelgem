# ADR-0004 — AuthN mínimo (cookie HttpOnly + JWT)

## Status

Aceito

## Contexto

Antes do upload de imagens (Fase 2), a API precisa de autenticação multi-tenant real: rotas `/api/v1/tenants/{tenantId}/...` não podem permanecer abertas. O frontend em `http://localhost:3000` consome a API com CORS e credenciais.

## Decisão

1. **Password hashing:** `Microsoft.AspNetCore.Identity.PasswordHasher<User>` — senha nunca em texto aberto; colunas `PasswordHash` e `SecurityStamp` em `users`.
2. **Sessão:** JWT assinado (HMAC-SHA256) emitido em `register`/`login`, gravado em cookie **HttpOnly** `mf_auth` (`SameSite=Lax`; `Secure` em Production). O mesmo JWT é aceito no header `Authorization: Bearer`.
3. **Claims:** `sub` / `userId`, `tenant_id`, email, role, `organization_name`, `security_stamp`.
4. **Autorização de tenant:** middleware exige usuário autenticado e `tenantId` do path igual ao claim; caso contrário 401/403.
5. **Endpoints:** `POST /api/v1/auth/register|login|logout`, `GET /api/v1/auth/me`.
6. **Dev:** seed/bootstrap cria tenant estável + `demo@modelaflow.local` / `ChangeMe!` (somente Development). `POST /api/v1/dev/bootstrap` → 404 fora de Development.
7. **Audit:** `auth.register`, `auth.login`, `auth.logout` (metadata sem senha).
8. **CORS:** origem explícita `http://localhost:3000` + `AllowCredentials`.

## Fora de escopo (este ADR)

OAuth, convites, RBAC fino, refresh tokens, Redis de sessão.

## Consequências

- Isolamento de tenant na borda HTTP antes de upload/OCR.
- Clientes browser usam cookie; testes/CLI podem usar Bearer.
- Chave `Auth:JwtSigningKey` deve ser trocada em produção (≥ 32 chars).
- Email é único globalmente (login por email).
