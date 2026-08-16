import { StateMessage } from "@/components/ui/Primitives";

export default function SettingsPage() {
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
      <h1 style={{ fontSize: "1.75rem" }}>Configuração do ateliê</h1>
      <StateMessage tone="info">
        Placeholder da Fase 1. Organização, usuárias e preferências entram com AuthN /
        multi-tenant completo.
      </StateMessage>
      <p style={{ fontSize: "0.875rem", color: "var(--mf-text-secondary)" }}>
        Tenant atual: <code>localStorage.mf_tenant_id</code> ou{" "}
        <code>NEXT_PUBLIC_TENANT_ID</code> / bootstrap{" "}
        <code>POST /api/v1/dev/bootstrap</code>.
      </p>
    </div>
  );
}
