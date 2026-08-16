"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { api, formatApiError, isNotFound } from "@/lib/api";
import { formatDate } from "@/lib/geometry";
import type { Customer } from "@/lib/types";
import { Button, EmptyState, Input, LoadingBlock, StateMessage, TextArea } from "@/components/ui/Primitives";
import styles from "./clients.module.css";

export function ClientsListView() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [notes, setNotes] = useState("");
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const list = await api.listCustomers();
      setCustomers(Array.isArray(list) ? list : []);
    } catch (err) {
      setCustomers([]);
      setError(
        isNotFound(err)
          ? "Endpoint de clientes não encontrado (404)."
          : formatApiError(err),
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return customers;
    return customers.filter(
      (c) =>
        c.name.toLowerCase().includes(q) ||
        c.id.toLowerCase().includes(q) ||
        (c.notes ?? "").toLowerCase().includes(q),
    );
  }, [customers, query]);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    setCreateError(null);
    if (!name.trim()) {
      setCreateError("Informe o nome da cliente.");
      return;
    }
    setCreating(true);
    try {
      await api.createCustomer({ name: name.trim(), notes: notes.trim() || undefined });
      setName("");
      setNotes("");
      await load();
    } catch (err) {
      setCreateError(formatApiError(err));
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Clientes</p>
          <h1>Gestão de clientes</h1>
          <p className={styles.lead}>Medidas em cm, versionadas por cliente.</p>
        </div>
      </header>

      {error ? <StateMessage tone="warning">{error}</StateMessage> : null}

      <div className={styles.layout}>
        <section className={styles.panel}>
          <div className={styles.toolbar}>
            <Input
              label="Buscar"
              placeholder="Nome ou ID"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
            <Button variant="secondary" size="sm" onClick={() => void load()}>
              Atualizar
            </Button>
          </div>

          {loading ? (
            <LoadingBlock />
          ) : filtered.length === 0 ? (
            <EmptyState
              title="Nenhuma cliente encontrada"
              description="Crie a primeira cliente ao lado ou ajuste a busca."
            />
          ) : (
            <ul className={styles.list}>
              {filtered.map((c) => (
                <li key={c.id}>
                  <Link href={`/clients/${c.id}`} className={styles.row}>
                    <div>
                      <strong>{c.name}</strong>
                      <span className={styles.meta}>{c.notes || "Sem notas"}</span>
                    </div>
                    <time dateTime={c.createdAt}>{formatDate(c.createdAt)}</time>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className={styles.panel}>
          <h2>Nova cliente</h2>
          <form className={styles.form} onSubmit={(e) => void onCreate(e)}>
            <Input
              label="Nome"
              name="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <TextArea
              label="Notas"
              name="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              hint="Opcional"
            />
            {createError ? <StateMessage tone="danger">{createError}</StateMessage> : null}
            <Button type="submit" disabled={creating}>
              {creating ? "Salvando…" : "Criar cliente"}
            </Button>
          </form>
        </section>
      </div>
    </div>
  );
}
