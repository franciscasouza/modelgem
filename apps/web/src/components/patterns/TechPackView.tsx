"use client";

import Link from "next/link";
import { useCallback, useEffect, useRef, useState } from "react";
import { api, formatApiError, isNotFound } from "@/lib/api";
import { MEASUREMENT_LABELS, formatCm, formatDate } from "@/lib/geometry";
import type { ExportJob, PatternDetail, TechnicalSheet } from "@/lib/types";
import {
  Button,
  EmptyState,
  LoadingBlock,
  StateMessage,
  TextArea,
} from "@/components/ui/Primitives";
import styles from "./patterns.module.css";

export function TechPackView({ patternId }: { patternId: string }) {
  const [pattern, setPattern] = useState<PatternDetail | null>(null);
  const [sheet, setSheet] = useState<TechnicalSheet | null>(null);
  const [materials, setMaterials] = useState("");
  const [construction, setConstruction] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveBusy, setSaveBusy] = useState(false);
  const [saveMsg, setSaveMsg] = useState<string | null>(null);
  const [includeSheet, setIncludeSheet] = useState(true);
  const [exportBusy, setExportBusy] = useState(false);
  const [job, setJob] = useState<ExportJob | null>(null);
  const [exportError, setExportError] = useState<string | null>(null);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const stopPoll = useCallback(() => {
    if (pollRef.current) {
      clearInterval(pollRef.current);
      pollRef.current = null;
    }
  }, []);

  useEffect(() => () => stopPoll(), [stopPoll]);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const detail = await api.getPattern(patternId);
      setPattern(detail);
      setMaterials(detail.materialsNotes ?? "");
      setConstruction(detail.constructionNotes ?? "");
    } catch (err) {
      setPattern(null);
      if (!isNotFound(err)) setError(formatApiError(err));
      else setError("Modelo não encontrado (404).");
    }

    try {
      const ts = await api.getTechnicalSheet(patternId);
      setSheet(ts);
      setMaterials(ts.materialsNotes ?? "");
      setConstruction(ts.constructionNotes ?? "");
    } catch {
      setSheet(null);
    } finally {
      setLoading(false);
    }
  }, [patternId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function save() {
    setSaveBusy(true);
    setSaveMsg(null);
    try {
      const updated = await api.updateTechnicalSheet(patternId, {
        materialsNotes: materials,
        constructionNotes: construction,
      });
      setSheet(updated);
      setSaveMsg("Ficha salva.");
    } catch (err) {
      setSaveMsg(formatApiError(err));
    } finally {
      setSaveBusy(false);
    }
  }

  async function startExport() {
    setExportBusy(true);
    setExportError(null);
    setJob(null);
    stopPoll();
    try {
      const created = await api.startExport(patternId);
      setJob(created);
      if (created.status === "succeeded" || created.status === "failed") {
        setExportBusy(false);
        return;
      }
      pollRef.current = setInterval(() => {
        void (async () => {
          try {
            const next = await api.getExportJob(patternId, created.id);
            setJob(next);
            if (next.status === "succeeded" || next.status === "failed") {
              stopPoll();
              setExportBusy(false);
            }
          } catch (err) {
            stopPoll();
            setExportError(formatApiError(err));
            setExportBusy(false);
          }
        })();
      }, 1500);
    } catch (err) {
      setExportError(formatApiError(err));
      setExportBusy(false);
    }
  }

  if (loading) return <LoadingBlock label="Carregando ficha…" />;

  const measures =
    sheet?.measurementsCm ??
    pattern?.document?.resolvedParametersCm ??
    null;

  return (
    <div className={styles.page}>
      <header className={styles.canvasHeader}>
        <div>
          <Link href={`/patterns/${patternId}/canvas`} className={styles.back}>
            ← Canvas 2D
          </Link>
          <h1>Ficha técnica e exportação</h1>
          <p className={styles.lead}>
            {pattern?.name ?? patternId}
            {pattern?.version != null ? ` · v${pattern.version}` : ""}
          </p>
        </div>
      </header>

      {error ? <StateMessage tone="warning">{error}</StateMessage> : null}

      <div className={styles.techGrid}>
        <section className={styles.panel}>
          <h2>Ficha mínima</h2>
          <div className={styles.stack}>
            <TextArea
              label="Materiais"
              value={materials}
              onChange={(e) => setMaterials(e.target.value)}
              hint="Tecidos, forros, aviamentos (texto livre no MVP)"
            />
            <TextArea
              label="Observações de montagem"
              value={construction}
              onChange={(e) => setConstruction(e.target.value)}
            />
            {saveMsg ? (
              <StateMessage tone={saveMsg.includes("salva") ? "success" : "danger"}>
                {saveMsg}
              </StateMessage>
            ) : null}
            <Button onClick={() => void save()} disabled={saveBusy}>
              {saveBusy ? "Salvando…" : "Salvar ficha"}
            </Button>
          </div>

          <h3 className={styles.subHead}>Medidas (cm)</h3>
          {measures && Object.keys(measures).length > 0 ? (
            <dl className={styles.measureTable}>
              {Object.entries(measures).map(([k, v]) => (
                <div key={k}>
                  <dt>{MEASUREMENT_LABELS[k] ?? k}</dt>
                  <dd>{formatCm(v)}</dd>
                </div>
              ))}
            </dl>
          ) : (
            <EmptyState
              title="Sem tabela de medidas"
              description="Vincule um MeasurementSet na geração ou aguarde a API de ficha."
            />
          )}
        </section>

        <section className={styles.panel}>
          <h2>Exportação PDF A4</h2>
          <p className={styles.hint}>
            Formato A4 com escala. A exportação é assíncrona (job na API). Escala real é
            responsabilidade do serviço de export — não do browser.
          </p>
          <label className={styles.check}>
            <input
              type="checkbox"
              checked={includeSheet}
              onChange={(e) => setIncludeSheet(e.target.checked)}
            />
            Incluir ficha técnica no PDF
          </label>
          <div className={styles.actions}>
            <Button onClick={() => void startExport()} disabled={exportBusy}>
              {exportBusy ? "Exportando…" : "Exportar PDF"}
            </Button>
          </div>
          {exportError ? <StateMessage tone="danger">{exportError}</StateMessage> : null}
          {job ? (
            <div className={styles.jobBox}>
              <p>
                <strong>Status:</strong> {job.status}
              </p>
              {job.createdAt ? (
                <p className={styles.hint}>Criado: {formatDate(job.createdAt)}</p>
              ) : null}
              {job.error ? <StateMessage tone="danger">{job.error}</StateMessage> : null}
              {job.status === "succeeded" ? (
                <a
                  className={styles.download}
                  href={
                    job.downloadUrl ||
                    api.exportDownloadUrl(patternId, job.id)
                  }
                  target="_blank"
                  rel="noreferrer"
                >
                  Baixar PDF
                </a>
              ) : null}
            </div>
          ) : null}
        </section>
      </div>
    </div>
  );
}
