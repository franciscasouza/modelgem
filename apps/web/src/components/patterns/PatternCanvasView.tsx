"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { api, formatApiError, isNotFound } from "@/lib/api";
import { MOCK_SKIRT_DOCUMENT } from "@/lib/fixtures/skirt-mock";
import {
  MEASUREMENT_LABELS,
  baseLabel,
  formatCm,
  qualityLabel,
  sideLabel,
} from "@/lib/geometry";
import type { PatternDetail, PatternDocument } from "@/lib/types";
import {
  Button,
  EmptyState,
  Input,
  LoadingBlock,
  StateMessage,
} from "@/components/ui/Primitives";
import { SvgPatternViewer } from "./SvgPatternViewer";
import styles from "./patterns.module.css";

export function PatternCanvasView({ patternId }: { patternId: string }) {
  const [pattern, setPattern] = useState<PatternDetail | null>(null);
  const [document, setDocument] = useState<PatternDocument | null>(null);
  const [usingMock, setUsingMock] = useState(false);
  const [selectedPieceId, setSelectedPieceId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [paramDraft, setParamDraft] = useState<Record<string, string>>({});
  const [regenBusy, setRegenBusy] = useState(false);
  const [regenError, setRegenError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    setUsingMock(false);
    try {
      const detail = await api.getPattern(patternId);
      setPattern(detail);
      if (detail.document?.pieces?.length) {
        setDocument(detail.document);
        setSelectedPieceId(detail.document.pieces[0]?.id ?? null);
        setParamDraft(
          Object.fromEntries(
            Object.entries(detail.document.resolvedParametersCm ?? {}).map(([k, v]) => [
              k,
              String(v),
            ]),
          ),
        );
      } else if (detail.baseId === "blank") {
        setDocument(null);
        setSelectedPieceId(null);
        setParamDraft({});
      } else {
        // API returned pattern without geometry yet — visual fixture for non-blank bases
        setDocument(MOCK_SKIRT_DOCUMENT);
        setUsingMock(true);
        setSelectedPieceId(MOCK_SKIRT_DOCUMENT.pieces[0]?.id ?? null);
        setParamDraft(
          Object.fromEntries(
            Object.entries(MOCK_SKIRT_DOCUMENT.resolvedParametersCm).map(([k, v]) => [
              k,
              String(v),
            ]),
          ),
        );
      }
    } catch (err) {
      setPattern(null);
      if (isNotFound(err)) {
        setDocument(MOCK_SKIRT_DOCUMENT);
        setUsingMock(true);
        setSelectedPieceId(MOCK_SKIRT_DOCUMENT.pieces[0]?.id ?? null);
        setParamDraft(
          Object.fromEntries(
            Object.entries(MOCK_SKIRT_DOCUMENT.resolvedParametersCm).map(([k, v]) => [
              k,
              String(v),
            ]),
          ),
        );
        setError(
          "Modelo não encontrado na API (404). Exibindo fixture local só para desenvolvimento visual.",
        );
      } else {
        setError(formatApiError(err));
        setDocument(null);
      }
    } finally {
      setLoading(false);
    }
  }, [patternId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function regenerate() {
    setRegenBusy(true);
    setRegenError(null);
    const parameters: Record<string, number> = {};
    for (const [k, v] of Object.entries(paramDraft)) {
      const n = Number(String(v).replace(",", "."));
      if (Number.isFinite(n)) parameters[k] = n;
    }
    try {
      const detail = await api.regeneratePattern(patternId, parameters);
      setPattern(detail);
      if (detail.document) {
        setDocument(detail.document);
        setUsingMock(false);
        setSelectedPieceId(detail.document.pieces[0]?.id ?? null);
      }
    } catch (err) {
      setRegenError(formatApiError(err));
    } finally {
      setRegenBusy(false);
    }
  }

  if (loading) return <LoadingBlock label="Carregando canvas…" />;

  const issues = pattern?.qualityIssues ?? [];
  const limitations = document?.limitations ?? [];
  const pieces = document?.pieces ?? [];

  return (
    <div className={styles.canvasPage}>
      <header className={styles.canvasHeader}>
        <div>
          <p className={styles.eyebrow}>Editor 2D</p>
          <h1>{pattern?.name ?? "Molde (preview)"}</h1>
          <p className={styles.lead}>
            {pattern
              ? `${baseLabel(pattern.baseId)}${pattern.version != null ? ` · v${pattern.version}` : ""}`
              : "Fixture local"}
            {pattern?.customerName ? ` · ${pattern.customerName}` : ""}
          </p>
        </div>
        <div className={styles.headerActions}>
          <Link href={`/patterns/${patternId}/tech-pack`}>
            <Button variant="secondary">Ficha técnica</Button>
          </Link>
          <Button variant="ghost" onClick={() => void load()}>
            Atualizar
          </Button>
        </div>
      </header>

      {error ? <StateMessage tone="warning">{error}</StateMessage> : null}
      {usingMock ? (
        <StateMessage tone="info">
          Geometria de fixture local — preferir o PatternDocument serializado pela API quando
          disponível.
        </StateMessage>
      ) : null}
      {issues.length > 0 ? (
        <StateMessage tone="warning">
          <strong>Avisos de qualidade:</strong>{" "}
          {issues.map((i) => qualityLabel(i)).join(" · ")}
        </StateMessage>
      ) : null}
      {limitations.length > 0 ? (
        <StateMessage tone="info">
          <strong>Limitações da base:</strong> {limitations.join(" ")}
        </StateMessage>
      ) : null}

      {!document ? (
        <EmptyState
          title="Documento sem geometria"
          description="Tela em branco ou API ainda não retornou peças. Use o wizard paramétrico para gerar."
        />
      ) : (
        <div className={styles.canvasLayout}>
          <aside className={styles.sidePanel}>
            <h2>Peças</h2>
            <ul className={styles.pieceList}>
              {pieces.map((p) => (
                <li key={p.id}>
                  <button
                    type="button"
                    className={
                      selectedPieceId === p.id ? styles.pieceActive : styles.pieceBtn
                    }
                    onClick={() => setSelectedPieceId(p.id)}
                  >
                    <strong>{p.name}</strong>
                    <span>
                      {sideLabel(p.side)} · cortar {p.quantityToCut}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSelectedPieceId(null)}
            >
              Ver todas
            </Button>
          </aside>

          <SvgPatternViewer
            document={document}
            selectedPieceId={selectedPieceId}
            onSelectPiece={setSelectedPieceId}
          />

          <aside className={styles.sidePanel}>
            <h2>Recalcular base</h2>
            <p className={styles.hint}>
              Ajustes via parâmetros (cm). Não edita Bézier livre no MVP.
            </p>
            <div className={styles.paramList}>
              {Object.entries(paramDraft).map(([key, value]) => (
                <Input
                  key={key}
                  label={MEASUREMENT_LABELS[key] ?? key}
                  value={value}
                  onChange={(e) =>
                    setParamDraft((prev) => ({ ...prev, [key]: e.target.value }))
                  }
                />
              ))}
            </div>
            {regenError ? <StateMessage tone="danger">{regenError}</StateMessage> : null}
            <Button onClick={() => void regenerate()} disabled={regenBusy || usingMock && !pattern}>
              {regenBusy ? "Recalculando…" : "Recalcular"}
            </Button>
            {selectedPieceId ? (
              <div className={styles.pieceMeta}>
                {(() => {
                  const p = pieces.find((x) => x.id === selectedPieceId);
                  if (!p) return null;
                  return (
                    <>
                      <h3>{p.name}</h3>
                      <p>
                        Margem costura {formatCm(p.margins.seamAllowanceCm)} · Bainha{" "}
                        {formatCm(p.margins.hemAllowanceCm)}
                      </p>
                      <p>Piques: {p.notches?.length ?? 0}</p>
                    </>
                  );
                })()}
              </div>
            ) : null}
          </aside>
        </div>
      )}
    </div>
  );
}
