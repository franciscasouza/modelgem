"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { api, formatApiError, isNotFound } from "@/lib/api";
import { MEASUREMENT_LABELS, formatCm, formatDate } from "@/lib/geometry";
import type { Customer, MeasurementSet } from "@/lib/types";
import {
  Button,
  EmptyState,
  Input,
  LoadingBlock,
  StateMessage,
} from "@/components/ui/Primitives";
import styles from "./clients.module.css";

const RANGES = {
  waistCircCm: { min: 50, max: 150 },
  hipCircCm: { min: 70, max: 180 },
  bustCircCm: { min: 70, max: 160 },
  skirtLengthCm: { min: 30, max: 120 },
  dressLengthCm: { min: 70, max: 160 },
  waistToHipCm: { min: 14, max: 30 },
  easeWaistCm: { min: 0, max: 8 },
  easeHipCm: { min: 0, max: 12 },
  easeBustCm: { min: 0, max: 12 },
} as const;

type FormState = {
  waistCircCm: string;
  hipCircCm: string;
  bustCircCm: string;
  skirtLengthCm: string;
  dressLengthCm: string;
  waistToHipCm: string;
  easeWaistCm: string;
  easeHipCm: string;
  easeBustCm: string;
};

const DEFAULT_FORM: FormState = {
  waistCircCm: "70",
  hipCircCm: "96",
  bustCircCm: "90",
  skirtLengthCm: "55",
  dressLengthCm: "100",
  waistToHipCm: "20",
  easeWaistCm: "2",
  easeHipCm: "4",
  easeBustCm: "4",
};

function parseOptional(value: string): number | null {
  const t = value.trim();
  if (!t) return null;
  const n = Number(t.replace(",", "."));
  return Number.isFinite(n) ? n : null;
}

export function ClientDetailView({ customerId }: { customerId: string }) {
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [sets, setSets] = useState<MeasurementSet[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<FormState>(DEFAULT_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const c = await api.getCustomer(customerId);
      setCustomer(c);
      if (!c) {
        setError("Cliente não encontrada.");
        setSets([]);
        return;
      }
      try {
        const versions = await api.listMeasurementSets(customerId);
        setSets(Array.isArray(versions) ? versions : []);
      } catch (err) {
        setSets([]);
        if (!isNotFound(err)) setError(formatApiError(err));
      }
    } catch (err) {
      setCustomer(null);
      setError(formatApiError(err));
    } finally {
      setLoading(false);
    }
  }, [customerId]);

  useEffect(() => {
    void load();
  }, [load]);

  function updateField(key: keyof FormState, value: string) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setFormError(null);

    const payload = {
      waistCircCm: parseOptional(form.waistCircCm),
      hipCircCm: parseOptional(form.hipCircCm),
      bustCircCm: parseOptional(form.bustCircCm),
      skirtLengthCm: parseOptional(form.skirtLengthCm),
      dressLengthCm: parseOptional(form.dressLengthCm),
      waistToHipCm: parseOptional(form.waistToHipCm),
      easeWaistCm: parseOptional(form.easeWaistCm),
      easeHipCm: parseOptional(form.easeHipCm),
      easeBustCm: parseOptional(form.easeBustCm),
    };

    for (const [key, range] of Object.entries(RANGES) as [
      keyof typeof RANGES,
      { min: number; max: number },
    ][]) {
      const v = payload[key];
      if (v == null) continue;
      if (v < range.min || v > range.max) {
        setFormError(`${key}: valor fora de [${range.min}, ${range.max}] cm.`);
        return;
      }
    }

    if (payload.waistCircCm == null && payload.hipCircCm == null && payload.bustCircCm == null) {
      setFormError("Informe ao menos cintura, quadril ou busto.");
      return;
    }

    setSaving(true);
    try {
      await api.createMeasurementSet(customerId, payload);
      await load();
    } catch (err) {
      setFormError(formatApiError(err));
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <LoadingBlock label="Carregando cliente…" />;

  if (!customer) {
    return (
      <div className={styles.page}>
        <StateMessage tone="danger">{error ?? "Cliente não encontrada."}</StateMessage>
        <Link href="/clients">← Voltar</Link>
      </div>
    );
  }

  const sorted = [...sets].sort((a, b) => b.version - a.version);

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div>
          <Link href="/clients" className={styles.back}>
            ← Clientes
          </Link>
          <h1>{customer.name}</h1>
          <p className={styles.lead}>{customer.notes || "Sem notas"}</p>
          <p className={styles.meta}>ID: {customer.id}</p>
        </div>
      </header>

      {error ? <StateMessage tone="warning">{error}</StateMessage> : null}

      <div className={styles.layout}>
        <section className={styles.panel}>
          <h2>Versões de medidas</h2>
          {sorted.length === 0 ? (
            <EmptyState
              title="Nenhuma versão"
              description="Crie o primeiro conjunto de medidas (cm)."
            />
          ) : (
            <ul className={styles.versions}>
              {sorted.map((s) => (
                <li key={s.id} className={styles.versionCard}>
                  <div className={styles.versionHead}>
                    <strong>v{s.version}</strong>
                    <time dateTime={s.createdAt}>{formatDate(s.createdAt)}</time>
                  </div>
                  <dl className={styles.measureGrid}>
                    {Object.entries(s.valuesCm).map(([key, value]) => (
                      <div key={key}>
                        <dt>{MEASUREMENT_LABELS[key] ?? key}</dt>
                        <dd>{formatCm(value)}</dd>
                      </div>
                    ))}
                  </dl>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className={styles.panel}>
          <h2>Novo conjunto (cm)</h2>
          <form className={styles.form} onSubmit={(e) => void onSubmit(e)}>
            <div className={styles.formGrid}>
              <Input
                label="Cintura"
                value={form.waistCircCm}
                onChange={(e) => updateField("waistCircCm", e.target.value)}
                hint="50–150 cm"
              />
              <Input
                label="Quadril"
                value={form.hipCircCm}
                onChange={(e) => updateField("hipCircCm", e.target.value)}
                hint="70–180 cm"
              />
              <Input
                label="Busto"
                value={form.bustCircCm}
                onChange={(e) => updateField("bustCircCm", e.target.value)}
                hint="70–160 cm"
              />
              <Input
                label="Comprimento saia"
                value={form.skirtLengthCm}
                onChange={(e) => updateField("skirtLengthCm", e.target.value)}
              />
              <Input
                label="Comprimento vestido"
                value={form.dressLengthCm}
                onChange={(e) => updateField("dressLengthCm", e.target.value)}
              />
              <Input
                label="Cintura → quadril"
                value={form.waistToHipCm}
                onChange={(e) => updateField("waistToHipCm", e.target.value)}
              />
              <Input
                label="Folga cintura"
                value={form.easeWaistCm}
                onChange={(e) => updateField("easeWaistCm", e.target.value)}
              />
              <Input
                label="Folga quadril"
                value={form.easeHipCm}
                onChange={(e) => updateField("easeHipCm", e.target.value)}
              />
              <Input
                label="Folga busto"
                value={form.easeBustCm}
                onChange={(e) => updateField("easeBustCm", e.target.value)}
              />
            </div>
            {formError ? <StateMessage tone="danger">{formError}</StateMessage> : null}
            <Button type="submit" disabled={saving}>
              {saving ? "Salvando…" : "Criar versão"}
            </Button>
          </form>
        </section>
      </div>
    </div>
  );
}
