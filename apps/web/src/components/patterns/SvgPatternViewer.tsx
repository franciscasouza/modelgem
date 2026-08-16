"use client";

import { useCallback, useMemo, useRef, useState } from "react";
import type { PatternDocument, PatternPiece } from "@/lib/types";
import {
  boundsOf,
  collectPoints,
  contourToPathD,
  sideLabel,
  viewBoxString,
} from "@/lib/geometry";
import styles from "./patterns.module.css";

interface SvgPatternViewerProps {
  document: PatternDocument;
  selectedPieceId: string | null;
  onSelectPiece: (id: string) => void;
}

export function SvgPatternViewer({
  document,
  selectedPieceId,
  onSelectPiece,
}: SvgPatternViewerProps) {
  const bounds = useMemo(
    () => boundsOf(collectPoints(document), 3),
    [document],
  );
  const baseViewBox = viewBoxString(bounds);

  const [scale, setScale] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const dragging = useRef(false);
  const last = useRef({ x: 0, y: 0 });

  const onWheel = useCallback((e: React.WheelEvent) => {
    e.preventDefault();
    setScale((s) => Math.min(4, Math.max(0.4, s * (e.deltaY > 0 ? 0.92 : 1.08))));
  }, []);

  const pieces = useMemo(() => document.pieces ?? [], [document.pieces]);
  const selected = pieces.find((p) => p.id === selectedPieceId) ?? null;
  const renderPieces = selected ? [selected] : pieces;

  // Offset pieces horizontally when showing all
  const offsets = useMemo(() => {
    if (selected) return { [selected.id]: 0 };
    const map: Record<string, number> = {};
    let cursor = 0;
    const gap = 8;
    for (const piece of pieces) {
      const pts = collectPoints({ ...document, pieces: [piece] });
      const b = boundsOf(pts, 0);
      const width = b.maxX - b.minX;
      map[piece.id] = cursor - b.minX;
      cursor += width + gap;
    }
    return map;
  }, [document, pieces, selected]);

  return (
    <div className={styles.viewport}>
      <div className={styles.viewportTools}>
        <button type="button" onClick={() => setScale((s) => Math.min(4, s * 1.15))}>
          Zoom +
        </button>
        <button type="button" onClick={() => setScale((s) => Math.max(0.4, s * 0.87))}>
          Zoom −
        </button>
        <button
          type="button"
          onClick={() => {
            setScale(1);
            setPan({ x: 0, y: 0 });
          }}
        >
          Reset
        </button>
        <span className={styles.zoomLabel}>{Math.round(scale * 100)}%</span>
      </div>

      <div
        className={styles.svgWrap}
        onWheel={onWheel}
        onPointerDown={(e) => {
          dragging.current = true;
          last.current = { x: e.clientX, y: e.clientY };
          (e.target as HTMLElement).setPointerCapture?.(e.pointerId);
        }}
        onPointerMove={(e) => {
          if (!dragging.current) return;
          const dx = e.clientX - last.current.x;
          const dy = e.clientY - last.current.y;
          last.current = { x: e.clientX, y: e.clientY };
          setPan((p) => ({ x: p.x + dx, y: p.y + dy }));
        }}
        onPointerUp={() => {
          dragging.current = false;
        }}
      >
        <svg
          className={styles.svg}
          viewBox={baseViewBox}
          style={{
            transform: `translate(${pan.x}px, ${pan.y}px) scale(${scale})`,
            transformOrigin: "center center",
          }}
        >
          <defs>
            <pattern id="grid" width="5" height="5" patternUnits="userSpaceOnUse">
              <path d="M 5 0 L 0 0 0 5" fill="none" stroke="#dce3ec" strokeWidth="0.15" />
            </pattern>
          </defs>
          <rect
            x={bounds.minX - 20}
            y={bounds.minY - 20}
            width={(bounds.maxX - bounds.minX) * pieces.length + 80}
            height={bounds.maxY - bounds.minY + 40}
            fill="url(#grid)"
          />

          {renderPieces.map((piece) => (
            <PieceGroup
              key={piece.id}
              piece={piece}
              offsetX={offsets[piece.id] ?? 0}
              active={!selected || piece.id === selectedPieceId}
              onSelect={() => onSelectPiece(piece.id)}
            />
          ))}
        </svg>
      </div>
    </div>
  );
}

function PieceGroup({
  piece,
  offsetX,
  active,
  onSelect,
}: {
  piece: PatternPiece;
  offsetX: number;
  active: boolean;
  onSelect: () => void;
}) {
  const cut = contourToPathD(piece.cutContour);
  const stitch = contourToPathD(piece.stitchContour);
  const g = piece.grainline;
  const opacity = active ? 1 : 0.35;

  return (
    <g
      transform={`translate(${offsetX} 0)`}
      opacity={opacity}
      onClick={onSelect}
      style={{ cursor: "pointer" }}
    >
      {cut ? (
        <path d={cut} fill="rgba(11, 61, 145, 0.06)" stroke="#0b3d91" strokeWidth={0.35} />
      ) : null}
      {stitch ? (
        <path d={stitch} fill="none" stroke="#1a2332" strokeWidth={0.25} strokeDasharray="1 0.6" />
      ) : null}
      {g ? (
        <g>
          <line
            x1={g.start.x}
            y1={g.start.y}
            x2={g.end.x}
            y2={g.end.y}
            stroke="#0d8a7c"
            strokeWidth={0.3}
            markerEnd="url(#arrow)"
          />
          <text
            x={(g.start.x + g.end.x) / 2 + 1}
            y={(g.start.y + g.end.y) / 2}
            fontSize={2}
            fill="#0d8a7c"
          >
            fio
          </text>
        </g>
      ) : null}
      {(piece.notches ?? []).map((n) => (
        <g key={n.id}>
          <line
            x1={n.position.x - 0.8}
            y1={n.position.y}
            x2={n.position.x + 0.8}
            y2={n.position.y}
            stroke="#b42318"
            strokeWidth={0.35}
          />
          <line
            x1={n.position.x}
            y1={n.position.y - 0.8}
            x2={n.position.x}
            y2={n.position.y + 0.8}
            stroke="#b42318"
            strokeWidth={0.35}
          />
        </g>
      ))}
      <text x={0} y={-2} textAnchor="middle" fontSize={2.4} fill="#1a2332" fontWeight={600}>
        {piece.name} ({sideLabel(piece.side)})
      </text>
    </g>
  );
}
