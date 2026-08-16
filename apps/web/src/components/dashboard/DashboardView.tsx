"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { api, formatApiError, isNotFound } from "@/lib/api";
import { baseLabel, formatDate } from "@/lib/geometry";
import type { OverviewCounts, PatternSummary } from "@/lib/types";
import { Button, EmptyState, LoadingBlock, StateMessage } from "@/components/ui/Primitives";
import styles from "./dashboard.module.css";

export function DashboardView() {
  const router = useRouter();
  const [patterns, setPatterns] = useState<PatternSummary[]>([]);
  const [overview, setOverview] = useState<OverviewCounts | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [blankBusy, setBlankBusy] = useState(false);
  const [blankError, setBlankError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    let list: PatternSummary[] = [];
    const warnings: string[] = [];

    try {
      const res = await api.listPatterns();
      list = Array.isArray(res) ? res : [];
      setPatterns(list);
    } catch (err) {
      setPatterns([]);
      warnings.push(
        isNotFound(err)
          ? "API de modelos ainda não disponível (404)."
          : formatApiError(err),
      );
    }

    let customersCount: number | null = null;
    try {
      const customers = await api.listCustomers();
      customersCount = Array.isArray(customers) ? customers.length : 0;
    } catch (err) {
      if (!isNotFound(err) && warnings.length === 0) {
        warnings.push(formatApiError(err));
      }
    }

    try {
      const ov = await api.getOverview();
      setOverview(ov);
    } catch {
      if (customersCount != null || list.length > 0) {
        setOverview({
          patternsCount: list.length,
          customersCount: customersCount ?? 0,
          pendingApprovalsCount: null,
        });
      } else {
        setOverview(null);
      }
    }

    setError(warnings[0] ?? null);
    setLoading(false);
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function createBlank() {
    setBlankBusy(true);
    setBlankError(null);
    try {
      const created = await api.createBlankPattern({ name: "Tela em branco" });
      router.push(`/patterns/${created.id}/canvas`);
    } catch (err) {
      setBlankError(formatApiError(err));
      setBlankBusy(false);
    }
  }

  const recent = [...patterns]
    .sort(
      (a, b) =>
        new Date(b.updatedAt ?? b.createdAt).getTime() -
        new Date(a.updatedAt ?? a.createdAt).getTime(),
    )
    .slice(0, 8);

  return (
    <div className={styles.page}>
      <header className={styles.welcome}>
        <div>
          <p className={styles.eyebrow}>Biblioteca</p>
          <h1>Bem-vinda ao ModelaFlow</h1>
          <p className={styles.lead}>
            Crie moldes a partir de bases paramétricas. A interpretação por IA chega na Fase 2 —
            a modelista permanece a autoridade final.
          </p>
        </div>
      </header>

      {error ? <StateMessage tone="warning">{error}</StateMessage> : null}

      <section className={styles.section} aria-labelledby="start-title">
        <h2 id="start-title">Começar novo modelo</h2>
        <div className={styles.actions}>
          <Link href="/patterns/new" className={styles.actionCard}>
            <span className={`${styles.actionIcon} ${styles.iconParam}`} aria-hidden />
            <strong>Base paramétrica</strong>
            <span>Saia reta ou vestido simples com medidas em cm.</span>
          </Link>

          <div className={`${styles.actionCard} ${styles.actionDisabled}`} title="Em breve (Fase 2)">
            <span className={`${styles.actionIcon} ${styles.iconAi}`} aria-hidden />
            <strong>Upload com IA</strong>
            <span>Em breve (Fase 2) — sem geração automática sem confirmação.</span>
          </div>

          <button
            type="button"
            className={styles.actionCard}
            onClick={() => void createBlank()}
            disabled={blankBusy}
          >
            <span className={`${styles.actionIcon} ${styles.iconBlank}`} aria-hidden />
            <strong>Tela em branco</strong>
            <span>{blankBusy ? "Criando…" : "Documento vazio para revisão manual."}</span>
          </button>
        </div>
        {blankError ? <StateMessage tone="danger">{blankError}</StateMessage> : null}
      </section>

      <div className={styles.grid}>
        <section className={styles.panel} aria-labelledby="recent-title">
          <div className={styles.panelHead}>
            <h2 id="recent-title">Modelos recentes</h2>
            <Button variant="ghost" size="sm" onClick={() => void load()}>
              Atualizar
            </Button>
          </div>
          {loading ? (
            <LoadingBlock />
          ) : recent.length === 0 ? (
            <EmptyState
              title="Nenhum modelo ainda"
              description="Comece por uma base paramétrica ou aguarde a API de patterns."
            />
          ) : (
            <ul className={styles.modelList}>
              {recent.map((p) => (
                <li key={p.id}>
                  <Link href={`/patterns/${p.id}/canvas`} className={styles.modelRow}>
                    <div>
                      <strong>{p.name}</strong>
                      <span className={styles.meta}>
                        {p.reference ? `${p.reference} · ` : ""}
                        {baseLabel(p.baseId)}
                        {p.customerName ? ` · ${p.customerName}` : ""}
                      </span>
                    </div>
                    <time dateTime={p.updatedAt ?? p.createdAt}>
                      {formatDate(p.updatedAt ?? p.createdAt)}
                    </time>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className={styles.panel} aria-labelledby="overview-title">
          <h2 id="overview-title">Visão geral</h2>
          {overview ? (
            <dl className={styles.stats}>
              <div>
                <dt>Modelos</dt>
                <dd>{overview.patternsCount}</dd>
              </div>
              <div>
                <dt>Clientes</dt>
                <dd>{overview.customersCount}</dd>
              </div>
              {overview.pendingApprovalsCount != null ? (
                <div>
                  <dt>Pendências</dt>
                  <dd>{overview.pendingApprovalsCount}</dd>
                </div>
              ) : null}
            </dl>
          ) : loading ? (
            <LoadingBlock />
          ) : (
            <EmptyState
              title="Contagens indisponíveis"
              description="Sem métricas inventadas — conecte a API para ver totais reais."
            />
          )}
        </section>
      </div>
    </div>
  );
}
