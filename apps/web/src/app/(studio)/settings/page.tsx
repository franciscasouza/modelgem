"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button, StateMessage } from "@/components/ui/Primitives";
import { formatApiError } from "@/lib/api";

export default function SettingsPage() {
  const { user, logout } = useAuth();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onLogout() {
    setError(null);
    setPending(true);
    try {
      await logout();
      router.replace("/login");
    } catch (err) {
      setError(formatApiError(err));
      setPending(false);
    }
  }

  const displayName = user?.displayName?.trim() || "—";

  return (
    <div style={{ maxWidth: 640, display: "flex", flexDirection: "column", gap: "1rem" }}>
      <p
        style={{
          fontSize: "0.75rem",
          fontWeight: 700,
          letterSpacing: "0.06em",
          textTransform: "uppercase",
          color: "var(--mf-primary)",
        }}
      >
        Configurações
      </p>
      <h1 style={{ fontSize: "1.75rem" }}>Conta e organização</h1>

      <StateMessage tone="info">
        A sessão usa cookie HttpOnly. O tenant das APIs vem de{" "}
        <code>GET /api/v1/auth/me</code>, não de localStorage.
      </StateMessage>

      {error ? <StateMessage tone="danger">{error}</StateMessage> : null}

      {user ? (
        <dl
          style={{
            display: "grid",
            gridTemplateColumns: "8rem 1fr",
            gap: "0.65rem 1rem",
            fontSize: "0.9rem",
            background: "var(--mf-surface)",
            border: "1px solid var(--mf-border)",
            borderRadius: "var(--mf-radius)",
            padding: "1.1rem 1.25rem",
          }}
        >
          <dt style={{ color: "var(--mf-text-secondary)", fontWeight: 600 }}>Organização</dt>
          <dd>{user.organizationName}</dd>
          <dt style={{ color: "var(--mf-text-secondary)", fontWeight: 600 }}>Usuária</dt>
          <dd>{displayName}</dd>
          <dt style={{ color: "var(--mf-text-secondary)", fontWeight: 600 }}>E-mail</dt>
          <dd>{user.email}</dd>
          <dt style={{ color: "var(--mf-text-secondary)", fontWeight: 600 }}>Papel</dt>
          <dd>{user.role}</dd>
          <dt style={{ color: "var(--mf-text-secondary)", fontWeight: 600 }}>Tenant</dt>
          <dd>
            <code style={{ fontSize: "0.8rem" }}>{user.tenantId}</code>
          </dd>
        </dl>
      ) : null}

      <div>
        <Button type="button" variant="danger" disabled={pending} onClick={() => void onLogout()}>
          {pending ? "Saindo…" : "Sair"}
        </Button>
      </div>
    </div>
  );
}
