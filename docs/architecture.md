# Arquitetura inicial

## Direção técnica

Aplicação web/PWA com API modular, banco relacional, armazenamento de objetos e processamento assíncrono. A arquitetura deve permitir evolução para multi-tenant, sem introduzir microserviços antes da necessidade.

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
apps/api.tests              Testes de tenant e versionamento
apps/web                    Next.js (App Router) — UI placeholder
packages/pattern-core       Medidas + modelo geométrico + bases saia/vestido
packages/pattern-core.tests Testes de regressão / determinismo / validação
docs/                       Brief, arquitetura, roadmap, ADRs, discovery
```

Solução: `ModelaFlow.sln`. Multi-tenant por `tenant_id` em toda entidade de domínio (ver ADR-0002). Redis, S3 e filas permanecem previstos, ainda sem implementação neste incremento.

## Módulos de domínio

- Identity: usuários, organizações, papéis e permissões.
- Customer: clientes e medidas versionadas.
- Design: modelos, referências, componentes e versões.
- Pattern: pontos, linhas, curvas, partes, regras paramétricas e validações.
- Interpretation: resultados de IA, confiança, confirmações e avisos.
- TechnicalSheet: materiais, aviamentos, montagem e observações.
- Costing: consumo, mão de obra, despesas, preço e margem.
- Files: origem, armazenamento, hash, permissões e ciclo de vida.
- Billing: planos, créditos, consumo e pagamentos.
- Audit: eventos relevantes e trilha de alterações.

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

- isolamento por `tenant_id`;
- autorização em toda operação de arquivo e domínio;
- criptografia em trânsito e em repouso;
- consentimento para imagens de clientes;
- minimização de dados corporais;
- logs sem expor imagens ou medidas desnecessariamente;
- exclusão e exportação de dados;
- backups e restauração testados.

## Observabilidade

Registrar duração e falha de jobs, versão do interpretador, custo de IA, exportações, erros de escala e validações rejeitadas. Não registrar conteúdo sensível em texto aberto.
