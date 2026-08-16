import type { ReactNode } from "react";
import styles from "@/components/auth/auth.module.css";

export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className={styles.page}>
      <div className={styles.hero}>
        <div className={styles.heroBrand}>
          <span className={styles.heroMark} aria-hidden />
          <span className={styles.heroBrandName}>ModelaFlow</span>
        </div>
        <h1 className={styles.heroTitle}>Studio de modelagem assistida</h1>
        <p className={styles.heroText}>
          A modelista confirma cada interpretação. A IA sugere; o núcleo
          paramétrico calcula; você decide.
        </p>
      </div>
      <div className={styles.panel}>
        <div className={styles.panelInner}>{children}</div>
      </div>
    </div>
  );
}
