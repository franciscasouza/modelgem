"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { RequireGuest } from "@/components/auth/AuthGuards";
import { useAuth } from "@/components/auth/AuthProvider";
import styles from "@/components/auth/auth.module.css";
import { Button, Input, StateMessage } from "@/components/ui/Primitives";
import { formatApiError } from "@/lib/api";

export default function LoginPage() {
  return (
    <RequireGuest>
      <LoginForm />
    </RequireGuest>
  );
}

function LoginForm() {
  const { login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setPending(true);
    try {
      await login({ email: email.trim(), password });
      router.replace("/");
    } catch (err) {
      setError(formatApiError(err));
    } finally {
      setPending(false);
    }
  }

  return (
    <>
      <p className={styles.eyebrow}>Acesso</p>
      <h2 className={styles.panelTitle}>Entrar no studio</h2>
      <p className={styles.panelLead}>
        Use o e-mail e a senha da sua organização. A sessão fica em cookie
        HttpOnly.
      </p>

      {error ? <StateMessage tone="danger">{error}</StateMessage> : null}

      <form className={styles.form} onSubmit={onSubmit} noValidate>
        <Input
          label="E-mail"
          name="email"
          type="email"
          autoComplete="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={pending}
        />
        <Input
          label="Senha"
          name="password"
          type="password"
          autoComplete="current-password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={pending}
        />
        <div className={styles.actions}>
          <Button type="submit" disabled={pending || !email || !password}>
            {pending ? "Entrando…" : "Entrar"}
          </Button>
        </div>
      </form>

      <p className={styles.footerLink}>
        Ainda não tem conta? <Link href="/register">Criar organização</Link>
      </p>
    </>
  );
}
