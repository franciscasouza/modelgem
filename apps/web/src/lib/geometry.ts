import type { Contour2D, PathEdge, PatternDocument, Point2D } from "./types";

function edgeKind(kind: PathEdge["kind"]): "segment" | "bezier" {
  if (kind === "CubicBezier" || kind === 1) return "bezier";
  return "segment";
}

/** Build SVG path `d` from a contour (cm coordinates). */
export function contourToPathD(contour: Contour2D | null | undefined): string {
  if (!contour?.edges?.length) return "";

  const parts: string[] = [];
  let started = false;

  for (const edge of contour.edges) {
    const kind = edgeKind(edge.kind);
    if (kind === "segment" && edge.segment) {
      const { start, end } = edge.segment;
      if (!started) {
        parts.push(`M ${n(start.x)} ${n(start.y)}`);
        started = true;
      }
      parts.push(`L ${n(end.x)} ${n(end.y)}`);
    } else if (kind === "bezier" && edge.curve) {
      const { p0, p1, p2, p3 } = edge.curve;
      if (!started) {
        parts.push(`M ${n(p0.x)} ${n(p0.y)}`);
        started = true;
      }
      parts.push(
        `C ${n(p1.x)} ${n(p1.y)} ${n(p2.x)} ${n(p2.y)} ${n(p3.x)} ${n(p3.y)}`,
      );
    }
  }

  if (contour.isClosed) parts.push("Z");
  return parts.join(" ");
}

function n(v: number): string {
  return Number(v).toFixed(3);
}

export interface Bounds {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;
}

export function collectPoints(doc: PatternDocument): Point2D[] {
  const pts: Point2D[] = [];
  for (const piece of doc.pieces ?? []) {
    for (const contour of [piece.cutContour, piece.stitchContour]) {
      for (const edge of contour?.edges ?? []) {
        if (edge.segment) {
          pts.push(edge.segment.start, edge.segment.end);
        }
        if (edge.curve) {
          pts.push(edge.curve.p0, edge.curve.p1, edge.curve.p2, edge.curve.p3);
        }
      }
    }
    if (piece.grainline) {
      pts.push(piece.grainline.start, piece.grainline.end);
    }
    for (const notch of piece.notches ?? []) {
      pts.push(notch.position);
    }
  }
  return pts;
}

export function boundsOf(points: Point2D[], pad = 2): Bounds {
  if (!points.length) {
    return { minX: -10, minY: -10, maxX: 40, maxY: 80 };
  }
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;
  for (const p of points) {
    minX = Math.min(minX, p.x);
    minY = Math.min(minY, p.y);
    maxX = Math.max(maxX, p.x);
    maxY = Math.max(maxY, p.y);
  }
  return {
    minX: minX - pad,
    minY: minY - pad,
    maxX: maxX + pad,
    maxY: maxY + pad,
  };
}

export function viewBoxString(b: Bounds): string {
  const w = Math.max(1, b.maxX - b.minX);
  const h = Math.max(1, b.maxY - b.minY);
  return `${b.minX} ${b.minY} ${w} ${h}`;
}

export function sideLabel(side: string | number): string {
  if (side === "Front" || side === 0) return "Frente";
  if (side === "Back" || side === 1) return "Costas";
  return String(side);
}

export function baseLabel(baseId: string): string {
  switch (baseId) {
    case "straight_skirt":
      return "Saia reta";
    case "simple_dress":
      return "Vestido simples";
    case "blank":
      return "Em branco";
    default:
      return baseId;
  }
}

export function formatCm(value: number | undefined | null): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `${Number(value).toFixed(1)} cm`;
}

export function formatDate(iso: string | undefined | null): string {
  if (!iso) return "—";
  try {
    return new Intl.DateTimeFormat("pt-BR", {
      dateStyle: "short",
      timeStyle: "short",
    }).format(new Date(iso));
  } catch {
    return iso;
  }
}

/** Measurement key labels (measurements.v1). */
export const MEASUREMENT_LABELS: Record<string, string> = {
  bust_circ: "Busto",
  waist_circ: "Cintura",
  hip_circ: "Quadril",
  skirt_length: "Comprimento saia",
  dress_length: "Comprimento vestido",
  shoulder_width: "Largura ombro",
  waist_to_hip: "Cintura → quadril",
  ease_bust: "Folga busto",
  ease_waist: "Folga cintura",
  ease_hip: "Folga quadril",
  seam_allowance: "Margem costura",
  hem_allowance: "Bainha",
};

export const QUALITY_LABELS: Record<string, string> = {
  no_pieces: "Documento sem peças",
  missing_front_or_back: "Falta frente ou costas",
  grainline_policy: "Política de fio inválida",
  grainline_not_parallel_center: "Fio fora do centro",
  missing_seam_allowance: "Sem margem de costura",
  missing_hem_allowance: "Sem bainha",
  empty_stitch: "Contorno de costura vazio",
  empty_cut: "Contorno de corte vazio",
  missing_notches: "Sem piques",
  missing_name: "Peça sem nome",
  invalid_quantity: "Quantidade inválida",
};

export function qualityLabel(code: string): string {
  if (QUALITY_LABELS[code]) return QUALITY_LABELS[code];
  const parts = code.split(":");
  if (parts.length === 2) {
    const detail = QUALITY_LABELS[parts[1]] ?? parts[1];
    return `${parts[0]}: ${detail}`;
  }
  return code;
}
