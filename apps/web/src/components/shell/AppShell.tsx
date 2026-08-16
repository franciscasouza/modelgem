"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Primitives";
import styles from "./shell.module.css";

const NAV = [
  { href: "/", label: "Biblioteca", icon: "lib" },
  { href: "/ai", label: "Editor IA", icon: "ai", disabled: true },
  { href: "/patterns/new", label: "Canvas 2D", icon: "canvas" },
  { href: "/clients", label: "Clientes", icon: "clients" },
  { href: "/settings", label: "Config", icon: "settings" },
] as const;

function NavIcon({ name }: { name: string }) {
  const common = {
    width: 18,
    height: 18,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.8,
  };
  switch (name) {
    case "lib":
      return (
        <svg {...common}>
          <rect x="3" y="4" width="7" height="7" rx="1.5" />
          <rect x="14" y="4" width="7" height="7" rx="1.5" />
          <rect x="3" y="13" width="7" height="7" rx="1.5" />
          <rect x="14" y="13" width="7" height="7" rx="1.5" />
        </svg>
      );
    case "ai":
      return (
        <svg {...common}>
          <path d="M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3z" />
        </svg>
      );
    case "canvas":
      return (
        <svg {...common}>
          <rect x="4" y="4" width="16" height="16" rx="2" />
          <path d="M8 16l3-6 3 4 2-3 2 5" />
        </svg>
      );
    case "clients":
      return (
        <svg {...common}>
          <circle cx="9" cy="8" r="3" />
          <path d="M4 19c0-2.8 2.2-5 5-5s5 2.2 5 5" />
          <circle cx="17" cy="9" r="2.5" />
          <path d="M16 19c0-1.7 1-3.2 2.5-4" />
        </svg>
      );
    default:
      return (
        <svg {...common}>
          <circle cx="12" cy="12" r="3" />
          <path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M5.6 18.4l1.4-1.4M17 7l1.4-1.4" />
        </svg>
      );
  }
}

export function Sidebar() {
  const pathname = usePathname();
  const router = useRouter();
  const { user, logout } = useAuth();
  const [loggingOut, setLoggingOut] = useState(false);

  async function onLogout() {
    setLoggingOut(true);
    try {
      await logout();
      router.replace("/login");
    } finally {
      setLoggingOut(false);
    }
  }

  const displayName = user?.displayName?.trim() || user?.email || "Usuária";

  return (
    <aside className={styles.sidebar}>
      <div className={styles.brandBlock}>
        <Link href="/" className={styles.brand}>
          <span className={styles.brandMark} aria-hidden />
          <span className={styles.brandText}>ModelaFlow</span>
        </Link>
        <Link href="/patterns/new" className={styles.newProject}>
          <span aria-hidden>+</span> Novo projeto
        </Link>
      </div>

      <nav className={styles.nav} aria-label="Principal">
        {NAV.map((item) => {
          const active =
            item.href === "/"
              ? pathname === "/"
              : pathname === item.href || pathname.startsWith(`${item.href}/`);
          if ("disabled" in item && item.disabled) {
            return (
              <span
                key={item.href}
                className={`${styles.navItem} ${styles.navDisabled}`}
                title="Em breve (Fase 2)"
              >
                <NavIcon name={item.icon} />
                <span>{item.label}</span>
                <span className={styles.soon}>Em breve</span>
              </span>
            );
          }
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`${styles.navItem} ${active ? styles.navActive : ""}`}
            >
              <NavIcon name={item.icon} />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </nav>

      <div className={styles.accountBlock}>
        {user ? (
          <>
            <p className={styles.accountOrg}>{user.organizationName}</p>
            <p className={styles.accountUser}>{displayName}</p>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              className={styles.logoutBtn}
              disabled={loggingOut}
              onClick={() => void onLogout()}
            >
              {loggingOut ? "Saindo…" : "Sair"}
            </Button>
          </>
        ) : null}
        <p className={styles.sidebarFoot}>Unidade: cm · IA não é autoridade final</p>
      </div>
    </aside>
  );
}

function tabForPath(pathname: string): string {
  if (pathname.startsWith("/ai")) return "ai";
  if (pathname.includes("/tech-pack")) return "tech";
  if (pathname.includes("/canvas") || pathname.startsWith("/patterns")) return "2d";
  return "dashboard";
}

export function TopTabs({ patternId }: { patternId?: string }) {
  const pathname = usePathname();
  const current = tabForPath(pathname);
  const canvasHref = patternId ? `/patterns/${patternId}/canvas` : "/patterns/new";
  const techHref = patternId ? `/patterns/${patternId}/tech-pack` : "/patterns/new";

  const tabs = [
    { id: "dashboard", label: "Dashboard", href: "/" },
    { id: "ai", label: "Editor IA", href: "/ai", stub: true },
    { id: "2d", label: "2D", href: canvasHref },
    { id: "tech", label: "Tech Pack", href: techHref },
  ];

  return (
    <div className={styles.topTabs} role="tablist" aria-label="Áreas do studio">
      {tabs.map((tab) => {
        const active = current === tab.id;
        if (tab.stub) {
          return (
            <Link
              key={tab.id}
              href={tab.href}
              className={`${styles.tab} ${active ? styles.tabActive : ""} ${styles.tabStub}`}
              role="tab"
              aria-selected={active}
            >
              {tab.label}
            </Link>
          );
        }
        return (
          <Link
            key={tab.id}
            href={tab.href}
            className={`${styles.tab} ${active ? styles.tabActive : ""}`}
            role="tab"
            aria-selected={active}
          >
            {tab.label}
          </Link>
        );
      })}
    </div>
  );
}

export function AppShell({
  children,
  patternId,
}: {
  children: React.ReactNode;
  patternId?: string;
}) {
  return (
    <div className={styles.shell}>
      <Sidebar />
      <div className={styles.mainCol}>
        <header className={styles.topBar}>
          <TopTabs patternId={patternId} />
        </header>
        <main className={styles.content}>{children}</main>
      </div>
    </div>
  );
}
