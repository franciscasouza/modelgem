"use client";

import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { api, formatApiError } from "@/lib/api";
import type { Customer, MeasurementSet } from "@/lib/types";
import {
  Button,
  Input,
  LoadingBlock,
  Select,
  StateMessage,
} from "@/components/ui/Primitives";
import styles from "./patterns.module.css";

type BaseId = "straight_skirt" | "simple_dress";
type Step = 1 | 2 | 3 | 4;

const SKIRT_DEFAULTS = {
  waist_circ: 70,
  hip_circ: 96,
  skirt_length: 55,
  ease_waist: 2,
  ease_hip: 4,
  waist_to_hip: 20,
  seam_allowance: 1,
  hem_allowance: 3,
};

const DRESS_DEFAULTS = {
  bust_circ: 90,
  waist_circ: 70,
  hip_circ: 96,
  dress_length: 100,
  ease_bust: 4,
  ease_waist: 2,
  ease_hip: 4,
  shoulder_to_bust: 26,
  bust_to_waist: 20,
  waist_to_hip: 20,
  seam_allowance: 1,
  hem_allowance: 3,
};

function num(v: string, fallback: number): number {
  const n = Number(v.replace(",", "."));
  return Number.isFinite(n) ? n : fallback;
}

export function PatternWizardView() {
  const router = useRouter();
  const [step, setStep] = useState<Step>(1);
  const [baseId, setBaseId] = useState<BaseId>("straight_skirt");
  const [name, setName] = useState("Saia reta");
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [customerId, setCustomerId] = useState("");
  const [sets, setSets] = useState<MeasurementSet[]>([]);
  const [measurementSetId, setMeasurementSetId] = useState("");
  const [params, setParams] = useState<Record<string, string>>({});
  const [loadingClients, setLoadingClients] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const defaults = baseId === "straight_skirt" ? SKIRT_DEFAULTS : DRESS_DEFAULTS;
    setParams(
      Object.fromEntries(Object.entries(defaults).map(([k, v]) => [k, String(v)])),
    );
    setName(baseId === "straight_skirt" ? "Saia reta" : "Vestido simples");
  }, [baseId]);

  useEffect(() => {
    if (step !== 2) return;
    setLoadingClients(true);
    void (async () => {
      try {
        const list = await api.listCustomers();
        setCustomers(Array.isArray(list) ? list : []);
      } catch {
        setCustomers([]);
      } finally {
        setLoadingClients(false);
      }
    })();
  }, [step]);

  useEffect(() => {
    if (!customerId) {
      setSets([]);
      setMeasurementSetId("");
      return;
    }
    void (async () => {
      try {
        const versions = await api.listMeasurementSets(customerId);
        const sorted = [...(Array.isArray(versions) ? versions : [])].sort(
          (a, b) => b.version - a.version,
        );
        setSets(sorted);
        setMeasurementSetId(sorted[0]?.id ?? "");
      } catch {
        setSets([]);
        setMeasurementSetId("");
      }
    })();
  }, [customerId]);

  const selectedSet = useMemo(
    () => sets.find((s) => s.id === measurementSetId) ?? null,
    [sets, measurementSetId],
  );

  useEffect(() => {
    if (!selectedSet) return;
    setParams((prev) => {
      const next = { ...prev };
      for (const [key, value] of Object.entries(selectedSet.valuesCm)) {
        if (key in next) next[key] = String(value);
      }
      return next;
    });
  }, [selectedSet]);

  function setParam(key: string, value: string) {
    setParams((prev) => ({ ...prev, [key]: value }));
  }

  async function generate() {
    setSubmitting(true);
    setError(null);
    const defaults = baseId === "straight_skirt" ? SKIRT_DEFAULTS : DRESS_DEFAULTS;
    const parameters: Record<string, number> = {};
    for (const key of Object.keys(defaults)) {
      parameters[key] = num(params[key] ?? "", defaults[key as keyof typeof defaults]);
    }

    try {
      const pattern = await api.generatePattern({
        name: name.trim() || (baseId === "straight_skirt" ? "Saia reta" : "Vestido simples"),
        baseId,
        customerId: customerId || null,
        measurementSetId: measurementSetId || null,
        parameters,
      });
      router.push(`/patterns/${pattern.id}/canvas`);
    } catch (err) {
      setError(formatApiError(err));
      setSubmitting(false);
    }
  }

  const paramFields =
    baseId === "straight_skirt"
      ? [
          ["waist_circ", "Cintura (cm)"],
          ["hip_circ", "Quadril (cm)"],
          ["skirt_length", "Comprimento (cm)"],
          ["ease_waist", "Folga cintura"],
          ["ease_hip", "Folga quadril"],
          ["waist_to_hip", "Cintura → quadril"],
          ["seam_allowance", "Margem costura"],
          ["hem_allowance", "Bainha"],
        ]
      : [
          ["bust_circ", "Busto (cm)"],
          ["waist_circ", "Cintura (cm)"],
          ["hip_circ", "Quadril (cm)"],
          ["dress_length", "Comprimento (cm)"],
          ["ease_bust", "Folga busto"],
          ["ease_waist", "Folga cintura"],
          ["ease_hip", "Folga quadril"],
          ["shoulder_to_bust", "Ombro → busto"],
          ["bust_to_waist", "Busto → cintura"],
          ["waist_to_hip", "Cintura → quadril"],
          ["seam_allowance", "Margem costura"],
          ["hem_allowance", "Bainha"],
        ];

  return (
    <div className={styles.page}>
      <header>
        <p className={styles.eyebrow}>Novo modelo</p>
        <h1>Base paramétrica</h1>
        <p className={styles.lead}>
          Escolha a base, confirme medidas em cm e gere o molde. A geometria vem do núcleo
          determinístico — não da IA.
        </p>
      </header>

      <ol className={styles.steps} aria-label="Etapas">
        {[
          [1, "Tipo"],
          [2, "Cliente"],
          [3, "Parâmetros"],
          [4, "Gerar"],
        ].map(([n, label]) => (
          <li key={n} className={step === n ? styles.stepActive : step > (n as number) ? styles.stepDone : ""}>
            <span>{n}</span> {label}
          </li>
        ))}
      </ol>

      <section className={styles.panel}>
        {step === 1 ? (
          <div className={styles.stack}>
            <Select
              label="Base"
              value={baseId}
              onChange={(e) => setBaseId(e.target.value as BaseId)}
              options={[
                { value: "straight_skirt", label: "Saia reta (straight_skirt.v1)" },
                { value: "simple_dress", label: "Vestido simples (simple_dress.v1)" },
              ]}
            />
            <Input
              label="Nome do modelo"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
            <div className={styles.actions}>
              <Button onClick={() => setStep(2)}>Continuar</Button>
            </div>
          </div>
        ) : null}

        {step === 2 ? (
          <div className={styles.stack}>
            {loadingClients ? <LoadingBlock label="Carregando clientes…" /> : null}
            <Select
              label="Cliente (opcional)"
              value={customerId}
              onChange={(e) => setCustomerId(e.target.value)}
              options={[
                { value: "", label: "Sem cliente" },
                ...customers.map((c) => ({ value: c.id, label: c.name })),
              ]}
            />
            <Select
              label="Conjunto de medidas"
              value={measurementSetId}
              onChange={(e) => setMeasurementSetId(e.target.value)}
              options={[
                { value: "", label: sets.length ? "Não usar" : "Nenhuma versão" },
                ...sets.map((s) => ({
                  value: s.id,
                  label: `v${s.version} · ${new Date(s.createdAt).toLocaleDateString("pt-BR")}`,
                })),
              ]}
            />
            <StateMessage tone="info">
              Selecionar medidas pré-preenche circunferências e comprimentos no próximo passo.
              Você pode ajustar antes de gerar.
            </StateMessage>
            <div className={styles.actions}>
              <Button variant="secondary" onClick={() => setStep(1)}>
                Voltar
              </Button>
              <Button onClick={() => setStep(3)}>Continuar</Button>
            </div>
          </div>
        ) : null}

        {step === 3 ? (
          <div className={styles.stack}>
            <div className={styles.formGrid}>
              {paramFields.map(([key, label]) => (
                <Input
                  key={key}
                  label={label}
                  value={params[key] ?? ""}
                  onChange={(e) => setParam(key, e.target.value)}
                />
              ))}
            </div>
            <div className={styles.actions}>
              <Button variant="secondary" onClick={() => setStep(2)}>
                Voltar
              </Button>
              <Button onClick={() => setStep(4)}>Revisar</Button>
            </div>
          </div>
        ) : null}

        {step === 4 ? (
          <div className={styles.stack}>
            <dl className={styles.summary}>
              <div>
                <dt>Base</dt>
                <dd>{baseId === "straight_skirt" ? "Saia reta" : "Vestido simples"}</dd>
              </div>
              <div>
                <dt>Nome</dt>
                <dd>{name}</dd>
              </div>
              <div>
                <dt>Cliente</dt>
                <dd>
                  {customerId
                    ? customers.find((c) => c.id === customerId)?.name ?? customerId
                    : "—"}
                </dd>
              </div>
            </dl>
            <StateMessage tone="warning">
              Limitações da base serão exibidas no canvas. Confirme o resultado antes de exportar.
            </StateMessage>
            {error ? <StateMessage tone="danger">{error}</StateMessage> : null}
            <div className={styles.actions}>
              <Button variant="secondary" onClick={() => setStep(3)} disabled={submitting}>
                Voltar
              </Button>
              <Button onClick={() => void generate()} disabled={submitting}>
                {submitting ? "Gerando…" : "Gerar molde"}
              </Button>
            </div>
          </div>
        ) : null}
      </section>
    </div>
  );
}
