"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { RequireGuest } from "@/components/auth/AuthGuards";
import { useAuth } from "@/components/auth/AuthProvider";
import styles from "@/components/auth/auth.module.css";
import { Button, Input, StateMessage } from "@/components/ui/Primitives";
import { formatApiError } from "@/lib/api";

export default function RegisterPage() {
  return (
    <RequireGuest>
      <RegisterForm />
    </RequireGuest>
  );
}

function RegisterForm() {
  const { register } = useAuth();
  const router = useRouter();
  const [organizationName, setOrganizationName] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setPending(true);
    try {
      const user = await register({
        organizationName: organizationName.trim(),
        email: email.trim(),
        displayName: displayName.trim() || undefined,
        password,
      });
      if (user) {
        router.replace("/");
        return;
      }
      setInfo("Conta criada. Faça login para entrar no studio.");
      router.replace("/login");
    } catch (err) {
      setError(formatApiError(err));
    } finally {
      setPending(false);
    }
  }

  const canSubmit =
    organizationName.trim().length > 0 &&
    email.trim().length > 0 &&
    password.length >= 6;

  return (
    <>
      <p className={styles.eyebrow}>Cadastro</p>
      <h2 className={styles.panelTitle}>Criar organização</h2>
      <p className={styles.panelLead}>
        Cadastre o ateliê e a primeira usuária. Você confirma e controla cada
        molde gerado.
      </p>

      {error ? <StateMessage tone="danger">{error}</StateMessage> : null}
      {info ? <StateMessage tone="success">{info}</StateMessage> : null}

      <form className={styles.form} onSubmit={onSubmit} noValidate>
        <Input
          label="Nome da organização"
          name="organizationName"
          autoComplete="organization"
          required
          value={organizationName}
          onChange={(e) => setOrganizationName(e.target.value)}
          disabled={pending}
        />
        <Input
          label="Seu nome"
          name="displayName"
          autoComplete="name"
          hint="Opcional"
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
          disabled={pending}
        />
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
          autoComplete="new-password"
          required
          minLength={6}
          hint="Mínimo de 6 caracteres"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={pending}
        />
        <div className={styles.actions}>
          <Button type="submit" disabled={pending || !canSubmit}>
            {pending ? "Criando…" : "Criar conta"}
          </Button>
        </div>
      </form>

      <p className={styles.footerLink}>
        Já tem conta? <Link href="/login">Entrar</Link>
      </p>
    </>
  );
}
