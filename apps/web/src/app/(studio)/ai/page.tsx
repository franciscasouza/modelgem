import Link from "next/link";
import { StateMessage } from "@/components/ui/Primitives";

export default function AiStubPage() {
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
        Editor IA
      </p>
      <h1 style={{ fontSize: "1.75rem" }}>Fase 2 — em breve</h1>
      <StateMessage tone="info">
        A interpretação assistida por IA (upload de referência, confiança e confirmação
        explícita) entra na Fase 2. Nada vira molde sem ação da modelista (ADR-0001).
      </StateMessage>
      <p style={{ color: "var(--mf-text-secondary)" }}>
        Enquanto isso, use a{" "}
        <Link href="/patterns/new" style={{ color: "var(--mf-primary)", fontWeight: 600 }}>
          base paramétrica
        </Link>{" "}
        ou o canvas 2D.
      </p>
    </div>
  );
}
