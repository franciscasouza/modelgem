"use client";

import { useEffect, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import { LoadingBlock, StateMessage } from "@/components/ui/Primitives";
import { useAuth } from "./AuthProvider";

export function RequireAuth({ children }: { children: ReactNode }) {
  const { status, sessionError } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (status === "anonymous") {
      router.replace("/login");
    }
  }, [status, router]);

  if (status === "loading") {
    return <LoadingBlock label="Verificando sessão…" />;
  }

  if (status === "anonymous") {
    return (
      <div style={{ padding: "2rem", maxWidth: 480 }}>
        <LoadingBlock label="Redirecionando para login…" />
        {sessionError ? (
          <div style={{ marginTop: "1rem" }}>
            <StateMessage tone="warning">{sessionError}</StateMessage>
          </div>
        ) : null}
      </div>
    );
  }

  return <>{children}</>;
}

export function RequireGuest({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (status === "authenticated") {
      router.replace("/");
    }
  }, [status, router]);

  if (status === "loading") {
    return <LoadingBlock label="Verificando sessão…" />;
  }

  if (status === "authenticated") {
    return <LoadingBlock label="Redirecionando…" />;
  }

  return <>{children}</>;
}
