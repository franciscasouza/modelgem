# ADR-0003 — Exportação PDF com QuestPDF

## Status

Aceito

## Contexto

A Fase 1 exige PDF A4 com régua de escala (10 cm), marcação `escala 100% / 1:1`, partes, fio, piques e margens. O exportador não pode recalcular o molde — apenas desenhar um `PatternDocument` já gerado pelo `pattern-core`.

## Decisão

1. Novo pacote `packages/pattern-export` (`ModelaFlow.PatternExport`), referenciado pela API.
2. Biblioteca **QuestPDF** (licença Community) para gerar bytes PDF.
3. Conversão cm → pontos PDF **somente** na borda de exportação (`PointsPerCm = 72/2.54`).
4. Jobs de exportação na API: entidade `ExportJob` com ciclo `queued → running → succeeded|failed`, processados **in-process** (síncrono no MVP). Redis/fila externa permanece adiados (ADR-0002).

## Consequências

- Dependência NuGet QuestPDF no pacote de exportação.
- Preview de peças grandes pode reduzir desenho no quadro da página; a régua do cabeçalho permanece 10 cm reais e o metadado/assunto do PDF declara `escala 100% / 1:1`.
- Trocar a lib PDF no futuro não deve alterar o contrato geométrico (`pattern.v1`).
