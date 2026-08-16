import { RequireAuth } from "@/components/auth/AuthGuards";
import { AppShell } from "@/components/shell/AppShell";

export default function StudioLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RequireAuth>
      <AppShell>{children}</AppShell>
    </RequireAuth>
  );
}
