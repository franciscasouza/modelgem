import type { PatternDocument } from "../types";

/**
 * Fixture local para desenvolvimento visual do canvas quando a API
 * ainda não serializa PatternDocument. Preferir sempre a API em runtime.
 */
export const MOCK_SKIRT_DOCUMENT: PatternDocument = {
  schemaId: "pattern.v1",
  schemaVersion: 1,
  unit: "cm",
  baseId: "straight_skirt",
  baseVersion: "v1",
  resolvedParametersCm: {
    waist_circ: 70,
    hip_circ: 96,
    skirt_length: 55,
    ease_waist: 2,
    ease_hip: 4,
    waist_to_hip: 20,
    seam_allowance: 1,
    hem_allowance: 3,
  },
  limitations: [
    "Fixture local — substituída pelo documento da API quando disponível.",
    "MVP: 2 partes (frente + costas), simétricas.",
  ],
  pieces: [
    {
      id: "skirt_front",
      name: "Saia frente",
      side: "Front",
      quantityToCut: 1,
      onFold: false,
      margins: { seamAllowanceCm: 1, hemAllowanceCm: 3 },
      grainline: { start: { x: 0, y: 2 }, end: { x: 0, y: 53 }, policy: "parallel_center" },
      notches: [
        { id: "n1", position: { x: -18.5, y: 20 }, kind: "SideSeam", pairKey: "hip" },
        { id: "n2", position: { x: 18.5, y: 20 }, kind: "SideSeam", pairKey: "hip" },
      ],
      stitchContour: {
        isClosed: true,
        edges: [
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: -18, y: 0 }, end: { x: 18, y: 0 } },
          },
          {
            kind: "CubicBezier",
            role: "Stitch",
            curve: {
              p0: { x: 18, y: 0 },
              p1: { x: 18.2, y: 8 },
              p2: { x: 24, y: 14 },
              p3: { x: 24.5, y: 20 },
            },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: 24.5, y: 20 }, end: { x: 24.5, y: 55 } },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: 24.5, y: 55 }, end: { x: -24.5, y: 55 } },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: -24.5, y: 55 }, end: { x: -24.5, y: 20 } },
          },
          {
            kind: "CubicBezier",
            role: "Stitch",
            curve: {
              p0: { x: -24.5, y: 20 },
              p1: { x: -24, y: 14 },
              p2: { x: -18.2, y: 8 },
              p3: { x: -18, y: 0 },
            },
          },
        ],
      },
      cutContour: {
        isClosed: true,
        edges: [
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: -19, y: -1 }, end: { x: 19, y: -1 } },
          },
          {
            kind: "CubicBezier",
            role: "Cut",
            curve: {
              p0: { x: 19, y: -1 },
              p1: { x: 19.3, y: 8 },
              p2: { x: 25.3, y: 14 },
              p3: { x: 25.5, y: 20 },
            },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: 25.5, y: 20 }, end: { x: 25.5, y: 58 } },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: 25.5, y: 58 }, end: { x: -25.5, y: 58 } },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: -25.5, y: 58 }, end: { x: -25.5, y: 20 } },
          },
          {
            kind: "CubicBezier",
            role: "Cut",
            curve: {
              p0: { x: -25.5, y: 20 },
              p1: { x: -25.3, y: 14 },
              p2: { x: -19.3, y: 8 },
              p3: { x: -19, y: -1 },
            },
          },
        ],
      },
    },
    {
      id: "skirt_back",
      name: "Saia costas",
      side: "Back",
      quantityToCut: 1,
      onFold: false,
      margins: { seamAllowanceCm: 1, hemAllowanceCm: 3 },
      grainline: { start: { x: 0, y: 2 }, end: { x: 0, y: 53 }, policy: "parallel_center" },
      notches: [
        { id: "n3", position: { x: -18.5, y: 20 }, kind: "SideSeam", pairKey: "hip" },
        { id: "n4", position: { x: 18.5, y: 20 }, kind: "SideSeam", pairKey: "hip" },
      ],
      stitchContour: {
        isClosed: true,
        edges: [
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: -18, y: 0 }, end: { x: 18, y: 0 } },
          },
          {
            kind: "CubicBezier",
            role: "Stitch",
            curve: {
              p0: { x: 18, y: 0 },
              p1: { x: 18.2, y: 8 },
              p2: { x: 24, y: 14 },
              p3: { x: 24.5, y: 20 },
            },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: 24.5, y: 20 }, end: { x: 24.5, y: 55 } },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: 24.5, y: 55 }, end: { x: -24.5, y: 55 } },
          },
          {
            kind: "Segment",
            role: "Stitch",
            segment: { start: { x: -24.5, y: 55 }, end: { x: -24.5, y: 20 } },
          },
          {
            kind: "CubicBezier",
            role: "Stitch",
            curve: {
              p0: { x: -24.5, y: 20 },
              p1: { x: -24, y: 14 },
              p2: { x: -18.2, y: 8 },
              p3: { x: -18, y: 0 },
            },
          },
        ],
      },
      cutContour: {
        isClosed: true,
        edges: [
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: -19, y: -1 }, end: { x: 19, y: -1 } },
          },
          {
            kind: "CubicBezier",
            role: "Cut",
            curve: {
              p0: { x: 19, y: -1 },
              p1: { x: 19.3, y: 8 },
              p2: { x: 25.3, y: 14 },
              p3: { x: 25.5, y: 20 },
            },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: 25.5, y: 20 }, end: { x: 25.5, y: 58 } },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: 25.5, y: 58 }, end: { x: -25.5, y: 58 } },
          },
          {
            kind: "Segment",
            role: "Cut",
            segment: { start: { x: -25.5, y: 58 }, end: { x: -25.5, y: 20 } },
          },
          {
            kind: "CubicBezier",
            role: "Cut",
            curve: {
              p0: { x: -25.5, y: 20 },
              p1: { x: -25.3, y: 14 },
              p2: { x: -19.3, y: 8 },
              p3: { x: -19, y: -1 },
            },
          },
        ],
      },
    },
  ],
};
