/** Types aligned with pattern-core + expected API contracts (camelCase JSON). */

export type PieceSide = "Front" | "Back" | 0 | 1;
export type EdgeKind = "Segment" | "CubicBezier" | 0 | 1;
export type EdgeRole = "Stitch" | "Cut" | "Construction" | 0 | 1 | 2;
export type NotchKind = "SideSeam" | "Center" | "Construction" | 0 | 1 | 2;

export interface Point2D {
  x: number;
  y: number;
}

export interface Segment2D {
  start: Point2D;
  end: Point2D;
}

export interface CubicBezier2D {
  p0: Point2D;
  p1: Point2D;
  p2: Point2D;
  p3: Point2D;
}

export interface PathEdge {
  kind: EdgeKind;
  role: EdgeRole;
  segment?: Segment2D | null;
  curve?: CubicBezier2D | null;
  label?: string | null;
}

export interface Contour2D {
  isClosed: boolean;
  edges: PathEdge[];
}

export interface Grainline {
  start: Point2D;
  end: Point2D;
  policy?: string;
}

export interface Notch {
  id: string;
  position: Point2D;
  kind: NotchKind;
  pairKey?: string | null;
  label?: string | null;
}

export interface MarginSpec {
  seamAllowanceCm: number;
  hemAllowanceCm: number;
}

export interface PatternPiece {
  id: string;
  name: string;
  side: PieceSide;
  quantityToCut: number;
  onFold: boolean;
  stitchContour: Contour2D;
  cutContour: Contour2D;
  grainline: Grainline;
  notches: Notch[];
  margins: MarginSpec;
  notes?: string | null;
}

export interface PatternDocument {
  schemaId?: string;
  schemaVersion?: number;
  unit?: string;
  baseId: string;
  baseVersion: string;
  pieces: PatternPiece[];
  resolvedParametersCm: Record<string, number>;
  limitations?: string[];
}

export interface Customer {
  id: string;
  tenantId: string;
  name: string;
  notes?: string | null;
  createdAt: string;
}

export interface MeasurementSet {
  id: string;
  tenantId: string;
  customerId: string;
  version: number;
  schemaVersion: number;
  unit: string;
  valuesCm: Record<string, number>;
  createdByUserId?: string | null;
  createdAt: string;
}

export type PatternBaseId = "straight_skirt" | "simple_dress" | "blank";

export interface PatternSummary {
  id: string;
  tenantId: string;
  name: string;
  reference?: string | null;
  baseId: PatternBaseId | string;
  customerId?: string | null;
  customerName?: string | null;
  version?: number;
  createdAt: string;
  updatedAt?: string;
  status?: string | null;
}

export interface PatternDetail extends PatternSummary {
  document?: PatternDocument | null;
  qualityIssues?: string[];
  materialsNotes?: string | null;
  constructionNotes?: string | null;
  measurementSetId?: string | null;
}

export interface OverviewCounts {
  patternsCount: number;
  customersCount: number;
  pendingApprovalsCount?: number | null;
}

export interface TechnicalSheet {
  patternId: string;
  materialsNotes: string;
  constructionNotes: string;
  measurementsCm?: Record<string, number> | null;
  updatedAt?: string;
}

export type ExportJobStatus = "queued" | "running" | "succeeded" | "failed";

export interface ExportJob {
  id: string;
  patternId: string;
  status: ExportJobStatus | string;
  format?: string;
  includeTechnicalSheet?: boolean;
  downloadUrl?: string | null;
  error?: string | null;
  createdAt?: string;
  completedAt?: string | null;
}

export interface BootstrapResponse {
  tenantId: string;
  organizationId?: string;
  userId?: string;
}

export interface CreateCustomerInput {
  name: string;
  notes?: string;
}

export interface CreateMeasurementSetInput {
  bustCircCm?: number | null;
  waistCircCm?: number | null;
  hipCircCm?: number | null;
  skirtLengthCm?: number | null;
  dressLengthCm?: number | null;
  shoulderWidthCm?: number | null;
  waistToHipCm?: number | null;
  easeBustCm?: number | null;
  easeWaistCm?: number | null;
  easeHipCm?: number | null;
}

export interface GeneratePatternInput {
  name: string;
  baseId: "straight_skirt" | "simple_dress";
  customerId?: string | null;
  measurementSetId?: string | null;
  parameters: Record<string, number>;
}

export interface CreateBlankPatternInput {
  name?: string;
}
