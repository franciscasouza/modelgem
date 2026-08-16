# Núcleo geométrico (`pattern-core`)

Biblioteca C# determinística para bases paramétricas do MVP. Unidade de domínio: **centímetros**. Sem OCR/LLM no cálculo.

| Campo | Valor |
| --- | --- |
| Pacote | `packages/pattern-core` (`ModelaFlow.PatternCore`) |
| Schema geométrico | `pattern.v1` (`PatternDocument.SchemaVersion` = 1) |
| Medidas canônicas | `measurements.v1` (ver `docs/discovery/measurement-schema.md`) |
| Bases | `straight_skirt.v1`, `simple_dress.v1` |

## Modelo geométrico mínimo

| Tipo | Papel |
| --- | --- |
| `Point2D`, `Segment2D`, `CubicBezier2D` | Primitivas em cm |
| `PathEdge` / `Contour2D` | Contorno de costura (`Stitch`) e de corte (`Cut`) |
| `PatternPiece` | Parte com margens, piques (`Notch`), fio (`Grainline`) |
| `PatternDocument` | Resultado versionado da base |

Sistema de coordenadas das bases v1: **X = 0 no centro** da parte; **+X** para a costura lateral direita; **+Y** da cintura/ombro em direção à barra.

Política de fio MVP: `parallel_center` (paralelo ao eixo longitudinal no centro).

## Decisões MVP (congeladas neste incremento)

1. **Saia e vestido: sempre 2 partes** (frente + costas), simétricas. Sem opção de 1 peça com costura única na base v1.
2. **`waistband_height` = 0** na saia v1 — cós separado fica fora da base (ficha/acabamento posterior).
3. Vestido v1: silhueta reta/levemente acinturada; **sem mangas complexas**; decote/topo em linha reta; `length_from` = `shoulder` apenas.

## `StraightSkirtPattern` (`straight_skirt.v1`)

Parâmetros (cm):

| Id | Obrig. | Default | Faixa |
| --- | --- | --- | --- |
| `waist_circ` | sim | — | 50–150 |
| `hip_circ` | sim | — | 70–180 |
| `skirt_length` | sim | — | 30–120 |
| `ease_waist` | não | 2 | 0–8 |
| `ease_hip` | não | 4 | 0–12 |
| `waist_to_hip` | não | 20 | 14–30 |
| `seam_allowance` | não | 1,0 | 0,5–2,5 |
| `hem_allowance` | não | 3 | 1–8 |
| `waistband_height` | não | 0 | deve ser 0 |
| `length_includes_hem` | não | false | — |

Regras: `hip+ease_hip ≥ waist+ease_waist`; `skirt_length ≥ waist_to_hip + 8`. Largura de cada parte na estação = circunferência efetiva / 2 (half-width centro→lateral = circ/4).

## `SimpleDressPattern` (`simple_dress.v1`)

Parâmetros (cm): `bust_circ`, `waist_circ`, `hip_circ`, `dress_length` + folgas; opcionais `shoulder_to_bust` (26), `bust_to_waist` (20), `waist_to_hip` (20), margens iguais à saia.

Regras: `hip+ease ≥ waist+ease`; se `length_from=shoulder`, `dress_length ≥ shoulder_to_bust + bust_to_waist + waist_to_hip + 10`. Aviso (não erro) se busto efetivo &lt; cintura efetiva.

## Validação

Valores fora de min–max ou inconsistentes → `PatternValidationException` (`validation_failed` + detalhes). **Não há cálculo silencioso** de geometria inválida. Falhas podem ser mapeadas para `PatternCalculationFailure` (observabilidade básica, sem dump sensível obrigatório).

## Limitações conhecidas

- Sem dardos, godês, gradação, DXF, editor ou PDF neste pacote.
- Laterais: Bézier cúbica suave entre estações (controles no cordão) — suficiente para teste, não substitui modelagem industrial.
- Vestido sem cava/manga/decote modelado.
- Critérios de qualidade de impressão A4 ficam para exportação (`exec-export`).

## API pública (C#)

```csharp
var skirt = StraightSkirtPattern.Generate(new StraightSkirtInput {
    WaistCirc = 70, HipCirc = 96, SkirtLength = 60
});
var dress = SimpleDressPattern.Generate(new SimpleDressInput {
    BustCirc = 90, WaistCirc = 72, HipCirc = 98, DressLength = 110
});
```

Wiring HTTP fica fora deste incremento.

## Testes

```bash
dotnet test ModelaFlow.sln
```

Projeto: `packages/pattern-core.tests` — regressão de larguras/comprimento, determinismo, validação e critérios mínimos (fio, margem, frente/costas, piques).
