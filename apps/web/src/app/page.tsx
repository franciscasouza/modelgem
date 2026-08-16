import styles from "./page.module.css";

export default function Home() {
  return (
    <div className={styles.page}>
      <main className={styles.main}>
        <p className={styles.brand}>ModelaFlow</p>
        <h1>Studio placeholder</h1>
        <p>
          Frontend base (Next.js App Router). O editor 2D e fluxos de modelagem
          entram em incrementos posteriores — este app só valida o monorepo.
        </p>
        <p className={styles.meta}>API: <code>/api/v1</code> · unidade de negócio: cm</p>
      </main>
    </div>
  );
}
